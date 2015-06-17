using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables.NetworkTables2.Stream
{
    
    public class SocketStreams
    {
        public static IOStreamFactory NewStreamFactory(string host, int port)
        {
            return new SocketStreamFactory(host, port);
        }

        public static IOStreamProvider NewStreamProvider(int port)
        {
            return new SocketServerStreamProvider(port);
        }
    }
    
}
