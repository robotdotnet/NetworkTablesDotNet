using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetworkTables;
using NetworkTables.NetworkTables;

namespace NetworkTablesTest
{
    class Program
    {
        static void Main(string[] args)
        {
            
            //var s = new TcpClient("172.22.11.2", 1735);
           
            NetworkTable.SetIPAddress("roborio-4488.local");
            NetworkTable.SetClientMode();
            NetworkTable.Initialize();
            
            var table = NetworkTable.GetTable("SmartDashboard");

            while (true)
            {
                try
                {
                    Console.WriteLine(table.GetValue("test"));
                    table.PutValue("test", "YOLO");
                }
                catch
                {
                }
                Thread.Sleep(500);
            }
            
            
            
            
            NetworkTable.SetServerMode();
            NetworkTable.Initialize();

            var sd = NetworkTable.GetTable("SmartDashboard");
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
            

        }
    }
}
