using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables.TcpSockets
{
    public class TCPConnector
    {
        private static int ResolveHostName(string hostName, ref IPAddress addr)
        {
            addr = IPAddress.Parse(hostName);
            return 0;
        }

        public static INetworkStream Connect(string server, int port, int timeout = 0)
        {
            IPAddress addr = null;
            ResolveHostName(server, ref addr);

            if (timeout == 0)
            {
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream,  0);

                socket.Connect(addr, port);

                return new TCPStream(socket);
            }

            var sct = new Socket(AddressFamily.InterNetwork, SocketType.Stream, 0);
            sct.Blocking = false;
            //Add timeout
        }
    }
}
