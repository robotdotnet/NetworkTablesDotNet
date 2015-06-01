using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkTablesDotNet.NetworkTables2.Connection;
using NetworkTablesDotNet.NetworkTables2.Type;
using System.IO;

namespace NetworkTablesDotNet.NetworkTables2
{
    public class NetworkTableEntry
    {
        public static char UNKNOWN_ID = (char)0xFFFF;
        private char id;
        private char sequenceNumber;

        public string name;

        private NetworkTableEntryType type;
        private object value;
        private volatile bool isDirty = false;
        private volatile bool isNew = true;

        public NetworkTableEntry(string name, NetworkTableEntryType type, object value)
            : this(UNKNOWN_ID, name, (char)0, type, value)
        {

        }

        public NetworkTableEntry(char id, string name, char sequenceNumber, NetworkTableEntryType type, object value)
        {
            this.id = id;
            this.name = name;
            this.sequenceNumber = sequenceNumber;
            this.type = type;
            this.value = value;
        }

        public char GetId()
        {
            return id;
        }

        public object GetValue()
        {
            return value;
        }

        public new NetworkTableEntryType GetType()
        {
            return type;
        }

        private static char HALF_OF_CHAR = (char)32768;

        public bool PutValue(char newSequenceNumber, object newValue)
        {
            if ((sequenceNumber < newSequenceNumber && newSequenceNumber - sequenceNumber < HALF_OF_CHAR)
                || (sequenceNumber > newSequenceNumber && sequenceNumber - newSequenceNumber > HALF_OF_CHAR))
            {
                value = newValue;
                sequenceNumber = newSequenceNumber;
                return true;
            }
            return false;
        }

        public void ForcePut(char newSequenceNumber, object newValue)
        {
            value = newValue;
            sequenceNumber = newSequenceNumber;
        }

        public void ForcePut(char newSequenceNumber, NetworkTableEntryType type, Object newValue)
        {
            this.type = type;
            ForcePut(newSequenceNumber, newValue);
        }

        public void MakeDirty()
        {
            isDirty = true;
        }

        public void MakeClean()
        {
            isDirty = false;
        }

        public bool IsDirty()
        {
            return isDirty;
        }

        public void SendValue(BinaryWriter os)
        {
            type.SendValue(value, os);
        }

        public char GetSequenceNumber()
        {
            return sequenceNumber;
        }

        public void SetId(char id)
        {
            if (this.id != UNKNOWN_ID)
            {
                throw new InvalidOperationException("Cannot set the ID of a table entry that already has a valid id");
            }
            this.id = id;
        }

        public void ClearId()
        {
            id = UNKNOWN_ID;
        }

        public void Send(NetworkTableConnection connection)
        {
            //connection.SendEntryAssignment(this);
        }

        public void FireListener(AbstractNetworkTableEntryStore.TableListenerManager listenerManager)
        {
            listenerManager.FireTableListeners(name, value, isNew);
            isNew = false;
        }

        public new string ToString()
        {
            return "Network Table " + type.name + " entry: " + name + ": " + GetId() + " - " + (int)GetSequenceNumber() + " - " + GetValue();
        }
    }
}
