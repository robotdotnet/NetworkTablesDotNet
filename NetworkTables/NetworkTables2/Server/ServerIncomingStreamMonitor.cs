using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NetworkTables.NetworkTables2.Stream;
using NetworkTables.NetworkTables2.Thread;
using NetworkTables.NetworkTables2.Type;

namespace NetworkTables.NetworkTables2.Server
{
    public class ServerIncomingStreamMonitor : PeriodicRunnable
    {
        private readonly IOStreamProvider streamProvider;
        private readonly ServerIncomingConnectionListener incomingListener;
        private readonly ServerNetworkTableEntryStore entryStore;
        private readonly ServerAdapterManager adapterListener;

        private NTThread monitorThread;
        private NTThreadManager threadManager;
        private readonly NetworkTableEntryTypeManager typeManager;

        public ServerIncomingStreamMonitor(IOStreamProvider streamProvider, ServerNetworkTableEntryStore entryStore,
            ServerIncomingConnectionListener incomingListener, ServerAdapterManager adapterListener,
            NetworkTableEntryTypeManager typeManager, NTThreadManager threadManager)
        {
            this.streamProvider = streamProvider;
            this.entryStore = entryStore;
            this.incomingListener = incomingListener;
            this.adapterListener = adapterListener;
            this.typeManager = typeManager;
            this.threadManager = threadManager;
        }

        public void Start()
        {
            if (monitorThread != null)
            {
                Stop();
            }
            monitorThread = threadManager.NewBlockingPeriodicThread(this, "Server Incoming Stream Monitor Thread");
        }

        public void Stop()
        {
            monitorThread?.Stop();
        }

        public void Run()
        {
            IOStream newStream = null;
            try
            {
                newStream = streamProvider.Accept();
                if (newStream != null)
                {
                    ServerConnectionAdapter connectionAdapter = new ServerConnectionAdapter(newStream, entryStore, entryStore, adapterListener, typeManager, threadManager);
                    incomingListener.OnNewConnection(connectionAdapter);
                }
            }
            catch (IOException)
            {
            }
        }
    }
}
