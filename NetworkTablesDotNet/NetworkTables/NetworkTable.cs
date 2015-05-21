using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkTablesDotNet.Tables;
using NetworkTablesDotNet.NetworkTables2.Thread;

namespace NetworkTablesDotNet.NetworkTables
{
    public class NetworkTable : ITable, IRemote
    {
        private static NTThreadManager threadManager = new DefaultThreadManager();

        public static readonly char PATH_SEPARATOR = '/';
        public static readonly int DEFAULT_PORT = 1735;

        public bool ContainsKey(string key)
        {
            throw new NotImplementedException();
        }

        public bool ContainsSubTable(string key)
        {
            throw new NotImplementedException();
        }

        public ITable GetSubTable(string key)
        {
            throw new NotImplementedException();
        }

        public object GetValue(string key)
        {
            throw new NotImplementedException();
        }

        public void PutValue(string key, object value)
        {
            throw new NotImplementedException();
        }

        public void RetrieveValue(string key, object externalValue)
        {
            throw new NotImplementedException();
        }

        public void PutNumber(string key, double value)
        {
            throw new NotImplementedException();
        }

        public double GetNumber(string key)
        {
            throw new NotImplementedException();
        }

        public double GetNumber(string key, double defaultValue)
        {
            throw new NotImplementedException();
        }

        public void PutString(string key, string value)
        {
            throw new NotImplementedException();
        }

        public string GetString(string key)
        {
            throw new NotImplementedException();
        }

        public string GetString(string key, string defaultValue)
        {
            throw new NotImplementedException();
        }

        public void PutBoolean(string key, bool value)
        {
            throw new NotImplementedException();
        }

        public bool GetBoolean(string key)
        {
            throw new NotImplementedException();
        }

        public bool GetBoolean(string key, bool defaultValue)
        {
            throw new NotImplementedException();
        }

        public void AddTableListener(ITableListener listener)
        {
            throw new NotImplementedException();
        }

        public void AddTableListener(ITableListener listener, bool immediateNotify)
        {
            throw new NotImplementedException();
        }

        public void AddTableListener(string key, ITableListener listener, bool immediateNotify)
        {
            throw new NotImplementedException();
        }

        public void AddSubTableListener(ITableListener listener)
        {
            throw new NotImplementedException();
        }

        public void RemoveTableListener(ITableListener listener)
        {
            throw new NotImplementedException();
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
    }
}
