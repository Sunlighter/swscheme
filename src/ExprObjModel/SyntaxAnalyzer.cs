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
using BigMath;
using ExprObjModel.MatchSyntax;
using ExprObjModel.ObjectSystem;

namespace ExprObjModel.SyntaxAnalysis
{
    public class SchemeSyntaxException : ApplicationException
    {
        public SchemeSyntaxException() : base() { }
        public SchemeSyntaxException(string excuse) : base(excuse) { }
        public SchemeSyntaxException(string excuse, Exception cause) : base(excuse, cause) { }
    }

    public static class SyntaxAnalyzer
    {
        private static IExpressionSource AnalyzeQuote(object obj)
        {
            return new LiteralSource(obj);
        }

        private static IExpressionSource QuasiCons(IExpressionSource i1, IExpressionSource i2)
        {
            if ((i1 is LiteralSource) && (i2 is LiteralSource))
            {
                LiteralSource l1 = (LiteralSource)i1;
                LiteralSource l2 = (LiteralSource)i2;
                return new LiteralSource(new ConsCell(l1.Value, l2.Value));
            }
            else
            {
                return new QuasiConsSource(i1, i2);
            }
        }

        private static IExpressionSource BuildQuasiquote(object obj, int level)
        {
            System.Diagnostics.Debug.Assert(level > 0);

            if (obj is ConsCell)
            {
                MatchCaptureSet m;

                m = pUnquote.Match(obj);
                if (m != null)
                {
                    if (level == 1)
                    {
                        return Analyze(m["<item>"]);
                    }
                    else
                    {
                        return QuasiCons
                        (
                            new LiteralSource(new Symbol("unquote")),
                            QuasiCons
                            (
                                BuildQuasiquote(m["<item>"], level - 1),
                                new LiteralSource(SpecialValue.EMPTY_LIST)
                            )
                        );
                    }
                }

                m = pUnquoteSplicing.Match(obj);
                if (m != null)
                {
                    if (level == 1)
                    {
                        return new QuasiAppendSource
                        (
                            Analyze(m["<item>"]),
                            BuildQuasiquote(m["<tail>"], level)
                        );
                    }
                    else
                    {
                        return QuasiCons
                        (
                            QuasiCons
                            (
                                new LiteralSource(new Symbol("unquote-splicing")),
                                BuildQuasiquote(m["<item>"], level - 1)
                            ),
                            BuildQuasiquote(m["<tail>"], level)
                        );
                    }
                }

                m = pQuasiquote.Match(obj);
                if (m != null)
                {
                    return QuasiCons
                    (
                        new LiteralSource(new Symbol("quasiquote")),
                        QuasiCons
                        (
                            BuildQuasiquote(m["<item>"], level + 1),
                            new LiteralSource(SpecialValue.EMPTY_LIST)
                        )
                    );
                }

                ConsCell cObj = (ConsCell)obj;

                return QuasiCons(BuildQuasiquote(cObj.car, level), BuildQuasiquote(cObj.cdr, level));
            }
            else if (obj is object[])
            {
                object list = ConsCell.VectorToList(obj);
                IExpressionSource listSrc = BuildQuasiquote(list, level);
                if (listSrc is LiteralSource)
                {
                    return new LiteralSource(obj);
                }
                else
                {
                    return new QuasiListToVectorSource(listSrc);
                }
            }
            else return new LiteralSource(obj);
        }

        private static IExpressionSource AnalyzeQuasiquote(object obj)
        {
            return BuildQuasiquote(obj, 1);
        }

        private static IExpressionSource AnalyzeSet(object var, object value)
        {
            if (!(var is Symbol))
            {
                throw new SchemeSyntaxException("Ill-formed set!: variable is not a symbol");
            }
            return new VarSetSource((Symbol)var, Analyze(value));
        }

        private static IExpressionSource AnalyzeDynamicRef(object var)
        {
            if (!(var is Symbol))
            {
                throw new SchemeSyntaxException("Ill-formed dynamic reference: variable is not a symbol");
            }
            return new DynamicVarRefSource((Symbol)var);
        }

        private static IExpressionSource AnalyzeDynamicSet(object var, object value)
        {
            if (!(var is Symbol))
            {
                throw new SchemeSyntaxException("Ill-formed dynamic set!: variable is not a symbol");
            }
            return new DynamicVarSetSource((Symbol)var, Analyze(value));
        }

