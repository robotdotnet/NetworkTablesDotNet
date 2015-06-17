using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkTables.NetworkTables2;
using NetworkTables.NetworkTables2.Client;
using NetworkTables.NetworkTables2.Server;
using NetworkTables.NetworkTables2.Stream;
using NetworkTables.NetworkTables2.Thread;
using NetworkTables.NetworkTables2.Type;

namespace NetworkTables.NetworkTables
{

    public class NetworkTableMode
    {
        

        /*
        private string name;

        internal NetworkTableMode(string name)
        {
            this.name = name;
        }

        public override string ToString()
        {
            return name;
        }

        internal abstract NetworkTableNode CreateNode(string ipAddress, int port, NTThreadManager threadManger);
        */

        public delegate NetworkTableNode CreateNodeDelegate(string ipAddress, int port, NTThreadManager threadManger);

        
        public static NetworkTableNode CreateServerNode(string ipAddress, int port, NTThreadManager threadManager)
        {
            IOStreamProvider streamProvider = SocketStreams.NewStreamProvider(port);
            return new NetworkTableServer(streamProvider, new NetworkTableEntryTypeManager(), threadManager);
        }

        public static NetworkTableNode CreateClientNode(string ipAddres, int port, NTThreadManager threadManager)
        {
            if (ipAddres == null)
            {
                throw new ArgumentNullException(nameof(ipAddres), "IP address cannnot be null when in client mode.");
            }
            var streamFactory = SocketStreams.NewStreamFactory(ipAddres, port);
            NetworkTableClient client = new NetworkTableClient(streamFactory, new NetworkTableEntryTypeManager(), threadManager);
            client.Reconnect();
            return client;
        }
        
    }
}
