using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetworkTables.NetworkTables2.Util;

namespace NetworkTables.Test.NetworkTables2.Util
{
    [TestClass]
    public class ListTest
    {
        [TestMethod]
        public void TestIsEmpty()
        {
            List list = new List();
            Assert.IsTrue(list.IsEmpty());
            list.Add(42);
            Assert.IsTrue(!list.IsEmpty());
        }

        [TestMethod]
        public void TestSize()
        {
            List list = new List();
            Assert.IsTrue(list.IsEmpty());
            for (int i = 0; i < 100; i++)
            {
                list.Add(i);
                Assert.IsTrue(list.HasSize(i+1));
            }
        }

        [TestMethod]
        public void TestSizeGrowth()
        {
            List list = new List();
            Assert.IsTrue(list.IsEmpty());
            for (int i = 0; i < 100; i++)
            {
                list.Add(i);
            }
            for (int i = 0; i < 100; i++)
            {
                Assert.IsTrue(list.HasItem(i, i));
            }
        }

        [TestMethod]
        public void TestContains()
        {
            List list = new List();
            int testInt1 = 42;
            int testInt2 = 43;

            Assert.IsFalse(list.Contains(testInt1));

            list.Add(testInt1);
            
            Assert.IsTrue(list.Contains(testInt1));
            Assert.IsFalse(list.Contains(testInt2));

            list.Add(testInt2);

            Assert.IsTrue(list.Contains(testInt2));
        }

        [TestMethod]
        public void TestAddRemoveObject()
        {
            List list = new List();

            int testInt1 = 42;
            int testInt2 = 43;
            int testInt3 = 44;

            list.Add(testInt1);
            list.Add(testInt2);
            list.Add(testInt3);

            Assert.IsTrue(list.Contains(testInt1));
            Assert.IsTrue(list.Contains(testInt2));
            Assert.IsTrue(list.Contains(testInt3));
            Assert.IsTrue(list.HasSize(3));

            list.Remove((object)testInt1);
            list.Remove((object)testInt2);
            list.Remove((object)testInt3);

            Assert.IsFalse(list.Contains(testInt1));
            Assert.IsFalse(list.Contains(testInt2));
            Assert.IsFalse(list.Contains(testInt3));
            Assert.IsTrue(list.HasSize(0));
        }

        [TestMethod]
        public void TestAddRemoveIndex()
        {
            List list = new List();

            int testInt1 = 42;
            int testInt2 = 43;
            int testInt3 = 44;

            list.Add(testInt1);
            list.Add(testInt2);
            list.Add(testInt3);

            Assert.IsTrue(list.HasItem(0, testInt1));
            Assert.IsTrue(list.HasItem(1, testInt2));
            Assert.IsTrue(list.HasItem(2, testInt3));
            Assert.IsTrue(list.HasSize(3));

            list.Remove(2);
            list.Remove(1);
            list.Remove(0);

            Assert.IsTrue(list.IsEmpty());
        }


        public void TestClear()
        {
            List list = new List();

            list.Add(42);
            list.Add(43);
            list.Add(44);

            Assert.IsFalse(list.IsEmpty());

            list.Clear();

            Assert.IsTrue(list.IsEmpty());
        }

        [TestMethod]
        public void TestOrdering()
        {
            int obj1 = 42;
            bool obj2 = false;
            double obj3 = 42.42;

            List list = new List();

            list.Add(obj1);
            list.Add(obj2);
            list.Add(obj3);

            Assert.IsTrue(list.HasItem(0, obj1));
            Assert.IsTrue(list.HasItem(1, obj2));
            Assert.IsTrue(list.HasItem(2, obj3));

            list.Remove(obj2);

            Assert.IsTrue(list.HasItem(0, obj1));
            Assert.IsTrue(list.HasItem(1, obj3));
        }

        [TestMethod]
        public void TestSet()
        {
            int obj1 = 42;
            bool obj2 = false;
            double obj3 = 42.42;
            string obj4 = "MyString";
            string obj5 = "MyString2";
            string obj6 = "MyString3";

            List list = new List();

            list.Add(obj1);
            list.Add(obj2);
            list.Add(obj3);

            Assert.IsTrue(list.HasItem(0, obj1));
            Assert.IsTrue(list.HasItem(1, obj2));
            Assert.IsTrue(list.HasItem(2, obj3));

            list.Set(1, obj4);

            Assert.IsTrue(list.HasItem(0, obj1));
            Assert.IsTrue(list.HasItem(1, obj4));
            Assert.IsTrue(list.HasItem(2, obj3));
            Assert.IsTrue(list.HasSize(3));

            list.Set(2, obj5);

            Assert.IsTrue(list.HasItem(0, obj1));
            Assert.IsTrue(list.HasItem(1, obj4));
            Assert.IsTrue(list.HasItem(2, obj5));
            Assert.IsTrue(list.HasSize(3));

            list.Set(0, obj6);

            Assert.IsTrue(list.HasItem(0, obj6));
            Assert.IsTrue(list.HasItem(1, obj4));
            Assert.IsTrue(list.HasItem(2, obj5));
            Assert.IsTrue(list.HasSize(3));
        }
    }

    public static class ListExtensions
    {
        public static bool Contains(this List list, object o)
        {
            return list.Contains(o);
        }

        public static bool HasSize(this List list, int size)
        {
            return list.Size() == size;
        }

        public static bool HasItem(this List list, int index, object o)
        {
            bool retVal = false;
            try
            {
                var ret = list.Get(index);

                retVal = ret.Equals(o);
                //retVal = (list.Get(index) == o);
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
            return retVal;
        }
    }
}