        private static IExpressionSource AnalyzeFieldRef(object var)
        {
            if (!(var is Symbol))
            {
                throw new SchemeSyntaxException("Ill-formed field reference: field name is not a symbol");
            }
            return new LocalRefSource((Symbol)var);
        }

        private static IExpressionSource AnalyzeFieldSet(object var, object value)
        {
            if (!(var is Symbol))
            {
                throw new SchemeSyntaxException("Ill-formed field set!: field name is not a symbol");
            }
            return new LocalSetSource((Symbol)var, Analyze(value));
        }

        private static IExpressionSource AnalyzeBegin(object obj)
        {
            return BeginSource.New(AnalyzeList(obj));
        }

        private static IExpressionSource AnalyzeIfThen(object cond, object then)
        {
            return new IfThenElseSource(Analyze(cond), Analyze(then));
        }

        private static IExpressionSource AnalyzeIfThenElse(object cond, object then, object @else)
        {
            return new IfThenElseSource(Analyze(cond), Analyze(then), Analyze(@else));
        }

        private static IExpressionSource AnalyzeLambda(object vars, object body)
        {
            return new LambdaSource(vars, BeginSource.New(AnalyzeList(body)));
        }

        private static IExpressionSource AnalyzeAnd(object exprs)
        {
            return AndSource.New(AnalyzeList(exprs));
        }

        private static IExpressionSource AnalyzeOr(object exprs)
        {
            return OrSource.New(AnalyzeList(exprs));
        }

        private static IPattern pClauseApplyElse = PatternBuilder.BuildPattern("((<test1> => <body1>) (else . <body2>))");
        private static IPattern pClauseApplyEnd = PatternBuilder.BuildPattern("((<test1> => <body1>))");
        private static IPattern pClauseApplyMore = PatternBuilder.BuildPattern("((<test1> => <body1>) . <clauses>)");
        private static IPattern pClauseElse = PatternBuilder.BuildPattern("((<test1> . <body1>) (else . <body2>))");
        private static IPattern pClauseEnd = PatternBuilder.BuildPattern("((<test1> . <body1>))");
        private static IPattern pClauseMore = PatternBuilder.BuildPattern("((<test1> . <body1>) . <clauses>)");

        private static IExpressionSource AnalyzeCase(object expr, object clauses)
        {
            IExpressionSource aExpr = Analyze(expr);

            FList<CaseClause> clauseList = null;

            while (true)
            {
                MatchCaptureSet m = null;
                m = pClauseElse.Match(clauses);
                if (m != null)
                {
                    object test = m["<test1>"];
                    object body = m["<body1>"];
                    object elseClause = m["<body2>"];

                    SchemeHashSet set = ConsCell.Enumerate(test).ToSchemeHashSet();
                    IExpressionSource aBody = AnalyzeBegin(body);
                    IExpressionSource aElseClause = AnalyzeBegin(elseClause);
                    clauseList = new FList<CaseClause>(new CaseClause(set, aBody), clauseList);
                    clauseList = FListUtils.Reverse(clauseList);
                    return new CaseSource(aExpr, clauseList, aElseClause);
                }

                m = pClauseEnd.Match(clauses);
                if (m != null)
                {
                    object test = m["<test1>"];
                    object body = m["<body1>"];

                    SchemeHashSet set = ConsCell.Enumerate(test).ToSchemeHashSet();
                    IExpressionSource aBody = AnalyzeBegin(body);
                    IExpressionSource aElseClause = MakeUnspecifiedSource.Instance;
                    clauseList = new FList<CaseClause>(new CaseClause(set, aBody), clauseList);
                    clauseList = FListUtils.Reverse(clauseList);
                    return new CaseSource(aExpr, clauseList, aElseClause);
                }

                m = pClauseMore.Match(clauses);
                if (m != null)
                {
                    object test = m["<test1>"];
                    object body = m["<body1>"];
                    clauses = m["<clauses>"];

                    SchemeHashSet set = ConsCell.Enumerate(test).ToSchemeHashSet();
                    IExpressionSource aBody = AnalyzeBegin(body);
                    clauseList = new FList<CaseClause>(new CaseClause(set, aBody), clauseList);
                    // and loop around again
                }
                else
                {
                    throw new SchemeSyntaxException("Ill-formed case");
                }
            }
        }

