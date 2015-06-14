using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTablesDotNet.NetworkTables2.Connection
{
    public interface ConnectionAdapter : IncomingEntryReceiver
    {
        void KeepAlive();

        void ClientHello(char protocolRevision);

        void ProtocolVersionUnsupported(char protocolRevision);

        void ServerHelloComplete();

        NetworkTableEntry GetEntry(char id);

        void BadMessage(BadMessageException e);

        void IOException(Exception e);

    }
}
