using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NetworkTables.NetworkTables;
using NetworkTables.NetworkTables2;
using NetworkTables.Tables;
using NetworkTables.NetworkTables2.Thread;
using NetworkTables.NetworkTables2.Util;

namespace NetworkTables
{
    /// <summary>
    /// This class is the Main Class for interfacing with NetworkTables.
    /// </summary>
    /// <remarks>For most users, this will be the only class that will be needed.
    /// Any interfaces needed to work with this can be found in the NetworkTables.Tables 
    /// namespace. </remarks>
    /// <example>
    /// The following example demonstrates creating a server:
    /// 
    /// <code language="cs">
    /// //Set Server Mode
    /// NetworkTable.SetServerMode();
    /// 
    /// //Initialize the Server
    /// NetworkTable.Initialize();
    /// 
    /// //Get a reference to the smartdashboard.
    /// var smartDashboard = NetworkTable.GetTable("SmartDashboard");
    /// </code>
    /// <c>smartDashboard</c> can now be used to get and set values in the smart dashboard.
    /// Examples on this can be found below the client section.
    /// <para />
    /// The following example demonstrates creating a client and connecting it to a server:
    /// 
    /// <code language="cs">
    /// //Set IP Address. Replace xxxx with your team number if connecting to a RoboRIO,
    /// //or the server's IP if the server is not a RoboRIO.
    /// NetworkTable.SetIPAddress("roborio-xxxx.local");
    /// 
    /// //Set Client Mode
    /// NetworkTable.SetClientMode();
    /// 
    /// //Initialize the client
    /// NetworkTable.Initialize();
    /// 
    /// //Get a reference to the smartdashboard.
    /// var smartDashboard = NetworkTable.GetTable("SmartDashboard");
    /// </code>
    /// <c>smartDashboard</c> can now be used to get and set values in the smart dashboard.
    /// <para />
    /// The following example shows how to get and put values into the smart dashboard:
    /// 
    /// <code language="cs">
    /// //Strings
    /// smartDashboard.PutString("MyString", "MyValue");
    /// string s = smartDashboard.GetString("MyString");
    /// //Note that if the key has not been put in the smart dashboard,
    /// //the GetString function will throw a TableKeyNotDefinedException.
    /// //To get around this, set a default value to be returned if there is no key, like this:
    /// string s = smartDashboard.GetString("MyString", "Default");
    /// 
    /// //Numbers
    /// smartDashboard.PutNumber("MyNumber", 3.562);
    /// double s = smartDashboard.GetNumber("MyNumber");
    /// //Note that if the key has not been put in the smart dashboard,
    /// //the GetString function will throw a TableKeyNotDefinedException.
    /// //To get around this, set a default value to be returned if there is no key, like this:
    /// double s = smartDashboard.GetDouble("MyNumber", 0.0);
    /// 
    /// //Bools
    /// smartDashboard.PutBoolean("MyBool", true);
    /// bool s = smartDashboard.GetBoolean("MyBool");
    /// //Note that if the key has not been put in the smart dashboard,
    /// //the GetString function will throw a TableKeyNotDefinedException.
    /// //To get around this, set a default value to be returned if there is no key, like this:
    /// bool s = smartDashboard.GetBoolean("MyBool", false);
    /// </code>
    /// </example>
    public class NetworkTable : ITable, IRemote
    {
        private static NTThreadManager s_threadManager = new DefaultThreadManager();

        public static readonly char PATH_SEPARATOR = '/';
        public static readonly int DEFAULT_PORT = 1735;

        private static NetworkTableProvider s_staticProvider = null;
        private static NetworkTableMode.CreateNodeDelegate s_mode = NetworkTableMode.CreateServerNode;

        private object m_lockObject = new object();
        private static object s_lockObject = new object();

        private static int s_port = DEFAULT_PORT;
        private static string s_ipAddress = null;

        private static void CheckInit()
        {
            lock (s_lockObject)
            {
                if (s_staticProvider != null)
                    throw new InvalidOperationException("Network tables has already been initialized");
            }
        }

        public static void Initialize()
        {
            lock (s_lockObject)
            {
                CheckInit();
                s_staticProvider = new NetworkTableProvider(s_mode(s_ipAddress, s_port, s_threadManager));
            }
        }

        public static void SetTableProvider(NetworkTableProvider provider)
        {
            lock (s_lockObject)
            {
                CheckInit();
                s_staticProvider = provider;
            }
        }

