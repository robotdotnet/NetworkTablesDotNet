using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkTables.NetworkTables;

namespace NetworkTables.Tables
{
    public class TableKeyNotDefinedException : KeyNotFoundException
    {
        public TableKeyNotDefinedException(string key) : base("Unknown Table Key: " + key)
        {
            
        }
    }
}
