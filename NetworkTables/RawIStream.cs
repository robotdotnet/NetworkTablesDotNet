using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables
{
    public interface IRawIStream
    {
        bool Read(object data, int len);
        void Close();


    }

    public class RawMemIStream : IRawIStream
    {
        private byte[] m_cur;
        private int m_left;

        public RawMemIStream(byte[] mem, int len)
        {
            m_cur = mem;
            m_left = len;
        }

        public virtual bool Read(object data, int len)
        {
            if (len > m_left) return false;
            return true;
        }

        public virtual void Close()
        {
            
        }
    }
}