        public static void SetServerMode()
        {
            lock (s_lockObject)
            {
                CheckInit();
                s_mode = NetworkTableMode.CreateServerNode;
            }
        }

        public static void SetClientMode()
        {
            lock (s_lockObject)
            {
                CheckInit();
                s_mode = NetworkTableMode.CreateClientNode;
            }
        }

        public static void SetTeam(int team)
        {
            lock (s_lockObject)
            {
                SetIPAddress("10." + (team / 100) + "." + (team % 100) + ".2");
            }
        }

        public static void SetIPAddress(string address)
        {
            lock (s_lockObject)
            {
                CheckInit();
                s_ipAddress = address;
            }
        }

        public static NetworkTable GetTable(string key)
        {
            lock (s_lockObject)
            {
                if (s_staticProvider == null)
                {
                    try
                    {
                        Initialize();
                    }
                    catch (IOException e)
                    {
                        throw new SystemException("NetworkTable could not be initialized: " + e);
                    }
                }
                return (NetworkTable)s_staticProvider.GetTable(PATH_SEPARATOR + key);
            }
        }

        private readonly string path;
        private readonly EntryCache entryCache;
        private readonly NetworkTableProvider provider;
        private readonly NetworkTableNode node;
        private readonly NetworkTableKeyCache absoluteKeyCache;


        internal NetworkTable(string path, NetworkTableProvider provider)
        {
            this.path = path;
            absoluteKeyCache = new NetworkTableKeyCache(path);

            this.provider = provider;
            this.node = provider.GetNode();
            absoluteKeyCache = new NetworkTableKeyCache(path);
            entryCache = new EntryCache(path, ref node, ref absoluteKeyCache);
        }

        public override string ToString()
        {
            return $"NetworkTable: {path}";
        }

        public bool IsConnected()
        {
            return node.IsConnected();
        }

        public bool IsServer()
        {
            return node.IsServer();
        }


        internal class NetworkTableKeyCache : StringCache
        {
            private readonly string path;

            public NetworkTableKeyCache(string path)
            {
                this.path = path;
            }

            public override string Calc(string key)
            {
                return path + PATH_SEPARATOR + key;
            }
        }

        internal class EntryCache
        {
            private readonly Dictionary<string, NetworkTableEntry> cache = new Dictionary<string, NetworkTableEntry>();
            private readonly string path;
            private readonly NetworkTableNode node;
            private readonly NetworkTableKeyCache abs;


            public EntryCache(string path, ref NetworkTableNode node, ref NetworkTableKeyCache abs)
            {
                this.path = path;
                this.node = node;
                this.abs = abs;
            }

            public NetworkTableEntry Get(string key)
            {
                NetworkTableEntry cachedValue;
                if (!cache.TryGetValue(key, out cachedValue))
                {
                    cachedValue = node.GetEntryStore().GetEntry(abs.Get(key));
                    if (cachedValue != null)
                        cache.Add(key, cachedValue);
                }
                return cachedValue;
            }
        }

        private readonly Dictionary<IRemoteConnectionListener, NetworkTableConnectionListenerAdapter> connectionListenerMap =
            new Dictionary<IRemoteConnectionListener, NetworkTableConnectionListenerAdapter>();

        public void AddConnectionListener(IRemoteConnectionListener listener, bool immediateNotify)
        {
            NetworkTableConnectionListenerAdapter adapter;
            if (connectionListenerMap.TryGetValue(listener, out adapter))
                throw new ArgumentException("Cannot add the same listener twice");
            adapter = new NetworkTableConnectionListenerAdapter(this, listener);
            connectionListenerMap.Add(listener, adapter);
            node.AddConnectionListener(adapter, immediateNotify);
        }

        public void RemoveConnectionListener(IRemoteConnectionListener listener)
        {
            NetworkTableConnectionListenerAdapter adapter;
            if (connectionListenerMap.TryGetValue(listener, out adapter))
                node.RemoveConnectionListener(adapter);
        }

        private readonly Dictionary<ITableListener, List> listenerMap = new Dictionary<ITableListener, List>();

        public void AddTableListener(ITableListener listener)
        {
            AddTableListener(listener, false);
        }

