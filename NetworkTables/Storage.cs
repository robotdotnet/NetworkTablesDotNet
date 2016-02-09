using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static NetworkTables.Message.MsgType;

namespace NetworkTables
{
    public class Storage
    {
        private static Storage s_instance;

        public static Storage Instance
        {
            get
            {
                return (s_instance ?? new Storage());
            }
        }

        private Storage() : this(Notifier.Instance)
        {

        }

        private Storage(Notifier notifier)
        {
            m_notifier = notifier;
        }

        class Entry
        {
            public Entry(string name)
            {
                this.name = name;
                flags = 0;
                id = 0xffff;
                value = null;
                seqNum = new SequenceNumber();
            }

            internal bool IsPersistent() => (flags & (uint)NT_EntryFlags.NT_PERSISTENT) != 0;

            internal string name;
            internal NTValue value;
            internal uint flags;
            internal uint id;

            internal SequenceNumber seqNum;

        }

        private Dictionary<string, Entry> m_entries = new Dictionary<string, Entry>();
        private List<Entry> m_idMap = new List<Entry>();

        private readonly object m_mutex = new object();

        QueueOutgoingFunc m_queueOutgoing;
        bool m_server = true;

        bool m_persistentDirty = false;

        Notifier m_notifier;

        private bool GetPersistentEntries(bool periodic, List<Tuple<string, NTValue>> entries)
        {

        }

        public delegate void QueueOutgoingFunc(Message msg, NetworkConnection only, NetworkConnection except);

        public void SetOutgoing(QueueOutgoingFunc queueOutgoing, bool server)
        {
            lock (m_mutex)
            {
                m_queueOutgoing = queueOutgoing;
                m_server = server;
            }
        }

        public void ClearOutgoing()
        {
            m_queueOutgoing = null;
        }

        public NtType GetEntryType(uint id)
        {
            lock (m_mutex)
            {
                if (id >= m_idMap.Count) return NtType.Unassigned;
                Entry entry = m_idMap[(int)id];
                if (entry == null || entry.value == null) return NtType.Unassigned;
                return entry.value.Type;
            }
        }

