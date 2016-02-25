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

            Socket socket;

            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
            catch (SocketException)
            {
                Error("could not create socket");
                return null;
            }

            if (timeout == 0)
            {


                try
                {
                    socket.Connect(addr, port);
                }
                catch (SocketException ex)
                {
                    Error($"Connect() to {server} port {port} failed: {ex.SocketErrorCode}");
                    socket.Dispose();
                    return null;
                }
                return new TCPStream(socket);
            }

            //Connect with time limit
            try
            {
                var result = socket.BeginConnect(addr, port, null, null);
                if (!result.AsyncWaitHandle.WaitOne(timeout))
                {
                    try
                    {
                        socket.EndConnect(result);
                    }
                    catch (SocketException)
                    {
                    }
                }
                //Connected
                if (socket.Connected)
                {
                    return new TCPStream(socket);
                }
                Info($"Connect() to {server} port {port} timed out");
                socket.Dispose();
                return null;
            }
            catch (SocketException ex)
            {
                //Failed to connect
                Error($"Connect() to {server} port {port} error {ex.NativeErrorCode} - {ex.SocketErrorCode.ToString()}");
                socket.Dispose();
                return null;
            }
        }
    }
}