        private static IExpressionSource MakeCondApplication(IExpressionSource test1, IExpressionSource body1, IExpressionSource body2)
        {
            Symbol testResult = new Symbol();
            return new LetSource
            (
                new FList<LetClause>
                (
                    new LetClause
                    (
                        testResult,
                        test1
                    )
                ),
                new IfThenElseSource
                (
                    new VarRefSource(testResult),
                    new InvocationSource
                    (
                        new FList<IExpressionSource>
                        (
                            body1,
                            new FList<IExpressionSource>
                            (
                                new VarRefSource(testResult)
                            )
                        )
                    ),
                    body2
                )
            );
        }

        private static IExpressionSource AnalyzeCond(object clauses)
        {
            MatchCaptureSet captures;
            captures = pClauseApplyElse.Match(clauses);
            if (captures != null)
            {
                Symbol test = new Symbol();
                return MakeCondApplication
                (
                    Analyze(captures["<test1>"]),
                    Analyze(captures["<body1>"]),
                    BeginSource.New(AnalyzeList(captures["<body2>"]))
                );
            }

            captures = pClauseApplyEnd.Match(clauses);
            if (captures != null)
            {
                return MakeCondApplication
                (
                    Analyze(captures["<test1>"]),
                    Analyze(captures["<body1>"]),
                    MakeUnspecifiedSource.Instance
                );
            }

            captures = pClauseApplyMore.Match(clauses);
            if (captures != null)
            {
                return MakeCondApplication
                (
                    Analyze(captures["<test1>"]),
                    Analyze(captures["<body1>"]),
                    AnalyzeCond(captures["<clauses>"])
                );
            }

            captures = pClauseElse.Match(clauses);
            if (captures != null)
            {
                return new IfThenElseSource
                (
                    Analyze(captures["<test1>"]),
                    BeginSource.New(AnalyzeList(captures["<body1>"])),
                    BeginSource.New(AnalyzeList(captures["<body2>"]))
                );
            }

            captures = pClauseEnd.Match(clauses);
            if (captures != null)
            {
                return new IfThenElseSource
                (
                    Analyze(captures["<test1>"]),
                    BeginSource.New(AnalyzeList(captures["<body1>"]))
                );
            }

            captures = pClauseMore.Match(clauses);
            if (captures != null)
            {
                return new IfThenElseSource
                (
                    Analyze(captures["<test1>"]),
                    BeginSource.New(AnalyzeList(captures["<body1>"])),
                    AnalyzeCond(captures["<clauses>"])
                );
            }

            throw new SchemeSyntaxException("Ill-formed cond clause");
        }

        private static LetClause AnalyzeLetClause(object var, object val)
        {
            Symbol sVar = (Symbol)var;
            LetClause l = new LetClause(sVar, Analyze(val));
            return l;
        }

        private static FList<LetClause> AnalyzeLetClauseList(object lcList)
        {
            return ConsCell.MapToFList
            (
                lcList,
                delegate(object obj)
                {
                    MatchCaptureSet m = pLetClause.Match(obj);
                    if (m != null)
                    {
                        return AnalyzeLetClause(m["<var>"], m["<value>"]);
                    }
                    else throw new SchemeSyntaxException("Ill-formed let clause");
                }
            );
        }

        private delegate IExpressionSource NewLetFamilyProc(FList<LetClause> aClauseList, IExpressionSource aBody);

        private static IExpressionSource AnalyzeLetFamily(object clauseList, object body, NewLetFamilyProc newLet, string name)
        {
            if (ConsCell.IsList(clauseList))
            {
                FList<LetClause> aClauseList = AnalyzeLetClauseList(clauseList);
                FList<IExpressionSource> aBody = AnalyzeList(body);
                return newLet(aClauseList, BeginSource.New(aBody));
            }
            else throw new SchemeSyntaxException("Ill-formed " + name);
        }

        private static IExpressionSource AnalyzeLet(object clauseList, object body)
        {
            return AnalyzeLetFamily
            (
                clauseList, body,
                delegate(FList<LetClause> aClauseList, IExpressionSource aBody) { return new LetSource(aClauseList, aBody); },
                "let"
            );
        }

