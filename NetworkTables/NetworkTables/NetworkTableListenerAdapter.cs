using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkTables;
using NetworkTables.Tables;

namespace NetworkTables.NetworkTables
{
    public class NetworkTableListenerAdapter : ITableListener
    {
        private readonly ITableListener targetListener;
        private readonly ITable targetSource;
        private readonly string prefix;

        public NetworkTableListenerAdapter(string prefix, ITable targetSource, ITableListener targetListener)
        {
            this.prefix = prefix;
            this.targetSource = targetSource;
            this.targetListener = targetListener;
        }

        public void ValueChanged(ITable source, string key, object value, bool isNew)
        {
            if (key.StartsWith(prefix))
            {
                string relativeKey = key.Substring(prefix.Length);
                if (Contains(relativeKey, NetworkTable.PATH_SEPARATOR))
                    return;
                targetListener.ValueChanged(targetSource, relativeKey, value, isNew);
            }
        }

        private static bool Contains(string source, char target)
        {
            return source.Any(t => t == target);
        }
    }
}
