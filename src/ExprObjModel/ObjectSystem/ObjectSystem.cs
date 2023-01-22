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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ControlledWindowLib;

namespace ExprObjModel.ObjectSystem
{
    public struct OldObjectID : IEquatable<OldObjectID>
    {
        public uint id;

        public OldObjectID(uint id)
        {
            this.id = id;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is OldObjectID)) return false;
            OldObjectID d = (OldObjectID)obj;
            return this.id == d.id;
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public override string ToString()
        {
            return "(object id: " + id.ToString() + ")";
        }

        public static bool operator ==(OldObjectID a, OldObjectID b)
        {
            return a.id == b.id;
        }

        public static bool operator !=(OldObjectID a, OldObjectID b)
        {
            return a.id != b.id;
        }

        public static explicit operator uint(OldObjectID d)
        {
            return d.id;
        }

        public static explicit operator OldObjectID(uint d)
        {
            return new OldObjectID(d);
        }

        public static OldObjectID operator ++(OldObjectID d)
        {
            return new OldObjectID(d.id + 1u);
        }

        public bool Equals(OldObjectID other)
        {
            return this.id == other.id;
        }

        public static OldObjectID Zero { get { return new OldObjectID((ushort)0u); } }
    }

    public interface IMessageHandler<M> : IDisposable
    {
        void Welcome(ObjectSystem<M> objectSystem, OldObjectID self);
        void Handle(M message);
    }

    public class AsyncContinuation<T>
    {
        private T value;
        private ManualResetEventSlim e;

        public AsyncContinuation()
        {
            value = default(T);
            e = new ManualResetEventSlim();
        }

        public void Return(T item)
        {
            value = item;
            e.Set();
        }

        public T GetValue()
        {
            e.Wait();
            return value;
        }
    }

    public partial class ObjectSystem<M> : IDisposable
    {
        private AsyncQueue<InternalCommand> commandQueue;
        private Thread dispatchThread;
        private int workerCount;

        public ObjectSystem(int workerCount)
        {
            commandQueue = new AsyncQueue<InternalCommand>();
            this.workerCount = workerCount;
            dispatchThread = new Thread(new ThreadStart(ThreadProc));
            dispatchThread.Start();
        }

        public OldObjectID AddObject(IMessageHandler<M> obj)
        {
            if (!(commandQueue.IsClosed))
            {
                AsyncContinuation<OldObjectID> k = new AsyncContinuation<OldObjectID>();
                commandQueue.Put(new IC_AddObject(obj, k));
                return k.GetValue();
            }
            else throw new ObjectDisposedException("ObjectSystem");
        }

        public void RemoveObject(OldObjectID id)
        {
            if (!(commandQueue.IsClosed))
            {
                commandQueue.Put(new IC_RemoveObject(id));
            }
        }

        public bool Post(OldObjectID dest, M message)
        {
            if (!(commandQueue.IsClosed))
            {
                AsyncContinuation<bool> k = new AsyncContinuation<bool>();
                commandQueue.Put(new IC_PostMessage(dest, message, k));
                return k.GetValue();
            }
            else return false;
        }

        public void PostLater(uint delay_ms, OldObjectID dest, M message, Action<M> cancelled)
        {
            if (!(commandQueue.IsClosed))
            {
                System.Diagnostics.Debug.WriteLine("PostLater: posted to command queue");
                commandQueue.Put(new IC_PostMessageLater(delay_ms, dest, message, cancelled));
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("PostLater: Trivially cancelled (command queue closed)");
                cancelled(message);
            }
        }

        public void PostOnCompletion(OldObjectID dest, IAsyncResult iar, Func<IAsyncResult, M> completion, Action<M> cancelled)
        {
            throw new NotImplementedException();
#if false
            if (!(commandQueue.IsClosed))
            {
                System.Diagnostics.Debug.WriteLine("PostOnCompletion: posted to command queue");
                commandQueue.Put(new IC_PostMessageOnCompletion(dest, iar, completion, cancelled));
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("PostOnCompletion: Cancellation queued (command queue closed)");
                WaitOrTimerCallback cb = delegate(object state, bool timedOut)
                {
                    M msg = completion(iar);
                    cancelled(msg);
                };
                ThreadPool.RegisterWaitForSingleObject(iar.AsyncWaitHandle, cb, null, -1, true);
            }
#endif
        }

        public void Dispose()
        {
            commandQueue.Put(new IC_Shutdown());
            commandQueue.Close();
            dispatchThread.Join();
        }

        private class DelayedMessage
        {
            private OldObjectID dest;
            private M message;
            private Action<M> cancelled;

            public DelayedMessage(OldObjectID dest, M message, Action<M> cancelled)
            {
                this.dest = dest;
                this.message = message;
                this.cancelled = cancelled;
            }

            public OldObjectID Dest { get { return dest; } }
            public M Message { get { return message; } }
            public Action<M> Cancelled { get { return cancelled; } }
        }

        private enum WaitType
        {
            NextEvent,
            LongWait,
            Wrap
        }

        private class DelayQueueState
        {
            private bool isQueue;
            private SortedDictionary<uint, FList<DelayedMessage>> queue;
            private SortedDictionary<uint, FList<DelayedMessage>> postWrap;
            private Dictionary<OldObjectID, FList<Tuple<uint, bool>>> delayedMessages;

            public DelayQueueState()
            {
                isQueue = false;
                queue = new SortedDictionary<uint, FList<DelayedMessage>>();
                postWrap = new SortedDictionary<uint, FList<DelayedMessage>>();
                delayedMessages = new Dictionary<OldObjectID, FList<Tuple<uint, bool>>>();
            }

            public void Add(uint currentTime, uint delay, OldObjectID dest, M message, Action<M> cancelled)
            {
                DelayedMessage dm = new DelayedMessage(dest, message, cancelled);
                uint dueTime = currentTime + delay;
                bool isQueue2;
                if (dueTime >= currentTime)
                {
                    queue.AddToList(dueTime, dm);
                    isQueue2 = isQueue;
                }
                else
                {
                    postWrap.AddToList(dueTime, dm);
                    isQueue2 = !isQueue;
                }
                delayedMessages.AddToList(dest, new Tuple<uint, bool>(dueTime, isQueue2));
            }

            public void CancelAll(OldObjectID id)
            {
                foreach (Tuple<uint, bool> times in delayedMessages.ValuesForKey(id))
                {
                    WaitCallback w = delegate(object state)
                    {
                        if (state is DelayedMessage)
                        {
                            DelayedMessage dm = (DelayedMessage)state;
                            dm.Cancelled(dm.Message);
                        }
                    };

                    Action<DelayedMessage> dispose = delegate(DelayedMessage d)
                    {
                        ThreadPool.QueueUserWorkItem(w, d);
                    };

                    if (times.Item2 == isQueue)
                    {
                        queue.FilterList(times.Item1, x => x.Dest != id, dispose);
                    }
                    else
                    {
                        postWrap.FilterList(times.Item1, x => x.Dest != id, dispose);
                    }
                }
                delayedMessages.Remove(id);
            }

            public Tuple<int, WaitType> ComputeWait(uint currentTime)
            {
                if (queue.Count > 0)
                {
                    uint firstEvent = queue.Keys.First();
                    if (firstEvent <= currentTime) return new Tuple<int, WaitType>(0, WaitType.NextEvent);

                    uint wait = firstEvent - currentTime;
                    if (wait > (uint)(int.MaxValue))
                    {
                        return new Tuple<int, WaitType>(int.MaxValue, WaitType.LongWait);
                    }
                    else
                    {
                        return new Tuple<int, WaitType>((int)wait, WaitType.NextEvent);
                    }
                }
                else
                {
                    uint wait = unchecked(0u - currentTime);
                    if (wait > (uint)(int.MaxValue))
                    {
                        return new Tuple<int, WaitType>(int.MaxValue, WaitType.LongWait);
                    }
                    else
                    {
                        return new Tuple<int, WaitType>((int)wait, WaitType.Wrap);
                    }
                }
            }

            public FList<DelayedMessage> GetNextAndRemove()
            {
                if (queue.Count == 0) throw new InvalidOperationException("No next event to remove");
                uint firstEvent = queue.Keys.First();
                FList<DelayedMessage> d = queue[firstEvent];
                queue.Remove(firstEvent);
                return d;
            }

            public void Wrap()
            {
                if (queue.Count != 0) throw new InvalidOperationException("Can't wrap while events remain in pre-wrap queue");
                isQueue = !isQueue;
                var x = queue; queue = postWrap; postWrap = x;
            }
        }

        private class State : IDisposable
        {
            private ObjectSystem<M> parent;

            private OldObjectID nextID;

            private Dictionary<OldObjectID, IMessageHandler<M>> idleObjects;
            private Dictionary<OldObjectID, Tuple<Queue<M>, IMessageHandler<M>>> busyObjects;
            private HashSet<OldObjectID> dyingObjects;

            private HashSet<int> idleThreads;
            private HashSet<int> busyThreads;

            private Queue<InternalQueueItem> globalQueue;

            private DelayQueueState delayQueueState;

            private bool isShuttingDown;

            private Action<IMessageHandler<M>, M, OldObjectID, int> beginHandling;

            public State(ObjectSystem<M> parent, int workerCount, Action<IMessageHandler<M>, M, OldObjectID, int> beginHandling)
            {
                this.parent = parent;

                nextID = OldObjectID.Zero;

                idleObjects = new Dictionary<OldObjectID,IMessageHandler<M>>();
                busyObjects = new Dictionary<OldObjectID,Tuple<Queue<M>,IMessageHandler<M>>>();
                dyingObjects = new HashSet<OldObjectID>();

                idleThreads = Enumerable.Range(0, workerCount).ToHashSet();
                busyThreads = new HashSet<int>();

                globalQueue = new Queue<InternalQueueItem>();

                delayQueueState = new DelayQueueState();

                isShuttingDown = false;

                this.beginHandling = beginHandling;
            }

            public OldObjectID AddObject(IMessageHandler<M> handler)
            {
                while (idleObjects.ContainsKey(nextID) || busyObjects.ContainsKey(nextID)) ++nextID;
                OldObjectID id = nextID;
                ++nextID;
                idleObjects.Add(id, handler);
                handler.Welcome(parent, id);
                return id;
            }

            public void RemoveObject(OldObjectID id)
            {
                if (idleObjects.ContainsKey(id))
                {
                    idleObjects[id].Dispose();
                    idleObjects.Remove(id);
                }
                else if (busyObjects.ContainsKey(id))
                {
                    dyingObjects.Add(id);
                }
                else
                {
                    throw new InvalidOperationException("RemoveObject: " + id + " not found");
                }

                delayQueueState.CancelAll(id);
            }

            public bool PostMessage(OldObjectID id, M message)
            {
                if (isShuttingDown)
                {
                    return false;
                }
                else if (idleObjects.ContainsKey(id))
                {
                    if (idleThreads.Count > 0)
                    {
                        int th = idleThreads.Min();
                        idleThreads.Remove(th);
                        IMessageHandler<M> obj = idleObjects[id];
                        idleObjects.Remove(id);
                        busyThreads.Add(th);
                        busyObjects.Add(id, new Tuple<Queue<M>, IMessageHandler<M>>(new Queue<M>(), obj));
                        beginHandling(obj, message, id, th);
                        return true;
                    }
                    else
                    {
                        globalQueue.Enqueue(new IQI_Post(id, message));
                        return true;
                    }
                }
                else if (busyObjects.ContainsKey(id))
                {
                    if (dyingObjects.Contains(id))
                    {
                        return false;
                    }
                    else
                    {
                        busyObjects[id].Item1.Enqueue(message);
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }

            public void PostMessageLater(uint currentTime, uint delay, OldObjectID id, M message, Action<M> cancelled)
            {
                delayQueueState.Add(currentTime, delay, id, message, cancelled);
            }

            public void PostMessageDueTime()
            {
                FList<DelayedMessage> d = delayQueueState.GetNextAndRemove();
                foreach (DelayedMessage dm in FListUtils.ToEnumerable(d))
                {
                    bool result = PostMessage(dm.Dest, dm.Message);
                    if (!result) dm.Cancelled(dm.Message);
                }
            }

            public bool MessageComplete(OldObjectID id, int thread)
            {
                System.Diagnostics.Debug.Assert(busyObjects.ContainsKey(id));
                System.Diagnostics.Debug.Assert(busyThreads.Contains(thread));

                if (busyObjects[id].Item1.Count == 0)
                {
                    IMessageHandler<M> handler = busyObjects[id].Item2;
                    busyObjects.Remove(id);
                    if (dyingObjects.Contains(id))
                    {
                        handler.Dispose();
                        dyingObjects.Remove(id);
                    }
                    else
                    {
                        idleObjects.Add(id, handler);
                    }
                }
                else
                {
                    globalQueue.Enqueue(new IQI_Revisit(id));
                }

                bool threadIdle = true;
                while(threadIdle && (globalQueue.Count > 0))
                {
                    InternalQueueItem iqi = globalQueue.Dequeue();
                    if (iqi is IQI_Post)
                    {
                        IQI_Post iqip = (IQI_Post)iqi;
                        if (idleObjects.ContainsKey(iqip.ID))
                        {
                            IMessageHandler<M> handler = idleObjects[iqip.ID];
                            idleObjects.Remove(iqip.ID);
                            busyObjects.Add(iqip.ID, new Tuple<Queue<M>, IMessageHandler<M>>(new Queue<M>(), handler));
                            beginHandling(handler, iqip.Message, iqip.ID, thread);
                            threadIdle = false;
                        }
                        else 
                        {
                            System.Diagnostics.Debug.Assert(busyObjects.ContainsKey(iqip.ID));
                            busyObjects[iqip.ID].Item1.Enqueue(iqip.Message);
                        }
                    }
                    else if (iqi is IQI_Revisit)
                    {
                        IQI_Revisit iqir = (IQI_Revisit)iqi;
                        Tuple<Queue<M>, IMessageHandler<M>> x = busyObjects[iqir.ID];
                        M message = x.Item1.Dequeue();
                        beginHandling(x.Item2, message, iqir.ID, thread);
                        threadIdle = false;
                    }
                }

                if (threadIdle)
                {
                    busyThreads.Remove(thread);
                    idleThreads.Add(thread);
                }

                return isShuttingDown && (busyThreads.Count == 0);
            }

            public bool Shutdown()
            {
                isShuttingDown = true;
                return isShuttingDown && (busyThreads.Count == 0);
            }

            public Tuple<int, WaitType> ComputeWait(uint currentTime)
            {
                return delayQueueState.ComputeWait(currentTime);
            }

            public void Wrap()
            {
                delayQueueState.Wrap();
            }

            public void Dispose()
            {
                System.Diagnostics.Debug.Assert(busyObjects.Count == 0);
                System.Diagnostics.Debug.Assert(busyThreads.Count == 0);

                foreach (IMessageHandler<M> handler in idleObjects.Values)
                {
                    handler.Dispose();
                }
            }
        }

        private void ThreadProc()
        {
            Action<IMessageHandler<M>, M, OldObjectID, int> launchThread = delegate(IMessageHandler<M> handler, M message, OldObjectID id, int thread)
            {
                WaitCallback w = delegate(object dummy)
                {
                    handler.Handle(message);
                    commandQueue.Put(new IC_MessageComplete(id, thread));
                };
                ThreadPool.QueueUserWorkItem(w);
            };

            using (State s = new State(this, workerCount, launchThread))
            {
                IAsyncResult aGet = commandQueue.BeginGet(null, null);
                while (true)
                {
                    uint waitStart = ExprObjModel.Utils.GetTickCount();
                    Tuple<int, WaitType> wait = s.ComputeWait(waitStart);
                    //System.Diagnostics.Debug.WriteLine("wait duration = " + wait.Item1 + ", type = " + wait.Item2);
                    int waitResult;
                    if (wait.Item1 != 0)
                    {
                        waitResult = System.Threading.WaitHandle.WaitAny(new WaitHandle[] { aGet.AsyncWaitHandle }, wait.Item1);
                    }
                    else
                    {
                        waitResult = System.Threading.WaitHandle.WaitTimeout;
                    }
                    uint waitEnd = ExprObjModel.Utils.GetTickCount();
                    if (waitResult == 0)
                    {
                        InternalCommand ic;
                        try
                        {
                            ic = commandQueue.EndGet(aGet);
                        }
                        catch(AsyncQueueClosedException)
                        {
                            Console.WriteLine("Object system disposed unexpectedly!");
                            ic = new IC_Shutdown();
                        }

                        if (ic is IC_AddObject)
                        {
                            IC_AddObject ica = (IC_AddObject)ic;
                            OldObjectID id = s.AddObject(ica.Object);
                            ica.K.Return(id);
                        }
                        else if (ic is IC_RemoveObject)
                        {
                            IC_RemoveObject icr = (IC_RemoveObject)ic;
                            s.RemoveObject(icr.ID);
                        }
                        else if (ic is IC_PostMessage)
                        {
                            IC_PostMessage icp = (IC_PostMessage)ic;
                            bool queued = s.PostMessage(icp.ID, icp.Message);
                            icp.K.Return(queued);
                        }
                        else if (ic is IC_PostMessageLater)
                        {
                            //System.Diagnostics.Debug.WriteLine("Object system thread proc: Post Message Later"); 
                            IC_PostMessageLater icpl = (IC_PostMessageLater)ic;
                            s.PostMessageLater(waitEnd, icpl.Delay_ms, icpl.ID, icpl.Message, icpl.Cancelled);
                        }
#if false
                        else if (ic is IC_PostMessageOnCompletion)
                        {
                            System.Diagnostics.Debug.WriteLine("Object system thread proc: Post Message on Completion");
                        }
#endif
                        else if (ic is IC_MessageComplete)
                        {
                            IC_MessageComplete icm = (IC_MessageComplete)ic;
                            bool breakLoop = s.MessageComplete(icm.ID, icm.Thread);
                            if (breakLoop) break;
                        }
                        else if (ic is IC_Shutdown)
                        {
                            bool breakLoop = s.Shutdown();
                            if (breakLoop) break;
                        }
                        else
                        {
                            throw new InvalidOperationException("Unknown InternalCommand");
                        }
                        aGet = commandQueue.BeginGet(null, null);
                    }
                    else
                    {
                        if (wait.Item2 == WaitType.NextEvent)
                        {
                            //System.Diagnostics.Debug.WriteLine("Object system thread proc: Post Message Due Time"); 
                            s.PostMessageDueTime();
                        }
                        else if (wait.Item2 == WaitType.Wrap)
                        {
                            while (waitEnd > 0x80000000u)
                            {
                                // too soon?
                                System.Diagnostics.Debug.WriteLine("Too soon..."); 
                                System.Threading.Thread.Sleep(1);
                                waitEnd = ExprObjModel.Utils.GetTickCount();
                            }
                            //System.Diagnostics.Debug.WriteLine("Object system thread proc: Wrap..."); 
                            s.Wrap();
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(wait.Item2 == WaitType.LongWait);
                            // do nothing and wait again
                        }
                    }
                }
            }
            commandQueue.Dispose();
        }
    }

    public static partial class Utils
    {
        public static void AddToList<K, V>(this IDictionary<K, FList<V>> dict, K key, V value)
        {
            if (dict.ContainsKey(key))
            {
                FList<V> list = dict[key];
                dict[key] = new FList<V>(value, list);
            }
            else
            {
                dict.Add(key, new FList<V>(value));
            }
        }

        public static void FilterList<K, V>(this IDictionary<K, FList<V>> dict, K key, Func<V, bool> where)
        {
            if (dict.ContainsKey(key))
            {
                FList<V> results = FListUtils.Filter(dict[key], where);
                if (results == null)
                {
                    dict.Remove(key);
                }
                else
                {
                    dict[key] = results;
                }
            }
        }

        public static void FilterList<K, V>(this IDictionary<K, FList<V>> dict, K key, Func<V, bool> where, Action<V> dispose)
        {
            if (dict.ContainsKey(key))
            {
                FList<V> results = FListUtils.FilterDispose(dict[key], where, dispose);
                if (results == null)
                {
                    dict.Remove(key);
                }
                else
                {
                    dict[key] = results;
                }
            }
        }

        public static IEnumerable<V> ValuesForKey<K, V>(this IDictionary<K, FList<V>> dict, K key)
        {
            FList<V> items = null;
            if (dict.ContainsKey(key))
            {
                items = dict[key];
            }

            while (items != null)
            {
                yield return items.Head;
                items = items.Tail;
            }
        }
    }
}