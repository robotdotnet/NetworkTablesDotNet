using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables.Support
{
    public static class Timestamp
    {
        public static ulong Now()
        {
            return (ulong)DateTime.UtcNow.ToFileTimeUtc();
        }
    }
}
