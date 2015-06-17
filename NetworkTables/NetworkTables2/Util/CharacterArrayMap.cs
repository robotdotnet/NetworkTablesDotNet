using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkTables.NetworkTables2.Util
{
    public class CharacterArrayMap : ResizeableArrayObject
    {
        public void Put(char key, object value)
        {
            EnsureSize(key + 1);
            array[key] = value;
        }

        public object Get(char key)
        {
            if (key > array.Length)
                return null;
            return array[key];
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
            if (key > array.Length)
                return;
            array[key] = null;
        }
    }
}
