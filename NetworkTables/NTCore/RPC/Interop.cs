using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables.NTCore.RPC
{
    [SuppressUnmanagedCodeSecurity]
    internal class Interop
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr NT_RPCCallback(
            IntPtr data, IntPtr name, UIntPtr name_len, IntPtr param, UIntPtr params_len, ref UIntPtr results_len);

        public static extern void NT_CreateRpc(byte[] name, UIntPtr name_len, byte[] def, UIntPtr def_len, IntPtr data,
            NT_RPCCallback callback);

        public static extern void NT_CreatePolledRpc(byte[] name, UIntPtr name_len, byte[] def, UIntPtr def_len);

        public static extern int NT_PollRpc(int blocking, ref NT_RpcCallInfo call_info);

        public static extern void NT_PostRpcResponse(uint rpc_id, uint call_uid, byte[] result, UIntPtr result_len);

        public static extern uint NT_CallRpc(byte[] name, UIntPtr name_len, byte[] param, UIntPtr params_len);

        public static extern IntPtr NT_GetRpcResult(int blocking, uint call_uid, ref UIntPtr result_len);

        public static extern IntPtr NT_PackRpcDefinition(ref NT_RpcDefinition def, ref UIntPtr packed_len);

        public static extern int NT_UnpackRpcDefinition(byte[] packed, UIntPtr packed_len, ref NT_RpcDefinition def);

        //The RPC Values are going to be a pain to deal with. That is going to need wrapper functions as well.
    }
}
