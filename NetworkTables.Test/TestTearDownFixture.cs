using NetworkTables;
using NUnit.Framework;

namespace NetworkTablesCore.Test
{
    [SetUpFixture]
    public class TestTearDownFixture
    {
        [TearDown]
        public void TearDown()
        {
            NtCore.StopClient();
            NtCore.StopServer();

            NtCore.StopNotifier();
            NtCore.StopRpcServer();
        }
    }
}
