using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkTables.NTCore;
using NetworkTables.Tables;
using static NetworkTables.NTCore.CoreMethods;

namespace NetworkTables
{
    /// <summary>
    /// This class is the Main Class for interfacing with NetworkTables. This is based on the new ntcore 3.0 protocol, and should be used on all Windows and Arm Linux implementations.
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

    public class NetworkTableCore : ITable, IDisposable
    {
        public string Path { get; }

        private readonly Dictionary<uint, ITableListener> m_listeners = new Dictionary<uint, ITableListener>();

        internal NetworkTableCore(string path)
        {
            this.Path = path;
        }

        public void Dispose()
        {
            foreach (var key in m_listeners.Keys)
            {
                RemoveEntryListener(key);
            }
            m_listeners.Clear();
        }

        public void AddTableListener(ITableListener listener, bool immediateNotify = false)
        {
            string path = Path + NetworkTable.PATH_SEPERATOR_CHAR;
            uint id = AddEntryListener(path, this, listener, immediateNotify);
            m_listeners.Add(id, listener);
        }

        public void AddTableListener(string key, ITableListener listener, bool immediateNotify)
        {
            string path = Path + NetworkTable.PATH_SEPERATOR_CHAR + key;
            uint id = AddEntryListener(path, this, listener, immediateNotify);
            m_listeners.Add(id, listener);
        }

        public void RemoveTableListener(ITableListener listener)
        {
            List<uint> keyMatches = new List<uint>();
            foreach (KeyValuePair<uint, ITableListener> valuePair in m_listeners)
            {
                if (valuePair.Value == listener)
                {
                    RemoveEntryListener(valuePair.Key);
                    keyMatches.Add(valuePair.Key);
                }
            }
            foreach (var keyMatch in keyMatches)
            {
                m_listeners.Remove(keyMatch);
            }
        }

        public ITable GetSubTable(string key)
        {
            string path = Path + NetworkTable.PATH_SEPERATOR_CHAR + key;
            return new NetworkTableCore(path);
        }

        public bool ContainsKey(string key)
        {
            string path = Path + NetworkTable.PATH_SEPERATOR_CHAR + key;
            return CoreMethods.GetType(path) != NT_Type.NT_UNASSIGNED;
        }

        public bool ContainsSubTable(string key)
        {
            string path = Path + NetworkTable.PATH_SEPERATOR_CHAR + key;
            using (EntryInfoArray array = GetEntryInfo(path, 0))
            {
                return array.Length != 0;
            }
        }

        public void Persist(string key)
        {
            string path = Path + NetworkTable.PATH_SEPERATOR_CHAR + key;
            SetEntryFlags(path, (uint)NT_EntryFlags.NT_PERSISTENT);
        }

        public object GetValue(string key)
        {
            string path = Path + NetworkTable.PATH_SEPERATOR_CHAR + key;
            NT_Type type;
            int status = 0;
            ulong lc = 0;
            type = CoreMethods.GetType(path);
            switch (type)
            {
                case NT_Type.NT_BOOLEAN:
                    return GetEntryBoolean(path, ref lc, ref status);
                case NT_Type.NT_DOUBLE:
                    return GetEntryDouble(path, ref lc, ref status);
                case NT_Type.NT_STRING:
                    return GetEntryString(path, ref lc);
                case NT_Type.NT_RAW:
                    return GetEntryRaw(path, ref lc);
                case NT_Type.NT_BOOLEAN_ARRAY:
                    return GetEntryBooleanArray(path, ref lc);
                case NT_Type.NT_DOUBLE_ARRAY:
                    return GetEntryDoubleArray(path, ref lc);
                case NT_Type.NT_STRING_ARRAY:
                    return GetEntryStringArray(path, ref lc);
                default:
                    return null;
            }
        }

        public void PutValue(string key, object value)
        {
            if (value is double) PutNumber(key, (double)value);
            else if (value is string) PutString(key, (string)value);
            else if (value is bool) PutBoolean(key, (bool)value);
            else if (value is double[])
            {
                string path = Path + NetworkTable.PATH_SEPERATOR_CHAR + key;
                SetEntryDoubleArray(path, (double[])value);
            }
            else if (value is bool[])
            {
                string path = Path + NetworkTable.PATH_SEPERATOR_CHAR + key;
                SetEntryBooleanArray(path, (bool[])value);
            }
            else if (value is string[])
            {
                string path = Path + NetworkTable.PATH_SEPERATOR_CHAR + key;
                SetEntryStringArray(path, (string[])value);
            }
            else
            {
                throw new ArgumentException("Value is either null or an invalid type.");
            }
        }

        public void PutNumber(string key, double value)
        {
            string path = Path + NetworkTable.PATH_SEPERATOR_CHAR + key;
            SetEntryDouble(path, value);
        }

        public double GetNumber(string key, double defaultValue)
        {
            string path = Path + NetworkTable.PATH_SEPERATOR_CHAR + key;
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
            string path = Path + NetworkTable.PATH_SEPERATOR_CHAR + key;
            SetEntryString(path, value);
        }

        public string GetString(string key, string defaultValue)
        {
            string path = Path + NetworkTable.PATH_SEPERATOR_CHAR + key;
            ulong lc = 0;
            string retVal = GetEntryString(path, ref lc);
            return retVal ?? defaultValue;
        }

        public void PutBoolean(string key, bool value)
        {
            string path = Path + NetworkTable.PATH_SEPERATOR_CHAR + key;
            SetEntryBoolean(path, value);
        }

        public bool GetBoolean(string key, bool defaultValue)
        {
            string path = Path + NetworkTable.PATH_SEPERATOR_CHAR + key;
            int status = 0;
            ulong lc = 0;
            bool retVal = GetEntryBoolean(path, ref lc, ref status);
            if (status == 0)
            {
                return defaultValue;
            }
            return retVal;
        }
    }
}
