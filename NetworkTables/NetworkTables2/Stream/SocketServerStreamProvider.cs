using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables.NetworkTables2.Stream
{
    
    public class SocketServerStreamProvider : IOStreamProvider
    {
        private TcpListener server = null;

        public SocketServerStreamProvider(int port)
        {
           server = new TcpListener(IPAddress.Any, port);
            server.Start();
        }

        public IOStream Accept()
        {
            NetworkStream stream = new NetworkStream(server.AcceptSocket());
            return new SocketStream(stream);
        }

        public void Close() => server?.Stop();
    }
    
}
