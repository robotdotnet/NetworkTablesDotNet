using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTablesDotNet.NetworkTables2.Type
{
    public class BooleanArray : ArrayData
    {
        private static readonly byte BOOLEAN_ARRAY_RAW_ID = 0x10;

        public static readonly ArrayEntryType TYPE = new ArrayEntryType(BOOLEAN_ARRAY_RAW_ID, DefaultEntryTypes.BOOLEAN);

        public BooleanArray() : base(TYPE)
        {
            
        }

        public bool Get(int index)
        {
            return (bool) GetAsObject(index);
        }

        public void Set(int index, bool value)
        {
            _Set(index, value);
        }

        public void Add(bool value)
        {
            _Add(value);
        }
    }
}
