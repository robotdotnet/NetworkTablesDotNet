using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using static NetworkTables.Logger;

namespace NetworkTables.TcpSockets
{
    internal class TCPConnector
    {
        private static int ResolveHostName(string hostName, out IPAddress[] addr)
        {
            try
            {
                
                var addressEntry = Dns.GetHostEntry(hostName);
                addr = addressEntry.AddressList;

            }
            catch (SocketException e)
            {
                addr = null;
                return e.NativeErrorCode;
            }
            return 0;
        }

        public static INetworkStream Connect(string server, int port, int timeout = 0)
        {
            IPAddress[] addr = null;
            if (ResolveHostName(server, out addr) != 0)
            {
                try
                {
                    addr = new IPAddress[1];
                    addr[0] = IPAddress.Parse(server);
                }
                catch (FormatException)
                {
                    Error($"could not resolve {server} address");
                    return null;
                }
            }

            //Create our client
            TcpClient client = new TcpClient();
            //TODO: Figure this out
            if (true)
            {
                try
                {
                    //Try to connect to client
                    client.Connect(addr, port);
                }
                catch (SocketException ex)
                {
                    Error($"Connect() to {server} port {port} failed: {ex.SocketErrorCode.ToString()}");
                    client.Close();
                    return null;
                }

                return new TCPStream(client.Client);
            }            

            client.Client.Blocking = false;

            try
            {
                client.Connect(addr, port);

                if (true)
                {
                    //Connected successfully
                    client.Client.Blocking = true;
                    return new TCPStream(client.Client);
                }
                else
                {
                    //Connection timed out.
                    Info($"Connect() to {server} port {port} timed out");
                    client.Close();
                    return null;
                }
            }
            catch (SocketException ex)
            {
                Error($"Connect() to {server} port {port} error {ex.NativeErrorCode} - {ex.SocketErrorCode.ToString()}");
                client.Close();
                return null;
            }



        }
    }
}
