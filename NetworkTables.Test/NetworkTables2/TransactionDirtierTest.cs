using NetworkTables.NetworkTables2;
using NUnit.Framework;
using Telerik.JustMock;

namespace NetworkTables.Test.NetworkTables2
{
    //TODO: Figure This Out
    [TestFixture]
    public class TransactionDirtierTest
    {
        private static TransactionDirtier dirtier;

        private static OutgoingEntryReceiver receiver = Mock.Create<OutgoingEntryReceiver>();

        [TestFixtureSetUp]
        public static void Init()
        {
            dirtier = new TransactionDirtier(receiver);
        }

        public static void Cleanup()
        {
            
        }

        [Test]
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
