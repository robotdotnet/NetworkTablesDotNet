using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables.NetworkTables2.Stream
{
    
    public interface IOStreamFactory
    {
        IOStream CreateStream();
    }
    
}
