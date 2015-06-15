using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NetworkTablesDotNet.NetworkTables2.Connection;

namespace NetworkTablesDotNet.NetworkTables2.Stream
{
    public interface IOStream
    {
        DataIOStream GetInputStream();
        DataIOStream GetOutputStream();

        void Close();
    }

    
}
