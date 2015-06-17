using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkTables;
using NetworkTables.NetworkTables2;
using NetworkTables.Tables;

namespace NetworkTables.NetworkTables
{
    public class NetworkTableProvider
    {
        private Dictionary<string, ITable> tables = new Dictionary<string, ITable>();
        private readonly NetworkTableNode node;


        public NetworkTableProvider(NetworkTableNode node)
        {
            this.node = node;
        }

        public ITable GetRootTable()
        {
            return GetTable("");
        }

        public ITable GetTable(string key)
        {
            if (tables.ContainsKey(key))
            {
                return tables[key];
            }
            else
            {
                NetworkTable table = new NetworkTable(key, this);
                tables.Add(key, table);
                return table;
            }
        }
        public NetworkTableNode GetNode()
        {
            return node;
        }

        public void Close()
        {
            node.Close();
        }
    }
}
