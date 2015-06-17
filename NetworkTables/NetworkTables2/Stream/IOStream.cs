using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NetworkTables.NetworkTables2.Connection;

namespace NetworkTables.NetworkTables2.Stream
{
    public interface IOStream
    {
        DataIOStream GetInputStream();
        DataIOStream GetOutputStream();

        void Close();
    }

    
}
