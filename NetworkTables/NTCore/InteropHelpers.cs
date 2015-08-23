using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NetworkTables.Tables;
using static NetworkTables.NTCore.Interop;

namespace NetworkTables.NTCore
{
    internal class InteropHelpers
    {
        public static void SetEntryFlags(string name, uint flags)
        {
            UIntPtr size;
            byte[] str = CreateUTF8String(name, out size);
            NT_SetEntryFlags(str, size, flags);
        }

        public static uint GetEntryFlags(string name)
        {
            UIntPtr size;
            byte[] str = CreateUTF8String(name, out size);
            uint flags = NT_GetEntryFlags(str, size);
            return flags;
        }

        public static void DeleteEntry(string name)
        {
            UIntPtr size;
            byte[] str = CreateUTF8String(name, out size);
            NT_DeleteEntry(str, size);
        }

        public static NT_Type GetType(string name)
        {
            UIntPtr size;
            byte[] str = CreateUTF8String(name, out size);
            NT_Type retVal = NT_GetType(str, size);
            return retVal;
        }

        public static void StartClient(string serverName, uint port)
        {
            if (serverName == null)
            {
                throw new ArgumentNullException(nameof(serverName), "Server cannot be null");
            }
            UIntPtr size;
            byte[] serverNamePtr = CreateUTF8String(serverName, out size);
            NT_StartClient(serverNamePtr, port);
        }

        public static void StartServer(string fileName, string listenAddress, uint port)
        {
            UIntPtr size = UIntPtr.Zero;
            byte[] fileNamePtr = CreateUTF8String("networktables.ini", out size);
            byte[] listenAddressPtr = CreateUTF8String("", out size);
            NT_StartServer(fileNamePtr, listenAddressPtr, port);
        }

        public static EntryInfoArray GetEntryInfo(string prefix, uint types)
        {
            UIntPtr size;
            byte[] str = CreateUTF8String(prefix, out size);
            UIntPtr arrSize = UIntPtr.Zero;
            IntPtr arr = NT_GetEntryInfo(str, size, types, ref arrSize);

            int entryInfoSize = Marshal.SizeOf(typeof(NT_EntryInfo));
            int arraySize = (int)arrSize.ToUInt64();
            NT_EntryInfo[] entryArray = new NT_EntryInfo[arraySize];

            for (int i = 0; i < arraySize; i++)
            {
                IntPtr data = new IntPtr(arr.ToInt64() + entryInfoSize * i);
                entryArray[i] = (NT_EntryInfo)Marshal.PtrToStructure(data, typeof(NT_EntryInfo));
            }
            return new EntryInfoArray(entryArray, arr, arrSize);
        }

        public static ConnectionInfoArray GetConnectionInfo()
        {
            UIntPtr count = UIntPtr.Zero;
            IntPtr connections = NT_GetConnections(ref count);

            int connectionInfoSize = Marshal.SizeOf(typeof(NT_ConnectionInfo));
            int arraySize = (int)count.ToUInt64();

            NT_ConnectionInfo[] connectionArray = new NT_ConnectionInfo[arraySize];

            for (int i = 0; i < arraySize; i++)
            {
                IntPtr data = new IntPtr(connections.ToInt64() + connectionInfoSize * i);
                connectionArray[i] = (NT_ConnectionInfo)Marshal.PtrToStructure(data, typeof(NT_ConnectionInfo));
            }
            return new ConnectionInfoArray(connectionArray, connections, count);
        }

