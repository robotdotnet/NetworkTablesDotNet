using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables.NTCore.RPC
{
    public struct NT_RpcDefinition
    {
        private uint version;
        private NT_String name;
        private UIntPtr num_params;
        private IntPtr param;
        private UIntPtr num_results;
        private IntPtr results;
    }

    public struct NT_RpcResultDef
    {
        private NT_String name;
        private NT_Type type;
    }

    public struct NT_RpcCallInfo
    {
        private uint rpc_id;
        private uint call_uid;
        private NT_String name;
        private NT_String param;
    }
}
