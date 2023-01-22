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
using System.Runtime.Serialization;

namespace ExprObjModel
{
    public interface IPartialContinuation
    {
        IContinuation Attach(IContinuation theNewBase, ItemAssociation a);
    }

#if false
    public class FinalPartialContinuation : IPartialContinuation
    {
        private FinalPartialContinuation() { }
        private static FinalPartialContinuation instance;
        public static FinalPartialContinuation Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (typeof(FinalPartialContinuation))
                    {
                        if (instance == null)
                        {
                            instance = new FinalPartialContinuation();
                        }
                    }
                }
                return instance;
            }
        }

        public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
        {
            return theNewBase;
        }
    }
#endif

    public sealed class ItemAssociation
    {
        private ObjectIDGenerator g;
        private Dictionary<long, object> dict;

        public ItemAssociation()
        {
            g = new ObjectIDGenerator();
            dict = new Dictionary<long, object>();
        }

        public bool HasAssociatedItem(object obj)
        {
            bool isFirstTime;
            long l = g.HasId(obj, out isFirstTime);
            if (isFirstTime) return false;
            return dict.ContainsKey(l);
        }

        public object GetAssociatedItem(object obj)
        {
            bool isFirstTime;
            long l = g.GetId(obj, out isFirstTime);
            if (isFirstTime || !(dict.ContainsKey(l))) throw new ArgumentException("Object not found in ItemAssociation");
            return dict[l];
        }

        public void SetAssociatedItem(object key, object value)
        {
            bool isFirstTime;
            long l = g.GetId(key, out isFirstTime);
            dict[l] = value;
        }

        public delegate V ValueConstructor<V>();

        public V Assoc<K, V>(K key, ValueConstructor<V> constructor) where K: class where V: class
        {
            if (HasAssociatedItem(key))
            {
                return (V)GetAssociatedItem(key);
            }
            else
            {
                V item = constructor();
                SetAssociatedItem(key, item);
                return item;
            }
        }
    }

    public class FinalPartialContinuation : IPartialContinuation
    {
        private Symbol mark;

        public FinalPartialContinuation(Symbol mark)
        {
            this.mark = mark;
        }

        public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
        {
            return a.Assoc<FinalPartialContinuation, BaseContinuation>(this, delegate() { return new BaseContinuation(theNewBase, mark); });
        }
    }

    public class BasePartialContinuation : IPartialContinuation
    {
        private IPartialContinuation k;
        private Symbol mark;

        public BasePartialContinuation(IPartialContinuation k, Symbol mark)
        {
            this.k = k;
            this.mark = mark;
        }

        public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
        {
            return a.Assoc<BasePartialContinuation, BaseContinuation>(this, delegate() { return new BaseContinuation(k.Attach(theNewBase, a), mark); });
        }
    }

    public class BaseContinuation : IContinuation
    {
        private IContinuation k;
        private Symbol mark;

        public BaseContinuation(IContinuation k, Symbol mark)
        {
            this.k = k;
            this.mark = mark;
        }

        #region IContinuation Members

        public IRunnableStep Return(IGlobalState gs, object v) { return new RunnableReturn(k, v); }
        public IRunnableStep Throw(IGlobalState gs, object exc) { return new RunnableThrow(k, exc); }

        public IContinuation Parent { get { return k; } }
        public IProcedure EntryProc { get { return null; } }
        public IProcedure ExitProc { get { return null; } }

        public IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
        {
            if (mark == baseMark)
                return a.Assoc<BaseContinuation, FinalPartialContinuation>(this, delegate() { return new FinalPartialContinuation(mark); });
            else
                return a.Assoc<BaseContinuation, BasePartialContinuation>(this, delegate() { return new BasePartialContinuation(k.PartialCapture(baseMark, a), mark); });
        }

        public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }
        public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }

        public Symbol Mark { get { return mark; } }

        #endregion
    }

    [SchemeSingleton("call-with-base-continuation")]
    public class CallWithContinuationBase : IProcedure
    {
        public CallWithContinuationBase() { }

        #region IProcedure Members

        public int Arity { get { return 1; } }
        public bool More { get { return false; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            if (argList == null) return new RunnableThrow(k, new SchemeRuntimeException("call-with-base-continuation: Insufficient arguments"));
            if (argList.Tail != null) return new RunnableThrow(k, new SchemeRuntimeException("call-with-base-continuation: Excessive arguments"));
            if (!(argList.Head is IProcedure)) return new RunnableThrow(k, new SchemeRuntimeException("call-with-base-continuation: Procedure expected"));
            IProcedure proc = (IProcedure)(argList.Head);

            BaseContinuation k2 = new BaseContinuation(k, new Symbol());

            return new RunnableCall(proc, new FList<object>(k2.Mark), k2);
        }

        #endregion
    }

    [SchemeSingleton("call-with-partial-continuation")]
    public class CallWithPartialContinuation : IProcedure
    {
        public CallWithPartialContinuation() { }

        private class PartialContinuationProcedure : IProcedure
        {
            private IPartialContinuation pc;

            public PartialContinuationProcedure(IPartialContinuation pc)
            {
                this.pc = pc;
            }

            #region IProcedure Members

            public int Arity { get { return 1; } }
            public bool More { get { return false; } }

            public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
            {
                if (FListUtils.CountUpTo(argList, 2) != 1) return new RunnableThrow(k, new SchemeRuntimeException("partial continuation: Wrong number of arguments"));
                IContinuation ak = pc.Attach(k, new ItemAssociation());
                return ContinuationUtilities.MoveToAndReturn(k, ak, argList.Head);
            }

            #endregion
        }

        #region IProcedure Members

        public int Arity { get { return 2; } }

        public bool More { get { return false; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            if (FListUtils.CountUpTo(argList, 3) != 2) return new RunnableThrow(k, new SchemeRuntimeException("call-with-partial-continuation: Wrong number of arguments"));
            if (!(argList.Head is Symbol)) return new RunnableThrow(k, new SchemeRuntimeException("call-with-partial-continuation: First argument is not a base continuation"));
            Symbol baseMark = (Symbol)(argList.Head);
            argList = argList.Tail;
            if (!(argList.Head is IProcedure)) return new RunnableThrow(k, new SchemeRuntimeException("call-with-partial-continuation: Second argument is not a procedure"));
            IProcedure proc = (IProcedure)(argList.Head);
            if (proc.Arity > 1 || ((proc.Arity < 1) && !(proc.More))) return new RunnableThrow(k, new SchemeRuntimeException("call-with-partial-continuation: Second argument has wrong arity"));

            IContinuation k2 = k;
            while (!(k2 == null) && ((!(k2 is BaseContinuation)) || ((BaseContinuation)k2).Mark != baseMark)) k2 = k2.Parent;
            if (k2 == null) throw new SchemeRuntimeException("call-with-partial-continuation: Mark not found");
            
            IPartialContinuation pc = k.PartialCapture(baseMark, new ItemAssociation());

            return ContinuationUtilities.MoveToAndCall(k, k2.Parent, proc, new FList<object>(new PartialContinuationProcedure(pc)));
            //return new RunnableCall(proc, new FList<object>(new PartialContinuationProcedure(pc)), k);
        }

        #endregion
    }

#if false
    [SchemeSingleton("call-with-partial-exception-handler")]
    public class CallWithPartialExceptionHandler : IProcedure
    {
        public CallWithPartialExceptionHandler() { }

        private class PartialExceptionHandlerProcedure : IProcedure
        {
            private IPartialContinuation pc;

            public PartialExceptionHandlerProcedure(IPartialContinuation pc)
            {
                this.pc = pc;
            }

            #region IProcedure Members

            public int Arity { get { return 1; } }
            public bool More { get { return false; } }

            public IRunnableStep Call(FList<object> argList, IContinuation k)
            {
                if (FList<object>.CountUpTo(argList, 2) != 1) return new RunnableThrow(k, new SchemeRuntimeException("partial exception handler: Wrong number of argumnets"));
                IContinuation ak = pc.Attach(k, new ItemAssociation());
                return ContinuationUtilities.MoveToAndThrow(k, ak, argList.Head);
            }

            #endregion
        }

        #region IProcedure Members

        public int Arity { get { return 2; } }

        public bool More { get { return false; } }

        public IRunnableStep Call(FList<object> argList, IContinuation k)
        {
            if (FList<object>.CountUpTo(argList, 3) != 2) return new RunnableThrow(k, new SchemeRuntimeException("call-with-partial-exception-handler: Wrong number of arguments"));
            if (!(argList.Head is IContinuation)) return new RunnableThrow(k, new SchemeRuntimeException("call-with-partial-exception-handler: First argument is not a base continuation"));
            IContinuation theBase = (IContinuation)(argList.Head);
            argList = argList.Tail;
            if (!(argList.Head is IProcedure)) return new RunnableThrow(k, new SchemeRuntimeException("call-with-partial-exception-handler: Second argument is not a procedure"));
            IProcedure proc = (IProcedure)(argList.Head);
            if (proc.Arity > 1 || ((proc.Arity < 1) && !(proc.More))) return new RunnableThrow(k, new SchemeRuntimeException("call-with-partial-exception-handler: Second argument has wrong arity"));

            IPartialContinuation pc = k.PartialCapture(theBase, new ItemAssociation());

            return new RunnableCall(proc, new FList<object>(new PartialExceptionHandlerProcedure(pc)), k);
        }

        #endregion
    }
#endif
}