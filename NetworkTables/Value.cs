using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables
{
    public class Value
    {
        public NtType Type { get; }

        public object Val { get; }

        public ulong LastChange { get; }

        public override string ToString()
        {
            return Val.ToString();
        }

        public static Value MakeDouble(double val)
        {
            return new Value(val);
        }

        public static bool operator==(Value lhs, Value rhs)
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

        public static bool operator!=(Value lhs, Value rhs)
        {
            return !(lhs == rhs);
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
            return new Value(val);
        }

        public static Value MakeBooleanArray(params bool[] val)
        {
            return new Value(val);
        }

        public static Value MakeDoubleArray(params double[] val)
        {
            return new Value(val);
        }

        public static Value MakeStringArray(params string[] val)
        {
            return new Value(val);
        }

        public static Value MakeRPC(string val)
        {
            return new Value(val, true);
        }

        internal static Value MakeEmpty()
        {
            return new Value();
        }

        private Value()
        {
            Type = NtType.Unassigned;
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
