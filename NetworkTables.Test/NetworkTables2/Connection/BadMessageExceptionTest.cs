using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetworkTables.NetworkTables2.Connection;

namespace NetworkTables.Test.NetworkTables2.Connection
{
    [TestClass]
    public class BadMessageExceptionTest
    {
        [TestMethod]
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
