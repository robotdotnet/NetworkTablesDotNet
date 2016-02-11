﻿using System;
using System.Collections.Generic;
using NetworkTables.Native.Exceptions;
using NetworkTables.Tables;
using static NetworkTables.NtCore;

namespace NetworkTables
{
    /// <summary>
    /// This class is the Main Class for interfacing with NetworkTables.
    /// </summary>
    /// <remarks>For most users, this will be the only class that will be needed.
    /// Any interfaces needed to work with this can be found in the <see cref="NetworkTables.Tables"/> 
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
        /// <summary>The character used to seperate tables and keys.</summary>
        public const char PathSeperatorChar = '/';
        /// <summary>The default port NetworkTables listens on.</summary>
        public const int DefaultPort = 1735;

        private static object s_lockObject = new object();

        /// <summary>
        /// The default file name used for Persistent Storage.
        /// </summary>
        public const string DefaultPersistentFileName = "networktables.ini";
        internal static int Port { get; private set; } = DefaultPort;
        internal static string IPAddress { get; private set; } = "";
        internal static bool Client { get; private set; }
        internal static bool Running { get; private set; }

        internal static string PersistentFilename { get; private set; } = DefaultPersistentFileName;

        private static void CheckInit()
        {
            lock (s_lockObject)
            {
                if (Running)
                    throw new InvalidOperationException("Network Tables has already been initialized");
            }
        }

        /// <summary>
        /// Initializes NetworkTables. Please call <see cref="SetServerMode"/> or <see cref="SetClientMode"/>
        /// first.
        /// </summary>
        /// <remarks>
        /// If NetworkTables is already running, the old instance will be shutdown and a new instance will 
        /// be created.
        /// </remarks>
        public static void Initialize()
        {
            lock (s_lockObject)
            {
                if (Running)
                    Shutdown();
                if (Client)
                {
                    NtCore.StartClient(IPAddress, Port);
                }
                else
                {
                    NtCore.StartServer(PersistentFilename, "", Port);
                }
                Running = true;
            }
        }

        /// <summary>
        /// Shuts down NetworkTables.
        /// </summary>
        public static void Shutdown()
        {
            lock (s_lockObject)
            {
                if (!Running)
                    return;
                if (Client)
                {
                    NtCore.StopClient();
                }
                else
                {
                    NtCore.StopServer();
                }
                Running = false;
            }
        }

        /// <summary>
        /// Sets NetworkTables to be a client.
        /// </summary>
        /// <exception cref="InvalidOperationException">This is thrown if Network Tables
        /// has already been initialized.</exception>
        /// <remarks>This or <see cref="SetServerMode"/> must be called
        /// before <see cref="Initialize"/> or <see cref="GetTable(string)"/>.</remarks>
        public static void SetClientMode()
        {
            lock (s_lockObject)
            {
                if (Client)
                    return;
                CheckInit();
                Client = true;
            }
        }

        /// <summary> 
        /// Sets NetworkTables to be a server
        /// </summary>
        /// <exception cref="InvalidOperationException">This is thrown if Network Tables
        /// has already been initialized.</exception>
        /// <remarks>This or <see cref="SetClientMode"/> must be called
        /// before <see cref="Initialize"/> or <see cref="GetTable(string)"/></remarks>
        public static void SetServerMode()
        {
            lock (s_lockObject)
            {
                if (!Client)
                    return;
                CheckInit();
                Client = false;
            }
        }

        /// <summary>
        /// Sets the team the robot is configured for. This will set the Mdns
        /// address that NetworkTables will connect to in client mode.
        /// </summary>
        /// <param name="team">The team number</param>
        /// /// <remarks>This must be called before <see cref="Initialize"/> or 
        /// <see cref="GetTable(string)"/> if the system is a client.</remarks>
        public static void SetTeam(int team)
        {
            lock (s_lockObject)
            {
                SetIPAddress($"roboRIO-{team}-FRC.local");
            }
        }

        /// <summary>
        /// Sets the IP address that will be connected to in client mode.
        /// </summary>
        /// <param name="address">The IP address to connect to in client mode</param>
        public static void SetIPAddress(string address)
        {
            lock (s_lockObject)
            {
                if (IPAddress == address)
                    return;
                CheckInit();
                IPAddress = address;
            }
        }

