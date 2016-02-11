using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetworkTables.Support;
using NetworkTables.TcpSockets;

namespace NetworkTables
{
    internal class NetworkConnection : IDisposable
    {

        private struct Pair
        {
            public int First { get; private set; }
            public int Second { get; private set; }

            public void SetFirst(int first)
            {
                First = first;
            }

            public void SetSecond(int second)
            {
                Second = second;
            }
        }

        private uint m_protoRev;

        public uint ProtoRev => m_protoRev;

        public enum State { kCreated, kInit, kHandshake, kSynchronized, kActive, kDead };

        public delegate bool HandshakeFunc(NetworkConnection conn, Func<Message> getMsg, Action<Message[]> sendMsgs);

        public delegate void ProcessIncomingFunc(Message msg, NetworkConnection conn);

        private static uint s_uid;

        private uint m_uid;

        private INetworkStream m_stream;

        private Notifier m_notifier;

        private ConcurrentQueue<List<Message>> m_outgoing = new ConcurrentQueue<List<Message>>();

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

        private DateTime m_lastPost = DateTime.UtcNow;

        private readonly object m_pendingMutex = new object();

        private readonly object m_remoteIdMutex = new object();

        private List<Message> m_pendingOutgoing = new List<Message>();

        private List<Pair> m_pendingUpdate = new List<Pair>();

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
            m_writeThread.IsBackground = true;
            m_writeThread.Name = "Connection Write Thread";
            m_writeThread.Start();

            m_readThread = new Thread(ReadThreadMain);
            m_readThread.IsBackground = true;
            m_readThread.Name = "Connection Read Thread";
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

        public ConnectionInfo GetConnectionInfo()
        {
            return new ConnectionInfo(RemoteId(), m_stream.GetPeerIP(), m_stream.GetPeerPort(), (int)m_lastUpdate, m_protoRev);
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
            lock (m_pendingMutex)
            {
                Message.MsgType type = msg.Type();

                switch (type)
                {
                    case Message.MsgType.kEntryAssign:
                    case Message.MsgType.kEntryUpdate:
                        {
                            uint id = msg.Id();
                            if (id == 0xffff)
                            {
                                m_pendingOutgoing.Add(msg);
                                break;
                            }
                            if (id < m_pendingUpdate.Count && m_pendingUpdate[(int)id].First != 0)
                            {
                                var oldmsg = m_pendingOutgoing[m_pendingUpdate[(int)id].First];
                                if (oldmsg != null && oldmsg.Is(Message.MsgType.kEntryAssign) &&
                                    msg.Is(Message.MsgType.kEntryUpdate))
                                {
                                    m_pendingOutgoing[m_pendingUpdate[(int)id].First] = Message.EntryAssign(oldmsg.Str(), id, msg.SeqNumUid(), msg.Val(),
                                        (EntryFlags)oldmsg.Flags());

                                }
                                else
                                {
                                    m_pendingOutgoing[m_pendingUpdate[(int)id].First] = msg;
                                }
                            }
                            else
                            {
                                int pos = m_pendingOutgoing.Count;
                                m_pendingOutgoing.Add(msg);
                                if (id >= m_pendingUpdate.Count) m_pendingUpdate.Add(new Pair());
                                m_pendingUpdate[(int)id].SetFirst(pos + 1);
                            }
                            break;
                        }
                    case Message.MsgType.kEntryDelete:
                        {
                            uint id = msg.Id();
                            if (id == 0xffff)
                            {
                                m_pendingOutgoing.Add(msg);
                                break;
                            }

                            if (id < m_pendingUpdate.Count)
                            {
                                if (m_pendingUpdate[(int)id].First != 0)
                                {
                                    m_pendingOutgoing[m_pendingUpdate[(int)id].First - 1] = new Message();
                                    m_pendingUpdate[(int)id].SetFirst(0);
                                }
                                if (m_pendingUpdate[(int)id].Second != 0)
                                {
                                    m_pendingOutgoing[m_pendingUpdate[(int)id].Second - 1] = new Message();
                                    m_pendingUpdate[(int)id].SetSecond(0);
                                }
                            }

                            m_pendingOutgoing.Add(msg);
                            break;
                        }
                    case Message.MsgType.kFlagsUpdate:
                        {
                            uint id = msg.Id();
                            if (id == 0xffff)
                            {
                                m_pendingOutgoing.Add(msg);
                                break;
                            }

                            if (id < m_pendingUpdate.Count && m_pendingUpdate[(int)id].Second != 0)
                            {
                                m_pendingOutgoing[m_pendingUpdate[(int)id].Second - 1] = msg;
                            }
                            else
                            {
                                int pos = m_pendingOutgoing.Count;
                                m_pendingOutgoing.Add(msg);
                                if (id > m_pendingUpdate.Count) m_pendingUpdate.Add(new Pair());
                                m_pendingUpdate[(int)id].SetSecond(pos + 1);

                            }
                            break;
                        }
                    case Message.MsgType.kClearEntries:
                        {
                            for (int i = 0; i < m_pendingOutgoing.Count; i++)
                            {
                                var message = m_pendingOutgoing[i];
                                if (message == null) continue;
                                var t = message.Type();
                                if (t == Message.MsgType.kEntryAssign || t == Message.MsgType.kEntryUpdate
                                    || t == Message.MsgType.kFlagsUpdate || t == Message.MsgType.kEntryDelete
                                    || t == Message.MsgType.kClearEntries)
                                {
                                    m_pendingOutgoing[i] = new Message();
                                }
                            }
                            m_pendingUpdate.Clear();
                            m_pendingOutgoing.Add(msg);
                            break;
                        }
                    default:
                        m_pendingOutgoing.Add(msg);
                        break;
                }
            }
        }

