using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ControlledWindowLib;
using ControlledWindowLib.Scheduling;
using System.Drawing;
using System.Windows.Forms;
using ExprObjModel;

namespace SchedulerTest
{
    class Program
    {
        static void Test1()
        {
            using (Scheduler s = new Scheduler())
            {
                Random r = new Random();
                HashSet<SignalID> signals = new HashSet<SignalID>();
                for (int i = 0; i < 30; ++i)
                {
                    SignalID sig = s.GetNewSignalID();
                    int delay = 2000 + r.Next(10000);
                    string msg = "{ " + i + ", " + delay + " }";
                    signals.Add(sig);
                    s.PostDelayedAction
                    (
                        (long)delay,
                        delegate()
                        {
                            s.PostSignal(sig, msg, false);
                        }
                    );
                }
                while (signals.Count > 0)
                {
                    Tuple<SignalID, object, bool> t = s.BlockingWaitAny(signals);
                    signals.Remove(t.Item1);
                    Console.WriteLine(t.Item2);
                }
            }
        }

        static void Test2()
        {
            using (Scheduler s = new Scheduler())
            {
                Random r = new Random();
                IMessageReceiver mr = s.GetBlockingObject();
                ObjectID oid = mr.ID;
                int COUNT = 30;

                for (int i = 0; i < COUNT; ++i)
                {
                    int delay = 2000 + r.Next(10000);
                    string msg = "{ " + i + ", " + delay + " }";
                    s.PostDelayedAction
                    (
                        (long)delay,
                        delegate()
                        {
                            s.PostMessage(oid, msg);
                        }
                    );
                }
                for (int i = 0; i < COUNT; ++i)
                {
                    if ((i & 1) == 0)
                    {
                        object obj = mr.BlockingGet();
                        Console.WriteLine(obj);
                    }
                    else
                    {
                        SignalID sid = mr.BeginGet();
                        Tuple<SignalID, object, bool> result = s.BlockingWaitAny(new SignalID[] { sid });
                        Console.WriteLine(result.Item2);
                    }
                }
                s.PostMessage(oid, Scheduler.TheFinalMessage);
                object obj2 = mr.BlockingGet();
                Console.WriteLine(obj2);
                SignalID sid3 = mr.BeginGet();
                Tuple<SignalID, object, bool> result3 = s.BlockingWaitAny(new SignalID[] { sid3 });
                Console.WriteLine(result3.Item2);
            }
        }

        private class WindowController
        {
            private Scheduler s;
            private ObjectID window;
            private ObjectID hideNotify;
            private SignalID terminate;
            private int x;
            private int y;
            private bool closing;

            public WindowController(Scheduler s, ObjectID window, ObjectID hideNotify, SignalID terminate)
            {
                this.s = s;
                this.window = window;
                this.hideNotify = hideNotify;
                this.terminate = terminate;
                this.x = 10;
                this.y = 10;
                this.closing = false;
            }

            public void Handler(object obj)
            {
                if (obj is CW_Hello)
                {
                    s.WindowDraw(window, new Action<Graphics>(Draw));
                    s.WindowShow(window);
                }
                else if (obj is CW_KeyDown)
                {
                    KeyDown((CW_KeyDown)obj);
                    s.WindowDraw(window, new Action<Graphics>(Draw));
                }
                else if (obj is CW_CloseRequested)
                {
                    if (!closing)
                    {
                        closing = true;
                        s.WindowDraw(window, new Action<Graphics>(DrawEnd));
                        s.PostDelayedAction
                        (
                            2000L,
                            delegate()
                            {
                                s.PostMessage(window, Scheduler.TheFinalMessage);
                                s.PostSignal(terminate, null, false);
                            }
                        );
                    }
                }
            }

