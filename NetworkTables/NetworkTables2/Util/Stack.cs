using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables.NetworkTables2.Util
{
    public class Stack : List
    {
        public void Push(object element)
        {
            Add(element);
        }

        public object Pop()
        {
            if (size == 0)
                return null;
            object value = Get(size - 1);
            Remove(size - 1);
            return value;
        }
    }
}
