using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ControlledWindowLib.Scheduling;
using System.IO;

namespace ControlledWindowLib
{
    public enum SendResult
    {
        Succeeded,
        Queued,
        Closed,
        Disposed,
        ResetByPeer
    }

    public enum WouldBlockResult
    {
        No,
        NoBecauseEof,
        Yes,
        Disposed
    }

    public enum ReceiveResult
    {
        OK,
        Eof,
        WouldHaveBlocked,
        Disposed
    }

    public interface IAsyncSender<T> : IDisposable
    {
        void Send(T item, Action<SendResult> a);
    }

    public interface IAsyncReceiver<T> : IDisposable
    {
        void Receive(Action<ReceiveResult, T> receive, bool allowBlock);
        void ReceiveWouldBlock(Action<WouldBlockResult> wouldBlock);
    }

    public static class AsyncPipeFactory<T>
    {
        private class CmdSend
        {
            public T item;
            public Action<SendResult> result;
        }

        private class CmdReceive
        {
            public Action<ReceiveResult, T> receive;
            public bool allowBlock;
        }

        private class CmdSendClose
        {
        }

        private class CmdReceiveWouldBlock
        {
            public Action<WouldBlockResult> a;
        }

        private class AsyncPipe
        {
            private Scheduler s;
            private ObjectID myid;

            private Queue<T> items;
            private Queue<Action<ReceiveResult, T>> waits;
            private bool isClosed;

            public AsyncPipe(Scheduler s)
            {
                this.s = s;
                this.myid = s.RegisterObject(new Action<object>(HandleMessage));
                this.items = new Queue<T>();
                this.waits = new Queue<Action<ReceiveResult, T>>();
                this.isClosed = false;
            }

            private void HandleMessage(object obj)
            {
                if (obj is CmdSend)
                {
                    CmdSend cs = (CmdSend)obj;
                    if (isClosed)
                    {
                        cs.result(SendResult.Closed);
                    }
                    else
                    {
                        if (waits.Count > 0)
                        {
                            Action<ReceiveResult, T> receive = waits.Dequeue();
                            receive(ReceiveResult.OK, cs.item);
                            cs.result(SendResult.Succeeded);
                        }
                        else
                        {
                            items.Enqueue(cs.item);
                            cs.result(SendResult.Queued);
                        }
                    }
                }
                else if (obj is CmdReceive)
                {
                    CmdReceive cr = (CmdReceive)obj;
                    if (items.Count > 0)
                    {
                        T item = items.Dequeue();
                        cr.receive(ReceiveResult.OK, item);
                    }
                    else
                    {
                        if (isClosed)
                        {
                            cr.receive(ReceiveResult.Eof, default(T));
                        }
                        else
                        {
                            if (cr.allowBlock)
                            {
                                waits.Enqueue(cr.receive);
                            }
                            else
                            {
                                cr.receive(ReceiveResult.WouldHaveBlocked, default(T));
                            }
                        }
                    }
                }
                else if (obj is CmdReceiveWouldBlock)
                {
                    CmdReceiveWouldBlock crwb = (CmdReceiveWouldBlock)obj;
                    if (items.Count > 0)
                    {
                        crwb.a(WouldBlockResult.No);
                    }
                    else
                    {
                        if (isClosed)
                        {
                            crwb.a(WouldBlockResult.NoBecauseEof);
                        }
                        else
                        {
                            crwb.a(WouldBlockResult.Yes);
                        }
                    }
                }
                else if (obj is CmdSendClose)
                {
                    isClosed = true;
                }
                else
                {
                    // ignore it, even if it's TheFinalMessage
                }
            }

            public ObjectID ID { get { return myid; } }
        }

        private class AsyncSender : IAsyncSender<T>
        {
            private Scheduler s;
            private ObjectID? id;
            private object syncRoot;

            public AsyncSender(Scheduler s, ObjectID id)
            {
                this.s = s;
                this.id = id;
                this.syncRoot = new object();
            }

            public void Send(T item, Action<SendResult> a)
            {
                Action reset = delegate()
                {
                    a(SendResult.ResetByPeer);
                };

                Action disposed = delegate()
                {
                    a(SendResult.Disposed);
                };

                lock (syncRoot)
                {
                    if (id.HasValue)
                    {
                        bool b = s.PostMessage(id.Value, new CmdSend() { item = item, result = a });
                        if (!b)
                        {
                            s.PostAction(reset);
                        }
                    }
                    else
                    {
                        s.PostAction(disposed);
                    }
                }
            }

            public void Dispose()
            {
                lock (syncRoot)
                {
                    if (id.HasValue)
                    {
                        s.UnregisterObject(id.Value);
                        id = null;
                    }
                }
            }
        }

        private class AsyncReceiver : IAsyncReceiver<T>
        {
            private Scheduler s;
            private ObjectID? id;
            private object syncRoot;

            public AsyncReceiver(Scheduler s, ObjectID id)
            {
                this.s = s;
                this.id = id;
                this.syncRoot = new object();
            }

            public void Receive(Action<ReceiveResult, T> receive, bool allowBlock)
            {
                Action reset = delegate()
                {
                    receive(ReceiveResult.Disposed, default(T));
                };

                lock (syncRoot)
                {
                    if (id.HasValue)
                    {
                        bool b = s.PostMessage(id.Value, new CmdReceive() { receive = receive, allowBlock = allowBlock });
                        if (!b)
                        {
                            id = null;
                            s.PostAction(reset);
                        }
                    }
                    else
                    {
                        s.PostAction(reset);
                    }
                }
            }

            public void ReceiveWouldBlock(Action<WouldBlockResult> wouldBlock)
            {
                Action reset = delegate()
                {
                    wouldBlock(WouldBlockResult.Disposed);
                };

                lock (syncRoot)
                {
                    if (id.HasValue)
                    {
                        bool b = s.PostMessage(id.Value, new CmdReceiveWouldBlock() { a = wouldBlock });
                        if (!b)
                        {
                            id = null;
                            s.PostAction(reset);
                        }
                    }
                    else
                    {
                        s.PostAction(reset);
                    }
                }
            }

            public void Dispose()
            {
                lock (syncRoot)
                {
                    if (id.HasValue)
                    {
                        s.UnregisterObject(id.Value);
                        id = null;
                    }
                }
            }
        }

        public static Tuple<IAsyncSender<T>, IAsyncReceiver<T>> MakeAsyncPipe(Scheduler s)
        {
            AsyncPipe a = new AsyncPipe(s);
            AsyncSender @as = new AsyncSender(s, a.ID);
            AsyncReceiver ar = new AsyncReceiver(s, a.ID);
            return new Tuple<IAsyncSender<T>, IAsyncReceiver<T>>(@as, ar);
        }
    }
}
