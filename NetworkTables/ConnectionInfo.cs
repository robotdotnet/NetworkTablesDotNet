using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables
{
    public struct ConnectionInfo
    {
        public string RemoteId { get; }
        public string RemoteName { get; }
        public uint RemotePort { get; }
        public ulong LastUpdate { get; }
        public uint ProtocolVersion { get; }

        public ConnectionInfo(string remoteId, string remoteName, uint remotePort, uint lastUpdate, uint protocolVersion)
        {
            RemoteId = remoteId;
            RemoteName = remoteName;
            RemotePort = remotePort;
            LastUpdate = lastUpdate;
            ProtocolVersion = protocolVersion;
        }
    }
}
