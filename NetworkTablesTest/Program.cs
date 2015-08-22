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
    class Program
    {
        static void Main(string[] args)
        {
            /*
            NetworkTable.SetServerMode();
            NetworkTable.Initialize();

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

            /*var table = NetworkTableOld.GetTable("SD");
            table.PutBoolean("boolean", false);
            TableTest t = new TableTest();
            t.InitTable(table);

            //TableTest2 t2 = new TableTest2();
            //t2.InitTable(table);

            Thread.Sleep(500);


            table.PutString("string", "value");

            Thread.Sleep(100);

            Console.WriteLine(table.GetString("string", "default"));


            Thread.Sleep(Timeout.Infinite);
            //var s = new TcpClient("172.22.11.2", 1735);
           */
            NetworkTable.SetIPAddress("172.22.11.2");
            NetworkTable.SetClientMode();
            NetworkTable.Initialize();
            
            var table = NetworkTable.GetTable("SmartDashboard");

            while (true)
            {
                try
                {
                    Console.WriteLine(table.GetString("test", "DEFAULt"));
                    table.PutString("test", "YOLO");
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
