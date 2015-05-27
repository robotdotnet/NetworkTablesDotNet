using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkTablesDotNet.NetworkTables2.Type;
using NetworkTablesDotNet.Tables;

namespace NetworkTablesDotNet.NetworkTables2
{
    public abstract class AbstractNetworkTableEntryStore : IncomingEntryReceiver
    {
        protected List<NetworkTableEntry> idEntries = new List<NetworkTableEntry>();
        protected Dictionary<string, NetworkTableEntry> namedEntries = new Dictionary<string, NetworkTableEntry>();

        protected TableListenerManager listenerManager;

        private object m_lockObject = new object();

        protected AbstractNetworkTableEntryStore(TableListenerManager listenerManager)
        {
            this.listenerManager = listenerManager;
        }

        public NetworkTableEntry GetEntry(char entryID)
        {
            lock (m_lockObject)
            {
                return (NetworkTableEntry)idEntries[entryID];
            }
        }

        public NetworkTableEntry GetEntry(string name)
        {
            lock (m_lockObject)
            {
                return (NetworkTableEntry)namedEntries[name];
            }
        }

        public List<string> Keys()
        {
            lock (m_lockObject)
            {
                return namedEntries.Keys.ToList();
            }
        }

        public void ClearEntries()
        {
            lock (m_lockObject)
            {
                idEntries.Clear();
                namedEntries.Clear();
            }
        }

        public void ClearIds()
        {
            lock (m_lockObject)
            {
                idEntries.Clear();
                foreach (var s in namedEntries.Values)
                {
                    s.ClearId();
                }
            }
        }

        protected OutgoingEntryReceiver outgoingReceiver;
        protected OutgoingEntryReceiver incomingReceiver;
        public void SetOutgoingReceiver(OutgoingEntryReceiver receiver)
        {
            outgoingReceiver = receiver;
        }
        public void SetIncomingReceiver(OutgoingEntryReceiver receiver)
        {
            incomingReceiver = receiver;
        }

        protected abstract bool AddEntry(NetworkTableEntry entry);

        protected abstract bool UpdateEntry(NetworkTableEntry entry, char sequenceNumber, object value);

        private static bool ValuesEqual(object o1, object o2)
        {
            if (o1 is object[])
            {
                Object[] a1 = (Object[])o1;
                Object[] a2 = (Object[])o2;
                if (a1.Length != a2.Length)
                    return false;
                for (int i = 0; i < a1.Length; ++i)
                    if (!ValuesEqual(a1[i], a2[i]))
                        return false;
                return true;
            }
            return o1 != null ? o1.Equals(o2) : o2 == null;
        }

        public void PutOutgoing(string name, NetworkTableEntryType type, object value)
        {
            lock (m_lockObject)
            {

                NetworkTableEntry tableEntry = null;
                if (namedEntries.ContainsKey(name))
                    tableEntry = (NetworkTableEntry) namedEntries[name];
                if (tableEntry == null)
                {
                    tableEntry = new NetworkTableEntry(name, type, value);

                    if (AddEntry(tableEntry))
                    {
                        tableEntry.FireListener(listenerManager);
                        outgoingReceiver.OfferOutgoingAssignment(tableEntry);
                    }

                }
                else
                {
                    if (tableEntry.GetType().id != type.id)
                    {
                        throw new TableKeyExistsWithDifferentTypeException(name, tableEntry.GetType());
                    }
                    if (!ValuesEqual(value, tableEntry.GetValue()))
                    {
                        if (UpdateEntry(tableEntry, (char)(tableEntry.GetSequenceNumber() + 1), value))
                        {
                            outgoingReceiver.OfferOutgoingUpdate(tableEntry);
                        }
                        tableEntry.FireListener(listenerManager);
                    }
                }
            }
        }

        public void PutOutgoing(NetworkTableEntry tableEntry, object value)
        {
            lock (m_lockObject)
            {
                if (!ValuesEqual(value, tableEntry.GetValue()))
                {
                    if (UpdateEntry(tableEntry, (char)(tableEntry.GetSequenceNumber() + 1), value))
                    {
                        outgoingReceiver.OfferOutgoingUpdate(tableEntry);
                    }
                    tableEntry.FireListener(listenerManager);
                }
            }
        }

        public void OfferIncomingAssignment(NetworkTableEntry entry)
        {
            lock (m_lockObject)
            {
                NetworkTableEntry tableEntry = null;
                if (namedEntries.ContainsKey(entry.name))
                    tableEntry = (NetworkTableEntry)namedEntries[entry.name];
                if (AddEntry(entry))
                {
                    if (tableEntry == null)
                        tableEntry = entry;
                    tableEntry.FireListener(listenerManager);
                    incomingReceiver.OfferOutgoingAssignment(tableEntry);
                }
            }
        }

        public void OfferIncomingUpdate(NetworkTableEntry entry, char entrySequenceNumber, object value)
        {
            lock (m_lockObject)
            {
                if (UpdateEntry(entry, entrySequenceNumber, value))
                {
                    entry.FireListener(listenerManager);
                    incomingReceiver.OfferOutgoingUpdate(entry);
                }
            }
        }

        public void NotifyEntries(ITable table, ITableListener listener)
        {
            lock (m_lockObject)
            {
                foreach (var entry in namedEntries.Values)
                {
                    listener.ValueChanged(table, entry.name, entry.GetValue(), true);
                }
            }
        }

        public interface TableListenerManager
        {
            void FireTableListeners(string key, object value, bool isNew);
        }

    }
}
