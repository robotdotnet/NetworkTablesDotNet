using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkTables
{
    internal class Notifier
    {
        

        private static Notifier s_instance;
        private static bool s_destroyed;
        private volatile bool m_localNotifiers;

        private Thread m_thread;

        private struct EntryListener
        {
            public EntryListener(string prefix_, NtCore.EntryListenerCallback callback_, NotifyFlags flags_)
            {
                prefix = prefix_;
                callback = callback_;
                flags = flags_;
            }

            public string prefix;
            public NtCore.EntryListenerCallback callback;
            public NotifyFlags flags;
        }

        private List<EntryListener> m_entryListeners;
        private List<NtCore.ConnectionListenerCallback> m_connListeners;

        private struct EntryNotification
        {
            public EntryNotification(string name_, Value value_, NotifyFlags flags_,
                NtCore.EntryListenerCallback only_)
            {
                name = name_;
                value = value_;
                flags = flags_;
                only = only_;
            }

            public string name;
            public Value value;
            public NotifyFlags flags;
            public NtCore.EntryListenerCallback only;
        }

        private Queue<EntryNotification> m_entryNotifications;

        private struct ConnectionNotification
        {
            public ConnectionNotification(bool connected_, ConnectionInfo conn_info_, NtCore.ConnectionListenerCallback only_)
            {
                connected = connected_;
                conn_info = conn_info_;
                only = only_;
            }
            public bool connected;
            public ConnectionInfo conn_info;
            public NtCore.ConnectionListenerCallback only;
        }

        private Queue<ConnectionNotification> m_connNotifications;

        public static Notifier Instance
        {
            get
            {
                return s_instance ?? (s_instance = new Notifier());
            }
        }

        private Notifier()
        {
            m_active = false;
            m_localNotifiers = false;
            s_destroyed = false;
        }

        ~Notifier()
        {
            s_destroyed = true;
            Stop();
        }

        private void ThreadMain()
        {
            bool lockEntered = false;
            try
            {
                Monitor.Enter(m_mutex, ref lockEntered);
                while (m_active)
                {
                    while (m_entryNotifications.Count == 0 && m_connNotifications.Count == 0)
                    {
                        Monitor.Exit(m_mutex);
                        lockEntered = false;
                        m_cond.WaitOne();
                        Monitor.Enter(m_mutex, ref lockEntered);
                        if (!m_active) return;
                    }

                    while (m_entryNotifications.Count != 0)
                    {
                        if (!m_active) return;
                        var item = m_entryNotifications.Dequeue();
                        if (item.value == null) continue;
                        string name = item.name;

                        if (item.only != null)
                        {
                            Monitor.Exit(m_mutex);
                            lockEntered = false;
                            item.only(0, name, item.value, (NotifyFlags)item.flags);
                            Monitor.Enter(m_mutex, ref lockEntered);
                            continue;
                        }

                        for (int i = 0; i < m_entryListeners.Count; ++i)
                        {
                            if (m_entryListeners[i].callback == null) continue;

                            NotifyFlags listenFlags = m_entryListeners[i].flags;
                            NotifyFlags flags = (NotifyFlags)item.flags;
                            NotifyFlags assignBoth = (NotifyFlags.NotifyUpdate | NotifyFlags.NotifyFlagsChanged);

                            if ((flags & assignBoth) == assignBoth)
                            {
                                if ((listenFlags & assignBoth) == 0) continue;
                                listenFlags &= ~assignBoth;
                                flags &= ~assignBoth;
                            }
                            if ((flags & ~listenFlags) != 0) continue;

                            if (!name.StartsWith(m_entryListeners[i].prefix)) continue;

                            var callback = m_entryListeners[i].callback;

                            Monitor.Exit(m_mutex);
                            lockEntered = false;
                            callback((uint)(i + 1), name, item.value, (NotifyFlags)item.flags);
                            Monitor.Enter(m_mutex, ref lockEntered);
                        }

                    }

                    while (m_connNotifications.Count != 0)
                    {
                        if (!m_active) return;
                        var item = m_connNotifications.Dequeue();

                        if (item.only != null)
                        {
                            Monitor.Exit(m_mutex);
                            lockEntered = false;
                            item.only(0, item.connected, item.conn_info);
                            Monitor.Enter(m_mutex, ref lockEntered);
                            continue;
                        }

                        for (int i = 0; i < m_connListeners.Count; ++i)
                        {
                            if (m_connListeners[i] == null) continue;
                            var callback = m_connListeners[i];

                            Monitor.Exit(m_mutex);
                            lockEntered = false;
                            callback((uint)(i + 1), item.connected, item.conn_info);
                            Monitor.Enter(m_mutex, ref lockEntered);
                        }
                    }
                }
            }
            finally
            {
                if (lockEntered) Monitor.Exit(m_mutex);
            }
        }

        private volatile bool m_active;
        private readonly object m_mutex = new object();
        private readonly AutoResetEvent m_cond = new AutoResetEvent(false);

        public void Start()
        {
            lock (m_mutex)
            {
                if (m_active) return;
                m_active = true;
            }
            m_thread = new Thread(ThreadMain);
            m_thread.Start();
        }

        public void Stop()
        {
            m_active = false;
            //Notify condition so thread terminates.
            m_cond.Set();
            TimeSpan timeout = TimeSpan.FromSeconds(1);
            if (m_thread == null) return;
            bool joined = m_thread.Join(timeout);
            if (!joined)
            {
                m_thread?.Abort();
            }
        }

        public bool LocalNotifiers()
        {
            return m_localNotifiers;
        }

        public static bool Destroyed()
        {
            return s_destroyed;
        }

        public uint AddEntryListener(string prefix, NtCore.EntryListenerCallback callback, NotifyFlags flags)
        {
            lock (m_mutex)
            {
                uint uid = (uint)m_entryListeners.Count;
                m_entryListeners.Add(new EntryListener(prefix, callback, flags));
                if ((flags & NotifyFlags.NotifyLocal) != 0) m_localNotifiers = true;
                return uid + 1;
            }
        }

        public void RemoveEntryListener(uint entryListenerUid)
        {
            --entryListenerUid;
            lock (m_mutex)
            {
                if (entryListenerUid < m_entryListeners.Count)
                {
                    var listener = m_entryListeners[(int)entryListenerUid];
                    listener.callback = null;
                }
            }
        }

        public void NotifyEntry(string name, Value value, NotifyFlags flags, NtCore.EntryListenerCallback only = null)
        {
            if (!m_active) return;
            if ((flags & NotifyFlags.NotifyLocal) != 0 && !m_localNotifiers) return;
            lock (m_mutex)
            {
                m_entryNotifications.Enqueue(new EntryNotification(name, value, flags, only));
            }
            m_cond.Set();
        }

        public uint AddConnectionListener(NtCore.ConnectionListenerCallback callback)
        {
            lock (m_mutex)
            {
                uint uid = (uint)m_connListeners.Count;
                m_connListeners.Add(callback);
                return uid + 1;
            }
        }

        public void RemoveConnectionListener(uint connListenerUid)
        {
            --connListenerUid;
            lock (m_mutex)
            {
                if (connListenerUid < m_connListeners.Count)
                {
                    m_connListeners[(int)connListenerUid] = null;
                }
            }
        }

        public void NotifyConnection(bool connected, ConnectionInfo conn_info, NtCore.ConnectionListenerCallback only = null)
        {
            if (!m_active) return;
            lock (m_mutex)
            {
                m_connNotifications.Enqueue(new ConnectionNotification(connected, conn_info, only));
            }
            m_cond.Set();
        }
    }
}
