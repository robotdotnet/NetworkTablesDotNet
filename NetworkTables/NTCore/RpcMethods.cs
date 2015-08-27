using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NetworkTables.NTCore.RPC;
using static NetworkTables.NTCore.Interop;

namespace NetworkTables.NTCore
{
    public static class RpcMethods
    {
        public delegate byte[] RPCCallback(string name, byte[] params_str);


        public static byte[] PackRpcValues(params RPCValue[] values)
        {
            RpcEncoder enc = new RpcEncoder();
            foreach (var value in values)
            {
                enc.WriteValue(value);
            }
            return enc.Buffer;
        }

        public static List<RPCValue> UnpackRpcValues(byte[] packed, params NT_Type[] types)
        {
            RpcDecoder dec = new RpcDecoder(packed);

            List<RPCValue> values = new List<RPCValue>();
            foreach (var type in types)
            {
                var item = dec.ReadValue(type);
                if (item == null) return null;
                values.Add(item);
            }
            return values;
        }

        private static readonly List<NT_RPCCallback> s_rpcCallbacks = new List<NT_RPCCallback>();

        public static void CreateRpc(string name, NT_RpcDefinition def, RPCCallback callback)
        {
            Interop.NT_RPCCallback modCallback =
                (IntPtr data, IntPtr ptr, UIntPtr len, IntPtr intPtr, UIntPtr paramsLen, ref UIntPtr resultsLen) =>
                {
                    string retName = CoreMethods.ReadUTF8String(ptr, len);
                    byte[] param = CoreMethods.ReadUTF8StringToByteArray(intPtr, paramsLen);
                    byte[] cb = callback(retName, param);
                    resultsLen = (UIntPtr)cb.Length;
                    IntPtr retPtr = NT_AllocateCharArray(resultsLen);
                    Marshal.Copy(cb, 0, retPtr, cb.Length);
                    return retPtr;
                };

            UIntPtr packed_len = UIntPtr.Zero;
            byte[] packed = PackRpcDefinition(def, ref packed_len);
            UIntPtr name_len;
            byte[] name_b = CoreMethods.CreateUTF8String(name, out name_len);
            Interop.NT_CreateRpc(name_b, name_len, packed, packed_len, IntPtr.Zero, modCallback);
            s_rpcCallbacks.Add(modCallback);
        }

        public static void CreatePolledPrc(string name, NT_RpcDefinition def)
        {
            UIntPtr packed_len = UIntPtr.Zero;
            byte[] packed = PackRpcDefinition(def, ref packed_len);
            UIntPtr name_len;
            byte[] name_b = CoreMethods.CreateUTF8String(name, out name_len);
            NT_CreatePolledRpc(name_b, name_len, packed, packed_len);
        }

        public static bool PollRpc(bool blocking, ref NT_RpcCallInfo info)
        {
            int retVal = NT_PollRpc(blocking ? 1 : 0, ref info);
            return retVal != 0;
        }

        public static byte[] PackRpcDefinition(NT_RpcDefinition def, ref UIntPtr packed_len)
        {
            RpcEncoder enc = new RpcEncoder();
            enc.Write8((byte)def.version);
            enc.WriteString(def.name);

            int params_size = def.paramsArray.Length;
            if (params_size > 0xff) params_size = 0xff;
            enc.Write8((byte)params_size);
            for (int i = 0; i < params_size; ++i)
            {
                enc.WriteType(def.paramsArray[i].value.Type);
                enc.WriteString(def.paramsArray[i].name);
                enc.WriteValue(def.paramsArray[i].value);
            }

            int results_size = def.resultsArray.Length;
            if (results_size > 0xff) results_size = 0xff;
            enc.Write8((byte)results_size);
            for (int i = 0; i < results_size; ++i)
            {
                enc.WriteType(def.resultsArray[i].type);
                enc.WriteString(def.resultsArray[i].name);
            }
            packed_len = (UIntPtr)enc.Buffer.Length;
            return enc.Buffer;
        }

        public static bool GetRpcResult(bool blocking, uint call_uid, out byte[] result)
        {
            UIntPtr size = UIntPtr.Zero;
            IntPtr retVal = NT_GetRpcResult(blocking ? 1 : 0, call_uid, ref size);
            if (retVal == IntPtr.Zero)
            {
                result = null;
                return false;

            }
            result = CoreMethods.ReadUTF8StringToByteArray(retVal, size);
            return true;
        }

        public static uint CallRpc(string name, params RPCValue[] rpcValues)
        {
            UIntPtr size;
            byte[] nameB = CoreMethods.CreateUTF8String(name, out size);
            var vals = PackRpcValues(rpcValues);
            return NT_CallRpc(nameB, size, vals, (UIntPtr)vals.Length);
        }

        
    }
}
