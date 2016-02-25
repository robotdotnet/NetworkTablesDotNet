using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkTables.TcpSockets;

namespace NetworkTables
{
    internal class SocketInputStream : IInputStream
    {
        private INetworkStream m_stream;
        private int m_timeout;

        public SocketInputStream(INetworkStream stream, int timeout = 0)
        {
            m_stream = stream;
            m_timeout = timeout;
        }

        public virtual bool Read(byte[] data, int len)
        {
            int pos = 0;

            while (pos < len)
            {
                NetworkStreamError err = NetworkStreamError.kConnectionClosed;
                int count = m_stream.Receive(data, pos, len - pos, ref err, m_timeout);
                if (count == 0) return false;
                pos += count;
            }
            return true;

        }

        public void Dispose()
        {
            Close();
        }

        public virtual void Close()
        {
            m_stream.Close();
        }
    }
}
