using System;
using System.Collections.Generic;
using NetworkTables.NetworkTables;
using NetworkTables.NetworkTables2.Client;
using NetworkTables.NetworkTables2.Stream;
using NetworkTables.NetworkTables2.Thread;
using NetworkTables.NetworkTables2.Type;
using NetworkTables.Tables;
using NUnit.Framework;

namespace NetworkTables.Test.NetworkTables
{
    public class IOClass : IOStreamFactory
    {
        public IOStream CreateStream()
        {
            return null;
        }
    }

    [TestFixture]
    public class NetworkTableListenerTest
    {
        private static NetworkTableClient client;
        private static  NetworkTableProvider provider;

        private static NetworkTableOld testTable1;
        static private NetworkTableOld testTable2;
        static private NetworkTableOld testTable3;
        static private NetworkTableOld testSubTable1;
        static private NetworkTableOld testSubTable2;
        static private NetworkTableOld testSubTable3;
        static private NetworkTableOld testSubTable4;

        [TestFixtureSetUp]
        public static void Init()
        {
            client = new NetworkTableClient(new IOClass());
            provider = new NetworkTableProvider(client);

            testTable1 = (NetworkTableOld)provider.GetTable("/test1");
            testTable1 = (NetworkTableOld)provider.GetTable("/test1");
            testTable2 = (NetworkTableOld)provider.GetTable("/test2");
            testSubTable1 = (NetworkTableOld)provider.GetTable("/test2/sub1");
            testSubTable2 = (NetworkTableOld)provider.GetTable("/test2/sub2");
            testTable3 = (NetworkTableOld)provider.GetTable("/test3");
            testSubTable3 = (NetworkTableOld)provider.GetTable("/test3/suba");
            testSubTable4 = (NetworkTableOld)provider.GetTable("/test3/suba/subb");
        }

        [TestFixtureTearDown]
        public static void Cleanup()
        {
            provider.Close();
        }

        public class MockTableListener : ITableListener
        {
            public struct ChangedStates
            {
                public ITable Source;
                public string Key;
                public object Value;
                public bool IsNew;
            }

            public List<ChangedStates> States = new List<ChangedStates>(); 

            public void ValueChanged(ITable source, string key, object value, bool isNew)
            {
                ChangedStates state = new ChangedStates
                {
                    Source = source,
                    Key = key,
                    Value = value,
                    IsNew = isNew,
                };

                States.Add(state);
            }

            public void AssertState(int count, ITable source, string key, object value, bool isNew)
            {
                Assert.AreEqual(source, States[count].Source);
                Assert.AreEqual(key, States[count].Key);
                Assert.AreEqual(value, States[count].Value);
                Assert.AreEqual(isNew, States[count].IsNew);
            }
        }

        

        [Test]
        public void KeyListenerImediateNotifyTest()
        {
            var listener1 = new MockTableListener();//Mock.Create<ITableListener>();


            testTable1.PutBoolean("MyKey1", true);
            testTable1.PutBoolean("MyKey1", false);
            testTable1.PutBoolean("MyKey2", true);
            testTable1.PutBoolean("MyKey4", false);

            

            testTable1.AddTableListener(listener1, true);

            Assert.AreEqual(3, listener1.States.Count);
            listener1.AssertState(0, testTable1, "MyKey1", false, true);
            listener1.AssertState(1, testTable1, "MyKey2", true, true);
            listener1.AssertState(2, testTable1, "MyKey4", false, true);

            listener1.States.Clear();

            testTable1.PutBoolean("MyKey", false);

            Assert.AreEqual(1, listener1.States.Count);
            listener1.AssertState(0, testTable1, "MyKey", false, true);

            listener1.States.Clear();

            testTable1.PutBoolean("MyKey1", true);

            Assert.AreEqual(1, listener1.States.Count);
            listener1.AssertState(0, testTable1, "MyKey1", true, false);

            listener1.States.Clear();

            testTable1.PutBoolean("MyKey1", false);

            Assert.AreEqual(1, listener1.States.Count);
            listener1.AssertState(0, testTable1, "MyKey1", false, false);

            listener1.States.Clear();

            testTable1.PutBoolean("MyKey4", true);

            Assert.AreEqual(1, listener1.States.Count);
            listener1.AssertState(0, testTable1, "MyKey4", true, false);
        }