        private static IExpressionSource AnalyzeDynamicLet(object clauseList, object body)
        {
            return AnalyzeLetFamily
            (
                clauseList, body,
                delegate(FList<LetClause> aClauseList, IExpressionSource aBody) { return new DynamicLetSource(aClauseList, aBody); },
                "dynamic-let"
            );
        }

        private static IExpressionSource AnalyzeLetStar(object clauseList, object body)
        {
            return AnalyzeLetFamily
            (
                clauseList, body,
                delegate(FList<LetClause> aClauseList, IExpressionSource aBody) { return new LetStarSource(aClauseList, aBody); },
                "let*"
            );
        }

        private static IExpressionSource AnalyzeUsing(object clauseList, object body)
        {
            return AnalyzeLetFamily
            (
                clauseList, body,
                delegate(FList<LetClause> aClauseList, IExpressionSource aBody) { return new UsingSource(aClauseList, aBody); },
                "using"
            );
        }

        private static IExpressionSource AnalyzeUsingStar(object clauseList, object body)
        {
            return AnalyzeLetFamily
            (
                clauseList, body,
                delegate(FList<LetClause> aClauseList, IExpressionSource aBody) { return new UsingStarSource(aClauseList, aBody); },
                "using*"
            );
        }

        private static IExpressionSource AnalyzeLetrec(object clauseList, object body)
        {
            return AnalyzeLetFamily
            (
                clauseList, body,
                delegate(FList<LetClause> aClauseList, IExpressionSource aBody) { return new LetrecSource(aClauseList, aBody); },
                "letrec"
            );
        }

        private static IExpressionSource AnalyzeLetrecStar(object clauseList, object body)
        {
            return AnalyzeLetFamily
            (
                clauseList, body,
                delegate(FList<LetClause> aClauseList, IExpressionSource aBody) { return new LetrecStarSource(aClauseList, aBody); },
                "letrec*"
            );
        }

        private static IExpressionSource AnalyzeLetLoop(object loop, object clauseList, object body)
        {
            Symbol aLoop = (Symbol)loop;
            if (ConsCell.IsList(clauseList))
            {
                FList<LetClause> aClauseList = AnalyzeLetClauseList(clauseList);
                FList<IExpressionSource> aBody = AnalyzeList(body);
                return new LetLoopSource(aLoop, aClauseList, BeginSource.New(aBody));
            }
            else throw new SchemeSyntaxException("Ill-formed let loop");
        }

        private static IExpressionSource AnalyzeMatch(object expr, object clauseList)
        {
            IExpressionSource aExpr = Analyze(expr);
            FList<IMatchClauseExpressionSource> aClauses;
            IExpressionSource aElse;
            AnalyzeMatchClauseList(clauseList, out aClauses, out aElse);
            return new MatchSource(aExpr, aClauses, aElse);
        }

        private static void AnalyzeMatchClauseList(object clauseList, out FList<IMatchClauseExpressionSource> aClauses, out IExpressionSource aElse)
        {
            aClauses = null;
            aElse = null;
            while (clauseList is ConsCell)
            {
                ConsCell cc = (ConsCell)clauseList;
                bool isElse = false;
                if (cc.cdr is SpecialValue && ((SpecialValue)cc.cdr == SpecialValue.EMPTY_LIST))
                {
                    MatchCaptureSet m2 = pMatchElseClause.Match(cc.car);
                    if (m2 != null)
                    {
                        isElse = true;
                        aElse = AnalyzeBegin(m2["<body>"]);
                    }
                }
                if (!isElse)
                {
                    MatchCaptureSet m1 = pMatchClause.Match(cc.car);
                    if (m1 != null)
                    {
                        IPatternMatchExpressionSource p = MatchSyntax.SyntaxAnalyzer.AnalyzePattern(m1["<pat>"]);
                        IExpressionSource guard = Analyze(m1["<guard>"]);
                        IExpressionSource body = AnalyzeBegin(m1["<body>"]);
                        aClauses = new FList<IMatchClauseExpressionSource>(new MatchClauseSource(p, guard, body), aClauses);
                    }
                    else throw new SchemeSyntaxException("Ill-formed match [2]");
                }

                clauseList = cc.cdr;
            }
            if (!(clauseList is SpecialValue) || ((SpecialValue)clauseList != SpecialValue.EMPTY_LIST))
            {
                throw new SchemeSyntaxException("Ill-formed match [3]");
            }
            aClauses = FListUtils.Reverse(aClauses);
        }

