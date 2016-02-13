using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using static NetworkTables.Storage;

namespace NetworkTables.Test
{
    [TestFixture(true)]
    [TestFixture(false)]
    public class TestStorage
    {
        Entry tmpEntry;

        Storage storage;

        private struct OutgoingData
        {
            public Message msg;
            public NetworkConnection only;
            public NetworkConnection except;

            public OutgoingData(Message m, NetworkConnection o, NetworkConnection e)
            {
                msg = m;
                only = o;
                except = e;
            }
        }
        private bool m_server = false;

        public TestStorage(bool server)
        {
            m_server = server;
        }

        List<OutgoingData> outgoing = new List<OutgoingData>();

        internal Dictionary<string, Entry> Entries => storage.Entries;
        internal List<Entry> IdMap => storage.IdMap;

        internal Entry GetEntry(string name)
        {
            Entry i = null;
            if (Entries.TryGetValue(name, out i))
            {
                return i;
            }
            return tmpEntry;
        }

        internal void HookOutgoing()
        {
            storage.SetOutgoing(QueueOutgoing, m_server);
        }

        internal void QueueOutgoing(Message msg, NetworkConnection only, NetworkConnection except)
        {
            outgoing.Add(new OutgoingData(msg, only, except));
        }

        [SetUp]
        public void Clear()
        {
            tmpEntry = new Entry("foobar");
            storage = new Storage();
            outgoing.Clear();
        }

        internal void SetTestEmpty()
        {
            HookOutgoing();
        }

        internal void SetTestPopulateOne()
        {
            SetTestEmpty();
            storage.SetEntryTypeValue("foo", Value.MakeBoolean(true));
            outgoing.Clear();
        }

        internal void TestSetPopulated()
        {
            SetTestEmpty();
            storage.SetEntryTypeValue("foo", Value.MakeBoolean(true));
            storage.SetEntryTypeValue("foo2", Value.MakeDouble(0.0));
            storage.SetEntryTypeValue("bar", Value.MakeDouble(1.0));
            storage.SetEntryTypeValue("bar2", Value.MakeBoolean(false));
            outgoing.Clear();
        }

        [Test]
        public void TestStorageEmptyEntryInit()
        {
            SetTestEmpty();
            var entry = GetEntry("foo");
            Assert.That(entry.value, Is.Null);
            Assert.That(entry.flags, Is.EqualTo(EntryFlags.None));
            Assert.That(entry.name, Is.EqualTo("foobar"));
            Assert.That(entry.id, Is.EqualTo(0xffffu));
            Assert.That(new SequenceNumber() == entry.seqNum);
        }

        [Test]
        public void TestStorageEmptyGetEntryValueNotExist()
        {
            SetTestEmpty();
            Assert.That(storage.GetEntryValue("foo"), Is.Null);
            Assert.That(Entries, Is.Empty);
            Assert.That(IdMap, Is.Empty);
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void TestStorageEmptyGetEntyValueExist()
        {
            SetTestEmpty();
            var value = Value.MakeBoolean(true);
            storage.SetEntryTypeValue("foo", value);
            outgoing.Clear();
            Assert.That(value, Is.EqualTo(storage.GetEntryValue("foo")));
        }

        [Test]
        public void TestStorageEmptySetEntryTypeValueAssignNew()
        {
            SetTestEmpty();
            var value = Value.MakeBoolean(true);
            storage.SetEntryTypeValue("foo", value);
            Assert.That(value, Is.EqualTo(GetEntry("foo").value));
            if (m_server)
            {
                Assert.That(IdMap, Has.Count.EqualTo(1));
                Assert.That(value, Is.EqualTo(IdMap[0].value));
            }
            else
            {
                Assert.That(IdMap, Is.Empty);
            }

            Assert.That(outgoing, Has.Count.EqualTo(1));
            Assert.That(outgoing[0].only, Is.Null);
            Assert.That(outgoing[0].except, Is.Null);

            var msg = outgoing[0].msg;

            Assert.That(msg.Type(), Is.EqualTo(Message.MsgType.kEntryAssign));

            Assert.That(msg.Str(), Is.EqualTo("foo"));

            if (m_server)
            {
                Assert.That(msg.Id(), Is.EqualTo(0u));
            }
            else
            {
                Assert.That(msg.Id(), Is.EqualTo(0xffffu));
            }

            Assert.That(msg.SeqNumUid(), Is.EqualTo(1u));
            Assert.That(msg.Val(), Is.EqualTo(value));
            Assert.That(msg.Flags(), Is.EqualTo(0u));
        }

        [Test]
        public void TestStoragePopulateOneSetEntryTypeValueAssignChangeType()
        {
            SetTestPopulateOne();
            var value = Value.MakeDouble(0.0);
            storage.SetEntryTypeValue("foo", value);
            Assert.That(value, Is.EqualTo(GetEntry("foo").value));

            Assert.That(outgoing, Has.Count.EqualTo(1u));
            Assert.That(outgoing[0].only, Is.Null);
            Assert.That(outgoing[0].except, Is.Null);

            var msg = outgoing[0].msg;

            Assert.That(msg.Type(), Is.EqualTo(Message.MsgType.kEntryAssign));

            Assert.That(msg.Str(), Is.EqualTo("foo"));

            if (m_server)
            {
                Assert.That(msg.Id(), Is.EqualTo(0u));
            }
            else
            {
                Assert.That(msg.Id(), Is.EqualTo(0xffffu));
            }

            Assert.That(msg.SeqNumUid(), Is.EqualTo(2u));
            Assert.That(msg.Val(), Is.EqualTo(value));
            Assert.That(msg.Flags(), Is.EqualTo(0u));
        }


    }
}
