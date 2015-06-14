using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetworkTablesDotNet.NetworkTables2.Thread;
using NetworkTablesDotNet.NetworkTables2.Util;

namespace NetworkTablesDotNet.NetworkTables2
{
    public class WriteManager : OutgoingEntryReceiver, PeriodicRunnable
    {
        private readonly int SLEEP_TIME = 100;
        private readonly int queueSize = 500;

        private object transactionLock = new object();
        private NTThread thread;
        private NTThreadManager threadManager;
        private readonly AbstractNetworkTableEntryStore entryStore;

        private volatile HalfQueue incomingAssignmentQueue;
        private volatile HalfQueue incomingUpdateQueue;
        private volatile HalfQueue outgoingAssignmentQueue;
        private volatile HalfQueue outgoingUpdateQueue;

        private FlushableOutgoingEntryReceiver receiver;
        private long lastWrite;

        private readonly long keepAliveDelay;

        public WriteManager(FlushableOutgoingEntryReceiver receiver, NTThreadManager threadManager,
            AbstractNetworkTableEntryStore entryStore, long keepAliveDelay)
        {
            this.receiver = receiver;
            this.threadManager = threadManager;
            this.entryStore = entryStore;

            incomingAssignmentQueue = new HalfQueue(queueSize);
            incomingUpdateQueue = new HalfQueue(queueSize);
            outgoingAssignmentQueue = new HalfQueue(queueSize);
            outgoingUpdateQueue = new HalfQueue(queueSize);

            this.keepAliveDelay = keepAliveDelay;

        }

        public void Start()
        {
            if (thread != null)
                Stop();
            lastWrite = Environment.TickCount;
            thread = threadManager.NewBlockingPeriodicThread(this, "Write Thread Manager");
        }

        public void Stop()
        {
            thread?.Stop();
        }

        public void OfferOutgoingAssignment(NetworkTableEntry entry)
        {
            lock (transactionLock)
            {
                incomingAssignmentQueue.Queue(entry);
                if (incomingAssignmentQueue.IsFull())
                {
                    try
                    {
                        Run();
                    }
                    catch (ThreadInterruptedException)
                    {
                    }
                    Console.WriteLine("Assignment queue overflowed. Decrease the rate at which you create new entriesor increase the write buffer size.");
                }
            }
        }

        public void OfferOutgoingUpdate(NetworkTableEntry entry)
        {
            lock (transactionLock)
            {
                incomingUpdateQueue.Queue(entry);
                if (incomingUpdateQueue.IsFull())
                {
                    try
                    {
                        Run();
                    }
                    catch (ThreadInterruptedException)
                    {
                    }
                    Console.WriteLine("Update queue overflowed, decrease the rate at which you update entries or increase the write buffer size.");
                }
            }
        }

        public void Run()
        {
            lock (transactionLock)
            {
                HalfQueue tmp = incomingAssignmentQueue;
                incomingAssignmentQueue = outgoingAssignmentQueue;
                outgoingAssignmentQueue = tmp;

                tmp = incomingUpdateQueue;
                incomingUpdateQueue = outgoingUpdateQueue;
                outgoingUpdateQueue = tmp;
            }

            bool wrote = false;
            NetworkTableEntry entry;
            int i;
            int size = outgoingAssignmentQueue.Size();
            object[] array = outgoingAssignmentQueue.array;

            for (i = 0; i < size; i++)
            {
                entry = (NetworkTableEntry) array[i];
                lock (entryStore)
                {
                    entry.MakeClean();
                }
                wrote = true;
                receiver.OfferOutgoingAssignment(entry);
            }
            outgoingAssignmentQueue.Clear();

            size = outgoingUpdateQueue.Size();
            array = outgoingUpdateQueue.array;
            for (i = 0; i < size; i++)
            {
                entry = (NetworkTableEntry) array[i];
                lock (entryStore)
                {
                    entry.MakeClean();
                }
                wrote = true;
                receiver.OfferOutgoingUpdate(entry);
            }
            outgoingUpdateQueue.Clear();

            if (wrote)
            {
                receiver.Flush();
                lastWrite = Environment.TickCount;
            }
            else if (Environment.TickCount - lastWrite > keepAliveDelay)
            {
                receiver.EnsureAlive();
            }

            System.Threading.Thread.Sleep(SLEEP_TIME);
        }
    }
}
