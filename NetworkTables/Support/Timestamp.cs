using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables.Support
{
    internal static class Timestamp
    {
        public static long Now()
        {
            return DateTime.UtcNow.ToFileTimeUtc();
        }
    }
}
