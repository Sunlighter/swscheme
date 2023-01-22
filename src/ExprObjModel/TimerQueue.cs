/*
    This file is part of Sunlit World Scheme
    http://swscheme.codeplex.com/
    Copyright (c) 2010 by Edward Kiser (edkiser@gmail.com)

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program; if not, write to the Free Software Foundation, Inc.,
    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
*/

using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using ControlledWindowLib;
using System.Runtime.InteropServices;

namespace ExprObjModel
{
    public class TimerQueue<T> : IDisposable
    {
        private AsyncQueue<Option<T>> results;
        private ManualResetEvent death;
        private ManualResetEvent changed;
        private Mutex syncRoot;
        private Thread worker;

        private SortedDictionary<uint, FList<T>> queue;
        private SortedDictionary<uint, FList<T>> postWrap;

        private bool alreadyDisposed;

        public TimerQueue()
        {
            results = new AsyncQueue<Option<T>>();
            death = new ManualResetEvent(false);
            changed = new ManualResetEvent(false);
            syncRoot = new Mutex();
            worker = new Thread(new ThreadStart(ThreadProc));
            queue = new SortedDictionary<uint, FList<T>>();
            postWrap = new SortedDictionary<uint, FList<T>>();
            alreadyDisposed = false;

            worker.Start();
        }

        private static void Add(SortedDictionary<uint, FList<T>> q, uint time, T item)
        {
            if (q.ContainsKey(time))
            {
                q[time] = new FList<T>(item, q[time]);
            }
            else
            {
                q.Add(time, new FList<T>(item));
            }
        }

        public void Put(uint delay, T item)
        {
            if (alreadyDisposed) throw new ObjectDisposedException("TimerQueue");
            syncRoot.WaitOne();
            try
            {
                uint eventTime = unchecked(Utils.GetTickCount() + delay);
                if (eventTime < delay)
                {
                    Add(postWrap, eventTime, item);
                }
                else
                {
                    Add(queue, eventTime, item);
                }
                changed.Set();
            }
            finally
            {
                syncRoot.ReleaseMutex();
            }
        }

        public IAsyncResult BeginGet(AsyncCallback callback, object state)
        {
            if (alreadyDisposed) throw new ObjectDisposedException("TimerQueue"); 
            return results.BeginGet(callback, state);
        }

        public Option<T> EndGet(IAsyncResult iar)
        {
            if (alreadyDisposed) throw new ObjectDisposedException("TimerQueue");
            return results.EndGet(iar);
        }

        public void Dispose()
        {
            if (alreadyDisposed) return;

            death.Set();
            worker.Join();

            death.Close();
            changed.Close();
            syncRoot.Close();

            results.Dispose();

            alreadyDisposed = true;
        }

        private enum WaitType
        {
            NextEvent,
            LongWait,
            Wrap
        }

        private Tuple<int, WaitType> ComputeWait()
        {
            uint currentTime = Utils.GetTickCount();
            if (queue.Count > 0)
            {
                uint firstEvent = queue.Keys.First();
                if (firstEvent <= currentTime) return new Tuple<int, WaitType>(0, WaitType.NextEvent);

                if ((firstEvent - currentTime) > (uint)(int.MaxValue))
                {
                    return new Tuple<int, WaitType>(int.MaxValue, WaitType.LongWait);
                }
                else
                {
                    return new Tuple<int, WaitType>((int)(firstEvent - currentTime), WaitType.NextEvent);
                }
            }
            else
            {
                uint waitTime = unchecked(0u - currentTime);
                if (waitTime > (uint)(int.MaxValue)) return new Tuple<int, WaitType>(int.MaxValue, WaitType.LongWait);
                else return new Tuple<int, WaitType>((int)waitTime, WaitType.Wrap);
            }
        }

        private void ThreadProc()
        {
            syncRoot.WaitOne();
            while (true)
            {
                Tuple<int, WaitType> waitLength;

                try
                {
                    waitLength = ComputeWait();
                    System.Diagnostics.Debug.WriteLine("WaitLength = (" + waitLength.Item1 + ", " + waitLength.Item2 + ")");
                }
                finally
                {
                    syncRoot.ReleaseMutex();
                }

                int which = WaitHandle.WaitAny(new WaitHandle[] { death, changed }, waitLength.Item1);
                uint actualTime = Utils.GetTickCount();

                syncRoot.WaitOne();
                if (which == WaitHandle.WaitTimeout)
                {
                    System.Diagnostics.Debug.WriteLine("-- Wait Timeout");
                    if (waitLength.Item2 == WaitType.NextEvent)
                    {
                        while (true)
                        {
                            if (queue.Count == 0) break;
                            uint firstEvent = queue.Keys.First();
                            if (firstEvent > actualTime) break;
                            FList<T> items = queue[firstEvent];
                            while (items != null)
                            {
                                results.Put(new Some<T>() { value = items.Head });
                                items = items.Tail;
                            }
                            queue.Remove(firstEvent);
                        }
                    }
                    else if (waitLength.Item2 == WaitType.Wrap)
                    {
                        var x = queue; queue = postWrap; postWrap = x;
                        // and fall through, which has the effect of recomputing the wait time
                    }
                    // else it's LongWait, so we merely fall through
                }
                else if (which == 0)
                {
                    System.Diagnostics.Debug.WriteLine("-- Death");
                    results.Put(new None<T>());
                    // death
                    break;
                }
                else if (which == 1)
                {
                    System.Diagnostics.Debug.WriteLine("-- Changed");
                    changed.Reset();
                    // changed, so fall through, which has the effect of recomputing the wait time
                }
            }
            syncRoot.ReleaseMutex();
        }
    }

    public static partial class Utils
    {
        [SchemeFunction("get-tick-count")]
        [DllImport("kernel32.dll")]
        public static extern uint GetTickCount();
    }
}