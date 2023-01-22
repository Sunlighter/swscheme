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

namespace ExprObjModel.ObjectSystem
{
    [SchemeIsAFunction("mprocedure?")]
    public interface IMsgProcedure
    {
        Signature Signature { [SchemeFunction("msignature")] get; }
        IRunnableStep MsgCall(IGlobalState gs, Message<object> message, IContinuation k);
    }

    [Serializable]
    public class RunnableMsgCall : IRunnableStep
    {
        private IMsgProcedure h;
        private Message<object> message;
        private IContinuation k;

        public RunnableMsgCall(IMsgProcedure h, Message<object> message, IContinuation k)
        {
            this.h = h;
            this.message = message;
            this.k = k;
        }

        public IRunnableStep Run(IGlobalState gs)
        {
            return h.MsgCall(gs, message, k);
        }
    }

    [Serializable]
    public class MessageSource : IExpressionSource
    {
        private Symbol type;
        private FList<Tuple<Symbol, IExpressionSource>> args;

        public MessageSource(Symbol type, FList<Tuple<Symbol, IExpressionSource>> args)
        {
            this.type = type;
            this.args = args;
        }

        #region IExpressionSource Members

        public EnvSpec GetRequirements()
        {
            return FListUtils.ToEnumerable(args).Select(x => x.Item2.GetRequirements()).EnvSpecUnion();
        }

        private class MessageExpr : IExpression
        {
            private Symbol type;
            private FList<Tuple<Symbol, IExpression>> args;

            public MessageExpr(Symbol type, FList<Tuple<Symbol, IExpression>> args)
            {
                this.type = type;
                this.args = args;
            }

            #region IExpression Members

            public IRunnableStep Eval(IGlobalState gs, Environment env, IContinuation k)
            {
                if (args == null)
                {
                    return new RunnableReturn(k, new ExprObjModel.ObjectSystem.Message<object>(type, Enumerable.Empty<Tuple<Symbol, object>>()));
                }
                else
                {
                    return new RunnableEval(args.Head.Item2, env, new MessageContinuation(type, args.Head.Item1, args.Tail, null, env, k));
                }
            }

            #endregion
        }

        [Serializable]
        private class MessagePartialContinuation : IPartialContinuation
        {
            private Symbol type;
            private Symbol argInProgress;
            private FList<Tuple<Symbol, IExpression>> remainingArgs;
            private FList<Tuple<Symbol, object>> resultsSoFar;
            private Environment env;
            private IPartialContinuation k;

            public MessagePartialContinuation(Symbol type, Symbol argInProgress, FList<Tuple<Symbol, IExpression>> remainingArgs, FList<Tuple<Symbol, object>> resultsSoFar, Environment env, IPartialContinuation k)
            {
                this.type = type;
                this.argInProgress = argInProgress;
                this.remainingArgs = remainingArgs;
                this.resultsSoFar = resultsSoFar;
                this.env = env;
                this.k = k;
            }

            #region IPartialContinuation Members

            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<MessagePartialContinuation, MessageContinuation>(this, delegate() { return new MessageContinuation(type, argInProgress, remainingArgs, resultsSoFar, env, k.Attach(theNewBase, a)); });
            }

            #endregion
        }

        [Serializable]
        private class MessageContinuation : IContinuation
        {
            private Symbol type;
            private Symbol argInProgress;
            private FList<Tuple<Symbol, IExpression>> remainingArgs;
            private FList<Tuple<Symbol, object>> resultsSoFar;
            private Environment env;
            private IContinuation k;

            public MessageContinuation(Symbol type, Symbol argInProgress, FList<Tuple<Symbol, IExpression>> remainingArgs, FList<Tuple<Symbol, object>> resultsSoFar, Environment env, IContinuation k)
            {
                this.type = type;
                this.argInProgress = argInProgress;
                this.remainingArgs = remainingArgs;
                this.resultsSoFar = resultsSoFar;
                this.env = env;
                this.k = k;
            }

            #region IContinuation Members

            public IRunnableStep Return(IGlobalState gs, object v)
            {
                if (remainingArgs != null)
                {
                    return new RunnableEval
                    (
                        remainingArgs.Head.Item2,
                        env,
                        new MessageContinuation
                        (
                            type,
                            remainingArgs.Head.Item1,
                            remainingArgs.Tail,
                            new FList<Tuple<Symbol, object>>(new Tuple<Symbol, object>(argInProgress, v), resultsSoFar),
                            env,
                            k
                        )
                    );
                }
                else
                {
                    FList<Tuple<Symbol, object>> results = new FList<Tuple<Symbol, object>>(new Tuple<Symbol, object>(argInProgress, v), resultsSoFar);
                    return new RunnableReturn(k, new ExprObjModel.ObjectSystem.Message<object>(type, FListUtils.ToEnumerable(results)));
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
                return a.Assoc<MessageContinuation, MessagePartialContinuation>(this, delegate() { return new MessagePartialContinuation(type, argInProgress, remainingArgs, resultsSoFar, env, k.PartialCapture(baseMark, a)); });
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }

            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }

