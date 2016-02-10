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
        private class StoragePair : IComparable<StoragePair>
        {
            public string First { get; }
            public Value Second { get; }

            public StoragePair(string first, Value second)
            {
                First = first;
                Second = second;
            }

            public int CompareTo(StoragePair other)
            {
                return First.CompareTo(other.First);
            }
        }

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

        private class Entry
        {
            public Entry(string name)
            {
                this.name = name;
                flags = 0;
                id = 0xffff;
                value = null;
                seqNum = new SequenceNumber();
            }

            internal bool IsPersistent() => (flags & (uint)EntryFlags.Persistent) != 0;

            internal string name;
            internal Value value;
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

        private bool GetPersistentEntries(bool periodic, List<StoragePair> entries)
        {
            lock (m_mutex)
            {
                if (periodic && !m_persistentDirty) return false;
                m_persistentDirty = false;
                foreach (var i in m_entries)
                {
                    Entry entry = i.Value;
                    if (!entry.IsPersistent()) continue;
                    entries.Add(new StoragePair(i.Key, entry.value));
                }
            }
            entries.Sort();
            return true;
        }

        private static void SavePersistentImpl(StreamWriter stream, StoragePair[] entries)
        {
            string base64_encoded;

            stream.Write("[NetworkTables Storage 3.0]\n");
            foreach (var i in entries)
            {
                var v = i.Second;
                if (v == null) continue;
                switch (v.Type)
                {
                    case NtType.Boolean:
                        stream.Write("boolean");
                        break;
                    //TODO: The rest of these.
                    default:
                        continue;
                }

                WriteString(stream, i.First);

                stream.Write('=');

                switch (v.Type)
                {
                    case NtType.Boolean:
                        stream.Write();
                        break;
                }

                stream.Write('\n');
            }
        }

        private static void WriteString(StreamWriter os, string str)
        {
            os.Write('"');
            foreach (var c in str)
            {
                switch (c)
                {
                    case '\\':
                        os.Write("\\\\");
                        break;
                    case '\t':
                        os.Write("\\t");
                        break;
                    case '\n':
                        os.Write("\\n");
                        break;
                    case '"':
                        os.Write("\\\"");
                        break;
                    default:
                        if (str.IsNormalized())
                        {
                            os.Write(c);
                            break;
                        }

                        //TODO: figure this out

                        break;
                }
                os.Write('"');
            }
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

        public void ProcessIncoming(Message msg, NetworkConnection conn)
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

                                    m_notifier.NotifyEntry(name, entry.value, (uint)NotifyFlags.NotifyNew);

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

                                        m_notifier.NotifyEntry(name, newEntry.value, (uint)NotifyFlags.NotifyNew);
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

                            uint notifyFlags = (uint)NotifyFlags.NotifyUpdate;

                            if (!mayNeedUpdate && conn.ProtoRev >= 0x0300)
                            {
                                if ((entry.flags & (uint)EntryFlags.Persistent) != (msg.Flags() & (uint)EntryFlags.Persistent))
                                {
                                    m_persistentDirty = true;
                                }
                                if (entry.flags != msg.Flags())
                                {
                                    notifyFlags |= (uint)NotifyFlags.NotifyFlagsChanged;
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
                            if (id >= m_idMap.Count || m_idMap[(int)id] == null)
                            {
                                Monitor.Exit(m_mutex);
                                lockEntered = false;
                                //Debug
                                return;
                            }

                            Entry entry = m_idMap[(int)id];

                            if (entry.flags == msg.Flags()) return;

                            if ((entry.flags & (int)EntryFlags.Persistent) !=
                                (msg.Flags() & (int)EntryFlags.Persistent))
                                m_persistentDirty = true;

                            entry.flags = msg.Flags();

                            m_notifier.NotifyEntry(entry.name, entry.value, (uint)NotifyFlags.NotifyFlagsChanged);

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
            lock (m_mutex)
            {
                conn.SetState(NetworkConnection.State.kSynchronized);
                foreach (var i in m_entries)
                {
                    Entry entry = i.Value;
                    msgs.Add(Message.EntryAssign(i.Key, entry.id, entry.seqNum.Value(), entry.value, entry.flags));
                }
            }
        }

        public void ApplyInitialAssignments(NetworkConnection conn, Message[] msgs, bool newServer, List<Message> outMsgs)
        {
            bool lockEntered = false;
            try
            {
                Monitor.Enter(m_mutex, ref lockEntered);
                if (m_server) return;

                conn.SetState(NetworkConnection.State.kSynchronized);

                List<Message> updateMsgs = new List<Message>();

                foreach (var i in m_entries) i.Value.id = 0xffff;

                m_idMap.Clear();

                foreach (var msg in msgs)
                {
                    if (!msg.Is(Message.MsgType.kEntryAssign))
                    {
                        //Debug
                        continue;
                    }

                    uint id = msg.Id();

                    if (id == 0xffff)
                    {
                        //Debug
                        continue;
                    }

                    SequenceNumber seqNum = new SequenceNumber(msg.SeqNumUid());
                    string name = msg.Str();


                    Entry entry;
                    if (!m_entries.TryGetValue(name, out entry))
                    {
                        entry = new Entry(name);
                        entry.value = msg.Value();
                        entry.flags = msg.Flags();
                        entry.seqNum = seqNum;
                        m_notifier.NotifyEntry(name, entry.value, (uint)NotifyFlags.NotifyNew);
                        m_entries.Add(name, entry);
                    }
                    else
                    {
                        if (!newServer && seqNum <= entry.seqNum)
                        {
                            updateMsgs.Add(Message.EntryUpdate(entry.id, entry.seqNum.Value(), entry.value));
                        }
                        else
                        {
                            entry.value = msg.Value();
                            entry.seqNum = seqNum;
                            uint notifyFlags = (uint)NotifyFlags.NotifyUpdate;

                            if (conn.ProtoRev >= 0x0300)
                            {
                                if (entry.flags != msg.Flags()) notifyFlags |= (uint)NotifyFlags.NotifyFlagsChanged;
                                entry.flags = msg.Flags();
                            }

                            m_notifier.NotifyEntry(name, entry.value, notifyFlags);

                        }
                    }

                    entry.id = id;
                    if (id >= m_idMap.Count) m_idMap.Add(entry);


                }

                foreach (var i in m_entries)
                {
                    Entry entry = i.Value;
                    if (entry.id != 0xffff) continue;
                    outMsgs.Add(Message.EntryAssign(entry.name, entry.id, entry.seqNum.Value(), entry.value, entry.flags));
                }

                var queueOutgoing = m_queueOutgoing;
                Monitor.Exit(m_mutex);
                lockEntered = false;
                foreach (var msg in updateMsgs) queueOutgoing(msg, null, null);
            }
            finally
            {
                if (lockEntered) Monitor.Exit(m_mutex);
            }
        }

        public Value GetEntryValue(string name)
        {
            lock (m_mutex)
            {
                Entry entry;
                if (m_entries.TryGetValue(name, out entry))
                {
                    //Grabbed
                    return entry.value;
                }
                else
                {
                    return null;
                }
            }
        }

        public bool SetEntryValue(string name, Value value)
        {
            if (string.IsNullOrEmpty(name)) return true;
            if (value == null) return true;
            bool lockEntered = false;
            try
            {
                Monitor.Enter(m_mutex, ref lockEntered);
                Entry entry;
                if (!m_entries.TryGetValue(name, out entry)) entry = new Entry(name);
                var oldValue = entry.value;
                if (oldValue != null && oldValue.Type != value.Type)
                {
                    return false; //Type mismatch error;
                }
                entry.value = value;

                if (m_server && entry.id == 0xffff)
                {
                    uint id = (uint)m_idMap.Count;
                    entry.id = id;
                    m_idMap.Add(entry);
                }

                if (entry.IsPersistent() && oldValue != value) m_persistentDirty = true;

                if (m_notifier.LocalNotifiers())
                {
                    if (oldValue == null)
                    {
                        m_notifier.NotifyEntry(name, value, (uint)(NotifyFlags.NotifyNew | NotifyFlags.NotifyLocal));
                    }
                    else if (oldValue != value)
                    {
                        m_notifier.NotifyEntry(name, value, (uint)(NotifyFlags.NotifyUpdate | NotifyFlags.NotifyLocal));
                    }
                }

                if (m_queueOutgoing == null) return true;
                var queueOutgoing = m_queueOutgoing;
                if (oldValue != null)
                {
                    var msg = Message.EntryAssign(name, entry.id, entry.seqNum.Value(), value, entry.flags);
                    Monitor.Exit(m_mutex);
                    lockEntered = false;
                    queueOutgoing(msg, null, null);
                }
                else if (oldValue != value)
                {
                    ++entry.seqNum;
                    if (entry.id != 0xffff)
                    {
                        var msg = Message.EntryUpdate(entry.id, entry.seqNum.Value(), value);
                        Monitor.Exit(m_mutex);
                        lockEntered = false;
                        queueOutgoing(msg, null, null);
                    }
                }
                return true;
            }
            finally
            {
                if (lockEntered) Monitor.Exit(m_mutex);
            }
        }

        public void SetEntryTypeValue(string name, Value value)
        {

        }

        public void SetEntryFlags(string name, uint flags)
        {

        }

        public uint GetEntryFlags(string name)
        {
            lock (m_mutex)
            {
                Entry entry;
                if (m_entries.TryGetValue(name, out entry))
                {
                    //Grabbed
                    return entry.flags;
                }
                else
                {
                    return 0;
                }
            }
        }

        public void DeleteEntry(string name)
        {
            bool lockEntered = false;
            try
            {
                Monitor.Enter(m_mutex, ref lockEntered);
                Entry entry;
                if (!m_entries.TryGetValue(name, out entry)) return;
                uint id = entry.id;
                if (entry.IsPersistent()) m_persistentDirty = true;

                m_entries.Remove(name);

                if (id < m_idMap.Count) m_idMap[(int)id] = null;
                if (entry.value == null) return;

                m_notifier.NotifyEntry(name, entry.value, (uint)(NotifyFlags.NotifyDelete | NotifyFlags.NotifyLocal));

                if (id != 0xffff)
                {
                    if (m_queueOutgoing == null) return;
                    var queueOutgoing = m_queueOutgoing;
                    Monitor.Exit(m_mutex);
                    lockEntered = false;
                    queueOutgoing(Message.EntryDelete(id), null, null);
                }
            }
            finally
            {
                if (lockEntered) Monitor.Exit(m_mutex);
            }
        }

        public void DeleteAllEntries()
        {
            bool lockEntered = false;
            try
            {
                Monitor.Enter(m_mutex, ref lockEntered);
                if (m_entries.Count == 0) return;

                m_idMap.Clear();

                m_persistentDirty = true;

                if (m_notifier.LocalNotifiers())
                {
                    foreach (var entry in m_entries)
                    {
                        m_notifier.NotifyEntry(entry.Key, entry.Value.value, (uint)(NotifyFlags.NotifyDelete | NotifyFlags.NotifyLocal));
                    }
                }
                if (m_queueOutgoing == null) return;
                var queueOutgoing = m_queueOutgoing;
                Monitor.Exit(m_mutex);
                lockEntered = false;
                queueOutgoing(Message.ClearEntries(), null, null);
            }
            finally
            {
                if (lockEntered) Monitor.Exit(m_mutex);
            }
        }

        public List<EntryInfo> GetEntryInfo(string prefix, uint types)
        {
            lock (m_mutex)
            {
                List<EntryInfo> infos = new List<EntryInfo>();
                foreach (var i in m_entries)
                {
                    if (!i.Key.StartsWith(prefix)) continue;
                    Entry entry = i.Value;
                    var value = entry.value;
                    if (value == null) continue;
                    if (types != 0 && (types & (uint)value.Type) == 0) continue;
                    EntryInfo info = new EntryInfo(i.Key, value.Type, (EntryFlags)entry.flags, (uint)value.LastChange);
                    infos.Add(info);
                }
                return infos;
            }
        }

        public void NotifyEntries(string prefix, Notifier.EntryListenerCallback only = null)
        {
            lock (m_mutex)
            {
                foreach (var i in m_entries)
                {
                    if (!i.Key.StartsWith(prefix)) continue;
                    m_notifier.NotifyEntry(i.Key, i.Value.value, (uint)NotifyFlags.NotifyImmediate, only);
                }
            }
        }

        public string SavePersistent(string filename, bool periodic)
        {
            string fn = filename;
            string tmp = filename;

            tmp += ".tmp";
            string bak = filename;
            bak += ".bak";

            List<StoragePair> entries = new List<StoragePair>();
            if (!GetPersistentEntries(periodic, entries)) return null;

            string err = null;
            try
            {
                using (StreamWriter writer = new StreamWriter(tmp))
                {
                    //Debug
                    SavePersistentImpl(writer, entries.ToArray());
                    writer.Flush();
                }
            }
            catch (IOException)
            {
                err = "could not open file";
                goto done;
            }

            File.Delete(bak);
            File.Move(fn, bak);
            try
            {
                File.Move(tmp, fn);
            }
            catch (IOException)
            {
                File.Move(bak, fn);
                err = "could not rename temp file to real file";
                goto done;
            }

            done:

            if (err != null && periodic) m_persistentDirty = true;
            return err;
        }

        public string LoadPersistent(string filename, Action<int, string> warn)
        {

        }

        public void SavePersistent(Stream stream, bool periodic)
        {
            List<StoragePair> entries = new List<StoragePair>();
            if (!GetPersistentEntries(periodic, entries)) return;
            SavePersistentImpl(new StreamWriter(stream), entries.ToArray());
        }

        public bool LoadPersistent(Stream stream, Action<int, string> warn)
        {

        }


    }
}
