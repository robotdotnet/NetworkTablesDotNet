using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTablesDotNet.NetworkTables2.Stream
{
    public class BinaryWriterBE : BinaryWriter
    {

        public BinaryWriterBE(System.IO.Stream stream) : base(stream)
        {
        }

        public override void Write(char value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            base.Write(BitConverter.ToChar(bytes, 0));
        }

        public override void Write(double value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
			//if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
			base.Write(BitConverter.ToDouble(bytes, 0));
        }
    }
}
