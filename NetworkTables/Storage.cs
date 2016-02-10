﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static NetworkTables.Message.MsgType;
using static NetworkTables.Logger;

namespace NetworkTables
{


    internal class Storage
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
                return s_instance ?? (s_instance = new Storage());
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

            internal bool IsPersistent() => (flags & EntryFlags.Persistent) != 0;

            internal string name;
            internal Value value;
            internal EntryFlags flags;
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
                        stream.Write(v.GetBoolean() ? "true" : "false");
                        break;
                    case NtType.Double:
                        stream.Write(v.GetDouble());
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
                SequenceNumber seqNum = null;
                Entry entry = null;
                uint id = 0;
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
                            id = msg.Id();
                            string name = msg.Str();
                            bool mayNeedUpdate = false;
                            if (m_server)
                            {
                                if (id == 0xffff)
                                {
                                    if (m_entries.ContainsKey(name)) return;


                                    id = (uint)m_idMap.Count;
                                    entry = new Entry(name);
                                    entry.value = msg.Val();
                                    entry.flags = (EntryFlags)msg.Flags();
                                    entry.id = id;
                                    m_entries[name] = entry;
                                    m_idMap.Add(entry);

                                    if (entry.IsPersistent()) m_persistentDirty = true;

                                    m_notifier.NotifyEntry(name, entry.value, NotifyFlags.NotifyNew);

                                    if (m_queueOutgoing != null)
                                    {
                                        var queueOutgoing = m_queueOutgoing;
                                        var outMsg = Message.EntryAssign(name, id, entry.seqNum.Value(), msg.Val(), (EntryFlags)msg.Flags());
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
                                    Debug("server: received assignment to unknown entry");
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
                                    Debug("client: received entry assignment request?");
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
                                        newEntry.value = msg.Val();
                                        newEntry.flags = (EntryFlags)msg.Flags();
                                        newEntry.id = id;
                                        m_idMap[(int)id] = newEntry;
                                        m_entries[name] = newEntry;

                                        m_notifier.NotifyEntry(name, newEntry.value, NotifyFlags.NotifyNew);
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

                                    if ((EntryFlags)msg.Flags() != entry.flags)
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

                            seqNum = new SequenceNumber(msg.SeqNumUid());
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
                            //Sanity check. Name should match id
                            if (msg.Str() != entry.name)
                            {
                                Monitor.Exit(m_mutex);
                                lockEntered = false;
                                Debug("entry assignment for same id with different name?");
                                return;
                            }

                            NotifyFlags notifyFlags = NotifyFlags.NotifyUpdate;

                            if (!mayNeedUpdate && conn.ProtoRev >= 0x0300)
                            {
                                if ((entry.flags & EntryFlags.Persistent) != ((EntryFlags)msg.Flags() & EntryFlags.Persistent))
                                {
                                    m_persistentDirty = true;
                                }
                                if (entry.flags != (EntryFlags)msg.Flags())
                                {
                                    notifyFlags |= NotifyFlags.NotifyFlagsChanged;
                                }
                                entry.flags = (EntryFlags)msg.Flags();
                            }

                            if (entry.IsPersistent() && entry.value != msg.Val())
                            {
                                m_persistentDirty = true;
                            }

                            entry.value = msg.Val();
                            entry.seqNum = seqNum;

                            m_notifier.NotifyEntry(name, entry.value, notifyFlags);

                            if (m_server && m_queueOutgoing != null)
                            {
                                var queueOutgoing = m_queueOutgoing;
                                var outmsg = Message.EntryAssign(entry.name, id, msg.SeqNumUid(), msg.Val(), entry.flags);
                                Monitor.Exit(m_mutex);
                                lockEntered = false;
                                queueOutgoing(outmsg, null, conn);
                            }
                            break;
                        }
                    case Message.MsgType.kEntryUpdate:
                        id = msg.Id();
                        if (id >= m_idMap.Count || m_idMap[(int)id] == null)
                        {
                            Monitor.Exit(m_mutex);
                            lockEntered = false;
                            Debug("received update to unknown entyr");
                            return;
                        }

                        entry = m_idMap[(int)id];

                        seqNum = new SequenceNumber(msg.SeqNumUid());

                        if (seqNum <= entry.seqNum) return;

                        entry.value = msg.Val();
                        entry.seqNum = seqNum;

                        if (entry.IsPersistent()) m_persistentDirty = true;
                        m_notifier.NotifyEntry(entry.name, entry.value, NotifyFlags.NotifyUpdate);

                        if (m_server && m_queueOutgoing != null)
                        {
                            var queueOutgoing = m_queueOutgoing;
                            Monitor.Exit(m_mutex);
                            lockEntered = false;
                            queueOutgoing(msg, null, conn);
                        }
                        break;
                    case kFlagsUpdate:
                        {
                            id = msg.Id();
                            if (id >= m_idMap.Count || m_idMap[(int)id] == null)
                            {
                                Monitor.Exit(m_mutex);
                                lockEntered = false;
                                Debug("reeived flags update to unknown entry");
                                return;
                            }

                            entry = m_idMap[(int)id];

                            if (entry.flags == (EntryFlags)msg.Flags()) return;

                            if ((entry.flags & EntryFlags.Persistent) !=
                                ((EntryFlags)msg.Flags() & EntryFlags.Persistent))
                                m_persistentDirty = true;

                            entry.flags = (EntryFlags)msg.Flags();

                            m_notifier.NotifyEntry(entry.name, entry.value, NotifyFlags.NotifyFlagsChanged);

                            if (m_server && m_queueOutgoing != null)
                            {
                                var queueOutgoing = m_queueOutgoing;
                                Monitor.Exit(m_mutex);
                                lockEntered = false;
                                queueOutgoing(msg, null, conn);
                            }
                            break;
                        }
                    case kEntryDelete:
                        {
                            id = msg.Id();
                            if (id >= m_idMap.Count || m_idMap[(int)id] == null)
                            {
                                Monitor.Exit(m_mutex);
                                lockEntered = false;
                                Debug("received delete to unknown entry");
                                return;
                            }


                            entry = m_idMap[(int)id];

                            if (entry.IsPersistent()) m_persistentDirty = true;

                            m_idMap[(int)id] = null;

                            if (m_entries.TryGetValue(entry.name, out entry))
                            {
                                m_entries.Remove(entry.name);

                                m_notifier.NotifyEntry(entry.name, entry.value, NotifyFlags.NotifyDelete);
                            }

                            if (m_server && m_queueOutgoing != null)
                            {
                                var queueOutgoing = m_queueOutgoing;
                                Monitor.Exit(m_mutex);
                                lockEntered = false;
                                queueOutgoing(msg, null, conn);
                            }
                            break;
                        }
                    case Message.MsgType.kClearEntries:
                        {
                            m_idMap.Clear();

                            m_persistentDirty = true;

                            foreach(var e in m_entries)
                            {
                                m_notifier.NotifyEntry(e.Key, e.Value.value, NotifyFlags.NotifyDelete);
                            }

                            m_entries.Clear();

                            if (m_server && m_queueOutgoing != null)
                            {
                                var queueOutgoing = m_queueOutgoing;
                                Monitor.Exit(m_mutex);
                                lockEntered = false;
                                queueOutgoing(msg, null, conn);
                            }
                            break;
                        }
                    default:
                        break;
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
                        entry.value = msg.Val();
                        entry.flags = (EntryFlags)msg.Flags();
                        entry.seqNum = seqNum;
                        m_notifier.NotifyEntry(name, entry.value, NotifyFlags.NotifyNew);
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
                            entry.value = msg.Val();
                            entry.seqNum = seqNum;
                            NotifyFlags notifyFlags = NotifyFlags.NotifyUpdate;

                            if (conn.ProtoRev >= 0x0300)
                            {
                                if (entry.flags != (EntryFlags)msg.Flags()) notifyFlags |= NotifyFlags.NotifyFlagsChanged;
                                entry.flags = (EntryFlags)msg.Flags();
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
                        m_notifier.NotifyEntry(name, value, (NotifyFlags.NotifyNew | NotifyFlags.NotifyLocal));
                    }
                    else if (oldValue != value)
                    {
                        m_notifier.NotifyEntry(name, value, (NotifyFlags.NotifyUpdate | NotifyFlags.NotifyLocal));
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
            if (string.IsNullOrEmpty(name)) return;
            if (value == null) return;
            bool lockEntered = false;
            try
            {
                Monitor.Enter(m_mutex, ref lockEntered);
                Entry entry = null;
                if (!m_entries.TryGetValue(name, out entry))
                {
                    entry = new Entry(name);
                    m_entries.Add(name, entry);
                }
                var oldValue = entry.value;
                entry.value = value;
                if (oldValue != null && oldValue == value) return;

                if (m_server && entry.id == 0xffff)
                {
                    int id = m_idMap.Count;
                    entry.id = (uint)id;
                    m_idMap.Add(entry);
                }

                if (entry.IsPersistent()) m_persistentDirty = true;

                if (m_notifier.LocalNotifiers())
                {
                    if (oldValue == null)
                    {
                        m_notifier.NotifyEntry(name, value, NotifyFlags.NotifyNew | NotifyFlags.NotifyLocal);
                    }
                    else
                    {
                        m_notifier.NotifyEntry(name, value, NotifyFlags.NotifyUpdate | NotifyFlags.NotifyLocal);
                    }
                }

                if (m_queueOutgoing == null) return;
                var queueOutgoing = m_queueOutgoing;
                if (oldValue == null || oldValue.Type != value.Type)
                {
                    ++entry.seqNum;
                    var msg = Message.EntryAssign(name, entry.id, entry.seqNum.Value(), value, entry.flags);
                    Monitor.Exit(m_mutex);
                    lockEntered = false;
                    queueOutgoing(msg, null, null);
                }
                else
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
            }
            finally
            {
                if (lockEntered) Monitor.Exit(m_mutex);
            }
        }

        public void SetEntryFlags(string name, EntryFlags flags)
        {
            if (string.IsNullOrEmpty(name)) return;
            bool lockEntered = false;
            try
            {
                Monitor.Enter(m_mutex, ref lockEntered);
                Entry entry = null;
                if (!m_entries.TryGetValue(name, out entry))
                {
                    //Key does not exist. Return
                    return;
                }
                if (entry.flags == flags) return;

                if ((entry.flags & EntryFlags.Persistent) != (flags & EntryFlags.Persistent))
                    m_persistentDirty = true;

                entry.flags = flags;

                m_notifier.NotifyEntry(name, entry.value, NotifyFlags.NotifyFlagsChanged | NotifyFlags.NotifyLocal);

                if (m_queueOutgoing == null) return;
                var queueOutgoing = m_queueOutgoing;
                uint id = entry.id;
                if (id != 0xffff)
                {
                    Monitor.Exit(m_mutex);
                    lockEntered = false;
                    queueOutgoing(Message.FlagsUpdate(id, flags), null, null);
                }
            }
            finally
            {
                if (lockEntered) Monitor.Exit(lockEntered);
            }
        }

        public EntryFlags GetEntryFlags(string name)
        {
            lock (m_mutex)
            {
                Entry entry;
                if (m_entries.TryGetValue(name, out entry))
                {
                    //Grabbed
                    return (EntryFlags)entry.flags;
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

                m_notifier.NotifyEntry(name, entry.value, (NotifyFlags.NotifyDelete | NotifyFlags.NotifyLocal));

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
                        m_notifier.NotifyEntry(entry.Key, entry.Value.value, (NotifyFlags.NotifyDelete | NotifyFlags.NotifyLocal));
                    }
                }
                m_entries.Clear();
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

        public List<EntryInfo> GetEntryInfo(string prefix, NtType types)
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
                    if (types != 0 && (types & value.Type) == 0) continue;
                    EntryInfo info = new EntryInfo(i.Key, value.Type, entry.flags, (uint)value.LastChange);
                    infos.Add(info);
                }
                return infos;
            }
        }

