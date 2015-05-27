﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

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

            public override void SendValue(object value, BinaryWriter os)
            {
                if (value is bool)
                    os.Write((bool) value);
                else
                {
                    throw new IOException("Cannot write " + value + " as " + name);
                }
            }

            public override object ReadValue(BinaryReader reader)
            {
                return reader.ReadBoolean();
            }
        }

        internal class DoubleEntryType : NetworkTableEntryType
        {
            public DoubleEntryType(byte id, string name) : base(id, name)
            {
                
            }

            public override void SendValue(object value, BinaryWriter os)
            {
                if (value is double)
                    os.Write((double) value);
                else
                {
                    throw new IOException("Cannot write " + value + " as " + name);
                }
            }

            public override object ReadValue(BinaryReader reader)
            {
                return reader.ReadDouble();
            }
        }

        internal class StringEntryType : NetworkTableEntryType
        {
            public StringEntryType(byte id, string name)
                : base(id, name)
            {

            }

            public override void SendValue(object value, BinaryWriter os)
            {
                if (value is string)
                    os.Write((string)value);
                else
                {
                    throw new IOException("Cannot write " + value + " as " + name);
                }
            }

            public override object ReadValue(BinaryReader reader)
            {
                return reader.ReadString();
            }
        }
    }
}
