﻿using System;
using System.Collections.Generic;
using System.Threading;
using NetworkTables.Extensions;
using NetworkTables.TcpSockets;
using static NetworkTables.Logger;

namespace NetworkTables
{
    internal class DispatcherBase : IDisposable
    {
        public const double MinimumUpdateTime = 0.1; //100ms
        public const double MaximumUpdateTime = 1.0; //1 second

        private static readonly TimeSpan s_saveDeltaTime = TimeSpan.FromSeconds(1);
        private readonly AutoResetEvent m_flushCv = new AutoResetEvent(false);

        private readonly object m_flushMutex = new object();
        private readonly Notifier m_notifier;

        private readonly AutoResetEvent m_reconnectCv = new AutoResetEvent(false);

        private readonly Storage m_storage;

        private readonly object m_userMutex = new object();

        private bool m_active;
        private Thread m_clientServerThread;
        private List<NetworkConnection> m_connections = new List<NetworkConnection>();
        private Thread m_dispatchThread;
        private bool m_doFlush;
        private bool m_doReconnect = true;
        private string m_identity = "";

        private DateTime m_lastFlush;

        private string m_persistFilename;
        private uint m_reconnectProtoRev = 0x0300;

        private bool m_server;

        private INetworkAcceptor m_serverAccepter;
        private uint m_updateRate;


        protected DispatcherBase(Storage storage, Notifier notifier)
        {
            m_storage = storage;
            m_notifier = notifier;
            m_active = false;
            m_updateRate = 100;
        }

        public bool Active => m_active;

        public double UpdateRate
        {
            set
            {
                //Don't allow update rates faster the 100ms or slower then 1 second
                if (value < MinimumUpdateTime)
                    value = MinimumUpdateTime;
                else if (value > MaximumUpdateTime)
                    value = MaximumUpdateTime;
                m_updateRate = (uint) (value*1000);
            }
        }

        public string Identity
        {
            set
            {
                lock (m_userMutex)
                {
                    m_identity = value;
                }
            }
        }

        //Disposable
        public void Dispose()
        {
            Instance.SetLogger(null);
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

            //Load persistent file, and ignore erros but pass along warnings.
            if (!string.IsNullOrEmpty(persistentFilename))
            {
                bool first = true;
                m_storage.LoadPersistent(persistentFilename, (line, msg) =>
                {
                    if (first)
                    {
                        first = false;
                        Warning($"When reading initial persistent values from \" {persistentFilename} \":");
                    }
                    Warning($"{persistentFilename} : {line} : {msg}");
                });
            }

            //Bind SetOutgoing
            m_storage.SetOutgoing(QueueOutgoing, m_server);

            //Start our threads
            m_dispatchThread = new Thread(DispatchThreadMain);
            m_dispatchThread.IsBackground = true;
            m_dispatchThread.Name = "Dispatch Thread";
            m_dispatchThread.Start();

            m_clientServerThread = new Thread(ServerThreadMain);
            m_clientServerThread.IsBackground = true;
            m_clientServerThread.Name = "Client Server Thread";
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

            //Bind SetOutgoing
            m_storage.SetOutgoing(QueueOutgoing, m_server);

            //Start our threads
            m_dispatchThread = new Thread(DispatchThreadMain);
            m_dispatchThread.IsBackground = true;
            m_dispatchThread.Name = "Dispatch Thread";
            m_dispatchThread.Start();

            m_clientServerThread = new Thread(ClientThreadMain);
            m_clientServerThread.IsBackground = true;
            m_clientServerThread.Name = "Client Server Thread";
            m_clientServerThread.Start(connect);
        }

        public void Stop()
        {
            m_active = false;

            // Wake up dispatch thread with a flush
            m_flushCv.Set();

            //Wake up client thread with a reconnected
            ClientReconnect();

            //Wake up server thread by a socket shutdown
            m_serverAccepter?.Shutdown();

            if (m_dispatchThread != null)
            {
                //Join our dispatch thread.
                bool shutdown = m_dispatchThread.Join(TimeSpan.FromSeconds(1));
                //If it fails to join, abort the thread
                if (!shutdown) m_dispatchThread.Abort();
            }
            if (m_clientServerThread != null)
            {
                //Join our Client Server Thread
                bool shutdown = m_clientServerThread.Join(TimeSpan.FromSeconds(1));
                //If it fails to join, abort the thread.
                if (!shutdown) m_clientServerThread.Abort();
            }
            List<NetworkConnection> conns = new List<NetworkConnection>();
            lock (m_userMutex)
            {
                var tmp = m_connections;
                m_connections = conns;
                conns = tmp;
            }

            //Dispose and close all connections
            foreach (var networkConnection in conns)
            {
                networkConnection.Dispose();
            }

            m_connections.Clear();
        }

