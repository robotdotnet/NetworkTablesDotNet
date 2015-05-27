using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkTablesDotNet.NetworkTables2
{
    public interface OutgoingEntryReceiver
    {
        void OfferOutgoingAssignment(NetworkTableEntry entry);
        void OfferOutgoingUpdate(NetworkTableEntry entry);
    }
}
