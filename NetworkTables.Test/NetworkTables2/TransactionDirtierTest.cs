using System;
using NetworkTables.NetworkTables2;
using NetworkTables.NetworkTables2.Type;
using NUnit.Framework;

namespace NetworkTables.Test.NetworkTables2
{
    //TODO: Figure This Out
    [TestFixture]
    public class TransactionDirtierTest
    {
        private static TransactionDirtier dirtier;

        private static MockOutgoingEntryReceiver receiver;

        [TestFixtureSetUp]
        public static void Init()
        {
            receiver = new MockOutgoingEntryReceiver();
            dirtier = new TransactionDirtier(receiver);
        }

        public static void Cleanup()
        {
            
        }

        public void Setup()
        {
            receiver.OutgoingUpdateCount = 0;
            receiver.OutgoingAssignmentCount = 0;
        }

        [Test]
        public void TestOutgoingCleanUpdate()
        {
            NetworkTableEntry entry = new MockNetworkTableEntry();

            Assert.IsFalse(entry.IsDirty());
            dirtier.OfferOutgoingUpdate(entry);


            Assert.AreEqual(1, receiver.OutgoingUpdateCount);
            Assert.IsTrue(entry.IsDirty());
        }

        [Test]
        public void TestOutgoingCleanAssignment()
        {
            NetworkTableEntry entry = new MockNetworkTableEntry();

            Assert.IsFalse(entry.IsDirty());
            dirtier.OfferOutgoingAssignment(entry);

            Assert.AreEqual(1, receiver.OutgoingAssignmentCount);
            Assert.IsTrue(entry.IsDirty());


        }

        [Test]
        public void TestOutgoingDirtyAssignment()
        {
            NetworkTableEntry entry = new MockNetworkTableEntry();
            entry.MakeDirty();
            dirtier.OfferOutgoingAssignment(entry);

            Assert.AreEqual(1, receiver.OutgoingAssignmentCount);
            Assert.IsTrue(entry.IsDirty());
        }

        [Test]
        public void TestOutgoingDirtyUpdate()
        {
            NetworkTableEntry entry = new MockNetworkTableEntry();
            entry.MakeDirty();
            dirtier.OfferOutgoingUpdate(entry);

            Assert.AreEqual(1, receiver.OutgoingUpdateCount);
            Assert.IsTrue(entry.IsDirty());
        }

        public class MockNetworkTableEntry : NetworkTableEntry
        {
            public MockNetworkTableEntry() : base("bool", DefaultEntryTypes.BOOLEAN, true)
            {
            }

            public MockNetworkTableEntry(bool ds) : base('k', "test", (char)3, DefaultEntryTypes.BOOLEAN, true)
            {
            }
        }

        public class MockOutgoingEntryReceiver : OutgoingEntryReceiver
        {
            public int OutgoingAssignmentCount { get; set; } = 0;

            public int OutgoingUpdateCount { get; set; } = 0;

            public void OfferOutgoingAssignment(NetworkTableEntry entry)
            {
                OutgoingAssignmentCount++;
            }

            public void OfferOutgoingUpdate(NetworkTableEntry entry)
            {
                OutgoingUpdateCount++;
            }
        }
    }
}
