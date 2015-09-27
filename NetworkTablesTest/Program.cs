using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetworkTables;
using NetworkTables.NetworkTables;
using NetworkTables.NetworkTables2.Util;
using NetworkTables.NTCore;
using NetworkTables.NTCore.RPC;
using NetworkTables.Tables;

namespace NetworkTablesTest
{

    class TableTest : ITableListener
    {
        private ITable table;
        public void InitTable(ITable table)
        {
            this.table = table;
            table?.AddTableListener(this, true);
        }

        public void ValueChanged(ITable source, string key, object value, bool isNew)
        {
            Console.WriteLine(key);
            if (key == "boolean")
            {
                Console.WriteLine((bool)value);
            }
            else if (key == "string")
            {
                Console.WriteLine(value + " Table Listener");
            }
        }
    }

    class TableTest2 : ITableListener
    {
        private ITable table;
        public void InitTable(ITable table)
        {
            this.table = table;
            table?.AddTableListener(this, true);
        }

        public void ValueChanged(ITable source, string key, object value, bool isNew)
        {
            Console.WriteLine(key);
            if (key == "boolean")
            {
                Console.WriteLine((bool)value);
            }
            else if (key == "string")
            {
                Console.WriteLine(value + " Table Listener");
            }
        }
    }

    struct str
    {
        private IntPtr ptr;
        private UIntPtr uptr;
        //bool Arr { get; set; }
    }

