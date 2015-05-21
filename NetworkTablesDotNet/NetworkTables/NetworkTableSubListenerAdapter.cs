using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkTablesDotNet.Tables;

namespace NetworkTablesDotNet.NetworkTables
{
    public class NetworkTableSubListenerAdapter : ITableListener
    {
        private ITableListener targetListener;
        private NetworkTable targetSource;
        private string prefix;

        private HashSet<string> notifiedTables = new HashSet<string>();

        public NetworkTableSubListenerAdapter(string prefix, NetworkTable targetSource, ITableListener targetListener)
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
                    if(relativeKey[i] == NetworkTable.PATH_SEPARATOR)
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
