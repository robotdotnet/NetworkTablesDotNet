using NetworkTables.TcpSockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables.Test
{
    internal class MockNetworkStream : INetworkStream
    {
        private byte[] m_buffer;
        public byte[] SendBuffer { get; private set; }

        public MockNetworkStream(byte[] buffer)
        {
            m_buffer = buffer;
        }

        public void Close()
        {
            //noop
        }

        public string GetPeerIP()
        {
            return "MockRemote";
        }

        public int GetPeerPort()
        {
            return 1234;
        }

        public int Receive(byte[] buffer, int pos, int len, ref NetworkStreamError err, int timeout = 0)
        {
            return 0;
        }

        public int Send(byte[] buffer, int pos, int len, ref NetworkStreamError error)
        {
            SendBuffer = new byte[len];
            Array.Copy(buffer, pos, SendBuffer, 0, len);
            return len;
        }

        public void SetNoDelay()
        {
            //Noop
        }

        public void Dispose()
        {
            //noop
        }
    }

    public partial class TestStorage
    {
    }
}