        /// <summary>
        /// Gets the table with the specified key.
        /// </summary>
        /// <remarks>If the table does not exist, a new table will be created.
        /// This will automatically initialize network tables if it has not been already.</remarks>
        /// <param name="key">The network table key to request.</param>
        /// <returns>The <see cref="NetworkTable"/> requested.</returns>
        public static NetworkTable GetTable(string key)
        {
            lock (s_lockObject)
            {
                if (!Running) Initialize();
                if (key == "" || key[0] == PathSeperatorChar)
                    return new NetworkTable(key);
                return new NetworkTable(PathSeperatorChar + key);
            }
        }

        /// <summary>
        /// Sets the Port for NetworkTables to connect to in client mode or listen to
        /// in server mode.
        /// </summary>
        /// <param name="port">The port number to listen on or connect to.</param>
        public static void SetPort(int port)
        {
            if (port == Port)
                return;
            CheckInit();
            Port = port;
        }

        /// <summary>
        /// Sets the Persistent file name.
        /// </summary>
        /// <param name="filename">The filename that the NetworkTables server uses
        /// for automatic loading and saving of persistent values.</param>
        public static void SetPersistentFilename(string filename)
        {
            if (PersistentFilename == filename)
                return;
            CheckInit();
            PersistentFilename = filename;
        }

        /// <summary>
        /// Sets the Network Identity
        /// </summary>
        /// <param name="name">The name to identify this program as on the network.</param>
        public static void SetNetworkIdentity(string name)
        {
            NtCore.SetNetworkIdentity(name);
        }

        private readonly string m_path;

        private NetworkTable(string path)
        {
            m_path = path;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"NetworkTable: {m_path}";
        }

        /// <summary>
        /// Checkts the table and tells if it contains the specified key.
        /// </summary>
        /// <param name="key">The key to be checked.</param>
        /// <returns>True if the table contains the key, otherwise false.</returns>
        public bool ContainsKey(string key)
        {
            return (GetEntryValue(m_path + PathSeperatorChar + key) != null);
        }

        /// <summary>
        /// Checks the table and tells if if contains the specified sub-table.
        /// </summary>
        /// <param name="key">The sub-table to check for</param>
        /// <returns>True if the table contains the sub-table, otherwise false</returns>
        public bool ContainsSubTable(string key)
        {
            return GetEntryInfo(m_path + PathSeperatorChar + key + PathSeperatorChar, 0).Count != 0;
        }

        /// <summary>
        /// Gets a set of all the keys contained in the table with the specified type.
        /// </summary>
        /// <param name="types">Bitmask of types to check for; 0 is treated as a "don't care".</param>
        /// <returns>A set of all keys currently in the table.</returns>
        public HashSet<string> GetKeys(NtType types)
        {
            HashSet<string> keys = new HashSet<string>();
            int prefixLen = m_path.Length + 1;
            foreach (EntryInfo entry in GetEntryInfo(m_path + PathSeperatorChar, types))
            {
                string relativeKey = entry.Name.Substring(prefixLen);
                if (relativeKey.IndexOf(PathSeperatorChar) != -1)
                    continue;
                keys.Add(relativeKey);
            }
            return keys;
        }

        /// <summary>
        /// Gets a set of all the keys contained in the table.
        /// </summary>
        /// <returns>A set of all keys currently in the table.</returns>
        public HashSet<string> GetKeys()
        {
            return GetKeys(0);
        }

        /// <summary>
        /// Gets a set of all the sub-tables contained in the table.
        /// </summary>
        /// <returns>A set of all subtables currently contained in the table.</returns>
        public HashSet<string> GetSubTables()
        {
            HashSet<string> keys = new HashSet<string>();
            int prefixLen = m_path.Length + 1;
            foreach (EntryInfo entry in NtCore.GetEntryInfo(m_path + PathSeperatorChar, 0))
            {
                string relativeKey = entry.Name.Substring(prefixLen);
                int endSubTable = relativeKey.IndexOf(PathSeperatorChar);
                if (endSubTable == -1)
                    continue;
                keys.Add(relativeKey.Substring(0, endSubTable));
            }
            return keys;
        }

