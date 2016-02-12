﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkTables.Extensions
{
    internal static class AutoResetEventExtensions
    {
        public static bool WaitTimeout(this AutoResetEvent e, object mutex, ref bool lockEntered,
            TimeSpan timeout, Func<bool> pred)
        {
            //Throw if thread currently doesn't own the lock
            if (!Monitor.IsEntered(mutex))
            {
                throw new SynchronizationLockException();
            }
            if (timeout < TimeSpan.Zero)
                timeout = TimeSpan.Zero;
            //While pred is false.
            while (!pred())
            {
                Monitor.Exit(mutex);
                lockEntered = false;
                if (!e.WaitOne(timeout))
                {
                    //Timed out
                    Monitor.Enter(mutex, ref lockEntered);
                    return pred();
                }
                Monitor.Enter(mutex, ref lockEntered);
            }

            return true;
        }

        public static void Wait(this AutoResetEvent e, object mutex, ref bool lockEntered, Func<bool> pred)
        {
            //Throw if thread currently doesn't own the lock
            if (!Monitor.IsEntered(mutex))
            {
                throw new SynchronizationLockException();
            }
            //While pred is false.
            while (!pred())
            {
                Monitor.Exit(mutex);
                lockEntered = false;
                e.WaitOne();
                Monitor.Enter(mutex, ref lockEntered);
            }
        }

        public static void Wait(this AutoResetEvent e, object mutex, ref bool lockEntered)
        {
            //Throw if thread currently doesn't own the lock
            if (!Monitor.IsEntered(mutex))
            {
                throw new SynchronizationLockException();
            }
            Monitor.Exit(mutex);
            lockEntered = false;
            e.WaitOne();
            Monitor.Enter(mutex, ref lockEntered);
        }
    }
}
