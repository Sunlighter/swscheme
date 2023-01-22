using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ControlledWindowLib.Scheduling
{
    /// <summary>
    /// Similar to System.Threading.CountdownEvent but can visit zero multiple times.
    /// </summary>
    class IdleDetector : IDisposable
    {
        private int count;
        private object syncRoot;
        private ManualResetEventSlim e;

        public IdleDetector()
        {
            count = 0;
            syncRoot = new object();
            e = new ManualResetEventSlim();
        }

        public void Enter()
        {
            lock (syncRoot)
            {
                if (count == 0) e.Reset();
                count = checked(count + 1);
            }
        }

        public void Leave()
        {
            lock (syncRoot)
            {
                if (count > 0)
                {
                    --count;
                }
                if (count == 0)
                {
                    e.Set();
                }
            }
        }

        public bool IsIdle
        {
            get
            {
                lock (syncRoot)
                {
                    return count == 0;
                }
            }
        }

        public void WaitForIdle()
        {
            e.Wait();
        }

        public void WaitForIdle(int msTimeout)
        {
            e.Wait(msTimeout);
        }

        public void Dispose()
        {
            e.Dispose();
        }
    }
}
