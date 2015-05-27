using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkTablesDotNet.NetworkTables2.Type
{
    public class NetworkTableEntryTypeManager
    {
        private Dictionary<byte, NetworkTableEntryType> typeMap = new Dictionary<byte, NetworkTableEntryType>();

        public NetworkTableEntryType GetType(byte id)
        {
            return typeMap[id];
        }

        internal void RegisterType(NetworkTableEntryType type)
        {
            typeMap.Add(type.id, type);
        }

        public NetworkTableEntryTypeManager()
        {
            DefaultEntryTypes.RegisterTypes(this);
        }
    }
}
