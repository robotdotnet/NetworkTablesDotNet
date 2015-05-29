using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTablesDotNet.NetworkTables2.Util
{
    public class List :ResizeableArrayObject
    {
        protected int size = 0;

        public List()
        {
            
        }

        public List(int initialSize) : base(initialSize)
        {
        }

        public bool IsEmpty()
        {
            return size == 0;
        }

        public void Add(object o)
        {
            EnsureSize(size + 1);
            array[size++] = o;
        }

        public void Remove(int index)
        {
            if (index < 0 || index >= size)
                throw new IndexOutOfRangeException();
            if (index < size - 1)
                Array.Copy(array, index + 1, array, index, size - index - 1);
            size--;
        }

        public void Clear()
        {
            size = 0;
        }

        public object Get(int index)
        {
            if (index < 0 || index >= size)
                throw new IndexOutOfRangeException();
            return array[index];
        }

        public bool Remove(object obj)
        {
            for (int i = 0; i < size; ++i)
            {
                object value = array[i];
                if (obj == null ? value == null : obj.Equals(value))
                {
                    Remove(i);
                    return true;
                }
            }
            return false;
        }

        public bool Contains(object obj)
        {
            for (int i = 0; i < size; ++i)
            {
                object value = array[i];
                if (obj == null ? value == null : obj.Equals(value))
                {
                    return true;
                }
            }
            return false;
        }

        public void Set(int index, object obj)
        {
            if (index < 0 || index >= size)
                throw new IndexOutOfRangeException();
            array[index] = obj;
        }

    }
}
