using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using NetworkTables.NetworkTables2.Stream;
using NetworkTables.NetworkTables2.Type;

namespace NetworkTables.NetworkTables2.Connection
{
    public class NetworkTableConnection
    {
        public static char PROTOCOL_REVISION = (char)0x0200;

        private object WRITE_LOCK;

        private DataIOStream inStream;
        private DataIOStream outStream;

        private readonly IOStream stream;

        private readonly NetworkTableEntryTypeManager typeManager;
        private bool isValid;

        public NetworkTableConnection(IOStream stream, NetworkTableEntryTypeManager typeManager)
        {
            WRITE_LOCK = new object();
            this.stream = stream;

            this.typeManager = typeManager;
            this.inStream = stream.GetInputStream();
            this.outStream = stream.GetOutputStream();
            isValid = true;
        }

        public void Close()
        {
            if (isValid)
            {
                isValid = false;
                stream.Close();
            }
        }

        private void SendMessageHeader(int messageType)
        {
            lock (WRITE_LOCK)
            {
                outStream.WriteByte(((byte)messageType));
            }
        }

        public void Flush()
        {
            lock (WRITE_LOCK)
            {
                outStream.Flush();
            }
        }

        public void SendKeepAlive()
        {
            lock (WRITE_LOCK)
            {
                SendMessageHeader(NetworkTableMessageType.KEEP_ALIVE);
                Flush();
            }
        }

        public void SendClientHello()
        {
            lock (WRITE_LOCK)
            {
                SendMessageHeader(NetworkTableMessageType.CLIENT_HELLO);
                outStream.WriteCharBE(PROTOCOL_REVISION);
                Flush();
            }
        }

        public void SendServerHelloComplete()
        {
            lock (WRITE_LOCK)
            {
                SendMessageHeader(NetworkTableMessageType.SERVER_HELLO_COMPLETE);
                Flush();
            }
        }

        public void SendProtocolVersionUnsupported()
        {
            lock (WRITE_LOCK)
            {
                SendMessageHeader(NetworkTableMessageType.PROTOCOL_VERSION_UNSUPPORTED);
                outStream.WriteCharBE(PROTOCOL_REVISION);
                Flush();
            }
        }

        public void SendEntryAssignment(NetworkTableEntry entry)
        {
            lock (WRITE_LOCK)
            {
                SendMessageHeader(NetworkTableMessageType.ENTRY_ASSIGNMENT);
                outStream.WriteString(entry.name);
                outStream.WriteByte((byte)entry.GetType().id);
                outStream.WriteCharBE((char)entry.GetId());
                outStream.WriteCharBE((char)entry.GetSequenceNumber());
                entry.SendValue(outStream);

            }
        }

        public void SendEntryUpdate(NetworkTableEntry entry)
        {
            lock (WRITE_LOCK)
            {
                SendMessageHeader(NetworkTableMessageType.FIELD_UPDATE);
                outStream.WriteCharBE((char)entry.GetId());
                outStream.WriteCharBE((char)entry.GetSequenceNumber());
                entry.SendValue(outStream);
            }
        }

        public void Read(ConnectionAdapter adapter)
        {
            try
            {
                int messageType = inStream.ReadByte();
                switch (messageType)
                {
                    case NetworkTableMessageType.KEEP_ALIVE:
                        adapter.KeepAlive();
                        return;
                    case NetworkTableMessageType.CLIENT_HELLO:
                    {
                        char protocolRevision = inStream.ReadCharBE();
                        adapter.ClientHello(protocolRevision);
                        return;
                    }
                    case NetworkTableMessageType.SERVER_HELLO_COMPLETE:
                    {
                        adapter.ServerHelloComplete();
                        return;
                    }
                    case NetworkTableMessageType.PROTOCOL_VERSION_UNSUPPORTED:
                    {
                        char protocolRevision = inStream.ReadCharBE();
                        adapter.ProtocolVersionUnsupported(protocolRevision);
                        return;
                    }
                    case NetworkTableMessageType.ENTRY_ASSIGNMENT:
                    {
                        string entryName = inStream.ReadString();
                        byte typeId = inStream.ReadByte();
                        NetworkTableEntryType entryType = typeManager.GetType(typeId);
                        if (entryType == null)
                            throw new BadMessageException("Unknown data type: 0x" + typeId.ToString("X"));
                        char entryId = inStream.ReadCharBE();
                        char entrySequenceNumber = inStream.ReadCharBE();
                        object value = entryType.ReadValue(inStream);
                        adapter.OfferIncomingAssignment(new NetworkTableEntry(entryId, entryName, entrySequenceNumber,
                            entryType, value));
                        return;
                    }
                    case NetworkTableMessageType.FIELD_UPDATE:
                    {
                        char entryId = inStream.ReadCharBE();
                        char entrySequenceNumber = inStream.ReadCharBE();
                        NetworkTableEntry entry = adapter.GetEntry(entryId);
                        if (entry == null)
                            throw new BadMessageException("Received update for unknown entry id: " + (int) entryId);
                        object value = entry.GetType().ReadValue(inStream);

                        adapter.OfferIncomingUpdate(entry, entrySequenceNumber, value);
                        return;
                    }
                    default:
                        throw new BadMessageException("Unknown Network Table Message Type: " + messageType);
                }

            }
            catch
            {
                throw;
            }
        }

    }
}
