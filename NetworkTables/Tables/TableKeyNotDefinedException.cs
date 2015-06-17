using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkTables.NetworkTables;

namespace NetworkTables.Tables
{
    public class TableKeyNotDefinedException : NetworkTableKeyNotDefined
    {
        public TableKeyNotDefinedException(string key) : base(key)
        {
            
        }
    }
}
