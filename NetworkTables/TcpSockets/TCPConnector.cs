using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables.TcpSockets
{
    internal class TCPConnector
    {
        private static int ResolveHostName(string hostName, ref IPAddress addr)
        {
            try
            {
                var addressEntry = Dns.GetHostEntry(hostName);
                addr = addressEntry.AddressList[0];
            }
            catch (SocketException e)
            {
                return e.NativeErrorCode;
            }
            return 0;
        }

        public static INetworkStream Connect(string server, int port, int timeout = 0)
        {
            try
            {
                TcpClient client = new TcpClient(server, port);
                return new TCPStream(client.Client);
            }
            catch (SocketException)
            {
                return null;
            }
            
            /*
            IPAddress addr = null;
            if (ResolveHostName(server, ref addr) != 0)
            {
                try
                {
                    addr = IPAddress.Parse(server);
                }
                catch (SocketException e)
                {
                    //Error
                    return null;
                }
            }

            if (timeout == 0)
            {
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, 0);

                socket.Connect(addr, port);

                return new TCPStream(socket);
            }

            var sct = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //TODO: A lot more timeout stuff.
            sct.Blocking = false;
            sct.Connect(addr, port);

            ArrayList list = new ArrayList { sct };
            Socket.Select(null, list, null, timeout * 1000000);
            if (list.Count > 0)
            {
                //Get Val Opt.
            }
            else
            {
                //Report Timeout
            }

            sct.Blocking = true;

            return new TCPStream(sct);
            */
        }
    }
}
