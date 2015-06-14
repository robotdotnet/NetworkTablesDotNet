using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTablesDotNet.NetworkTables2.Connection
{
    public class BadMessageException : IOException
    {
        public BadMessageException(string message) : base(message)
        {
            
        }
    }
}