        public void AddTableListener(ITableListener listener, bool immediateNotify)
        {
            List adapters;
            if (!listenerMap.TryGetValue(listener, out adapters))
            {
                adapters = new List();
                listenerMap.Add(listener, adapters);
            }
            NetworkTableListenerAdapter adapter = new NetworkTableListenerAdapter(path + PATH_SEPARATOR, this, listener);
            adapters.Add(adapter);
            node.AddTableListener(adapter, immediateNotify);
        }

        public void AddTableListener(string key, ITableListener listener, bool immediateNotify)
        {
            List adapters;
            if (!listenerMap.TryGetValue(listener, out adapters))
            {
                adapters = new List();
                listenerMap.Add(listener, adapters);
            }

            NetworkTableKeyListenerAdapter adapter = new NetworkTableKeyListenerAdapter(key, absoluteKeyCache.Get(key), this, listener);
            adapters.Add(adapter);
            node.AddTableListener(adapter, immediateNotify);
        }

        public void AddSubTableListener(ITableListener listener)
        {
            List adapters;
            if (!listenerMap.TryGetValue(listener, out adapters))
            {
                adapters = new List();
                listenerMap.Add(listener, adapters);
            }
            NetworkTableSubListenerAdapter adapter = new NetworkTableSubListenerAdapter(path, this, listener);
            adapters.Add(adapter);
            node.AddTableListener(adapter, true);

        }

        public void RemoveTableListener(ITableListener listener)
        {
            List adapters;
            if (listenerMap.TryGetValue(listener, out adapters))
            {
                for (int i = 0; i < adapters.Size(); i++)
                {
                    node.RemoveTableListener((ITableListener)adapters.Get(i));
                }
                adapters.Clear();
            }
        }

        private NetworkTableEntry GetEntry(string key)
        {
            lock (s_lockObject)
            {
                return entryCache.Get(key);
            }
        }


        public bool ContainsKey(string key)
        {
            return node.ContainsKey(absoluteKeyCache.Get(key));
        }

        public bool ContainsSubTable(string key)
        {
            string subtablePrefix = absoluteKeyCache.Get(key) + PATH_SEPARATOR;
            var keys = node.GetEntryStore().Keys();
            return keys.Any(k => k.StartsWith(subtablePrefix));
        }

        public ITable GetSubTable(string key)
        {
            lock (m_lockObject)
            {
                return provider.GetTable(absoluteKeyCache.Get(key));
            }
        }



        public object GetValue(string key)
        {
            return node.GetValue(absoluteKeyCache.Get(key));
        }

        public object GetValue(string key, object defaultValue)
        {
            try
            {
                return node.GetValue(absoluteKeyCache.Get(key));
            }
            catch (TableKeyNotDefinedException)
            {
                return defaultValue;
            }
        }

        public void PutValue(string key, object value)
        {
            NetworkTableEntry entry = entryCache.Get(key);
            if (entry != null)
                node.PutValue(entry, value);
            else
            {
                node.PutValue(absoluteKeyCache.Get(key), value);
            }
        }

        public void RetrieveValue(string key, object externalValue)
        {
            node.RetrieveValue(absoluteKeyCache.Get(key), externalValue);
        }

        public void PutNumber(string key, double value)
        {
            PutValue(key, value);
        }

        public double GetNumber(string key)
        {
            return node.GetDouble(absoluteKeyCache.Get(key));
        }

        public double GetNumber(string key, double defaultValue)
        {
            try
            {
                return node.GetDouble(absoluteKeyCache.Get(key));
            }
            catch (TableKeyNotDefinedException e)
            {
                return defaultValue;
            }
        }

        public void PutString(string key, string value)
        {
            PutValue(key, value);
        }

        public string GetString(string key)
        {
            return node.GetString(absoluteKeyCache.Get(key));
        }

        public string GetString(string key, string defaultValue)
        {
            try
            {
                return node.GetString(absoluteKeyCache.Get(key));
            }
            catch (TableKeyNotDefinedException e)
            {
                return defaultValue;
            }
        }

        public void PutBoolean(string key, bool value)
        {
            PutValue(key, value);
        }

        public bool GetBoolean(string key)
        {
            return node.GetBoolean(absoluteKeyCache.Get(key));
        }

        public bool GetBoolean(string key, bool defaultValue)
        {
            try
            {
                return node.GetBoolean(absoluteKeyCache.Get(key));
            }
            catch (TableKeyNotDefinedException e)
            {
                return defaultValue;
            }
        }




    }
}
