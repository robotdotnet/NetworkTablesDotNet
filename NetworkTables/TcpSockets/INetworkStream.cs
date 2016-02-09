using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables.TcpSockets
{
    public enum NetworkStreamError
    {
        kConnectionClosed = 0,
        kConnectionReset = -1,
        kConnectionTimedOut = -2,
    }
    public interface INetworkStream
    {
        int Send(byte[] buffer, int pos, int len, ref NetworkStreamError error);
        int Receive(byte[] buffer, int pos, int len, ref NetworkStreamError err, int timeout = 0);
        void Close();

        string GetPeerIP();
        int GetPeerPort();
        void SetNoDelay();
    }
}
