using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables.NetworkTables2.Util
{
    class ByteArrayMap : ResizeableArrayObject
    {
        public void Put(byte key, object value)
        {
            int offsetKey = key + 128;
            EnsureSize(offsetKey + 1);
            array[offsetKey] = value;
        }

        public object Get(byte key)
        {
            int offsetKey = key + 128;
            if (offsetKey >= array.Length)
            {
                return null;
            }
            return array[offsetKey];
        }

        public void Clear()
        {
            for (int i = 0; i < array.Length; ++i)
            {
                array[i] = null;
            }
        }

        public void Remove(char key)
        {
            int offsetKey = key + 128;
            if (offsetKey >= array.Length)
            {
                return;
            }
            array[offsetKey] = null;
        }
    }
}
