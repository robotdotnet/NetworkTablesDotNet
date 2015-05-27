using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NetworkTablesDotNet.NetworkTables2.Connection
{
    public class NetworkTableConnection
    {
        public static char PROTOCOL_REVISION = (char)0x0200;

        private object WRITE_LOCK = new object();

        private BinaryReader inStream;
        private BinaryWriter outStream;



    }
}
