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

namespace ExprObjModel
{
    [Serializable]
    public class DynamicLetSource : IExpressionSource
    {
        private FList<LetClause> clauseList;
        private IExpressionSource body;

        public DynamicLetSource(FList<LetClause> clauseList, IExpressionSource body)
        {
            this.clauseList = clauseList;
            this.body = body;
        }

        #region IExpressionSource Members

        public EnvSpec GetRequirements()
        {
            EnvSpec reqs = FListUtils.ToEnumerable(clauseList).Select(l => l.Value.GetRequirements()).EnvSpecUnion();
            reqs |= body.GetRequirements();
            return reqs;
        }

        [Serializable]
        private class DynamicLetPartialContinuation : IPartialContinuation
        {
            private IPartialContinuation k;
            private FList<Symbol> symbolsToAssign;
            private FList<IExpression> exprsToAssign;
            private Symbol symbolInProgress;
            private Environment env;
            private IExpression body;
            private FList<Symbol> symbolsAssigned;
            private FList<object> values;

            public DynamicLetPartialContinuation
            (
                IPartialContinuation k,
                FList<Symbol> symbolsToAssign,
                FList<IExpression> exprsToAssign,
                Symbol symbolInProgress,
                Environment env,
                IExpression body,
                FList<Symbol> symbolsAssigned,
                FList<object> values
            )
            {
                this.k = k;
                this.symbolsToAssign = symbolsToAssign;
                this.exprsToAssign = exprsToAssign;
                this.symbolInProgress = symbolInProgress;
                this.env = env;
                this.body = body;
                this.symbolsAssigned = symbolsAssigned;
                this.values = values;
            }
        
            #region IPartialContinuation Members

            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
 	            return a.Assoc<DynamicLetPartialContinuation, DynamicLetContinuation>
                (
                    this,
                    delegate()
                    {
                        return new DynamicLetContinuation
                        (
                            k.Attach(theNewBase, a),
                            symbolsToAssign, exprsToAssign, symbolInProgress, env, body, symbolsAssigned, values
                        );
                    }
                );
            }

            #endregion
        }

        [Serializable]
        private class DynamicLetContinuation : IContinuation
        {
            private IContinuation k;
            private FList<Symbol> symbolsToAssign;
            private FList<IExpression> exprsToAssign;
            private Symbol symbolInProgress;
            private Environment env;
            private IExpression body;
            private FList<Symbol> symbolsAssigned;
            private FList<object> values;

            public DynamicLetContinuation
            (
                IContinuation k,
                FList<Symbol> symbolsToAssign,
                FList<IExpression> exprsToAssign,
                Symbol symbolInProgress,
                Environment env,
                IExpression body,
                FList<Symbol> symbolsAssigned,
                FList<object> values
            )
            {
                this.k = k;
                this.symbolsToAssign = symbolsToAssign;
                this.exprsToAssign = exprsToAssign;
                this.symbolInProgress = symbolInProgress;
                this.env = env;
                this.body = body;
                this.symbolsAssigned = symbolsAssigned;
                this.values = values;
            }

            #region IContinuation Members

            public IRunnableStep Return(IGlobalState gs, object v)
            {
                FList<Symbol> symbolsAssigned2 = new FList<Symbol>(symbolInProgress, symbolsAssigned);
                FList<object> values2 = new FList<object>(v, values);
                if (symbolsToAssign == null)
                {
                    Dictionary<Symbol, Box> dv = new Dictionary<Symbol,Box>();
                    while (symbolsAssigned2 != null)
                    {
                        if (!(dv.ContainsKey(symbolsAssigned2.Head)))
                        {
                            dv.Add(symbolsAssigned2.Head, new Box(values2.Head));
                        }
                        symbolsAssigned2 = symbolsAssigned2.Tail;
                        values2 = values2.Tail;
                    }
                    return new RunnableEval
                    (
                        body,
                        env,
                        new DynamicLetBodyContinuation
                        (
                            k,
                            dv
                        )
                    );
                }
                else
                {
                    return new RunnableEval
                    (
                        exprsToAssign.Head,
                        env,
                        new DynamicLetContinuation
                        (
                            k,
                            symbolsToAssign.Tail,
                            exprsToAssign.Tail,
                            symbolsToAssign.Head,
                            env,
                            body,
                            symbolsAssigned2,
                            values2
                        )
                    );
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
                return a.Assoc<DynamicLetContinuation, DynamicLetPartialContinuation>
                (
                    this,
                    delegate()
                    {
                        return new DynamicLetPartialContinuation
                        (
                            k.PartialCapture(baseMark, a),
                            symbolsToAssign, exprsToAssign, symbolInProgress, env, body, symbolsAssigned, values
                        );
                    }
                );
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }

            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }

