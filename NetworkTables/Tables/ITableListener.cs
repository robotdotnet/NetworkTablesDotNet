using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkTables.Tables
{
    public interface ITableListener
    {
        void ValueChanged(ITable source, string key, object value, bool isNew);
    }
}
