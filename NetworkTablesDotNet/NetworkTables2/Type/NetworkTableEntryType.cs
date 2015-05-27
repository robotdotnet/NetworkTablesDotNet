using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NetworkTablesDotNet.NetworkTables2.Type
{
    public abstract class NetworkTableEntryType
    {
        public byte id;
        public string name;

        protected NetworkTableEntryType(byte id, string name)
        {
            this.id = id;
            this.name = name;
        }

        public new string ToString()
        {
            return "NetworkTable type: " + name;
        }

        public abstract void SendValue(object value, BinaryWriter os);
        public abstract object ReadValue(BinaryReader inStream);
    }
}
