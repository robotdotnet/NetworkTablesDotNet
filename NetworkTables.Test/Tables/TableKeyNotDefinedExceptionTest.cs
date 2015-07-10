using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetworkTables.Tables;

namespace NetworkTables.Test.Tables
{
    [TestClass]
    public class TableKeyNotDefinedExceptionTest
    {
        [TestMethod, ]
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
