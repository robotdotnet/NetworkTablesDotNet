using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static NetworkTables.Logger;

namespace NetworkTables.TcpSockets
{
    internal class TCPAcceptor : INetworkAcceptor
    {
        private TcpListener m_server;
        private int m_port;
        private string m_address;
        private bool m_shutdown = false;

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
            try
            {
                m_server.Start();
            }
            catch (SocketException ex)
            {
                Error($"Socket Start(): failed {ex.SocketErrorCode}");
                return ex.NativeErrorCode;
            }
            

            return 0;
        }

        public void Shutdown()
        {
            Console.WriteLine("Stopping");
            m_shutdown = true;
            m_server?.Stop();
            m_server = null;
        }

        public INetworkStream Accept()
        {
            if (m_server == null)
            {
                if (!m_shutdown)
                {
                    Error("Accept() failed because of null TcpListener");
                }
                return null;
            }
            try
            {
                Socket sd = m_server.AcceptSocket();
                if (m_shutdown)
                {
                    sd.Close();
                    return null;
                }
                TCPStream stream = new TCPStream(sd);
                return stream;
            }
            catch (SocketException ex)
            {
                if (!m_shutdown)
                    Error($"Accept() failed: {ex.SocketErrorCode.ToString()}");
                return null;
            }
        }
    }
}
