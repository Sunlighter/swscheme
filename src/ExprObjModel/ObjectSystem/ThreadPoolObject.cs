using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ExprObjModel.ObjectSystem
{
    class ThreadPoolObject : IMessageHandler<ExtendedMessage>
    {
        private ObjectSystem<ExtendedMessage> objectSystem;
        private OldObjectID self;
        private IGlobalState gs;

        private HashSet<int> idleThreads;
        private PriorityQueue<Tuple<int, Action>> queue;

        public ThreadPoolObject(int threadCount, IGlobalState gs)
        {
            idleThreads = Enumerable.Range(0, threadCount).ToHashSet();
            queue = new PriorityQueue<Tuple<int, Action>>(Utils.CompareBy<Tuple<int, Action>, int>(x => x.Item1));
            this.gs = gs;
        }

        #region IMessageHandler<ExtendedMessage> Members

        public void Welcome(ObjectSystem<ExtendedMessage> objectSystem, OldObjectID self)
        {
            this.objectSystem = objectSystem;
            this.self = self;
        }

        private void Dispatch()
        {
            int thread = idleThreads.First();
            idleThreads.Remove(thread);
            Tuple<int, Action> a = queue.Pop();
            WaitCallback c = delegate(object state)
            {
                try
                {
                    a.Item2();
                }
                catch (Exception exc)
                {
                    Console.WriteLine(exc);
                    // gulp
                }
                objectSystem.Post(self, new EM_PoolComplete(thread));
            };
            ThreadPool.QueueUserWorkItem(c);
        }

        public void Handle(ExtendedMessage message)
        {
            if (message is EM_PoolQueue)
            {
                EM_PoolQueue mpq = (EM_PoolQueue)message;
                queue.Push(new Tuple<int, Action>(mpq.Priority, mpq.Action));
                if (idleThreads.Count > 0) Dispatch();
            }
            else if (message is EM_PoolComplete)
            {
                EM_PoolComplete mpc = (EM_PoolComplete)message;
                idleThreads.Add(mpc.Thread);
                if (queue.Count > 0) Dispatch();
            }
            else if (message is EM_Close)
            {
                if (gs == null)
                {
                    objectSystem.RemoveObject(self);
                }
                else
                {
                    gs.RemoveOldObject(self);
                }
                this.Dispose();
            }
            else
            {
                Console.WriteLine("Thread Pool: Unknown message type: " + message.GetType().FullName);
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            // do nothing
        }

        #endregion
    }
}
