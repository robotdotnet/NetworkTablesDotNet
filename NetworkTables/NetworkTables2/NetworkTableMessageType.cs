using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables.NetworkTables2
{
    public static class NetworkTableMessageType
    {
        public const int KEEP_ALIVE = 0x00;
        /**
         * a client hello message that a client sends
         */
        public const int CLIENT_HELLO = 0x01;
        /**
         * a protocol version unsupported message that the server sends to a client
         */
        public const int PROTOCOL_VERSION_UNSUPPORTED = 0x02;
        public const int SERVER_HELLO_COMPLETE = 0x03;
        /**
         * an entry assignment message
         */
        public const int ENTRY_ASSIGNMENT = 0x10;
        /**
         * a field update message
         */
        public const int FIELD_UPDATE = 0x11;
    }
}