        [Test]
        public void KeyListenerNotImediateNotifyTest()
        {
            var listener1 = new MockTableListener();


            testTable1.PutBoolean("MyKey1", true);
            testTable1.PutBoolean("MyKey1", false);
            testTable1.PutBoolean("MyKey2", true);
            testTable1.PutBoolean("MyKey4", false);

            testTable1.AddTableListener(listener1, false);

            Assert.AreEqual(0, listener1.States.Count);

            listener1.States.Clear();

            testTable1.PutBoolean("MyKey", false);

            Assert.AreEqual(0, listener1.States.Count);

            listener1.States.Clear();

            testTable1.PutBoolean("MyKey1", false);

            Assert.AreEqual(0, listener1.States.Count);

            listener1.States.Clear();

            testTable1.PutBoolean("MyKey1", true);

            Assert.AreEqual(1, listener1.States.Count);
            listener1.AssertState(0, testTable1, "MyKey1", true, false);

            listener1.States.Clear();

            testTable1.PutBoolean("MyKey4", true);

            Assert.AreEqual(1, listener1.States.Count);
            listener1.AssertState(0, testTable1, "MyKey4", true, false);
        }

        [Test]
        public void SubTableListenerTest()
        {
            var listener1 = new MockTableListener();

            testTable2.PutBoolean("MyKey1", true);
            testTable2.PutBoolean("MyKey2", true);
            testTable2.AddSubTableListener(listener1);
            testTable2.PutBoolean("MyKey1", false);
            testTable2.PutBoolean("MyKey4", false);

            testSubTable1.PutBoolean("MyKey1", false);

            Assert.AreEqual(1, listener1.States.Count);
            listener1.AssertState(0, testTable2, "sub1", testSubTable1, true);

            listener1.States.Clear();

            testSubTable1.PutBoolean("MyKey2", true);
            testSubTable1.PutBoolean("MyKey1", true);

            testSubTable2.PutBoolean("MyKey1", false);

            Assert.AreEqual(1, listener1.States.Count);
            listener1.AssertState(0, testTable2, "sub2", testSubTable2, true);
        }

        [Test]
        public void SubSubTableListenerTest()
        {
            var listener1 = new MockTableListener();
            var listener2 = new MockTableListener();

            testTable3.AddSubTableListener(listener1);
            testSubTable3.AddSubTableListener(listener1);
            testSubTable4.AddTableListener(listener1, true);

            testSubTable4.PutBoolean("MyKey1", false);

            Assert.AreEqual(3, listener1.States.Count);
            listener1.AssertState(0, testTable3, "suba", testSubTable3, true);
            listener1.AssertState(1, testSubTable3, "subb", testSubTable4, true);
            listener1.AssertState(2, testSubTable4, "MyKey1", false, true);

            listener1.States.Clear();

            testSubTable4.PutBoolean("MyKey1", true);

            Assert.AreEqual(1, listener1.States.Count);
            listener1.AssertState(0, testSubTable4, "MyKey1", true, false);

            listener1.States.Clear();


            testTable3.AddSubTableListener(listener2);
            testSubTable3.AddSubTableListener(listener2);
            testSubTable4.AddTableListener(listener2, true);

            Assert.AreEqual(3, listener2.States.Count);
            listener2.AssertState(0, testTable3, "suba", testSubTable3, true);
            listener2.AssertState(1, testSubTable3, "subb", testSubTable4, true);
            listener2.AssertState(2, testSubTable4, "MyKey1", true, true);
        }
    }
}
