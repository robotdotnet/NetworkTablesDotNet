using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkTables.NTCore;

namespace NetworkTables
{
    public static class CoreConnections
    {
        public delegate void ConnectionListenerCallback(uint uid, bool connected, NT_ConnectionInfo connection);

        public static bool enabled = false;

        public static void EnablePrintOnConnectionChanged()
        {
        }

        public static void DisablePrintOnConnectionChanged()
        {
            
        }
    }
}
