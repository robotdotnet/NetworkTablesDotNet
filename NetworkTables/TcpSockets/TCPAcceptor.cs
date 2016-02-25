using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using static NetworkTables.Logger;

namespace NetworkTables.TcpSockets
{
    internal class TCPAcceptor : INetworkAcceptor, IDisposable
    {
        private Socket m_server;
        private IPEndPoint m_ipEp;
        private int m_port;
        private string m_address;
        private bool m_listening = false;
        private bool m_shutdown = false;

        public TCPAcceptor(int port, string address)
        {
            m_port = port;
            m_address = address;
        }

        public void Dispose()
        {
            if (m_server != null)
            {
                Shutdown();
                m_server.Dispose();
            }
        }

        public int Start()
        {
            if (m_listening) return 0;

            try
            {
                m_server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
            catch (SocketException)
            {
                Error("could not create socket");
                return -1;
            }
            
            IPAddress address = null;
            if (!string.IsNullOrEmpty(m_address))
            {
                address = IPAddress.Parse(m_address);
            }
            else
            {
                address = IPAddress.Any;
            }

            m_server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);

            m_ipEp = new IPEndPoint(address, m_port);

            try
            {
                m_server.Bind(m_ipEp);
            }
            catch (SocketException ex)
            {
                Error($"Bind() failed: {ex.SocketErrorCode.ToString()}");
                return ex.NativeErrorCode;
            }

            try
            {
                m_server.Listen(5);
            }
            catch (SocketException ex)
            {
                Error($"Listen() failed: {ex.SocketErrorCode.ToString()}");
                return ex.NativeErrorCode;
            }
            m_listening = true;
            return 0;
        }

        public void Shutdown()
        {
            m_shutdown = true;

            //Force wakeup with non-blocking connect to ourselves
            IPAddress address = null;
            if (!string.IsNullOrEmpty(m_address))
            {
                address = IPAddress.Parse(m_address);
            }
            else
            {
                address = IPAddress.Loopback;
            }

            Socket connectSocket;
            try
            {
                connectSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            }
            catch (SocketException)
            {
                return;
            }

            connectSocket.Blocking = false;

            try
            {
                connectSocket.Connect(address, m_port);
                connectSocket.Dispose();
            }
            catch (SocketException)
            {
            }

            m_listening = false;
            m_server?.Dispose();
            m_server = null;
        }

        public INetworkStream Accept()
        {
            if (!m_listening || m_shutdown) return null;

            Socket socket;
            try
            {
                socket = m_server.Accept();
            }
            catch (SocketException ex)
            {
                if (!m_shutdown) Error($"Accept() failed: {ex.SocketErrorCode}");
                return null;
            }

            if (m_shutdown)
            {
                socket.Dispose();
                return null;
            }

            return new TCPStream(socket);
        }
    }
}
