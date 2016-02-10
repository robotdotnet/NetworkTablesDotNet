using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetworkTables.TcpSockets;
using static NetworkTables.Logger;

namespace NetworkTables
{
    internal class DispatcherBase : IDisposable
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
                        Waring($"When reading initial persistent values from \" {persistentFilename} \":");
                    }
                    Waring($"{persistentFilename} : {i} : {s}");
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
            m_updateRate = (uint)(interval * 1000);
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

        public void NotifyConnections(NtCore.ConnectionListenerCallback callback)
        {
            lock(m_userMutex)
            {
                foreach(var conn in m_connections)
                {
                    if (conn.GetState() != NetworkConnection.State.kActive) continue;
                    m_notifier.NotifyConnection(true, conn.GetConnectionInfo(), callback);
                }
            }
        }

        public bool Active() => m_active;

        protected DispatcherBase(Storage storage, Notifier notifier)
        {
            m_storage = storage;
            m_notifier = notifier;
            m_active = false;
            m_updateRate = 100;
        }

        private static readonly TimeSpan saveDeltaTime = TimeSpan.FromSeconds(1);

        private void DispatchThreadMain()
        {
            var timeoutTime = DateTime.UtcNow;

            var nextSaveTime = timeoutTime + saveDeltaTime;

            int count = 0;

            bool lockEntered = false;
            try
            {
                Monitor.Enter(m_flushMutex, ref lockEntered);
                while (m_active)
                {
                    var start = DateTime.UtcNow;
                    if (start > timeoutTime)
                        timeoutTime = start;

                    timeoutTime += TimeSpan.FromMilliseconds(m_updateRate);
                    Monitor.Exit(m_flushMutex);
                    lockEntered = false;
                    m_flushCv.WaitOne(TimeSpan.FromMilliseconds(m_updateRate));
                    Monitor.Enter(m_flushMutex, ref lockEntered);
                    m_doFlush = false;
                    if (!m_active) break;

                    if (m_server && !string.IsNullOrEmpty(m_persistFilename) && start > nextSaveTime)
                    {
                        nextSaveTime += saveDeltaTime;
                        if (start > nextSaveTime) nextSaveTime = start + saveDeltaTime;
                        string err = m_storage.SavePersistent(m_persistFilename, true);
                        if (err != null)
                        {
                            Waring($"periodic persistent save: {err}");
                        }
                    }

                    lock (m_userMutex)
                    {
                        bool reconnect = false;

                        if (++count > 10)
                        {
                            Debug($"dispatch running {m_connections.Count} connections");
                            count = 0;
                        }

                        foreach (var conn in m_connections)
                        {
                            if (conn.GetState() == NetworkConnection.State.kActive)
                                conn.PostOutgoing(!m_server);

                            if (!m_server && conn.GetState() == NetworkConnection.State.kDead)
                                reconnect = true;
                        }

                        if (reconnect && !m_doReconnect)
                        {
                            m_doReconnect = true;
                            m_reconnectCv.Set();
                        }
                    }
                }
            }
            finally
            {
                if (lockEntered) Monitor.Exit(m_flushMutex);
            }

        }

        private void ServerThreadMain()
        {
            if (m_serverAccepter.Start() != 0)
            {
                m_active = false;
                return;
            }

            while (m_active)
            {
                var stream = m_serverAccepter.Accept();
                if (stream == null)
                {
                    m_active = false;
                    return;
                }
                if (!m_active) return;

                Debug($"server: client connection from {stream.GetPeerIP()} port {stream.GetPeerPort()}");

                var conn = new NetworkConnection(stream, m_notifier, ServerHandshake, m_storage.GetEntryType);
                conn.SetProcessIncoming(m_storage.ProcessIncoming);

                lock (m_userMutex)
                {
                    bool placed = false;
                    for (int i = 0; i < m_connections.Count; i++)
                    {
                        var c = m_connections[i];
                        if (c.GetState() == NetworkConnection.State.kDead)
                        {
                            m_connections[i] = conn;
                            placed = true;
                            break;
                        }
                    }

                    if (!placed) m_connections.Add(conn);
                    conn.Start();
                }
            }
        }

        private void ClientThreadMain(object o)
        {
            Func<INetworkStream> connect = o as Func<INetworkStream>;
            if (connect == null)
            {
                throw new Exception("Client not passed a correct variable");
            }

            while (m_active)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(500));

                Debug("client trying to connect");
                var stream = connect();
                if (stream == null) continue; //keep retrying
                Debug("client connected");

                bool lockEntered = false;
                try
                {
                    Monitor.Enter(m_userMutex, ref lockEntered);
                    var conn = new NetworkConnection(stream, m_notifier, ClientHandshake, m_storage.GetEntryType);
                    conn.SetProcessIncoming(m_storage.ProcessIncoming);
                    foreach(var s in m_connections)
                    {
                        s.Dispose();
                    }

                    m_connections.Clear();

                    m_connections.Add(conn);

                    conn.SetProtoRev(m_reconnectProtoRev);

                    conn.Start();

                    m_doReconnect = false;

                    Monitor.Exit(m_userMutex);
                    lockEntered = false;
                    m_reconnectCv.WaitOne();
                }
                finally
                {
                    if (lockEntered) Monitor.Exit(m_userMutex);
                }
            }
        }

        private bool ClientHandshake(NetworkConnection conn, Func<Message> getMsg, Action<Message[]> sendMsgs)
        {
            string selfId;
            lock(m_userMutex)
            {
                selfId = m_identity;
            }

            Debug("client: sending hello");
            sendMsgs(new Message[] { Message.ClientHello(selfId) });

            var msg = getMsg();
            if (msg == null)
            {
                //Disconnected
                Debug("client: server disconnected before first response");
                return false;
            }

            if (msg.Is(Message.MsgType.kProtoUnsup))
            {
                if (msg.Id() == 0x0200) ClientReconnect(0x0200);
                return false;
            }

            bool newServer = true;
            if (conn.ProtoRev >= 0x0300)
            {
                if (!msg.Is(Message.MsgType.kServerHello)) return false;
                conn.SetRemoteId(msg.Str());
                if ((msg.Flags() & 1) != 0) newServer = false;
                msg = getMsg();
            }

            List<Message> incoming = new List<Message>();

            for(;;)
            {
                if (msg == null)
                {
                    //disconnected, retry
                    Debug("client: server disconnected during initial entries");
                    return false;
                }
                Debug4($"received init str={msg.Str()} id={msg.Id()} seqNum={msg.SeqNumUid()}");

                if (msg.Is(Message.MsgType.kServerHelloDone)) break;
                if (!msg.Is(Message.MsgType.kEntryAssign))
                {
                    //Unexpected
                    Debug($"client: received message ({msg.Type()}) other then entry assignment during initial handshake");
                    return false;
                }

                incoming.Add(msg);

                msg = getMsg();
            }

            List<Message> outgoing = new List<Message>();
            m_storage.ApplyInitialAssignments(conn, incoming.ToArray(), newServer, outgoing);

            if (conn.ProtoRev >= 0x0300)
            {
                outgoing.Add(Message.ClientHelloDone());
            }

            if (outgoing.Count != 0) sendMsgs(outgoing.ToArray());

            Info($"client: CONNECTED to server {conn.Stream().GetPeerIP()} port {conn.Stream().GetPeerPort()}");

            return true;
        }

        private bool ServerHandshake(NetworkConnection conn, Func<Message> getMsg, Action<Message[]> sendMsgs)
        {
            var msg = getMsg();

            if (msg == null)
            {
                Debug("server: client disconnected before sending hello");
                return false;
            }

            if (!msg.Is(Message.MsgType.kClientHello))
            {
                Debug("server: client initial message was not client hello");
                return false;
            }

            uint protoRev = msg.Id();

            if (protoRev > 0x0300)
            {
                Debug("server: client requested proto > 0x0300");
                sendMsgs(new Message[] { Message.ProtoUnsup() });
                return false;
            }

            if (protoRev >= 0x0300) conn.SetRemoteId(msg.Str());

            Debug($"server: client protocol {protoRev}");
            conn.SetProtoRev(protoRev);

            List<Message> outgoing = new List<Message>();

            if (protoRev >= 0x0300)
            {
                lock (m_userMutex)
                {
                    outgoing.Add(Message.ServerHello(0, m_identity));
                }
            }

            m_storage.GetInitialAssignments(conn, outgoing);

            Debug("server: sending initial assignments");
            sendMsgs(outgoing.ToArray());

            if (protoRev >= 0x0300)
            {
                List<Message> incoming = new List<Message>();

                msg = getMsg();

                for (;;)
                {
                    if (msg == null)
                    {
                        //Disconnected Retry
                        Debug("server: disconnected waiting for initial entries");
                        return false;
                    }

                    if (msg.Is(Message.MsgType.kClientHelloDone)) break;
                    if (!msg.Is(Message.MsgType.kEntryAssign))
                    {
                        Debug($"server: received message ({msg.Type()}) other than entry assignment during initial handshake");
                        return false;
                    }

                    incoming.Add(msg);

                    msg = getMsg();
                }

                foreach(var m in incoming)
                {
                    m_storage.ProcessIncoming(msg, conn);
                }
            }

            Info($"server: client CONNECTED: {conn.Stream().GetPeerIP()} port {conn.Stream().GetPeerPort()}");
            return true;
        }

        private void ClientReconnect(uint protoRev = 0x0300)
        {
            if (m_server) return;
            lock (m_userMutex)
            {
                m_reconnectProtoRev = protoRev;
                m_doReconnect = true;
            }

            m_reconnectCv.Set();
        }

        private void QueueOutgoing(Message msg, NetworkConnection only, NetworkConnection except)
        {
            lock (m_userMutex)
            {
                foreach (var conn in m_connections)
                {
                    if (conn == except) continue;
                    if (only != null && conn != only) continue;
                    var state = conn.GetState();
                    if (state != NetworkConnection.State.kSynchronized &&
                        state != NetworkConnection.State.kActive) continue;
                    conn.QueueOutgoing(msg);
                }
            }
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
        private string m_identity = "";

        private bool m_active;
        private uint m_updateRate;

        private DateTime m_lastFlush;
        private bool m_doFlush = false;

        private AutoResetEvent m_reconnectCv = new AutoResetEvent(false);
        private uint m_reconnectProtoRev = 0x0300;
        private bool m_doReconnect = true;
    }

    internal class Dispatcher : DispatcherBase
    {
        private static Dispatcher s_instance;

        public static Dispatcher Instance
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
            base.StartClient(() => TCPConnector.Connect(serverName, (int)port, 1));
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
