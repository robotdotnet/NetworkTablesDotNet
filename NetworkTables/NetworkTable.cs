using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkTables.NTCore;
using NetworkTables.Tables;
using static NetworkTables.NTCore.InteropHelpers;

namespace NetworkTables
{
    public class NetworkTable : ITable, IDisposable
    {
        public string Path { get; }

        internal const char PATH_SEPERATOR_CHAR = '/';
        private const uint DEFAULT_PORT = 1735;
        private static string s_ipAddress = null;
        private static bool client = false;
        private static bool running = false;

        private readonly Dictionary<uint, ITableListener> m_listeners = new Dictionary<uint, ITableListener>();

        public static void Initialize()
        {
            if (client)
            {
                StartClient(s_ipAddress, DEFAULT_PORT);
            }
            else
            {
                StartServer("networktables.ini", "", DEFAULT_PORT);
            }
            running = true;
        }

        public static void Shutdown()
        {
            if (client)
            {
                Interop.NT_StopClient();
            }
            else
            {
                Interop.NT_StopServer();
            }
            running = false;
        }

        public static void SetClientMode()
        {
            client = true;
        }

        public static void SetServerMode()
        {
            client = false;
        }

        public static void SetTeam(int team)
        {
            SetIPAddress($"10.{(team / 100)}.{(team % 100)}.2");
        }

        public static void SetIPAddress(string address)
        {
            s_ipAddress = address;
        }

        public static NetworkTable GetTable(string key)
        {
            if (!running) Initialize();
            return new NetworkTable(PATH_SEPERATOR_CHAR + key);
        }

        private NetworkTable(string path)
        {
            this.Path = path;
        }

        public void Dispose()
        {
            foreach (var key in m_listeners.Keys)
            {
                Interop.NT_RemoveEntryListener(key);
            }
            m_listeners.Clear();
        }

        public bool ContainsKey(string key)
        {
            string path = Path + PATH_SEPERATOR_CHAR + key;
            return InteropHelpers.GetType(path) != NT_Type.NT_UNASSIGNED;
        }

        public bool ContainsSubTable(string key)
        {
            string path = Path + PATH_SEPERATOR_CHAR + key;
            using (EntryInfoArray array = GetEntryInfo(path, 0))
            {
                return array.Length != 0;
            }
        }

        public void Persist(string key)
        {
            string path = Path + PATH_SEPERATOR_CHAR + key;
            SetEntryFlags(path, (uint)NT_EntryFlags.NT_PERSISTENT);
        }

        public ITable GetSubTable(string key)
        {
            string path = Path + PATH_SEPERATOR_CHAR + key;
            return new NetworkTable(key);
        }

        public void PutNumber(string key, double value)
        {
            string path = Path + PATH_SEPERATOR_CHAR + key;
            SetEntryDouble(path, value);
        }

        public double GetNumber(string key, double defaultValue)
        {
            string path = Path + PATH_SEPERATOR_CHAR + key;
            int status = 0;
            ulong lc = 0;
            double retVal = GetEntryDouble(path, ref lc, ref status);
            if (status == 0)
            {
                return defaultValue;
            }
            return retVal;
        }

        public void PutString(string key, string value)
        {
            string path = Path + PATH_SEPERATOR_CHAR + key;
            SetEntryString(path, value);
        }

        public string GetString(string key, string defaultValue)
        {
            string path = Path + PATH_SEPERATOR_CHAR + key;
            ulong lc = 0;
            string retVal = GetEntryString(path, ref lc);
            return retVal ?? defaultValue;
        }

        public void PutBoolean(string key, bool value)
        {
            string path = Path + PATH_SEPERATOR_CHAR + key;
            SetEntryBoolean(path, value);
        }

        public bool GetBoolean(string key, bool defaultValue)
        {
            string path = Path + PATH_SEPERATOR_CHAR + key;
            int status = 0;
            ulong lc = 0;
            bool retVal = GetEntryBoolean(path, ref lc, ref status);
            if (status == 0)
            {
                return defaultValue;
            }
            return retVal;
        }

        public void AddTableListener(ITableListener listener)
        {
            AddTableListener(listener, false);
        }

        public void AddTableListener(ITableListener listener, bool immediateNotify)
        {
            string path = Path + PATH_SEPERATOR_CHAR;
            uint id = AddEntryListener(path, this, listener.ValueChanged, immediateNotify);
            m_listeners.Add(id, listener);
        }

        public void AddTableListener(string key, ITableListener listener, bool immediateNotify)
        {
            string path = Path + PATH_SEPERATOR_CHAR + key;
            uint id = AddEntryListener(path, this, listener.ValueChanged, immediateNotify);
            m_listeners.Add(id, listener);
        }

        public void RemoveTableListener(ITableListener listener)
        {
            List<uint> keyMatches = new List<uint>();
            foreach (KeyValuePair<uint, ITableListener> valuePair in m_listeners)
            {
                if (valuePair.Value == listener)
                {
                    Interop.NT_RemoveEntryListener(valuePair.Key);
                    keyMatches.Add(valuePair.Key);
                }
            }
            foreach (var keyMatch in keyMatches)
            {
                m_listeners.Remove(keyMatch);
            }
        }
    }
}
