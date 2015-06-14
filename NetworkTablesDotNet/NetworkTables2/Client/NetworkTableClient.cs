using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NetworkTablesDotNet.NetworkTables2.Stream;
using NetworkTablesDotNet.NetworkTables2.Thread;
using NetworkTablesDotNet.NetworkTables2.Type;

namespace NetworkTablesDotNet.NetworkTables2.Client
{
    public class NetworkTableClient : NetworkTableNode
    {
        private readonly ClientConnectionAdapter adapter;
        private readonly WriteManager writeManager;

        public NetworkTableClient(IOStreamFactory streamFactory, NetworkTableEntryTypeManager typeManager, NTThreadManager threadManager)
        {
            ClientNetworkTableEntryStore entryStore;
            Init(entryStore = new ClientNetworkTableEntryStore(this));
            adapter = new ClientConnectionAdapter(entryStore, threadManager, streamFactory, this, typeManager);
            writeManager = new WriteManager(adapter, threadManager, GetEntryStore(), 1000);

            GetEntryStore().SetOutgoingReceiver(new TransactionDirtier(writeManager));
            GetEntryStore().SetIncomingReceiver(new OutgoingEntryReceiverNull());
            writeManager.Start();
        }

        public NetworkTableClient(IOStreamFactory streamFactory) :
            this(streamFactory, new NetworkTableEntryTypeManager(), new DefaultThreadManager())
        {
            
        }

        public void Reconnect() => adapter.Reconnect();

        public void Stop()
        {
            writeManager.Stop();
            Close();
        }

        public override bool IsConnected() => adapter.IsConnected();

        public override bool IsServer() => false;

        public override void Close() => adapter.Close();
    }
}
