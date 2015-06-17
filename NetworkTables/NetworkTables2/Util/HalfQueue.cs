using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables.NetworkTables2.Util
{
    public class HalfQueue
    {
        public readonly object[] array;
        private int size = 0;

        public HalfQueue(int maxSize)
        {
            array = new object[maxSize];
        }

        public void Queue(object element)
        {
            array[size++] = element;
        }

        public bool IsFull()
        {
            return size == array.Length;
        }

        public int Size()
        {
            return size;
        }

        public void Clear()
        {
            size = 0;
        }
    }
}
