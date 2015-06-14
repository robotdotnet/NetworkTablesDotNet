using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTablesDotNet.NetworkTables2.Stream
{
    class BinaryWriterBE : BinaryWriter
    {

        public BinaryWriterBE(System.IO.Stream stream) : base(stream)
        {
        }

        public new void Write(char value)
        {
            byte[] bytes = BitConverter.GetBytes(base.ReadChar())
			if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
			base.Write(BitConverter.ToChar(bytes));
        }

        public new void Write(double value)
        {
            byte[] bytes = BitConverter.GetBytes(base.ReadDouble())
			if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
			base.Write(BitConverter.ToDouble(bytes));
        }
    }
}
