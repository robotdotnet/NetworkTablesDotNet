using System;
using System.Runtime.Remoting.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetworkTables.NetworkTables2;
using Telerik.JustMock;

namespace NetworkTables.Test.NetworkTables2
{
    //TODO: Figure This Out
    [TestClass]
    public class TransactionDirtierTest
    {
        private static TransactionDirtier dirtier;

        private static OutgoingEntryReceiver receiver = Mock.Create<OutgoingEntryReceiver>();

        [ClassInitialize]
        public static void Init(TestContext ctx)
        {
            dirtier = new TransactionDirtier(receiver);
        }

        public static void Cleanup()
        {
            
        }

        [TestMethod]
        public void TestOutgoingCleanUpdate()
        {
            NetworkTableEntry entry = Mock.Create<NetworkTableEntry>();
            dirtier.OfferOutgoingUpdate(entry);



            //Mock.Arrange(() => entry.IsDirty()).Returns(false);
            //Mock.Assert(() => entry.MakeDirty(), Occurs.Once());
            //Mock.Assert(() => receiver.OfferOutgoingAssignment(entry), Occurs.Once());
        }
    }
}
