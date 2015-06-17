using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkTables.Tables
{
    public interface ITable
    {
         bool ContainsKey(string key);
         bool ContainsSubTable(string key);
         ITable GetSubTable(string key);
         object GetValue(string key);
         void PutValue(string key, object value);
         void RetrieveValue(string key, object externalValue);

         void PutNumber(string key, double value);
         double GetNumber(string key);
         double GetNumber(string key, double defaultValue);

         void PutString(string key, string value);
         string GetString(string key);
         string GetString(string key, string defaultValue);

         void PutBoolean(string key, bool value);
         bool GetBoolean(string key);
         bool GetBoolean(string key, bool defaultValue);

         void AddTableListener(ITableListener listener);
         void AddTableListener(ITableListener listener, bool immediateNotify);
         void AddTableListener(string key, ITableListener listener, bool immediateNotify);
         void AddSubTableListener(ITableListener listener);
         void RemoveTableListener(ITableListener listener);

        //Add Depriciated Methods Later
    }
}
