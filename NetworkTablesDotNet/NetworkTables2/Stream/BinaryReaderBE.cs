using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NetworkTablesDotNet.NetworkTables2.Connection;

namespace NetworkTablesDotNet.NetworkTables2.Stream
{
    public class BinaryReaderBE
    {

        DataIOStream stream;
		public BinaryReaderBE(System.IO.Stream stream)
		{
            this.stream = new DataIOStream(ref stream);
		}
		
		public char ReadChar()
		{
            return stream.ReadCharBE();
		}

        public byte ReadByte()
        {
            return stream.ReadByte();
        }

        public string ReadString()
        {
            return stream.ReadString();
        }

        public void Flush()
        {
            stream.Flush();
        }

        public void Close()
        {
            stream.Close();
        }
    }
}
