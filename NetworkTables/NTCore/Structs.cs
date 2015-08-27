using System;
using System.Runtime.InteropServices;
using System.Text;
using static NetworkTables.NTCore.Interop;

namespace NetworkTables.NTCore
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NT_String : IDisposable
    {
        private readonly IntPtr str;
        private readonly UIntPtr len;

        public NT_String(string str)
        {
            int bytes = Encoding.UTF8.GetByteCount(str);
            var allocString = NT_AllocateNTString((UIntPtr)bytes);
            byte[] buffer = new byte[bytes];
            Encoding.UTF8.GetBytes(str, 0, str.Length, buffer, 0);
            Marshal.Copy(buffer, 0, allocString.str, bytes);
            this.len = allocString.len;
            this.str = allocString.str;
        }

        public override string ToString()
        {
            byte[] arr = new byte[len.ToUInt64()];
            Marshal.Copy(str, arr, 0, (int)len.ToUInt64());
            return Encoding.UTF8.GetString(arr);
        }

        public bool IsNull()
        {
            return str == IntPtr.Zero;
        }

        public void Dispose()
        {
            NT_DisposeString(ref this);
        }
    }

    //This is not marked as IDisposable because we need to free it entirely as an array.
    [StructLayout(LayoutKind.Sequential)]
    public struct NT_EntryInfo
    {
        private readonly NT_String name;
        public readonly NT_Type Type;
        public readonly uint Flags;
        public readonly ulong LastChange;

        public string Name => name.ToString();
    }

    public struct EntryInfoArray : IDisposable
    {
        private NT_EntryInfo[] info;
        private readonly IntPtr arrayPtr;
        private readonly UIntPtr arraySize;

        public EntryInfoArray(NT_EntryInfo[] info, IntPtr arrayPtr, UIntPtr arraySize)
        {
            this.info = info;
            this.arraySize = arraySize;
            this.arrayPtr = arrayPtr;
        }

        public NT_EntryInfo this[int i] => info[i];

        public int Length => info.Length;

        public void Dispose()
        {
            NT_DisposeEntryInfoArray(arrayPtr, arraySize);
            info = null;
        }
    }

    //This is not marked as IDisposable because we need to free it entirely as an array.
    [StructLayout(LayoutKind.Sequential)]
    public struct NT_ConnectionInfo
    {
        private readonly NT_String remote_id;
        private readonly IntPtr remote_name;
        public readonly uint RemotePort;
        public readonly ulong LastUpdate;
        public readonly uint ProtocolVersion;

        public string RemoteName => CoreMethods.ReadUTF8String(remote_name);

        public string RemoteID => remote_id.ToString();
    }

    public struct ConnectionInfoArray : IDisposable
    {
        private NT_ConnectionInfo[] info;
        private readonly IntPtr arrayPtr;
        private readonly UIntPtr arraySize;

        public ConnectionInfoArray(NT_ConnectionInfo[] info, IntPtr arrayPtr, UIntPtr arraySize)
        {
            this.info = info;
            this.arraySize = arraySize;
            this.arrayPtr = arrayPtr;
        }

        public NT_ConnectionInfo this[int i] => info[i];

        public int Length => info.Length;

        public void Dispose()
        {
            NT_DisposeConnectionInfoArray(arrayPtr, arraySize);
            info = null;
        }
    }



    public class RPCValue
    {
        private readonly NT_Type type;
        private readonly object value;

        public NT_Type Type => type;
        public object Value => value;

        public RPCValue(string val)
        {
            type = NT_Type.NT_STRING;
            value = val;
        }

        public RPCValue(bool val)
        {
            type = NT_Type.NT_BOOLEAN;
            value = val;
        }

        public RPCValue(double val)
        {
            type = NT_Type.NT_DOUBLE;
            value = val;
        }

        public RPCValue(string[] val)
        {
            type = NT_Type.NT_STRING_ARRAY;
            value = val;
        }

        public RPCValue(double[] val)
        {
            type = NT_Type.NT_DOUBLE_ARRAY;
            value = val;
        }

        public RPCValue(bool[] val)
        {
            type = NT_Type.NT_BOOLEAN_ARRAY;
            value = val;
        }
    }

    public class NT_RpcDefinition
    {
        public readonly uint version;
        public readonly string name;
        public readonly NT_RpcParamDef[] paramsArray;
        public readonly NT_RpcResultDef[] resultsArray;

        public NT_RpcDefinition(uint version, string name, NT_RpcParamDef[] p, NT_RpcResultDef[] r)
        {
            this.version = version;
            this.name = name;
            this.paramsArray = p;
            this.resultsArray = r;
        }
    }


    public class NT_RpcResultDef
    {
        public readonly string name;
        public readonly NT_Type type;

        public NT_RpcResultDef(string name, NT_Type type)
        {
            this.name = name;
            this.type = type;
        }
    }

    public class NT_RpcParamDef
    {
        public readonly string name;
        public readonly RPCValue value;

        public NT_RpcParamDef(string name, RPCValue value)
        {
            this.value = value;
            this.name = name;
        }
    }

    public struct NT_RpcCallInfo
    {
        private uint rpc_id;
        private uint call_uid;
        private NT_String name;
        private NT_String param;
    }


}