        /// <summary>
        /// Returns the <see cref="ITable"/> at the specified key. If there is no 
        /// table at the specified key, it will create a new table.
        /// </summary>
        /// <param name="key">The key name.</param>
        /// <returns>The <see cref="ITable"/> to be returned.</returns>
        public ITable GetSubTable(string key)
        {
            return new NetworkTable(m_path + PathSeperatorChar + key);
        }

        /// <summary>
        /// Makes a key's value persistent through program restarts.
        /// </summary>
        /// <param name="key">The key name (cannot be null).</param>
        public void SetPersistent(string key)
        {
            SetFlags(key, EntryFlags.Persistent);
        }

        /// <summary>
        /// Stop making a key's value persistent through program restarts.
        /// </summary>
        /// <param name="key">The key name (cannot be null).</param>
        public void ClearPersistent(string key)
        {
            ClearFlags(key, EntryFlags.Persistent);
        }

        /// <summary>
        /// Returns whether a value is persistent through program restarts.
        /// </summary>
        /// <param name="key">The key name (cannot be null).</param>
        /// <returns>True if the value is persistent.</returns>
        public bool IsPersistent(string key)
        {
            return GetFlags(key).HasFlag(EntryFlags.Persistent);
        }

        /// <summary>
        /// Sets flags on the specified key in this table.
        /// </summary>
        /// <param name="key">The key name.</param>
        /// <param name="flags">The flags to set. (Bitmask)</param>
        public void SetFlags(string key, EntryFlags flags)
        {
            NtCore.SetEntryFlags(m_path + PathSeperatorChar + key, GetFlags(key) | flags);
        }

        /// <summary>
        /// Clears flags on the specified key in this table.
        /// </summary>
        /// <param name="key">The key name.</param>
        /// <param name="flags">The flags to clear. (Bitmask)</param>
        public void ClearFlags(string key, EntryFlags flags)
        {
            NtCore.SetEntryFlags(m_path + PathSeperatorChar + key, GetFlags(key) & ~flags);
        }

        /// <summary>
        /// Returns the flags for the specified key.
        /// </summary>
        /// <param name="key">The key name.</param>
        /// <returns>The flags attached to the key.</returns>
        public EntryFlags GetFlags(string key)
        {
            return NtCore.GetEntryFlags(m_path + PathSeperatorChar + key);
        }

        /// <summary>
        /// Deletes the specifed key in this table.
        /// </summary>
        /// <param name="key">The key name.</param>
        public void Delete(string key)
        {
            NtCore.DeleteEntry(m_path + PathSeperatorChar + key);
        }

        /// <summary>
        /// Deletes ALL keys in ALL subtables. Use with caution!
        /// </summary>
        public static void GlobalDeleteAll()
        {
            NtCore.DeleteAllEntries();
        }

        /// <summary>
        /// Flushes all updated values immediately to the network.
        /// </summary>
        /// <remarks>
        /// Note that this is rate-limited to protect the network from flooding.
        /// This is primarily useful for synchronizing network updates with user code.
        /// </remarks>
        public static void Flush()
        {
            NtCore.Flush();
        }

        /// <summary>
        /// Sets the periodic update rate of the NetworkTables.
        /// </summary>
        /// <param name="interval">The update interval in seconds (0.1 to 1.0).</param>
        public static void SetUpdateRate(double interval)
        {
            NtCore.SetUpdateRate(interval);
        }

        /// <summary>
        /// Saves persistent keys to a file. The server does this automatically.
        /// </summary>
        /// <param name="filename">The file name.</param>
        /// <exception cref="PersistentException">Thrown if there is an error
        /// saving the file.</exception>
        public static void SavePersistent(string filename)
        {
            NtCore.SavePersistent(filename);
        }

        /// <summary>
        /// Loads persistent keys from a file. The server does this automatically.
        /// </summary>
        /// <param name="filename">The file name.</param>
        /// <returns>A List of warnings (errors result in an exception instead.)</returns>
        /// <exception cref="PersistentException">Thrown if there is an error
        /// loading the file.</exception>
        public static string[] LoadPersistent(string filename)
        {
            List<string> warns = new List<string>();
            string err = NtCore.LoadPersistent(filename, (line, msg) =>
            {
                warns.Add($"{line.ToString()}: {msg}");
            });
            if (err != null) throw new PersistentException("Load Persistent Failed");
            return warns.ToArray();
        }

