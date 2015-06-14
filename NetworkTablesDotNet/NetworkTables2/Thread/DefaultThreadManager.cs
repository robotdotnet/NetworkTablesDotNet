using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;


namespace NetworkTablesDotNet.NetworkTables2.Thread
{
    class PeriodicNTThread : NTThread
    {
        private System.Threading.Thread thread;
        private bool run = true;

        private PeriodicRunnable r;

        public PeriodicNTThread(PeriodicRunnable r, string name)
        {
            this.r = r;
            thread = new System.Threading.Thread(Run);
            thread.Start();
        }

        public void Run()
        {
            try
            {
                while (run)
                {
                    r.Run();
                }
            }
            catch (ThreadInterruptedException e)
            {

            }
        }

        public void Stop()
        {
            run = false;
            thread.Interrupt();
        }
        public bool IsRunning()
        {
            return thread.IsAlive;
        }
    }

    public class DefaultThreadManager : NTThreadManager
    {
        public NTThread NewBlockingPeriodicThread(PeriodicRunnable r, string name)
        {
            return new PeriodicNTThread(r, name);
        }
    }
}
