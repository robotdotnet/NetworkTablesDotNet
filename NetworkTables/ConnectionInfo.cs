using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables
{
    public struct ConnectionInfo
    {
        public string remote_id;
        public string remote_name;
        public uint remote_port;
        public ulong last_update;
        public uint protocol_version;
    }
}
