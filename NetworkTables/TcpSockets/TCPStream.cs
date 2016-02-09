using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace NetworkTables.TcpSockets
{
    public class TCPStream : INetworkStream, IDisposable
    {
        Socket m_socket;
        string m_peerIP;
        int m_peerPort;

        public TCPStream(Socket socket)
        {
            m_socket = socket;

            IPEndPoint ipEp = socket.RemoteEndPoint as IPEndPoint;
            m_peerIP = ipEp.Address.ToString();
            m_peerPort = ipEp.Port;
        }

        public void Dispose()
        {
            Close();
        }

        public int Send(byte[] buffer, int pos, int len, ref NetworkStreamError err)
        {
            if (m_socket == null)
            {
                err = NetworkStreamError.kConnectionClosed;
                return 0;
            }
            int rv = 0;
            int errorCode = 0;
            while (true)
            {
                try
                {
                    rv = m_socket.Send(buffer, pos, len, SocketFlags.None);
                    break;
                }
                catch (SocketException e)
                {
                    if (e.NativeErrorCode != 10035)//WSAEWOULDBLOCK 
                    {
                        errorCode = e.NativeErrorCode;
                        break;
                    }
                    else
                    {
                        Thread.Sleep(1);
                    }
                }

            }
            if (errorCode != 0)
            {
                string error = $"Send() failed: WSA error={errorCode}\n";
                //Send the error
                err = NetworkStreamError.kConnectionReset;
                return 0;
            }
            return rv;
        }

        public int Receive(byte[] buffer, int pos, int len, ref NetworkStreamError err, int timeout = 0)
        {
            if (m_socket == null)
            {
                err = NetworkStreamError.kConnectionClosed;
                return 0;
            }

            int rv;

            if (timeout <= 0)
            {
                rv = m_socket.Receive(buffer, pos, len, 0);
            }
            else if (WaitForReadEvent(timeout))
            {
                rv = m_socket.Receive(buffer, pos, len, 0);
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
