using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables.NetworkTables2.Server
{
    public interface ServerAdapterManager
    {
        void Close(ServerConnectionAdapter connectionAdapter, bool closeStream);
    }
}
