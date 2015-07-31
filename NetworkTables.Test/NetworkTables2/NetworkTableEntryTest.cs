using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using NetworkTables.NetworkTables2;
using NetworkTables.NetworkTables2.Connection;
using NetworkTables.NetworkTables2.Stream;
using NetworkTables.NetworkTables2.Type;
using NetworkTables.Test.Util;
using NUnit.Framework;
using Telerik.JustMock;

namespace NetworkTables.Test.NetworkTables2
{
    [TestFixture]
    public class NetworkTableEntryTest
    {
        [TestFixtureSetUp]
        public static void Init()
        {
            
        }

        [Test]
        public void TestSendValue()
        {
            NetworkTableEntryType type = Mock.Create<NetworkTableEntryType>();
            object value = "MyValue";
            NetworkTableEntry entry = new NetworkTableEntry((char) 0, "MyKey", (char) 0 , type, value);
            DataIOStream os = Mock.Create<DataIOStream>();


            entry.SendValue(os);

            Mock.Assert(() => type.SendValue(value, os), Occurs.Once());
        }

        [Test]
        public void TestToString()
        {
            NetworkTableEntry entry = NetworkTableEntryUtil.NewBooleanEntry("MyKey", false);
            Assert.AreEqual("Network Table Boolean entry: MyKey: 65535 - 0 - false", entry.ToString());
        }

        public class NStream : Socket
        {
            public NStream(SocketType socketType, ProtocolType protocolType) : base(socketType, protocolType)
            {
            }

            public NStream(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType) : base(addressFamily, socketType, protocolType)
            {
            }

            public NStream(SocketInformation socketInformation) : base(socketInformation)
            {
            }

            public new void Send(byte[] b)
            {
                
            }
        }
        //For some reason this is failing. Need to figure out
        /*
        [TestMethod]
        public void TestSend()
        {
            NetworkTableEntry entry = NetworkTableEntryUtil.NewBooleanEntry((char) 0, "MyBoolean", (char) 0, true);
            NetworkTableConnection connection = Mock.Create(() => new NetworkTableConnection(new SimpleIOStream(new DataIOStream(
                new NetworkStream(new NStream(SocketType.Dgram,ProtocolType.IP))),
                new DataIOStream(new NetworkStream(new NStream(SocketType.Dgram, ProtocolType.IP)))), new NetworkTableEntryTypeManager()));
            
            

            entry.Send(connection);

            Mock.Assert(() => connection.SendEntryAssignment(entry), Occurs.Once());
        }
        */

        [Test]
        public void TestConstructor()
        {
            NetworkTableEntry entry = new NetworkTableEntry((char)10, "MyNotBoolean", (char) 2, DefaultEntryTypes.STRING, "Test1");
            Assert.AreEqual((char)10, entry.GetId());
            Assert.AreEqual("MyNotBoolean", entry.name);
            Assert.AreEqual((char)2, entry.GetSequenceNumber());
            Assert.AreEqual(DefaultEntryTypes.STRING, entry.GetType());
            Assert.AreEqual("Test1", entry.GetValue());
        }

        [Test]
        public void TestPut()
        {
            NetworkTableEntry entry = NetworkTableEntryUtil.NewStringEntry((char)10, "MyString", (char)2, "Test1");
            Assert.IsTrue(entry.PutValue((char)3, "Test5"));
            Assert.AreEqual("Test5", entry.GetValue());
            Assert.AreEqual((char)3, entry.GetSequenceNumber());

            Assert.IsTrue(entry.PutValue((char)4, "Test2"));
            Assert.AreEqual("Test2", entry.GetValue());
            Assert.AreEqual((char)4, entry.GetSequenceNumber());

            Assert.IsFalse(entry.PutValue((char)3, "Test3"));
            Assert.AreEqual("Test2", entry.GetValue());
            Assert.AreEqual((char)4, entry.GetSequenceNumber());

            Assert.IsFalse(entry.PutValue((char)40000, "Test22"));
            Assert.AreEqual("Test2", entry.GetValue());
            Assert.AreEqual((char)4, entry.GetSequenceNumber());

            Assert.IsTrue(entry.PutValue((char)30000, "Test10"));
            Assert.AreEqual("Test10", entry.GetValue());
            Assert.AreEqual((char)30000, entry.GetSequenceNumber());

            Assert.IsTrue(entry.PutValue((char)40000, "Test23"));
            Assert.AreEqual("Test23", entry.GetValue());
            Assert.AreEqual((char)40000, entry.GetSequenceNumber());

            Assert.IsFalse(entry.PutValue((char)30000, "Test100"));
            Assert.AreEqual("Test23", entry.GetValue());
            Assert.AreEqual((char)40000, entry.GetSequenceNumber());

            Assert.IsTrue(entry.PutValue((char)0, "Test0"));
            Assert.AreEqual("Test0", entry.GetValue());
            Assert.AreEqual((char)0, entry.GetSequenceNumber());
        }

