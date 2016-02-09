using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetworkTables.TcpSockets;

namespace NetworkTables
{
    public class NetworkConnection : IDisposable
    {

        private uint m_protoRev;

        public uint ProtoRev => m_protoRev;

        public enum State { kCreated, kInit, kHandshake, kSynchronized, kActive, kDead };

        public delegate bool HandshakeFunc(NetworkConnection conn, Func<Message> getMsg, Action<Message[]> sendMsgs);

        public delegate void ProcessIncomingFunc(Message msg, NetworkConnection conn);

        private static uint s_uid;

        private uint m_uid;

        private INetworkStream m_stream;

        private Notifier m_notifier;

        private ConcurrentQueue<List<Message>> m_outgoing;

        private HandshakeFunc m_handshake;

        private Message.GetEntryTypeFunc m_getEntryType;

        private ProcessIncomingFunc m_processIncoming;

        private Thread m_readThread;
        private Thread m_writeThread;

        private bool m_active;
        private State m_state;

        private readonly object m_mutex = new object();

        private string m_remoteId;

        private ulong m_lastUpdate;

        private DateTime m_lastPost;

        private readonly object m_pendingMutex = new object();

        private readonly object m_remoteIdMutex = new object();
        
        private List<Message> m_pendingOutgoing = new List<Message>(); 

        public NetworkConnection(INetworkStream stream, Notifier notifier, HandshakeFunc handshake,
            Message.GetEntryTypeFunc getEntryType)
        {
            m_uid = s_uid + 1;
            m_stream = stream;
            m_notifier = notifier;
            m_handshake = handshake;
            m_getEntryType = getEntryType;

            m_active = false;
            m_protoRev = 0x0300;
            m_state = State.kCreated;

            m_stream.SetNoDelay();
        }

        public void Dispose()
        {
            Stop();
        }

        public void SetProcessIncoming(ProcessIncomingFunc func)
        {
            m_processIncoming = func;
        }

        public void Start()
        {
            if (m_active) return;
            m_active = true;
            m_state = State.kInit;
            List<Message> temp = new List<Message>();
            while (!m_outgoing.IsEmpty) m_outgoing.TryDequeue(out temp);
            
            m_writeThread = new Thread(WriteThreadMain);
            m_writeThread.Start();

            m_readThread = new Thread(ReadThreadMain);
            m_readThread.Start();
        }

        public void Stop()
        {
            m_state = State.kDead;

            m_active = false;

            m_stream?.Close();
            List<Message> temp = new List<Message>();
            m_outgoing.Enqueue(temp);

            bool writeJoined = m_writeThread.Join(TimeSpan.FromSeconds(1));
            if (!writeJoined)
            {
                m_writeThread.Abort();
            }

            bool readJoined = m_readThread.Join(TimeSpan.FromSeconds(1));
            if (!readJoined)
            {
                m_readThread.Abort();
            }

            while (!m_outgoing.IsEmpty) m_outgoing.TryDequeue(out temp);
        }

        public ConnectionInfo Info()
        {
            ConnectionInfo info = new ConnectionInfo
            {
                remote_id = RemoteId(),
                remote_port = (uint) m_stream.GetPeerPort(),
                remote_name = m_stream.GetPeerIP(),
                last_update = m_lastUpdate,
                protocol_version = m_protoRev
            };
            return info;
        }

        public bool Active()
        {
            return m_active;
        }

        public INetworkStream Stream()
        {
            return m_stream;
        }

        public void QueueOutgoing(Message msg)
        {
            
        }

        public void PostOutgoing(bool keepAlive)
        {
            
        }

        public uint Uid()
        {
            return m_uid;
        }

        public void SetProtoRev(uint protoRev)
        {
            m_protoRev = protoRev;
        }

        public State GetState()
        {
            return m_state;
        }

        public void SetState(State state)
        {
            m_state = state;
        }

        public string RemoteId()
        {
            lock (m_remoteIdMutex)
            {
                return m_remoteId;
            }
        }

        public void SetRemoteId(string remoteId)
        {
            lock (m_remoteIdMutex)
            {
                m_remoteId = remoteId;
            }
        }

        public ulong LastUpdate() => m_lastUpdate;


        private void ReadThreadMain()
        {
            
            WireDecoder decoder = new WireDecoder();

            m_state = State.kHandshake;

            if (!m_handshake(this, () =>
            {
                decoder.ProtoRev = m_protoRev;
                var msg = Message.Read(decoder, m_getEntryType);

            }))
        }

        private void WriteThreadMain()
        {
            
        }

    }
}
