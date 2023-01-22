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
using System.Linq;

namespace ExprObjModel
{
    [Serializable]
    public class UsingSource : IExpressionSource
    {
        private FList<LetClause> clauseList;
        private IExpressionSource body;

        public UsingSource(FList<LetClause> clauseList, IExpressionSource body)
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
            EnvSpec reqs = FListUtils.ToEnumerable(clauseList).Select(l => l.Value.GetRequirements()).EnvSpecUnion();
            EnvSpec reqsBody = body.GetRequirements() - vars;
            return reqs | reqsBody;
        }

        [Serializable]
        private class UsingExpr : IExpression
        {
            private FList<IExpression> clauseList;
            private int[] captures;
            private IExpression body;

            public UsingExpr(FList<IExpression> clauseList, int[] captures, IExpression body)
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
                    IContinuation k2 = new UsingContinuation1
                    (
                        null, 0,
                        clauseList.Tail,
                        captured, body, env, k
                    );
                    return new RunnableEval(clauseList.Head, env, k2);
                }
            }
        }

        [Serializable]
        private class UsingPartialContinuation1 : IPartialContinuation
        {
            private FList<object> valuesSoFar;
            private int countSoFar;
            private FList<IExpression> clauseListTail;
            private Environment captured;
            private IExpression body;
            private Environment outerEnv;
            private IPartialContinuation k;

            public UsingPartialContinuation1
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
                return a.Assoc<UsingPartialContinuation1, UsingContinuation1>(this, delegate() { return new UsingContinuation1(valuesSoFar, countSoFar, clauseListTail, captured, body, outerEnv, k.Attach(theNewBase, a)); });
            }
        }

        [Serializable]
        private class UsingContinuation1 : IContinuation
        {
            private FList<object> valuesSoFar;
            private int countSoFar;
            private FList<IExpression> clauseListTail;
            private Environment captured;
            private IExpression body;
            private Environment outerEnv;
            private IContinuation k;

            public UsingContinuation1
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
                    return new RunnableEval(body, e2, new UsingContinuation2(values, k));
                }
                else
                {
                    IContinuation k2 = new UsingContinuation1
                    (
                        valuesSoFar2, countSoFar2,
                        clauseListTail.Tail,
                        captured, body, outerEnv, k
                    );
                    return new RunnableEval(clauseListTail.Head, outerEnv, k2);
                }
            }

            public IRunnableStep Throw(IGlobalState gs, object exc)
            {
                foreach (object obj in FListUtils.ToEnumerable(valuesSoFar))
                {
                    if (obj is DisposableID)
                    {
                        DisposableID d = (DisposableID)obj;
                        gs.DisposeByID(d);
                    }
                }
                return new RunnableThrow(k, exc);
            }

            public IContinuation Parent { get { return k; } }
            public IProcedure EntryProc { get { return null; } }
            public IProcedure ExitProc { get { return null; } }

            public IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
            {
                return a.Assoc<UsingContinuation1, UsingPartialContinuation1>(this, delegate() { return new UsingPartialContinuation1(valuesSoFar, countSoFar, clauseListTail, captured, body, outerEnv, k.PartialCapture(baseMark, a)); });
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }

            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }

            #endregion
        }

        [Serializable]
        private class UsingPartialContinuation2 : IPartialContinuation
        {
            private IPartialContinuation k;
            private object[] values;

            public UsingPartialContinuation2(object[] values, IPartialContinuation k)
            {
                this.values = values;
                this.k = k;
            }

            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<UsingPartialContinuation2, UsingContinuation2>(this, delegate() { return new UsingContinuation2(values, k.Attach(theNewBase, a)); });
            }
        }

        [Serializable]
        private class UsingContinuation2 : IContinuation
        {
            private object[] values;
            private IContinuation k;

            public UsingContinuation2(object[] values, IContinuation k)
            {
                this.values = values;
                this.k = k;
            }

            public IRunnableStep Return(IGlobalState gs, object v)
            {
                foreach (object obj in values)
                {
                    if (obj is DisposableID)
                    {
                        DisposableID d = (DisposableID)obj;
                        gs.DisposeByID(d);
                    }
                }
                return new RunnableReturn(k, v);
            }

            public IRunnableStep Throw(IGlobalState gs, object exc)
            {
                foreach (object obj in values)
                {
                    if (obj is DisposableID)
                    {
                        DisposableID d = (DisposableID)obj;
                        gs.DisposeByID(d);
                    }
                }
                return new RunnableThrow(k, exc);
            }

            public IContinuation Parent { get { return k; } }
            public IProcedure EntryProc { get { return null; } }
            public IProcedure ExitProc { get { return null; } }

            public IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
            {
                return a.Assoc<UsingContinuation2, UsingPartialContinuation2>(this, delegate() { return new UsingPartialContinuation2(values, k.PartialCapture(baseMark, a)); });
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }

            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }
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

            return new UsingExpr(compiledClauseList, captures, bodyC);
        }

        #endregion
    }

    [Serializable]
    public class UsingStarSource : IExpressionSource
    {
        private LetClause letClause;
        private IExpressionSource body;

        public UsingStarSource(FList<LetClause> clauseList, IExpressionSource body)
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
                this.body = new UsingStarSource(clauseList.Tail, body);
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
        private class UsingStarExpr : IExpression
        {
            private int[] mapping;
            private IExpression value;
            private IExpression body;

            public UsingStarExpr(int[] mapping, IExpression value, IExpression body)
            {
                this.mapping = mapping;
                this.value = value;
                this.body = body;
            }

            #region IExpression Members

            public IRunnableStep Eval(IGlobalState gs, Environment env, IContinuation k)
            {
                return new RunnableEval(value, env, new UsingStarContinuation1(mapping, body, env, k));
            }

            #endregion
        }

        [Serializable]
        private class UsingStarPartialContinuation1 : IPartialContinuation
        {
            private int[] mapping;
            private IExpression body;
            private Environment env;
            private IPartialContinuation k;

            public UsingStarPartialContinuation1(int[] mapping, IExpression body, Environment env, IPartialContinuation k)
            {
                this.mapping = mapping;
                this.body = body;
                this.env = env;
                this.k = k;
            }

            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<UsingStarPartialContinuation1, UsingStarContinuation1>(this, delegate() { return new UsingStarContinuation1(mapping, body, env, k.Attach(theNewBase, a)); });
            }
        }

        [Serializable]
        private class UsingStarContinuation1 : IContinuation
        {
            private int[] mapping;
            private IExpression body;
            private Environment env;
            private IContinuation k;

            public UsingStarContinuation1(int[] mapping, IExpression body, Environment env, IContinuation k)
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
                return new RunnableEval(body, e2, new UsingStarContinuation2(v, k));
            }

            public IRunnableStep Throw(IGlobalState gs, object exc) { return new RunnableThrow(k, exc); }

            public IContinuation Parent { get { return k; } }
            public IProcedure EntryProc { get { return null; } }
            public IProcedure ExitProc { get { return null; } }

            public IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
            {
                return a.Assoc<UsingStarContinuation1, UsingStarPartialContinuation1>(this, delegate() { return new UsingStarPartialContinuation1(mapping, body, env, k.PartialCapture(baseMark, a)); });
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }

            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }

            #endregion
        }

        [Serializable]
        private class UsingStarPartialContinuation2 : IPartialContinuation
        {
            private IPartialContinuation k;
            private object val;

            public UsingStarPartialContinuation2(object val, IPartialContinuation k)
            {
                this.val = val;
                this.k = k;
            }

            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<UsingStarPartialContinuation2, UsingStarContinuation2>(this, delegate() { return new UsingStarContinuation2(val, k.Attach(theNewBase, a)); });
            }
        }

        [Serializable]
        private class UsingStarContinuation2 : IContinuation
        {
            private object val;
            private IContinuation k;

            public UsingStarContinuation2(object val, IContinuation k)
            {
                this.val = val;
                this.k = k;
            }

            public IRunnableStep Return(IGlobalState gs, object v)
            {
                if (val is DisposableID)
                {
                    DisposableID d = (DisposableID)val;
                    gs.DisposeByID(d);
                }
                return new RunnableReturn(k, v);
            }

            public IRunnableStep Throw(IGlobalState gs, object exc)
            {
                if (val is DisposableID)
                {
                    DisposableID d = (DisposableID)val;
                    gs.DisposeByID(d);
                }
                return new RunnableThrow(k, exc);
            }

            public IContinuation Parent { get { return k; } }
            public IProcedure EntryProc { get { return null; } }
            public IProcedure ExitProc { get { return null; } }

            public IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
            {
                return a.Assoc<UsingStarContinuation2, UsingStarPartialContinuation2>(this, delegate() { return new UsingStarPartialContinuation2(val, k.PartialCapture(baseMark, a)); });
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }

            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }
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
                return new UsingStarExpr(mapping, compiledValue, compiledBody);
            }
            else
            {
                return body.Compile(ed);
            }
        }

        #endregion
    }

}