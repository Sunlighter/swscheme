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
using System.Linq;
using System.Runtime.Serialization;

namespace ExprObjModel.MatchSyntax
{
    public interface IMatchSuccessContinuation
    {
        IRunnableStep Succeed(object[] matches);
        IMatchSuccessPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a);
    }

    public interface IMatchSuccessPartialContinuation
    {
        IMatchSuccessContinuation Attach(IContinuation theNewBase, ItemAssociation a);
    }

    public interface IMatchFailureContinuation
    {
        IRunnableStep Fail();
        IMatchFailurePartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a);
    }

    public interface IMatchFailurePartialContinuation
    {
        IMatchFailureContinuation Attach(IContinuation theNewBase, ItemAssociation a);
    }

    public interface IPatternMatchExpression
    {
        IRunnableStep Match(object obj, object[] matches, IMatchSuccessContinuation ks, IMatchFailureContinuation kf);
    }

    public class RunnableSuccess : IRunnableStep
    {
        private IMatchSuccessContinuation ks;
        private object[] matches;

        public RunnableSuccess(IMatchSuccessContinuation ks, object[] matches)
        {
            this.ks = ks; this.matches = matches;
        }

        public IRunnableStep Run(IGlobalState gs)
        {
            return ks.Succeed(matches);
        }
    }

    public class RunnableFailure : IRunnableStep
    {
        private IMatchFailureContinuation kf;

        public RunnableFailure(IMatchFailureContinuation kf)
        {
            this.kf = kf;
        }

        public IRunnableStep Run(IGlobalState gs)
        {
            return kf.Fail();
        }
    }

    public class RunnableMatch : IRunnableStep
    {
        private IPatternMatchExpression p;
        private object obj;
        private object[] matches;
        private IMatchSuccessContinuation ks;
        private IMatchFailureContinuation kf;

        public RunnableMatch(IPatternMatchExpression p, object obj, object[] matches, IMatchSuccessContinuation ks, IMatchFailureContinuation kf)
        {
            this.p = p;
            this.obj = obj;
            this.matches = matches;
            this.ks = ks;
            this.kf = kf;
        }

        public IRunnableStep Run(IGlobalState gs)
        {
            return p.Match(obj, matches, ks, kf);
        }
    }

    public interface IPatternMatchExpressionSource
    {
        EnvSpec GetParameters();
        IPatternMatchExpression Compile(Symbol[] placements);
    }

    public class MatchAtomicSource : IPatternMatchExpressionSource
    {
        private object target;

        public MatchAtomicSource(object target)
        {
            this.target = target;
        }

        public EnvSpec GetParameters() { return EnvSpec.EmptySet; }

        private class MatchAtomic : IPatternMatchExpression
        {
            private object target;

            public MatchAtomic(object target)
            {
                this.target = target;
            }

            public IRunnableStep Match(object obj, object[] matches, IMatchSuccessContinuation ks, IMatchFailureContinuation kf)
            {
                if (ExprObjModel.Procedures.ProxyDiscovery.FastEqual(obj, target))
                {
                    return new RunnableSuccess(ks, matches);
                }
                else
                {
                    return new RunnableFailure(kf);
                }
            }
        }

        public IPatternMatchExpression Compile(Symbol[] placements) { return new MatchAtomic(target); }
    }

    public class MatchBindSource : IPatternMatchExpressionSource
    {
        private Symbol var;

        public MatchBindSource(Symbol var)
        {
            this.var = var;
        }

        public EnvSpec GetParameters() { return EnvSpec.Only(var); }

        private class MatchBind : IPatternMatchExpression
        {
            private int pos;

            public MatchBind(int pos)
            {
                this.pos = pos;
            }

            public IRunnableStep Match(object obj, object[] matches, IMatchSuccessContinuation ks, IMatchFailureContinuation kf)
            {
                matches[pos] = obj;
                return new RunnableSuccess(ks, matches);
            }
        }

        public IPatternMatchExpression Compile(Symbol[] placements)
        {
            return new MatchBind(placements.Find(var));
        }
    }

    public class MatchConsSource : IPatternMatchExpressionSource
    {
        private IPatternMatchExpressionSource carPattern;
        private IPatternMatchExpressionSource cdrPattern;

        public MatchConsSource(IPatternMatchExpressionSource carPattern, IPatternMatchExpressionSource cdrPattern)
        {
            this.carPattern = carPattern;
            this.cdrPattern = cdrPattern;
        }

        public EnvSpec GetParameters() { return carPattern.GetParameters() | cdrPattern.GetParameters(); }

        private class MatchCons : IPatternMatchExpression
        {
            private IPatternMatchExpression carPattern;
            private IPatternMatchExpression cdrPattern;

            public MatchCons(IPatternMatchExpression carPattern, IPatternMatchExpression cdrPattern)
            {
                this.carPattern = carPattern;
                this.cdrPattern = cdrPattern;
            }

            private class FirstHalfSuccessPartial : IMatchSuccessPartialContinuation
            {
                private IPatternMatchExpression cdrPattern;
                private IMatchSuccessPartialContinuation ks;
                private IMatchFailurePartialContinuation kf;
                private object cdr;

                public FirstHalfSuccessPartial(IPatternMatchExpression cdrPattern, IMatchSuccessPartialContinuation ks, IMatchFailurePartialContinuation kf, object cdr)
                {
                    this.cdrPattern = cdrPattern;
                    this.ks = ks;
                    this.kf = kf;
                    this.cdr = cdr;
                }

                public IMatchSuccessContinuation Attach(IContinuation theNewBase, ItemAssociation a)
                {
                    return a.Assoc<FirstHalfSuccessPartial, FirstHalfSuccess>(this, delegate() { return new FirstHalfSuccess(cdrPattern, ks.Attach(theNewBase, a), kf.Attach(theNewBase, a), cdr); });
                }
            }

            private class FirstHalfSuccess : IMatchSuccessContinuation
            {
                private IPatternMatchExpression cdrPattern;
                private IMatchSuccessContinuation ks;
                private IMatchFailureContinuation kf;
                private object cdr;

                public FirstHalfSuccess(IPatternMatchExpression cdrPattern, IMatchSuccessContinuation ks, IMatchFailureContinuation kf, object cdr)
                {
                    this.cdrPattern = cdrPattern;
                    this.ks = ks;
                    this.kf = kf;
                    this.cdr = cdr;
                }

                public IRunnableStep Succeed(object[] matches)
                {
                    return new RunnableMatch(cdrPattern, cdr, matches, ks, kf);
                }

                public IMatchSuccessPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
                {
                    return a.Assoc<FirstHalfSuccess, FirstHalfSuccessPartial>(this, delegate() { return new FirstHalfSuccessPartial(cdrPattern, ks.PartialCapture(baseMark, a), kf.PartialCapture(baseMark, a), cdr); });
                }
            }

            public IRunnableStep Match(object obj, object[] matches, IMatchSuccessContinuation ks, IMatchFailureContinuation kf)
            {
                if (obj is ConsCell)
                {
                    ConsCell cc = (ConsCell)obj;
                    return new RunnableMatch(carPattern, cc.car, matches, new FirstHalfSuccess(cdrPattern, ks, kf, cc.cdr), kf);
                }
                else
                {
                    return new RunnableFailure(kf);
                }
            }
        }

        public IPatternMatchExpression Compile(Symbol[] placements)
        {
            return new MatchCons(carPattern.Compile(placements), cdrPattern.Compile(placements));
        }
    }

    public interface IMatchClauseExpression
    {
        IRunnableStep Run(object dataToMatch, Environment env, IContinuation k, IMatchFailureContinuation kf);
    }

    public interface IMatchClauseExpressionSource
    {
        EnvSpec GetRequirements();
        IMatchClauseExpression Compile(EnvDesc env);
    }

    public class RunnableMatchClause : IRunnableStep
    {
        private IMatchClauseExpression expr;
        private object dataToMatch;
        private Environment env;
        private IContinuation k;
        private IMatchFailureContinuation kf;

        public RunnableMatchClause
        (
            IMatchClauseExpression expr,
            object dataToMatch,
            Environment env,
            IContinuation k,
            IMatchFailureContinuation kf
        )
        {
            this.expr = expr;
            this.dataToMatch = dataToMatch;
            this.env = env;
            this.k = k;
            this.kf = kf;
        }

        public IRunnableStep Run(IGlobalState gs)
        {
            return expr.Run(dataToMatch, env, k, kf);
        }
    }

    public class SyntaxAnalyzer
    {
        public static IPatternMatchExpressionSource AnalyzePattern(object pat)
        {
            if (pat is ConsCell)
            {
                return AnalyzeConsCell((ConsCell)pat);
            }
            else if (pat is Symbol)
            {
                return AnalyzeSymbol((Symbol)pat);
            }
            else if (ExprObjModel.Procedures.ProxyDiscovery.IsAtom(pat))
            {
                return new MatchAtomicSource(pat);
            }
            else throw new ExprObjModel.SyntaxAnalysis.SchemeSyntaxException("Ill-formed pattern");
        }

        private static IPatternMatchExpressionSource AnalyzeConsCell(ConsCell pat)
        {
            return new MatchConsSource(AnalyzePattern(pat.car), AnalyzePattern(pat.cdr));
        }

        private static IPatternMatchExpressionSource AnalyzeSymbol(Symbol pat)
        {
            if (ExprObjModel.SyntaxAnalysis.PatternBuilder.IsPatternVariable(pat))
            {
                return new MatchBindSource(pat);
            }
            else
            {
                return new MatchAtomicSource(pat);
            }
        }
    }

    public class MatchClauseSource : IMatchClauseExpressionSource
    {
        private IPatternMatchExpressionSource pattern;
        private IExpressionSource guard;
        private IExpressionSource body;

        public MatchClauseSource(IPatternMatchExpressionSource pattern, IExpressionSource guard, IExpressionSource body)
        {
            this.pattern = pattern;
            this.guard = guard;
            this.body = body;
        }

        public class MatchClauseExpression : IMatchClauseExpression
        {
            private IPatternMatchExpression pattern;
            private int patternVars;
            private IExpression guard;
            private IExpression body;
            private int[] guardBodyMapping;

            public MatchClauseExpression
            (
                IPatternMatchExpression pattern,
                int patternVars,
                IExpression guard,
                IExpression body,
                int[] guardBodyMapping
            )
            {
                this.pattern = pattern;
                this.patternVars = patternVars;
                this.guard = guard;
                this.body = body;
                this.guardBodyMapping = guardBodyMapping;
            }
            
            public IRunnableStep Run(object dataToMatch, Environment env, IContinuation k, IMatchFailureContinuation kf)
            {
                return new RunnableMatch
                (
                    pattern, dataToMatch, new object[patternVars],
                    new SuccessContinuation
                    (
                        guard, body, guardBodyMapping, env, k, kf
                    ),
                    kf
                );
            }
        }

        public class SuccessPartialContinuation : IMatchSuccessPartialContinuation
        {
            IExpression guard;
            IExpression body;
            int[] guardBodyMapping;
            Environment env;
            IPartialContinuation k;
            IMatchFailurePartialContinuation kf;

            public SuccessPartialContinuation
            (
                IExpression guard,
                IExpression body,
                int[] guardBodyMapping,
                Environment env,
                IPartialContinuation k,
                IMatchFailurePartialContinuation kf
            )
            {
                this.guard = guard;
                this.body = body;
                this.guardBodyMapping = guardBodyMapping;
                this.env = env;
                this.k = k;
                this.kf = kf;
            }

            public IMatchSuccessContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<SuccessPartialContinuation, SuccessContinuation>(this, delegate() { return new SuccessContinuation(guard, body, guardBodyMapping, env, k.Attach(theNewBase, a), kf.Attach(theNewBase, a)); });
            }
        }

        public class SuccessContinuation : IMatchSuccessContinuation
        {
            IExpression guard;
            IExpression body;
            int[] guardBodyMapping;
            Environment env;
            IContinuation k;
            IMatchFailureContinuation kf;

            public SuccessContinuation
            (
                IExpression guard,
                IExpression body,
                int[] guardBodyMapping,
                Environment env,
                IContinuation k,
                IMatchFailureContinuation kf
            )
            {
                this.guard = guard;
                this.body = body;
                this.guardBodyMapping = guardBodyMapping;
                this.env = env;
                this.k = k;
                this.kf = kf;
            }

            public IRunnableStep Succeed(object[] matches)
            {
                Environment envGuardBody = env.Extend(guardBodyMapping, matches);
                return new RunnableEval(guard, envGuardBody, new GuardContinuation(body, envGuardBody, k, kf));
            }

            public IMatchSuccessPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
            {
                return a.Assoc<SuccessContinuation, SuccessPartialContinuation>(this, delegate() { return new SuccessPartialContinuation(guard, body, guardBodyMapping, env, k.PartialCapture(baseMark, a), kf.PartialCapture(baseMark, a)); });
            }
        }

        public class GuardPartialContinuation : IPartialContinuation
        {
            IExpression body;
            Environment env;
            IPartialContinuation k;
            IMatchFailurePartialContinuation kf;

            public GuardPartialContinuation(IExpression body, Environment env, IPartialContinuation k, IMatchFailurePartialContinuation kf)
            {
                this.body = body; this.env = env; this.k = k; this.kf = kf;
            }

            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<GuardPartialContinuation, GuardContinuation>(this, delegate() { return new GuardContinuation(body, env, k.Attach(theNewBase, a), kf.Attach(theNewBase, a)); });
            }
        }

        public class GuardContinuation : IContinuation
        {
            IExpression body;
            Environment env;
            IContinuation k;
            IMatchFailureContinuation kf;

            public GuardContinuation(IExpression body, Environment env, IContinuation k, IMatchFailureContinuation kf)
            {
                this.body = body; this.env = env; this.k = k; this.kf = kf;
            }

            public IRunnableStep Return(IGlobalState gs, object v)
            {
                if (!(v is bool) || ((bool)v == true))
                {
                    return new RunnableEval(body, env, k);
                }
                else
                {
                    return new RunnableFailure(kf);
                }
            }

            public IRunnableStep Throw(IGlobalState gs, object exc) { return new RunnableThrow(k, exc); }

            public IContinuation Parent { get { return k; } }
            public IProcedure EntryProc { get { return null; } }
            public IProcedure ExitProc { get { return null; } }

            public IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
            {
                return a.Assoc<GuardContinuation, GuardPartialContinuation>(this, delegate() { return new GuardPartialContinuation(body, env, k.PartialCapture(baseMark, a), kf.PartialCapture(baseMark, a)); });
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }

            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }
        }

        #region IMatchClauseExpressionSource Members

        public EnvSpec GetRequirements()
        {
            return (guard.GetRequirements() | body.GetRequirements()) - pattern.GetParameters();
        }

        public IMatchClauseExpression Compile(EnvDesc envDesc)
        {
            Symbol[] s = pattern.GetParameters().ToArray();

            EnvSpec guardBodyEnvSpec = GetRequirements();

            EnvDesc guardBodyEnvDesc; int[] guardBodyMapping;
            envDesc.SubsetShadowExtend(guardBodyEnvSpec, s, out guardBodyEnvDesc, out guardBodyMapping);

            IPatternMatchExpression cPattern = pattern.Compile(s);
            IExpression cGuard = guard.Compile(guardBodyEnvDesc);
            IExpression cBody = body.Compile(guardBodyEnvDesc);

            return new MatchClauseExpression(cPattern, s.Length, cGuard, cBody, guardBodyMapping);
        }

        #endregion
    }

    public class MatchSource : IExpressionSource
    {
        IExpressionSource dataToMatch;
        FList<IMatchClauseExpressionSource> matchClauses;
        IExpressionSource elseClause;

        public MatchSource(IExpressionSource dataToMatch, FList<IMatchClauseExpressionSource> matchClauses, IExpressionSource elseClause)
        {
            this.dataToMatch = dataToMatch;
            this.matchClauses = matchClauses;
            this.elseClause = elseClause;
        }

        #region IExpressionSource Members

        public EnvSpec GetRequirements()
        {
            EnvSpec e = dataToMatch.GetRequirements() | ((elseClause == null) ? EnvSpec.EmptySet : elseClause.GetRequirements());
            EnvSpec f = FListUtils.ToEnumerable(matchClauses).Select(ee => ee.GetRequirements()).EnvSpecUnion();
            return e | f;
        }

        private class DataToMatchPartialContinuation : IPartialContinuation
        {
            private FList<IMatchClauseExpression> matchClauses;
            private IExpression elseClause;
            private Environment env;
            private IPartialContinuation k;

            public DataToMatchPartialContinuation(FList<IMatchClauseExpression> matchClauses, IExpression elseClause, Environment env, IPartialContinuation k)
            {
                this.matchClauses = matchClauses;
                this.elseClause = elseClause;
                this.env = env;
                this.k = k;
            }

            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<DataToMatchPartialContinuation, DataToMatchContinuation>(this, delegate() { return new DataToMatchContinuation(matchClauses, elseClause, env, k.Attach(theNewBase, a)); });
            }
        }

        private class DataToMatchContinuation : IContinuation
        {
            private FList<IMatchClauseExpression> matchClauses;
            private IExpression elseClause;
            private Environment env;
            private IContinuation k;

            public DataToMatchContinuation(FList<IMatchClauseExpression> matchClauses, IExpression elseClause, Environment env, IContinuation k)
            {
                this.matchClauses = matchClauses;
                this.elseClause = elseClause;
                this.env = env;
                this.k = k;
            }

            public IRunnableStep Return(IGlobalState gs, object v)
            {
                if (matchClauses == null)
                {
                    return new RunnableEval(elseClause, env, k);
                }
                else
                {
                    return new RunnableMatchClause
                    (
                        matchClauses.Head,
                        v,
                        env,
                        k,
                        new MatchFailureContinuation
                        (
                            v,
                            matchClauses.Tail,
                            elseClause,
                            env,
                            k
                        )
                    );
                }
            }

            public IRunnableStep Throw(IGlobalState gs, object exc) { return new RunnableThrow(k, exc); }

            public IContinuation Parent { get { return k; } }
            public IProcedure EntryProc { get { return null; } }
            public IProcedure ExitProc { get { return null; } }

            public IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
            {
                return a.Assoc<DataToMatchContinuation, DataToMatchPartialContinuation>(this, delegate() { return new DataToMatchPartialContinuation(matchClauses, elseClause, env, k.PartialCapture(baseMark, a)); });
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }

            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }
        }

        private class MatchFailurePartialContinuation : IMatchFailurePartialContinuation
        {
            private object dataToMatch;
            private FList<IMatchClauseExpression> matchClauses;
            private IExpression elseClause;
            private Environment env;
            private IPartialContinuation k;
            
            public MatchFailurePartialContinuation
            (
                object dataToMatch,
                FList<IMatchClauseExpression> matchClauses,
                IExpression elseClause,
                Environment env,
                IPartialContinuation k
            )
            {
                this.dataToMatch = dataToMatch;
                this.matchClauses = matchClauses;
                this.elseClause = elseClause;
                this.env = env;
                this.k = k;
            }

            public IMatchFailureContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<MatchFailurePartialContinuation, MatchFailureContinuation>(this, delegate() { return new MatchFailureContinuation(dataToMatch, matchClauses, elseClause, env, k.Attach(theNewBase, a)); });
            }
        }

        private class MatchFailureContinuation : IMatchFailureContinuation
        {
            private object dataToMatch;
            private FList<IMatchClauseExpression> matchClauses;
            private IExpression elseClause;
            private Environment env;
            private IContinuation k;
            
            public MatchFailureContinuation
            (
                object dataToMatch,
                FList<IMatchClauseExpression> matchClauses,
                IExpression elseClause,
                Environment env,
                IContinuation k
            )
            {
                this.dataToMatch = dataToMatch;
                this.matchClauses = matchClauses;
                this.elseClause = elseClause;
                this.env = env;
                this.k = k;
            }

            public IRunnableStep Fail()
            {
                if (matchClauses == null)
                {
                    return new RunnableEval(elseClause, env, k);
                }
                else
                {
                    return new RunnableMatchClause
                    (
                        matchClauses.Head,
                        dataToMatch,
                        env,
                        k,
                        new MatchFailureContinuation
                        (
                            dataToMatch,
                            matchClauses.Tail,
                            elseClause,
                            env,
                            k
                        )
                    );
                }
            }

            public IMatchFailurePartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
            {
                return a.Assoc<MatchFailureContinuation, MatchFailurePartialContinuation>(this, delegate() { return new MatchFailurePartialContinuation(dataToMatch, matchClauses, elseClause, env, k.PartialCapture(baseMark, a)); });
            }
        }

        private class Match : IExpression
        {
            private IExpression dataToMatch;
            private FList<IMatchClauseExpression> matchClauses;
            private IExpression elseClause;

            public Match(IExpression dataToMatch, FList<IMatchClauseExpression> matchClauses, IExpression elseClause)
            {
                this.dataToMatch = dataToMatch;
                this.matchClauses = matchClauses;
                this.elseClause = elseClause;
            }

            public IRunnableStep Eval(IGlobalState gs, Environment env, IContinuation k)
            {
                return new RunnableEval(dataToMatch, env, new DataToMatchContinuation(matchClauses, elseClause, env, k));
            }
        }

        public IExpression Compile(EnvDesc ed)
        {
            IExpression cDataToMatch = dataToMatch.Compile(ed);
            FList<IMatchClauseExpression> f = FListUtils.Map
            (
                matchClauses,
                delegate(IMatchClauseExpressionSource s) { return s.Compile(ed); }
            );
            IExpression cElseClause = (elseClause == null) ? MakeUnspecified.Instance : elseClause.Compile(ed);

            return new Match(cDataToMatch, f, cElseClause);
        }

        #endregion
    }
}
