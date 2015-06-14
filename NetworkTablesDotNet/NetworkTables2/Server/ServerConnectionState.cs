using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTablesDotNet.NetworkTables2.Server
{
    public class ServerConnectionState
    {
        public static readonly ServerConnectionState GOT_CONNECTION_FROM_CLIENT = new ServerConnectionState("GOT_CONNECTION_FROM_CLIENT");
        /**
         * represents that the client is in a connected non-error state
         */
        public static readonly ServerConnectionState CONNECTED_TO_CLIENT = new ServerConnectionState("CONNECTED_TO_CLIENT");
        /**
         * represents that the client has disconnected from the server
         */
        public static readonly ServerConnectionState CLIENT_DISCONNECTED = new ServerConnectionState("CLIENT_DISCONNECTED");

        public class Error : ServerConnectionState
        {
            private readonly Exception e;

            public Error(Exception e) : base("SERVER_ERROR")
            {
                this.e = e;
            }

            public override string ToString()
            {
                return $"SERVER_ERROR: {e.GetType()}: {e.Message}";
            }
        }


        private string name;

        protected ServerConnectionState(string name)
        {
            this.name = name;
        }

        public override string ToString() => name;
    }
}
