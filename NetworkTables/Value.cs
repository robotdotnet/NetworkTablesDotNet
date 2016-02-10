using System;
using NetworkTables.Native.Exceptions;

namespace NetworkTables
{


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
            if (ReferenceEquals(lhs, rhs)) return true;
            if (((object)lhs == null) || ((object)rhs == null)) return false;
            if (lhs.Type != rhs.Type) return false;
            switch (lhs.Type)
            {
                case NtType.Unassigned:
                    return true;
                case NtType.Boolean:
                    return (bool)lhs.Val == (bool)rhs.Val;
                case NtType.Double:
                    return (double)lhs.Val == (double)rhs.Val;
                case NtType.String:
                    return (string)lhs.Val == (string)rhs.Val;
                case NtType.Raw:
                case NtType.Rpc:
                    byte[] rawLhs = (byte[])lhs.Val;
                    byte[] rawRhs = (byte[])rhs.Val;
                    if (rawLhs.Length != rawRhs.Length) return false;
                    for (int i = 0; i < rawLhs.Length; i++)
                    {
                        if (rawLhs[i] != rawRhs[i]) return false;
                    }
                    return true;
                case NtType.BooleanArray:
                    bool[] boolLhs = (bool[])lhs.Val;
                    bool[] boolRhs = (bool[])rhs.Val;
                    if (boolLhs.Length != boolRhs.Length) return false;
                    for (int i = 0; i < boolLhs.Length; i++)
                    {
                        if (boolLhs[i] != boolRhs[i]) return false;
                    }
                    return true;
                case NtType.DoubleArray:
                    double[] doubleLhs = (double[])lhs.Val;
                    double[] doubleRhs = (double[])rhs.Val;
                    if (doubleLhs.Length != doubleRhs.Length) return false;
                    for (int i = 0; i < doubleLhs.Length; i++)
                    {
                        if (doubleLhs[i] != doubleRhs[i]) return false;
                    }
                    return true;
                case NtType.StringArray:
                    string[] stringLhs = (string[])lhs.Val;
                    string[] stringRhs = (string[])rhs.Val;
                    if (stringLhs.Length != stringRhs.Length) return false;
                    for (int i = 0; i < stringLhs.Length; i++)
                    {
                        if (stringLhs[i] != stringRhs[i]) return false;
                    }
                    return true;
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

        public static Value MakeRpc(byte[] val, int size)
        {
            if (size > val.Length) return null;
            byte[] tmp = new byte[size];
            Array.Copy(val, tmp, size);
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
