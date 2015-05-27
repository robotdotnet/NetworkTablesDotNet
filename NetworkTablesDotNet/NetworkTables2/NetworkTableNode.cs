using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkTablesDotNet.NetworkTables2.Type;
using NetworkTablesDotNet.Tables;

namespace NetworkTablesDotNet.NetworkTables2
{
    public class NetworkTableNode : AbstractNetworkTableEntryStore.TableListenerManager, IRemote
    {
        protected AbstractNetworkTableEntryStore entryStore;

        protected void Init(AbstractNetworkTableEntryStore entryStore)
        {
            this.entryStore = entryStore;
        }

        public AbstractNetworkTableEntryStore GetEntryStore()
        {
            return entryStore;
        }

        public void PutBoolean(string name, bool value)
        {
            PutValue(name, DefaultEntryTypes.BOOLEAN, value);
        }

        public void PutValue(string name, object value)
        {
            
        }

        public void PutValue(string name, NetworkTableEntryType type, object value)
        {
            if (type is ComplexEntryType)
            {
                lock (entryStore)
                {
                    ComplexEntryType entryType = (ComplexEntryType)type;

                }
            }
        }

        public void AddConnectionListener(IRemoteConnectionListener listener, bool immediateNotify)
        {
            throw new NotImplementedException();
        }

        public void RemoveConnectionListener(IRemoteConnectionListener listener)
        {
            throw new NotImplementedException();
        }

        public bool IsConnected()
        {
            throw new NotImplementedException();
        }

        public bool ISServer()
        {
            throw new NotImplementedException();
        }

        public void FireTableListeners(string key, object value, bool isNew)
        {
            throw new NotImplementedException();
        }
    }
}