        [Test]
        public void TestSetWhenValid()
        {
            NetworkTableEntry entry = NetworkTableEntryUtil.NewStringEntry("MyString", "Test1");
            entry.SetId((char)100);
        }

        [Test]
        public void TestSetWhenNotValid()
        {
            NetworkTableEntry entry = NetworkTableEntryUtil.NewStringEntry((char)10, "MyString", (char)2, "Test1");
            try
            {
                entry.SetId((char) 100);
                Assert.Fail();
            }
            catch (InvalidOperationException e)
            {
                
            }
        }

        [Test]
        public void TestClearId()
        {
            NetworkTableEntry entry = NetworkTableEntryUtil.NewDoubleEntry("MyString", 203.2);
            entry.SetId((char)10);
            Assert.AreEqual((char)10, entry.GetId());
            entry.ClearId();
            Assert.AreEqual(NetworkTableEntry.UNKNOWN_ID, entry.GetId());
            entry.SetId((char)22);
            Assert.AreEqual((char)22, entry.GetId());
        }

        [Test]
        public void TestDirtyness()
        {
            NetworkTableEntry entry = NetworkTableEntryUtil.NewDoubleEntry("MyString", 203.2);
            Assert.AreEqual(false, entry.IsDirty());
            entry.MakeClean();
            Assert.AreEqual(false, entry.IsDirty());
            entry.MakeDirty();
            Assert.AreEqual(true, entry.IsDirty());
            entry.MakeClean();
            Assert.AreEqual(false, entry.IsDirty());
        }

        [Test]
        public void TestForcePut()
        {
            NetworkTableEntry entry = NetworkTableEntryUtil.NewStringEntry((char)10, "MyString", (char)2, "Test1");
            entry.ForcePut((char)3, "Test5");
            Assert.AreEqual("Test5", entry.GetValue());
            Assert.AreEqual((char)3, entry.GetSequenceNumber());

            entry.ForcePut((char)4, "Test2");
            Assert.AreEqual("Test2", entry.GetValue());
            Assert.AreEqual((char)4, entry.GetSequenceNumber());

            entry.ForcePut((char)3, "Test3");
            Assert.AreEqual("Test3", entry.GetValue());
            Assert.AreEqual((char)3, entry.GetSequenceNumber());

            entry.ForcePut((char)40000, "Test22");
            Assert.AreEqual("Test22", entry.GetValue());
            Assert.AreEqual((char)40000, entry.GetSequenceNumber());

            entry.ForcePut((char)30000, "Test10");
            Assert.AreEqual("Test10", entry.GetValue());
            Assert.AreEqual((char)30000, entry.GetSequenceNumber());
        }

        [Test]
        public void TestForcePutWithType()
        {
            NetworkTableEntry entry = NetworkTableEntryUtil.NewStringEntry((char)10, "MyString", (char)2, "Test1");
            entry.ForcePut((char)3, DefaultEntryTypes.BOOLEAN, true);
            Assert.AreEqual(DefaultEntryTypes.BOOLEAN, entry.GetType());
            Assert.AreEqual(true, entry.GetValue());
            Assert.AreEqual((char)3, entry.GetSequenceNumber());

            entry.ForcePut((char)4, DefaultEntryTypes.STRING, "HELLO");
            Assert.AreEqual("HELLO", entry.GetValue());
            Assert.AreEqual(DefaultEntryTypes.STRING, entry.GetType());
            Assert.AreEqual((char)4, entry.GetSequenceNumber());

            entry.ForcePut((char)3, DefaultEntryTypes.DOUBLE, 11.5);
            Assert.AreEqual(DefaultEntryTypes.DOUBLE, entry.GetType());
            Assert.AreEqual(11.5, entry.GetValue());
            Assert.AreEqual((char)3, entry.GetSequenceNumber());

            entry.ForcePut((char)3, DefaultEntryTypes.STRING, "HI");
            Assert.AreEqual(DefaultEntryTypes.STRING, entry.GetType());
            Assert.AreEqual("HI", entry.GetValue());
            Assert.AreEqual((char)3, entry.GetSequenceNumber());
        }

        [Test]
        public void TestFireListener()
        {
            NetworkTableEntry entry = NetworkTableEntryUtil.NewStringEntry((char)10, "MyString", (char)2, "Test1");
            AbstractNetworkTableEntryStore.TableListenerManager listenerManager =
                Mock.Create<AbstractNetworkTableEntryStore.TableListenerManager>();

            entry.FireListener(listenerManager);
            Mock.Assert(() => listenerManager.FireTableListeners("MyString", "Test1", true), Occurs.Once());

            entry.FireListener(listenerManager);
            Mock.Assert(() => listenerManager.FireTableListeners("MyString", "Test1", false), Occurs.Once());

            entry.ForcePut((char)0, "TEST3");
            entry.FireListener(listenerManager);

            Mock.Assert(() => listenerManager.FireTableListeners("MyString", "TEST3", false), Occurs.Once());
        }
    }
}
