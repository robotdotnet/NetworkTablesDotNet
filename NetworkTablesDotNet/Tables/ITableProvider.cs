using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkTablesDotNet.Tables
{
    public interface ITableProvider
    {
        ITable GetTable(string name);
    }
}