        public void NotifyEntries(string prefix, NtCore.EntryListenerCallback only = null)
        {
            lock (m_mutex)
            {
                foreach (var i in m_entries)
                {
                    if (!i.Key.StartsWith(prefix)) continue;
                    m_notifier.NotifyEntry(i.Key, i.Value.value, NotifyFlags.NotifyImmediate, only);
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

        private static void ReadStringToken(out string first, out string second, string source)
        {
            if (string.IsNullOrEmpty(source) || source[0] == '"')
            {
                first = "";
                second = source;
                return;
            }
            int size = source.Length;
            int pos;
            for (pos = 1; pos < size; ++pos)
            {
                if (source[pos] == '"' && source[pos - 1] != '\\')
                {
                    ++pos;
                    break;
                }
            }
            first = source.Substring(0, pos);
            second = source.Substring(pos);
            return;
        }

        private static bool IsXDigit(char c)
        {
            if ('0' <= c && c <= '9') return true;
            if ('a' <= c && c <= 'f') return true;
            if ('A' <= c && c <= 'F') return true;
            return false;
        }

        private static int FromXDigit(char ch)
        {
            if (ch >= 'a' && ch <= 'f')
                return (ch - 'a' + 10);
            else if (ch >= 'A' && ch <= 'F')
                return (ch - 'A' + 10);
            else
                return ch - '0';
        }

        private static void UnescapeString(string source, out string dest)
        {
            if (!(source.Length >= 2 && source[0] == '"' && source[source.Length - 1] == '"'))
            {
                throw new ArgumentOutOfRangeException(nameof(source), "Source not correct");
            }

            StringBuilder builder = new StringBuilder(source.Length - 2);
            int s = 1;
            int end = source.Length - 1;

            for (; s != end; ++s)
            {
                if (source[s] != '\\')
                {
                    builder.Append(source[s]);
                    continue;
                }
                switch (source[++s])
                {
                    case '\\':
                    case '"':
                        builder.Append(source[s - 1]);
                        break;
                    case 't':
                        builder.Append('\t');
                        break;
                    case 'n':
                        builder.Append('\n');
                        break;
                    case 'x':
                        if (!IsXDigit(source[s + 1]))
                        {
                            builder.Append('x');
                            break;
                        }
                        int ch = FromXDigit(source[++s]);
                        if (IsXDigit(source[s + 1]))
                        {
                            ch <<= 4;
                            ch |= FromXDigit(source[++s]);
                        }
                        builder.Append((char)ch);
                        break;
                    default:
                        builder.Append(source[s - 1]);
                        break;
                }
            }
            dest = builder.ToString();
        }

        public string LoadPersistent(string filename, Action<int, string> warn)
        {
            try
            {
                using (Stream stream = new FileStream(filename, FileMode.Open))
                {
                    if (!LoadPersistent(stream, warn)) return "error reading file";
                    return null;
                }
            }
            catch (FileNotFoundException)
            {
                return "could not open file";
            }

        }

        public void SavePersistent(Stream stream, bool periodic)
        {
            List<StoragePair> entries = new List<StoragePair>();
            if (!GetPersistentEntries(periodic, entries)) return;
            SavePersistentImpl(new StreamWriter(stream), entries.ToArray());
        }

        public bool LoadPersistent(Stream stream, Action<int, string> warn)
        {
            string lineStr;
            int lineNum = 1;

            List<StoragePair> entries = new List<StoragePair>();

            string name = null, str = null;

            List<bool> boolArray = new List<bool>();
            List<double> doubleArray = new List<double>();
            List<string> stringArray = new List<string>();

            using (StreamReader reader = new StreamReader(stream))
            {
                while ((lineStr = reader.ReadLine()) != null)
                {
                    string line = lineStr.Trim();
                    if (line != string.Empty && line[0] != ';' && line[0] != '#')
                    {
                        break;
                    }
                }

                if (lineStr != "[NetworkTables Storage 3.0]")
                {
                    warn?.Invoke(lineNum, "header line mismatch, ignoring rest of file");
                    return false;
                }

                while ((lineStr = reader.ReadLine()) != null)
                {
                    string line = lineStr.Trim();
                    ++lineNum;

                    if (line == string.Empty || line[0] == ';' || line[0] == '#')
                    {
                        continue;
                    }

                    string typeTok;
                    string[] split = line.Split(new[] { ' ' }, 2);
                    typeTok = split[0];
                    line = split[1];
                    NtType type = NtType.Unassigned;
                    if (typeTok == "boolean") type = NtType.Boolean;
                    else if (typeTok == "double") type = NtType.Double;
                    else if (typeTok == "string") type = NtType.String;
                    else if (typeTok == "raw") type = NtType.Raw;
                    else if (typeTok == "array")
                    {
                        string arrayTok;
                        split = line.Split(new[] { ' ' }, 2);
                        arrayTok = split[0];
                        line = split[1];
                        if (arrayTok == "boolean") type = NtType.BooleanArray;
                        else if (arrayTok == "double") type = NtType.DoubleArray;
                        else if (arrayTok == "string") type = NtType.StringArray;
                    }

                    if (type == NtType.Unassigned)
                    {
                        warn?.Invoke(lineNum, "unrecognized type");
                        continue;
                    }

                    string nameTok;
                    ReadStringToken(out nameTok, out line, line);
                    if (string.IsNullOrEmpty(nameTok))
                    {
                        warn?.Invoke(lineNum, "unterminated name string");
                        continue;
                    }
                    UnescapeString(nameTok, out name);

                    line = line.TrimStart('\t');
                    if (string.IsNullOrEmpty(line) || line[0] != '=')
                    {
                        warn?.Invoke(lineNum, "expected = after name");
                        continue;
                    }
                    line = line.Substring(1).TrimStart(' ', '\t');

                    Value value = null;
                    switch (type)
                    {
                        case NtType.Boolean:
                            if (line == "true")
                                value = Value.MakeBoolean(true);
                            else if (line == "false")
                                value = Value.MakeBoolean(false);
                            else
                            {
                                warn?.Invoke(lineNum, "unrecognized boolean value, not 'true' or 'false'");
                                goto nextLine;
                            }
                            break;
                        default:
                            break;
                    }
                    if (name.Length != 0 && value != null)
                    {
                        entries.Add(new StoragePair(name, value));
                    }
                    nextLine:
                    ;
                }

                List<Message> msgs = new List<Message>();

                bool lockTaken = false;
                try
                {
                    Monitor.Enter(m_mutex, ref lockTaken);
                    foreach (var i in entries)
                    {
                        Entry entry;
                        if (!m_entries.TryGetValue(i.First, out entry))
                        {
                            entry = new Entry(i.First);
                            m_entries.Add(i.First, entry);
                        }
                        var oldValue = entry.value;
                        entry.value = i.Second;
                        bool wasPersist = entry.IsPersistent();
                        if (!wasPersist) entry.flags |= EntryFlags.Persistent;

                        if (m_server && entry.id != 0xffff)
                        {
                            uint id = (uint)m_idMap.Count;
                            entry.id = id;
                            m_idMap.Add(entry);
                        }

                        if (m_notifier.LocalNotifiers())
                        {
                            if (oldValue != null)
                            {
                                m_notifier.NotifyEntry(i.First, i.Second,
                                     (NotifyFlags.NotifyNew | NotifyFlags.NotifyLocal));
                            }
                            else if (oldValue != i.Second)
                            {
                                NotifyFlags notifyFlags = NotifyFlags.NotifyUpdate | NotifyFlags.NotifyLocal;
                                if (!wasPersist) notifyFlags |= NotifyFlags.NotifyFlagsChanged;
                                m_notifier.NotifyEntry(i.First, i.Second, notifyFlags);
                            }
                        }

                        if (m_queueOutgoing == null) continue;
                        ++entry.seqNum;

                        if (oldValue == null || oldValue.Type != i.Second.Type)
                        {
                            msgs.Add(Message.EntryAssign(i.First, entry.id, entry.seqNum.Value(), i.Second, entry.flags));
                        }
                        else if (entry.id != 0xffff)
                        {
                            if (oldValue != i.Second)
                            {
                                msgs.Add(Message.EntryUpdate(entry.id, entry.seqNum.Value(), i.Second));
                            }
                            if (!wasPersist)
                                msgs.Add(Message.FlagsUpdate(entry.id, entry.flags));
                        }
                    }

                    if (m_queueOutgoing != null)
                    {
                        var queuOutgoing = m_queueOutgoing;
                        Monitor.Exit(m_mutex);
                        lockTaken = false;
                        foreach (var msg in msgs) queuOutgoing(msg, null, null);
                    }
                }
                finally
                {
                    if (lockTaken) Monitor.Exit(m_mutex);
                }
            }
            return true;
        }


    }
}
