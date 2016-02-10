using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables
{
    public static class NtCore
    {
        public static Value GetEntryValue(string name)
        {
            return Storage.Instance.GetEntryValue(name);
        }

        public static bool SetEntryValue(string name, Value value)
        {
            return Storage.Instance.SetEntryValue(name, value);
        }

        public static void SetEntryTypeValue(string name, Value value)
        {
            Storage.Instance.SetEntryTypeValue(name, value);
        }

        public static void SetEntryFlags(string name, EntryFlags flags)
        {
            Storage.Instance.SetEntryFlags(name, flags);
        }

        public static EntryFlags GetEntryFlags(string name)
        {
            return Storage.Instance.GetEntryFlags(name);
        }

        public static void DeleteEntry(string name)
        {
            Storage.Instance.DeleteEntry(name);
        }

        public static void DeleteAllEntries()
        {
            Storage.Instance.DeleteAllEntries();
        }

        public static List<EntryInfo> GetEntryInfo(string prefix, NtType types)
        {
            return Storage.Instance.GetEntryInfo(prefix, types);
        }

        public static void Flush()
        {
            Dispatcher.Instance.Flush();
        }

        public delegate void EntryListenerCallback(uint uid, string name, Value value, NotifyFlags flags);

        public delegate void ConnectionListenerCallback(uint uid, bool connected, ConnectionInfo conn);

        public static uint AddEntryListener(string prefix, EntryListenerCallback callback, NotifyFlags flags)
        {
            uint uid = Notifier.Instance.AddEntryListener(prefix, callback, flags);
            if ((flags & NotifyFlags.NotifyImmediate) != 0)
                Storage.Instance.NotifyEntries(prefix, callback);
            return uid;
        }

        public static void RemoveEntryListener(uint uid)
        {
            Notifier.Instance.RemoveEntryListener(uid);
        }

        public static uint AddConnectionListener(ConnectionListenerCallback callback, bool immediateNotify)
        {
            uint uid = Notifier.Instance.AddConnectionListener(callback);
            if (immediateNotify) Dispatcher.Instance.NotifyConnections(callback);
            return uid;
        }

        public static void RemoveConnectionListener(uint uid)
        {
            Notifier.Instance.RemoveConnectionListener(uid);
        }

        public static bool NotifierDestroyed()
        {
            return Notifier.Destroyed();
        }


        public static void SetNetworkIdentity(string name)
        {
            Dispatcher.Instance.SetIdentity(name);
        }

        public static void StartServer(string persistFilename, string listenAddress, uint port)
        {
            Dispatcher.Instance.StartServer(persistFilename, listenAddress, port);
        }

        public static void StopServer()
        {
            Dispatcher.Instance.Stop();
        }

        public static void StartClient(string serverName, uint port)
        {
            Dispatcher.Instance.StartClient(serverName, port);
        }

        public static void StopRpcServer()
        {
            //Noop
        }

        public static void StopNotifier()
        {
            Notifier.Instance.Stop();
        }

        public static void SetUpdateRate(double interval)
        {
            Dispatcher.Instance.SetUpdateRate(interval);
        }

        public static List<ConnectionInfo> GetConnections()
        {
            return Dispatcher.Instance.GetConnections();
        }

        public static string SavePersistent(string filename)
        {
            return Storage.Instance.SavePersistent(filename, false);
        }

        public static string LoadPersistent(string filename, Action<int, string> warn)
        {
            return Storage.Instance.LoadPersistent(filename, warn);
        }

        public static ulong Now()
        {
            return Support.Timestamp.Now();
        }

        public delegate void LogFunc(uint level, string file, uint line, string msg);

        public static void SetLogger(LogFunc func, uint minLevel)
        {
            //Setup Logger
        }
    }
}
