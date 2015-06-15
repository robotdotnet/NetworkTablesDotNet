using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace NetworkTablesDotNet.NetworkTables2.Stream
{
    public class BinaryReaderBE : BinaryReader
    {
		public BinaryReaderBE(System.IO.Stream stream) : base(stream)
		{
			
		}
		
		public override char ReadChar()
		{
		    byte[] bytes = BitConverter.GetBytes(base.ReadChar());
			if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
			return BitConverter.ToChar(bytes, 0);
		}
		
		public override double ReadDouble()
		{
		    byte[] bytes = BitConverter.GetBytes(base.ReadDouble());
			if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
			return BitConverter.ToDouble(bytes, 0);
		}		
    }
}
