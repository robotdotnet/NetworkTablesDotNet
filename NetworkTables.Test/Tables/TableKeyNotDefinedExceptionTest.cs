
using NetworkTables.Tables;
using NUnit.Framework;

namespace NetworkTables.Test.Tables
{
    [TestFixture]
    public class TableKeyNotDefinedExceptionTest
    {
        [Test]
        public void ThrowExceptionTest()
        {
            try
            {
                throw new TableKeyNotDefinedException("Key 1");
            }
            catch (TableKeyNotDefinedException e)
            {
                Assert.AreEqual("Unknown Table Key: Key 1", e.Message);
            }
        }
    }
}
