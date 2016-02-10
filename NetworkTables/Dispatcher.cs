using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetworkTables.TcpSockets;

namespace NetworkTables
{
    public class DispatcherBase : IDisposable
    {
        public void Dispose()
        {
            Stop();
        }

        public void StartServer(string persistentFilename, INetworkAcceptor acceptor)
        {
            lock (m_userMutex)
            {
                if (m_active) return;
                m_active = true;
            }
            m_server = true;
            m_persistFilename = persistentFilename;
            m_serverAccepter = acceptor;

            if (!string.IsNullOrEmpty(persistentFilename))
            {
                bool first = true;
                m_storage.LoadPersistent(persistentFilename, (i, s) =>
                {
                    if (first)
                    {
                        first = false;
                        //Waring
                    }
                    //Warning
                });
            }

            m_storage.SetOutgoing(QueueOutgoing, m_server);

            m_dispatchThread = new Thread(DispatchThreadMain);
            m_dispatchThread.Start();

            m_clientServerThread = new Thread(ServerThreadMain);
            m_clientServerThread.Start();
        }

        public void StartClient(Func<INetworkStream> connect)
        {
            lock (m_userMutex)
            {
                if (m_active) return;
                m_active = true;
            }
            m_server = false;

            m_storage.SetOutgoing(QueueOutgoing, m_server);

            m_dispatchThread = new Thread(DispatchThreadMain);
            m_dispatchThread.Start();

            m_clientServerThread = new Thread(ClientThreadMain);
            m_clientServerThread.Start(connect);
        }

        private readonly object m_flushMutex = new object();
        private readonly AutoResetEvent m_flushCv = new AutoResetEvent(false);

        public void Stop()
        {
            m_active = false;
            m_flushCv.Set();

            ClientReconnect();

            m_serverAccepter?.Shutdown();

            bool shutdown = m_dispatchThread.Join(TimeSpan.FromSeconds(1));

            if (!shutdown) m_dispatchThread.Abort();

            shutdown = m_clientServerThread.Join(TimeSpan.FromSeconds(1));

            if (!shutdown) m_clientServerThread.Abort();

            foreach (var networkConnection in m_connections)
            {
                networkConnection.Dispose();
            }

            m_connections.Clear();

        }

        public void SetUpdateRate(double interval)
        {
            if (interval < 0.1)
                interval = 0.1;
            else if (interval > 1.0)
                interval = 1.0;
            m_updateRate = (uint) (interval*1000);
        }

        public void SetIdentity(string name)
        {
            lock (m_userMutex)
            {
                m_identity = name;
            }
        }

        public void Flush()
        {
            var now = DateTime.UtcNow;
            lock (m_flushMutex)
            {
                if ((now - m_lastFlush) < TimeSpan.FromMilliseconds(100))
                {
                    return;
                }

                m_lastFlush = now;
                m_doFlush = true;
            }
            m_flushCv.Set();
        }

        public List<ConnectionInfo> GetConnections()
        {
            List<ConnectionInfo> conns = new List<ConnectionInfo>();
            if (!m_active) return conns;

            lock (m_userMutex)
            {
                foreach (var networkConnection in m_connections)
                {
                    if (networkConnection.GetState() != NetworkConnection.State.kActive) continue;
                    conns.Add(networkConnection.GetConnectionInfo());
                }
            }

            return conns;
        }

        public void NotifyConnections(Notifier.ConnectionListenerCallback callback)
        {
            
        }

        public bool Active() => m_active;

        protected DispatcherBase(Storage storage, Notifier notifier)
        {
            m_storage = storage;
            m_notifier = notifier;
            m_active = false;
            m_updateRate = 100;
        }

        private void DispatchThreadMain()
        {
            
        }

        private void ServerThreadMain()
        {
            
        }

        private void ClientThreadMain(object o)
        {
            Func<INetworkStream> connect = o as Func<INetworkStream>;
            if (connect == null)
            {
                throw new Exception("Client not passed a correct variable");
            }


        }

        private bool ClientHandshake(NetworkConnection conn, Func<Message> getMsg, Action<Message[]> sendMsgs)
        {
            
        }

        private bool ServerHandshake(NetworkConnection conn, Func<Message> getMsg, Action<Message[]> sendMsgs)
        {
            
        }

        private void ClientReconnect(uint protoRev = 0x0300)
        {
            
        }

        private void QueueOutgoing(Message msg, NetworkConnection only, NetworkConnection except)
        {
            
        }

        private Storage m_storage;
        private Notifier m_notifier;

        private bool m_server = false;

        private string m_persistFilename;
        private Thread m_dispatchThread;
        private Thread m_clientServerThread;

        private INetworkAcceptor m_serverAccepter;

        private readonly object m_userMutex = new object();
        private List<NetworkConnection> m_connections = new List<NetworkConnection>();
        private string m_identity;

        private bool m_active;
        private uint m_updateRate;

        private DateTime m_lastFlush;
        private bool m_doFlush = false;

        private AutoResetEvent m_reconnectCv = new AutoResetEvent(false);
        private uint m_reconnectProtoRev = 0x0300;
        private bool m_doReconnect = true;
    }

    public class Dispatcher : DispatcherBase
    {
        private static Dispatcher s_instance;

        public Dispatcher Instance
        {
            get
            {
                return (s_instance ?? new Dispatcher());
            }
        }

        public void StartServer(string persistentFilename, string listenAddress, uint port)
        {
            base.StartServer(persistentFilename, new TCPAcceptor((int)port, listenAddress));   
        }

        public void StartClient(string serverName, uint port)
        {
            base.StartClient(() => TCPConnector.Connect(serverName, (int) port, 1));
        }

        private Dispatcher() : this(Storage.Instance, Notifier.Instance)
        {
            
        }

        private Dispatcher(Storage storage, Notifier notifier)
            : base(storage, notifier)
        {
        }
    
    
    }
}