            private void KeyDown(CW_KeyDown k)
            {
                if (k.KeyData == Keys.Up)
                {
                    if (y > 0) --y;
                }
                else if (k.KeyData == Keys.Down)
                {
                    if (y < 20) ++y;
                }
                else if (k.KeyData == Keys.Left)
                {
                    if (x > 0) --x;
                }
                else if (k.KeyData == Keys.Right)
                {
                    if (x < 20) ++x;
                }
                else if (k.KeyData == (Keys.Up | Keys.Shift))
                {
                    y = 0;
                }
                else if (k.KeyData == (Keys.Down | Keys.Shift))
                {
                    y = 20;
                }
                else if (k.KeyData == (Keys.Left | Keys.Shift))
                {
                    x = 0;
                }
                else if (k.KeyData == (Keys.Right | Keys.Shift))
                {
                    x = 20;
                }
                else if (k.KeyData == Keys.Home)
                {
                    x = 10;
                    y = 10;
                }
                else if (k.KeyData == Keys.H)
                {
                    s.WindowHide(window);
                    s.PostMessage(hideNotify, true);
                }
            }

            public void Draw(Graphics g)
            {
                g.Clear(Color.Black);
                using (Pen p = new Pen(Color.Blue))
                {
                    for (int i = 0; i <= 21; ++i)
                    {
                        int z = i * 10 + 2;
                        g.DrawLine(p, z, 2, z, 212);
                        g.DrawLine(p, 2, z, 212, z);
                    }
                }
                using (Brush b = new SolidBrush(Color.White))
                {
                    g.FillRectangle(b, new Rectangle(x * 10 + 2, y * 10 + 2, 11, 11));
                }
            }

            public void DrawEnd(Graphics g)
            {
                g.Clear(Color.Black);
            }
        }

        static void Test3()
        {
            using (Scheduler s = new Scheduler())
            {
                SignalID s1 = s.PostCreateWindow(256, 256);
                Tuple<SignalID, object, bool> t = s.BlockingWaitAny(new SignalID[] { s1 });
                ObjectID win = (ObjectID)t.Item2;
                IMessageReceiver mr = s.GetBlockingObject();

                SignalID terminate = s.GetNewSignalID();

                WindowController wc = new WindowController(s, win, mr.ID, terminate);

                ObjectID controller = s.RegisterObject(new Action<object>(wc.Handler));

                s.WindowSetDest(win, controller, null);

                SignalID hnGet;
                while (true)
                {
                    hnGet = mr.BeginGet();
                    Tuple<SignalID, object, bool> t2 = s.BlockingWaitAny(new SignalID[] { hnGet, terminate });
                    if (t2.Item1 == hnGet)
                    {
                        Console.Write("Hidden. Press any key to show: ");
                        Console.ReadKey(true);
                        Console.WriteLine("ok.");
                        s.WindowShow(win);
                    }
                    else break;
                }
            }
        }

        static void Test4()
        {
            Func<Deque<int>> make = delegate()
            {
                Deque<int> dd = new Deque<int>();
                for (int i = 0; i < 10; ++i)
                {
                    dd.PushFront(i);
                }
                return dd;
            };

            Func<int, Deque<int>> make2 = delegate(int size)
            {
                Deque<int> dd = new Deque<int>();
                for (int i = 0; i < size; ++i)
                {
                    dd.PushFront(100 + i);
                }
                return dd;
            };

            Action<Deque<int>> print = delegate(Deque<int> dd)
            {
                Console.Write("{ ");
                bool needComma = false;
                foreach (int i in dd)
                {
                    if (needComma) Console.Write(", ");
                    Console.Write(i);
                    needComma = true;
                }
                Console.WriteLine(" }");
            };
            
            Deque<int> d;
            for (int i = 0; i <= 10; ++i)
            {
                d = make();
                d.AddAt(i, 100);
                print(d);
            }
            for (int i = 0; i <= 10; ++i)
            {
                d = make();
                d.AddAt(i, new int[] { 100, 101, 102 });
                print(d);
            }
            for (int i = 0; i <= 9; ++i)
            {
                d = make();
                d.RemoveAt(i);
                print(d);
            }
            for (int i = 0; i <= 7; ++i)
            {
                d = make();
                d.RemoveAt(i, 3);
                print(d);
            }
            for (int i = 0; i <= 5; ++i)
            {
                d = make();
                Deque<int> e = make2(i);
                d.PushBack(e);
                d.PopFront(i);
                print(d);
            }
            for (int i = 0; i <= 5; ++i)
            {
                d = make();
                Deque<int> e = make2(i);
                d.PushFront(e);
                d.PopBack(i);
                print(d);
            }
        }

