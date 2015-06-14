using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTablesDotNet.NetworkTables2
{
    public interface FlushableOutgoingEntryReceiver : OutgoingEntryReceiver
    {
        void Flush();
        void EnsureAlive();
    }
}
