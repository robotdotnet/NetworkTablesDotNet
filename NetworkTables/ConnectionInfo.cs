using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables
{
    public struct ConnectionInfo
    {
        string remote_id;
        string remote_name;
        uint remote_port;
        ulong last_update;
        uint protocol_version;
    }
}
