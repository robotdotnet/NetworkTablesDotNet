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
        private readonly BinaryReader reader;
        private readonly BinaryWriter writer;

        public SimpleIOStream(BinaryReader reader, BinaryWriter writer)
        {
            this.reader = reader;
            this.writer = writer;
        }


        public BinaryReader GetInputStream()
        {
            return reader;
        }

        public BinaryWriter GetOutputStread()
        {
            return writer;
        }

        public void Close()
        {
            try
            {
                reader.Close();
            }
            catch (IOException e)
            {
                
            }

            try
            {
                writer.Close();
            }
            catch(IOException e)
            {
                
            }

        }
    }
}
