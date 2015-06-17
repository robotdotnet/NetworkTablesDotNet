using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkTables.NetworkTables2.Util
{
    public class ResizeableArrayObject
    {
        private static int GROW_FACTOR = 3;
        protected object[] array;

        protected ResizeableArrayObject() : this(10)
        {
            
        }

        protected ResizeableArrayObject(int initialSize)
        {
            array = new object[initialSize];
        }

        protected int ArraySize()
        {
            return array.Length;
        }

        protected void EnsureSize(int size)
        {
            if (size > array.Length)
            {
                int newSize = array.Length;
                while (size > newSize)
                {
                    newSize *= GROW_FACTOR;
                }

                object[] newArray = new object[newSize];
                Array.Copy(array, 0, newArray, 0, array.Length);
                array = newArray;
            }
        }
    }
}
