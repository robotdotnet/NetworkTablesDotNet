using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkTablesDotNet.NetworkTables2.Util;

namespace NetworkTablesDotNet.NetworkTables2.Server
{
    public class ServerConnectionList : FlushableOutgoingEntryReceiver, ServerAdapterManager
    {
        private  List connections = new List();
        private readonly object connectionsLock = new object();

        public void Add(ServerConnectionAdapter connection)
        {
            lock (connectionsLock)
            {
                connections.Add(connection);
            }
        } 

        public void OfferOutgoingAssignment(NetworkTableEntry entry)
        {
            lock (connectionsLock)
            {
                for (int i = 0; i < connections.Size(); ++i)
                {
                    ((ServerConnectionAdapter) connections.Get(i)).OfferOutgoingAssignment(entry);
                }
            }
        }

        public void OfferOutgoingUpdate(NetworkTableEntry entry)
        {
            lock (connectionsLock)
            {
                for (int i = 0; i < connections.Size(); ++i)
                {
                    ((ServerConnectionAdapter)connections.Get(i)).OfferOutgoingUpdate(entry);
                }
            }
        }

        public void Flush()
        {
            lock (connectionsLock)
            {
                for (int i = 0; i < connections.Size(); ++i)
                {
                    ((ServerConnectionAdapter)connections.Get(i)).Flush();
                }
            }
        }

        public void EnsureAlive()
        {
            lock (connectionsLock)
            {
                for (int i = 0; i < connections.Size(); ++i)
                {
                    ((ServerConnectionAdapter) connections.Get(i)).EnsureAlive();
                }
            }
        }

        public void Close(ServerConnectionAdapter connectionAdapter, bool closeStream)
        {
            lock (connectionsLock)
            {
                if (connections.Remove(connectionAdapter))
                {
                    Console.WriteLine($"Close: {connectionAdapter}");
                    connectionAdapter.Shutdown(closeStream);
                }
            }
        }

        public void CloseAll()
        {
            lock (connectionsLock)
            {
                while (connections.Size() > 0)
                {
                    Close((ServerConnectionAdapter)connections.Get(0), true);
                }
            }
        }
    }
}
