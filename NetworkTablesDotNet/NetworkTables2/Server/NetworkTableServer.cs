using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using NetworkTablesDotNet.NetworkTables2.Stream;
using NetworkTablesDotNet.NetworkTables2.Thread;
using NetworkTablesDotNet.NetworkTables2.Type;

namespace NetworkTablesDotNet.NetworkTables2.Server
{
    class NetworkTableServer : NetworkTableNode, ServerIncomingConnectionListener
    {
        private readonly ServerIncomingStreamMonitor incomingStreamMonitor;
        private readonly WriteManager writeManager;
        private readonly IOStreamProvider streamProvider;
        private readonly ServerConnectionList connectionList;

        public NetworkTableServer(IOStreamProvider streamProvider, NetworkTableEntryTypeManager typeManager, NTThreadManager threadManager)
        {
            ServerNetworkTableEntryStore entryStore;
            Init(entryStore = new ServerNetworkTableEntryStore(this));
            this.streamProvider = streamProvider;

            connectionList = new ServerConnectionList();
            writeManager = new WriteManager(connectionList, threadManager, GetEntryStore(), long.MaxValue);

            incomingStreamMonitor = new ServerIncomingStreamMonitor(streamProvider, entryStore, this, connectionList, typeManager, threadManager);

            GetEntryStore().SetIncomingReceiver(new TransactionDirtier(writeManager));
            GetEntryStore().SetOutgoingReceiver(new TransactionDirtier(writeManager));

            incomingStreamMonitor.Start();
            writeManager.Start();
        }

        public NetworkTableServer(IOStreamProvider streamProvider)
            : this(streamProvider, new NetworkTableEntryTypeManager(), new DefaultThreadManager())
        {
            
        }

        public override bool IsConnected()
        {
            return true;
        }

        public override bool IsServer()
        {
            return true;
        }

        public override void Close()
        {
            try
            {
                incomingStreamMonitor.Stop();
                writeManager.Stop();
                connectionList.CloseAll();
                System.Threading.Thread.Sleep(1000);
                streamProvider.Close();
                System.Threading.Thread.Sleep(1000);
            }
            catch (Exception e)
            {
                Console.Write(e);
            }
        }

        public void OnNewConnection(ServerConnectionAdapter connectionAdapter)
        {
            connectionList.Add(connectionAdapter);
        }
    }
}