        public static uint AddEntryListener(string prefix, ITable table, Action<ITable, string, object, bool> callback,
            bool immediate_notify)
        {
            NT_EntryListenerCallback modCallback = (uid, ptr, name, len, value, is_new) =>
            {
                string key = ReadUTF8String(name, len);
                key = key.Replace(table.Path + NetworkTable.PATH_SEPERATOR_CHAR, "");
                NT_Type type = NT_GetTypeFromValue(value);
                object obj;
                ulong lastChange = 0;
                UIntPtr size = UIntPtr.Zero;
                switch (type)
                {
                    case NT_Type.NT_UNASSIGNED:
                        obj = null;
                        break;
                    case NT_Type.NT_BOOLEAN:
                        int boolean = 0;
                        NT_GetEntryBooleanFromValue(value, ref lastChange, ref boolean);
                        obj = boolean != 0;
                        break;
                    case NT_Type.NT_DOUBLE:
                        double val = 0;
                        NT_GetEntryDoubleFromValue(value, ref lastChange, ref val);
                        obj = val;
                        break;
                    case NT_Type.NT_STRING:
                        NT_String str = new NT_String();
                        NT_GetEntryStringFromValue(value, ref lastChange, ref str);
                        obj = str;
                        break;
                    case NT_Type.NT_RAW:
                        NT_String strr = new NT_String();
                        NT_GetEntryRawFromValue(value, ref lastChange, ref strr);
                        obj = strr;
                        break;
                    case NT_Type.NT_BOOLEAN_ARRAY:
                        IntPtr boolArr = NT_GetEntryBooleanArrayFromValue(value, ref lastChange, ref size);
                        obj = GetBooleanArrayFromPtr(boolArr, size);
                        break;
                    case NT_Type.NT_DOUBLE_ARRAY:
                        IntPtr doubleArr = NT_GetEntryDoubleArrFromValue(value, ref lastChange, ref size);
                        obj = GetDoubleArrayFromPtr(doubleArr, size);
                        break;
                    case NT_Type.NT_STRING_ARRAY:
                        IntPtr stringArr = NT_GetEntryStringArrayFromValue(value, ref lastChange, ref size);
                        obj = GetStringArrayFromPtr(stringArr, size);
                        break;
                    case NT_Type.NT_RPC:
                        obj = null;
                        break;
                    default:
                        obj = null;
                        break;
                }
                callback(table, key, obj, is_new != 0);
            };

            UIntPtr prefixSize;
            byte[] prefixStr = CreateUTF8String(prefix, out prefixSize);
            uint retVal = NT_AddEntryListener(prefixStr, prefixSize, IntPtr.Zero, modCallback, immediate_notify ? 1 : 0);
            return retVal;
        }

        public static bool GetEntryBoolean(string name, ref ulong last_change, ref int status)
        {
            UIntPtr size;
            byte[] namePtr = CreateUTF8String(name, out size);
            int boolean = 0;
            status = NT_GetEntryBoolean(namePtr, size, ref last_change, ref boolean);
            return boolean != 0;
        }

        public static double GetEntryDouble(string name, ref ulong last_change, ref int status)
        {
            UIntPtr size;
            byte[] namePtr = CreateUTF8String(name, out size);
            double retVal = 0;
            status = NT_GetEntryDouble(namePtr, size, ref last_change, ref retVal);
            return retVal;
        }

        public static string GetEntryString(string name, ref ulong last_change)
        {
            UIntPtr size;
            byte[] namePtr = CreateUTF8String(name, out size);
            NT_String ntstr = new NT_String();
            int ret = NT_GetEntryString(namePtr, size, ref last_change, ref ntstr);
            if (ret == 0)
            {
                return null;
            }
            else
            {
                string str = ntstr.ToString();
                ntstr.Dispose();
                return str;
            }
        }

        public static string GetEntryRaw(string name, ref ulong last_change)
        {
            UIntPtr size;
            byte[] namePtr = CreateUTF8String(name, out size);
            NT_String ntstr = new NT_String();
            int ret = NT_GetEntryRaw(namePtr, size, ref last_change, ref ntstr);
            if (ret == 0)
            {
                return null;
            }
            else
            {
                string str = ntstr.ToString();
                ntstr.Dispose();
                return str;
            }
        }

