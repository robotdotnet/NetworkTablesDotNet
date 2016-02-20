using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetworkTables.Extensions;

namespace NetworkTables.Support
{
    internal class NTConcurrentQueue<T>
    {
        private Queue<T> queue_ = new Queue<T>();
        private readonly object mutex_ = new object();
        private readonly AutoResetEvent cond_ = new AutoResetEvent(false);

        public bool Empty
        {
            get
            {
                lock(mutex_)
                {
                    return queue_.Count == 0;
                }
            }
        }

        public int Size
        {
            get
            {
                lock(mutex_)
                {
                    return queue_.Count;
                }
            }
        }

        public T Pop()
        {
            bool lockEntered = false;
            try
            {
                Monitor.Enter(mutex_, ref lockEntered);
                while(queue_.Count == 0)
                {
                    cond_.Wait(mutex_, ref lockEntered);
                }
                var item = queue_.Dequeue();
                return item;
            }
            finally
            {
                if (lockEntered) Monitor.Exit(mutex_);
            }
        }

        public void Push(T item)
        {
            bool lockEntered = false;
            try
            {
                Monitor.Enter(mutex_, ref lockEntered);
                queue_.Enqueue(item);
                Monitor.Exit(mutex_);
                lockEntered = false;
                cond_.Set();
            }
            finally
            {
                if (lockEntered) Monitor.Exit(mutex_);
            }
        }
    }
}
