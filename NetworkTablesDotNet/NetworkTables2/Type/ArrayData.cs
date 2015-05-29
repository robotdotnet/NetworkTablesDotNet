using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkTablesDotNet.NetworkTables2.Type
{
    public class ArrayData : ComplexData
    {
        private readonly ArrayEntryType m_type;
        private object[] m_data = new object[0];

        public ArrayData(ArrayEntryType type) : base(type)
        {
            this.m_type = type;
        }

        protected object GetAsObject(int index)
        {
            return m_data[index];
        }

        protected void _Set(int index, object value)
        {
            m_data[index] = value;
        }

        protected void _Add(object value)
        {
            SetSize(Size() + 1);
            m_data[Size() - 1] = value;
        }

        public void Remove(int index)
        {
            if (index < 0 || index >= Size())
                throw new IndexOutOfRangeException();
            if (index < Size() - 1)
                Array.Copy(m_data, index + 1, m_data, index, Size() - index - 1);
            SetSize(Size() - 1);
        }

        public void SetSize(int size)
        {
            if (size == m_data.Length)
                return;
            object[] newArray = new object[size];//TODO cache arrays
            if (size < m_data.Length)
                Array.Copy(m_data, 0, newArray, 0, size);
            else
            {
                Array.Copy(m_data, 0, newArray, 0, m_data.Length);
                for (int i = m_data.Length; i < newArray.Length; ++i)
                    newArray[i] = null;
            }
            m_data = newArray;
        }

        public int Size()
        {
            return m_data.Length;
        }

        internal object[] GetDataArray()
        {
            return m_data;
        }
    }
}