        public void Flush()
        {
            var now = DateTime.UtcNow;
            lock (m_flushMutex)
            {
                if (now - m_lastFlush < TimeSpan.FromMilliseconds(100))
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

        public void NotifyConnections(ConnectionListenerCallback callback)
        {
            lock (m_userMutex)
            {
                foreach (var conn in m_connections)
                {
                    if (conn.GetState() != NetworkConnection.State.kActive) continue;
                    m_notifier.NotifyConnection(true, conn.GetConnectionInfo(), callback);
                }
            }
        }

        private void DispatchThreadMain()
        {
            var timeoutTime = DateTime.UtcNow;

            var nextSaveTime = timeoutTime + s_saveDeltaTime;

            int count = 0;

            bool lockEntered = false;
            try
            {
                Monitor.Enter(m_flushMutex, ref lockEntered);
                while (m_active)
                {
                    //Handle loop taking too long
                    var start = DateTime.UtcNow;
                    if (start > timeoutTime)
                        timeoutTime = start;
                    //Wait for periodic or when flushed
                    timeoutTime += TimeSpan.FromMilliseconds(m_updateRate);
                    TimeSpan waitTime = timeoutTime - start;
                    m_flushCv.WaitTimeout(m_flushMutex, ref lockEntered, waitTime,
                        () => { return !m_active || m_doFlush; });
                    m_doFlush = false;
                    if (!m_active) break; //in case we were woken up to terminate

                    if (m_server && !string.IsNullOrEmpty(m_persistFilename) && start > nextSaveTime)
                    {
                        nextSaveTime += s_saveDeltaTime;
                        //Handle loop taking too long
                        if (start > nextSaveTime) nextSaveTime = start + s_saveDeltaTime;
                        string err = m_storage.SavePersistent(m_persistFilename, true);
                        if (err != null)
                        {
                            Warning($"periodic persistent save: {err}");
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
                            //Post outgoing messages if connection is active
                            //only send keep-alives on client
                            if (conn.GetState() == NetworkConnection.State.kActive)
                                conn.PostOutgoing(!m_server);

                            //if client, reconnect if connection died
                            if (!m_server && conn.GetState() == NetworkConnection.State.kDead)
                                reconnect = true;
                        }

                        //reconnect if we disconnected and a reconnect is not in progress
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
            //If we were passed object, something bad has broken
            //Fail fast
            Func<INetworkStream> connect = o as Func<INetworkStream>;
            if (connect == null)
            {
                //TODO: Switch to fail fast
                throw new Exception("Client not passed a correct variable");
            }

            while (m_active)
            {
                //Sleep between retries
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
                    foreach (var s in m_connections) //Disconnect any current
                    {
                        s.Dispose();
                    }

                    m_connections.Clear();

                    m_connections.Add(conn);

                    conn.SetProtoRev(m_reconnectProtoRev);

                    conn.Start();

                    m_doReconnect = false;
                    m_reconnectCv.Wait(m_userMutex, ref lockEntered, () => { return !m_active || m_doReconnect; });
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
            lock (m_userMutex)
            {
                selfId = m_identity;
            }

            Debug("client: sending hello");
            sendMsgs(new[] {Message.ClientHello(selfId)});

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

            for (;;)
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
                    Debug(
                        $"client: received message ({msg.Type()}) other then entry assignment during initial handshake");
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
                sendMsgs(new[] {Message.ProtoUnsup()});
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

            outgoing.Add(Message.ServerHelloDone());

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
                        Debug(
                            $"server: received message ({msg.Type()}) other than entry assignment during initial handshake");
                        return false;
                    }

                    incoming.Add(msg);

                    msg = getMsg();
                }

                foreach (var m in incoming)
                {
                    m_storage.ProcessIncoming(m, conn);
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
    }

    internal class Dispatcher : DispatcherBase
    {
        private static Dispatcher s_instance;

        private Dispatcher() : this(Storage.Instance, Notifier.Instance)
        {
        }

        private Dispatcher(Storage storage, Notifier notifier)
            : base(storage, notifier)
        {
        }

        /// <summary>
        /// Gets the local instance of Dispatcher
        /// </summary>
        public static Dispatcher Instance
        {
            get
            {
                if (s_instance == null)
                {
                    Dispatcher d = new Dispatcher();
                    Interlocked.CompareExchange(ref s_instance, d, null);
                }
                return s_instance;
            }
        }

        public void StartServer(string persistentFilename, string listenAddress, int port)
        {
            StartServer(persistentFilename, new TCPAcceptor(port, listenAddress));
        }

        public void StartClient(string serverName, int port)
        {
            StartClient(() => TCPConnector.Connect(serverName, port, 1));
        }
    }
}