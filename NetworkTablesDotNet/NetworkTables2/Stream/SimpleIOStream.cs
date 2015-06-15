using NetworkTablesDotNet.NetworkTables2.Connection;
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
        private readonly DataIOStream inS;
        private readonly DataIOStream outS;

        public SimpleIOStream(DataIOStream inS, DataIOStream outS)
        {
            this.inS = inS;
            this.outS = outS;
        }

        public DataIOStream GetInputStream()
        {
            return inS;
        }

        public DataIOStream GetOutputStream()
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
