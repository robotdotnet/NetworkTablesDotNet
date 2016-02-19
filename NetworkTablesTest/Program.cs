using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetworkTables;
using System.Net;

namespace NetworkTablesTest
{
    class Program
    {
        static void Main(string[] args)
        {/*
            try
            {
                var addrEntry = Dns.GetHostEntry("roborio-4488-frc.local");
                Console.WriteLine(addrEntry.HostName);

                foreach(var v in addrEntry.AddressList)
                {
                    Console.WriteLine(v);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.NativeErrorCode);
            }
            */

            /*
            NetworkTable.SetServerMode();
            NetworkTable.SetIPAddress("localhost");
            NetworkTable.Initialize();

            var t = NetworkTable.GetTable("test");

            //t.PutNumber("Key1", 5.89);

            //t.PutNumber("ke2", 675);

            int count = 0;

            while (true)
            {
                t.PutNumber("Key1", count);
                Console.WriteLine(count);
                count++;
                Thread.Sleep(500);

            }
            
            */

            NetworkTable.SetServerMode();
            NetworkTable.SetIPAddress("localhost");
            NetworkTable.Initialize();

            var t = NetworkTable.GetTable("test");

            //t.PutNumber("Key1", 5.89);

            //t.PutNumber("ke2", 675);

            int count = 0;

            while (true)
            {
                double v = t.GetNumber("Key1", count);
                Console.WriteLine(v);
                //count++;
                Thread.Sleep(500);

            }
            //var s = new TcpClient("172.22.11.2", 1735);
            /*
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

     */
        }
    }
}
