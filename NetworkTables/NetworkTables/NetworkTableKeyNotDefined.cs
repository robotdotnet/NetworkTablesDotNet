using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables.NetworkTables
{
    public class NetworkTableKeyNotDefined : InvalidOperationException
    {
        public NetworkTableKeyNotDefined(string key) : base($"Unknown Table Key: {key}")
        {
            
        }
    }
}