        private static IExpressionSource AnalyzeCatch(object handler, object bodyList)
        {
            IExpressionSource sHandler = Analyze(handler);
            IExpressionSource sBody = AnalyzeBegin(bodyList);
            return new CatchSource(sHandler, sBody);
        }

        private static IExpressionSource AnalyzeDynamicWind(object entry, object body, object exit)
        {
            IExpressionSource sEntry = Analyze(entry);
            IExpressionSource sBody = Analyze(body);
            IExpressionSource sExit = Analyze(exit);
            return new DynamicWindSource(sEntry, sBody, sExit);
        }

        private static IExpressionSource AnalyzeMsgLambda(object args, object body)
        {
            if (args is Signature)
            {
                Signature sig = (Signature)args;
                args = new Message<object>(sig.Type, sig.Parameters.Select(x => new Tuple<Symbol, object>(x, x)));
            }
            if (!(args is Message<object>)) throw new SchemeSyntaxException("Ill-formed mlambda: arg must be a message");
            Message<object> args1 = (Message<object>)args;
            if (args1.Values.Any(x => !(x is Symbol))) throw new SchemeSyntaxException("Ill-formed mlambda: arg message must bind to symbols");
            Message<Symbol> args2 = args1.Map<Symbol>(x => (Symbol)x);
            IExpressionSource sBody = AnalyzeBegin(body);
            return new MsgLambdaSource(args2, sBody);
        }

        private static Message<Symbol> AnalyzeSelector(object obj)
        {
            if (obj is Message<object>)
            {
                Message<object> m = (Message<object>)obj;
                if (m.Values.Any(x => !(x is Symbol))) throw new SchemeSyntaxException("Message arguments must be bound to symbols");
                return m.Map<Symbol>(x => (Symbol)x);
            }
            else if (obj is Signature)
            {
                Signature s = (Signature)obj;
                return new Message<Symbol>(s.Type, s.Parameters.Select(x => new Tuple<Symbol, Symbol>(x, x)));
            }
            else throw new SchemeSyntaxException("Message or signature expected");
        }

        private static IExpressionSource AnalyzeMsgCase(object expr, object clauses)
        {
            IExpressionSource aExpr = Analyze(expr);

            FList<MsgCaseClauseSource> clauseList = null;

            while (true)
            {
                MatchCaptureSet m = null;
                m = pClauseElse.Match(clauses);
                if (m != null)
                {
                    object test = m["<test1>"];
                    object body = m["<body1>"];
                    object elseClause = m["<body2>"];

                    IExpressionSource aBody = AnalyzeBegin(body);
                    IExpressionSource aElseClause = AnalyzeBegin(elseClause);
                    clauseList = new FList<MsgCaseClauseSource>(new MsgCaseClauseSource(AnalyzeSelector(test), aBody), clauseList);
                    clauseList = FListUtils.Reverse(clauseList);
                    return new MsgCaseSource(aExpr, clauseList, aElseClause);
                }

                m = pClauseEnd.Match(clauses);
                if (m != null)
                {
                    object test = m["<test1>"];
                    object body = m["<body1>"];

                    IExpressionSource aBody = AnalyzeBegin(body);
                    IExpressionSource aElseClause = MakeUnspecifiedSource.Instance;
                    clauseList = new FList<MsgCaseClauseSource>(new MsgCaseClauseSource(AnalyzeSelector(test), aBody), clauseList);
                    clauseList = FListUtils.Reverse(clauseList);
                    return new MsgCaseSource(aExpr, clauseList, aElseClause);
                }

                m = pClauseMore.Match(clauses);
                if (m != null)
                {
                    object test = m["<test1>"];
                    object body = m["<body1>"];
                    clauses = m["<clauses>"];

                    IExpressionSource aBody = AnalyzeBegin(body);
                    clauseList = new FList<MsgCaseClauseSource>(new MsgCaseClauseSource(AnalyzeSelector(test), aBody), clauseList);
                    // and loop around again
                }
                else
                {
                    throw new SchemeSyntaxException("Ill-formed mcase");
                }
            }
        }