            #endregion
        }

        public IExpression Compile(EnvDesc ed)
        {
            return new MessageExpr
            (
                type,
                FListUtils.Map
                (
                    args,
                    delegate(Tuple<Symbol, IExpressionSource> e)
                    {
                        return new Tuple<Symbol, IExpression>(e.Item1, e.Item2.Compile(ed));
                    }
                )
            );
        }

        #endregion
    }

    [Serializable]
    public class MsgLambdaSource : IExpressionSource
    {
        private Message<Symbol> parameters;
        private IExpressionSource body;

        public MsgLambdaSource(Message<Symbol> parameters, IExpressionSource body)
        {
            this.parameters = parameters;
            this.body = body;
        }

        private EnvSpec GetParameters()
        {
            return EnvSpec.FromEnumerable(parameters.Arguments.Select(x => x.Item2));
        }

        public EnvSpec GetRequirements()
        {
            return body.GetRequirements() - GetParameters();
        }

        [Serializable]
        private class MsgLambdaExpr : IExpression
        {
            Signature parameters;
            private IExpression body;
            private int[] mapping;

            public MsgLambdaExpr(Signature parameters, int[] mapping, IExpression body)
            {
                this.parameters = parameters;
                this.mapping = mapping;
                this.body = body;
            }

            public IRunnableStep Eval(IGlobalState gs, Environment env, IContinuation k)
            {
                Environment captured = env.Extend(mapping, 0);
                IMsgProcedure proc = new MsgLambdaProcedure(parameters, body, captured);
                return new RunnableReturn(k, proc);
            }
        }

        [Serializable]
        public class MsgLambdaProcedure : IMsgProcedure
        {
            private Signature signature;
            private IExpression body;
            private Environment captured;

            public MsgLambdaProcedure(Signature signature, IExpression body, Environment captured)
            {
                this.signature = signature;
                this.body = body;
                this.captured = captured;
            }

            #region ISchemeMessageHandler Members

            public Signature Signature
            {
                get { return signature; }
            }

            public IRunnableStep MsgCall(IGlobalState gs, Message<object> message, IContinuation k)
            {
                if (message.Matches(signature))
                {
                    Environment extended = captured.Extend(message.Values.ToArray());
                    return new RunnableEval(body, extended, new MsgLambdaContinuation(k));
                }
                else
                {
                    return new RunnableReturn(k, new ConsCell(false, false));
                }
            }

            #endregion
        }

        [Serializable]
        private class MsgLambdaPartialContinuation : IPartialContinuation
        {
            private IPartialContinuation k;

            public MsgLambdaPartialContinuation(IPartialContinuation k)
            {
                this.k = k;
            }

            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<MsgLambdaPartialContinuation, MsgLambdaContinuation>(this, delegate() { return new MsgLambdaContinuation(k.Attach(theNewBase, a)); });
            }
        }

        private class MsgLambdaContinuation : IContinuation
        {
            private IContinuation k;

            public MsgLambdaContinuation(IContinuation k)
            {
                this.k = k;
            }
            #region IContinuation Members

            public IRunnableStep Return(IGlobalState gs, object v)
            {
                return new RunnableReturn(k, new ConsCell(true, v));
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
                return a.Assoc<MsgLambdaContinuation, MsgLambdaPartialContinuation>(this, delegate() { return new MsgLambdaPartialContinuation(k.PartialCapture(baseMark, a)); });
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }
            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }

            #endregion
        }

        public IExpression Compile(EnvDesc ed)
        {
            Symbol[] lambdaParams = parameters.Values.ToArray();

            EnvDesc edInner;
            int[] captures;
            ed.SubsetShadowExtend(GetRequirements(), lambdaParams, out edInner, out captures);

            // compile the body with the new EnvDesc

            IExpression bodyC = body.Compile(edInner);
            return new MsgLambdaExpr(parameters.Signature, captures, bodyC);
        }
    }

    public interface IMsgCaseClause
    {
        IRunnableStep MsgEval(IGlobalState gs, Message<object> message, Environment env, IContinuation k);
    }

    [Serializable]
    public class RunnableMsgCaseClause : IRunnableStep
    {
        private IMsgCaseClause clause;
        private Message<object> message;
        private Environment env;
        private IContinuation k;

        public RunnableMsgCaseClause(IMsgCaseClause clause, Message<object> message, Environment env, IContinuation k)
        {
            this.clause = clause;
            this.message = message;
            this.env = env;
            this.k = k;
        }
        public IRunnableStep Run(IGlobalState gs)
        {
            return clause.MsgEval(gs, message, env, k);
        }
    }

    [Serializable]
    public class MsgCaseClauseSource
    {
        private Message<Symbol> selector;
        private IExpressionSource body;

        public MsgCaseClauseSource(Message<Symbol> selector, IExpressionSource body)
        {
            this.selector = selector;
            this.body = body;
        }

        public Message<Symbol> Selector { get { return selector; } }
        public IExpressionSource Body { get { return body; } }

        public EnvSpec GetRequirements()
        {
            return body.GetRequirements() - EnvSpec.FromEnumerable(selector.Arguments.Select(x => x.Item2));
        }

        private class MsgCaseClause : IMsgCaseClause
        {
            private Signature signature;
            private int[] mapping;
            private IExpression body;

            public MsgCaseClause(Signature signature, int[] mapping, IExpression body)
            {
                this.signature = signature;
                this.mapping = mapping;
                this.body = body;
            }

            public IRunnableStep MsgEval(IGlobalState gs, Message<object> message, Environment env, IContinuation k)
            {
                Environment e2 = env.Extend(mapping, message.Values.ToArray());
                return new RunnableEval(body, e2, k);
            }
        }

        public IMsgCaseClause Compile(EnvDesc ed)
        {
            EnvDesc ed2;
            int[] mapping;
            ed.SubsetShadowExtend(this.GetRequirements(), selector.Values.ToArray(), out ed2, out mapping);
            return new MsgCaseClause(selector.Signature, mapping, body.Compile(ed2));
        }
    }

    [Serializable]
    public class MsgCaseSource : IExpressionSource
    {
        public IExpressionSource expr;
        public FList<MsgCaseClauseSource> clauses;
        public IExpressionSource elseClause;

        public MsgCaseSource(IExpressionSource expr, FList<MsgCaseClauseSource> clauses, IExpressionSource elseClause)
        {
            this.expr = expr;
            this.clauses = clauses;
            this.elseClause = elseClause;
        }

        public EnvSpec GetRequirements()
        {
            EnvSpec e = expr.GetRequirements();
            e |= FListUtils.ToEnumerable(clauses).Select(x => x.GetRequirements()).EnvSpecUnion();
            e |= elseClause.GetRequirements();
            return e;
        }

        private class MsgCaseExpr : IExpression
        {
            private IExpression expr;
            private Dictionary<Signature, IMsgCaseClause> map;
            private IExpression elseClause;

            public MsgCaseExpr(IExpression expr, Dictionary<Signature, IMsgCaseClause> map, IExpression elseClause)
            {
                this.expr = expr;
                this.map = map;
                this.elseClause = elseClause;
            }

            public IRunnableStep Eval(IGlobalState gs, Environment env, IContinuation k)
            {
                return new RunnableEval(expr, env, new MsgCaseContinuation(map, elseClause, env, k));
            }
        }

        [Serializable]
        private class MsgCasePartialContinuation : IPartialContinuation
        {
            private Dictionary<Signature, IMsgCaseClause> map;
            private IExpression elseClause;
            private Environment env;
            private IPartialContinuation k;

            public MsgCasePartialContinuation(Dictionary<Signature, IMsgCaseClause> map, IExpression elseClause, Environment env, IPartialContinuation k)
            {
                this.map = map;
                this.elseClause = elseClause;
                this.env = env;
                this.k = k;
            }

            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<MsgCasePartialContinuation, MsgCaseContinuation>(this, delegate() { return new MsgCaseContinuation(map, elseClause, env, k.Attach(theNewBase, a)); });
            }
        }

        private class MsgCaseContinuation : IContinuation
        {
            private Dictionary<Signature, IMsgCaseClause> map;
            private IExpression elseClause;
            private Environment env;
            private IContinuation k;

            public MsgCaseContinuation(Dictionary<Signature, IMsgCaseClause> map, IExpression elseClause, Environment env, IContinuation k)
            {
                this.map = map;
                this.elseClause = elseClause;
                this.env = env;
                this.k = k;
            }

            public IRunnableStep Return(IGlobalState gs, object v)
            {
                if (v is Message<object>)
                {
                    Message<object> vMsg = (Message<object>)v;
                    if (map.ContainsKey(vMsg.Signature))
                    {
                        return new RunnableMsgCaseClause(map[vMsg.Signature], vMsg, env, k);
                    }
                }
                return new RunnableEval(elseClause, env, k);
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
                return a.Assoc<MsgCaseContinuation, MsgCasePartialContinuation>(this, delegate() { return new MsgCasePartialContinuation(map, elseClause, env, k.PartialCapture(baseMark, a)); });
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }
            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }
        }

        public IExpression Compile(EnvDesc ed)
        {
            IExpression expr1 = expr.Compile(ed);
            IExpression elseClause1 = elseClause.Compile(ed);
            Dictionary<Signature, IMsgCaseClause> map1 = FListUtils.ToEnumerable(clauses).Select
            (
                x => new Tuple<Signature, IMsgCaseClause>(x.Selector.Signature, x.Compile(ed))
            ).ToDictionary(x => x.Item1, x => x.Item2);

            return new MsgCaseExpr(expr1, map1, elseClause1);
        }
    }
}
