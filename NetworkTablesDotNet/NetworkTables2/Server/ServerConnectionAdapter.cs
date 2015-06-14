using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NetworkTablesDotNet.NetworkTables2.Connection;
using NetworkTablesDotNet.NetworkTables2.Stream;
using NetworkTablesDotNet.NetworkTables2.Thread;
using NetworkTablesDotNet.NetworkTables2.Type;

namespace NetworkTablesDotNet.NetworkTables2.Server
{
    public class ServerConnectionAdapter : ConnectionAdapter, FlushableOutgoingEntryReceiver
    {
        private readonly ServerNetworkTableEntryStore entryStore;
        private readonly IncomingEntryReceiver transactionReceiver;
        private readonly ServerAdapterManager adapterListener;

        public readonly NetworkTableConnection connection;

        private readonly NTThread readThread;

        private ServerConnectionState connectionState;

        private void GotoState(ServerConnectionState newState)
        {
            if (connectionState != newState)
            {
                Console.WriteLine($"{this} entered connection state: {newState}");
                connectionState = newState;
            }
        }

        public ServerConnectionAdapter(IOStream stream, ServerNetworkTableEntryStore entryStore,
            IncomingEntryReceiver transactionReceiver, ServerAdapterManager adapterListener,
            NetworkTableEntryTypeManager typeManager, NTThreadManager threadManager)
        {
            connection = new NetworkTableConnection(stream, typeManager);
            this.entryStore = entryStore;
            this.transactionReceiver = transactionReceiver;
            this.adapterListener = adapterListener;

            GotoState(ServerConnectionState.GOT_CONNECTION_FROM_CLIENT);
            readThread = threadManager.NewBlockingPeriodicThread(new ConnectionMonitorThread(this, connection),
                "Server Connection Reader Thread");
        }

        public void Shutdown(bool closeStream)
        {
            readThread.Stop();
            if (closeStream)
                connection.Close();
        }


        public void OfferIncomingAssignment(NetworkTableEntry entry)
        {
            transactionReceiver.OfferIncomingAssignment(entry);
        }

        public void OfferIncomingUpdate(NetworkTableEntry entry, char entrySequenceNumber, object value)
        {
            transactionReceiver.OfferIncomingUpdate(entry, entrySequenceNumber, value);
        }

        public void KeepAlive()
        {
        }

        public void ClientHello(char protocolRevision)
        {
            if (connectionState != ServerConnectionState.GOT_CONNECTION_FROM_CLIENT)
                throw new BadMessageException("A server should not receive a client hello after it has already connected/entered an error state");
            if (protocolRevision != NetworkTableConnection.PROTOCOL_REVISION)
            {
                connection.SendProtocolVersionUnsupported();
                throw new BadMessageException("Client Connected with bad protocol revision: 0x" + ((int)protocolRevision).ToString("X"));
            }
            else
            {
                entryStore.SendServerHello(connection);
                GotoState(ServerConnectionState.CONNECTED_TO_CLIENT);
            }
        }

        public void ProtocolVersionUnsupported(char protocolRevision)
        {
            throw new BadMessageException("A server should not receive a protocol version unsupported message");
        }

        public void ServerHelloComplete()
        {
            throw new BadMessageException("A server should not receive a server hello complete message");
        }

        public NetworkTableEntry GetEntry(char id)
        {
            return entryStore.GetEntry(id);
        }

        public void BadMessage(BadMessageException e)
        {
            GotoState(new ServerConnectionState.Error(e));
            adapterListener.Close(this, true);
        }

        public void IOException(Exception e)
        {
            if (e is EndOfStreamException)
            {
                GotoState(ServerConnectionState.CLIENT_DISCONNECTED);
            }
            else
            {
                GotoState(new ServerConnectionState.Error(e));
            }
            adapterListener.Close(this, false);
        }



        public void Flush()
        {
            try
            {
                connection.Flush();
            }
            catch (SocketException e)
            {
                IOException(e);
            }
        }

        public ServerConnectionState GetConnectionState()
        {
            return connectionState;
        }

        public void EnsureAlive()
        {
            try
            {
                connection.SendKeepAlive();
            }
            catch (SocketException e)
            {
                IOException(e);
            }
        }

        public void OfferOutgoingAssignment(NetworkTableEntry entry)
        {
            try
            {
                if (connectionState == ServerConnectionState.CONNECTED_TO_CLIENT)
                    connection.SendEntryAssignment(entry);
            }
            catch (SocketException e)
            {
                IOException(e);
            }
        }

        public void OfferOutgoingUpdate(NetworkTableEntry entry)
        {
            try
            {
                if (connectionState == ServerConnectionState.CONNECTED_TO_CLIENT)
                    connection.SendEntryUpdate(entry);
            }
            catch (SocketException e)
            {
                IOException(e);
            }
        }
    }
}
