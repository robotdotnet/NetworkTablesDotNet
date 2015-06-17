using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkTables.NetworkTables2.Client;
using NetworkTables.NetworkTables2.Type;
using NetworkTables.NetworkTables2.Util;
using NetworkTables.Tables;

namespace NetworkTables.NetworkTables2
{
    public abstract class NetworkTableNode : AbstractNetworkTableEntryStore.TableListenerManager, IRemote, ClientConnectionListenerManager
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

        public bool GetBoolean(string name)
        {
            var entry = entryStore.GetEntry(name);
            if (entry == null)
            {
                throw new TableKeyNotDefinedException(name);
            }
            return (bool) entry.GetValue();
        }

        public void PutDouble(string name, double value)
        {
            PutValue(name, DefaultEntryTypes.DOUBLE, value);
        }

        public double GetDouble(string name)
        {
            var entry = entryStore.GetEntry(name);
            if (entry == null)
            {
                throw new TableKeyNotDefinedException(name);
            }
            return (double)entry.GetValue();
        }

        public void PutString(string name, double value)
        {
            PutValue(name, DefaultEntryTypes.STRING, value);
        }

        public string GetString(string name)
        {
            var entry = entryStore.GetEntry(name);
            if (entry == null)
            {
                throw new TableKeyNotDefinedException(name);
            }
            return (string)entry.GetValue();
        }

        public void PutComplex(string name, ComplexData value)
        {
            PutValue(name, value.GetType(), value);
        }


        public void PutValue(string name, object value)
        {
            if (value is double)
            {
                PutValue(name, DefaultEntryTypes.DOUBLE, value);
            }
            else if (value is string)
            {
                PutValue(name, DefaultEntryTypes.STRING, value);
            }
            else if (value is bool)
            {
                PutValue(name, DefaultEntryTypes.BOOLEAN, value);
            }
            else if (value is ComplexData)
            {
                PutValue(name, ((ComplexData)value).GetType(), value);
            }
            else if (value == null)
            {
                throw new ArgumentNullException(nameof(value), "Cannot put a null value into networktables.");
            }
            else
            {
                throw new ArgumentException("Invalid Type");
            }
        }

        public void PutValue(string name, NetworkTableEntryType type, object value)
        {
            if (type is ComplexEntryType)
            {
                lock (entryStore)
                {
                    ComplexEntryType entryType = (ComplexEntryType)type;
                    NetworkTableEntry entry = entryStore.GetEntry(name);
                    if (entry != null)
                        entryStore.PutOutgoing(entry, entryType.InternalizeValue(entry.name, value, entry.GetValue()));
                    else
                        entryStore.PutOutgoing(name, type, entryType.InternalizeValue(name, value, null));

                }
            }
            else
            {
                entryStore.PutOutgoing(name, type, value);
            }
        }

        public void PutValue(NetworkTableEntry entry, object value)
        {
            if (entry.GetType() is ComplexEntryType)
            {
                lock (entryStore)
                {
                    ComplexEntryType entryType = (ComplexEntryType) entry.GetType();
                    entryStore.PutOutgoing(entry, entryType.InternalizeValue(entry.name, value, entry.GetValue()));
                }
            }
            else
            {
                entryStore.PutOutgoing(entry, value);
            }
        }

        public object GetValue(string name)
        {
            lock (entryStore)
            {
                var entry = entryStore.GetEntry(name);
                if (entry == null)
                {
                    throw new TableKeyNotDefinedException(name);
                }
                return entry.GetValue();
            }
        }

        public void RetrieveValue(string name, object externalData)
        {
            lock (entryStore)
            {
                NetworkTableEntry entry = entryStore.GetEntry(name);
                if (entry == null)
                    throw new TableKeyNotDefinedException(name);
                NetworkTableEntryType entryType = entry.GetType();
                if (!(entryType is ComplexEntryType))
                {
                    throw new TableKeyExistsWithDifferentTypeException(name, entryType, "Is not a complex data type");
                }
                ComplexEntryType complexType = (ComplexEntryType) entryType;
                complexType.ExportValue(name, entry.GetValue(), externalData);
            }
        }

        public bool ContainsKey(string key)
        {
            return entryStore.GetEntry(key) != null;
        }

        private readonly List remoteListeners = new List();
        public void AddConnectionListener(IRemoteConnectionListener listener, bool immediateNotify)
        {
            remoteListeners.Add(listener);
            if (IsConnected())
            {
                listener.Connected(this);
            }
            else
            {
                listener.Disconnected(this);
            }
        }

        public void RemoveConnectionListener(IRemoteConnectionListener listener)
        {
            remoteListeners.Remove(listener);
        }

        public void FireConnectedEvent()
        {
            for (int i = 0; i < remoteListeners.Size(); ++i)
                ((IRemoteConnectionListener)remoteListeners.Get(i)).Connected(this);
        }
        public void FireDisconnectedEvent()
        {
            for (int i = 0; i < remoteListeners.Size(); ++i)
                ((IRemoteConnectionListener)remoteListeners.Get(i)).Disconnected(this);
        }

        public abstract bool IsConnected();

        public abstract bool IsServer();

        public void FireTableListeners(string key, object value, bool isNew)
        {
            for (int i = 0; i < tableListeners.Size(); ++i)
                ((ITableListener)tableListeners.Get(i)).ValueChanged(null, key, value, isNew);
        }

        public abstract void Close();

        private readonly List tableListeners = new List();

        public void AddTableListener(ITableListener listener, bool immediateNotify)
        {
            tableListeners.Add(listener);
            if (immediateNotify)
            {
                entryStore.NotifyEntries(null, listener);
            }
        }

        public void RemoveTableListener(ITableListener listener)
        {
            tableListeners.Remove(listener);
        }
    }
}