    class Program
    {
        static byte[] callback1(string name, byte[] params_str)
        {
            var param = RpcMethods.UnpackRpcValues(params_str, NT_Type.NT_DOUBLE);
            if (param.Count == 0)
            {
                Console.WriteLine("Empty Params?");
                return new byte[0];
            }
            Console.WriteLine($"Called with {param[0].Value}");
            return RpcMethods.PackRpcValues(new RPCValue((double) param[0].Value + 1.2));
        }
        static void Main(string[] args)
        {
            
            Console.WriteLine(Marshal.SizeOf(typeof(str)));
            Console.WriteLine(Marshal.SizeOf(typeof(NT_String)));

            Console.ReadKey();


            CoreLogging.SetLogFunction(((level, file, line, msg) =>
            {
                Console.Error.WriteLine(msg);
            }), 0);
            uint version = 1;
            string name = "myfunc1";
            List<NT_RpcParamDef> paramList = new List<NT_RpcParamDef>();
            paramList.Add(new NT_RpcParamDef("param1", new RPCValue(0.0)));
            List<NT_RpcResultDef> resultList = new List<NT_RpcResultDef>();
            resultList.Add(new NT_RpcResultDef("result1", NT_Type.NT_DOUBLE));
            NT_RpcDefinition def = new NT_RpcDefinition(version,name, paramList.ToArray(), resultList.ToArray());

            RpcMethods.CreateRpc("func1", def, callback1);
            Console.WriteLine("Calling rpc");
            uint call1_uid = RpcMethods.CallRpc("func1", new RPCValue(2.0));

            byte[] call1ResultStr;
            Console.WriteLine("Waiting for Rpc Result");

            RpcMethods.GetRpcResult(true, call1_uid, out call1ResultStr);

            var result = RpcMethods.UnpackRpcValues(call1ResultStr, NT_Type.NT_DOUBLE);
            if (result.Count == 0)
            {
                Console.WriteLine("Empty Result?");
                Console.ReadKey();
                return;
            }
            Console.WriteLine($"Got {result[0].Value}");
            Console.ReadKey();
            //def.SetParams(new NT_RpcParamDef("param1", NT_Type.NT_DOUBLE));


            /*
            NetworkTable.SetServerMode();
            //CoreMethods.EnableDebugLogging(true, NT_LogLevel.NT_LOG_INFO);
            //NetworkTable.SetIPAddress("172.22.11.2");
            
            CoreMethods.AddConnectionListener(((uid, connected, connection) =>
            {
                Console.WriteLine(uid);
                Console.WriteLine(connected);
                Console.WriteLine(connection.ProtocolVersion);
                Console.WriteLine(connection.RemoteName);
            }));
            
            var nt = NetworkTable.GetTable("SmartDashboard");

            
            nt.PutString("test", "HELLO FROM NT LAND");

            while (true)
            {
                
                var connections = CoreMethods.GetConnectionInfo();
                for (int i = 0; i < connections.Length; i++)
                {
                    Console.WriteLine(connections[i].ProtocolVersion);
                    Console.WriteLine(connections[i].RemoteName);
                    Console.WriteLine(connections[i].RemotePort);
                }
                connections.Dispose();
                
                Console.WriteLine(nt.GetString("test", "Default"));
                
                Thread.Sleep(500);
                //Console.WriteLine("Loop");
            }
            /*
            NetworkTable.SetServerMode();
            NetworkTable.Initialize();
            /*
            var sd = NetworkTable.GetTable("SmartDashboard");
            sd.PutString("test", "HELLO FROM NT LAND");
            sd.PutBoolean("MyBool", true);
            sd.PutNumber("MyNumber", 2.5);
            sd.PutString("MyString", "Default");
            Thread.Sleep(1000);

            while (true)
            {
                Console.WriteLine(sd.GetString("test", "Default"));
                Console.WriteLine(sd.GetBoolean("MyBool", false) + ": Bool");
                Console.WriteLine(sd.GetNumber("MyNumber", 3.2) + ": Number");
                Console.WriteLine(sd.GetString("MyString", "DefaultMyString") + ": String");
                Thread.Sleep(500);
            }

            var table = NetworkTable.GetTable("SD");
            table.PutBoolean("boolean", false);
            TableTest t = new TableTest();
            t.InitTable(table);

            //TableTest2 t2 = new TableTest2();
            //t2.InitTable(table);

            Thread.Sleep(500);

            InteropHelpers.SetEntryDoubleArray("/SD/myArray", new[] {2.56, 3.85, 4.58});

            Thread.Sleep(50);

            ulong lc = 0;
            double[] dbl = InteropHelpers.GetEntryDoubleArray("/SD/myArray", ref lc);

            foreach (var d in dbl)
            {
                Console.WriteLine(d);
            }

            /*
            table.PutString("string", "value");

            Thread.Sleep(100);

            Console.WriteLine(table.ContainsKey("string"));

            Thread.Sleep(50);

            ITable subtable = table.GetSubTable("subtable");

            Console.WriteLine(table.ContainsSubTable("subtable"));

            subtable.PutNumber("SubNumber", 3.56);

            Thread.Sleep(50);

            Console.WriteLine(table.ContainsSubTable("subtable"));


            Console.WriteLine(table.GetString("string", "default"));


            Thread.Sleep(Timeout.Infinite);
            /*
            //var s = new TcpClient("172.22.11.2", 1735);
           *
            NetworkTable.SetIPAddress("172.22.11.2");
            NetworkTable.SetClientMode();
            NetworkTable.Initialize();
            
            var table = NetworkTable.GetTable("SmartDashboard");

            while (true)
            {
                try
                {
                    Console.WriteLine(table.GetString("test", "DEFAULt"));
                    table.PutString("test", "New Client");
                    //NetworkTable.Flush();
                }
                catch
                {
                }
                Thread.Sleep(500);
            }
            
            
            /*
            
            NetworkTableOld.SetServerMode();
            NetworkTableOld.Initialize();

            var sd = NetworkTableOld.GetTable("SmartDashboard");
            sd.PutValue("test", "HELLO FROM NT LAND");
            sd.PutBoolean("MyBool", true);
            sd.PutNumber("MyNumber", 2.5);
            sd.PutString("MyString", "Default");
            Thread.Sleep(1000);

            while (true)
            {
                Console.WriteLine(sd.GetValue("test"));
                Console.WriteLine(sd.GetBoolean("MyBool") + ": Bool");
                Console.WriteLine(sd.GetNumber("MyNumber") + ": Number");
                Console.WriteLine(sd.GetString("MyString") + ": String");
                Thread.Sleep(500);
            }
            
    */
        }
    }
}
