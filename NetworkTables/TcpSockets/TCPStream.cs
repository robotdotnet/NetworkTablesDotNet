using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables.TcpSockets
{
    public class TCPStream : INetworkStream
    {
        Socket m_socket;
        string m_peerIP;
        int m_peerPort;

        public TCPStream(Socket socket)
        {
            IPEndPoint ipEp = socket.RemoteEndPoint as IPEndPoint;
            m_peerIP = ipEp.Address.ToString();
            m_peerPort = ipEp.Port;
            m_socket = socket;

        }

        public int Send(byte[] buffer, int len, ref NetworkStreamError error)
        {
            if (m_socket == null)
            {
                error = NetworkStreamError.kConnectionClosed;
                return 0;
            }

            int rv = m_socket.Send(buffer, len, SocketFlags.None);
            //TODO: Add tons of error checking
            return rv;
        }

        public int Receive(ref byte[] buffer, int len, ref NetworkStreamError err, int timeout = 0)
        {
            if (m_socket == null)
            {
                err = NetworkStreamError.kConnectionClosed;
                return 0;
            }

            int rv;

            if (timeout <= 0)
            {
                rv = m_socket.Receive(buffer, len, 0);
            }
            else if (WaitForReadEvent(timeout))
            {
                rv = m_socket.Receive(buffer, len, 0);
            }
            else
            {
                err = NetworkStreamError.kConnectionTimedOut;
                return 0;
            }

            if (rv < 0)
            {
                err = NetworkStreamError.kConnectionReset;
                return 0;
            }
            return rv;
        }

        public void Close()
        {
            m_socket?.Shutdown(SocketShutdown.Both);
            m_socket?.Close();
            m_socket = null;
        }

        public string GetPeerIP()
        {
            return m_peerIP;
        }

        public int GetPeerPort()
        {
            return m_peerPort;
        }

        public void SetNoDelay()
        {
            m_socket.NoDelay = true;
        }

        private bool WaitForReadEvent(int timeout)
        {
            ArrayList list = new ArrayList {m_socket};
            Socket.Select(list, null, null,timeout * 1000000);
            if (list.Count == 0)
            {
                return false;
            }
            return true;
        }
    }
}
