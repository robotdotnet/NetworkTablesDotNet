using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables.NetworkTables2.Client
{
    public interface ClientConnectionListenerManager
    {
        /**
         * called when something is connected
         */
        void FireConnectedEvent();
        /**
         * called when something is disconnected
         */
        void FireDisconnectedEvent();
    }
}
