using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkTables.NetworkTables2
{
    public class OutgoingEntryReceiverNull : OutgoingEntryReceiver
    {
        public void OfferOutgoingAssignment(NetworkTableEntry entry)
        {
        }

        public void OfferOutgoingUpdate(NetworkTableEntry entry)
        {
        }
    }

    public interface OutgoingEntryReceiver
    {
        void OfferOutgoingAssignment(NetworkTableEntry entry);
        void OfferOutgoingUpdate(NetworkTableEntry entry);
    }
}
