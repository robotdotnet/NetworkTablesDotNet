using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkTables.NetworkTables2.Connection;
using NetworkTables.NetworkTables2.Type;

namespace NetworkTables.NetworkTables2.Client
{
    public class ClientNetworkTableEntryStore : AbstractNetworkTableEntryStore
    {
        public ClientNetworkTableEntryStore(TableListenerManager listenerManager) : base(listenerManager)
        {
            
        }


        protected override bool AddEntry(NetworkTableEntry newEntry)
        {
            lock (this)
            {
                NetworkTableEntry entry;
                namedEntries.TryGetValue(newEntry.name, out entry);
                if (entry != null)
                {
                    if (entry.GetId() != newEntry.GetId())
                    {
                        idEntries.Remove(entry.GetId());
                        if (newEntry.GetId() != NetworkTableEntry.UNKNOWN_ID)
                        {
                            entry.SetId(newEntry.GetId());
                            idEntries.Put(newEntry.GetId(), entry);
                        }
                    }
                    entry.ForcePut(newEntry.GetSequenceNumber(), newEntry.GetType(), newEntry.GetValue());
                }
                else
                {
                    if (newEntry.GetId() != NetworkTableEntry.UNKNOWN_ID)
                    {
                        idEntries.Put(newEntry.GetId(), newEntry);
                    }
                    namedEntries.Add(newEntry.name, newEntry);
                }

            }
            return true;
        }

        protected override bool UpdateEntry(NetworkTableEntry entry, char sequenceNumber, object value)
        {
            lock (this)
            {
                entry.ForcePut(sequenceNumber, value);
                return entry.GetId() != NetworkTableEntry.UNKNOWN_ID;
            }
        }

        internal void SendUnknownEntries(NetworkTableConnection connection)
        {
            lock (this)
            {
                foreach (var entry in namedEntries.Values)
                {
                    if (entry.GetId() == NetworkTableEntry.UNKNOWN_ID)
                        connection.SendEntryAssignment(entry);
                }
                connection.Flush();
            }
        }
    }
}
