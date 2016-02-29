﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables
{
    internal interface IInputStream : IDisposable
    {
        bool Read(byte[] data, int len);
        void Close();


    }

    internal class ReadOnlyRawMemoryStream : IInputStream
    {
        private IReadOnlyList<byte> m_data;
        private int m_cur;
        private int m_left;

        public ReadOnlyRawMemoryStream(IReadOnlyList<byte> mem, int len)
        {
            m_data = mem;
            m_cur = 0;
            m_left = len;
        }

        public virtual bool Read(byte[] data, int len)
        {
            if (len > m_left) return false;
            //Array.Copy does not mutate state, so we can safely cast
            Array.Copy((byte[])m_data, m_cur, data, 0, len);
            m_cur += len;
            m_left -= len;
            return true;
        }

        public virtual void Close()
        {

        }

        public void Dispose()
        {
            Close();
        }
    }

    internal class RawMemoryStream : IInputStream
    {
        private byte[] m_data;
        private int m_cur;
        private int m_left;

        public RawMemoryStream(byte[] mem, int len)
        {
            m_data = mem;
            m_cur = 0;
            m_left = len;
        }

        public virtual bool Read(byte[] data, int len)
        {
            if (len > m_left) return false;
            Array.Copy(m_data, m_cur, data, 0, len);
            m_cur += len;
            m_left -= len;
            return true;
        }

        public virtual void Close()
        {
            
        }

        public void Dispose()
        {
            Close();
        }
    }
}
