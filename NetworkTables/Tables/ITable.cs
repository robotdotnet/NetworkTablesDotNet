using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkTables.Tables
{
    public interface ITable
    {
        string Path { get; }

         bool ContainsKey(string key);
         bool ContainsSubTable(string key);
         ITable GetSubTable(string key);

         void PutNumber(string key, double value);
         double GetNumber(string key, double defaultValue);

         void PutString(string key, string value);
         string GetString(string key, string defaultValue);

         void PutBoolean(string key, bool value);
         bool GetBoolean(string key, bool defaultValue);

         void AddTableListener(ITableListener listener);
         void AddTableListener(ITableListener listener, bool immediateNotify);
         void AddTableListener(string key, ITableListener listener, bool immediateNotify);
         void RemoveTableListener(ITableListener listener);

        void Persist(string key);

        //Add Depriciated Methods Later
    }
}
