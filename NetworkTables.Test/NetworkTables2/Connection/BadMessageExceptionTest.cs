using NetworkTables.NetworkTables2.Connection;
using NUnit.Framework;

namespace NetworkTables.Test.NetworkTables2.Connection
{
    [TestFixture]
    public class BadMessageExceptionTest
    {
        [Test]
        public void ThrowExceptionTest()
        {
            try
            {
                throw new BadMessageException("Got some bad message");
            }
            catch (BadMessageException e)
            {
                Assert.AreEqual("Got some bad message", e.Message);
            }
        }
    }
}
