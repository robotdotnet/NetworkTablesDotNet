using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkTables.NetworkTables2.Connection;

namespace NetworkTables.NetworkTables2.Server
{
    public class ServerNetworkTableEntryStore : AbstractNetworkTableEntryStore
    {
        public ServerNetworkTableEntryStore(TableListenerManager listenerManager) : base(listenerManager)
        {
            
        }

        private char nextId = (char) 0;
        //private object m_lockObject = new object();

        protected override bool AddEntry(NetworkTableEntry newEntry)
        {
            lock (this)
            {
                NetworkTableEntry entry;
                if (!namedEntries.TryGetValue(newEntry.name, out entry))
                {
                    newEntry.SetId((nextId++));
                    idEntries.Put(newEntry.GetId(), newEntry);
                    namedEntries.Add(newEntry.name, newEntry);
                    return true;
                }
                return false;
            }
        }

        protected override bool UpdateEntry(NetworkTableEntry entry, char sequenceNumber, object value)
        {
            lock (this)
            {
                return entry.PutValue(sequenceNumber, value);
            }
        }

        internal void SendServerHello(NetworkTableConnection connection)
        {
            lock (this)
            {
                foreach (var e in namedEntries)
                {
                    connection.SendEntryAssignment(e.Value);
                }
                connection.SendServerHelloComplete();
                connection.Flush();
            }
        }
    }
}
