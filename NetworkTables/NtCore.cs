using System;
using System.Collections.Generic;

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

        public delegate void EntryListenerCallback(int uid, string name, Value value, NotifyFlags flags);

        public delegate void ConnectionListenerCallback(int uid, bool connected, ConnectionInfo conn);

        public delegate byte[] RpcCallback(string name, byte[] param);

        public static int AddEntryListener(string prefix, EntryListenerCallback callback, NotifyFlags flags)
        {
            Notifier notifier = Notifier.Instance;
            int uid = notifier.AddEntryListener(prefix, callback, flags);
            notifier.Start();
            if ((flags & NotifyFlags.NotifyImmediate) != 0)
                Storage.Instance.NotifyEntries(prefix, callback);
            return uid;
        }

        public static void RemoveEntryListener(int uid)
        {
            Notifier.Instance.RemoveEntryListener(uid);
        }

        public static int AddConnectionListener(ConnectionListenerCallback callback, bool immediateNotify)
        {
            Notifier notifier = Notifier.Instance;
            int uid = notifier.AddConnectionListener(callback);
            notifier.Start();
            if (immediateNotify) Dispatcher.Instance.NotifyConnections(callback);
            return uid;
        }

        public static void RemoveConnectionListener(int uid)
        {
            Notifier.Instance.RemoveConnectionListener(uid);
        }

        public static bool NotifierDestroyed()
        {
            return Notifier.Destroyed();
        }

        public static void CreateRpc(string name, byte[] def, RpcCallback callback)
        {
            Storage.Instance.CreateRpc(name, def, callback);
        }

        public static void CreatePolledRpc(string name, byte[] def)
        {
            Storage.Instance.CreatePolledRpc(name, def);
        }

        public static bool PollRpc(bool blocking, ref RpcCallInfo callInfo)
        {
            return RpcServer.Instance.PollRpc(blocking, ref callInfo);
        }

        public static void PostRpcResponse(int rpcId, int callUid, params byte[] result)
        {
            RpcServer.Instance.PostRpcResponse(rpcId, callUid, result);
        }

        public static long CallRpc(string name, params byte[] param)
        {
            return Storage.Instance.CallRpc(name, param);
        }

        public static bool GetRpcResult(bool blocking, long callUid, ref byte[] result)
        {
            return Storage.Instance.GetRpcResult(blocking, callUid, ref result);
        }

        public static byte[] PackRpcDefinition(RpcDefinition def)
        {
            WireEncoder enc = new WireEncoder(0x0300);
            enc.Write8((byte)def.Version);
            enc.WriteString(def.Name);

            int paramsSize = def.Params.Count;
            if (paramsSize > 0xff) paramsSize = 0xff;
            enc.Write8((byte)paramsSize);
            for (int i = 0; i < paramsSize; i++)
            {
                enc.WriteType(def.Params[i].DefValue.Type);
                enc.WriteString(def.Params[i].Name);
                enc.WriteValue(def.Params[i].DefValue);
            }

            int resultsSize = def.Results.Count;
            if (resultsSize > 0xff) resultsSize = 0xff;
            enc.Write8((byte)resultsSize);
            for (int i = 0; i < resultsSize; i++)
            {
                enc.WriteType(def.Results[i].Type);
                enc.WriteString(def.Results[i].Name);
            }
            return enc.Buffer;
        }

        public static bool UnpackRpcDefinition(byte[] packed, ref RpcDefinition def)
        {
            RawMemoryStream iStream = new RawMemoryStream(packed, packed.Length);
            WireDecoder dec = new WireDecoder(iStream, 0x0300);
            byte ref8 = 0;
            ushort ref16 = 0;
            ulong ref32 = 0;
            string str = "";

            if (!dec.Read8(ref ref8)) return false;
            def.Version = ref8;
            if (!dec.ReadString(ref str)) return false;
            def.Name = str;

            int paramsSize = 0;
            if (!dec.Read8(ref ref8)) return false;
            paramsSize = ref8;
            def.Params.Clear();
            NtType type = 0;
            for (int i = 0; i < paramsSize; i++)
            {
                
                if (!dec.ReadType(ref type)) return false;
                if (!dec.ReadString(ref str)) return false;
                var val = dec.ReadValue(type);
                if (val == null) return false;
                def.Params.Add(new RpcParamDef(str, val));
            }

            int resultsSize = 0;
            if (!dec.Read8(ref ref8)) return false;
            resultsSize = ref8;
            def.Results.Clear();
            for (int i = 0; i < resultsSize; i++)
            {
                type = 0;
                if (!dec.ReadType(ref type)) return false;
                if (!dec.ReadString(ref str)) return false;
                def.Results.Add(new RpcResultsDef(str, type));
            }

            return true;
        }

        public static byte[] PackRpcValues(params Value[] values)
        {
            WireEncoder enc = new WireEncoder(0x0300);
            foreach (var value in values)
            {
                enc.WriteValue(value);
            }
            return enc.Buffer;
        }

        public static byte[] PackRpcValues(List<Value> values)
        {
            WireEncoder enc = new WireEncoder(0x0300);
            foreach (var value in values)
            {
                enc.WriteValue(value);
            }
            return enc.Buffer;
        }

        public static List<Value> UnpackRpcValues(byte[] packed, params NtType[] types)
        {
            RawMemoryStream iStream = new RawMemoryStream(packed, packed.Length);
            WireDecoder dec = new WireDecoder(iStream, 0x0300);
            List<Value> vec = new List<Value>();
            foreach (var ntType in types)
            {
                var item = dec.ReadValue(ntType);
                if (item == null) return new List<Value>();
                vec.Add(item);
            }
            return vec;
        }

        public static List<Value> UnpackRpcValues(byte[] packed, List<NtType> types)
        {
            RawMemoryStream iStream = new RawMemoryStream(packed, packed.Length);
            WireDecoder dec = new WireDecoder(iStream, 0x0300);
            List<Value> vec = new List<Value>();
            foreach (var ntType in types)
            {
                var item = dec.ReadValue(ntType);
                if (item == null) return new List<Value>();
                vec.Add(item);
            }
            return vec;
        }

        public static void SetNetworkIdentity(string name)
        {
            Dispatcher.Instance.SetIdentity(name);
        }

        public static void StartServer(string persistFilename, string listenAddress, int port)
        {
            Dispatcher.Instance.StartServer(persistFilename, listenAddress, port);
        }

        public static void StopServer()
        {
            Dispatcher.Instance.Stop();
        }

        public static void StartClient(string serverName, int port)
        {
            Dispatcher.Instance.StartClient(serverName, port);
        }

        public static void StopClient()
        {
            Dispatcher.Instance.Stop();
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

        public delegate void LogFunc(LogLevel level, string file, int line, string msg);

        public static void SetLogger(LogFunc func, LogLevel minLevel)
        {
            Logger logger = Logger.Instance;
            logger.SetLogger(func);
            logger.SetMinLevel(minLevel);
        }
        /*
        public static NtType GetType(string name)
        {
            var v = GetEntryValue(name);
            if (v == null) return NtType.Unassigned;
            return v.Type;
        }

        public static bool ContainsKey(string name)
        {
            return GetType(name) != NtType.Unassigned;
        }

        public static EntryInfo[] GetEntries(string prefix, NtType types)
        {
            var arr = GetEntryInfo(prefix, types);
            return arr.ToArray();
        }

        public static bool SetEntryDouble(string name, double value, bool force)
        {
            if (force)
            {
                SetEntryTypeValue(name, Value.MakeDouble(value));
                return true;
            }
            else return SetEntryValue(name, Value.MakeDouble(value));
        }

        public static bool SetEntryBoolean(string name, bool value, bool force)
        {
            if (force)
            {
                SetEntryTypeValue(name, Value.MakeBoolean(value));
                return true;
            }
            else return SetEntryValue(name, Value.MakeBoolean(value));
        }

        public static bool SetEntryString(string name, string value, bool force)
        {
            if (force)
            {
                SetEntryTypeValue(name, Value.MakeString(value));
                return true;
            }
            else return SetEntryValue(name, Value.MakeString(value));
        }

        public static bool SetEntryRaw(string name, byte[] value, bool force)
        {
            if (force)
            {
                SetEntryTypeValue(name, Value.MakeRaw(value));
                return true;
            }
            else return SetEntryValue(name, Value.MakeRaw(value));
        }

        public static bool SetEntryBooleanArray(string name, bool[] value, bool force)
        {
            if (force)
            {
                SetEntryTypeValue(name, Value.MakeBooleanArray(value));
                return true;
            }
            else return SetEntryValue(name, Value.MakeBooleanArray(value));
        }

        public static bool SetEntryDoubleArray(string name, double[] value, bool force)
        {
            if (force)
            {
                SetEntryTypeValue(name, Value.MakeDoubleArray(value));
                return true;
            }
            else return SetEntryValue(name, Value.MakeDoubleArray(value));
        }

        public static bool SetStringArray(string name, string[] value, bool force)
        {
            if (force)
            {
                SetEntryTypeValue(name, Value.MakeStringArray(value));
                return true;
            }
            else return SetEntryValue(name, Value.MakeStringArray(value));
        }

        public static bool GetEntryBoolean(string name, ref ulong lastChange, ref bool value)
        {
            var v = GetEntryValue(name);
            if (v == null || !v.IsBoolean()) return false;
            value = v.GetBoolean();
            lastChange = v.LastChange;
            return true;
        }

        public static bool GetEntryDouble(string name, ref ulong lastChange, ref double value)
        {
            var v = GetEntryValue(name);
            if (v == null || !v.IsDouble()) return false;
            value = v.GetDouble();
            lastChange = v.LastChange;
            return true;
        }
        */
    }
}
