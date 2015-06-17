using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables.NetworkTables2
{
    public class TransactionDirtier : OutgoingEntryReceiver
    {
        private readonly OutgoingEntryReceiver continuingReceiver;

        public TransactionDirtier(OutgoingEntryReceiver continuingReceiver)
        {
            this.continuingReceiver = continuingReceiver;
        }

        public void OfferOutgoingAssignment(NetworkTableEntry entry)
        {
            if (entry.IsDirty())
                return;
            entry.MakeDirty();
            continuingReceiver.OfferOutgoingAssignment(entry);
        }

        public void OfferOutgoingUpdate(NetworkTableEntry entry)
        {
            if (entry.IsDirty())
                return;
            entry.MakeDirty();
            continuingReceiver.OfferOutgoingUpdate(entry);
        }
    }
}
