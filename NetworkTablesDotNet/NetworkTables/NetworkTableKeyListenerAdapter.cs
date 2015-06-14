using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkTablesDotNet.Tables;

namespace NetworkTablesDotNet.NetworkTables
{
    public class NetworkTableKeyListenerAdapter : ITableListener
    {
        private readonly ITableListener targetListener;
        private readonly NetworkTable targetSource;
        private readonly string relativeKey;
        private readonly string fullKey;

        public NetworkTableKeyListenerAdapter(string relativeKey, string fullKey, NetworkTable targetSource,
            ITableListener targetListener)
        {
            this.relativeKey = relativeKey;
            this.fullKey = fullKey;
            this.targetSource = targetSource;
            this.targetListener = targetListener;
        }


        public void ValueChanged(ITable source, string key, object value, bool isNew)
        {
            if (key.Equals(fullKey))
                targetListener.ValueChanged(targetSource, relativeKey, value, isNew);
        }
    }
}
