using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkTables
{
    public class Notifier
    {
        public delegate void EntryListenerCallback(uint uid, string name, NTValue value, uint flags);
        public delegate void ConnectionListenerCallback(uint uid, string name, uint port, uint flags);

        private static Notifier s_instance;
        private static bool s_destroyed;
        private volatile bool m_localNotifiers;

        private Thread m_thread;

        private struct EntryListener
        {
            public EntryListener(string prefix_, EntryListenerCallback callback_, uint flags_)
            {
                prefix = prefix_;
                callback = callback_;
                flags = flags_;
            }

            public string prefix;
            public EntryListenerCallback callback;
            public uint flags;
        }

        private List<EntryListener> m_entryListeners;
        private List<ConnectionListenerCallback> m_connListeners;

        private struct EntryNotification
        {
            public EntryNotification(string name_, NTValue value_, uint flags_,
                EntryListenerCallback only_)
            {
                name = name_;
                value = value_;
                flags = flags_;
                only = only_;
            }

            public string name;
            public NTValue value;
            public uint flags;
            public EntryListenerCallback only;
        }

        private Queue<EntryNotification> m_entryNotifications;

        private struct ConnectionNotification
        {
            public ConnectionNotification(bool connected_, ConnectionListenerCallback only_)
            {
                connected = connected_;
                only = only_;
            }
            public bool connected;
            public ConnectionListenerCallback only;
        }

        public static Notifier Instance
        {
            get
            {
                return (s_instance ?? new Notifier());
            }
        }

        private Notifier()
        {

        }

        private void ThreadMain()
        {

        }

        private volatile bool m_active;
        private readonly object m_mutex = new object();
        private readonly object m_shutdownMutex = new object();

        public void Start()
        {

        }

        public void Stop()
        {

        }

        public bool LocalNotifiers()
        {
            return m_localNotifiers;
        }

        public static bool Destroyed()
        {
            return s_destroyed;
        }

        public uint AddEntryListener(string prefix, EntryListenerCallback callback, uint flags)
        {

        }

        public void RemoveEntryListener(uint entryListenerUid)
        {

        }

        public void NotifyEntry(string name, NTValue value, uint flags, EntryListenerCallback only = null)
        {

        }

        public uint AddConnectionListener(ConnectionListenerCallback callback)
        {

        }

        public void RemoveConnectionListener(uint connListenerUid)
        {

        }

        public void NotifyConnection(bool connected, ConnectionListenerCallback only = null)
        {

        }
    }
}
