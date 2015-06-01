using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTablesDotNet.NetworkTables2.Stream
{
    public class SocketServerStreamProvider : IOStreamProvider
    {
        private int serverSocket;

        public SocketServerStreamProvider(int port)
        {
           
        }

        public IOStream Accept()
        {
            return null;
        }

        public void Close()
        {
            
        }
    }
}
