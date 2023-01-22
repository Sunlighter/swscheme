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

namespace ExprObjModel.ObjectSystem
{
    public class LocalRefSource : IExpressionSource
    {
        private Symbol varname;

        public LocalRefSource(Symbol varname)
        {
            this.varname = varname;
        }

        public EnvSpec GetRequirements()
        {
            return EnvSpec.EmptySet;
        }

        [Serializable]
        private class LocalRefExpr : IExpression
        {
            private Symbol varname;

            public LocalRefExpr(Symbol varname)
            {
                this.varname = varname;
            }

            public IRunnableStep Eval(IGlobalState gs, Environment env, IContinuation k)
            {
                if (gs.CurrentObject == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException("No current object"));
                }
                else
                {
                    try
                    {
                        Box b = gs.CurrentObject.GetLocal(varname);
                        if (b.HasContents)
                        {
                            return new RunnableReturn(k, b.Contents);
                        }
                        else
                        {
                            return new RunnableThrow(k, new SchemeRuntimeException("Attempt to read from uninitialized variable!"));
                        }
                    }
                    catch (Exception exc)
                    {
                        return new RunnableThrow(k, exc);
                    }
                }
            }
        }

        public IExpression Compile(EnvDesc ed)
        {
            return new LocalRefExpr(varname);
        }
    }

    public class LocalSetSource : IExpressionSource
    {
        private Symbol varname;
        private IExpressionSource valExpr;

        public LocalSetSource(Symbol varname, IExpressionSource valExpr)
        {
            this.varname = varname;
            this.valExpr = valExpr;
        }

        public EnvSpec GetRequirements()
        {
            return valExpr.GetRequirements();
        }

        [Serializable]
        private class LocalSetExpr : IExpression
        {
            private Symbol var;
            private IExpression valExpr;

            public LocalSetExpr(Symbol var, IExpression valExpr)
            {
                this.var = var;
                this.valExpr = valExpr;
            }

            public IRunnableStep Eval(IGlobalState gs, Environment env, IContinuation k)
            {
                if (gs.CurrentObject == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException("No current object"));
                }
                else
                {
                    IContinuation k2 = new LocalSetContinuation(var, env, k);
                    return new RunnableEval(valExpr, env, k2);
                }
            }
        }

        private class LocalSetPartialContinuation : IPartialContinuation
        {
            private Symbol var;
            private Environment env;
            private IPartialContinuation k;

            public LocalSetPartialContinuation(Symbol var, Environment env, IPartialContinuation k)
            {
                this.var = var;
                this.env = env;
                this.k = k;
            }

            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<LocalSetPartialContinuation, LocalSetContinuation>(this, delegate() { return new LocalSetContinuation(var, env, k.Attach(theNewBase, a)); });
            }
        }

        [Serializable]
        private class LocalSetContinuation : IContinuation
        {
            private Symbol var;
            private Environment env;
            private IContinuation k;

            public LocalSetContinuation(Symbol var, Environment env, IContinuation k)
            {
                this.var = var;
                this.env = env;
                this.k = k;
            }

            public IRunnableStep Return(IGlobalState gs, object v)
            {
                if (gs.CurrentObject == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException("No current object"));
                }
                else
                {
                    try
                    {
                        gs.CurrentObject.GetLocal(var).Contents = v;
                        return new RunnableReturn(k, SpecialValue.UNSPECIFIED);
                    }
                    catch (Exception exc)
                    {
                        return new RunnableThrow(k, exc);
                    }
                }
            }

            public IRunnableStep Throw(IGlobalState gs, object exc) { return new RunnableThrow(k, exc); }

            public IContinuation Parent { get { return k; } }
            public IProcedure EntryProc { get { return null; } }
            public IProcedure ExitProc { get { return null; } }

            public IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
            {
                return a.Assoc<LocalSetContinuation, LocalSetPartialContinuation>(this, delegate() { return new LocalSetPartialContinuation(var, env, k.PartialCapture(baseMark, a)); });
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }

            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }
        }

        public IExpression Compile(EnvDesc ed)
        {
            return new LocalSetExpr(varname, valExpr.Compile(ed));
        }
    }

}