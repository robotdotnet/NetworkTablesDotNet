using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTablesDotNet.NetworkTables2.Type
{
    class NumberArray : ArrayData
    {
        private static readonly byte NUMBER_ARRAY_RAW_ID = 0x11;
        public static readonly ArrayEntryType TYPE = new ArrayEntryType(NUMBER_ARRAY_RAW_ID, DefaultEntryTypes.DOUBLE);

        public NumberArray() : base(TYPE)
        {
            
        }

        public double Get(int index)
        {
            return (double) GetAsObject(index);
        }

        public void Set(int index, double value)
        {
            _Set(index, value);
        }

        public void Add(double value)
        {
            _Add(value);
        }
    }
}
