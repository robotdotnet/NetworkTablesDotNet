using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTablesDotNet.NetworkTables2.Stream
{
    public class SimpleIOStream : IOStream
    {
        private readonly BinaryReaderBE inS;
        private readonly BinaryWriterBE outS;

        public SimpleIOStream(BinaryReaderBE inS, BinaryWriterBE outS)
        {
            this.inS = inS;
            this.outS = outS;
        }

        public BinaryReaderBE GetInputStream()
        {
            return inS;
        }

        public BinaryWriterBE GetOutputStream()
        {
            return outS;
        }

        public void Close()
        {
            try
            {
                inS.Close();
            }
            catch (IOException)
            {
            }
            try
            {
                outS.Close();
            }
            catch (IOException)
            {
            }

        }
    }
}
