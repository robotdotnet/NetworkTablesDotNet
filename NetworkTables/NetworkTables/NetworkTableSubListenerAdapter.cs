using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkTables;
using NetworkTables.Tables;

namespace NetworkTables.NetworkTables
{
    public class NetworkTableSubListenerAdapter : ITableListener
    {
        private ITableListener targetListener;
        private NetworkTableOld targetSource;
        private string prefix;

        private HashSet<string> notifiedTables = new HashSet<string>();

        public NetworkTableSubListenerAdapter(string prefix, NetworkTableOld targetSource, ITableListener targetListener)
        {
            this.prefix = prefix;
            this.targetSource = targetSource;
            this.targetListener = targetListener;
        }



        public void ValueChanged(ITable source, string key, object value, bool isNew)
        {
            if(key.StartsWith(prefix))
            {
                string relativeKey = key.Substring(prefix.Length + 1);
                int endSubTable = -1;
                for (int i = 0; i < relativeKey.Length; ++i)
                {
                    if(relativeKey[i] == NetworkTableOld.PATH_SEPARATOR)
                    {
                        endSubTable = i;
                        break;
                    }
                }
                if (endSubTable!=-1)
                {
                    string subTableKey = relativeKey.Substring(0, endSubTable);
                    if(!notifiedTables.Contains(subTableKey))
                    {
                        notifiedTables.Add(subTableKey);
                        targetListener.ValueChanged(targetSource, subTableKey, targetSource.GetSubTable(subTableKey), true);
                    }
                }
            }
        }
    }
}
