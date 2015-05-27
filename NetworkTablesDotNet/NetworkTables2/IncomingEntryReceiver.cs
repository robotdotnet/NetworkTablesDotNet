using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkTablesDotNet.NetworkTables2
{
    public interface IncomingEntryReceiver
    {
        void OfferIncomingAssignment(NetworkTableEntry entry);
        void OfferIncomingUpdate(NetworkTableEntry entry, char entrySequenceNumber, object value);
    }
}
