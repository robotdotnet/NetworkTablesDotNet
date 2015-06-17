using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkTables.Tables;

namespace NetworkTables.NetworkTables
{
    public class NetworkTableConnectionListenerAdapter : IRemoteConnectionListener
    {
        private IRemoteConnectionListener targetListener;
        private IRemote targetSource;

        public NetworkTableConnectionListenerAdapter(IRemote targetSource, IRemoteConnectionListener targetListener)
        {
            this.targetSource = targetSource;
            this.targetListener = targetListener;
        }

        public void Connected(IRemote remote)
        {
            targetListener.Connected(targetSource);
        }
        public void Disconnected(IRemote remote)
        {
            targetListener.Disconnected(targetSource);
        }
    }
}
