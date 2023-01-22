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
using ControlledWindowLib.Scheduling;

namespace ExprObjModel.Procedures
{
    public static partial class ProxyDiscovery
    {
        [Obsolete]
        [SchemeFunction("begin-thread-old")]
        public static AsyncID BeginThreadOld(IGlobalState gs, IProcedure schemeThreadProc)
        {
            Func<IGlobalState, IProcedure, FList<object>, DoerResult> apply = new Func<IGlobalState, IProcedure, FList<object>, DoerResult>(Doer.Apply);
            IAsyncResult iar = apply.BeginInvoke(gs, schemeThreadProc, null, null, null);

            AdvancedCompletionProc getResult = delegate(AsyncID a, IGlobalState gs2, IAsyncResult iar2)
            {
                try
                {
                    DoerResult dr = apply.EndInvoke(iar2);
                    return new WaitResult() { id = a, isException = dr.IsException, result = dr.Result };
                }
                catch (Exception exc)
                {
                    return new WaitResult() { id = a, isException = true, result = exc };
                }
            };

            AsyncID b = gs.RegisterAsync(iar, getResult, "Thread");
            return b;
        }

        [SchemeFunction("begin-thread")]
        public static SignalID BeginThread(IGlobalState gs, IProcedure schemeThreadProc)
        {
            Tuple<SignalID, IContinuation> thread = Doer.CreateThread(gs);
            Doer.PostApply(gs, schemeThreadProc, null, thread.Item2);
            gs.RegisterSignal(thread.Item1, "Thread", false);
            return thread.Item1;
        }

#if false
        [SchemeFunction("threadpool-get-max-workers")]
        public static int ThreadPoolGetMaxWorkers()
        {
            int workers;
            int completionPort;
            ThreadPool.GetMaxThreads(out workers, out completionPort);
            return workers;
        }

        [SchemeFunction("threadpool-get-max-completion-port-threads")]
        public static int ThreadPoolGetMaxCpts()
        {
            int workers;
            int completionPort;
            ThreadPool.GetMaxThreads(out workers, out completionPort);
            return completionPort;
        }

        [SchemeFunction("threadpool-get-min-workers")]
        public static int ThreadPoolGetMinWorkers()
        {
            int workers;
            int completionPort;
            ThreadPool.GetMinThreads(out workers, out completionPort);
            return workers;
        }

        [SchemeFunction("threadpool-get-min-completion-port-threads")]
        public static int ThreadPoolGetMinCpts()
        {
            int workers;
            int completionPort;
            ThreadPool.GetMinThreads(out workers, out completionPort);
            return completionPort;
        }

        [SchemeFunction("threadpool-get-available-workers")]
        public static int ThreadPoolGetAvailWorkers()
        {
            int workers;
            int completionPort;
            ThreadPool.GetAvailableThreads(out workers, out completionPort);
            return workers;
        }

        [SchemeFunction("threadpool-get-available-completion-port-threads")]
        public static int ThreadPoolGetAvailCpts()
        {
            int workers;
            int completionPort;
            ThreadPool.GetAvailableThreads(out workers, out completionPort);
            return completionPort;
        }

        [SchemeFunction("threadpool-set-max-workers!")]
        public static void ThreadPoolSetMaxWorkers(int i)
        {
            int workers;
            int completionPort;
            ThreadPool.GetMaxThreads(out workers, out completionPort);
            ThreadPool.SetMaxThreads(i, completionPort);
        }

        [SchemeFunction("threadpool-set-max-completion-port-threads!")]
        public static void ThreadPoolSetMaxCpts(int i)
        {
            int workers;
            int completionPort;
            ThreadPool.GetMaxThreads(out workers, out completionPort);
            ThreadPool.SetMaxThreads(workers, i);
        }

        [SchemeFunction("threadpool-set-min-workers!")]
        public static void ThreadPoolSetMinWorkers(int i)
        {
            int workers;
            int completionPort;
            ThreadPool.GetMinThreads(out workers, out completionPort);
            ThreadPool.SetMinThreads(i, completionPort);
        }

        [SchemeFunction("threadpool-set-min-completion-port-threads!")]
        public static void ThreadPoolSetMinCpts(int i)
        {
            int workers;
            int completionPort;
            ThreadPool.GetMinThreads(out workers, out completionPort);
            ThreadPool.SetMinThreads(workers, i);
        }
#endif
    }
}