        ///<inheritdoc/>
        [Obsolete("Please use the Default Value Get... Methods instead.")]
        public object GetValue(string key)
        {
            string localPath = m_path + PathSeperatorChar + key;
            var v = GetEntryValue(localPath);
            if (v == null) throw new TableKeyNotDefinedException(localPath);
            NtType type = v.Type;
            switch (type)
            {
                case NtType.Boolean:
                    return v.GetBoolean();
                case NtType.Double:
                    return v.GetDouble();
                case NtType.String:
                    return v.GetString();
                case NtType.Raw:
                    return v.GetRaw();
                case NtType.BooleanArray:
                    return v.GetBooleanArray();
                case NtType.DoubleArray:
                    return v.GetDoubleArray();
                case NtType.StringArray:
                    return v.GetStringArray();
                default:
                    throw new TableKeyNotDefinedException(localPath);
            }
        }

        ///<inheritdoc/>
        public object GetValue(string key, object defaultValue)
        {
            string localPath = m_path + PathSeperatorChar + key;
            var v = GetEntryValue(localPath);
            if (v == null) return defaultValue;
            NtType type = v.Type;
            switch (type)
            {
                case NtType.Boolean:
                    return v.GetBoolean();
                case NtType.Double:
                    return v.GetDouble();
                case NtType.String:
                    return v.GetString();
                case NtType.Raw:
                    return v.GetRaw();
                case NtType.BooleanArray:
                    return v.GetBooleanArray();
                case NtType.DoubleArray:
                    return v.GetDoubleArray();
                case NtType.StringArray:
                    return v.GetStringArray();
                default:
                    return defaultValue;
            }
        }

        ///<inheritdoc/>
        public bool PutValue(string key, object value)
        {
            key = m_path + PathSeperatorChar + key;
            //TODO: Make number accept all numbers.
            if (value is double) return SetEntryValue(key, Value.MakeDouble((double)value));
            else if (value is string) return SetEntryValue(key, Value.MakeString((string)value));
            else if (value is bool) return SetEntryValue(key, Value.MakeBoolean((bool)value));
            else if (value is byte[])
            {
                return SetEntryValue(key, Value.MakeRaw((byte[])value));
            }
            else if (value is double[])
            {
                return SetEntryValue(key, Value.MakeDoubleArray((double[])value));
            }
            else if (value is bool[])
            {
                return SetEntryValue(key, Value.MakeBooleanArray((bool[])value));
            }
            else if (value is string[])
            {
                return SetEntryValue(key, Value.MakeStringArray((string[])value));
            }
            else
            {
                throw new ArgumentException("Value is either null or an invalid type.");
            }
        }

        private void ThrowException(string name, Value v, NtType requestedType)
        {
            if (v == null || v.Type == NtType.Unassigned)
            {
                throw new TableKeyNotDefinedException(name);
            }
            else
            {
                throw new TableKeyDifferentTypeException(name, requestedType, v.Type);
            }
        }

        ///<inheritdoc/>
        public bool PutNumber(string key, double value)
        {
            return SetEntryValue(m_path + PathSeperatorChar + key, Value.MakeDouble(value));
        }

        ///<inheritdoc/>
        public double GetNumber(string key, double defaultValue)
        {
            var v = GetEntryValue(m_path + PathSeperatorChar + key);
            if (v == null || v.Type != NtType.Double) return defaultValue;
            return v.GetDouble();
        }

        ///<inheritdoc/>
        [Obsolete("Please use the Default Value Get... Methods instead.")]
        public double GetNumber(string key)
        {
            var v = GetEntryValue(m_path + PathSeperatorChar + key);
            if (v == null || v.Type != NtType.Double)
                ThrowException(m_path + PathSeperatorChar + key, v, NtType.Double);
            return v.GetDouble();
        }

        ///<inheritdoc/>
        public bool PutString(string key, string value)
        {
            return SetEntryValue(m_path + PathSeperatorChar + key, Value.MakeString(value));
        }

