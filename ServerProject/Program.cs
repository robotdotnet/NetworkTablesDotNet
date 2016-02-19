using NetworkTables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerProject
{
    class Program
    {
        static void Main(string[] args)
        {
            NetworkTable.SetClientMode();
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
        }
    }
}