        private class LWC_Self
        {
            private readonly ObjectID id;

            public LWC_Self(ObjectID id)
            {
                this.id = id;
            }

            public ObjectID ID { get { return id; } }
        }

        private class LWC_Again
        {
            readonly int delayWas;

            public LWC_Again(int delayWas)
            {
                this.delayWas = delayWas;
            }

            public int DelayWas { get { return delayWas; } }
        }

        private class LogWindowController
        {
            private Scheduler s;
            private ObjectID logwin;
            private SignalID terminate;
            private ObjectID? self;
            private Random random;
            private bool closing;

            public LogWindowController(Scheduler s, ObjectID logwin, SignalID terminate)
            {
                this.s = s;
                this.logwin = logwin;
                this.terminate = terminate;
                this.self = null;
                this.random = new Random();
            }

            private void DelayTick()
            {
                int delay = random.Next(1000) + 1000;
                s.LogWindowPost(logwin, "Delay of " + delay + " ms", false);
                s.PostDelayedAction
                (
                    (long)delay,
                    delegate()
                    {
                        s.PostMessage(self.Value, new LWC_Again(delay));
                    }
                );
            }

            public void Handler(object obj)
            {
                if (obj is LWC_Self)
                {
                    s.LogWindowPost(logwin, "Controller Self", false);
                    LWC_Self obj1 = (LWC_Self)obj;
                    self = obj1.ID;
                    DelayTick();
                }
                else if (obj is CW_Hello)
                {
                    s.LogWindowPost(logwin, "Hello", true);
                }
                else if (obj is CW_Goodbye)
                {
                    s.LogWindowPost(logwin, "Goodbye", true);
                    // should never be received
                }
                else if (obj is LWC_Again)
                {
                    LWC_Again a = (LWC_Again)obj;
                    s.LogWindowPost(logwin, "End of delay " + a.DelayWas, false);
                    if (!closing) DelayTick();
                }
                else if (obj is CW_CloseRequested)
                {
                    closing = true;
                    s.LogWindowPost(logwin, "Closing...", true);
                    s.PostDelayedAction
                    (
                        3000,
                        delegate()
                        {
                            s.PostMessage(logwin, Scheduler.TheFinalMessage);
                        }
                    );
                }
                else if (obj is CW_Closed)
                {
                    s.PostMessage(self.Value, Scheduler.TheFinalMessage);
                }
                else if (Scheduler.IsTheFinalMessage(obj))
                {
                    s.PostSignal(terminate, null, false);
                }
            }
        }

        static void Test5()
        {
            using (Scheduler s = new Scheduler())
            {
                SignalID s1 = s.PostCreateLogWindow();
                Tuple<SignalID, object, bool> t = s.BlockingWaitAny(new SignalID[] { s1 });
                ObjectID logwin = (ObjectID)t.Item2;

                SignalID terminate = s.GetNewSignalID();

                LogWindowController lwc = new LogWindowController(s, logwin, terminate);

                ObjectID controller = s.RegisterObject(new Action<object>(lwc.Handler));

                s.PostMessage(controller, new LWC_Self(controller));

                s.WindowSetDest(logwin, controller, null);

                s.BlockingWaitAny(new SignalID[] { terminate });
            }
        }