        ///<inheritdoc/>
        public string GetString(string key, string defaultValue)
        {
            var v = GetEntryValue(m_path + PathSeperatorChar + key);
            if (v == null || v.Type != NtType.String) return defaultValue;
            return v.GetString();
        }

        ///<inheritdoc/>
        [Obsolete("Please use the Default Value Get... Methods instead.")]
        public string GetString(string key)
        {
            var v = GetEntryValue(m_path + PathSeperatorChar + key);
            if (v == null || v.Type != NtType.String)
                ThrowException(m_path + PathSeperatorChar + key, v, NtType.String);
            return v.GetString();
        }

        ///<inheritdoc/>
        public bool PutBoolean(string key, bool value)
        {
            return SetEntryValue(m_path + PathSeperatorChar + key, Value.MakeBoolean(value));
        }

        ///<inheritdoc/>
        public bool GetBoolean(string key, bool defaultValue)
        {
            var v = GetEntryValue(m_path + PathSeperatorChar + key);
            if (v == null || v.Type != NtType.Boolean) return defaultValue;
            return v.GetBoolean();
        }

        ///<inheritdoc/>
        [Obsolete("Please use the Default Value Get... Methods instead.")]
        public bool GetBoolean(string key)
        {
            var v = GetEntryValue(m_path + PathSeperatorChar + key);
            if (v == null || v.Type != NtType.Boolean)
                ThrowException(m_path + PathSeperatorChar + key, v, NtType.Boolean);
            return v.GetBoolean();
        }

        ///<inheritdoc/>
        public bool PutStringArray(string key, string[] value)
        {
            return SetEntryValue(m_path + PathSeperatorChar + key, Value.MakeStringArray(value));
        }

        ///<inheritdoc/>
        [Obsolete("Please use the Default Value Get... Methods instead.")]
        public string[] GetStringArray(string key)
        {
            var v = GetEntryValue(m_path + PathSeperatorChar + key);
            if (v == null || v.Type != NtType.StringArray)
                ThrowException(m_path + PathSeperatorChar + key, v, NtType.StringArray);
            return v.GetStringArray();
        }

        ///<inheritdoc/>
        public string[] GetStringArray(string key, string[] defaultValue)
        {
            var v = GetEntryValue(m_path + PathSeperatorChar + key);
            if (v == null || v.Type != NtType.StringArray) return defaultValue;
            return v.GetStringArray();
        }

        ///<inheritdoc/>
        public bool PutNumberArray(string key, double[] value)
        {
            return SetEntryValue(m_path + PathSeperatorChar + key, Value.MakeDoubleArray(value));
        }

        ///<inheritdoc/>
        [Obsolete("Please use the Default Value Get... Methods instead.")]
        public double[] GetNumberArray(string key)
        {
            var v = GetEntryValue(m_path + PathSeperatorChar + key);
            if (v == null || v.Type != NtType.DoubleArray)
                ThrowException(m_path + PathSeperatorChar + key, v, NtType.DoubleArray);
            return v.GetDoubleArray();
        }

        ///<inheritdoc/>
        public double[] GetNumberArray(string key, double[] defaultValue)
        {
            var v = GetEntryValue(m_path + PathSeperatorChar + key);
            if (v == null || v.Type != NtType.DoubleArray) return defaultValue;
            return v.GetDoubleArray();
        }

        ///<inheritdoc/>
        public bool PutBooleanArray(string key, bool[] value)
        {
            return SetEntryValue(m_path + PathSeperatorChar + key, Value.MakeBooleanArray(value));
        }

        ///<inheritdoc/>
        [Obsolete("Please use the Default Value Get... Methods instead.")]
        public bool[] GetBooleanArray(string key)
        {
            var v = GetEntryValue(m_path + PathSeperatorChar + key);
            if (v == null || v.Type != NtType.BooleanArray)
                ThrowException(m_path + PathSeperatorChar + key, v, NtType.BooleanArray);
            return v.GetBooleanArray();
        }

