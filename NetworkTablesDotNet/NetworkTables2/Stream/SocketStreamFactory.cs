using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTablesDotNet.NetworkTables2.Stream
{
    public class SocketStreamFactory : IOStreamFactory
    {

        private string host;
        private int port;

        public SocketStreamFactory(string host, int port)
        {
            
        }

        public IOStream CreateStream()
        {
            return null;   
        }

    }
}
