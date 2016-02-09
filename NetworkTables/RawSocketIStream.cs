using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkTables.TcpSockets;

namespace NetworkTables
{
    public class RawSocketIStream : IRawIStream
    {
        private INetworkStream m_stream;
        private int m_timeout;

        public RawSocketIStream(INetworkStream stream, int timeout = 0)
        {
            
        }

        public virtual bool Read(object data, int len)
        {
            bool[] cdata = (bool[]) data;

        }

        public virtual void Close()
        {
            m_stream.Close();
        }
    }
}
