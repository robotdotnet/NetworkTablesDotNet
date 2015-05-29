using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTablesDotNet.NetworkTables2.Type
{
    class StringArray : ArrayData
    {
        private static readonly byte NUMBER_ARRAY_RAW_ID = 0x11;
        public static readonly ArrayEntryType TYPE = new ArrayEntryType(NUMBER_ARRAY_RAW_ID, DefaultEntryTypes.DOUBLE);

        public StringArray()
            : base(TYPE)
        {

        }

        public string Get(int index)
        {
            return (string)GetAsObject(index);
        }

        public void Set(int index, string value)
        {
            _Set(index, value);
        }

        public void Add(string value)
        {
            _Add(value);
        }
    }
}
