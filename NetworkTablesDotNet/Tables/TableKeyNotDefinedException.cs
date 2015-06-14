using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkTablesDotNet.NetworkTables;

namespace NetworkTablesDotNet.Tables
{
    public class TableKeyNotDefinedException : NetworkTableKeyNotDefined
    {
        public TableKeyNotDefinedException(string key) : base(key)
        {
            
        }
    }
}