        public void ProcessIncoming(Message msg, NetworkConnection conn, NetworkConnection conn_weak)
        {
            bool lockEntered = false;
            try
            {
                Monitor.Enter(m_mutex, ref lockEntered);
                Message.MsgType type = msg.Type();
                switch (type)
                {
                    case kKeepAlive:
                        break;  // ignore
                    case kClientHello:
                    case kProtoUnsup:
                    case kServerHelloDone:
                    case kServerHello:
                    case kClientHelloDone:
                        // shouldn't get these, but ignore if we do
                        break;
                    case kEntryAssign:
                        {
                            uint id = msg.Id();
                            string name = msg.Str();
                            Entry entry;
                            bool mayNeedUpdate = false;
                            if (m_server)
                            {
                                if (id == 0xffff)
                                {
                                    if (m_entries.ContainsKey(name)) return;


                                    id = (uint)m_idMap.Count;
                                    entry = new Entry(name);
                                    entry.value = msg.Value();
                                    entry.flags = msg.Flags();
                                    entry.id = id;
                                    m_entries[name] = entry;
                                    m_idMap.Add(entry);

                                    if (entry.IsPersistent()) m_persistentDirty = true;

                                    m_notifier.NotifyEntry(name, entry.value, (uint)NT_NotifyKind.NT_NOTIFY_NEW);

                                    if (m_queueOutgoing != null)
                                    {
                                        var queueOutgoing = m_queueOutgoing;
                                        var outMsg = Message.EntryAssign(name, id, entry.seqNum.Value(), msg.Value(), msg.Flags());
                                        Monitor.Exit(m_mutex);
                                        lockEntered = false;
                                        queueOutgoing(outMsg, null, null);
                                    }

                                    return;
                                }
                                if (id >= m_idMap.Count || m_idMap[(int)id] == null)
                                {
                                    Monitor.Exit(m_mutex);
                                    lockEntered = false;
                                    //Debug
                                    return;
                                }
                                entry = m_idMap[(int)id];
                            }
                            else
                            {
                                if (id == 0xffff)
                                {
                                    Monitor.Exit(m_mutex);
                                    lockEntered = false;
                                    //Debug
                                    return;
                                }

                                entry = m_idMap[(int)id];
                                if (entry == null)
                                {
                                    Entry newEntry;
                                    if (!m_entries.ContainsKey(name))
                                    {
                                        //Entry didn't exist at all.
                                        newEntry = new Entry(name);
                                        newEntry.value = msg.Value();
                                        newEntry.flags = msg.Flags();
                                        newEntry.id = id;
                                        m_idMap[(int)id] = newEntry;
                                        m_entries[name] = newEntry;

                                        m_notifier.NotifyEntry(name, newEntry.value, (uint)NT_NotifyKind.NT_NOTIFY_NEW);
                                        return;
                                    }
                                    else
                                    {
                                        newEntry = m_entries[name];
                                    }
                                    mayNeedUpdate = true;
                                    entry = newEntry;
                                    entry.id = id;
                                    m_idMap[(int)id] = entry;

                                    if (msg.Flags() != entry.flags)
                                    {
                                        var queueOutgoing = m_queueOutgoing;
                                        var outmsg = Message.FlagsUpdate(id, entry.flags);
                                        Monitor.Exit(m_mutex);
                                        lockEntered = false;
                                        queueOutgoing(outmsg, null, null);
                                        Monitor.Enter(m_mutex, ref lockEntered);
                                    }
                                }
                            }

                            SequenceNumber seqNum = new SequenceNumber(msg.SeqNumUid());
                            if (seqNum < entry.seqNum)
                            {
                                if (mayNeedUpdate)
                                {
                                    var queueOutgoing = m_queueOutgoing;
                                    var outmsg = Message.EntryUpdate(entry.id, entry.seqNum.Value(), entry.value);
                                    Monitor.Exit(m_mutex);
                                    lockEntered = false;
                                    queueOutgoing(outmsg, null, null);
                                }
                                return;
                            }

                            if (msg.Str() != entry.name)
                            {
                                Monitor.Exit(m_mutex);
                                lockEntered = false;
                                //Debug
                                return;
                            }

                            uint notifyFlags = (uint)NT_NotifyKind.NT_NOTIFY_UPDATE;

                            if (!mayNeedUpdate && conn.ProtoRev >= 0x0300)
                            {
                                if ((entry.flags & (uint)NT_EntryFlags.NT_PERSISTENT) != (msg.Flags() & (uint)NT_EntryFlags.NT_PERSISTENT))
                                {
                                    m_persistentDirty = true;
                                }
                                if (entry.flags != msg.Flags())
                                {
                                    notifyFlags |= (uint)NT_NotifyKind.NT_NOTIFY_FLAGS;
                                }
                                entry.flags = msg.Flags();
                            }

                            if (entry.IsPersistent() && entry.value != msg.Value())
                            {
                                m_persistentDirty = true;
                            }

                            entry.value = msg.Value();
                            entry.seqNum = seqNum;

                            m_notifier.NotifyEntry(name, entry.value, notifyFlags);

                            if (m_server && m_queueOutgoing != null)
                            {
                                var queueOutgoing = m_queueOutgoing;
                                var outmsg = Message.EntryAssign(entry.name, id, msg.SeqNumUid(), msg.Value(), entry.flags);
                                Monitor.Exit(m_mutex);
                                lockEntered = false;
                                queueOutgoing(outmsg, null, conn);
                            }
                            break;
                        }
                    case kFlagsUpdate:
                        {
                            uint id = msg.Id();
                            if (id >= m_idMap.Count || m_idMap[(int) id] == null)
                            {
                                Monitor.Exit(m_mutex);
                                lockEntered = false;
                                //Debug
                                return;
                            }

                            Entry entry = m_idMap[(int) id];

                            if (entry.flags == msg.Flags()) return;

                            if ((entry.flags & (int) NT_EntryFlags.NT_PERSISTENT) !=
                                (msg.Flags() & (int) NT_EntryFlags.NT_PERSISTENT))
                                m_persistentDirty = true;

                            entry.flags = msg.Flags();

                            m_notifier.NotifyEntry(entry.name, entry.value, (uint)NT_NotifyKind.NT_NOTIFY_FLAGS);

                            if (m_server && m_queueOutgoing != null)
                            {
                                var queueOutgoing = m_queueOutgoing;
                                Monitor.Exit(m_mutex);
                                lockEntered = false;
                                queueOutgoing(msg, null, conn);
                            }
                            break;
                        }

                }
            }
            finally
            {
                if (lockEntered) Monitor.Exit(m_mutex);
            }
        }

        public void GetInitialAssignments(NetworkConnection conn, List<Message> msgs)
        {

        }

        public void ApplyInitialAssignments(NetworkConnection conn, Message[] msgs, bool newServer, List<Message> outMsgs)
        {

        }

        public NTValue GetEntryValue(string name)
        {

        }

        public bool SetEntryValue(string name, NTValue value)
        {

        }

        public void SetEntryTypeValue(string name, NTValue value)
        {

        }

        public void SetEntryFlags(string name, uint flags)
        {

        }

        public uint GetEntryFlags(string name)
        {

        }

        public void DeleteEntry(string name)
        {

        }

        public void DeleteAllEntries()
        {

        }

        public List<EntryInfo> GetEntryInfo(string prefix, uint types)
        {

        }

        public void NotifyEntries(string prefix, Notifier.EntryListenerCallback only = null)
        {

        }

        public string SavePersistent(string filename, bool periodic)
        {

        }

        public string LoadPersistent(string filename, Action<int, string> warn)
        {

        }

        public void SavePersistent(Stream stream, bool periodic)
        {

        }

        public bool LoadPersistent(Stream stream, Action<int, string> warn)
        {

        }


    }
}
