using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace NetworkTablesDotNet.NetworkTables2.Stream
{
    public class SocketStream :SimpleIOStream
    {
        private Socket socket;

        public SocketStream(Socket socket) : base(socket.i)
        {
            
        }
    }
}
