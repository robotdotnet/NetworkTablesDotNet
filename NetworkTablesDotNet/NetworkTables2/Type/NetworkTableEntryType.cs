﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NetworkTablesDotNet.NetworkTables2.Stream;
using NetworkTablesDotNet.NetworkTables2.Connection;

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

        public abstract void SendValue(object value, DataIOStream os);
        public abstract object ReadValue(DataIOStream inStream);
    }
}
