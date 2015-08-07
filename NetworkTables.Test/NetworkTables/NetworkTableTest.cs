using System;
using NetworkTables.NetworkTables;
using NetworkTables.NetworkTables2.Client;
using NetworkTables.Tables;
using NUnit.Framework;

namespace NetworkTables.Test.NetworkTables
{
    [TestFixture]
    public class NetworkTableTest
    {
        private static NetworkTableClient client;
        private static NetworkTableProvider provider;
        private static NetworkTable testTable1;
        private static NetworkTable testTable2;

        [TestFixtureSetUp]
        public static void Init()
        {
            client = new NetworkTableClient(new IOClass());
            provider = new NetworkTableProvider(client);
            testTable1 = (NetworkTable)provider.GetTable("/test1");
            testTable2 = (NetworkTable)provider.GetTable("/test2");
        }

        [TestFixtureTearDown]
        public static void Cleanup()
        {
            provider.Close();
        }

        [Test]
        public void PutDoubleTest()
        {
            double testDouble = 43.43;
            testTable1.PutNumber("double", 42.42);
            try
            {
                testDouble = testTable1.GetNumber("double");
            }
            catch (TableKeyNotDefinedException e)
            {
                Console.WriteLine(e);
            }

            Assert.AreEqual(42.42, testDouble, 0.0);

            try
            {
                testDouble = testTable1.GetNumber("Non-Existant");
                Assert.Fail();
            }
            catch (TableKeyNotDefinedException e)
            {
            }
            testDouble = testTable1.GetNumber("Non-Existant", 44.44);
            Assert.AreEqual(44.44, testDouble, 0.0);
        }

        [Test]
        public void PutBooleanTest()
        {
            bool testBool = false;
            testTable1.PutBoolean("boolean", true);
            try
            {
                testBool = testTable1.GetBoolean("boolean");
            }
            catch (TableKeyNotDefinedException e)
            {
                Console.WriteLine(e);
            }
            Assert.IsTrue(testBool);
            try
            {
                testBool = testTable1.GetBoolean("Non-Existant");
                Assert.Fail();
            }
            catch (TableKeyNotDefinedException e)
            {
            }
            testBool = testTable1.GetBoolean("Non-Existant", false);
            Assert.IsFalse(testBool);
        }

        [Test]
        public void PutStringTest()
        {
            string testString = "Initialized Test";
            testTable1.PutString("String", "Test 1");
            try
            {
                testString = testTable1.GetString("String");
            }
            catch (TableKeyNotDefinedException e)
            {
                Console.WriteLine(e);
            }
            Assert.AreEqual("Test 1", testString);
            try
            {
                testString = testTable1.GetString("Non-Existant");
                Assert.Fail();
            }
            catch (TableKeyNotDefinedException)
            {
            }
            testString = testTable1.GetString("Non-Existant", "Test 3");
            Assert.AreEqual("Test 3", testString);
        }

        [Test]
        public void PutMultiDataTypeTest()
        {
            double double1 = 1;
            double double2 = 2;
            double double3 = 3;
            bool bool1 = false;
            bool bool2 = true;
            string string1 = "String 1";
            string string2 = "String 2";
            string string3 = "String 3";

            testTable1.PutNumber("double1", double1);
            testTable1.PutNumber("double2", double2);
            testTable1.PutNumber("double3", double3);
            testTable1.PutBoolean("bool1", bool1);
            testTable1.PutBoolean("bool2", bool2);
            testTable1.PutString("string1", string1);
            testTable1.PutString("string2", string2);
            testTable1.PutString("string3", string3);

            Assert.AreEqual(double1, testTable1.GetNumber("double1"), 0.0);
            Assert.AreEqual(double2, testTable1.GetNumber("double2"), 0.0);
            Assert.AreEqual(double3, testTable1.GetNumber("double3"), 0.0);
            Assert.AreEqual(bool1, testTable1.GetBoolean("bool1"));
            Assert.AreEqual(bool2, testTable1.GetBoolean("bool2"));
            Assert.AreEqual(string1, testTable1.GetString("string1"));
            Assert.AreEqual(string2, testTable1.GetString("string2"));
            Assert.AreEqual(string3, testTable1.GetString("string3"));

            double1 = 4;
            double2 = 5;
            double3 = 6;
            bool1 = true;
            bool2 = false;
            string1 = "String 4";
            string2 = "String 5";
            string3 = "String 6";

            testTable1.PutNumber("double1", double1);
            testTable1.PutNumber("double2", double2);
            testTable1.PutNumber("double3", double3);
            testTable1.PutBoolean("bool1", bool1);
            testTable1.PutBoolean("bool2", bool2);
            testTable1.PutString("string1", string1);
            testTable1.PutString("string2", string2);
            testTable1.PutString("string3", string3);

            Assert.AreEqual(double1, testTable1.GetNumber("double1"), 0.0);
            Assert.AreEqual(double2, testTable1.GetNumber("double2"), 0.0);
            Assert.AreEqual(double3, testTable1.GetNumber("double3"), 0.0);
            Assert.AreEqual(bool1, testTable1.GetBoolean("bool1"));
            Assert.AreEqual(bool2, testTable1.GetBoolean("bool2"));
            Assert.AreEqual(string1, testTable1.GetString("string1"));
            Assert.AreEqual(string2, testTable1.GetString("string2"));
            Assert.AreEqual(string3, testTable1.GetString("string3"));
        }

        [Test]
        public void MultiTableTest()
        {
            double table1double = 1;
            double table2double = 2;
            bool table1boolean = true;
            bool table2boolean = false;
            string table1String = "Table 1";
            string table2String = "Table 2";

            testTable1.PutNumber("table1double", table1double);
            testTable1.PutBoolean("table1boolean", table1boolean);
            testTable1.PutString("table1String", table1String);

            try
            {
                testTable2.GetNumber("table1double");
                Assert.Fail();
            }
            catch (TableKeyNotDefinedException e) { }
            try
            {
                testTable2.GetBoolean("table1boolean");
                Assert.Fail();
            }
            catch (TableKeyNotDefinedException e) { }
            try
            {
                testTable2.GetString("table1String");
                Assert.Fail();
            }
            catch (TableKeyNotDefinedException e) { }

            testTable2.PutNumber("table2double", table2double);
            testTable2.PutBoolean("table2boolean", table2boolean);
            testTable2.PutString("table2String", table2String);

            try
            {
                testTable1.GetNumber("table2double");
                Assert.Fail();
            }
            catch (TableKeyNotDefinedException e) { }
            try
            {
                testTable1.GetBoolean("table2boolean");
                Assert.Fail();
            }
            catch (TableKeyNotDefinedException e) { }
            try
            {
                testTable1.GetString("table2String");
                Assert.Fail();
            }
            catch (TableKeyNotDefinedException e) { }
        }

        [Test]
        public void GetTableTest()
        {
            Assert.AreSame(testTable1, provider.GetTable("/test1"));
            Assert.AreSame(testTable2, provider.GetTable("/test2"));
            Assert.AreNotSame(testTable1, provider.GetTable("/test2"));
            ITable testTable3 = provider.GetTable("/test3");
            Assert.AreNotSame(testTable1, testTable3);
            Assert.AreNotSame(testTable2, testTable3);
        }
    }
}