        ///<inheritdoc/>
        public bool PutRaw(string key, byte[] value)
        {
            return SetEntryValue(m_path + PathSeperatorChar + key, Value.MakeRaw(value));
        }
        ///<inheritdoc/>
        [Obsolete("Please use the Default Value Get... Methods instead.")]
        public byte[] GetRaw(string key)
        {
            var v = GetEntryValue(m_path + PathSeperatorChar + key);
            if (v == null || v.Type != NtType.Raw)
                ThrowException(m_path + PathSeperatorChar + key, v, NtType.Raw);
            return v.GetRaw();
        }
        ///<inheritdoc/>
        public byte[] GetRaw(string key, byte[] defaultValue)
        {
            var v = GetEntryValue(m_path + PathSeperatorChar + key);
            if (v == null || v.Type != NtType.Raw) return defaultValue;
            return v.GetRaw();
        }

        ///<inheritdoc/>
        public bool[] GetBooleanArray(string key, bool[] defaultValue)
        {
            var v = GetEntryValue(m_path + PathSeperatorChar + key);
            if (v == null || v.Type != NtType.BooleanArray) return defaultValue;
            return v.GetBooleanArray(); ;
        }

        private readonly Dictionary<ITableListener, List<int>> m_listenerMap = new Dictionary<ITableListener, List<int>>();

        private readonly Dictionary<Action<ITable, string, object, NotifyFlags>, List<int>> m_actionListenerMap = new Dictionary<Action<ITable, string, object, NotifyFlags>, List<int>>();

        ///<inheritdoc/>
        public void AddTableListenerEx(ITableListener listener, NotifyFlags flags)
        {
            List<int> adapters;
            if (!m_listenerMap.TryGetValue(listener, out adapters))
            {
                adapters = new List<int>();
                m_listenerMap.Add(listener, adapters);
            }

            // ReSharper disable once InconsistentNaming
            EntryListenerCallback func = (uid, key, value, flags_) =>
            {
                string relativeKey = key.Substring(m_path.Length + 1);
                if (relativeKey.IndexOf(PathSeperatorChar) != -1)
                {
                    return;
                }
                listener.ValueChanged(this, relativeKey, value.Val, flags_);
            };

            int id = NtCore.AddEntryListener(m_path + PathSeperatorChar, func, flags);

            adapters.Add(id);
        }

        ///<inheritdoc/>
        public void AddTableListenerEx(string key, ITableListener listener, NotifyFlags flags)
        {
            List<int> adapters;
            if (!m_listenerMap.TryGetValue(listener, out adapters))
            {
                adapters = new List<int>();
                m_listenerMap.Add(listener, adapters);
            }
            string fullKey = m_path + PathSeperatorChar + key;
            // ReSharper disable once InconsistentNaming
            EntryListenerCallback func = (uid, funcKey, value, flags_) =>
            {
                if (!funcKey.Equals(fullKey))
                    return;
                listener.ValueChanged(this, key, value.Val, flags_);
            };

            int id = NtCore.AddEntryListener(fullKey, func, flags);

            adapters.Add(id);
        }

        ///<inheritdoc/>
        public void AddSubTableListener(ITableListener listener, bool localNotify)
        {
            List<int> adapters;
            if (!m_listenerMap.TryGetValue(listener, out adapters))
            {
                adapters = new List<int>();
                m_listenerMap.Add(listener, adapters);
            }
            HashSet<string> notifiedTables = new HashSet<string>();
            // ReSharper disable once InconsistentNaming
            EntryListenerCallback func = (uid, key, value, flags_) =>
            {
                string relativeKey = key.Substring(m_path.Length + 1);
                int endSubTable = relativeKey.IndexOf(PathSeperatorChar);
                if (endSubTable == -1)
                    return;
                string subTableKey = relativeKey.Substring(0, endSubTable);
                if (notifiedTables.Contains(subTableKey))
                    return;
                notifiedTables.Add(subTableKey);
                listener.ValueChanged(this, subTableKey, GetSubTable(subTableKey), flags_);
            };
            NotifyFlags flags = NotifyFlags.NotifyNew | NotifyFlags.NotifyUpdate;
            if (localNotify)
                flags |= NotifyFlags.NotifyLocal;
            int id = NtCore.AddEntryListener(m_path + PathSeperatorChar, func, flags);

            adapters.Add(id);
        }

