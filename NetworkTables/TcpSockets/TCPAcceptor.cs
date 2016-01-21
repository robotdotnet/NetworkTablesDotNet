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
    public class TCPAcceptor : INetworkAcceptor
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

            m_server = new TcpListener(IPAddress.Parse(m_address), m_port);
            m_server.Start();

            return 1;
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
