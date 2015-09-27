using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkTables.Tables;

namespace NetworkTables
{
    public class NetworkTable : ITable, IRemote
    {
        private static StaticNetworkTable NetworkTableProvider = null;

        private ITable Table;

        public const char PATH_SEPERATOR_CHAR = '/';
        internal const uint DEFAULT_PORT = 1735;
        internal static uint Port = DEFAULT_PORT;
        internal static string s_ipAddress = null;
        internal static bool client = false;
        internal static bool running = false;

        static NetworkTable()
        {
            if (IntPtr.Size != 4)
            {
                //Running in 64 Bit. NTCore only supports 32 bit processes, so run as managed
                NetworkTableProvider = new StaticNetworkTableManaged();
                return;
            }
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                //If not on WinNT, Check for RoboRIO
                if (File.Exists("/usr/local/frc/bin/frcRunRobot.sh"))
                {
                    //We are on RoboRIO, Use NT Core
                    NetworkTableProvider = new StaticNetworkTableCore();
                }
                else
                {
                    //We are on either linux or unix, use NT Managed
                    NetworkTableProvider = new StaticNetworkTableManaged();
                }
                return;
            }
            //We can assume we are running on Windows 32 Bit, so we can use NTCore.
            //This will probably not be right if used on Windows Phone, 
            //So we have the force method that users can use there.
            NetworkTableProvider = new StaticNetworkTableCore();

            //If we are core, Check for the library. If not found fall back to managed

            Console.WriteLine(NetworkTableProvider.ToString());
        }

        public static void ForceManaged()
        {
            NetworkTableProvider = new StaticNetworkTableManaged();
        }

        public static void Initialize()
        {
            NetworkTableProvider?.Initialize();
        }


        public static void Shutdown()
        {
            NetworkTableProvider?.Shutdown();
        }

        public static void SetClientMode()
        {
            NetworkTableProvider?.SetClientMode();
        }

        public static void SetServerMode()
        {
            NetworkTableProvider?.SetServerMode();
        }

        public static void SetTeam(int team)
        {
            NetworkTableProvider?.SetTeam(team);
        }

        public static void SetIPAddress(string address)
        {
            NetworkTableProvider?.SetIPAddress(address);
        }

        public static NetworkTable GetTable(string key)
        {
            
            return new NetworkTable(NetworkTableProvider?.GetTable(key));
        }

        public static void SetPort(uint port)
        {
            NetworkTableProvider?.SetPort(port);
        }

        internal NetworkTable(ITable table)
        {
            Path = table.Path;
            Table = table;
        }

        public string Path { get; }

        public bool ContainsKey(string key)
        {
            return Table.ContainsKey(key);
        }

        public bool ContainsSubTable(string key)
        {
            return Table.ContainsKey(key);
        }

        public ITable GetSubTable(string key)
        {
            return Table.GetSubTable(key);
        }

        public void Persist(string key)
        {
            Table.Persist(key);
        }

        public object GetValue(string key)
        {
            return Table.GetValue(key);
        }

        public void PutValue(string key, object value)
        {
            Table.PutValue(key, value);
        }

        public void PutNumber(string key, double value)
        {
            Table.PutNumber(key, value);
        }

        public double GetNumber(string key, double defaultValue)
        {
            return Table.GetNumber(key, defaultValue);
        }

        public void PutString(string key, string value)
        {
            Table.PutString(key, value);
        }

        public string GetString(string key, string defaultValue)
        {
            return Table.GetString(key, defaultValue);
        }

        public void PutBoolean(string key, bool value)
        {
            Table.PutBoolean(key, value);
        }

        public bool GetBoolean(string key, bool defaultValue)
        {
            return Table.GetBoolean(key, defaultValue);
        }

        public void AddTableListener(ITableListener listener, bool immediateNotify = false)
        {
            Table.AddTableListener(listener, immediateNotify);
        }

        public void AddTableListener(string key, ITableListener listener, bool immediateNotify)
        {
            Table.AddTableListener(key, listener, immediateNotify);
        }

        public void RemoveTableListener(ITableListener listener)
        {
            Table.RemoveTableListener(listener);
        }

        public void AddConnectionListener(IRemoteConnectionListener listener, bool immediateNotify)
        {
            throw new NotImplementedException();
        }

        public void RemoveConnectionListener(IRemoteConnectionListener listener)
        {
            throw new NotImplementedException();
        }

        public bool IsConnected()
        {
            return true;
        }

        public bool IsServer()
        {
            throw new NotImplementedException();
        }
    }
}
