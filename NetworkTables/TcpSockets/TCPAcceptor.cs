using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables.TcpSockets
{
    internal class TCPAcceptor : INetworkAcceptor
    {
        private TcpListener m_server;
        private int m_port;
        private string m_address;

        public TCPAcceptor(int port, string address)
        {
            m_port = port;
            m_address = address;
        }

        public int Start()
        {
            IPAddress address = null;
            if (!string.IsNullOrEmpty(m_address))
            {
                address = IPAddress.Parse(m_address);
            }
            else
            {
                address = IPAddress.Any;
            }
            m_server = new TcpListener(address, m_port);
            m_server.Start();

            return 0;
        }

        public void Shutdown()
        {
            m_server?.Stop();
        }

        public INetworkStream Accept()
        {
            Socket sd = m_server.AcceptSocket();
            TCPStream stream = new TCPStream(sd);
            return stream;
        }
    }
}
