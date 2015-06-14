using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTablesDotNet.NetworkTables2.Client
{
    public class ClientConnectionState
    {
        public static ClientConnectionState DISCONNECTED_FROM_SERVER = new ClientConnectionState("DISCONNECTED_FROM_SERVER");
        /**
         * indicates that the client is connected to the server but has not yet begun communication
         */
        public static ClientConnectionState CONNECTED_TO_SERVER = new ClientConnectionState("CONNECTED_TO_SERVER");
        /**
         * represents that the client has sent the hello to the server and is waiting for a response
         */
        public static ClientConnectionState SENT_HELLO_TO_SERVER = new ClientConnectionState("SENT_HELLO_TO_SERVER");
        /**
         * represents that the client is now in sync with the server
         */
        public static ClientConnectionState IN_SYNC_WITH_SERVER = new ClientConnectionState("IN_SYNC_WITH_SERVER");

        public class ProtocolUnsupportedByServer : ClientConnectionState
        {
            private readonly char serverVersion;

            public ProtocolUnsupportedByServer(char serverVersion) : base("PROTOCOL_UNSUPPORTED_BY_SERVER")
            {
                this.serverVersion = serverVersion;
            }

            public char GetServerVersion() => serverVersion;
            public override string ToString() => $"PROTOCOL_UNSUPPORTED_BY_SERVER: Server Version: 0x{((int)serverVersion).ToString("X")}";
        }

        public class Error : ClientConnectionState
        {
            private readonly Exception e;

            public Error(Exception e) : base("CLIENT_ERROR")
            {
                this.e = e;
            }

            public override string ToString()
            {
                return $"CLIENT_ERROR: {e.GetType()}: {e.Message}";
            }
        }

        private string name;

        protected ClientConnectionState(string name)
        {
            this.name = name;
        }

        public override string ToString()
        {
            return name;
        }
    }
}
