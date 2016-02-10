using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables
{
    public class NtTypeMismatchException : InvalidOperationException
    {
        public NtTypeMismatchException(NtType requested, NtType actual)
            : base($"Requested Type {requested} does not match actual Type {actual}.")
        {

        }
    }

    public class Value
    {
        public Value()
        {
            Type = NtType.Unassigned;
        }

        public NtType Type { get; }

        public object Val { get; }

        public ulong LastChange { get; }

        public bool IsBoolean() => Type == NtType.Boolean;
        public bool IsDouble() => Type == NtType.Double;
        public bool IsString() => Type == NtType.String;

        public bool IsRaw() => Type == NtType.Raw;

        public bool IsRpc() => Type == NtType.Rpc;

        public bool IsBooleanArray() => Type == NtType.BooleanArray;

        public bool IsDoubleArray() => Type == NtType.DoubleArray;

        public bool IsStringArray() => Type == NtType.StringArray;

        public bool GetBoolean()
        {
            if (Type != NtType.Boolean)
            {
                throw new NtTypeMismatchException(NtType.Boolean, Type);
            }
            return (bool)Val;
        }

        public double GetDouble()
        {
            if (Type != NtType.Double)
            {
                throw new NtTypeMismatchException(NtType.Double, Type);
            }
            return (double)Val;
        }

        public string GetString()
        {
            if (Type != NtType.String)
            {
                throw new NtTypeMismatchException(NtType.String, Type);
            }
            return (string)Val;
        }

        public byte[] GetRaw()
        {
            if (Type != NtType.Raw)
            {
                throw new NtTypeMismatchException(NtType.Raw, Type);
            }

            return (byte[])Val;
        }

        public byte[] GetRpc()
        {
            if (Type != NtType.Rpc)
            {
                throw new NtTypeMismatchException(NtType.Rpc, Type);
            }
            return (byte[])Val;
        }

        public bool[] GetBooleanArray()
        {
            if (Type != NtType.BooleanArray)
            {
                throw new NtTypeMismatchException(NtType.BooleanArray, Type);
            }
            return (bool[])Val;
        }

        public double[] GetDoubleArray()
        {
            if (Type != NtType.DoubleArray)
            {
                throw new NtTypeMismatchException(NtType.DoubleArray, Type);
            }
            return (double[])Val;
        }

        public string[] GetStringArray()
        {
            if (Type != NtType.StringArray)
            {
                throw new NtTypeMismatchException(NtType.StringArray, Type);
            }
            return (string[])Val;
        }

        public override string ToString()
        {
            return Val.ToString();
        }



        public static bool operator ==(Value lhs, Value rhs)
        {
            if (lhs.Type != rhs.Type) return false;
            switch (lhs.Type)
            {
                case NtType.Unassigned:
                    return true;
                case NtType.Boolean:
                    return (bool)lhs.Val == (bool)rhs.Val;
                default:
                    return false;

            }

        }

        public static bool operator !=(Value lhs, Value rhs)
        {
            return !(lhs == rhs);
        }

        public static Value MakeDouble(double val)
        {
            return new Value(val);
        }

        public static Value MakeBoolean(bool val)
        {
            return new Value(val);
        }

        public static Value MakeString(string val)
        {
            return new Value(val);
        }

        public static Value MakeRaw(params byte[] val)
        {
            byte[] tmp = new byte[val.Length];
            Array.Copy(val, tmp, val.Length);
            return new Value(tmp);
        }

        public static Value MakeRpc(params byte[] val)
        {
            byte[] tmp = new byte[val.Length];
            Array.Copy(val, tmp, val.Length);
            return new Value(tmp, true);
        }

        public static Value MakeBooleanArray(params bool[] val)
        {
            bool[] tmp = new bool[val.Length];
            Array.Copy(val, tmp, val.Length);
            return new Value(tmp);
        }

        public static Value MakeDoubleArray(params double[] val)
        {
            double[] tmp = new double[val.Length];
            Array.Copy(val, tmp, val.Length);
            return new Value(tmp);
        }

        public static Value MakeStringArray(params string[] val)
        {
            string[] tmp = new string[val.Length];
            Array.Copy(val, tmp, val.Length);
            return new Value(tmp);
        }

        public static Value MakeRPC(string val)
        {
            return new Value(val, true);
        }

        internal static Value MakeEmpty()
        {
            return new Value();
        }

        private Value(string val, bool rpc)
        {
            Type = NtType.Rpc;
            Val = val;
        }

        private Value(string val)
        {
            Type = NtType.String;
            Val = val;
        }

        private Value(byte[] val)
        {
            Type = NtType.Raw;
            Val = val;
        }

        private Value(byte[] val, bool rpc)
        {
            Type = NtType.Rpc;
            Val = val;
        }

        private Value(bool val)
        {
            Type = NtType.Boolean;
            Val = val;
        }

        private Value(double val)
        {
            Type = NtType.Double;
            Val = val;
        }

        private Value(string[] val)
        {
            Type = NtType.StringArray;
            Val = val;
        }

        private Value(double[] val)
        {
            Type = NtType.DoubleArray;
            Val = val;
        }

        private Value(bool[] val)
        {
            Type = NtType.BooleanArray;
            Val = val;
        }
    }
}
