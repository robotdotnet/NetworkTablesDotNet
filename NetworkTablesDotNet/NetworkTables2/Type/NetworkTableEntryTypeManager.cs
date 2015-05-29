﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkTablesDotNet.NetworkTables2.Util;

namespace NetworkTablesDotNet.NetworkTables2.Type
{
    public class NetworkTableEntryTypeManager
    {
        private readonly ByteArrayMap typeMap = new ByteArrayMap();

        public NetworkTableEntryType GetType(byte id)
        {
            return (NetworkTableEntryType) typeMap.Get(id);
        }

        internal void RegisterType(NetworkTableEntryType type)
        {
            typeMap.Put(type.id, type);
        }

        public NetworkTableEntryTypeManager()
        {
            DefaultEntryTypes.RegisterTypes(this);
        }
    }
}
