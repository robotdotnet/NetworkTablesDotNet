using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkTables.NetworkTables;
using NetworkTables.NetworkTables2.Thread;
using NetworkTables.Tables;
using static NetworkTables.NTCore.CoreMethods;

namespace NetworkTables
{
    internal class StaticNetworkTableManaged : StaticNetworkTable
    {
        internal static NTThreadManager s_threadManager = new DefaultThreadManager();


        internal static NetworkTableProvider s_staticProvider = null;
        internal static NetworkTableMode.CreateNodeDelegate s_mode = NetworkTableMode.CreateServerNode;


        internal static object s_lockObject = new object();


        internal static void CheckInit()
        {
            lock (s_lockObject)
            {
                if (s_staticProvider != null)
                    throw new InvalidOperationException("Network tables has already been initialized");
            }
        }

        /// <summary>
        /// Initialized a NetworkTable.
        /// </summary>
        /// <exception cref="InvalidOperationException">This is thrown if Network Tables
        /// has already been initialized.</exception>
        public void Initialize()
        {
            lock (s_lockObject)
            {
                CheckInit();
                s_staticProvider = new NetworkTableProvider(s_mode(NetworkTable.s_ipAddress, (int)NetworkTable.Port, s_threadManager));
            }
        }

        public void Shutdown()
        {
            //Noop, since shutdown didnt exist with the old network tables.
        }

        /// <summary>
        /// Sets that network tables should be in server mode.
        /// </summary>
        /// <remarks>This or <see cref="SetClientMode"/> must be called
        /// before <see cref="Initialize"/></remarks>
        public void SetServerMode()
        {
            lock (s_lockObject)
            {
                CheckInit();
                s_mode = NetworkTableMode.CreateServerNode;
            }
        }

        /// <summary>
        /// Sets that network tables should be in client mode.
        /// </summary>
        /// <remarks>This or <see cref="SetServerMode"/> must be called
        /// before <see cref="Initialize"/></remarks>
        public void SetClientMode()
        {
            lock (s_lockObject)
            {
                CheckInit();
                s_mode = NetworkTableMode.CreateClientNode;
            }
        }

        /// <summary>
        /// Sets the team that the robot is configured for.
        /// </summary>
        /// <param name="team">Your team number.</param>
        public void SetTeam(int team)
        {
            lock (s_lockObject)
            {
                SetIPAddress("10." + (team / 100) + "." + (team % 100) + ".2");
            }
        }

        /// <summary>
        /// Sets the ip address that will be connected to in client mode.
        /// </summary>
        /// <param name="address">The IP address to connect to.</param>
        public void SetIPAddress(string address)
        {
            lock (s_lockObject)
            {
                CheckInit();
                NetworkTable.s_ipAddress = address;
            }
        }

        /// <summary>
        /// Gets the table with the specified key.
        /// </summary>
        /// <remarks>If the table does not exist, a new table will be created.
        /// This will automatically initialize network tables if it has not been already.</remarks>
        /// <param name="key">The network table key to request.</param>
        /// <returns>The <see cref="NetworkTableManaged"/> requested.</returns>
        public ITable GetTable(string key)
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
                return s_staticProvider?.GetTable(NetworkTable.PATH_SEPERATOR_CHAR + key);
            }
        }

        public void SetPort(uint port)
        {
            CheckInit();
            NetworkTable.Port = port;
        }
    }

    internal class StaticNetworkTableCore : StaticNetworkTable
    {
        private void CheckInit()
        {
            if (NetworkTable.running)
                throw new InvalidOperationException("Network Tables has already been initialized");
        }

        public void Initialize()
        {
            CheckInit();
            if (NetworkTable.client)
            {
                StartClient(NetworkTable.s_ipAddress, NetworkTable.Port);
            }
            else
            {
                StartServer("networktables.ini", "", NetworkTable.Port);
            }
            NetworkTable.running = true;
        }

        public void Shutdown()
        {
            CheckInit();
            if (NetworkTable.client)
            {
                StopClient();
            }
            else
            {
                StopServer();
            }
            NetworkTable.running = false;
        }

        public void SetClientMode()
        {
            CheckInit();
            NetworkTable.client = true;
        }

        public void SetServerMode()
        {
            CheckInit();
            NetworkTable.client = false;
        }

        public void SetTeam(int team)
        {
            SetIPAddress($"10.{(team / 100)}.{(team % 100)}.2");
        }

        public void SetIPAddress(string address)
        {
            CheckInit();
            NetworkTable.s_ipAddress = address;
        }

        public ITable GetTable(string key)
        {
            if (!NetworkTable.running) Initialize();
            if (key == "")
                return new NetworkTableCore(key);
            return new NetworkTableCore(NetworkTable.PATH_SEPERATOR_CHAR + key);
        }

        public void SetPort(uint port)
        {
            CheckInit();
            NetworkTable.Port = port;
        }
    }

    interface StaticNetworkTable
    {
        void Initialize();
        void Shutdown();
        void SetClientMode();
        void SetServerMode();
        void SetTeam(int team);
        void SetIPAddress(string address);
        ITable GetTable(string key);

        void SetPort(uint port);
    }
}
