using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace NetworkTablesDotNet.NetworkTables2.Stream
{
    public interface IOStream
    {
        BinaryReader GetInputStream();
        BinaryWriter GetOutputStream();

        void Close();
    }

    
}
