using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables
{
    public class NTValue
    {
        public NtType Type { get; }

        public object Value { get; }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static NTValue MakeDouble(double val)
        {
            return new NTValue(val);
        }

        public static NTValue MakeBoolean(bool val)
        {
            return new NTValue(val);
        }

        public static NTValue MakeString(string val)
        {
            return new NTValue(val);
        }

        public static NTValue MakeRaw(params byte[] val)
        {
            return new NTValue(val);
        }

        public static NTValue MakeBooleanArray(params bool[] val)
        {
            return new NTValue(val);
        }

        public static NTValue MakeDoubleArray(params double[] val)
        {
            return new NTValue(val);
        }

        public static NTValue MakeStringArray(params string[] val)
        {
            return new NTValue(val);
        }

        public static NTValue MakeRPC(string val)
        {
            return new NTValue(val, true);
        }

        internal static NTValue MakeEmpty()
        {
            return new NTValue();
        }

        private NTValue()
        {
            Type = NtType.Unassigned;
        }

        private NTValue(string val, bool rpc)
        {
            Type = NtType.Rpc;
            Value = val;
        }

        private NTValue(string val)
        {
            Type = NtType.String;
            Value = val;
        }

        private NTValue(byte[] val)
        {
            Type = NtType.Raw;
            Value = val;
        }

        private NTValue(bool val)
        {
            Type = NtType.Boolean;
            Value = val;
        }

        private NTValue(double val)
        {
            Type = NtType.Double;
            Value = val;
        }

        private NTValue(string[] val)
        {
            Type = NtType.StringArray;
            Value = val;
        }

        private NTValue(double[] val)
        {
            Type = NtType.DoubleArray;
            Value = val;
        }

        private NTValue(bool[] val)
        {
            Type = NtType.BooleanArray;
            Value = val;
        }
    }

    /// <summary>
    /// An enumeration of all types allowed in the NetworkTables.
    /// </summary>
    [Flags]
    public enum NtType
    {
        /// <summary>
        /// No type assigned
        /// </summary>
        Unassigned = 0,
        /// <summary>
        /// Boolean type
        /// </summary>
        Boolean = 0x01,
        /// <summary>
        /// Double type
        /// </summary>
        Double = 0x02,
        /// <summary>
        /// String type
        /// </summary>
        String = 0x04,
        /// <summary>
        /// Raw type
        /// </summary>
        Raw = 0x08,
        /// <summary>
        /// Boolean Array type
        /// </summary>
        BooleanArray = 0x10,
        /// <summary>
        /// Double Array type
        /// </summary>
        DoubleArray = 0x20,
        /// <summary>
        /// String Array type
        /// </summary>
        StringArray = 0x40,
        /// <summary>
        /// Rpc type
        /// </summary>
        Rpc = 0x80
    }
}