        public void PostOutgoing(bool keepAlive)
        {
            lock (m_pendingMutex)
            {
                var now = DateTime.UtcNow;
                if (m_pendingOutgoing.Count == 0)
                {
                    if (!keepAlive) return;
                    if ((now - m_lastPost) < TimeSpan.FromSeconds(1)) return;
                    m_outgoing.Enqueue(new List<Message> {Message.KeepAlive()});
                }
                else
                {
                    m_outgoing.Enqueue(new List<Message>(m_pendingOutgoing));
                    m_pendingOutgoing.Clear();
                    m_pendingUpdate.Clear();
                    
                }
                m_lastPost = DateTime.UtcNow;
            }
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
            SocketInputStream istream = new SocketInputStream(m_stream);
            WireDecoder decoder = new WireDecoder(istream, m_protoRev);

            m_state = State.kHandshake;

            if (!m_handshake(this, () =>
            {
                decoder.SetProtoRev(m_protoRev);
                var msg = Message.Read(decoder, m_getEntryType);
                if (msg == null && decoder.Error != null)
                {
                    //Debug
                    Console.WriteLine("Error reading in handchake: " + decoder.Error);
                }
                return msg;
            }, messages =>
            {
                m_outgoing.Enqueue(messages.ToList());
            }))
            {
                m_state = State.kDead;
                m_active = false;
                return;
            }

            m_state = State.kActive;
            m_notifier.NotifyConnection(true, GetConnectionInfo());
            while (m_active)
            {
                if (m_stream == null) break;
                decoder.SetProtoRev(m_protoRev);
                decoder.Reset();
                var msg = Message.Read(decoder, m_getEntryType);
                if (msg == null)
                {
                    if (decoder.Error != null) //Debug;
                    {

                    }
                    m_stream?.Close();
                    break;
                }
                //Debug
                m_lastUpdate = Timestamp.Now();
                m_processIncoming(msg, this);
            }

            //Debug
            if (m_state != State.kDead) m_notifier.NotifyConnection(false, GetConnectionInfo());
            m_active = false;
            m_outgoing.Enqueue(new List<Message>()); // Also kill write thread
        }

        private void WriteThreadMain()
        {
            WireEncoder encoder = new WireEncoder(m_protoRev);

            while (m_active)
            {
                List<Message> messages;
                bool validQueueData = m_outgoing.TryDequeue(out messages);

                if (!validQueueData) continue;
                encoder.SetProtoRev(m_protoRev);
                encoder.Reset();
                //Debug
                foreach (var message in messages)
                {
                    if (message != null)
                    {
                        //Debug
                        message.Write(encoder);
                    }
                }

                NetworkStreamError err = NetworkStreamError.kConnectionClosed;
                if (m_stream == null) break;
                if (encoder.Size() == 0) continue;
                if (m_stream.Send(encoder.Buffer, 0, encoder.Size(), ref err) == 0) break;
                //Debug
            }
            //Debug
            if (m_state != State.kDead) m_notifier.NotifyConnection(false, GetConnectionInfo());
            m_state = State.kDead;
            m_active = false;
            m_stream?.Close();
        }

    }
}
