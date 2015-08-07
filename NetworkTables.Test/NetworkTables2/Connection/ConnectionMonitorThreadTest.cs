using NetworkTables.NetworkTables2.Connection;
using NUnit.Framework;

namespace NetworkTables.Test.NetworkTables2.Connection
{
    [TestFixture]
    public class ConnectionMonitorThreadTest
    {
        private static ConnectionMonitorThread thread;

        private static ConnectionAdapter adapter;
        private static NetworkTableConnection connection;

        [TestFixtureSetUp]
        public static void Init()
        {
            //adapter = Mock.Create<ConnectionAdapter>();
            //connection = Mock.Create<NetworkTableConnection>();
            //thread = new ConnectionMonitorThread(adapter, connection);
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
