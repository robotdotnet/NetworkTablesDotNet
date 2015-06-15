using NetworkTablesDotNet.NetworkTables2.Connection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTablesDotNet.NetworkTables2.Stream
{
    public class SocketStream : SimpleIOStream
    {
        private readonly NetworkStream stream;

        public SocketStream(string host, int port) : this(new NetworkStream(new TcpClient(host, port).Client))
        {
            
        }

        public SocketStream(NetworkStream stream) : base(new DataIOStream(stream), new DataIOStream(stream))
        {
            this.stream = stream;
        }

        public void Close()
        {
            base.Close();
            try
            {
                stream.Close();
            }
            catch (IOException)
            {
            }
        }
    }
}
