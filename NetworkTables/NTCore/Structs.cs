using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
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

    [StructLayout(LayoutKind.Sequential)]
    public struct NT_EntryInfo
    {
        public NT_String name;
        public NT_Type type;
        public uint flags;
        public ulong last_change;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NT_ConnectionInfo
    {
        public NT_String remote_id;
        private IntPtr remote_name;
        public uint remote_port;
        public ulong last_update;
        public uint protocol_version;

        private string RemoteName()
        {
            return InteropHelpers.ReadUTF8String(remote_name);
        }

    }

   
}
