using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetworkTablesDotNet.NetworkTables;

namespace NetworkTablesTest
{
    class Program
    {
        static void Main(string[] args)
        {
            
            //var s = new TcpClient("172.22.11.2", 1735);
           
            NetworkTable.SetIPAddress("172.22.11.2");
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
            Thread.Sleep(1000);

            while (true)
            {
                Console.WriteLine(sd.GetValue("test"));
                Thread.Sleep(500);
            }
            

        }
    }
}
