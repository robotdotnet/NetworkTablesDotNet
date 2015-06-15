using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NetworkTablesDotNet.NetworkTables2.Stream;

namespace NetworkTablesDotNet.NetworkTables2.Type
{
    public class DefaultEntryTypes
    {
        private static byte BOOLEAN_RAW_ID = 0x00;
        private static byte DOUBLE_RAW_ID = 0x01;
        private static byte STRING_RAW_ID = 0x02;

        public static NetworkTableEntryType BOOLEAN = new BooleanEntryType(BOOLEAN_RAW_ID, "Boolean");
        public static NetworkTableEntryType DOUBLE = new DoubleEntryType(DOUBLE_RAW_ID, "Double");
        public static NetworkTableEntryType STRING = new StringEntryType(STRING_RAW_ID, "String");

        public static void RegisterTypes(NetworkTableEntryTypeManager manager)
        {
            manager.RegisterType(BOOLEAN);
            manager.RegisterType(DOUBLE);
            manager.RegisterType(STRING);
            manager.RegisterType(BooleanArray.TYPE);
            manager.RegisterType(NumberArray.TYPE);
            manager.RegisterType(StringArray.TYPE);
        }

        internal class BooleanEntryType : NetworkTableEntryType
        {
            public BooleanEntryType(byte id, string name) : base(id, name)
            {
                
            }

            public override void SendValue(object value, BinaryWriterBE os)
            {
                if (value is bool)
                    os.WriteByte((bool)value ? (byte)1 : (byte)0);
                else
                {
                    throw new IOException("Cannot write " + value + " as " + name);
                }
            }

            public override object ReadValue(BinaryReaderBE reader)
            {
                return reader.ReadByte() != 0;
            }
        }

        internal class DoubleEntryType : NetworkTableEntryType
        {
            public DoubleEntryType(byte id, string name) : base(id, name)
            {
                
            }

            public override void SendValue(object value, BinaryWriterBE os)
            {
                if (value is double)
                {
                    var tmp = BitConverter.DoubleToInt64Bits((double)value);
                    for (int i = 0; i < 8; i++)
                    {
                        os.WriteByte((byte)((tmp >> 56) & 0xFF));
                        tmp = tmp << 8;
                    }
                }

                else
                {
                    throw new IOException("Cannot write " + value + " as " + name);
                }
            }

            public override object ReadValue(BinaryReaderBE reader)
            {
                long value = 0;
                for (int i = 0; i < 8; ++i)
                {
                    value = value << 8;
                    value |= (reader.ReadByte() & 0xFF);
                }

                return BitConverter.Int64BitsToDouble(value);
            }
        }

        internal class StringEntryType : NetworkTableEntryType
        {
            public StringEntryType(byte id, string name)
                : base(id, name)
            {

            }

            public override void SendValue(object value, BinaryWriterBE os)
            {
                if (value is string)
                    os.WriteString((string)value);
                else
                {
                    throw new IOException("Cannot write " + value + " as " + name);
                }
            }

            public override object ReadValue(BinaryReaderBE reader)
            {
                return reader.ReadString();
            }
        }
    }
}
