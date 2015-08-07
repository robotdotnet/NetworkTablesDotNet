
using NetworkTables.NetworkTables2.Util;
using NUnit.Framework;

namespace NetworkTables.Test.NetworkTables2.Util
{
    [TestFixture]
    public class StackTest
    {
        [Test]
        public void PushTest()
        {
            int obj1 = 0;
            Stack stack = new Stack();
            Assert.IsTrue(stack.IsEmpty());
            stack.Push(obj1);
            Assert.IsFalse(stack.IsEmpty());
        }

        [Test]
        public void PopTest()
        {
            int obj1 = 0;
            object obj2;
            Stack stack = new Stack();
            Assert.IsTrue(stack.IsEmpty());
            obj2 = stack.Pop();
            Assert.IsNull(obj2);

            stack.Push(obj1);
            obj2 = stack.Pop();
            Assert.AreEqual(obj1, obj2);
            Assert.IsTrue(stack.IsEmpty());
        }

        [Test]
        public void OrderingTest()
        {
            int obj1 = 42;
            int obj2 = 43;
            int obj3 = 44;
            object obj4;
            object obj5;
            object obj6;
            object obj7;
            Stack stack = new Stack();
            Assert.IsTrue(stack.IsEmpty());
            stack.Push(obj1);
            Assert.IsTrue(stack.HasSize(1));
            stack.Push(obj2);
            stack.Push(obj3);
            Assert.IsTrue(stack.HasSize(3));
            obj4 = stack.Pop();
            Assert.IsTrue(stack.HasSize(2));
            obj5 = stack.Pop();
            obj6 = stack.Pop();
            Assert.AreEqual(obj1, obj6);
            Assert.AreEqual(obj2, obj5);
            Assert.AreEqual(obj3, obj4);

            stack.Push(obj1);
            stack.Push(obj2);
            stack.Push(obj3);
            stack.Pop();
            stack.Push(obj1);
            obj7 = stack.Pop();
            Assert.AreEqual(obj1, obj7);
        }
    }

    public static class StackExtensions
    {
        public static bool HasSize(this Stack stack, int size)
        {
            return stack.Size() == size;
        }
    }
}
