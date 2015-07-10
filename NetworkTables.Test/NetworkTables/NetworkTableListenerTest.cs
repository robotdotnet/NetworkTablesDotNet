using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetworkTables.NetworkTables;
using NetworkTables.NetworkTables2.Client;
using NetworkTables.NetworkTables2.Stream;
using NetworkTables.NetworkTables2.Thread;
using NetworkTables.NetworkTables2.Type;
using NetworkTables.Tables;
using Telerik.JustMock;

namespace NetworkTables.Test.NetworkTables
{
    public class IOClass : IOStreamFactory
    {
        public IOStream CreateStream()
        {
            return null;
        }
    }

    [TestClass]
    public class NetworkTableListenerTest
    {
        private static NetworkTableClient client;
        private static  NetworkTableProvider provider;

        private static NetworkTable testTable1;
        static private NetworkTable testTable2;
        static private NetworkTable testTable3;
        static private NetworkTable testSubTable1;
        static private NetworkTable testSubTable2;
        static private NetworkTable testSubTable3;
        static private NetworkTable testSubTable4;

        [ClassInitialize]
        public static void Init(TestContext ctx)
        {
            client = new NetworkTableClient(new IOClass());
            provider = new NetworkTableProvider(client);

            testTable1 = (NetworkTable)provider.GetTable("/test1");
            testTable1 = (NetworkTable)provider.GetTable("/test1");
            testTable2 = (NetworkTable)provider.GetTable("/test2");
            testSubTable1 = (NetworkTable)provider.GetTable("/test2/sub1");
            testSubTable2 = (NetworkTable)provider.GetTable("/test2/sub2");
            testTable3 = (NetworkTable)provider.GetTable("/test3");
            testSubTable3 = (NetworkTable)provider.GetTable("/test3/suba");
            testSubTable4 = (NetworkTable)provider.GetTable("/test3/suba/subb");
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            provider.Close();
        }

        

        [TestMethod]
        public void KeyListenerImediateNotifyTest()
        {
            var listener1 = Mock.Create<ITableListener>();


            testTable1.PutBoolean("MyKey1", true);
            testTable1.PutBoolean("MyKey1", false);
            testTable1.PutBoolean("MyKey2", true);
            testTable1.PutBoolean("MyKey4", false);

            

            testTable1.AddTableListener(listener1, true);

            Mock.Assert(() => listener1.ValueChanged(testTable1, "MyKey1", false, true), Occurs.Once());
            Mock.Assert(() => listener1.ValueChanged(testTable1, "MyKey2", true, true), Occurs.Once());
            Mock.Assert(() => listener1.ValueChanged(testTable1, "MyKey4", false, true), Occurs.Once());

            testTable1.PutBoolean("MyKey", false);

            Mock.Assert(() => listener1.ValueChanged(testTable1, "MyKey", false, true), Occurs.Once());
            
            testTable1.PutBoolean("MyKey1", true);
            Mock.Assert(() => listener1.ValueChanged(testTable1, "MyKey1", true, false), Occurs.Once());

            testTable1.PutBoolean("MyKey1", false);
            Mock.Assert(() => listener1.ValueChanged(testTable1, "MyKey1", false, false), Occurs.Once());

            testTable1.PutBoolean("MyKey4", true);
            Mock.Assert(() => listener1.ValueChanged(testTable1, "MyKey4", true, false), Occurs.Once());
        }

        [TestMethod]
        public void KeyListenerNotImediateNotifyTest()
        {
            var listener1 = Mock.Create<ITableListener>();


            testTable1.PutBoolean("MyKey1", true);
            testTable1.PutBoolean("MyKey1", false);
            testTable1.PutBoolean("MyKey2", true);
            testTable1.PutBoolean("MyKey4", false);

            testTable1.AddTableListener(listener1, false);

            Mock.Assert(() => listener1.ValueChanged(testTable1, "MyKey1", false, true), Occurs.Never());
            Mock.Assert(() => listener1.ValueChanged(testTable1, "MyKey2", true, true), Occurs.Never());
            Mock.Assert(() => listener1.ValueChanged(testTable1, "MyKey4", false, true), Occurs.Never());

            testTable1.PutBoolean("MyKey", false);

            //Mock.Assert(() => listener1.ValueChanged(testTable1, "MyKey", false, true), Occurs.Once());

            testTable1.PutBoolean("MyKey1", false);

            testTable1.PutBoolean("MyKey1", true);
            Mock.Assert(() => listener1.ValueChanged(testTable1, "MyKey1", true, false), Occurs.Once());

            testTable1.PutBoolean("MyKey4", true);
            Mock.Assert(() => listener1.ValueChanged(testTable1, "MyKey4", true, false), Occurs.Once());
        }

        [TestMethod]
        public void SubTableListenerTest()
        {
            var listener1 = Mock.Create<ITableListener>();

            testTable2.PutBoolean("MyKey1", true);
            testTable2.PutBoolean("MyKey2", true);
            testTable2.AddSubTableListener(listener1);
            testTable2.PutBoolean("MyKey1", false);
            testTable2.PutBoolean("MyKey4", false);

            testSubTable1.PutBoolean("MyKey1", false);
            Mock.Assert(() => listener1.ValueChanged(testTable2, "sub1", testSubTable1, true), Occurs.Once());

            testSubTable1.PutBoolean("MyKey2", true);
            testSubTable1.PutBoolean("MyKey1", true);

            testSubTable2.PutBoolean("MyKey1", false);
            Mock.Assert(() => listener1.ValueChanged(testTable2, "sub2", testSubTable2, true), Occurs.Once());
        }

        [TestMethod]
        public void SubSubTableListenerTest()
        {
            var listener1 = Mock.Create<ITableListener>();
            var listener2 = Mock.Create<ITableListener>();

            testTable3.AddSubTableListener(listener1);
            testSubTable3.AddSubTableListener(listener1);
            testSubTable4.AddTableListener(listener1, true);

            testSubTable4.PutBoolean("MyKey1", false);

            Mock.Assert(() => listener1.ValueChanged(testTable3, "suba", testSubTable3, true), Occurs.Once());
            Mock.Assert(() => listener1.ValueChanged(testSubTable3, "subb", testSubTable4, true), Occurs.Once());
            Mock.Assert(() => listener1.ValueChanged(testSubTable4, "MyKey1", false, true), Occurs.Once());

            testSubTable4.PutBoolean("MyKey1", true);
            Mock.Assert(() => listener1.ValueChanged(testSubTable4, "MyKey1", true, false), Occurs.Once());

            testTable3.AddSubTableListener(listener2);
            testSubTable3.AddSubTableListener(listener2);
            testSubTable4.AddTableListener(listener2, true);

            Mock.Assert(() => listener2.ValueChanged(testTable3, "suba", testSubTable3, true), Occurs.Once());
            Mock.Assert(() => listener2.ValueChanged(testSubTable3, "subb", testSubTable4, true), Occurs.Once());
            Mock.Assert(() => listener2.ValueChanged(testSubTable4, "MyKey1", true, true), Occurs.Once());
        }
    }
}