        ///<inheritdoc/>
        public void AddTableListener(ITableListener listener, bool immediateNotify = false)
        {
            NotifyFlags flags = NotifyFlags.NotifyNew | NotifyFlags.NotifyUpdate;
            if (immediateNotify)
                flags |= NotifyFlags.NotifyImmediate;
            AddTableListenerEx(listener, flags);
        }

        ///<inheritdoc/>
        public void AddTableListener(string key, ITableListener listener, bool immediateNotify = false)
        {
            NotifyFlags flags = NotifyFlags.NotifyNew | NotifyFlags.NotifyUpdate;
            if (immediateNotify)
                flags |= NotifyFlags.NotifyImmediate;
            AddTableListenerEx(key, listener, flags);
        }

        ///<inheritdoc/>
        public void AddSubTableListener(ITableListener listener)
        {
            AddSubTableListener(listener, false);
        }

        ///<inheritdoc/>
        public void RemoveTableListener(ITableListener listener)
        {
            List<int> adapters;
            if (m_listenerMap.TryGetValue(listener, out adapters))
            {
                foreach (int t in adapters)
                {
                    NtCore.RemoveEntryListener(t);
                }
                adapters.Clear();
            }
        }


        ///<inheritdoc/>
        public void AddTableListenerEx(Action<ITable, string, object, NotifyFlags> listenerDelegate, NotifyFlags flags)
        {
            List<int> adapters;
            if (!m_actionListenerMap.TryGetValue(listenerDelegate, out adapters))
            {
                adapters = new List<int>();
                m_actionListenerMap.Add(listenerDelegate, adapters);
            }

            // ReSharper disable once InconsistentNaming
            EntryListenerCallback func = (uid, key, value, flags_) =>
            {
                string relativeKey = key.Substring(m_path.Length + 1);
                if (relativeKey.IndexOf(PathSeperatorChar) != -1)
                {
                    return;
                }
                listenerDelegate(this, relativeKey, value.Val, flags_);
            };

            int id = NtCore.AddEntryListener(m_path + PathSeperatorChar, func, flags);

            adapters.Add(id);
        }

        ///<inheritdoc/>
        public void AddTableListenerEx(string key, Action<ITable, string, object, NotifyFlags> listenerDelegate, NotifyFlags flags)
        {
            List<int> adapters;
            if (!m_actionListenerMap.TryGetValue(listenerDelegate, out adapters))
            {
                adapters = new List<int>();
                m_actionListenerMap.Add(listenerDelegate, adapters);
            }
            string fullKey = m_path + PathSeperatorChar + key;
            // ReSharper disable once InconsistentNaming
            EntryListenerCallback func = (uid, funcKey, value, flags_) =>
            {
                if (!funcKey.Equals(fullKey))
                    return;
                listenerDelegate(this, key, value.Val, flags_);
            };

            int id = NtCore.AddEntryListener(fullKey, func, flags);

            adapters.Add(id);
        }

        ///<inheritdoc/>
        public void AddSubTableListener(Action<ITable, string, object, NotifyFlags> listenerDelegate, bool localNotify)
        {
            List<int> adapters;
            if (!m_actionListenerMap.TryGetValue(listenerDelegate, out adapters))
            {
                adapters = new List<int>();
                m_actionListenerMap.Add(listenerDelegate, adapters);
            }
            HashSet<string> notifiedTables = new HashSet<string>();
            // ReSharper disable once InconsistentNaming
            EntryListenerCallback func = (uid, key, value, flags_) =>
            {
                string relativeKey = key.Substring(m_path.Length + 1);
                int endSubTable = relativeKey.IndexOf(PathSeperatorChar);
                if (endSubTable == -1)
                    return;
                string subTableKey = relativeKey.Substring(0, endSubTable);
                if (notifiedTables.Contains(subTableKey))
                    return;
                notifiedTables.Add(subTableKey);
                listenerDelegate(this, subTableKey, GetSubTable(subTableKey), flags_);
            };
            NotifyFlags flags = NotifyFlags.NotifyNew | NotifyFlags.NotifyUpdate;
            if (localNotify)
                flags |= NotifyFlags.NotifyLocal;
            int id = NtCore.AddEntryListener(m_path + PathSeperatorChar, func, flags);

            adapters.Add(id);
        }

