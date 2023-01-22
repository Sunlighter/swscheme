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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ExprObjModel.SyntaxAnalysis;
using System.Linq;

namespace ExprObjModel
{
    public interface IRunnableStep
    {
        IRunnableStep Run(IGlobalState gs);
    }

    public interface IExpressionSource
    {
        EnvSpec GetRequirements();
        IExpression Compile(EnvDesc ed);
    }

    public interface IExpression
    {
        IRunnableStep Eval(IGlobalState gs, Environment env, IContinuation k);
    }

    public interface IContinuation
    {
        IRunnableStep Return(IGlobalState gs, object v);
        IRunnableStep Throw(IGlobalState gs, object exc);

        IContinuation Parent { get; }
        IProcedure EntryProc { get; }
        IProcedure ExitProc { get; }

        IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a);

        Box DynamicLookup(Symbol s);
        EnvSpec DynamicEnv { get; }
    }

    public interface IProcedure
    {
        int Arity { get; }
        bool More { get; }
        IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k);
    }

    [Serializable]
    public class RunnableEval : IRunnableStep
    {
        private IExpression expr;
        private Environment env;
        private IContinuation k;

        public RunnableEval(IExpression expr, Environment env, IContinuation k)
        {
            this.expr = expr; this.env = env; this.k = k;
        }

        public IRunnableStep Run(IGlobalState gs)
        {
            return expr.Eval(gs, env, k);
        }

        public override string ToString()
        {
            return "Eval (" + expr.GetType() + ", " + k.GetType() + ")";
        }
    }

    [Serializable]
    public class RunnableReturn : IRunnableStep
    {
        private IContinuation k;
        private object v;

        public RunnableReturn(IContinuation k, object v)
        {
            this.k = k; this.v = v;
        }

        public IRunnableStep Run(IGlobalState gs)
        {
            return k.Return(gs, v);
        }

        public override string ToString()
        {
            return "Return (" + k.GetType() + ")";
        }
    }

    [Serializable]
    public class RunnableCall : IRunnableStep
    {
        private IProcedure proc;
        private FList<object> argList;
        private IContinuation k;

        public RunnableCall(IProcedure proc, FList<object> argList, IContinuation k)
        {
            this.proc = proc; this.argList = argList; this.k = k;
        }

        public IRunnableStep Run(IGlobalState gs)
        {
            return proc.Call(gs, argList, k);
        }

        public override string ToString()
        {
            return "Call (" + proc.GetType() + ", " + k.GetType() + ")";
        }
    }

    [Serializable]
    public class RunnableThrow : IRunnableStep
    {
        private IContinuation k;
        private object exc;

        public RunnableThrow(IContinuation k, object exc)
        {
            this.k = k; this.exc = exc;
        }

        public IRunnableStep Run(IGlobalState gs)
        {
            return k.Throw(gs, exc);
        }
    }

    [Serializable]
    public class TransitionPartialContinuation : IPartialContinuation
    {
        private IPartialContinuation failure;
        private IRunnableStep success;

        public TransitionPartialContinuation(IPartialContinuation failure, IRunnableStep success)
        {
            this.failure = failure;
            this.success = success;
        }

        #region IPartialContinuation Members

        public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
        {
            return a.Assoc<TransitionPartialContinuation, TransitionContinuation>(this, delegate() { return new TransitionContinuation(failure.Attach(theNewBase, a), success); });
        }

        #endregion
    }

    [Serializable]
    public class TransitionContinuation : IContinuation
    {
        private IContinuation failure;
        private IRunnableStep success;

        public TransitionContinuation(IContinuation failure, IRunnableStep success)
        {
            this.success = success;
            this.failure = failure;
        }

        public IRunnableStep Return(IGlobalState gs, object v) { return success; }

        public IRunnableStep Throw(IGlobalState gs, object exc) { return new RunnableThrow(failure, exc); }

        public IContinuation Parent { get { return failure; } }
        public IProcedure EntryProc { get { return null; } }
        public IProcedure ExitProc { get { return null; } }

        public IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
        {
            return a.Assoc<TransitionContinuation, TransitionPartialContinuation>(this, delegate() { return new TransitionPartialContinuation(failure.PartialCapture(baseMark, a), success); });
        }

        public Box DynamicLookup(Symbol s) { return failure.DynamicLookup(s); }

        public EnvSpec DynamicEnv { get { return failure.DynamicEnv; } }
    }

    public static class ContinuationUtilities
    {
        public static IRunnableStep MoveToAndReturn(IContinuation origin, IContinuation dest, object val)
        {
            return GetTransitionSteps(GetCommonLineage(origin, dest), new RunnableReturn(dest, val));
        }

        public static IRunnableStep MoveToAndThrow(IContinuation origin, IContinuation dest, object exc)
        {
            return GetTransitionSteps(GetCommonLineage(origin, dest), new RunnableThrow(dest, exc));
        }

        public static IRunnableStep MoveToAndCall(IContinuation origin, IContinuation dest, IProcedure proc, FList<object> argList)
        {
            return GetTransitionSteps(GetCommonLineage(origin, dest), new RunnableCall(proc, argList, dest));
        }

        private static FList<IContinuation> GetLineage(IContinuation i)
        {
            FList<IContinuation> result = null;
            while (i != null)
            {
                result = new FList<IContinuation>(i, result);
                i = i.Parent;
            }
            return result;
        }

        private class CommonLineageInfo
        {
            public CommonLineageInfo(FList<IContinuation> firstLineage, IContinuation commonAncestor, FList<IContinuation> secondLineage)
            {
                this.firstLineage = firstLineage;
                this.commonAncestor = commonAncestor;
                this.secondLineage = secondLineage;
            }

            public FList<IContinuation> firstLineage;
            public IContinuation commonAncestor;
            public FList<IContinuation> secondLineage;
        }

        private static CommonLineageInfo GetCommonLineage(IContinuation first, IContinuation second)
        {
            FList<IContinuation> firstLineage = GetLineage(first);
            IContinuation commonAncestor = null;
            FList<IContinuation> secondLineage = GetLineage(second);

            while (firstLineage != null && secondLineage != null && object.ReferenceEquals(firstLineage.Head, secondLineage.Head))
            {
                commonAncestor = firstLineage.Head;
                firstLineage = firstLineage.Tail;
                secondLineage = secondLineage.Tail;
            }

            return new CommonLineageInfo(firstLineage, commonAncestor, secondLineage);
        }

        private static IRunnableStep GetTransitionStep(IProcedure transitionProcedure, IContinuation failure, IRunnableStep success)
        {
            if (transitionProcedure == null) return success;
            return new RunnableCall(transitionProcedure, null, new TransitionContinuation(failure, success));
        }

        private static IRunnableStep GetTransitionSteps(CommonLineageInfo lineageInfo, IRunnableStep finalStep)
        {
            IRunnableStep step = finalStep;
            FList<IContinuation> secondLineage = FListUtils.Reverse(lineageInfo.secondLineage);
            while (secondLineage != null)
            {
                IContinuation ancestor = (secondLineage.Tail == null) ? lineageInfo.commonAncestor : secondLineage.Tail.Head;
                step = GetTransitionStep(secondLineage.Head.EntryProc, ancestor, step);
                secondLineage = secondLineage.Tail;
            }

            FList<IContinuation> firstLineage = lineageInfo.firstLineage;
            IContinuation ancestor2 = lineageInfo.commonAncestor;
            while (firstLineage != null)
            {
                step = GetTransitionStep(firstLineage.Head.ExitProc, ancestor2, step);
                ancestor2 = firstLineage.Head;
                firstLineage = firstLineage.Tail;
            }
            return step;
        }

        public static bool AcceptsParameterCount(this IProcedure proc, int count)
        {
            if (proc.Arity == count) return true;
            if (proc.Arity < count && proc.More == true) return true;
            return false;
        }
    }

    public class LiteralSource : IExpressionSource
    {
        public LiteralSource(object literal)
        {
            this.literal = literal;
        }

        private object literal;

        public object Value { get { return literal; } }

        #region IExpressionSource Members

        public EnvSpec GetRequirements()
        {
            return EnvSpec.EmptySet;
        }

        [Serializable]
        private class LiteralExpr : IExpression
        {
            public LiteralExpr(object literal)
            {
                this.literal = literal;
            }

            private object literal;

            public IRunnableStep Eval(IGlobalState gs, Environment env, IContinuation k)
            {
                return new RunnableReturn(k, literal);
            }
        }

        public IExpression Compile(EnvDesc ed)
        {
            return new LiteralExpr(literal);
        }

        #endregion

        public static bool IsSelfEvaluating(object obj)
        {
            return (obj is bool || obj is BigMath.BigInteger || obj is BigMath.BigRational || obj is SchemeString
                || obj is char || obj is double || obj is Guid || obj is System.Net.IPAddress
                || obj is System.Net.IPEndPoint || obj is ExprObjModel.ObjectSystem.Signature
                || obj is Vector3 || obj is Vertex3 || obj is Vector2 || obj is Vertex2 || obj is Quaternion);
        }
    }

    public class VarRefSource : IExpressionSource
    {
        public VarRefSource(Symbol varname)
        {
            this.varname = varname;
        }

        private Symbol varname;

        #region IExpressionSource Members

        public EnvSpec GetRequirements()
        {
            return EnvSpec.Only(varname);
        }

        [Serializable]
        private class VarRefExpr : IExpression
        {
            private int index;

            public VarRefExpr(int index)
            {
                this.index = index;
            }

            public IRunnableStep Eval(IGlobalState gs, Environment env, IContinuation k)
            {
                return new RunnableReturn(k, env[index]);
            }
        }

        public IExpression Compile(EnvDesc ed)
        {
            return new VarRefExpr(ed[varname]);
        }

        #endregion
    }

    public class VarSetSource : IExpressionSource
    {
        public VarSetSource(Symbol varname, IExpressionSource @value)
        {
            this.varname = varname;
            this.@value = @value;
        }

        private Symbol varname;

        private IExpressionSource @value;

        #region IExpressionSource Members

        public EnvSpec GetRequirements()
        {
            return EnvSpec.Only(varname) | @value.GetRequirements();
        }

        [Serializable]
        private class VarSetExpr : IExpression
        {
            private int index;
            private IExpression @value;

            public VarSetExpr(int index, IExpression @value)
            {
                this.index = index;
                this.@value = @value;
            }

            public IRunnableStep Eval(IGlobalState gs, Environment env, IContinuation k)
            {
                IContinuation k2 = new VarSetContinuation(index, env, k);
                return new RunnableEval(@value, env, k2);
            }
        }

        private class VarSetPartialContinuation : IPartialContinuation
        {
            private int index;
            private Environment env;
            private IPartialContinuation k;

            public VarSetPartialContinuation(int index, Environment env, IPartialContinuation k)
            {
                this.index = index;
                this.env = env;
                this.k = k;
            }

            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<VarSetPartialContinuation, VarSetContinuation>(this, delegate() { return new VarSetContinuation(index, env, k.Attach(theNewBase, a)); });
            }
        }

        [Serializable]
        private class VarSetContinuation : IContinuation
        {
            private int index;
            private Environment env;
            private IContinuation k;

            public VarSetContinuation(int index, Environment env, IContinuation k)
            {
                this.index = index;
                this.env = env;
                this.k = k;
            }

            public IRunnableStep Return(IGlobalState gs, object v)
            {
                env[index] = v;
                return new RunnableReturn(k, SpecialValue.UNSPECIFIED);
            }

            public IRunnableStep Throw(IGlobalState gs, object exc) { return new RunnableThrow(k, exc); }

            public IContinuation Parent { get { return k; } }
            public IProcedure EntryProc { get { return null; } }
            public IProcedure ExitProc { get { return null; } }

            public IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
            {
                return a.Assoc<VarSetContinuation, VarSetPartialContinuation>(this, delegate() { return new VarSetPartialContinuation(index, env, k.PartialCapture(baseMark, a)); });
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }

            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }
        }

        public IExpression Compile(EnvDesc ed)
        {
            return new VarSetExpr(ed[varname], value.Compile(ed));
        }

        #endregion
    }

    [Serializable] // TODO: singleton needs a surrogate
    public class MakeUnspecified : IExpression
    {
        private MakeUnspecified() { }

        static MakeUnspecified() { instance = new MakeUnspecified(); }

        private static MakeUnspecified instance;

        public static MakeUnspecified Instance { get { return instance; } }

        public IRunnableStep Eval(IGlobalState gs, Environment env, IContinuation k)
        {
            return new RunnableReturn(k, SpecialValue.UNSPECIFIED);
        }
    }

    [Serializable] // TODO: singleton needs a surrogate
    public class MakeUnspecifiedSource : IExpressionSource
    {
        public MakeUnspecifiedSource() { }

        static MakeUnspecifiedSource() { instance = new MakeUnspecifiedSource(); }

        private static MakeUnspecifiedSource instance;
    
        public static MakeUnspecifiedSource Instance { get { return instance; } }

        #region IExpressionSource Members

        public EnvSpec GetRequirements()
        {
            return EnvSpec.EmptySet;
        }

        public IExpression Compile(EnvDesc ed)
        {
            return MakeUnspecified.Instance;
        }

        #endregion
    }

    public class BeginSource : IExpressionSource
    {
        private BeginSource(FList<IExpressionSource> exprList)
        {
            this.exprList = exprList;
        }

        private FList<IExpressionSource> exprList;

        public static IExpressionSource New(FList<IExpressionSource> exprList)
        {
            int ct = FListUtils.CountUpTo(exprList, 2);

            if (ct == 1)
            {
                return exprList.Head;
            }
            else
            {
                return new BeginSource(exprList);
            }
        }

        #region IExpressionSource Members

        public EnvSpec GetRequirements()
        {
            return ConsCell.GetRequirements(exprList);
        }

        [Serializable]
        private class BeginExpr : IExpression
        {
            public BeginExpr(IExpression head, IExpression tail)
            {
                this.head = head;
                this.tail = tail;
            }

            private IExpression head;
            private IExpression tail;

            public IRunnableStep Eval(IGlobalState gs, Environment env, IContinuation k)
            {
                IContinuation k2 = new BeginContinuation(tail, env, k);
                return new RunnableEval(head, env, k2);
            }
        }

        private class BeginPartialContinuation : IPartialContinuation
        {
            private IExpression tail;
            private Environment env;
            private IPartialContinuation k;

            public BeginPartialContinuation(IExpression tail, Environment env, IPartialContinuation k)
            {
                this.tail = tail;
                this.env = env;
                this.k = k;
            }

            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<BeginPartialContinuation, BeginContinuation>(this, delegate() { return new BeginContinuation(tail, env, k.Attach(theNewBase, a)); });
            }
        }

        [Serializable]
        private class BeginContinuation : IContinuation
        {
            private IExpression tail;
            private Environment env;
            private IContinuation k;

            public BeginContinuation(IExpression tail, Environment env, IContinuation k)
            {
                this.tail = tail;
                this.env = env;
                this.k = k;
            }

            public IRunnableStep Return(IGlobalState gs, object v)
            {
                return new RunnableEval(tail, env, k);
            }

            public IRunnableStep Throw(IGlobalState gs, object exc) { return new RunnableThrow(k, exc); }

            public IContinuation Parent { get { return k; } }
            public IProcedure EntryProc { get { return null; } }
            public IProcedure ExitProc { get { return null; } }

            public IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
            {
                return a.Assoc<BeginContinuation, BeginPartialContinuation>(this, delegate() { return new BeginPartialContinuation(tail, env, k.PartialCapture(baseMark, a)); });
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }

            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }
        }

        private IExpression MakeBeginExpr(FList<IExpression> compiledExprList)
        {
            if (compiledExprList == null)
                return MakeUnspecified.Instance;
            else
            {
                if (compiledExprList.Tail == null)
                {
                    return compiledExprList.Head;
                }
                else
                {
                    return new BeginExpr(compiledExprList.Head, MakeBeginExpr(compiledExprList.Tail));
                }
            }
        }

        public IExpression Compile(EnvDesc ed)
        {
            return MakeBeginExpr(ConsCell.CompileList(exprList, ed));
        }

        #endregion
    }

    public class IfThenElseSource : IExpressionSource
    {
        public static bool IsTrue(object v)
        {
            return (!(v is bool) || ((bool)v == true));
        }

        public IfThenElseSource(IExpressionSource test, IExpressionSource consequence, IExpressionSource alternate)
        {
            this.test = test;
            this.consequence = consequence;
            this.alternate = alternate;
        }

        public IfThenElseSource(IExpressionSource test, IExpressionSource consequence)
        {
            this.test = test;
            this.consequence = consequence;
            this.alternate = null;
        }

        private IExpressionSource test;
        private IExpressionSource consequence;
        private IExpressionSource alternate;

        #region IExpressionSource Members

        public EnvSpec GetRequirements()
        {
            EnvSpec e = test.GetRequirements() | consequence.GetRequirements();
            if (alternate != null) e |= alternate.GetRequirements();
            return e;
        }

        [Serializable]
        private class IfThenElseExpr : IExpression
        {
            public IfThenElseExpr(IExpression test, IExpression conseq, IExpression alt)
            {
                this.test = test;
                this.conseq = conseq;
                this.alt = alt;
            }

            private IExpression test;
            private IExpression conseq;
            private IExpression alt;

            public IRunnableStep Eval(IGlobalState gs, Environment env, IContinuation k)
            {
                IContinuation k2 = new IfThenElseContinuation(conseq, alt, env, k);
                return new RunnableEval(test, env, k2);
            }
        }

        private class IfThenElsePartialContinuation : IPartialContinuation
        {
            private IExpression conseq;
            private IExpression alt;
            private Environment env;
            private IPartialContinuation k;

            public IfThenElsePartialContinuation(IExpression conseq, IExpression alt, Environment env, IPartialContinuation k)
            {
                this.conseq = conseq;
                this.alt = alt;
                this.env = env;
                this.k = k;
            }

            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<IfThenElsePartialContinuation, IfThenElseContinuation>(this, delegate() { return new IfThenElseContinuation(conseq, alt, env, k.Attach(theNewBase, a)); });
            }
        }

        [Serializable]
        private class IfThenElseContinuation : IContinuation
        {
            private IExpression conseq;
            private IExpression alt;
            private Environment env;
            private IContinuation k;

            public IfThenElseContinuation(IExpression conseq, IExpression alt, Environment env, IContinuation k)
            {
                this.conseq = conseq;
                this.alt = alt;
                this.env = env;
                this.k = k;
            }

            public IRunnableStep Return(IGlobalState gs, object v)
            {
                if (IsTrue(v))
                {
                    return new RunnableEval(conseq, env, k);
                }
                else
                {
                    return new RunnableEval(alt, env, k);
                }
            }

            public IRunnableStep Throw(IGlobalState gs, object exc) { return new RunnableThrow(k, exc); }

            public IContinuation Parent { get { return k; } }
            public IProcedure EntryProc { get { return null; } }
            public IProcedure ExitProc { get { return null; } }

            #region IContinuation Members


            public IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
            {
                return a.Assoc<IfThenElseContinuation, IfThenElsePartialContinuation>(this, delegate() { return new IfThenElsePartialContinuation(conseq, alt, env, k.PartialCapture(baseMark, a)); });
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }

            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }

            #endregion
        }

        public IExpression Compile(EnvDesc ed)
        {
            IExpression testC = test.Compile(ed);
            IExpression conseqC = consequence.Compile(ed);
            IExpression altC = (alternate == null) ? MakeUnspecified.Instance : alternate.Compile(ed);
            return new IfThenElseExpr(testC, conseqC, altC);
        }

        #endregion
    }

    [Serializable]
    public class CaseClause
    {
        private SchemeHashSet items;
        private IExpressionSource clause;

        public CaseClause(SchemeHashSet items, IExpressionSource clause)
        {
            this.items = items;
            this.clause = clause;
        }

        public SchemeHashSet Items { get { return items; } }
        public IExpressionSource Clause { get { return clause; } }
    }

    [Serializable]
    public class CaseSource : IExpressionSource
    {
        private IExpressionSource expr;
        private FList<CaseClause> clauses;
        private IExpressionSource elseClause;

        public CaseSource(IExpressionSource expr, FList<CaseClause> clauses, IExpressionSource elseClause)
        {
            this.expr = expr;
            this.clauses = clauses;
            this.elseClause = elseClause;
        }

        public EnvSpec GetRequirements()
        {
            EnvSpec e = expr.GetRequirements();
            e |= FListUtils.ToEnumerable(clauses).Select(x => x.Clause.GetRequirements()).EnvSpecUnion();
            e |= elseClause.GetRequirements();
            return e;
        }

        [Serializable]
        private class CaseExpr : IExpression
        {
            private IExpression expr;
            private SchemeHashMap map;
            private IExpression elseClause;

            public CaseExpr(IExpression expr, SchemeHashMap map, IExpression elseClause)
            {
                this.expr = expr;
                this.map = map;
                this.elseClause = elseClause;
            }

            public IRunnableStep Eval(IGlobalState gs, Environment env, IContinuation k)
            {
                return new RunnableEval(expr, env, new CaseContinuation(map, elseClause, env, k));
            }
        }

        [Serializable]
        private class CasePartialContinuation : IPartialContinuation
        {
            private SchemeHashMap map;
            private IExpression elseClause;
            private Environment env;
            private IPartialContinuation k;

            public CasePartialContinuation(SchemeHashMap map, IExpression elseClause, Environment env, IPartialContinuation k)
            {
                this.map = map;
                this.elseClause = elseClause;
                this.env = env;
                this.k = k;
            }

            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<CasePartialContinuation, CaseContinuation>(this, delegate() { return new CaseContinuation(map, elseClause, env, k.Attach(theNewBase, a)); });
            }
        }

        [Serializable]
        private class CaseContinuation : IContinuation
        {
            private SchemeHashMap map;
            private IExpression elseClause;
            private Environment env;
            private IContinuation k;

            public CaseContinuation(SchemeHashMap map, IExpression elseClause, Environment env, IContinuation k)
            {
                this.map = map;
                this.elseClause = elseClause;
                this.env = env;
                this.k = k;
            }

            public IRunnableStep Return(IGlobalState gs, object v)
            {
                IExpression e = elseClause;
                if (Procedures.ProxyDiscovery.IsHashable(v))
                {
                    if (map.ContainsKey(v))
                    {
                        e = (IExpression)(map[v]);
                    }
                }
                return new RunnableEval(e, env, k);
            }

            public IRunnableStep Throw(IGlobalState gs, object exc)
            {
                return new RunnableThrow(k, exc);
            }

            public IContinuation Parent { get { return k; } }
            public IProcedure EntryProc { get { return null; } }
            public IProcedure ExitProc { get { return null; } }

            public IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
            {
                return a.Assoc<CaseContinuation, CasePartialContinuation>(this, delegate() { return new CasePartialContinuation(map, elseClause, env, k.PartialCapture(baseMark, a)); });
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }
            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }
        }

        public IExpression Compile(EnvDesc ed)
        {
            IExpression testExpr = expr.Compile(ed);
            SchemeHashMap map = new SchemeHashMap();
            foreach (CaseClause clause in FListUtils.ToEnumerable(clauses))
            {
                IExpression trueClause = clause.Clause.Compile(ed);
                foreach (object item in clause.Items)
                {
                    if (Procedures.ProxyDiscovery.IsHashable(item))
                    {
                        if (!(map.ContainsKey(item)))
                        {
                            map[item] = trueClause;
                        }
                    }
                }
            }
            IExpression elseClauseExpr = elseClause.Compile(ed);
            return new CaseExpr(testExpr, map, elseClauseExpr);
        }
    }

    public class LambdaSource : IExpressionSource
    {
        private IExpressionSource body;
        private Symbol[] lambdaParams;
        private bool more;

        public LambdaSource(object paramList, IExpressionSource body)
        {
            AnalyzeParamList(paramList, out lambdaParams, out more);
            this.body = body;
        }

        #region IExpressionSource Members

        private static void AnalyzeParamList(object paramList, out Symbol[] @params, out bool more)
        {
            int listLength = ParamCountFromList(paramList);
            more = false;
            @params = new Symbol[listLength];

            int i = 0;
            while (paramList is ConsCell)
            {
                ConsCell cc = (ConsCell)paramList;
                @params[i++] = (Symbol)cc.car;
                paramList = cc.cdr;
            }
            if (paramList is Symbol)
            {
                @params[i++] = (Symbol)paramList;
                more = true;
            }
            System.Diagnostics.Debug.Assert(i == listLength);
        }

        private static int ParamCountFromList(object list)
        {
            int result = 0;
            while (list is ConsCell)
            {
                ConsCell l1 = (ConsCell)list;
                list = l1.cdr;
                ++result;
            }
            if (list is Symbol)
            {
                ++result;
            }
            else if (list is SpecialValue && ((SpecialValue)list == SpecialValue.EMPTY_LIST))
            {
                // ok
            }
            else throw new SchemeSyntaxException("Improper parameter list");
            return result;
        }

        public static object[] ParseArgList(int minParams, bool more, FList<object> argList)
        {
            object[] result = new object[minParams + (more ? 1 : 0)];
            for (int i = 0; i < minParams; ++i)
            {
                if (argList == null) throw new SchemeSyntaxException("Too few arguments");
                result[i] = argList.Head;
                argList = argList.Tail;
            }
            if (more)
            {
                FList<object> argListRev = FListUtils.Reverse(argList);
                object regularList = SpecialValue.EMPTY_LIST;
                foreach (object obj in FListUtils.ToEnumerable(argListRev))
                {
                    regularList = new ConsCell(obj, regularList);
                }
                result[minParams] = regularList;
            }
            else
            {
                if (argList != null)
                {
                    throw new SchemeSyntaxException("Too many arguments");
                }
            }

            return result;
        }

        public EnvSpec GetRequirements()
        {
            return body.GetRequirements() - EnvSpec.FromArray(lambdaParams);
        }

        [Serializable]
        private class LambdaExpr : IExpression
        {
            private int minParams;
            private bool more;
            private IExpression body;
            private int[] mapping;

            public LambdaExpr(int minParams, bool more, int[] mapping, IExpression body)
            {
                this.minParams = minParams;
                this.more = more;
                this.mapping = mapping;
                this.body = body;
            }

            public IRunnableStep Eval(IGlobalState gs, Environment env, IContinuation k)
            {
                Environment captured = env.Extend(mapping, 0);
                IProcedure proc = new LambdaProcedure(minParams, more, body, captured);
                return new RunnableReturn(k, proc);
            }
        }

        [Serializable]
        private class LambdaProcedure : IProcedure
        {
            private int minParams;
            private bool more;
            private IExpression body;
            private Environment captured;

            public LambdaProcedure(int minParams, bool more, IExpression body, Environment captured)
            {
                this.minParams = minParams;
                this.more = more;
                this.body = body;
                this.captured = captured;
            }

            public int Arity { get { return minParams; } }
            public bool More { get { return more; } }

            public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
            {
                Environment extended = captured.Extend(ParseArgList(minParams, more, argList));
                return new RunnableEval(body, extended, k);
            }
        }

        public IExpression Compile(EnvDesc ed)
        {
            EnvDesc edInner;
            int[] captures;
            ed.SubsetShadowExtend(GetRequirements(), lambdaParams, out edInner, out captures);

            // compile the body with the new EnvDesc

            IExpression bodyC = body.Compile(edInner);
            return new LambdaExpr(more ? lambdaParams.Length - 1 : lambdaParams.Length, more, captures, bodyC);
        }

        #endregion
    }

    public class InvocationSource : IExpressionSource
    {
        private FList<IExpressionSource> exprList;

        public InvocationSource(FList<IExpressionSource> exprList)
        {
            this.exprList = exprList;
        }

        #region IExpressionSource Members

        public EnvSpec GetRequirements()
        {
            EnvSpec e = EnvSpec.EmptySet;
            foreach (IExpressionSource expr in FListUtils.ToEnumerable(exprList))
            {
                e |= expr.GetRequirements();
            }
            return e;
        }

        [Serializable]
        private class InvocationExpr : IExpression
        {
            private FList<IExpression> exprList;

            public InvocationExpr(FList<IExpression> exprList)
            {
                this.exprList = exprList;
            }

            public IRunnableStep Eval(IGlobalState gs, Environment env, IContinuation k)
            {
                IExpression expr = exprList.Head;
                IContinuation k2 = new InvocationContinuation(null, exprList.Tail, env, k);
                return new RunnableEval(expr, env, k2);
            }
        }

        private class InvocationPartialContinuation : IPartialContinuation
        {
            private FList<object> argList;
            private FList<IExpression> exprList;
            private Environment env;
            private IPartialContinuation k;

            public InvocationPartialContinuation(FList<object> argList, FList<IExpression> exprList, Environment env, IPartialContinuation k)
            {
                this.argList = argList;
                this.exprList = exprList;
                this.env = env;
                this.k = k;
            }
        
            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<InvocationPartialContinuation, InvocationContinuation>(this, delegate() { return new InvocationContinuation(argList, exprList, env, k.Attach(theNewBase, a)); });
            }
        }

        [Serializable]
        private class InvocationContinuation : IContinuation
        {
            private FList<object> argList;
            private FList<IExpression> exprList;
            private Environment env;
            private IContinuation k;

            public InvocationContinuation(FList<object> argList, FList<IExpression> exprList, Environment env, IContinuation k)
            {
                this.argList = argList;
                this.exprList = exprList;
                this.env = env;
                this.k = k;
            }

            public IRunnableStep Return(IGlobalState gs, object v)
            {
                if (exprList != null)
                {
                    return new RunnableEval(exprList.Head, env, new InvocationContinuation(new FList<object>(v, argList), exprList.Tail, env, k));
                }
                else
                {
                    if (!(v is IProcedure)) throw new SchemeRuntimeException("Value is not a procedure");
                    IProcedure proc = (IProcedure)v;
                    return new RunnableCall(proc, argList, k);
                }
            }

            public IRunnableStep Throw(IGlobalState gs, object exc) { return new RunnableThrow(k, exc); }

            public IContinuation Parent { get { return k; } }
            public IProcedure EntryProc { get { return null; } }
            public IProcedure ExitProc { get { return null; } }

            public IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
            {
                return a.Assoc<InvocationContinuation, InvocationPartialContinuation>(this, delegate() { return new InvocationPartialContinuation(argList, exprList, env, k.PartialCapture(baseMark, a)); });
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }

            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }
        }

        public IExpression Compile(EnvDesc ed)
        {
            FList<IExpression> compiledExprList = FListUtils.ReverseMap
            (
                exprList,
                delegate(IExpressionSource src)
                {
                    return src.Compile(ed);
                }
            );
            return new InvocationExpr(compiledExprList);
        }

        #endregion
    }

    public class QuasiConsSource : IExpressionSource
    {
        public QuasiConsSource(IExpressionSource car, IExpressionSource cdr)
        {
            this.car = car;
            this.cdr = cdr;
        }

        private IExpressionSource car;
        private IExpressionSource cdr;

        #region IExpressionSource Members

        public EnvSpec GetRequirements()
        {
            return car.GetRequirements() | cdr.GetRequirements();
        }

        [Serializable]
        private class QuasiConsExpr : IExpression
        {
            public QuasiConsExpr(IExpression car, IExpression cdr)
            {
                this.car = car;
                this.cdr = cdr;
            }

            private IExpression car;
            private IExpression cdr;

            public IRunnableStep Eval(IGlobalState gs, Environment env, IContinuation k)
            {
                return new RunnableEval(car, env, new QuasiConsContinuation1(cdr, env, k));
            }
        }

        private class QuasiConsPartialContinuation1 : IPartialContinuation
        {
            private IExpression cdr;
            private Environment env;
            private IPartialContinuation k;

            public QuasiConsPartialContinuation1(IExpression cdr, Environment env, IPartialContinuation k)
            {
                this.cdr = cdr;
                this.env = env;
                this.k = k;
            }

            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<QuasiConsPartialContinuation1, QuasiConsContinuation1>(this, delegate() { return new QuasiConsContinuation1(cdr, env, k.Attach(theNewBase, a)); });
            }
        }

        [Serializable]
        private class QuasiConsContinuation1 : IContinuation
        {
            private IExpression cdr;
            private Environment env;
            private IContinuation k;

            public QuasiConsContinuation1(IExpression cdr, Environment env, IContinuation k)
            {
                this.cdr = cdr;
                this.env = env;
                this.k = k;
            }

            public IRunnableStep Return(IGlobalState gs, object v)
            {
                return new RunnableEval(cdr, env, new QuasiConsContinuation2(v, k));
            }

            public IRunnableStep Throw(IGlobalState gs, object exc) { return new RunnableThrow(k, exc); }

            public IContinuation Parent { get { return k; } }
            public IProcedure EntryProc { get { return null; } }
            public IProcedure ExitProc { get { return null; } }

            public IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
            {
                return a.Assoc<QuasiConsContinuation1, QuasiConsPartialContinuation1>(this, delegate() { return new QuasiConsPartialContinuation1(cdr, env, k.PartialCapture(baseMark, a)); });
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }

            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }
        }

        private class QuasiConsPartialContinuation2 : IPartialContinuation
        {
            private object car;
            private IPartialContinuation k;

            public QuasiConsPartialContinuation2(object car, IPartialContinuation k)
            {
                this.car = car;
                this.k = k;
            }

            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<QuasiConsPartialContinuation2, QuasiConsContinuation2>(this, delegate() { return new QuasiConsContinuation2(car, k.Attach(theNewBase, a)); });
            }
        }

        [Serializable]
        private class QuasiConsContinuation2 : IContinuation
        {
            private object car;
            private IContinuation k;

            public QuasiConsContinuation2(object car, IContinuation k)
            {
                this.car = car;
                this.k = k;
            }

            public IRunnableStep Return(IGlobalState gs, object v)
            {
                return new RunnableReturn(k, new ConsCell(car, v));
            }

            public IRunnableStep Throw(IGlobalState gs, object exc) { return new RunnableThrow(k, exc); }

            public IContinuation Parent { get { return k; } }
            public IProcedure EntryProc { get { return null; } }
            public IProcedure ExitProc { get { return null; } }

            public IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
            {
                return a.Assoc<QuasiConsContinuation2, QuasiConsPartialContinuation2>(this, delegate() { return new QuasiConsPartialContinuation2(car, k.PartialCapture(baseMark, a)); });
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }

            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }
        }

        public IExpression Compile(EnvDesc ed)
        {
            return new QuasiConsExpr(car.Compile(ed), cdr.Compile(ed));
        }

        #endregion
    }

    public class QuasiAppendSource : IExpressionSource
    {
        public QuasiAppendSource(IExpressionSource a, IExpressionSource b)
        {
            this.a = a;
            this.b = b;
        }

        private IExpressionSource a;
        private IExpressionSource b;

        #region IExpressionSource Members

        [Serializable]
        private class QuasiAppendExpr : IExpression
        {
            public QuasiAppendExpr(IExpression a, IExpression b)
            {
                this.a = a;
                this.b = b;
            }

            private IExpression a;
            private IExpression b;

            public IRunnableStep Eval(IGlobalState gs, Environment env, IContinuation k)
            {
                return new RunnableEval(a, env, new QuasiAppendContinuation1(b, env, k));
            }
        }

        private class QuasiAppendPartialContinuation1 : IPartialContinuation
        {
            private IExpression b;
            private Environment env;
            private IPartialContinuation k;

            public QuasiAppendPartialContinuation1(IExpression b, Environment env, IPartialContinuation k)
            {
                this.b = b;
                this.env = env;
                this.k = k;
            }

            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<QuasiAppendPartialContinuation1, QuasiAppendContinuation1>(this, delegate() { return new QuasiAppendContinuation1(b, env, k.Attach(theNewBase, a)); });
            }
        }

        [Serializable]
        private class QuasiAppendContinuation1 : IContinuation
        {
            private IExpression b;
            private Environment env;
            private IContinuation k;

            public QuasiAppendContinuation1(IExpression b, Environment env, IContinuation k)
            {
                this.b = b;
                this.env = env;
                this.k = k;
            }

            public IRunnableStep Return(IGlobalState gs, object a)
            {
                return new RunnableEval(b, env, new QuasiAppendContinuation2(a, k));
            }

            public IRunnableStep Throw(IGlobalState gs, object exc) { return new RunnableThrow(k, exc); }

            public IContinuation Parent { get { return k; } }
            public IProcedure EntryProc { get { return null; } }
            public IProcedure ExitProc { get { return null; } }

            public IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
            {
                return a.Assoc<QuasiAppendContinuation1, QuasiAppendPartialContinuation1>(this, delegate() { return new QuasiAppendPartialContinuation1(b, env, k.PartialCapture(baseMark, a)); });
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }

            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }
        }

        private class QuasiAppendPartialContinuation2 : IPartialContinuation
        {
            private object a;
            private IPartialContinuation k;

            public QuasiAppendPartialContinuation2(object a, IPartialContinuation k)
            {
                this.a = a;
                this.k = k;
            }

            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<QuasiAppendPartialContinuation2, QuasiAppendContinuation2>(this, delegate() { return new QuasiAppendContinuation2(a, k.Attach(theNewBase, a)); });
            }
        }

        [Serializable]
        private class QuasiAppendContinuation2 : IContinuation
        {
            private object a;
            private IContinuation k;

            public QuasiAppendContinuation2(object a, IContinuation k)
            {
                this.a = a;
                this.k = k;
            }

            public IRunnableStep Return(IGlobalState gs, object b)
            {
                return new RunnableReturn(k, ConsCell.Append(a, b));
            }

            public IRunnableStep Throw(IGlobalState gs, object exc) { return new RunnableThrow(k, exc); }

            public IContinuation Parent { get { return k; } }
            public IProcedure EntryProc { get { return null; } }
            public IProcedure ExitProc { get { return null; } }

            public IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
            {
                return a.Assoc<QuasiAppendContinuation2, QuasiAppendPartialContinuation2>(this, delegate() { return new QuasiAppendPartialContinuation2(a, k.PartialCapture(baseMark, a)); });
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }

            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }
        }

        public EnvSpec GetRequirements()
        {
            return a.GetRequirements() | b.GetRequirements();
        }

        public IExpression Compile(EnvDesc ed)
        {
            return new QuasiAppendExpr(a.Compile(ed), b.Compile(ed));
        }

        #endregion
    }

    public class QuasiListToVectorSource : IExpressionSource
    {
        public QuasiListToVectorSource(IExpressionSource expr)
        {
            this.expr = expr;
        }

        private IExpressionSource expr;

        #region IExpressionSource Members

        public EnvSpec GetRequirements()
        {
            return expr.GetRequirements();
        }

        [Serializable]
        private class QuasiListToVectorExpr : IExpression
        {
            public QuasiListToVectorExpr(IExpression expr)
            {
                this.expr = expr;
            }

            private IExpression expr;

            public IRunnableStep Eval(IGlobalState gs, Environment env, IContinuation k)
            {
                return new RunnableEval(expr, env, new QuasiListToVectorContinuation(k));
            }
        }

        private class QuasiListToVectorPartialContinuation : IPartialContinuation
        {
            private IPartialContinuation k;

            public QuasiListToVectorPartialContinuation(IPartialContinuation k)
            {
                this.k = k;
            }

            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<QuasiListToVectorPartialContinuation, QuasiListToVectorContinuation>(this, delegate() { return new QuasiListToVectorContinuation(k.Attach(theNewBase, a)); });
            }
        }

        [Serializable]
        private class QuasiListToVectorContinuation : IContinuation
        {
            private IContinuation k;

            public QuasiListToVectorContinuation(IContinuation k)
            {
                this.k = k;
            }

            public IRunnableStep Return(IGlobalState gs, object v)
            {
                return new RunnableReturn(k, ConsCell.ListToVector(v));
            }

            public IRunnableStep Throw(IGlobalState gs, object exc) { return new RunnableThrow(k, exc); }

            public IContinuation Parent { get { return k; } }
            public IProcedure EntryProc { get { return null; } }
            public IProcedure ExitProc { get { return null; } }

            public IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
            {
                return a.Assoc<QuasiListToVectorContinuation, QuasiListToVectorPartialContinuation>(this, delegate() { return new QuasiListToVectorPartialContinuation(k.PartialCapture(baseMark, a)); });
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }

            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }
        }

        public IExpression Compile(EnvDesc ed)
        {
            return new QuasiListToVectorExpr(expr.Compile(ed));
        }

        #endregion
    }

    public class AndSource : IExpressionSource
    {
        private AndSource(FList<IExpressionSource> exprList)
        {
            this.exprList = exprList;
        }

        private FList<IExpressionSource> exprList;

        public static IExpressionSource New(FList<IExpressionSource> exprList)
        {
            if (exprList == null)
            {
                return new LiteralSource(false);
            }
            else if (exprList.Tail == null)
            {
                return exprList.Head;
            }
            else
            {
                return new AndSource(exprList);
            }
        }

        #region IExpressionSource Members

        public EnvSpec GetRequirements()
        {
            return ConsCell.GetRequirements(exprList);
        }

        [Serializable]
        private class AndExpr : IExpression
        {
            public AndExpr(IExpression head, IExpression tail)
            {
                this.head = head;
                this.tail = tail;
            }

            private IExpression head;
            private IExpression tail;

            #region IExpression Members

            public IRunnableStep Eval(IGlobalState gs, Environment env, IContinuation k)
            {
                return new RunnableEval(head, env, new AndContinuation(tail, env, k));
            }

            #endregion
        }

        private IExpression BuildAndExpr(FList<IExpression> compiledExprList)
        {
            if ((compiledExprList != null) && (compiledExprList.Tail == null))
            {
                return compiledExprList.Head;
            }
            else
            {
                return new AndExpr(compiledExprList.Head, BuildAndExpr(compiledExprList.Tail));
            }
        }

        private class AndPartialContinuation : IPartialContinuation
        {
            private IExpression tail;
            private Environment env;
            private IPartialContinuation k;

            public AndPartialContinuation(IExpression tail, Environment env, IPartialContinuation k)
            {
                this.tail = tail;
                this.env = env;
                this.k = k;
            }

            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<AndPartialContinuation, AndContinuation>(this, delegate() { return new AndContinuation(tail, env, k.Attach(theNewBase, a)); });
            }
        }

        [Serializable]
        private class AndContinuation : IContinuation
        {
            private IExpression tail;
            private Environment env;
            private IContinuation k;

            public AndContinuation(IExpression tail, Environment env, IContinuation k)
            {
                this.tail = tail;
                this.env = env;
                this.k = k;
            }

            public IRunnableStep Return(IGlobalState gs, object v)
            {
                if (v is bool && ((bool)v) == false)
                {
                    return new RunnableReturn(k, false);
                }
                else
                {
                    return new RunnableEval(tail, env, k);
                }
            }

            public IRunnableStep Throw(IGlobalState gs, object exc) { return new RunnableThrow(k, exc); }

            public IContinuation Parent { get { return k; } }
            public IProcedure EntryProc { get { return null; } }
            public IProcedure ExitProc { get { return null; } }

            public IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
            {
                return a.Assoc<AndContinuation, AndPartialContinuation>(this, delegate() { return new AndPartialContinuation(tail, env, k.PartialCapture(baseMark, a)); });
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }

            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }
        }

        public IExpression Compile(EnvDesc ed)
        {
            return BuildAndExpr(ConsCell.CompileList(exprList, ed));
        }

        #endregion
    }

    public class OrSource : IExpressionSource
    {
        private OrSource(FList<IExpressionSource> exprList)
        {
            this.exprList = exprList;
        }

        private FList<IExpressionSource> exprList;

        public static IExpressionSource New(FList<IExpressionSource> exprList)
        {
            int len = FListUtils.CountUpTo(exprList, 2);
            if (len== 0)
            {
                return new LiteralSource(true);
            }
            else if (len == 1)
            {
                return exprList.Head;
            }
            else
            {
                return new OrSource(exprList);
            }
        }

        #region IExpressionSource Members

        public EnvSpec GetRequirements()
        {
            return ConsCell.GetRequirements(exprList);
        }

        [Serializable]
        private class OrExpr : IExpression
        {
            public OrExpr(IExpression head, IExpression tail)
            {
                this.head = head;
                this.tail = tail;
            }

            private IExpression head;
            private IExpression tail;

            #region IExpression Members

            public IRunnableStep Eval(IGlobalState gs, Environment env, IContinuation k)
            {
                return new RunnableEval(head, env, new OrContinuation(tail, env, k));
            }

            #endregion
        }

        private IExpression BuildOrExpr(FList<IExpression> compiledExprList)
        {
            if ((compiledExprList != null) && (compiledExprList.Tail == null))
            {
                return compiledExprList.Head;
            }
            else
            {
                return new OrExpr(compiledExprList.Head, BuildOrExpr(compiledExprList.Tail));
            }
        }

        private class OrPartialContinuation : IPartialContinuation
        {
            private IExpression tail;
            private Environment env;
            private IPartialContinuation k;

            public OrPartialContinuation(IExpression tail, Environment env, IPartialContinuation k)
            {
                this.tail = tail;
                this.env = env;
                this.k = k;
            }

            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<OrPartialContinuation, OrContinuation>(this, delegate() { return new OrContinuation(tail, env, k.Attach(theNewBase, a)); });
            }
        }

        [Serializable]
        private class OrContinuation : IContinuation
        {
            private IExpression tail;
            private Environment env;
            private IContinuation k;

            public OrContinuation(IExpression tail, Environment env, IContinuation k)
            {
                this.tail = tail;
                this.env = env;
                this.k = k;
            }

            public IRunnableStep Return(IGlobalState gs, object v)
            {
                if (v is bool && ((bool)v) == false)
                {
                    return new RunnableEval(tail, env, k);
                }
                else
                {
                    return new RunnableReturn(k, v);
                }
            }

            public IRunnableStep Throw(IGlobalState gs, object exc) { return new RunnableThrow(k, exc); }

            public IContinuation Parent { get { return k; } }
            public IProcedure EntryProc { get { return null; } }
            public IProcedure ExitProc { get { return null; } }

            public IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
            {
                return a.Assoc<OrContinuation, OrPartialContinuation>(this, delegate() { return new OrPartialContinuation(tail, env, k.PartialCapture(baseMark, a)); });
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }

            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }
        }

        public IExpression Compile(EnvDesc ed)
        {
            return BuildOrExpr(ConsCell.CompileList(exprList, ed));
        }

        #endregion
    }

    [Serializable]
    public class LetClause
    {
        public LetClause(Symbol name, IExpressionSource valExpr)
        {
            this.name = name;
            this.valExpr = valExpr;
        }

        private Symbol name;
        private IExpressionSource valExpr;

        public Symbol Name { get { return name; } }
        public IExpressionSource Value { get { return valExpr; } }

        public static Symbol[] GetSymbols(FList<LetClause> clauseList)
        {
            int iend = FListUtils.Count(clauseList);
            Symbol[] letvars = new Symbol[iend];
            FList<LetClause> c1 = clauseList;
            for (int i = 0; i < iend; ++i)
            {
                letvars[i] = c1.Head.Name;
                c1 = c1.Tail;
            }
            return letvars;
        }
    }

    /// <summary>
    /// This class only exists for the IStringTreeNode stuff.
    /// </summary>
    [Serializable]
    public class LetClauseList
    {
        private FList<LetClause> list;

        public LetClauseList(FList<LetClause> list)
        {
            this.list = list;
        }
    }

    public class LetSource : IExpressionSource
    {
        private FList<LetClause> clauseList;
        private IExpressionSource body;

        public LetSource(FList<LetClause> clauseList, IExpressionSource body)
        {
            this.clauseList = clauseList;
            this.body = body;
        }

        #region IExpressionSource Members

        private EnvSpec GetBodyRequirements()
        {
            EnvSpec vars = EnvSpec.FromArray(LetClause.GetSymbols(clauseList));
            return body.GetRequirements() - vars;
        }

        public EnvSpec GetRequirements()
        {
            EnvSpec vars = EnvSpec.FromArray(LetClause.GetSymbols(clauseList));
            EnvSpec reqs = EnvSpec.EmptySet;
            foreach (LetClause l in FListUtils.ToEnumerable(clauseList))
            {
                reqs |= l.Value.GetRequirements();
            }
            EnvSpec reqsBody = body.GetRequirements() - vars;
            return reqs | reqsBody;
        }

        [Serializable]
        private class LetExpr : IExpression
        {
            private FList<IExpression> clauseList;
            private int[] captures;
            private IExpression body;

            public LetExpr(FList<IExpression> clauseList, int[] captures, IExpression body)
            {
                this.clauseList = clauseList;
                this.captures = captures;
                this.body = body;
            }

            public IRunnableStep Eval(IGlobalState gs, Environment env, IContinuation k)
            {
                Environment captured = env.Extend(captures, 0);
                if (clauseList == null)
                {
                    return new RunnableEval(body, captured, k);
                }
                else
                {
                    IContinuation k2 = new LetContinuation
                    (
                        null, 0,
                        clauseList.Tail,
                        captured, body, env, k
                    );
                    return new RunnableEval(clauseList.Head, env, k2);
                }
            }
        }

        private class LetPartialContinuation : IPartialContinuation
        {
            private FList<object> valuesSoFar;
            private int countSoFar;
            private FList<IExpression> clauseListTail;
            private Environment captured;
            private IExpression body;
            private Environment outerEnv;
            private IPartialContinuation k;

            public LetPartialContinuation
            (
                FList<object> valuesSoFar,
                int countSoFar,
                FList<IExpression> clauseListTail,
                Environment captured,
                IExpression body,
                Environment outerEnv,
                IPartialContinuation k
            )
            {
                this.valuesSoFar = valuesSoFar;
                this.countSoFar = countSoFar;
                this.clauseListTail = clauseListTail;
                this.captured = captured;
                this.body = body;
                this.outerEnv = outerEnv;
                this.k = k;
            }

            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<LetPartialContinuation, LetContinuation>(this, delegate() { return new LetContinuation(valuesSoFar, countSoFar, clauseListTail, captured, body, outerEnv, k.Attach(theNewBase, a)); });
            }
        }

        [Serializable]
        private class LetContinuation : IContinuation
        {
            private FList<object> valuesSoFar;
            private int countSoFar;
            private FList<IExpression> clauseListTail;
            private Environment captured;
            private IExpression body;
            private Environment outerEnv;
            private IContinuation k;

            public LetContinuation
            (
                FList<object> valuesSoFar,
                int countSoFar,
                FList<IExpression> clauseListTail,
                Environment captured,
                IExpression body,
                Environment outerEnv,
                IContinuation k
            )
            {
                this.valuesSoFar = valuesSoFar;
                this.countSoFar = countSoFar;
                this.clauseListTail = clauseListTail;
                this.captured = captured;
                this.body = body;
                this.outerEnv = outerEnv;
                this.k = k;
            }

            #region IContinuation Members

            public IRunnableStep Return(IGlobalState gs, object v)
            {
                FList<object> valuesSoFar2 = new FList<object>(v, valuesSoFar);
                int countSoFar2 = countSoFar + 1;
                if (clauseListTail == null)
                {
                    object[] values = new object[countSoFar2];
                    int index = 0;
                    while (valuesSoFar2 != null)
                    {
                        values[index++] = valuesSoFar2.Head;
                        valuesSoFar2 = valuesSoFar2.Tail;
                    }

                    Environment e2 = captured.Extend(values);
                    return new RunnableEval(body, e2, k);
                }
                else
                {
                    IContinuation k2 = new LetContinuation
                    (
                        valuesSoFar2, countSoFar2,
                        clauseListTail.Tail,
                        captured, body, outerEnv, k
                    );
                    return new RunnableEval(clauseListTail.Head, outerEnv, k2);
                }
            }

            public IRunnableStep Throw(IGlobalState gs, object exc) { return new RunnableThrow(k, exc); }

            public IContinuation Parent { get { return k; } }
            public IProcedure EntryProc { get { return null; } }
            public IProcedure ExitProc { get { return null; } }

            public IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
            {
                return a.Assoc<LetContinuation, LetPartialContinuation>(this, delegate() { return new LetPartialContinuation(valuesSoFar, countSoFar, clauseListTail, captured, body, outerEnv, k.PartialCapture(baseMark, a)); });
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }

            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }

            #endregion
        }

        public IExpression Compile(EnvDesc ed)
        {
            // produce the array of captures
            // (variables to capture are precisely those given by GetRequirements)
            // produce a new EnvDesc reflecting the captured env and params

            EnvSpec req = GetBodyRequirements();

            EnvDesc bodyEnvDesc;
            int[] captures;
            ed.SubsetShadowExtend(req, LetClause.GetSymbols(clauseList), out bodyEnvDesc, out captures);

            // compile the body with the new EnvDesc

            IExpression bodyC = body.Compile(bodyEnvDesc);
            
            // compile the let clauses with the old EnvDesc

            FList<IExpression> compiledClauseList = FListUtils.ReverseMap
            (
                clauseList,
                delegate(LetClause lc)
                {
                    return lc.Value.Compile(ed);
                }
            );

            return new LetExpr(compiledClauseList, captures, bodyC);
        }

        #endregion
    }

    public class LetStarSource : IExpressionSource
    {
        private LetClause letClause;
        private IExpressionSource body;

        public LetStarSource(FList<LetClause> clauseList, IExpressionSource body)
        {
            if (clauseList == null)
            {
                this.letClause = null;
                this.body = body;
            }
            else if (clauseList.Tail == null)
            {
                this.letClause = clauseList.Head;
                this.body = body;
            }
            else
            {
                this.letClause = clauseList.Head;
                this.body = new LetStarSource(clauseList.Tail, body);
            }
        }

        #region IExpressionSource Members

        public EnvSpec GetRequirements()
        {
            if (letClause == null)
            {
                return body.GetRequirements();
            }
            else
            {
                EnvSpec z = body.GetRequirements();
                z -= letClause.Name;
                z |= letClause.Value.GetRequirements();
                return z;
            }
        }

        [Serializable]
        private class LetStarExpr : IExpression
        {
            private int[] mapping;
            private IExpression value;
            private IExpression body;

            public LetStarExpr(int[] mapping, IExpression value, IExpression body)
            {
                this.mapping = mapping;
                this.value = value;
                this.body = body;
            }

            #region IExpression Members

            public IRunnableStep Eval(IGlobalState gs, Environment env, IContinuation k)
            {
                return new RunnableEval(value, env, new LetStarContinuation(mapping, body, env, k));
            }

            #endregion
        }

        private class LetStarPartialContinuation : IPartialContinuation
        {
            private int[] mapping;
            private IExpression body;
            private Environment env;
            private IPartialContinuation k;

            public LetStarPartialContinuation(int[] mapping, IExpression body, Environment env, IPartialContinuation k)
            {
                this.mapping = mapping;
                this.body = body;
                this.env = env;
                this.k = k;
            }

            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<LetStarPartialContinuation, LetStarContinuation>(this, delegate() { return new LetStarContinuation(mapping, body, env, k.Attach(theNewBase, a)); });
            }
        }

        [Serializable]
        private class LetStarContinuation : IContinuation
        {
            private int[] mapping;
            private IExpression body;
            private Environment env;
            private IContinuation k;

            public LetStarContinuation(int[] mapping, IExpression body, Environment env, IContinuation k)
            {
                this.mapping = mapping;
                this.body = body;
                this.env = env;
                this.k = k;
            }

            #region IContinuation Members

            public IRunnableStep Return(IGlobalState gs, object v)
            {
                Environment e2 = env.Extend(mapping, new object[] { v });
                return new RunnableEval(body, e2, k);
            }

            public IRunnableStep Throw(IGlobalState gs, object exc) { return new RunnableThrow(k, exc); }

            public IContinuation Parent { get { return k; } }
            public IProcedure EntryProc { get { return null; } }
            public IProcedure ExitProc { get { return null; } }

            public IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
            {
                return a.Assoc<LetStarContinuation, LetStarPartialContinuation>(this, delegate() { return new LetStarPartialContinuation(mapping, body, env, k.PartialCapture(baseMark, a)); });
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }

            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }

            #endregion
        }

        public IExpression Compile(EnvDesc ed)
        {
            if (letClause != null)
            {
                EnvDesc edInner;
                int[] mapping;
                ed.ShadowExtend(new Symbol[] { letClause.Name }, out edInner, out mapping);
                IExpression compiledValue = letClause.Value.Compile(ed);
                IExpression compiledBody = body.Compile(edInner);
                return new LetStarExpr(mapping, compiledValue, compiledBody);
            }
            else
            {
                return body.Compile(ed);
            }
        }

        #endregion
    }

    public class LetrecSource : IExpressionSource
    {
        private FList<LetClause> clauseList;
        private IExpressionSource body;

        public LetrecSource(FList<LetClause> clauseList, IExpressionSource body)
        {
            this.clauseList = clauseList;
            this.body = body;
        }

        #region IExpressionSource Members

        public EnvSpec GetRequirements()
        {
            EnvSpec u1 = FListUtils.ToEnumerable(clauseList).Select(l => l.Value.GetRequirements()).EnvSpecUnion();
            EnvSpec vars = EnvSpec.FromEnumerable(FListUtils.ToEnumerable(clauseList).Select(l => l.Name));
            u1 |= body.GetRequirements();
            return u1 - vars;
        }

        [Serializable]
        private class LetrecExpr : IExpression
        {
            private int[] mapping;
            private FList<IExpression> clauses;
            private int clauseCount;
            private IExpression body;

            public LetrecExpr(int[] mapping, FList<IExpression> clauses, int clauseCount, IExpression body)
            {
                this.mapping = mapping;
                this.clauses = clauses;
                this.clauseCount = clauseCount;
                this.body = body;
            }

            #region IExpression Members

            public IRunnableStep Eval(IGlobalState gs, Environment env, IContinuation k)
            {
                Environment envInner = env.Extend(mapping, clauseCount);
                int pos = mapping.Length;
                return new RunnableEval(clauses.Head, envInner, new LetrecContinuation(envInner, pos, clauses.Tail, clauseCount - 1, body, k));
            }

            #endregion
        }

        private class LetrecPartialContinuation : IPartialContinuation
        {
            private Environment env;
            private int pos;
            private FList<IExpression> clauses;
            private int clauseCount;
            private IExpression body;
            private IPartialContinuation k;

            public LetrecPartialContinuation(Environment env, int pos, FList<IExpression> clauses, int clauseCount, IExpression body, IPartialContinuation k)
            {
                this.env = env;
                this.pos = pos;
                this.clauses = clauses;
                this.clauseCount = clauseCount;
                this.body = body;
                this.k = k;
            }

            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<LetrecPartialContinuation, LetrecContinuation>(this, delegate() { return new LetrecContinuation(env, pos, clauses, clauseCount, body, k.Attach(theNewBase, a)); });
            }
        }

        [Serializable]
        private class LetrecContinuation : IContinuation
        {
            private Environment env;
            private int pos;
            private FList<IExpression> clauses;
            private int clauseCount;
            private IExpression body;
            private IContinuation k;

            public LetrecContinuation(Environment env, int pos, FList<IExpression> clauses, int clauseCount, IExpression body, IContinuation k)
            {
                this.env = env;
                this.pos = pos;
                this.clauses = clauses;
                this.clauseCount = clauseCount;
                this.body = body;
                this.k = k;
            }

            #region IContinuation Members

            public IRunnableStep Return(IGlobalState gs, object v)
            {
                env[pos] = v;
                if (clauses == null)
                {
                    return new RunnableEval(body, env, k);
                }
                else
                {
                    FList<IExpression> clauses2 = clauses.Tail;
                    int clauseCount2 = clauseCount - 1;
                    return new RunnableEval(clauses.Head, env, new LetrecContinuation(env, pos + 1, clauses.Tail, clauseCount - 1, body, k));
                }
            }

            public IRunnableStep Throw(IGlobalState gs, object exc) { return new RunnableThrow(k, exc); }

            public IContinuation Parent { get { return k; } }
            public IProcedure EntryProc { get { return null; } }
            public IProcedure ExitProc { get { return null; } }

            public IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
            {
                return a.Assoc<LetrecContinuation, LetrecPartialContinuation>(this, delegate() { return new LetrecPartialContinuation(env, pos, clauses, clauseCount, body, k.PartialCapture(baseMark, a)); });
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }

            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }

            #endregion
        }

        public IExpression Compile(EnvDesc ed)
        {
            // create EnvDesc for internal environment

            EnvDesc edInner;
            int[] mapping;
            ed.SubsetShadowExtend(GetRequirements(), LetClause.GetSymbols(clauseList), out edInner, out mapping);

            // compile each let clause's value, and the main body, in the internal environment

            FList<IExpression> compiledClauseList = FListUtils.Map
            (
                clauseList,
                delegate(LetClause s)
                {
                    return s.Value.Compile(edInner);
                }
            );

            IExpression compiledBody = body.Compile(edInner);

            return new LetrecExpr(mapping, compiledClauseList, FListUtils.Count(compiledClauseList), compiledBody);
        }

        #endregion
    }

    public class LetrecStarSource : IExpressionSource
    {
        private LetClause letClause;
        private IExpressionSource body;

        public LetrecStarSource(FList<LetClause> clauseList, IExpressionSource body)
        {
            if (clauseList == null)
            {
                this.letClause = null;
                this.body = body;
            }
            else if (clauseList.Tail == null)
            {
                this.letClause = clauseList.Head;
                this.body = body;
            }
            else
            {
                this.letClause = clauseList.Head;
                this.body = new LetrecStarSource(clauseList.Tail, body);
            }
        }

        #region IExpressionSource Members

        public EnvSpec GetRequirements()
        {
            if (letClause == null)
            {
                return body.GetRequirements();
            }
            else
            {
                EnvSpec z = body.GetRequirements();
                z |= letClause.Value.GetRequirements();
                z -= letClause.Name;
                return z;
            }
        }

        [Serializable]
        private class LetrecStarExpr : IExpression
        {
            private int[] mapping;
            private IExpression value;
            private IExpression body;

            public LetrecStarExpr(int[] mapping, IExpression value, IExpression body)
            {
                this.mapping = mapping;
                this.value = value;
                this.body = body;
            }

            #region IExpression Members

            public IRunnableStep Eval(IGlobalState gs, Environment env, IContinuation k)
            {
                Environment e2 = env.Extend(mapping, 1);
                int index = mapping.Length;
                return new RunnableEval(value, e2, new LetrecStarContinuation(body, e2, index, k));
            }

            #endregion
        }

        private class LetrecStarPartialContinuation : IPartialContinuation
        {
            private IExpression body;
            private Environment env;
            private int index;
            private IPartialContinuation k;

            public LetrecStarPartialContinuation(IExpression body, Environment env, int index, IPartialContinuation k)
            {
                this.body = body;
                this.env = env;
                this.index = index;
                this.k = k;
            }

            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<LetrecStarPartialContinuation, LetrecStarContinuation>(this, delegate() { return new LetrecStarContinuation(body, env, index, k.Attach(theNewBase, a)); });
            }
        }

        [Serializable]
        private class LetrecStarContinuation : IContinuation
        {
            private IExpression body;
            private Environment env;
            private int index;
            private IContinuation k;

            public LetrecStarContinuation(IExpression body, Environment env, int index, IContinuation k)
            {
                this.body = body;
                this.env = env;
                this.index = index;
                this.k = k;
            }

            #region IContinuation Members

            public IRunnableStep Return(IGlobalState gs, object v)
            {
                env[index] = v;
                return new RunnableEval(body, env, k);
            }

            public IRunnableStep Throw(IGlobalState gs, object exc) { return new RunnableThrow(k, exc); }

            public IContinuation Parent { get { return k; } }
            public IProcedure EntryProc { get { return null; } }
            public IProcedure ExitProc { get { return null; } }

            public IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
            {
                return a.Assoc<LetrecStarContinuation, LetrecStarPartialContinuation>(this, delegate() { return new LetrecStarPartialContinuation(body, env, index, k.PartialCapture(baseMark, a)); });
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }

            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }

            #endregion
        }

        public IExpression Compile(EnvDesc ed)
        {
            if (letClause != null)
            {
                EnvDesc edInner;
                int[] mapping;
                ed.SubsetShadowExtend(GetRequirements(), new Symbol[] { letClause.Name }, out edInner, out mapping);
                return new LetrecStarExpr(mapping, letClause.Value.Compile(edInner), body.Compile(edInner));
            }
            else
            {
                return body.Compile(ed);
            }
        }

        #endregion
    }
    
    public class LetLoopSource : IExpressionSource
    {
        private Symbol loopName;
        private FList<LetClause> clauseList;
        private IExpressionSource body;

        public LetLoopSource(Symbol loopName, FList<LetClause> clauseList, IExpressionSource body)
        {
            this.loopName = loopName;
            this.clauseList = clauseList;
            this.body = body;
        }

        #region IExpressionSource Members

        private EnvSpec GetBodyRequirements()
        {
            EnvSpec vars = EnvSpec.FromArray(LetClause.GetSymbols(clauseList)) | loopName;
            return body.GetRequirements() - vars;
        }

        public EnvSpec GetRequirements()
        {
            EnvSpec reqs = FListUtils.ToEnumerable(clauseList).Select(l => l.Value.GetRequirements()).EnvSpecUnion();
            return reqs | GetBodyRequirements();
        }

        [Serializable]
        private class LetLoopProcedure : IProcedure
        {
            private int arity;
            private Environment env;
            private IExpression body;

            public LetLoopProcedure(int arity, Environment env, IExpression body)
            {
                this.arity = arity;
                this.env = env;
                this.body = body;
            }

            #region IProcedure Members

            public int Arity { get { return arity; } }

            public bool More { get { return false; } }

            public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
            {
                object[] args = LambdaSource.ParseArgList(arity, false, argList);
                Environment e2 = env.Extend(this, args);
                return new RunnableEval(body, e2, k);
            }

            #endregion
        }

        [Serializable]
        private class LetLoopExpr : IExpression
        {
            private FList<IExpression> clauseList;
            private int[] mapping;
            private int clauseCount;
            private IExpression loopBodyExpr;

            public LetLoopExpr(FList<IExpression> clauseList, int[] mapping, int clauseCount, IExpression loopBodyExpr)
            {
                this.clauseList = clauseList;
                this.mapping =  mapping;
                this.clauseCount = clauseCount;
                this.loopBodyExpr = loopBodyExpr;
            }

            #region IExpression Members

            public IRunnableStep Eval(IGlobalState gs, Environment env, IContinuation k)
            {
                Environment envCaptured = env.Extend(mapping, 0);
                IProcedure loopBody = new LetLoopProcedure(clauseCount, envCaptured, loopBodyExpr);

                if (clauseList == null)
                {
                    return new RunnableCall(loopBody, null, k);
                }
                else
                {
                    return new RunnableEval(clauseList.Head, env, new LetLoopContinuation(null, 0, clauseList.Tail, env, loopBody, k));
                }
            }

            #endregion
        }

        private class LetLoopPartialContinuation : IPartialContinuation
        {
            private FList<object> valuesSoFar;
            private int countSoFar;
            private FList<IExpression> clauseListTail;
            private Environment env;
            private IProcedure loopBody;
            private IPartialContinuation k;

            public LetLoopPartialContinuation
            (
                FList<object> valuesSoFar,
                int countSoFar,
                FList<IExpression> clauseListTail,
                Environment env,
                IProcedure loopBody,
                IPartialContinuation k
            )
            {
                this.valuesSoFar = valuesSoFar;
                this.countSoFar = countSoFar;
                this.clauseListTail = clauseListTail;
                this.env = env;
                this.loopBody = loopBody;
                this.k = k;
            }

            #region IPartialContinuation Members

            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<LetLoopPartialContinuation, LetLoopContinuation>(this, delegate() { return new LetLoopContinuation(valuesSoFar, countSoFar, clauseListTail, env, loopBody, k.Attach(theNewBase, a)); });
            }

            #endregion
        }

        [Serializable]
        private class LetLoopContinuation : IContinuation
        {
            private FList<object> valuesSoFar;
            private int countSoFar;
            private FList<IExpression> clauseListTail;
            private Environment env;
            private IProcedure loopBody;
            private IContinuation k;

            public LetLoopContinuation
            (
                FList<object> valuesSoFar,
                int countSoFar,
                FList<IExpression> clauseListTail,
                Environment env,
                IProcedure loopBody,
                IContinuation k
            )
            {
                this.valuesSoFar = valuesSoFar;
                this.countSoFar = countSoFar;
                this.clauseListTail = clauseListTail;
                this.env = env;
                this.loopBody = loopBody;
                this.k = k;
            }

            #region IContinuation Members

            public IRunnableStep Return(IGlobalState gs, object v)
            {
                FList<object> values2 = new FList<object>(v, valuesSoFar);
                int count2 = countSoFar + 1;
                if (clauseListTail == null)
                {
                    return new RunnableCall(loopBody, values2, k);
                }
                else
                {
                    return new RunnableEval(clauseListTail.Head, env, new LetLoopContinuation(values2, count2, clauseListTail.Tail, env, loopBody, k));
                }
            }

            public IRunnableStep Throw(IGlobalState gs, object exc) { return new RunnableThrow(k, exc); }

            public IContinuation Parent { get { return k; } }
            public IProcedure EntryProc { get { return null; } }
            public IProcedure ExitProc { get { return null; } }

            public IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
            {
                return a.Assoc<LetLoopContinuation, LetLoopPartialContinuation>(this, delegate() { return new LetLoopPartialContinuation(valuesSoFar, countSoFar, clauseListTail, env, loopBody, k.PartialCapture(baseMark, a)); });
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }

            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }

            #endregion
        }

        public IExpression Compile(EnvDesc ed)
        {
            EnvDesc edInner;
            int[] mapping;
            ed.SubsetShadowExtend(GetBodyRequirements(), loopName, LetClause.GetSymbols(clauseList), out edInner, out mapping);
            FList<IExpression> compiledExprs = FListUtils.ReverseMap
            (
                clauseList,
                delegate(LetClause lc)
                {
                    return lc.Value.Compile(ed);
                }
            );
            IExpression compiledBody = body.Compile(edInner);
            return new LetLoopExpr(compiledExprs, mapping, FListUtils.Count(clauseList), compiledBody);
        }

        #endregion
    }

    [Serializable]
    public class CatchSource : IExpressionSource
    {
        private IExpressionSource handler;
        private IExpressionSource body;

        public CatchSource(IExpressionSource handler, IExpressionSource body)
        {
            this.handler = handler;
            this.body = body;
        }

        #region IExpressionSource Members

        public EnvSpec GetRequirements()
        {
            return handler.GetRequirements() | body.GetRequirements();
        }

        [Serializable]
        private class CatchHandlerPartialContinuation : IPartialContinuation
        {
            private IExpression body;
            private Environment env;
            private IPartialContinuation k;

            public CatchHandlerPartialContinuation(IExpression body, Environment env, IPartialContinuation k)
            {
                this.body = body;
                this.env = env;
                this.k = k;
            }

            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<CatchHandlerPartialContinuation, CatchHandlerContinuation>(this, delegate() { return new CatchHandlerContinuation(body, env, k.Attach(theNewBase, a)); });
            }
        }

        [Serializable]
        private class CatchHandlerContinuation : IContinuation
        {
            private IExpression body;
            private Environment env;
            private IContinuation k;

            public CatchHandlerContinuation(IExpression body, Environment env, IContinuation k)
            {
                this.body = body;
                this.env = env;
                this.k = k;
            }

            public IRunnableStep Return(IGlobalState gs, object v)
            {
                if (!(v is IProcedure))
                {
                    return new RunnableThrow(k, new SchemeRuntimeException("Exception handler is not a procedure"));
                }

                IProcedure handler = (IProcedure)v;

                if (!ContinuationUtilities.AcceptsParameterCount(handler, 1))
                {
                    return new RunnableThrow(k, new SchemeRuntimeException("Exception handler has wrong arity"));
                }

                return new RunnableEval(body, env, new CatchBodyContinuation(k, handler));
            }

            public IRunnableStep Throw(IGlobalState gs, object exc) { return new RunnableThrow(k, exc); }

            public IContinuation Parent { get { return k; } }
            public IProcedure EntryProc { get { return null; } }
            public IProcedure ExitProc { get { return null; } }

            public IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
            {
                return a.Assoc<CatchHandlerContinuation, CatchHandlerPartialContinuation>(this, delegate() { return new CatchHandlerPartialContinuation(body, env, k.PartialCapture(baseMark, a)); });
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }

            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }
        }

        [Serializable]
        private class CatchBodyPartialContinuation : IPartialContinuation
        {
            private IPartialContinuation k;
            private IProcedure handler;

            public CatchBodyPartialContinuation(IPartialContinuation k, IProcedure handler)
            {
                this.k = k;
                this.handler = handler;
            }

            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<CatchBodyPartialContinuation, CatchBodyContinuation>(this, delegate() { return new CatchBodyContinuation(k.Attach(theNewBase, a), handler); });
            }
        }

        [Serializable]
        private class CatchBodyContinuation : IContinuation
        {
            private IContinuation k;
            private IProcedure handler;

            public CatchBodyContinuation(IContinuation k, IProcedure handler)
            {
                this.k = k;
                this.handler = handler;
            }

            public IRunnableStep Return(IGlobalState gs, object v)
            {
                return new RunnableReturn(k, v);
            }

            public IRunnableStep Throw(IGlobalState gs, object exc)
            {
                return new RunnableCall(handler, new FList<object>(exc), k);
            }

            public IContinuation Parent { get { return k; } }
            public IProcedure EntryProc { get { return null; } }
            public IProcedure ExitProc { get { return null; } }

            public IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
            {
                return a.Assoc<CatchBodyContinuation, CatchBodyPartialContinuation>(this, delegate() { return new CatchBodyPartialContinuation(k.PartialCapture(baseMark, a), handler); });
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }

            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }
        }

        [Serializable]
        private class CatchExpression : IExpression
        {
            private IExpression handler;
            private IExpression body;

            public CatchExpression(IExpression handler, IExpression body)
            {
                this.handler = handler;
                this.body = body;
            }

            public IRunnableStep Eval(IGlobalState gs, Environment env, IContinuation k)
            {
                return new RunnableEval(handler, env, new CatchHandlerContinuation(body, env, k));
            }
        }

        public IExpression Compile(EnvDesc ed)
        {
            IExpression cHandler = handler.Compile(ed);
            IExpression cBody = body.Compile(ed);
            return new CatchExpression(cHandler, cBody);
        }

        #endregion
    }

    [Serializable]
    public class DynamicWindSource : IExpressionSource
    {
        IExpressionSource entry;
        IExpressionSource body;
        IExpressionSource exit;

        public DynamicWindSource(IExpressionSource entry, IExpressionSource body, IExpressionSource exit)
        {
            this.entry = entry;
            this.body = body;
            this.exit = exit;
        }

        #region IExpressionSource Members

        public EnvSpec GetRequirements()
        {
            return entry.GetRequirements() | body.GetRequirements() | exit.GetRequirements();
        }

        [Serializable]
        private class DynamicWindPartialContinuation1 : IPartialContinuation
        {
            private IExpression exitLambda;
            private IExpression body;
            private Environment env;
            private IPartialContinuation k;

            public DynamicWindPartialContinuation1(IExpression exitLambda, IExpression body, Environment env, IPartialContinuation k)
            {
                this.exitLambda = exitLambda;
                this.body = body;
                this.env = env;
                this.k = k;
            }

            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<DynamicWindPartialContinuation1, DynamicWindContinuation1>(this, delegate() { return new DynamicWindContinuation1(exitLambda, body, env, k.Attach(theNewBase, a)); });
            }
        }

        [Serializable]
        private class DynamicWindContinuation1 : IContinuation
        {
            private IExpression exitLambda;
            private IExpression body;
            private Environment env;
            private IContinuation k;

            public DynamicWindContinuation1(IExpression exitLambda, IExpression body, Environment env, IContinuation k)
            {
                this.exitLambda = exitLambda;
                this.body = body;
                this.env = env;
                this.k = k;
            }

            #region IContinuation Members

            public IRunnableStep Return(IGlobalState gs, object v)
            {
                System.Diagnostics.Debug.Assert(v is IProcedure);

                return new RunnableEval(exitLambda, env, new DynamicWindContinuation2((IProcedure)v, body, env, k));
            }

            public IRunnableStep Throw(IGlobalState gs, object exc) { return new RunnableThrow(k, exc); }

            public IContinuation Parent { get { return k; } }
            public IProcedure EntryProc { get { return null; } }
            public IProcedure ExitProc { get { return null; } }

            public IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
            {
                return a.Assoc<DynamicWindContinuation1, DynamicWindPartialContinuation1>(this, delegate() { return new DynamicWindPartialContinuation1(exitLambda, body, env, k.PartialCapture(baseMark, a)); });
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }

            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }

            #endregion
        }

        [Serializable]
        private class DynamicWindPartialContinuation2 : IPartialContinuation
        {
            private IProcedure entryProc;
            private IExpression body;
            private Environment env;
            private IPartialContinuation k;

            public DynamicWindPartialContinuation2(IProcedure entryProc, IExpression body, Environment env, IPartialContinuation k)
            {
                this.entryProc = entryProc;
                this.body = body;
                this.env = env;
                this.k = k;
            }

            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<DynamicWindPartialContinuation2, DynamicWindContinuation2>(this, delegate() { return new DynamicWindContinuation2(entryProc, body, env, k.Attach(theNewBase, a)); });
            }
        }

        [Serializable]
        private class DynamicWindContinuation2 : IContinuation
        {
            private IProcedure entryProc;
            private IExpression body;
            private Environment env;
            private IContinuation k;

            public DynamicWindContinuation2(IProcedure entryProc, IExpression body, Environment env, IContinuation k)
            {
                this.entryProc = entryProc;
                this.body = body;
                this.env = env;
                this.k = k;
            }

            #region IContinuation Members

            public IRunnableStep Return(IGlobalState gs, object v)
            {
                System.Diagnostics.Debug.Assert(v is IProcedure);

                return new RunnableCall(entryProc, null, new DynamicWindContinuation3(entryProc, (IProcedure)v, body, env, k));
            }

            public IRunnableStep Throw(IGlobalState gs, object exc) { return new RunnableThrow(k, exc); }

            public IContinuation Parent { get { return k; } }
            public IProcedure EntryProc { get { return null; } }
            public IProcedure ExitProc { get { return null; } }

            public IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
            {
                return a.Assoc<DynamicWindContinuation2, DynamicWindPartialContinuation2>(this, delegate() { return new DynamicWindPartialContinuation2(entryProc, body, env, k.PartialCapture(baseMark, a)); });
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }

            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }

            #endregion
        }

        [Serializable]
        private class DynamicWindPartialContinuation3 : IPartialContinuation
        {
            private IProcedure entryProc;
            private IProcedure exitProc;
            private IExpression body;
            private Environment env;
            private IPartialContinuation k;

            public DynamicWindPartialContinuation3(IProcedure entryProc, IProcedure exitProc, IExpression body, Environment env, IPartialContinuation k)
            {
                this.entryProc = entryProc;
                this.exitProc = exitProc;
                this.body = body;
                this.env = env;
                this.k = k;
            }

            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<DynamicWindPartialContinuation3, DynamicWindContinuation3>(this, delegate() { return new DynamicWindContinuation3(entryProc, exitProc, body, env, k.Attach(theNewBase, a)); });
            }
        }

        [Serializable]
        private class DynamicWindContinuation3 : IContinuation
        {
            private IProcedure entryProc;
            private IProcedure exitProc;
            private IExpression body;
            private Environment env;
            private IContinuation k;

            public DynamicWindContinuation3(IProcedure entryProc, IProcedure exitProc, IExpression body, Environment env, IContinuation k)
            {
                this.entryProc = entryProc;
                this.exitProc = exitProc;
                this.body = body;
                this.env = env;
                this.k = k;
            }

            #region IContinuation Members

            public IRunnableStep Return(IGlobalState gs, object v)
            {
                return new RunnableEval(body, env, new DynamicWindContinuation4(entryProc, exitProc, k));
            }

            public IRunnableStep Throw(IGlobalState gs, object exc) { return new RunnableThrow(k, exc); }

            public IContinuation Parent { get { return k; } }
            public IProcedure EntryProc { get { return null; } }
            public IProcedure ExitProc { get { return null; } }

            public IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
            {
                return a.Assoc<DynamicWindContinuation3, DynamicWindPartialContinuation3>(this, delegate() { return new DynamicWindPartialContinuation3(entryProc, exitProc, body, env, k.PartialCapture(baseMark, a)); });
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }

            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }

            #endregion
        }

        [Serializable]
        private class DynamicWindPartialContinuation4 : IPartialContinuation
        {
            private IProcedure entryProc;
            private IProcedure exitProc;
            private IPartialContinuation k;

            public DynamicWindPartialContinuation4(IProcedure entryProc, IProcedure exitProc, IPartialContinuation k)
            {
                this.entryProc = entryProc;
                this.exitProc = exitProc;
                this.k = k;
            }

            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<DynamicWindPartialContinuation4, DynamicWindContinuation4>(this, delegate() { return new DynamicWindContinuation4(entryProc, exitProc, k.Attach(theNewBase, a)); });
            }
        }

        [Serializable]
        private class DynamicWindContinuation4 : IContinuation
        {
            private IProcedure entryProc;
            private IProcedure exitProc;
            private IContinuation k;

            public DynamicWindContinuation4(IProcedure entryProc, IProcedure exitProc, IContinuation k)
            {
                this.entryProc = entryProc;
                this.exitProc = exitProc;
                this.k = k;
            }

            #region IContinuation Members

            public IRunnableStep Return(IGlobalState gs, object v)
            {
                return new RunnableCall(exitProc, null, new DynamicWindContinuation5(v, false, k));
            }

            public IRunnableStep Throw(IGlobalState gs, object exc)
            {
                return new RunnableCall(exitProc, null, new DynamicWindContinuation5(exc, true, k));
            }

            public IContinuation Parent { get { return k; } }
            public IProcedure EntryProc { get { return entryProc; } }
            public IProcedure ExitProc { get { return exitProc; } }

            public IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
            {
                return a.Assoc<DynamicWindContinuation4, DynamicWindPartialContinuation4>(this, delegate() { return new DynamicWindPartialContinuation4(entryProc, exitProc, k.PartialCapture(baseMark, a)); });
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }

            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }

            #endregion
        }

        [Serializable]
        private class DynamicWindPartialContinuation5 : IPartialContinuation
        {
            private object retval;
            private bool thrown;
            private IPartialContinuation k;

            public DynamicWindPartialContinuation5(object retval, bool thrown, IPartialContinuation k)
            {
                this.retval = retval;
                this.thrown = thrown;
                this.k = k;
            }

            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<DynamicWindPartialContinuation5, DynamicWindContinuation5>(this, delegate() { return new DynamicWindContinuation5(retval, thrown, k.Attach(theNewBase, a)); });
            }
        }

        [Serializable]
        private class DynamicWindContinuation5 : IContinuation
        {
            private object retval;
            private bool thrown;
            private IContinuation k;

            public DynamicWindContinuation5(object retval, bool thrown, IContinuation k)
            {
                this.retval = retval;
                this.thrown = thrown;
                this.k = k;
            }

            #region IContinuation Members

            public IRunnableStep Return(IGlobalState gs, object v)
            {
                if (thrown) return new RunnableThrow(k, retval);
                else return new RunnableReturn(k, retval);
            }

            public IRunnableStep Throw(IGlobalState gs, object v)
            {
                // double fault: precedence goes to the newer exception
                return new RunnableThrow(k, v);
            }

            public IContinuation Parent { get { return k; } }
            public IProcedure EntryProc { get { return null; } }
            public IProcedure ExitProc { get { return null; } }

            public IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
            {
                return a.Assoc<DynamicWindContinuation5, DynamicWindPartialContinuation5>(this, delegate() { return new DynamicWindPartialContinuation5(retval, thrown, k.PartialCapture(baseMark, a)); });
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }

            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }

            #endregion
        }

        [Serializable]
        private class DynamicWindExpression : IExpression
        {
            private IExpression entryLambda;
            private IExpression body;
            private IExpression exitLambda;

            public DynamicWindExpression(IExpression entryLambda, IExpression body, IExpression exitLambda)
            {
                this.entryLambda = entryLambda;
                this.body = body;
                this.exitLambda = exitLambda;
            }

            #region IExpression Members

            public IRunnableStep Eval(IGlobalState gs, Environment env, IContinuation k)
            {
                return new RunnableEval(entryLambda, env, new DynamicWindContinuation1(exitLambda, body, env, k));
            }

            #endregion
        }

        public IExpression Compile(EnvDesc ed)
        {
            IExpressionSource entryLambda = new LambdaSource(SpecialValue.EMPTY_LIST, entry);
            IExpressionSource exitLambda = new LambdaSource(SpecialValue.EMPTY_LIST, exit);

            IExpression cEntryLambda = entryLambda.Compile(ed);
            IExpression cBody = body.Compile(ed);
            IExpression cExitLambda = exitLambda.Compile(ed);

            return new DynamicWindExpression(cEntryLambda, cBody, cExitLambda);
        }

        #endregion
    }

    [Serializable]
    public class MapSource : IExpressionSource
    {
        private FList<Tuple<object, IExpressionSource>> items;

        public MapSource(FList<Tuple<object, IExpressionSource>> items)
        {
            this.items = items;
        }

        #region IExpressionSource Members

        public EnvSpec GetRequirements()
        {
            return FListUtils.ToEnumerable(items).Select(x => x.Item2.GetRequirements()).EnvSpecUnion();
        }

        [Serializable]
        private class MapExpr : IExpression
        {
            private FList<Tuple<object, IExpression>> items;

            public MapExpr(FList<Tuple<object, IExpression>> items)
            {
                this.items = items;
            }

            #region IExpression Members

            public IRunnableStep Eval(IGlobalState gs, Environment env, IContinuation k)
            {
                if (items == null)
                {
                    return new RunnableReturn(k, new SchemeHashMap());
                }
                else
                {
                    return new RunnableEval(items.Head.Item2, env, new MapContinuation(items.Head.Item1, items.Tail, null, env, k));
                }
            }

            #endregion
        }

        [Serializable]
        private class MapPartialContinuation : IPartialContinuation
        {
            private object itemInProgress;
            private FList<Tuple<object, IExpression>> remainingItems;
            private FList<Tuple<object, object>> resultsSoFar;
            private Environment env;
            private IPartialContinuation k;

            public MapPartialContinuation(object itemInProgress, FList<Tuple<object, IExpression>> remainingItems, FList<Tuple<object, object>> resultsSoFar, Environment env, IPartialContinuation k)
            {
                this.itemInProgress = itemInProgress;
                this.remainingItems = remainingItems;
                this.resultsSoFar = resultsSoFar;
                this.env = env;
                this.k = k;
            }

            #region IPartialContinuation Members

            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<MapPartialContinuation, MapContinuation>(this, delegate() { return new MapContinuation(itemInProgress, remainingItems, resultsSoFar, env, k.Attach(theNewBase, a)); });
            }

            #endregion
        }

        [Serializable]
        private class MapContinuation : IContinuation
        {
            private object itemInProgress;
            private FList<Tuple<object, IExpression>> remainingItems;
            private FList<Tuple<object, object>> resultsSoFar;
            private Environment env;
            private IContinuation k;

            public MapContinuation(object itemInProgress, FList<Tuple<object, IExpression>> remainingItems, FList<Tuple<object, object>> resultsSoFar, Environment env, IContinuation k)
            {
                this.itemInProgress = itemInProgress;
                this.remainingItems = remainingItems;
                this.resultsSoFar = resultsSoFar;
                this.env = env;
                this.k = k;
            }

            #region IContinuation Members

            public IRunnableStep Return(IGlobalState gs, object v)
            {
                if (remainingItems != null)
                {
                    return new RunnableEval
                    (
                        remainingItems.Head.Item2,
                        env,
                        new MapContinuation
                        (
                            remainingItems.Head.Item1,
                            remainingItems.Tail,
                            new FList<Tuple<object, object>>(new Tuple<object, object>(itemInProgress, v), resultsSoFar),
                            env,
                            k
                        )
                    );
                }
                else
                {
                    FList<Tuple<object, object>> results = new FList<Tuple<object, object>>(new Tuple<object, object>(itemInProgress, v), resultsSoFar);
                    SchemeHashMap map = new SchemeHashMap();
                    foreach (Tuple<object, object> result in FListUtils.ToEnumerable(results))
                    {
                        map[result.Item1] = result.Item2;
                    }
                    return new RunnableReturn(k, map);
                }
            }

            public IRunnableStep Throw(IGlobalState gs, object exc)
            {
                return new RunnableThrow(k, exc);
            }

            public IContinuation Parent { get { return k; } }

            public IProcedure EntryProc { get { return null; } }

            public IProcedure ExitProc { get { return null; } }

            public IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
            {
                return a.Assoc<MapContinuation, MapPartialContinuation>(this, delegate() { return new MapPartialContinuation(itemInProgress, remainingItems, resultsSoFar, env, k.PartialCapture(baseMark, a)); });
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }

            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }

            #endregion
        }

        public IExpression Compile(EnvDesc ed)
        {
            return new MapExpr
            (
                FListUtils.Map
                (
                    items,
                    delegate(Tuple<object, IExpressionSource> e)
                    {
                        return new Tuple<object, IExpression>(e.Item1, e.Item2.Compile(ed));
                    }
                )
            );
        }

        #endregion
    }
}
