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

namespace NetworkTablesDotNet.NetworkTables2.Client
{
    public class ClientConnectionAdapter : ConnectionAdapter, FlushableOutgoingEntryReceiver
    {
        private readonly ClientNetworkTableEntryStore entryStore;
        private readonly IOStreamFactory streamFactory;
        private readonly NTThreadManager threadManager;

        private NetworkTableConnection connection;

        private NTThread readThread;
        private ClientConnectionState connectionState = ClientConnectionState.DISCONNECTED_FROM_SERVER;
        private readonly ClientConnectionListenerManager connectionListenerManager;
        private readonly object connectionLock = new object();
        private readonly NetworkTableEntryTypeManager typeManager;

        private void GotoState(ClientConnectionState newState)
        {
            lock (connectionLock)
            {
                if (connectionState != newState)
                {
                    Console.WriteLine($"{this} entered connection state: {newState}");
                    if (newState == ClientConnectionState.IN_SYNC_WITH_SERVER)
                    {
                        connectionListenerManager.FireConnectedEvent();
                    }
                    if (connectionState == ClientConnectionState.IN_SYNC_WITH_SERVER)
                    {
                        connectionListenerManager.FireDisconnectedEvent();
                    }
                    connectionState = newState;
                }
            }
        }

        public ClientConnectionState GetConnectionState() => connectionState;

        public bool IsConnected() => GetConnectionState() == ClientConnectionState.IN_SYNC_WITH_SERVER;

        public ClientConnectionAdapter(ClientNetworkTableEntryStore entryStore, NTThreadManager threadManager, IOStreamFactory streamFactory, ClientConnectionListenerManager connectionListenerManager, NetworkTableEntryTypeManager typeManager)
        {
            this.entryStore = entryStore;
            this.streamFactory = streamFactory;
            this.threadManager = threadManager;
            this.connectionListenerManager = connectionListenerManager;
            this.typeManager = typeManager;
        }

        public void Reconnect()
        {
            lock (connectionLock)
            {
                Close();
                try
                {
                    IOStream stream = streamFactory.CreateStream();
                    if (stream == null)
                        return;
                    connection = new NetworkTableConnection(stream, typeManager);
                    readThread = threadManager.NewBlockingPeriodicThread(new ConnectionMonitorThread(this, connection),
                        "Client Connection Reader Thread");
                    connection.SendClientHello();
                    GotoState(ClientConnectionState.CONNECTED_TO_SERVER);
                }
                catch (Exception e)
                {
                    Close();
                }
            }
        }

        public void Close()
        {
            Close(ClientConnectionState.DISCONNECTED_FROM_SERVER);
        }

        public void Close(ClientConnectionState newState)
        {
            lock (connectionLock)
            {
                GotoState(newState);
                if (readThread != null)
                {
                    readThread.Stop();
                    readThread = null;
                }
                if (connection != null)
                {
                    connection.Close();
                    connection = null;
                }
                entryStore.ClearIds();
            }
        }


        public void OfferIncomingAssignment(NetworkTableEntry entry)
        {
            entryStore.OfferIncomingAssignment(entry);
        }

        public void OfferIncomingUpdate(NetworkTableEntry entry, char entrySequenceNumber, object value)
        {
            entryStore.OfferIncomingUpdate(entry, entrySequenceNumber, value);
        }

        public void KeepAlive()
        {
        }

        public void ClientHello(char protocolRevision)
        {
            throw new BadMessageException("A client should not receive a client hello message");
        }

        public void ProtocolVersionUnsupported(char protocolRevision)
        {
            Close();
            GotoState(new ClientConnectionState.ProtocolUnsupportedByServer(protocolRevision));
        }

        public void ServerHelloComplete()
        {
            if (connectionState == ClientConnectionState.CONNECTED_TO_SERVER)
            {
                try
                {
                    GotoState(ClientConnectionState.IN_SYNC_WITH_SERVER);
                    entryStore.SendUnknownEntries(connection);
                }
                catch (SocketException e)
                {
                    IOException(e);
                }
            }
            else
                throw new BadMessageException("A client should only receive a server hello complete once and only after it has connected to the server");

        }

        public NetworkTableEntry GetEntry(char id)
        {
            return entryStore.GetEntry(id);
        }

        public void BadMessage(BadMessageException e)
        {
            Close(new ClientConnectionState.Error(e));
        }

        public void IOException(Exception e)
        {
            if (connectionState!=ClientConnectionState.DISCONNECTED_FROM_SERVER)
                Reconnect();
        }

        public void OfferOutgoingAssignment(NetworkTableEntry entry)
        {
            try
            {
                lock (connectionState)
                {
                    if (connection != null && connectionState == ClientConnectionState.IN_SYNC_WITH_SERVER)
                        connection.SendEntryAssignment(entry);
                }
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
                lock (connectionState)
                {
                    if (connection != null && connectionState == ClientConnectionState.IN_SYNC_WITH_SERVER)
                        connection.SendEntryUpdate(entry);
                }
            }
            catch (SocketException e)
            {
                IOException(e);
            }
        }

        public void Flush()
        {
            lock (connectionLock)
            {
                if (connection != null)
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
                else
                {
                    Reconnect();
                }
            }
        }

        public void EnsureAlive()
        {
        }
    }
}