        ///<inheritdoc/>
        public void AddTableListener(Action<ITable, string, object, NotifyFlags> listenerDelegate, bool immediateNotify = false)
        {
            NotifyFlags flags = NotifyFlags.NotifyNew | NotifyFlags.NotifyUpdate;
            if (immediateNotify)
                flags |= NotifyFlags.NotifyImmediate;
            AddTableListenerEx(listenerDelegate, flags);
        }

        ///<inheritdoc/>
        public void AddTableListener(string key, Action<ITable, string, object, NotifyFlags> listenerDelegate, bool immediateNotify = false)
        {
            NotifyFlags flags = NotifyFlags.NotifyNew | NotifyFlags.NotifyUpdate;
            if (immediateNotify)
                flags |= NotifyFlags.NotifyImmediate;
            AddTableListenerEx(key, listenerDelegate, flags);
        }

        ///<inheritdoc/>
        public void AddSubTableListener(Action<ITable, string, object, NotifyFlags> listenerDelegate)
        {
            AddSubTableListener(listenerDelegate, false);
        }

        ///<inheritdoc/>
        public void RemoveTableListener(Action<ITable, string, object, NotifyFlags> listenerDelegate)
        {
            List<int> adapters;
            if (m_actionListenerMap.TryGetValue(listenerDelegate, out adapters))
            {
                foreach (int t in adapters)
                {
                    NtCore.RemoveEntryListener(t);
                }
                adapters.Clear();
            }
        }

        private readonly Dictionary<IRemoteConnectionListener, int> m_connectionListenerMap =
            new Dictionary<IRemoteConnectionListener, int>();

        private readonly Dictionary<Action<IRemote, ConnectionInfo, bool>, int> m_actionConnectionListenerMap
            = new Dictionary<Action<IRemote, ConnectionInfo, bool>, int>();

        ///<inheritdoc/>
        public void AddConnectionListener(IRemoteConnectionListener listener, bool immediateNotify)
        {

            if (m_connectionListenerMap.ContainsKey(listener))
            {
                throw new ArgumentException("Cannot add the same listener twice", nameof(listener));
            }

            ConnectionListenerCallback func = (uid, connected, conn) =>
            {
                if (connected) listener.Connected(this, conn);
                else listener.Disconnected(this, conn);
            };

            int id = NtCore.AddConnectionListener(func, immediateNotify);

            m_connectionListenerMap.Add(listener, id);

        }

        ///<inheritdoc/>
        public void RemoveConnectionListener(IRemoteConnectionListener listener)
        {
            int val;
            if (m_connectionListenerMap.TryGetValue(listener, out val))
            {
                NtCore.RemoveConnectionListener(val);
            }
        }

        /// <inheritdoc/>
        public void AddConnectionListener(Action<IRemote, ConnectionInfo, bool> listener, bool immediateNotify)
        {
            if (m_actionConnectionListenerMap.ContainsKey(listener))
            {
                throw new ArgumentException("Cannot add the same listener twice", nameof(listener));
            }

            ConnectionListenerCallback func = (uid, connected, conn) =>
            {
                listener(this, conn, connected);
            };

            int id = NtCore.AddConnectionListener(func, immediateNotify);

            m_actionConnectionListenerMap.Add(listener, id);
        }

        /// <inheritdoc/>
        public void RemoveConnectionListener(Action<IRemote, ConnectionInfo, bool> listener)
        {
            int val;
            if (m_actionConnectionListenerMap.TryGetValue(listener, out val))
            {
                NtCore.RemoveConnectionListener(val);
            }
        }

        /// <summary>
        /// Gets if the NetworkTables is connected to a client or server.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                var conns = NtCore.GetConnections();
                return conns.Count > 0;
            }
        }

        /// <summary>
        /// Gets a list of all the connections attached to this instance.
        /// </summary>
        /// <remarks>
        /// Note that connections do not propogate through the server to clients.
        /// This means that a client will see at most 1 connection, and the server will see
        /// all connections to itself.
        /// </remarks>
        /// <returns>An array of all connections attached to this instance.</returns>
        public static List<ConnectionInfo> Connections()
        {
            return NtCore.GetConnections();
        }

        /// <summary>
        /// Gets if the NetworkTables instance is a Server.
        /// </summary>
        public bool IsServer => !Client;
    }
}