            #endregion
        }

        [Serializable]
        private class DynamicLetBodyPartialContinuation : IPartialContinuation
        {
            private IPartialContinuation k;
            private Dictionary<Symbol, Box> dynamicVars;

            public DynamicLetBodyPartialContinuation(IPartialContinuation k, Dictionary<Symbol, Box> dynamicVars)
            {
                this.k = k;
                this.dynamicVars = dynamicVars;
            }

            #region IPartialContinuation Members

            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<DynamicLetBodyPartialContinuation, DynamicLetBodyContinuation>(this, delegate() { return new DynamicLetBodyContinuation(k.Attach(theNewBase, a), dynamicVars); });
            }

            #endregion
        }

        [Serializable]
        private class DynamicLetBodyContinuation : IContinuation
        {
            private IContinuation k;
            private Dictionary<Symbol, Box> dynamicVars;

            public DynamicLetBodyContinuation(IContinuation k, Dictionary<Symbol, Box> dynamicVars)
            {
                this.k = k;
                this.dynamicVars = dynamicVars;
            }

            #region IContinuation Members

            public IRunnableStep Return(IGlobalState gs, object v) { return new RunnableReturn(k, v); }

            public IRunnableStep Throw(IGlobalState gs, object exc) { return new RunnableThrow(k, exc); }

            public IContinuation Parent { get { return k; } }
            public IProcedure EntryProc { get { return null; } }
            public IProcedure ExitProc { get { return null; } }

            public IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
            {
                return a.Assoc<DynamicLetBodyContinuation, DynamicLetBodyPartialContinuation>(this, delegate() { return new DynamicLetBodyPartialContinuation(k.PartialCapture(baseMark, a), dynamicVars); });
            }

            public Box DynamicLookup(Symbol s)
            {
                if (dynamicVars.ContainsKey(s)) return dynamicVars[s];
                else return k.DynamicLookup(s);
            }

            public EnvSpec DynamicEnv
            {
                get
                {
                    return k.DynamicEnv | EnvSpec.FromEnumerable(dynamicVars.Keys);
                }
            }

            #endregion
        }

        [Serializable]
        private class DynamicLet : IExpression
        {
            private FList<Symbol> symbolsToAssign;
            private FList<IExpression> exprsToAssign;
            private IExpression body;

            public DynamicLet(FList<Symbol> symbolsToAssign, FList<IExpression> exprsToAssign, IExpression body)
            {
                this.symbolsToAssign = symbolsToAssign;
                this.exprsToAssign = exprsToAssign;
                this.body = body;
            }

            public IRunnableStep Eval(IGlobalState gs, Environment env, IContinuation k)
            {
                return new RunnableEval
                (
                    exprsToAssign.Head,
                    env,
                    new DynamicLetContinuation
                    (
                        k,
                        symbolsToAssign.Tail,
                        exprsToAssign.Tail,
                        symbolsToAssign.Head,
                        env,
                        body,
                        null,
                        null
                    )
                );
            }
        }

        public IExpression Compile(EnvDesc ed)
        {
            FList<Symbol> symbolsToAssign = null;
            FList<IExpression> exprsToAssign = null;

            foreach (LetClause l in FListUtils.ToEnumerable(clauseList))
            {
                symbolsToAssign = new FList<Symbol>(l.Name, symbolsToAssign);
                exprsToAssign = new FList<IExpression>(l.Value.Compile(ed), exprsToAssign);
            }

            symbolsToAssign = FListUtils.Reverse(symbolsToAssign);
            exprsToAssign = FListUtils.Reverse(exprsToAssign);
            IExpression compiledBody = body.Compile(ed);

            return new DynamicLet(symbolsToAssign, exprsToAssign, compiledBody);
        }

        #endregion
    }

    [Serializable]
    class DynamicVarRefSource : IExpressionSource
    {
        private Symbol var;

        public DynamicVarRefSource(Symbol var)
        {
            this.var = var;
        }

        #region IExpressionSource Members

        public EnvSpec GetRequirements()
        {
            return EnvSpec.EmptySet;
        }

        [Serializable]
        private class DynamicVarRef : IExpression
        {
            private Symbol var;

            public DynamicVarRef(Symbol var)
            {
                this.var = var;
            }

            #region IExpression Members

            public IRunnableStep Eval(IGlobalState gs, Environment env, IContinuation k)
            {
                Box b = k.DynamicLookup(var);
                if (b == null) return new RunnableThrow(k, new SchemeRuntimeException("Undefined dynamic variable " + var));
                else return new RunnableReturn(k, b.Contents);
            }

            #endregion
        }

        public IExpression Compile(EnvDesc ed)
        {
            return new DynamicVarRef(var);
        }

        #endregion
    }

    [Serializable]
    class DynamicVarSetSource : IExpressionSource
    {
        private Symbol var;
        private IExpressionSource expr;

        public DynamicVarSetSource(Symbol var, IExpressionSource expr)
        {
            this.var = var;
            this.expr = expr;
        }

        [Serializable]
        private class DynamicVarSetPartialContinuation : IPartialContinuation
        {
            private Symbol var;
            private IPartialContinuation k;

            public DynamicVarSetPartialContinuation(Symbol var, IPartialContinuation k)
            {
                this.var = var;
                this.k = k;
            }

            #region IPartialContinuation Members

            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<DynamicVarSetPartialContinuation, DynamicVarSetContinuation>
                (
                    this,
                    delegate()
                    {
                        return new DynamicVarSetContinuation(var, k.Attach(theNewBase, a));
                    }
                );
            }

            #endregion
        }

        [Serializable]
        private class DynamicVarSetContinuation : IContinuation
        {
            private Symbol var;
            private IContinuation k;

            public DynamicVarSetContinuation(Symbol var, IContinuation k)
            {
                this.var = var;
                this.k = k;
            }

            #region IContinuation Members

            public IRunnableStep Return(IGlobalState gs, object v)
            {
                Box b = k.DynamicLookup(var);
                if (b == null) return new RunnableThrow(k, new SchemeRuntimeException("Undefined dynamic variable " + var));
                b.Contents = v;
                return new RunnableReturn(k, SpecialValue.UNSPECIFIED);
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
                return a.Assoc<DynamicVarSetContinuation, DynamicVarSetPartialContinuation>
                (
                    this,
                    delegate()
                    {
                        return new DynamicVarSetPartialContinuation(var, k.PartialCapture(baseMark, a));
                    }
                );
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }

            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }

            #endregion
        }

        [Serializable]
        private class DynamicVarSet : IExpression
        {
            private Symbol var;
            private IExpression expr;

            public DynamicVarSet(Symbol var, IExpression expr)
            {
                this.var = var;
                this.expr = expr;
            }

            #region IExpression Members

            public IRunnableStep Eval(IGlobalState gs, Environment env, IContinuation k)
            {
                return new RunnableEval(expr, env, new DynamicVarSetContinuation(var, k));
            }

            #endregion
        }

        #region IExpressionSource Members

        public EnvSpec GetRequirements()
        {
            return expr.GetRequirements();
        }
        
        public IExpression Compile(EnvDesc ed)
        {
            return new DynamicVarSet(var, expr.Compile(ed));
        }

        #endregion
    }

    [SchemeSingleton("is-defined-dynamic?")]
    public class IsDefinedDynamic : IProcedure
    {
        public IsDefinedDynamic()
        {
        }

        #region IProcedure Members

        public int Arity
        {
            get { return 1; }
        }

        public bool More
        {
            get { return false; }
        }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            if (argList == null)
            {
                return new RunnableThrow(k, new SchemeRuntimeException("is-dynamic-defined? : too few arguments (expected 1)"));
            }
            else if (argList.Tail == null)
            {
                object oArg = argList.Head;
                if (oArg is Symbol)
                {
                    Symbol arg = (Symbol)oArg;
                    return new RunnableReturn(k, k.DynamicEnv.Contains(arg));
                }
                else
                {
                    return new RunnableThrow(k, new SchemeRuntimeException("is-dynamic-defined? : type mismatch (expected a symbol)"));
                }
            }
            else
            {
                return new RunnableThrow(k, new SchemeRuntimeException("is-dynamic-defined? : too many arguments (expected 1)"));
            }
        }

        #endregion
    }
}