        private delegate IExpressionSource AnalyzeProc(MatchCaptureSet m);

        private class PatternParseInfo
        {
            private IPattern pattern;
            private AnalyzeProc analyzeProc;

            public PatternParseInfo(string pattern, AnalyzeProc analyzeProc)
            {
                this.pattern = PatternBuilder.BuildPattern(pattern);
                this.analyzeProc = analyzeProc;
            }

            public IExpressionSource Match(object obj)
            {
                MatchCaptureSet m = pattern.Match(obj);
                if (m == null) return null;
                return analyzeProc(m);
            }
        }

        private static PatternParseInfo[] patternParseInfoArray = new PatternParseInfo[]
        {
            new PatternParseInfo("'<item>", delegate(MatchCaptureSet m) { return AnalyzeQuote(m["<item>"]); }),
            new PatternParseInfo("`<item>", delegate(MatchCaptureSet m) { return AnalyzeQuasiquote(m["<item>"]); }),
            new PatternParseInfo("(dynamic <var>)", delegate(MatchCaptureSet m) { return AnalyzeDynamicRef(m["<var>"]); }),
            new PatternParseInfo("(local <var>)", delegate(MatchCaptureSet m) { return AnalyzeFieldRef(m["<var>"]); }),
            new PatternParseInfo("(set! (dynamic <var>) <value>)", delegate(MatchCaptureSet m) { return AnalyzeDynamicSet(m["<var>"], m["<value>"]); }),
            new PatternParseInfo("(set! (local <var>) <value>)", delegate(MatchCaptureSet m) { return AnalyzeFieldSet(m["<var>"], m["<value>"]); }),
            new PatternParseInfo("(set! <var> <value>)", delegate(MatchCaptureSet m) { return AnalyzeSet(m["<var>"], m["<value>"]); }),
            new PatternParseInfo("(begin . <body>)", delegate(MatchCaptureSet m) { return AnalyzeBegin(m["<body>"]); }),
            new PatternParseInfo("(if <test> <then>)", delegate(MatchCaptureSet m) { return AnalyzeIfThen(m["<test>"], m["<then>"]); }),
            new PatternParseInfo("(if <test> <then> <else>)", delegate(MatchCaptureSet m) { return AnalyzeIfThenElse(m["<test>"], m["<then>"], m["<else>"]); }),
            new PatternParseInfo("(lambda <vars> . <body>)", delegate(MatchCaptureSet m) { return AnalyzeLambda(m["<vars>"], m["<body>"]); }),
            new PatternParseInfo("(and . <exprs>)", delegate(MatchCaptureSet m) { return AnalyzeAnd(m["<exprs>"]); }),
            new PatternParseInfo("(or . <exprs>)", delegate(MatchCaptureSet m) { return AnalyzeOr(m["<exprs>"]); }),
            new PatternParseInfo("(cond . <clauses>)", delegate(MatchCaptureSet m) { return AnalyzeCond(m["<clauses>"]); }),
            new PatternParseInfo
            (
                "(let <loop> <clauselist> . <body>)",
                delegate(MatchCaptureSet m)
                {
                    object loop = m["<loop>"];
                    if (!(loop is Symbol)) return null;
                    return AnalyzeLetLoop(loop, m["<clauselist>"], m["<body>"]);
                }
            ),
            new PatternParseInfo("(case <expr> . <clauses>)", delegate(MatchCaptureSet m) { return AnalyzeCase(m["<expr>"], m["<clauses>"]); }),
            new PatternParseInfo("(let <clauselist> . <body>)", delegate(MatchCaptureSet m) { return AnalyzeLet(m["<clauselist>"], m["<body>"]); }),
            new PatternParseInfo("(dynamic-let <clauselist> . <body>)", delegate(MatchCaptureSet m) { return AnalyzeDynamicLet(m["<clauselist>"], m["<body>"]); }),
            new PatternParseInfo("(let* <clauselist> . <body>)", delegate(MatchCaptureSet m) { return AnalyzeLetStar(m["<clauselist>"], m["<body>"]); }),
            new PatternParseInfo("(using <clauselist> . <body>)", delegate(MatchCaptureSet m) { return AnalyzeUsing(m["<clauselist>"], m["<body>"]); }),
            new PatternParseInfo("(using* <clauselist> . <body>)", delegate(MatchCaptureSet m) { return AnalyzeUsingStar(m["<clauselist>"], m["<body>"]); }),
            new PatternParseInfo("(letrec <clauselist> . <body>)", delegate(MatchCaptureSet m) { return AnalyzeLetrec(m["<clauselist>"], m["<body>"]); }),
            new PatternParseInfo("(letrec* <clauselist> . <body>)", delegate(MatchCaptureSet m) { return AnalyzeLetrecStar(m["<clauselist>"], m["<body>"]); }),
            new PatternParseInfo("(__match <data> . <clauselist>)", delegate(MatchCaptureSet m) { return AnalyzeMatch(m["<data>"], m["<clauselist>"]); }),
            new PatternParseInfo("(catch <handler> . <body>)", delegate(MatchCaptureSet m) { return AnalyzeCatch(m["<handler>"], m["<body>"]); }),
            new PatternParseInfo("(dynamic-wind <entry> <body> <exit>)", delegate(MatchCaptureSet m) { return AnalyzeDynamicWind(m["<entry>"], m["<body>"], m["<exit>"]); }),
            new PatternParseInfo("(mlambda <args> . <body>)", delegate(MatchCaptureSet m) { return AnalyzeMsgLambda(m["<args>"], m["<body>"]); }),
            new PatternParseInfo("(mcase <expr> . <clauses>)", delegate(MatchCaptureSet m) { return AnalyzeMsgCase(m["<expr>"], m["<clauses>"]); }),
        };

