using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkTables.NetworkTables2;
using NetworkTables.NetworkTables2.Type;

namespace NetworkTables.Test.Util
{
    public class NetworkTableEntryUtil
    {
        public static NetworkTableEntry NewBooleanEntry(string name, bool value)
        {
            return new NetworkTableEntry(name, DefaultEntryTypes.BOOLEAN, value);
        }

        public static NetworkTableEntry NewBooleanEntry(char id, string name, char sequenceNumber, bool value)
        {
            return new NetworkTableEntry(id, name, sequenceNumber, DefaultEntryTypes.BOOLEAN, value);
        }

        public static NetworkTableEntry NewDoubleEntry(string name, double value)
        {
            return new NetworkTableEntry(name, DefaultEntryTypes.DOUBLE, value);
        }
        public static NetworkTableEntry NewDoubleEntry(char id, string name, char sequenceNumber, double value)
        {
            return new NetworkTableEntry(id, name, sequenceNumber, DefaultEntryTypes.DOUBLE, value);
        }

        public static NetworkTableEntry NewStringEntry(string name, string value)
        {
            return new NetworkTableEntry(name, DefaultEntryTypes.STRING, value);
        }
        public static NetworkTableEntry NewStringEntry(char id, string name, char sequenceNumber, string value)
        {
            return new NetworkTableEntry(id, name, sequenceNumber, DefaultEntryTypes.STRING, value);
        }
    }
}
