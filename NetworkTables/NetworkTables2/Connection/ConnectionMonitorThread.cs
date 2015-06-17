using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NetworkTables.NetworkTables2.Thread;

namespace NetworkTables.NetworkTables2.Connection
{
    public class ConnectionMonitorThread : PeriodicRunnable
    {
        private readonly ConnectionAdapter adapter;
        private readonly NetworkTableConnection connection;

        public ConnectionMonitorThread(ConnectionAdapter adapter, NetworkTableConnection connection)
        {
            this.adapter = adapter;
            this.connection = connection;
        }

        public void Run()
        {
            try
            {
                connection.Read(adapter);
            }
            catch (BadMessageException e)
            {
                adapter.BadMessage(e);
            }
            catch (SocketException e)
            {
                adapter.IOException(e);
            }
            catch (EndOfStreamException e)
            {
                adapter.IOException(e);
            }
            catch (IOException e)
            {
                adapter.IOException(e);
            }
        }
    }
}