        private static IPattern pQuasiquote = PatternBuilder.BuildPattern("`<item>");
        private static IPattern pUnquote = PatternBuilder.BuildPattern(",<item>");
        private static IPattern pUnquoteSplicing = PatternBuilder.BuildPattern("(,@<item> . <tail>)");
        private static IPattern pLetClause = PatternBuilder.BuildPattern("(<var> <value>)");

        private static IPattern pMatchClause = PatternBuilder.BuildPattern("(<pat> <guard> . <body>)");
        private static IPattern pMatchElseClause = PatternBuilder.BuildPattern("(else . <body>)");

        private static FList<IExpressionSource> AnalyzeList(object list)
        {
            try
            {
                return ConsCell.MapToFList(list, new Func<object, IExpressionSource>(Analyze));
            }
            catch (Exception e)
            {
                throw new SchemeSyntaxException("Ill-formed expression list", e);
            }
        }

        private static IExpressionSource AnalyzeMap(SchemeHashMap map)
        {
            FList<Tuple<object, IExpressionSource>> f = null;
            foreach (KeyValuePair<object, object> kvp in map)
            {
                IExpressionSource expr = Analyze(kvp.Value);
                f = new FList<Tuple<object, IExpressionSource>>(new Tuple<object, IExpressionSource>(kvp.Key, expr), f);
            }
            return new MapSource(f);
        }

        private static IExpressionSource AnalyzeMessage(Message<object> m)
        {
            FList<Tuple<Symbol, IExpressionSource>> f = m.Arguments.Select(x => new Tuple<Symbol, IExpressionSource>(x.Item1, Analyze(x.Item2))).ToFList(true);
            return new MessageSource(m.Type, f);
        }

        [SchemeFunction("analyze-syntax")]
        public static IExpressionSource Analyze(object obj)
        {
            if (LiteralSource.IsSelfEvaluating(obj))
            {
                return new LiteralSource(obj);
            }
            else if (obj is Symbol)
            {
                return new VarRefSource((Symbol)obj);
            }
            else if (obj is SchemeHashMap)
            {
                return AnalyzeMap((SchemeHashMap)obj);
            }
            else if (obj is Message<object>)
            {
                return AnalyzeMessage((Message<object>)obj);
            }
            else if (obj is ConsCell)
            {
                foreach (PatternParseInfo ppi in patternParseInfoArray)
                {
                    IExpressionSource ies = ppi.Match(obj);
                    if (ies != null) return ies;
                }

                FList<IExpressionSource> list = AnalyzeList(obj);
                return new InvocationSource(list);
            }
            throw new SchemeSyntaxException("Unrecognized or ill-formed syntax");
        }
    }
}