        public static double[] GetEntryDoubleArray(string name, ref ulong last_change)
        {
            UIntPtr size;
            byte[] namePtr = CreateUTF8String(name, out size);
            UIntPtr arrSize = UIntPtr.Zero;
            IntPtr arrPtr = NT_GetEntryDoubleArray(namePtr, size, ref last_change, ref arrSize);
            double[] arr = GetDoubleArrayFromPtr(arrPtr, arrSize);
            NT_FreeDoubleArray(arrPtr);
            return arr;
        }

        public static bool[] GetEntryBooleanArray(string name, ref ulong last_change)
        {
            UIntPtr size;
            byte[] namePtr = CreateUTF8String(name, out size);
            UIntPtr arrSize = UIntPtr.Zero;
            IntPtr arrPtr = NT_GetEntryBooleanArray(namePtr, size, ref last_change, ref arrSize);
            bool[] arr = GetBooleanArrayFromPtr(arrPtr, arrSize);
            NT_FreeBooleanArray(arrPtr);
            return arr;
        }

        public static string[] GetEntryStringArray(string name, ref ulong last_change)
        {
            UIntPtr size;
            byte[] namePtr = CreateUTF8String(name, out size);
            UIntPtr arrSize = UIntPtr.Zero;
            IntPtr arrPtr = NT_GetEntryBooleanArray(namePtr, size, ref last_change, ref arrSize);
            string[] arr = GetStringArrayFromPtr(arrPtr, arrSize);
            NT_FreeStringArray(arrPtr, arrSize);
            return arr;
        }

        public static bool SetEntryBoolean(string name, bool value, bool force = false)
        {
            UIntPtr size;
            byte[] namePtr = CreateUTF8String(name, out size);
            int retVal = NT_SetEntryBoolean(namePtr, size, value ? 1 : 0, force ? 1 : 0);
            return retVal != 0;
        }

        public static bool SetEntryDouble(string name, double value, bool force = false)
        {
            UIntPtr size;
            byte[] namePtr = CreateUTF8String(name, out size);
            int retVal = NT_SetEntryDouble(namePtr, size, value, force ? 1 : 0);
            return retVal != 0;
        }

        public static bool SetEntryString(string name, string value, bool force = false)
        {
            UIntPtr size;
            byte[] namePtr = CreateUTF8String(name, out size);
            NT_String str = new NT_String(value);
            int retVal = NT_SetEntryString(namePtr, size, str, force ? 1 : 0);
            str.Dispose();
            return retVal != 0;
        }

        public static bool SetEntryRaw(string name, string value, bool force = false)
        {
            UIntPtr size;
            byte[] namePtr = CreateUTF8String(name, out size);
            NT_String str = new NT_String(value);
            int retVal = NT_SetEntryRaw(namePtr, size, str, force ? 1 : 0);
            str.Dispose();
            return retVal != 0;
        }

        public static bool SetEntryBooleanArray(string name, bool[] value, bool force = false)
        {
            UIntPtr size;
            byte[] namePtr = CreateUTF8String(name, out size);

            UIntPtr arrSize = (UIntPtr)value.Length;
            IntPtr boolArr = NT_AllocateBooleanArray(arrSize);

            int[] valueIntArr = new int[value.Length];
            for (int i = 0; i < value.Length; i++)
            {
                valueIntArr[i] = value[i] ? 1 : 0;
            }

            Marshal.Copy(valueIntArr, 0, boolArr, value.Length);

            int retVal = NT_SetEntryBooleanArray(namePtr, size, boolArr, arrSize, force ? 1 : 0);

            NT_FreeBooleanArray(boolArr);
            return retVal != 0;
        }

        public static bool SetEntryDoubleArray(string name, double[] value, bool force = false)
        {
            UIntPtr size;
            byte[] namePtr = CreateUTF8String(name, out size);

            UIntPtr arrSize = (UIntPtr)value.Length;
            IntPtr nativeArray = NT_AllocateDoubleArray(arrSize);

            Marshal.Copy(value, 0, nativeArray, value.Length);

            int retVal = NT_SetEntryDoubleArray(namePtr, size, nativeArray, arrSize, force ? 1 : 0);

            NT_FreeDoubleArray(nativeArray);
            return retVal != 0;
        }

