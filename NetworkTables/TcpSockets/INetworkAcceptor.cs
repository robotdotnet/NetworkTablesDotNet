using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables.TcpSockets
{
    internal interface INetworkAcceptor
    {
        int Start();
        void Shutdown();
        INetworkStream Accept();
    }
}