        static void Test6()
        {
            using (Scheduler s = new Scheduler())
            {

                Tuple<IAsyncSender<int>, IAsyncReceiver<int>> t = AsyncPipeFactory<int>.MakeAsyncPipe(s);

                IAsyncSender<int> ps = t.Item1;
                IAsyncReceiver<int> pr = t.Item2;

                Tuple<IAsyncSender<int>, IAsyncReceiver<int>> t2 = AsyncPipeFactory<int>.MakeAsyncPipe(s);

                IAsyncSender<int> qs = t2.Item1;
                IAsyncReceiver<int> qr = t2.Item2;

                Action<IAsyncSender<int>, int> send = delegate(IAsyncSender<int> ps1, int d)
                {
                    SignalID sig = s.GetNewSignalID();
                    ps1.Send(d, delegate(SendResult sr) { s.PostSignal(sig, sr, false); });
                    Tuple<SignalID, object, bool> b = s.BlockingWaitAny(new SignalID[] { sig });
                    SendResult s2 = (SendResult)(b.Item2);
                    if (s2 != SendResult.Succeeded && s2 != SendResult.Queued)
                    {
                        Console.WriteLine("SendResult = " + s2);
                    }
                };

                Func<IAsyncReceiver<int>, int> receive = delegate(IAsyncReceiver<int> pr1)
                {
                    SignalID sig = s.GetNewSignalID();
                    pr1.Receive(delegate(ReceiveResult rr, int dd) { s.PostSignal(sig, new Tuple<ReceiveResult, int>(rr, dd), false); }, true);
                    Tuple<SignalID, object, bool> b = s.BlockingWaitAny(new SignalID[] { sig });
                    Tuple<ReceiveResult, int> t3 = (Tuple<ReceiveResult, int>)(b.Item2);
                    if (t3.Item1 != ReceiveResult.OK)
                    {
                        Console.WriteLine("ReceiveResult = " + t3.Item1);
                    }
                    return t3.Item2;
                };

                SignalID sig1 = s.GetNewSignalID();

                Action a1 = delegate()
                {
                    for (int i = 0; i < 10; ++i)
                    {
                        send(ps, i);
                    }
                    for (int i = 0; i < 20; ++i)
                    {
                        send(ps, 100 + i);
                    }
                    send(ps, -1);
                    s.PostSignal(sig1, null, false);
                };

                SignalID sig2 = s.GetNewSignalID();

                Action a2 = delegate()
                {
                    while (true)
                    {
                        int r = receive(pr);
                        if (r == -1)
                        {
                            send(qs, -1);
                            break;
                        }
                        else
                        {
                            send(qs, r * 2);
                            send(qs, (r * 3) + 10000);
                        }
                    }
                    s.PostSignal(sig2, null, false);
                };

                SignalID sig3 = s.GetNewSignalID();

                Action a3 = delegate()
                {
                    bool needSpace = false;
                    while (true)
                    {
                        int r = receive(qr);
                        if (r == -1)
                        {
                            Console.WriteLine();
                            break;
                        }
                        else
                        {
                            if (needSpace) Console.Write(" ");
                            Console.Write(r);
                            needSpace = true;
                        }
                    }
                    s.PostSignal(sig3, null, false);
                };

                s.PostAction(a1);
                s.PostAction(a2);
                s.PostAction(a3);

                s.BlockingWaitAny(new SignalID[] { sig1 });
                s.BlockingWaitAny(new SignalID[] { sig2 });
                s.BlockingWaitAny(new SignalID[] { sig3 });

                ps.Dispose();
                pr.Dispose();
                qs.Dispose();
                qr.Dispose();

                Console.WriteLine("Done.");
            }
        }

        static void Test7()
        {
            using (Scheduler s = new Scheduler())
            {
                int count = 10;

                Action a = null;

                a = delegate()
                {
                    Console.WriteLine("count = " + count);
                    --count;
                    if (count > 0) s.PostDelayedAction(1000L, a);
                };

                int count2 = 12;

                Action b = null;

                b = delegate()
                {
                    Console.WriteLine("count2 = " + count2);
                    --count2;
                    if (count2 > 0) s.PostDelayedAction(618L, b);
                };

                s.PostAction(a);
                s.PostAction(b);
            }
            Console.WriteLine("Scheduler disposed.");
        }

        static void Main(string[] args)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.Clear();
                Console.SetBufferSize(120, 300);
                Console.SetWindowSize(120, 60);
            }
            try
            {
                Test7();
            }
            catch (Exception exc)
            {
                Console.WriteLine();
                Console.WriteLine("***** Exception! *****");
                Console.WriteLine();
                Console.WriteLine(exc);
            }
            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine("Press a key...");
                Console.ReadKey(true);
            }
        }
    }
}
