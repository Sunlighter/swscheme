using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace ControlledWindowLib.Scheduling
{
    public enum SignalStatus
    {
        New = 0,
        Signalled = 1,
        Awaited = 2,
        Retired = 3,
        Unallocated = 4,
    }

    public class Scheduler : IDisposable
    {
        private object syncRoot;
        private SignalID nextSignalID;
        private TimerID nextTimerID;
        private WaitID nextWaitID;
        private ObjectID nextObjectID;
        private Dictionary<TimerID, Timer> timers;
        private Dictionary<SignalID, Tuple<object, bool>> signals;
        private Dictionary<SignalID, WaitID> waits;
        private HashSet<SignalID> retired;
        private Dictionary<WaitID, WaitData> waitData;
        private Dictionary<ObjectID, ObjectData> objects;
        private IdleDetector idleDetector;

        public Scheduler()
        {
            this.syncRoot = new object();
            this.nextSignalID = SignalID.MinValue;
            this.nextTimerID = TimerID.MinValue;
            this.nextWaitID = WaitID.MinValue;
            this.nextObjectID = ObjectID.MinValue;
            this.timers = new Dictionary<TimerID, Timer>();
            this.signals = new Dictionary<SignalID, Tuple<object, bool>>();
            this.waits = new Dictionary<SignalID, WaitID>();
            this.retired = new HashSet<SignalID>();
            this.waitData = new Dictionary<WaitID, WaitData>();
            this.objects = new Dictionary<ObjectID, ObjectData>();
            this.idleDetector = new IdleDetector();
        }

        public SignalID GetNewSignalID()
        {
            lock (syncRoot)
            {
                SignalID s = nextSignalID;
                ++nextSignalID;
                return s;
            }
        }

        public SignalStatus GetSignalStatus(SignalID s)
        {
            lock (syncRoot)
            {
                if (s >= nextSignalID) return SignalStatus.Unallocated;
                else if (retired.Contains(s)) return SignalStatus.Retired;
                else if (signals.ContainsKey(s)) return SignalStatus.Signalled;
                else if (waits.ContainsKey(s)) return SignalStatus.Awaited;
                else return SignalStatus.New;
            }
        }

        private TimerID GetNewTimerID()
        {
            TimerID t = nextTimerID;
            ++nextTimerID;
            return t;
        }

        private WaitID GetNewWaitID()
        {
            WaitID w = nextWaitID;
            ++nextWaitID;
            return w;
        }

        private ObjectID GetNewObjectID()
        {
            ObjectID o = nextObjectID;
            ++nextObjectID;
            return o;
        }

        public void PostAction(Action a)
        {
            WaitCallback w = delegate(object dummy) { a(); idleDetector.Leave(); };
            idleDetector.Enter();
            ThreadPool.QueueUserWorkItem(w, null);
        }

        public void PostDelayedAction(long delay_ms, Action a)
        {
            TimerID t;
            lock (syncRoot)
            {
                t = GetNewTimerID();
            }

            TimerCallback tc = delegate(object dummy)
            {
                a();
                lock(syncRoot)
                {
                    timers[t].Dispose();
                    timers.Remove(t);
                }
                idleDetector.Leave();
            };
            Timer tm = new Timer(tc, null, delay_ms, Timeout.Infinite);
            idleDetector.Enter();
            lock (syncRoot)
            {
                timers.Add(t, tm);
            }
        }

        public void PostActionOnCompletion(WaitHandle h, Action a)
        {
            WaitOrTimerCallback w = delegate(object obj, bool timedOut)
            {
                a(); idleDetector.Leave();
            };
            idleDetector.Enter();
            RegisteredWaitHandle rwh = ThreadPool.RegisterWaitForSingleObject(h, w, null, Timeout.Infinite, true);
        }

        private class WaitData
        {
            private HashSet<SignalID> signals;
            private Action<SignalID, object, bool> action;

            public WaitData(HashSet<SignalID> signals, Action<SignalID, object, bool> action)
            {
                this.signals = signals;
                this.action = action;
            }

            public HashSet<SignalID> Signals { get { return signals; } }
            public Action<SignalID, object, bool> Action { get { return action; } }
        }

        public void PostWait(IEnumerable<SignalID> src, Action<SignalID, object, bool> action)
        {
            HashSet<SignalID> h = new HashSet<SignalID>(); h.UnionWith(src);
            lock(syncRoot)
            {
                if (signals.Keys.Any(x => h.Contains(x)))
                {
                    SignalID s = signals.Keys.Where(x => h.Contains(x)).First();
                    Tuple<object, bool> v = signals[s];
                    signals.Remove(s);
                    retired.Add(s);

                    Action b = delegate()
                    {
                        action(s, v.Item1, v.Item2);
                    };
                    PostAction(b);
                }
                else
                {
                    if (waits.Keys.Any(x => h.Contains(x))) throw new ArgumentException
                    (
                        "Wait already in progress on { " + waits.Keys.Where(x => h.Contains(x)).Select(x => x.id.ToString()).Concatenate(", ") + " }"
                    );
                    WaitData wd = new WaitData(h, action);
                    WaitID w = GetNewWaitID();
                    waitData.Add(w, wd);
                    idleDetector.Enter();
                    foreach(SignalID s in h)
                    {
                        waits.Add(s, w);
                    }
                }
            }
        }

        public void PostSignal(SignalID dest, object data, bool isException)
        {
            lock (syncRoot)
            {
                if (waits.ContainsKey(dest))
                {
                    WaitID w = waits[dest];
                    WaitData wd = waitData[w];
                    waitData.Remove(w);
                    retired.Add(dest);
                    foreach (SignalID s in wd.Signals) waits.Remove(s);
                    Action<SignalID, object, bool> action = wd.Action;

                    Action b = delegate()
                    {
                        action(dest, data, isException);
                    };
                    PostAction(b);
                    idleDetector.Leave();
                }
                else
                {
                    if (signals.ContainsKey(dest))
                    {
                        throw new ArgumentException("Signal " + dest.id + " already pending");
                    }
                    signals.Add(dest, new Tuple<object, bool>(data, isException));
                }
            }
        }

        public bool SignalReady(SignalID s)
        {
            lock (syncRoot)
            {
                return signals.ContainsKey(s);
            }
        }

        private class ObjectData
        {
            public bool isBusy;
            public bool isDying;
            public Action<object> handler;
            public Queue<object> queue;
        }

        public ObjectID RegisterObject(Action<object> handler)
        {
            ObjectData od = new ObjectData()
            {
                isBusy = false,
                handler = handler,
                queue = new Queue<object>()
            };

            lock (syncRoot)
            {
                ObjectID o = GetNewObjectID();
                objects.Add(o, od);
                return o;
            }
        }

        public bool UnregisterObject(ObjectID o)
        {
            return PostMessage(o, TheFinalMessage);
        }

        private class FinalMessage
        {
            public FinalMessage() { }
        }

        private static FinalMessage instance = new FinalMessage();

        public static object TheFinalMessage { get { return instance; } }

        public static bool IsTheFinalMessage(object obj)
        {
            return (obj is FinalMessage);
        }

        private void FinishMessage(ObjectID o)
        {
            Action<object> handler = null;
            object msg = null;
            bool isNotDead = true;

            lock (syncRoot)
            {
                if (objects.ContainsKey(o))
                {
                    ObjectData od = objects[o];
                    if (od.queue.Count > 0)
                    {
                        handler = od.handler;
                        msg = od.queue.Dequeue();
                    }
                    else
                    {
                        od.isBusy = false;
                        if (od.isDying)
                        {
                            objects.Remove(o);
                            isNotDead = false;
                        }
                    }
                }
            }

            if (handler != null)
            {
                handler(msg);
                if (isNotDead)
                {
                    PostAction(delegate() { FinishMessage(o); });
                }
            }
        }

        /// <summary>
        /// Do not post Scheduler.TheFinalMessage with this function; use UnregisterObject instead.
        /// </summary>
        /// <param name="o">object to post to</param>
        /// <param name="message">message to post</param>
        /// <returns>true if the message was successfully queued, false otherwise</returns>
        public bool PostMessage(ObjectID o, object message)
        {
            lock (syncRoot)
            {
                if (objects.ContainsKey(o))
                {
                    ObjectData od = objects[o];
                    if (od.isDying)
                    {
                        return false;
                    }
                    else
                    {
                        od.queue.Enqueue(message);
                        if (IsTheFinalMessage(message))
                        {
                            od.isDying = true;
                        }
                        if (!(od.isBusy))
                        {
                            od.isBusy = true;

                            PostAction
                            (
                                delegate()
                                {
                                    Action<object> handler = null;
                                    object m2 = null;

                                    lock (syncRoot)
                                    {
                                        if (od.queue.Count > 0)
                                        {
                                            handler = od.handler;
                                            m2 = od.queue.Dequeue();
                                        }
                                        else
                                        {
                                            od.isBusy = false;
                                        }
                                    }

                                    if (handler != null)
                                    {
                                        try
                                        {
                                            handler(m2);
                                        }
                                        catch (Exception exc)
                                        {
                                            // I don't know what to do with it...
                                            System.Diagnostics.Debug.WriteLine("Exception in object message handler " + o + ":\r\n" + exc);
                                        }
                                        PostAction(delegate() { FinishMessage(o); });
                                    }
                                }
                            );
                        }
                        return true;
                    }
                }
                else return false;
            }
        }

        public Tuple<SignalID, object, bool> BlockingWaitAny(IEnumerable<SignalID> src)
        {
            ManualResetEventSlim m = new ManualResetEventSlim(false);
            Tuple<SignalID, object, bool> result = null;
            PostWait
            (
                src,
                delegate(SignalID sid, object obj, bool b)
                {
                    result = new Tuple<SignalID, object, bool>(sid, obj, b);
                    m.Set();
                }
            );
            m.Wait();
            return result;
        }

        private class MessageReceiver : IMessageReceiver
        {
            private Scheduler parent;
            private object syncRoot;
            private Queue<object> sends;
            private Queue<Action<object>> receives;
            private Queue<ManualResetEventSlim> events;
            private bool isDead;
            private ObjectID id;

            public MessageReceiver(Scheduler parent)
            {
                this.parent = parent;
                this.syncRoot = new object();
                this.sends = new Queue<object>();
                this.receives = new Queue<Action<object>>();
                this.events = new Queue<ManualResetEventSlim>();
                this.isDead = false;
                this.id = parent.RegisterObject(new Action<object>(Put));
            }

            public ObjectID ID { get { return id; } }

            private ManualResetEventSlim Alloc()
            {
                if (events.Count > 0)
                {
                    return events.Dequeue();
                }
                else
                {
                    return new ManualResetEventSlim();
                }
            }

            private void Free(ManualResetEventSlim m)
            {
                if (isDead)
                {
                    m.Dispose();
                }
                else
                {
                    m.Reset();
                    events.Enqueue(m);
                }
            }

            private void Put(object msg)
            {
                lock (syncRoot)
                {
                    if (IsTheFinalMessage(msg))
                    {
                        isDead = true;
                        while (receives.Count > 0)
                        {
                            Action<object> r = receives.Dequeue();
                            r(msg);
                        }
                    }
                    else
                    {
                        if (receives.Count > 0)
                        {
                            Action<object> r = receives.Dequeue();
                            r(msg);
                        }
                        else
                        {
                            sends.Enqueue(msg);
                        }
                    }
                }
            }

            public object BlockingGet()
            {
                lock (syncRoot)
                {
                    if (sends.Count > 0)
                    {
                        return sends.Dequeue();
                    }
                    else
                    {
                        if (isDead)
                        {
                            return TheFinalMessage;
                        }
                        else
                        {
                            object result = null;
                            Action<object> recv = delegate(object msg)
                            {
                                result = msg;
                                Monitor.Pulse(syncRoot);
                            };
                            receives.Enqueue(recv);
                            Monitor.Wait(syncRoot);
                            System.Diagnostics.Debug.Assert(result != null);
                            return result;
                        }
                    }
                }
            }

            public SignalID BeginGet()
            {
                SignalID result = parent.GetNewSignalID();

                lock (syncRoot)
                {
                    if (sends.Count > 0)
                    {
                        parent.PostSignal(result, sends.Dequeue(), false);
                    }
                    else
                    {
                        if (isDead)
                        {
                            parent.PostSignal(result, TheFinalMessage, false);
                        }
                        else
                        {
                            Action<object> recv = delegate(object msg)
                            {
                                parent.PostSignal(result, msg, false);
                            };
                            receives.Enqueue(recv);
                        }
                    }
                }

                return result;
            }
        }

        public IMessageReceiver GetBlockingObject()
        {
            return new MessageReceiver(this);
        }

        public void Dispose()
        {
            idleDetector.WaitForIdle();

            idleDetector.Dispose();
        }
    }

    public interface IMessageReceiver
    {
        ObjectID ID { get; }
        object BlockingGet();
        SignalID BeginGet();
    }
}