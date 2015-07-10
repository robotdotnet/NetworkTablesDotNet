using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetworkTables.NetworkTables2.Connection;
using Telerik.JustMock;

namespace NetworkTables.Test.NetworkTables2.Connection
{
    [TestClass]
    public class ConnectionMonitorThreadTest
    {
        private static ConnectionMonitorThread thread;

        private static ConnectionAdapter adapter;
        private static NetworkTableConnection connection;

        [ClassInitialize]
        public static void Init(TestContext ctx)
        {
            adapter = Mock.Create<ConnectionAdapter>();
            connection = Mock.Create<NetworkTableConnection>();
            thread = new ConnectionMonitorThread(adapter, connection);
        }
        //These run a thread, and since it blocks, its not running correctly.
        /*
        [TestMethod]
        public static void TestSimpleRead()
        {
            thread.Run();
            
            Mock.Assert(() => connection.Read(adapter), Occurs.Once()); 

            
        }
        */
    }
}
