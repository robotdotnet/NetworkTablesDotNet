using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTablesDotNet.NetworkTables2.Stream
{
    public interface IOStreamProvider
    {
        IOStream Accept();

        void Close();
    }
}