        public static bool SetEntryStringArray(string name, string[] value, bool force = false)
        {
            UIntPtr size;
            byte[] namePtr = CreateUTF8String(name, out size);

            UIntPtr arrSize;
            IntPtr nativeArray = StringArrayToPtr(value, out arrSize);

            int retVal = NT_SetEntryStringArray(namePtr, size, nativeArray, arrSize, force ? 1 : 0);

            NT_FreeStringArray(nativeArray, arrSize);
            return retVal != 0;
        }

        private static double[] GetDoubleArrayFromPtr(IntPtr ptr, UIntPtr size)
        {
            double[] arr = new double[size.ToUInt64()];
            Marshal.Copy(ptr, arr, 0, arr.Length);
            return arr;
        }

        private static bool[] GetBooleanArrayFromPtr(IntPtr ptr, UIntPtr size)
        {
            int[] arr = new int[size.ToUInt64()];
            Marshal.Copy(ptr, arr, 0, arr.Length);
            bool[] bArr = new bool[arr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                bArr[i] = arr[i] != 0;
            }
            return bArr;
        }

        private static string[] GetStringArrayFromPtr(IntPtr ptr, UIntPtr size)
        {

            int ntStringSize = Marshal.SizeOf(typeof(NT_String));
            int arraySize = (int)size.ToUInt64();
            string[] strArray = new string[arraySize];

            for (int i = 0; i < arraySize; i++)
            {
                IntPtr data = new IntPtr(ptr.ToInt64() + ntStringSize * i);
                strArray[i] = Marshal.PtrToStructure(data, typeof(NT_String)).ToString();
            }
            return strArray;
        }

        private static IntPtr StringArrayToPtr(string[] arr, out UIntPtr size)
        {
            size = (UIntPtr)arr.Length;
            IntPtr nativeArray = NT_AllocateNTStringArray(size);
            int ntStringSize = Marshal.SizeOf(typeof(NT_String));

            for (int i = 0; i < (int)size; i++)
            {
                NT_String str = new NT_String(arr[i]);
                IntPtr data = new IntPtr(nativeArray.ToInt64() + ntStringSize * i);
                Marshal.StructureToPtr(str, data, false);
            }
            return nativeArray;
        }

        private static byte[] CreateUTF8String(string str, out UIntPtr size)
        {
            var bytes = Encoding.UTF8.GetByteCount(str);

            var buffer = new byte[bytes + 1];
            size = (UIntPtr)bytes;
            Encoding.UTF8.GetBytes(str, 0, str.Length, buffer, 0);
            buffer[bytes] = 0;
            return buffer;
        }
        /*
        public static IntPtr CreateUTF8StringPtr(string str, out UIntPtr size)
        {
            var bytes = Encoding.UTF8.GetByteCount(str);

            var buffer = new byte[bytes + 1];

            Encoding.UTF8.GetBytes(str, 0, str.Length, buffer, 0);
            var ptr = Marshal.AllocHGlobal(bytes + 1);
            Marshal.Copy(buffer, 0, ptr, bytes);
            size = (UIntPtr)bytes;
            return ptr;
        }
        */

        //Must be null terminated
        internal static string ReadUTF8String(IntPtr str, UIntPtr size)
        {
            int iSize = (int)size.ToUInt64();
            byte[] data = new byte[iSize];
            Marshal.Copy(str, data, 0, iSize);
            return Encoding.UTF8.GetString(data);
        }

        internal static string ReadUTF8String(IntPtr ptr)
        {
            var data = new List<byte>();
            var off = 0;
            while (true)
            {
                var ch = Marshal.ReadByte(ptr, off++);
                if (ch == 0)
                {
                    break;
                }
                data.Add(ch);
            }
            return Encoding.UTF8.GetString(data.ToArray());
        }
    }
}
