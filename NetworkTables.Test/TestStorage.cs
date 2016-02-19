using System;
using System.Collections.Generic;
using System.IO;
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

        internal void SetTestPopulated()
        {
            SetTestEmpty();
            storage.SetEntryTypeValue("foo", Value.MakeBoolean(true));
            storage.SetEntryTypeValue("foo2", Value.MakeDouble(0.0));
            storage.SetEntryTypeValue("bar", Value.MakeDouble(1.0));
            storage.SetEntryTypeValue("bar2", Value.MakeBoolean(false));
            outgoing.Clear();
        }

        internal void TestSetPersistent()
        {
            string s = "\0\x03\x05\n";
            string specialLargeDigits = "\0\xAE\xFF\n";

            SetTestEmpty();
            storage.SetEntryTypeValue("boolean/true", Value.MakeBoolean(true));
            storage.SetEntryTypeValue("boolean/false", Value.MakeBoolean(false));
            storage.SetEntryTypeValue("double/neg", Value.MakeDouble(-1.5));
            storage.SetEntryTypeValue("double/zero", Value.MakeDouble(0.0));
            storage.SetEntryTypeValue("double/big", Value.MakeDouble(1.3e8));
            storage.SetEntryTypeValue("string/empty", Value.MakeString(""));
            storage.SetEntryTypeValue("string/normal", Value.MakeString("hello"));
            storage.SetEntryTypeValue("string/special",
                                      Value.MakeString(s));
            storage.SetEntryTypeValue("string/speciallarge",
                                      Value.MakeString(specialLargeDigits));
            storage.SetEntryTypeValue("string/paranth", Value.MakeString("M\"Q"));
            storage.SetEntryTypeValue("raw/empty", Value.MakeRaw());
            storage.SetEntryTypeValue("raw/normal", Value.MakeRaw(Encoding.UTF8.GetBytes("hello")));
            storage.SetEntryTypeValue("raw/special",
                                      Value.MakeRaw(Encoding.UTF8.GetBytes(s)));
            storage.SetEntryTypeValue("raw/speciallarge",
                                      Value.MakeRaw(Encoding.UTF8.GetBytes(specialLargeDigits)));
            storage.SetEntryTypeValue("booleanarr/empty",
                                      Value.MakeBooleanArray());
            storage.SetEntryTypeValue("booleanarr/one",
                                      Value.MakeBooleanArray(true));
            storage.SetEntryTypeValue("booleanarr/two",
                                      Value.MakeBooleanArray(true, false));
            storage.SetEntryTypeValue("doublearr/empty",
                                      Value.MakeDoubleArray());
            storage.SetEntryTypeValue("doublearr/one",
                                      Value.MakeDoubleArray(0.5));
            storage.SetEntryTypeValue(
                "doublearr/two",
                Value.MakeDoubleArray(0.5, -0.25));
            storage.SetEntryTypeValue(
                "stringarr/empty", Value.MakeStringArray());
            storage.SetEntryTypeValue(
                "stringarr/one",
                Value.MakeStringArray("hello"));
            storage.SetEntryTypeValue(
                "stringarr/two",
                Value.MakeStringArray("hello", "world\n"));
            storage.SetEntryTypeValue(s,
                                      Value.MakeBoolean(true));
            storage.SetEntryTypeValue(specialLargeDigits,
                                      Value.MakeBoolean(false));
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

        [Test]
        public void TestStoragePopulateOneSetEntryTypeEqualValue()
        {
            SetTestPopulateOne();
            var value = Value.MakeBoolean(true);
            storage.SetEntryTypeValue("foo", value);
            Assert.That(value, Is.EqualTo(GetEntry("foo").value));
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void TestStoragePopulatedSetEntryTypeValueDifferentValue()
        {
            SetTestPopulated();
            var value = Value.MakeDouble(1.0);
            storage.SetEntryTypeValue("foo2", value);
            Assert.That(value, Is.EqualTo(GetEntry("foo2").value));

            if (m_server)
            {
                Assert.That(outgoing, Has.Count.EqualTo(1));
                Assert.That(outgoing[0].only, Is.Null);
                Assert.That(outgoing[0].except, Is.Null);
                var msg = outgoing[0].msg;
                Assert.That(msg.Type(), Is.EqualTo(Message.MsgType.kEntryUpdate));
                Assert.That(msg.Id(), Is.EqualTo(1));
                Assert.That(msg.SeqNumUid(), Is.EqualTo(2));
                Assert.That(msg.Val(), Is.EqualTo(value));
            }
            else
            {
                Assert.That(outgoing, Is.Empty);
                Assert.That(GetEntry("foo2").seqNum.Value(), Is.EqualTo(2));
            }

        }

        [Test]
        public void TestStorageEmptySetEntryTypeValueEmptyName()
        {
            SetTestEmpty();
            var value = Value.MakeBoolean(true);
            storage.SetEntryTypeValue("", value);
            Assert.That(Entries, Is.Empty);
            Assert.That(IdMap, Is.Empty);
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void TestStorageEmptySetEntryTypeValueNullName()
        {
            SetTestEmpty();
            var value = Value.MakeBoolean(true);
            storage.SetEntryTypeValue(null, value);
            Assert.That(Entries, Is.Empty);
            Assert.That(IdMap, Is.Empty);
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void TestStorageEmptySetEntryValueEmptyValue()
        {
            SetTestEmpty();
            storage.SetEntryTypeValue(null, null);
            Assert.That(Entries, Is.Empty);
            Assert.That(IdMap, Is.Empty);
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void TestStorageEmptySetEntryValueAssignNew()
        {
            SetTestEmpty();
            var value = Value.MakeBoolean(true);
            Assert.That(storage.SetEntryValue("foo", value), Is.True);
            Assert.That(value, Is.EqualTo(GetEntry("foo").value));

            Assert.That(outgoing, Has.Count.EqualTo(1));
            Assert.That(outgoing[0].only, Is.Null);
            Assert.That(outgoing[0].except, Is.Null);
            var msg = outgoing[0].msg;
            Assert.That(msg.Type(), Is.EqualTo(Message.MsgType.kEntryAssign));
            Assert.That(msg.Str(), Is.EqualTo("foo"));
            if (m_server)
            {
                Assert.That(msg.Id(), Is.EqualTo(0));//Assigned as server
            }
            else
            {
                Assert.That(msg.Id(), Is.EqualTo(0xffff));
            }
            Assert.That(msg.SeqNumUid(), Is.EqualTo(0));
            Assert.That(msg.Val(), Is.EqualTo(value));
            Assert.That(msg.Flags(), Is.EqualTo(0));
        }

        [Test]
        public void TestStoragePopulateOneSetEntryValueAssignTypeChange()
        {
            //Update with different type reuslts in error and no message
            SetTestPopulateOne();
            var value = Value.MakeDouble(0.0);
            Assert.That(storage.SetEntryValue("foo", value), Is.False);
            var entry = GetEntry("foo");
            Assert.That(entry.value, Is.Not.EqualTo(value));
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void TestStoragePopulatedSetEntryValueDifferentValue()
        {
            SetTestPopulated();
            var value = Value.MakeDouble(1.0);
            Assert.That(storage.SetEntryValue("foo2", value), Is.True);
            var entry = GetEntry("foo2");
            Assert.That(entry.value, Is.EqualTo(value));

            if (m_server)
            {
                Assert.That(outgoing, Has.Count.EqualTo(1));
                Assert.That(outgoing[0].only, Is.Null);
                Assert.That(outgoing[0].except, Is.Null);
                var msg = outgoing[0].msg;
                Assert.That(msg.Type(), Is.EqualTo(Message.MsgType.kEntryUpdate));
                Assert.That(msg.Id(), Is.EqualTo(1));
                Assert.That(msg.SeqNumUid(), Is.EqualTo(2));
                Assert.That(msg.Val(), Is.EqualTo(value));
            }
            else
            {
                Assert.That(outgoing, Is.Empty);
                Assert.That(GetEntry("foo2").seqNum.Value(), Is.EqualTo(2));
            }
        }

        [Test]
        public void TestStorageEmptySetEntryValueEmptyName()
        {
            SetTestEmpty();
            var value = Value.MakeBoolean(true);
            Assert.That(storage.SetEntryValue("", value), Is.True);
            Assert.That(Entries, Is.Empty);
            Assert.That(IdMap, Is.Empty);
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void TestStorageEmptySetEntryValueNullName()
        {
            SetTestEmpty();
            var value = Value.MakeBoolean(true);
            Assert.That(storage.SetEntryValue(null, value), Is.True);
            Assert.That(Entries, Is.Empty);
            Assert.That(IdMap, Is.Empty);
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void TestStorageEmptySetEntryFlagsNew()
        {
            SetTestEmpty();
            storage.SetEntryFlags("foo", 0);
            Assert.That(Entries, Is.Empty);
            Assert.That(IdMap, Is.Empty);
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void TestStoragePopulateOneSetEntryFlagsEqualValue()
        {
            SetTestPopulateOne();
            //update with same value, no update message is issued
            storage.SetEntryFlags("foo", 0);
            var entry = GetEntry("foo");
            Assert.That(entry.flags, Is.EqualTo(EntryFlags.None));
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void TestStoragePopulatedSetEntryFlagsDifferentValue()
        {
            SetTestPopulated();
            storage.SetEntryFlags("foo2", EntryFlags.Persistent);
            Assert.That(GetEntry("foo2").flags, Is.EqualTo(EntryFlags.Persistent));

            if (m_server)
            {
                Assert.That(outgoing, Has.Count.EqualTo(1));
                Assert.That(outgoing[0].only, Is.Null);
                Assert.That(outgoing[0].except, Is.Null);
                var msg = outgoing[0].msg;
                Assert.That(msg.Type(), Is.EqualTo(Message.MsgType.kFlagsUpdate));
                Assert.That(msg.Id(), Is.EqualTo(1));
                Assert.That(msg.Flags(), Is.EqualTo(1));
            }
            else
            {
                Assert.That(outgoing, Is.Empty);
            }
        }

        [Test]
        public void TestStorageEmptySetEntryFlagsEmptyName()
        {
            SetTestEmpty();
            storage.SetEntryFlags("", EntryFlags.None);
            Assert.That(Entries, Is.Empty);
            Assert.That(IdMap, Is.Empty);
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void TestStorageEmptySetEntryFlagsNullName()
        {
            SetTestEmpty();
            storage.SetEntryFlags(null, EntryFlags.None);
            Assert.That(Entries, Is.Empty);
            Assert.That(IdMap, Is.Empty);
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void TestStorageEmptyGetEntryFlagsNotExist()
        {
            SetTestEmpty();
            Assert.That(storage.GetEntryFlags("foo"), Is.EqualTo(EntryFlags.None));
            Assert.That(Entries, Is.Empty);
            Assert.That(IdMap, Is.Empty);
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void TestStoragePopulateOneGetEntryFlagsExist()
        {
            SetTestPopulateOne();
            storage.SetEntryFlags("foo", EntryFlags.Persistent);
            outgoing.Clear();
            Assert.That(storage.GetEntryFlags("foo"), Is.EqualTo(EntryFlags.Persistent));
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void TestStorageEmptyDeleteEntryNotExist()
        {
            SetTestEmpty();
            storage.DeleteEntry("foo");
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void TestStoragePopulatedDeleteEntryExist()
        {
            SetTestPopulated();
            storage.DeleteEntry("foo2");
            Assert.That(Entries.ContainsKey("foo2"), Is.False);

            if (m_server)
            {
                Assert.That(IdMap, Has.Count.GreaterThanOrEqualTo(2));
                Assert.That(IdMap[1], Is.Null);
            }

            if (m_server)
            {

                Assert.That(outgoing, Has.Count.EqualTo(1));
                Assert.That(outgoing[0].only, Is.Null);
                Assert.That(outgoing[0].except, Is.Null);
                var msg = outgoing[0].msg;
                Assert.That(msg.Type(), Is.EqualTo(Message.MsgType.kEntryDelete));
                Assert.That(msg.Id(), Is.EqualTo(1));
            }
            else
            {
                Assert.That(outgoing, Is.Empty);
            }
        }

        [Test]
        public void TestStorageEmptyDeleteAllEntriesEmpty()
        {
            SetTestEmpty();
            storage.DeleteAllEntries();
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void TestStoragePopulatedDeleteAllEntries()
        {
            SetTestPopulated();
            storage.DeleteAllEntries();
            Assert.That(Entries, Is.Empty);

            Assert.That(outgoing, Has.Count.EqualTo(1));
            Assert.That(outgoing[0].only, Is.Null);
            Assert.That(outgoing[0].except, Is.Null);
            var msg = outgoing[0].msg;
            Assert.That(msg.Type(), Is.EqualTo(Message.MsgType.kClearEntries));
        }

        [Test]
        public void TestStoragePopulatedGetEntryInfoAll()
        {
            SetTestPopulated();
            var info = storage.GetEntryInfo("", 0);
            Assert.That(info, Has.Count.EqualTo(4));
        }

        [Test]
        public void TestStoragePopulatedGetEntryInfoAllNullPrefix()
        {
            SetTestPopulated();
            var info = storage.GetEntryInfo(null, 0);
            Assert.That(info, Has.Count.EqualTo(4));
        }

        [Test]
        public void TestStoragePopulatedGetEntryInfoPrefix()
        {
            SetTestPopulated();
            var info = storage.GetEntryInfo("foo", 0);
            Assert.That(info, Has.Count.EqualTo(2));

            if (info[0].Name == "foo")
            {
                Assert.That(info[0].Type, Is.EqualTo(NtType.Boolean));
                Assert.That(info[1].Name, Is.EqualTo("foo2"));
                Assert.That(info[1].Type, Is.EqualTo(NtType.Double));
            }
            else
            {
                Assert.That(info[0].Name, Is.EqualTo("foo2"));
                Assert.That(info[0].Type, Is.EqualTo(NtType.Double));
                Assert.That(info[1].Name, Is.EqualTo("foo"));
                Assert.That(info[1].Type, Is.EqualTo(NtType.Boolean));
            }
        }

        [Test]
        public void TestStoragePopulatedGetEntryInfoTypes()
        {
            SetTestPopulated();

            var info = storage.GetEntryInfo("", NtType.Double);
            Assert.That(info, Has.Count.EqualTo(2));
            Assert.That(info[0].Type, Is.EqualTo(NtType.Double));
            Assert.That(info[1].Type, Is.EqualTo(NtType.Double));

            if (info[0].Name == "foo2")
            {
                Assert.That(info[1].Name, Is.EqualTo("bar"));
            }
            else
            {
                Assert.That(info[0].Name, Is.EqualTo("bar"));
                Assert.That(info[1].Name, Is.EqualTo("foo2"));
            }
        }

        [Test]
        public void TestStoragePopulatedGetEntryInfoPrefixTypes()
        {
            SetTestPopulated();
            var info = storage.GetEntryInfo("bar", NtType.Boolean);
            Assert.That(info, Has.Count.EqualTo(1));
            Assert.That(info[0].Name, Is.EqualTo("bar2"));
            Assert.That(info[0].Type, Is.EqualTo(NtType.Boolean));
        }

        [Test]
        public void TestStoragePersistentSavePersistentEmpty()
        {
            TestSetPersistent();
            using (var s = new MemoryStream())
            {
                storage.SavePersistent(s, false);
                s.Position = 0;
                using (var r = new StreamReader(s))
                {
                    Assert.That(r.ReadToEnd(), Is.EqualTo("[NetworkTables Storage 3.0]\n"));
                }
            }
        }

        [Test]
        public void TestStoragePersistentSavePersistent()
        {
            TestSetPersistent();
            foreach (var entry in Entries)
            {
                entry.Value.flags = EntryFlags.Persistent;
            }

            using (var s = new MemoryStream())
            {
                storage.SavePersistent(s, false);
                s.Position = 0;
                string o = "";
                using (var r = new StreamReader(s))
                {
                    o = r.ReadToEnd();
                }
                string line, rem;
                line = String.Empty;
                rem = o;
                string[] split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("[NetworkTables Storage 3.0]"));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("boolean \"\\x00\\x03\\x05\\n\"=true"));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("boolean \"\\x00\\xAE\\xFF\\n\"=false"));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("boolean \"boolean/false\"=false"));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("boolean \"boolean/true\"=true"));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("array boolean \"booleanarr/empty\"="));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("array boolean \"booleanarr/one\"=true"));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("array boolean \"booleanarr/two\"=true,false"));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("double \"double/big\"=130000000"));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("double \"double/neg\"=-1.5"));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("double \"double/zero\"=0"));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("array double \"doublearr/empty\"="));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("array double \"doublearr/one\"=0.5"));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("array double \"doublearr/two\"=0.5,-0.25"));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("raw \"raw/empty\"="));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("raw \"raw/normal\"=aGVsbG8="));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("raw \"raw/special\"=AAMFCg=="));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("raw \"raw/speciallarge\"=AMKuw78K"));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("string \"string/empty\"=\"\""));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("string \"string/normal\"=\"hello\""));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("string \"string/paranth\"=\"M\\\"Q\""));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("string \"string/special\"=\"\\x00\\x03\\x05\\n\""));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("string \"string/speciallarge\"=\"\\x00\\xAE\\xFF\\n\""));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("array string \"stringarr/empty\"="));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("array string \"stringarr/one\"=\"hello\""));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("array string \"stringarr/two\"=\"hello\",\"world\\n\""));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                Assert.That(line, Is.EqualTo(""));
            }
        }

        [Test]
        public void TestStorageEmptyLoadPersistentBadHeader()
        {
            SetTestEmpty();
            int lastLine = -1;
            string lastString = String.Empty;



            Action<int, string> warn_func = (int line, string msg) =>
            {
                lastLine = line;
                lastString = msg;
            };

            using (MemoryStream stream = new MemoryStream())
            {
                Assert.That(storage.LoadPersistent(stream, warn_func), Is.False);
                Assert.That(lastLine, Is.EqualTo(1));
                Assert.That(lastString, Is.EqualTo("header line mismatch, ignoring rest of file"));
            }

            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes("[NetworkTables")))
            {
                Assert.That(storage.LoadPersistent(stream, warn_func), Is.False);
                Assert.That(lastLine, Is.EqualTo(1));
                Assert.That(lastString, Is.EqualTo("header line mismatch, ignoring rest of file"));
            }
            Assert.That(Entries, Is.Empty);
            Assert.That(IdMap, Is.Empty);
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void TestStorageEmptyLoadPersistentCommentHeader()
        {
            SetTestEmpty();
            int lastLine = -1;
            string lastString = String.Empty;

            Action<int, string> warn_func = (int line, string msg) =>
            {
                lastLine = line;
                lastString = msg;
            };

            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes("\n; comment\n# comment\n[NetworkTables Storage 3.0]\n")))
            {
                Assert.That(storage.LoadPersistent(stream, warn_func), Is.True);
                Assert.That(lastLine, Is.EqualTo(-1));
                Assert.That(lastString, Is.Empty);
            }
            Assert.That(Entries, Is.Empty);
            Assert.That(IdMap, Is.Empty);
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void TestStorageEmptyLoadPersistentEmptyName()
        {
            SetTestEmpty();
            int lastLine = -1;
            string lastString = String.Empty;

            Action<int, string> warn_func = (int line, string msg) =>
            {
                lastLine = line;
                lastString = msg;
            };

            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes("[NetworkTables Storage 3.0]\nboolean \"\"=true\n")))
            {
                Assert.That(storage.LoadPersistent(stream, warn_func), Is.True);
                Assert.That(lastLine, Is.EqualTo(-1));
                Assert.That(lastString, Is.Empty);
            }
            Assert.That(Entries, Is.Empty);
            Assert.That(IdMap, Is.Empty);
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void TestStorageEmptyLoadPersistentAssign()
        {
            SetTestEmpty();
            int lastLine = -1;
            string lastString = String.Empty;

            Action<int, string> warn_func = (int line, string mg) =>
            {
                lastLine = line;
                lastString = mg;
            };

            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes("[NetworkTables Storage 3.0]\nboolean \"foo\"=true\n")))
            {
                Assert.That(storage.LoadPersistent(stream, warn_func), Is.True);
                Assert.That(lastLine, Is.EqualTo(-1));
                Assert.That(lastString, Is.Empty);
            }
            var entry = GetEntry("foo");
            Assert.That(Value.MakeBoolean(true) == entry.value);
            Assert.That(entry.flags, Is.EqualTo(EntryFlags.Persistent));

            Assert.That(outgoing, Has.Count.EqualTo(1));
            Assert.That(outgoing[0].only, Is.Null);
            Assert.That(outgoing[0].except, Is.Null);
            var msg = outgoing[0].msg;
            Assert.That(msg.Type(), Is.EqualTo(Message.MsgType.kEntryAssign));
            Assert.That(msg.Str(), Is.EqualTo("foo"));
            if (m_server)
            {
                Assert.That(msg.Id(), Is.EqualTo(0));
            }
            else
            {
                Assert.That(msg.Id(), Is.EqualTo(0xffff));
            }
            Assert.That(msg.SeqNumUid(), Is.EqualTo(1));
            Assert.That(Value.MakeBoolean(true) == msg.Val());

            Assert.That(msg.Flags(), Is.EqualTo((uint)EntryFlags.Persistent));
        }

        [Test]
        public void TestStoragePopulatedLoadPersistentUpdateFlags()
        {
            SetTestPopulated();
            int lastLine = -1;
            string lastString = String.Empty;

            Action<int, string> warn_func = (int line, string mg) =>
            {
                lastLine = line;
                lastString = mg;
            };

            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes("[NetworkTables Storage 3.0]\ndouble \"foo2\"=0.0\n")))
            {
                Assert.That(storage.LoadPersistent(stream, warn_func), Is.True);
                Assert.That(lastLine, Is.EqualTo(-1));
                Assert.That(lastString, Is.Empty);
            }
            var entry = GetEntry("foo2");
            Assert.That(Value.MakeDouble(0.0) == entry.value);
            Assert.That(entry.flags, Is.EqualTo(EntryFlags.Persistent));

            if (m_server)
            {
                Assert.That(outgoing, Has.Count.EqualTo(1));
                Assert.That(outgoing[0].only, Is.Null);
                Assert.That(outgoing[0].except, Is.Null);
                var msg = outgoing[0].msg;
                Assert.That(msg.Type(), Is.EqualTo(Message.MsgType.kFlagsUpdate));
                Assert.That(msg.Id(), Is.EqualTo(1));
                Assert.That(msg.Flags(), Is.EqualTo((uint)EntryFlags.Persistent));
            }
            else
            {
                Assert.That(outgoing, Is.Empty);
            }
        }

        [Test]
        public void TestStoragePopulatedLoadPersistentUpdateValue()
        {
            SetTestPopulated();
            int lastLine = -1;
            string lastString = String.Empty;

            Action<int, string> warn_func = (int line, string mg) =>
            {
                lastLine = line;
                lastString = mg;
            };

            GetEntry("foo2").flags = EntryFlags.Persistent;

            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes("[NetworkTables Storage 3.0]\ndouble \"foo2\"=1.0\n")))
            {
                Assert.That(storage.LoadPersistent(stream, warn_func), Is.True);
                Assert.That(lastLine, Is.EqualTo(-1));
                Assert.That(lastString, Is.Empty);
            }
            var entry = GetEntry("foo2");
            Assert.That(Value.MakeDouble(1.0) == entry.value);
            Assert.That(entry.flags, Is.EqualTo(EntryFlags.Persistent));

            if (m_server)
            {
                Assert.That(outgoing, Has.Count.EqualTo(1));
                Assert.That(outgoing[0].only, Is.Null);
                Assert.That(outgoing[0].except, Is.Null);
                var msg = outgoing[0].msg;
                Assert.That(msg.Type(), Is.EqualTo(Message.MsgType.kEntryUpdate));
                Assert.That(msg.Id(), Is.EqualTo(1));
                Assert.That(msg.SeqNumUid(), Is.EqualTo(2));
                Assert.That(Value.MakeDouble(1.0) == msg.Val());
            }
            else
            {
                Assert.That(outgoing, Is.Empty);
                Assert.That(GetEntry("foo2").seqNum.Value(), Is.EqualTo(2));
            }
        }

        [Test]
        public void TestStoragePopulatedLoadPersistentUpdateValueFlags()
        {
            SetTestPopulated();
            int lastLine = -1;
            string lastString = String.Empty;

            Action<int, string> warn_func = (int line, string mg) =>
            {
                lastLine = line;
                lastString = mg;
            };

            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes("[NetworkTables Storage 3.0]\ndouble \"foo2\"=1.0\n")))
            {
                Assert.That(storage.LoadPersistent(stream, warn_func), Is.True);
                Assert.That(lastLine, Is.EqualTo(-1));
                Assert.That(lastString, Is.Empty);
            }
            var entry = GetEntry("foo2");
            Assert.That(Value.MakeDouble(1.0) == entry.value);
            Assert.That(entry.flags, Is.EqualTo(EntryFlags.Persistent));

            if (m_server)
            {
                Assert.That(outgoing, Has.Count.EqualTo(2));
                Assert.That(outgoing[0].only, Is.Null);
                Assert.That(outgoing[0].except, Is.Null);
                var msg = outgoing[0].msg;
                Assert.That(msg.Type(), Is.EqualTo(Message.MsgType.kEntryUpdate));
                Assert.That(msg.Id(), Is.EqualTo(1));
                Assert.That(msg.SeqNumUid(), Is.EqualTo(2));
                Assert.That(Value.MakeDouble(1.0) == msg.Val());

                Assert.That(outgoing[1].only, Is.Null);
                Assert.That(outgoing[1].except, Is.Null);
                msg = outgoing[1].msg;
                Assert.That(msg.Type(), Is.EqualTo(Message.MsgType.kFlagsUpdate));
                Assert.That(msg.Id(), Is.EqualTo(1));
                Assert.That(msg.Flags(), Is.EqualTo((uint)EntryFlags.Persistent));
            }
            else
            {
                Assert.That(outgoing, Is.Empty);
                Assert.That(GetEntry("foo2").seqNum.Value(), Is.EqualTo(2));
            }
        }

        [Test]
        public void TestStorageEmptyLoadPersistent()
        {
            SetTestEmpty();

            int lastLine = -1;
            string lastString = String.Empty;

            Action<int, string> warn_func = (int line, string mg) =>
            {
                lastLine = line;
                lastString = mg;
            };

            string specialLargeDigits = "\0\xAE\xFF\n";

            string i = "[NetworkTables Storage 3.0]\n";
            i += "boolean \"\\x00\\x03\\x05\\n\"=true\n";
            i += "boolean \"\\x00\\xAE\\xFF\\n\"=false\n";
            i += "boolean \"boolean/false\"=false\n";
            i += "boolean \"boolean/true\"=true\n";
            i += "array boolean \"booleanarr/empty\"=\n";
            i += "array boolean \"booleanarr/one\"=true\n";
            i += "array boolean \"booleanarr/two\"=true,false\n";
            i += "double \"double/big\"=1.3e+08\n";
            i += "double \"double/neg\"=-1.5\n";
            i += "double \"double/zero\"=0\n";
            i += "array double \"doublearr/empty\"=\n";
            i += "array double \"doublearr/one\"=0.5\n";
            i += "array double \"doublearr/two\"=0.5,-0.25\n";
            i += "raw \"raw/empty\"=\n";
            i += "raw \"raw/normal\"=aGVsbG8=\n";
            i += "raw \"raw/special\"=AAMFCg==\n";
            i += "raw \"raw/speciallarge\"=AMKuw78K\n";
            i += "string \"string/empty\"=\"\"\n";
            i += "string \"string/normal\"=\"hello\"\n";
            i += "string \"string/special\"=\"\\x00\\x03\\x05\\n\"\n";
            i += "string \"string/speciallarge\"=\"\\x00\\xAE\\xFF\\n\"\n";
            i += "string \"string/paranth\"=\"M\\\"Q\"\n";
            i += "array string \"stringarr/empty\"=\n";
            i += "array string \"stringarr/one\"=\"hello\"\n";
            i += "array string \"stringarr/two\"=\"hello\",\"world\\n\"\n";

            using (MemoryStream iss = new MemoryStream())
            {
                StreamWriter writer = new StreamWriter(iss);
                writer.Write(i);
                writer.Flush();
                iss.Position = 0;
                Assert.That(storage.LoadPersistent(iss, warn_func), Is.True);
            }
            Assert.That(Entries, Has.Count.EqualTo(25));
            Assert.That(outgoing, Has.Count.EqualTo(25));

            string s = "\0\x03\x05\n";

            Assert.That(Value.MakeBoolean(true) == storage.GetEntryValue("boolean/true"));
            Assert.That(Value.MakeBoolean(false) == storage.GetEntryValue("boolean/false"));
            Assert.That(Value.MakeDouble(-1.5) == storage.GetEntryValue("double/neg"));
            Assert.That(Value.MakeDouble(0.0) == storage.GetEntryValue("double/zero"));
            Assert.That(Value.MakeDouble(1.3e8) == storage.GetEntryValue("double/big"));
            Assert.That(Value.MakeString("") == storage.GetEntryValue("string/empty"));
            Assert.That(Value.MakeString("hello") == storage.GetEntryValue("string/normal"));
            Assert.That(Value.MakeString(s) == storage.GetEntryValue("string/special"));
            Assert.That(Value.MakeString(specialLargeDigits) == storage.GetEntryValue("string/speciallarge"));
            Assert.That(Value.MakeString("M\"Q") == storage.GetEntryValue("string/paranth"));
            Assert.That(Value.MakeRaw() == storage.GetEntryValue("raw/empty"));
            Assert.That(Value.MakeRaw(Encoding.UTF8.GetBytes("hello")) == storage.GetEntryValue("raw/normal"));
            Assert.That(Value.MakeRaw(Encoding.UTF8.GetBytes(s)) == storage.GetEntryValue("raw/special"));
            Assert.That(Value.MakeRaw(Encoding.UTF8.GetBytes(specialLargeDigits)) == storage.GetEntryValue("raw/speciallarge"));
            Assert.That(Value.MakeBooleanArray() == storage.GetEntryValue("booleanarr/empty"));
            Assert.That(Value.MakeBooleanArray(true) == storage.GetEntryValue("booleanarr/one"));
            Assert.That(Value.MakeBooleanArray(true, false) == storage.GetEntryValue("booleanarr/two"));
            Assert.That(Value.MakeDoubleArray() == storage.GetEntryValue("doublearr/empty"));
            Assert.That(Value.MakeDoubleArray(0.5) == storage.GetEntryValue("doublearr/one"));
            Assert.That(Value.MakeDoubleArray(0.5, -0.25) == storage.GetEntryValue("doublearr/two"));
            Assert.That(Value.MakeStringArray() == storage.GetEntryValue("stringarr/empty"));
            Assert.That(Value.MakeStringArray("hello") == storage.GetEntryValue("stringarr/one"));
            Assert.That(Value.MakeStringArray("hello", "world\n") == storage.GetEntryValue("stringarr/two"));
            Assert.That(Value.MakeBoolean(true) == storage.GetEntryValue(s));
            Assert.That(Value.MakeBoolean(false) == storage.GetEntryValue(specialLargeDigits));
        }

        [Test]
        public void TestStorageEmptyLoadPersistentWarn()
        {
            SetTestEmpty();

            int lastLine = -1;
            string lastString = String.Empty;

            Action<int, string> warn_func = (int line, string mg) =>
            {
                lastLine = line;
                lastString = mg;
            };

            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes("[NetworkTables Storage 3.0]\nboolean \"foo\"=foo\n")))
            {
                Assert.That(storage.LoadPersistent(stream, warn_func), Is.True);
                Assert.That(lastLine, Is.EqualTo(2));
                Assert.That(lastString, Is.EqualTo("unrecognized boolean value, not 'true' or 'false'"));
            }
            Assert.That(Entries, Is.Empty);
            Assert.That(IdMap, Is.Empty);
            Assert.That(outgoing, Is.Empty);
        }
    }
}
