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
using System.Text;
using System.Threading;

namespace ControlledWindowLib
{
    public class AsyncQueue<T> : IDisposable
    {
        private object syncRoot;
        private Queue<T> queue;
        private Queue<AsyncResult> waiting;
        private bool isClosed;
        private bool isDisposed;

        public AsyncQueue()
        {
            this.syncRoot = new object();
            this.queue = new Queue<T>();
            this.waiting = new Queue<AsyncResult>();
            this.isClosed = false;
            this.isDisposed = false;
        }

        public void Put(T item)
        {
            lock (syncRoot)
            {
                if (isClosed) throw new AsyncQueueClosedException("Put failed");

                if (isDisposed) throw new ObjectDisposedException("Put failed");

                if (waiting.Count > 0)
                {
                    AsyncResult a = waiting.Dequeue();
                    a.Return(item);
                }
                else
                {
                    queue.Enqueue(item);
                }
            }
        }

        public void Close()
        {
            lock (syncRoot)
            {
                isClosed = true;
                while (waiting.Count > 0)
                {
                    AsyncResult a = waiting.Dequeue();
                    a.ThrowException(new AsyncQueueClosedException("Get failed"));
                }
            }
        }

        public bool IsClosed
        {
            get
            {
                lock (syncRoot)
                {
                    return isClosed;
                }
            }
        }

        public bool IsEmpty
        {
            get
            {
                lock (syncRoot)
                {
                    return isClosed && (queue.Count == 0);
                }
            }
        }

        public IAsyncResult BeginGet(AsyncCallback callback, object state)
        {
            lock (syncRoot)
            {
                if (queue.Count > 0)
                {
                    T item = queue.Dequeue();
                    AsyncResult a = new AsyncResult(callback, state, item);
                    return a;
                }
                else if (isClosed)
                {
                    AsyncResult a = new AsyncResult(callback, state);
                    a.ThrowException(new AsyncQueueClosedException("Get failed"));
                    return a;
                }
                else
                {
                    AsyncResult a = new AsyncResult(callback, state);
                    waiting.Enqueue(a);
                    return a;
                }
            }
        }

        public T EndGet(IAsyncResult result)
        {
            AsyncResult a = (AsyncResult)(result);
            a.AsyncWaitHandle.WaitOne();
            if (a.Exception != null) throw a.Exception;
            return a.Result;
        }

        public T Get()
        {
            IAsyncResult a = BeginGet(null, null);
            return EndGet(a);
        }

        private class AsyncResult : IAsyncResult, IDisposable
        {
            private object syncRoot;
            private ManualResetEvent waitHandle;
            private bool completedSynchronously;
            private bool completed;
            private T result;
            private Exception exception;
            private object state;
            private AsyncCallback callback;

            public AsyncResult(AsyncCallback callback, object state)
            {
                this.syncRoot = new object();
                this.waitHandle = new ManualResetEvent(false);
                this.completedSynchronously = true;
                this.completed = false;
                this.state = state;
                this.callback = callback;
                this.result = default(T);
                this.exception = null;
            }

            public AsyncResult(AsyncCallback callback, object state, T result)
            {
                this.syncRoot = new object();
                this.waitHandle = new ManualResetEvent(true);
                this.completedSynchronously = true;
                this.completed = true;
                this.state = state;
                this.callback = callback;
                this.result = result;
                this.exception = null;

                DoCallback();
            }

            private void DoCallback()
            {
                if (callback != null)
                {
                    WaitCallback c = delegate(object state) { callback(this); };
                    ThreadPool.QueueUserWorkItem(c);
                }
            }

            public void Return(T result)
            {
                lock (syncRoot)
                {
                    this.result = result;
                    this.completed = true;
                    this.completedSynchronously = false;
                    waitHandle.Set();
                }
                DoCallback();
            }

            public void ThrowException(Exception exc)
            {
                lock (syncRoot)
                {
                    this.exception = exc;
                    this.completed = true;
                    this.completedSynchronously = false;
                    waitHandle.Set();
                }
            }

            public Exception Exception
            {
                get { lock (syncRoot) { return exception; } }
            }

            public T Result
            {
                get { lock (syncRoot) { return result; } }
            }

            public object AsyncState
            {
                get { lock (syncRoot) { return state; } }
            }

            public System.Threading.WaitHandle AsyncWaitHandle
            {
                get { return waitHandle; }
            }

            public bool CompletedSynchronously
            {
                get { lock (syncRoot) { return completedSynchronously; } }
            }

            public bool IsCompleted
            {
                get { lock (syncRoot) { return completed; } }
            }

            public void Dispose()
            {
                waitHandle.Dispose();
            }
        }

        public void Dispose()
        {
            if (isDisposed) return;
            while (waiting.Count > 0)
            {
                AsyncResult a = waiting.Dequeue();
                a.ThrowException(new ObjectDisposedException("AsyncQueue"));
            }
            isDisposed = true;
        }
    }

    [Serializable]
    public class AsyncQueueClosedException : Exception
    {
        public AsyncQueueClosedException() { }
        public AsyncQueueClosedException(string message) : base(message) { }
        public AsyncQueueClosedException(string message, Exception inner) : base(message, inner) { }
        protected AsyncQueueClosedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
