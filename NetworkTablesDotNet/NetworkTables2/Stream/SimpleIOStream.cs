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
        private readonly BinaryReader inS;
        private readonly BinaryWriter outS;

        public SimpleIOStream(BinaryReader inS, BinaryWriter outS)
        {
            this.inS = inS;
            this.outS = outS;
        }

        public BinaryReader GetInputStream()
        {
            return inS;
        }

        public BinaryWriter GetOutputStream()
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
