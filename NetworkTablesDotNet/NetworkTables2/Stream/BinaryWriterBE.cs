using NetworkTablesDotNet.NetworkTables2.Connection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTablesDotNet.NetworkTables2.Stream
{
    public class BinaryWriterBE
    {
        DataIOStream stream;

        public BinaryWriterBE(System.IO.Stream stream)
        {
            this.stream = new DataIOStream(ref stream);
        }

        public void WriteChar(char value)
        {
            stream.WriteCharBE(value);
        }

        public void WriteByte(byte value)
        {
            stream.WriteByte(value);
        }

        public void WriteString(string str)
        {
            stream.WriteString(str);
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
