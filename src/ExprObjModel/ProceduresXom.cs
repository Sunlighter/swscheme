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
using ControlledWindowLib;

namespace ExprObjModel.Procedures
{
    public static partial class ProxyDiscovery
    {
        [SchemeFunction("xom-expression-source?")]
        public static bool IsXomExpressionSource(object obj)
        {
            return (obj is IExpressionSource);
        }

        [SchemeFunction("xom-literal")]
        public static object XomLiteral(object literal)
        {
            return new LiteralSource(literal);
        }

        [SchemeFunction("xom-var-ref")]
        public static object XomVarRef(Symbol varName)
        {
            return new VarRefSource(varName);
        }

        [SchemeFunction("xom-var-set")]
        public static object XomVarSet(Symbol varName, IExpressionSource exprValue)
        {
            return new VarSetSource(varName, exprValue);
        }

        [SchemeFunction("xom-make-unspecified")]
        public static object XomMakeUnspecified()
        {
            return MakeUnspecifiedSource.Instance;
        }

        [SchemeFunction("xom-if-then")]
        public static object XomIfThen(IExpressionSource test, IExpressionSource consequence)
        {
            return new IfThenElseSource(test, consequence);
        }

        [SchemeFunction("xom-if-then-else")]
        public static object XomIfThenElse(IExpressionSource test, IExpressionSource consequence, IExpressionSource alternate)
        {
            return new IfThenElseSource(test, consequence, alternate);
        }

        [SchemeFunction("xom-lambda")]
        public static object XomLambda(object paramList, IExpressionSource body)
        {
            return new LambdaSource(paramList, body);
        }

        [SchemeFunction("xom-quasi-cons")]
        public static object XomQuasiCons(IExpressionSource car, IExpressionSource cdr)
        {
            return new QuasiConsSource(car, cdr);
        }

        [SchemeFunction("xom-quasi-append")]
        public static object XomQuasiAppend(IExpressionSource l1, IExpressionSource l2)
        {
            return new QuasiAppendSource(l1, l2);
        }

        [SchemeFunction("xom-quasi-list-to-vector")]
        public static object XomQuasiListToVector(IExpressionSource list)
        {
            return new QuasiListToVectorSource(list);
        }

        public static FList<IExpressionSource> ToExpressions(FList<object> argList)
        {
            FList<IExpressionSource> exprs = null;
            while (argList != null)
            {
                if (argList.Head is IExpressionSource)
                {
                    exprs = new FList<IExpressionSource>((IExpressionSource)(argList.Head), exprs);
                    argList = argList.Tail;
                }
                else throw new SchemeRuntimeException("Expecting parameters of type IExpressionSource");
            }
            return FListUtils.Reverse(exprs);
        }

        [SchemeFunction("xom-let-clause?")]
        public static bool IsXomLetSource(object obj)
        {
            return obj is LetSource;
        }

        [SchemeFunction("xom-let-clause")]
        public static object XomLetClause(Symbol name, IExpressionSource exprValue)
        {
            return new LetClause(name, exprValue);
        }

        public static Tuple<FList<LetClause>, IExpressionSource> ToLetClausesAndExpression(FList<object> argList)
        {
            FList<LetClause> letClauses = null;
            while (argList != null)
            {
                if (argList.Head is LetClause)
                {
                    letClauses = new FList<LetClause>((LetClause)(argList.Head), letClauses);
                    argList = argList.Tail;
                }
                else if (argList.Head is IExpressionSource) break;
                else throw new SchemeRuntimeException("Expecting parameters of type LetClause followed by an IExpressionSource");
            }
            if (argList == null) throw new SchemeRuntimeException("Expecting IExpressionSource at end of parameters");
            IExpressionSource expr = (IExpressionSource)(argList.Head);
            argList = argList.Tail;
            if (argList != null) throw new SchemeRuntimeException("Expecting IExpressionSource to be last parameter");
            return new Tuple<FList<LetClause>, IExpressionSource>(FListUtils.Reverse(letClauses), expr);
        }

        [SchemeFunction("xom-catch")]
        public static object XomCatch(IExpressionSource handler, IExpressionSource body)
        {
            return new CatchSource(handler, body);
        }

        [SchemeFunction("xom-dynamic-wind")]
        public static object XomDynamicWind(IExpressionSource entry, IExpressionSource body, IExpressionSource exit)
        {
            return new DynamicWindSource(entry, body, exit);
        }

        [SchemeFunction("xom-get-requirements")]
        public static object XomGetRequirements(IExpressionSource expr)
        {
            return expr.GetRequirements();
        }

        [SchemeFunction("xom-envspec?")]
        public static bool IsXomEnvSpec(object obj)
        {
            return obj is EnvSpec;
        }

        [SchemeFunction("xom-envdesc?")]
        public static bool IsXomEnvDesc(object obj)
        {
            return obj is EnvSpec;
        }

        [SchemeFunction("envspec->list")]
        public static object EnvSpecToList(EnvSpec envSpec)
        {
            object list = SpecialValue.EMPTY_LIST;
            foreach (Symbol sym in envSpec)
            {
                list = new ConsCell(sym, list);
            }
            ConsCell.Reverse(ref list);
            return list;
        }

        [SchemeFunction("list->envdesc")]
        public static object ListToEnvDesc(object list)
        {
            System.Collections.Generic.List<Symbol> symList = new System.Collections.Generic.List<Symbol>();
            while (!ConsCell.IsEmptyList(list))
            {
                if (!(list is ConsCell)) throw new SchemeRuntimeException("List expected");
                ConsCell cList = (ConsCell)list;
                if (!(cList.car is Symbol)) throw new SchemeRuntimeException("List of symbols expected");
                Symbol sCar = (Symbol)(cList.car);
                symList.Add(sCar);
                list = cList.cdr;
            }
            EnvDesc e = null;
            EnvDesc.Empty.Extend(symList.ToArray(), out e);
            return e;
        }

        [SchemeFunction("envspec->envdesc")]
        public static object EnvSpecToEnvDesc(EnvSpec spec)
        {
            EnvDesc e = null;
            EnvDesc.Empty.Extend(spec.ToArray(), out e);
            return e;
        }

        [SchemeFunction("envdesc->envspec")]
        public static object EnvDescToEnvSpec(EnvDesc desc)
        {
            return ((EnvSpec)desc);
        }

        [SchemeFunction("envdesc->vector")]
        public static object EnvDescToVector(EnvDesc desc)
        {
            Deque<object> vec = new Deque<object>();
            int iEnd = desc.Count;
            for (int i = 0; i < iEnd; ++i) vec.PushBack(false);
            foreach (System.Collections.Generic.KeyValuePair<Symbol, int> kvp in desc)
            {
                vec[kvp.Value] = kvp.Key;
            }
            return vec;
        }

        [SchemeFunction("xom-envdesc-lookup")]
        public static object EnvDescLookup(EnvDesc desc, Symbol sym)
        {
            if (desc.Defines(sym)) return BigMath.BigInteger.FromInt32(desc[sym]);
            else return false;
        }

        [SchemeFunction("xom-envdesc-count")]
        public static int EnvDescCount(EnvDesc desc)
        {
            return desc.Count;
        }

        [SchemeFunction("xom-get-empty-envdesc")]
        public static object XomEnvDescEmpty()
        {
            return EnvDesc.Empty;
        }

        [SchemeFunction("xom-compile")]
        public static object XomCompile(IExpressionSource expr, EnvDesc ed)
        {
            return expr.Compile(ed);
        }

        [SchemeFunction("xom-expression?")]
        public static bool IsXomExpression(object obj)
        {
            return (obj is IExpression);
        }

        [SchemeFunction("xom-environment?")]
        public static bool IsEomEnvironment(object obj)
        {
            return (obj is Environment);
        }

        [SchemeFunction("xom-get-empty-environment")]
        public static object XomGetEmptyEnvironment()
        {
            return Environment.Empty;
        }

        [SchemeFunction("vector->environment")]
        public static object VectorToEnvironment(Deque<object> values)
        {
            Box[] b = new Box[values.Count];
            int iEnd = values.Count;
            for (int i = 0; i < iEnd; ++i)
            {
                b[i] = new Box(values[i]);
            }
            return Environment.Empty.Extend(b);
        }

        [SchemeFunction("xom-placeholder")]
        public static object XomPlaceholder()
        {
            return new PlaceholderSource();
        }

        [SchemeFunction("xom-placeholder?")]
        public static bool IsXomPlaceholder(object obj)
        {
            return (obj is PlaceholderSource);
        }

        [SchemeFunction("xom-set-placeholder!")]
        public static void SetXomPlaceholder(PlaceholderSource p, IExpressionSource content)
        {
            p.Content = content;
        }

        [SchemeFunction("xom-is-placeholder-set?")]
        public static bool IsXomPlaceholderSet(PlaceholderSource p)
        {
            return p.Content != null;
        }
    }

    [SchemeSingleton("xom-begin")]
    public class XomBeginProc : IProcedure
    {
        public XomBeginProc() { }

        public int Arity { get { return 0; } }

        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            FList<IExpressionSource> exprs = ProxyDiscovery.ToExpressions(argList);
            return new RunnableReturn(k, BeginSource.New(exprs));
        }
    }

    [SchemeSingleton("xom-invocation")]
    public class XomInvocationProc : IProcedure
    {
        public XomInvocationProc() { }

        public int Arity { get { return 1; } }

        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            FList<IExpressionSource> exprs = ProxyDiscovery.ToExpressions(argList);
            return new RunnableReturn(k, new InvocationSource(exprs));
        }
    }

    [SchemeSingleton("xom-and")]
    public class XomAndProc : IProcedure
    {
        public XomAndProc() { }

        public int Arity { get { return 0; } }

        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            FList<IExpressionSource> exprs = ProxyDiscovery.ToExpressions(argList);
            return new RunnableReturn(k, AndSource.New(exprs));
        }
    }

    [SchemeSingleton("xom-or")]
    public class XomOrProc : IProcedure
    {
        public XomOrProc() { }

        public int Arity { get { return 0; } }

        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            FList<IExpressionSource> exprs = ProxyDiscovery.ToExpressions(argList);
            return new RunnableReturn(k, OrSource.New(exprs));
        }
    }

    [SchemeSingleton("xom-let")]
    public class XomLetProc : IProcedure
    {
        public XomLetProc() { }

        public int Arity { get { return 1; } }

        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            Tuple<FList<LetClause>, IExpressionSource> args = ProxyDiscovery.ToLetClausesAndExpression(argList);
            return new RunnableReturn(k, new LetSource(args.Item1, args.Item2));
        }
    }

    [SchemeSingleton("xom-let*")]
    public class XomLetStarProc : IProcedure
    {
        public XomLetStarProc() { }

        public int Arity { get { return 1; } }

        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            Tuple<FList<LetClause>, IExpressionSource> args = ProxyDiscovery.ToLetClausesAndExpression(argList);
            return new RunnableReturn(k, new LetStarSource(args.Item1, args.Item2));
        }
    }

    [SchemeSingleton("xom-letrec")]
    public class XomLetrecProc : IProcedure
    {
        public XomLetrecProc() { }

        public int Arity { get { return 1; } }

        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            Tuple<FList<LetClause>, IExpressionSource> args = ProxyDiscovery.ToLetClausesAndExpression(argList);
            return new RunnableReturn(k, new LetrecSource(args.Item1, args.Item2));
        }
    }

    [SchemeSingleton("xom-letrec*")]
    public class XomLetrecStarProc : IProcedure
    {
        public XomLetrecStarProc() { }

        public int Arity { get { return 1; } }

        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            Tuple<FList<LetClause>, IExpressionSource> args = ProxyDiscovery.ToLetClausesAndExpression(argList);
            return new RunnableReturn(k, new LetrecStarSource(args.Item1, args.Item2));
        }
    }

    [SchemeSingleton("xom-let-loop")]
    public class XomLetLoopProc : IProcedure
    {
        public XomLetLoopProc() { }

        public int Arity { get { return 2; } }

        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            Symbol loopName;
            if (argList != null)
            {
                if (argList.Head is Symbol)
                {
                    loopName = (Symbol)(argList.Head);
                    argList = argList.Tail;
                }
                else throw new SchemeRuntimeException("Expecting symbol");
            }
            else throw new SchemeRuntimeException("Expecting symbol");
            FList<LetClause> clauseList = null;
            while (argList != null)
            {
                if (argList.Head is LetClause)
                {
                    clauseList = new FList<LetClause>((LetClause)(argList.Head), clauseList);
                    argList = argList.Tail;
                }
                else if (argList.Head is IExpressionSource) break;
                else throw new SchemeRuntimeException("Expecting parameters of type LetClause followed by an IExpressionSource");
            }
            if (argList == null) throw new SchemeRuntimeException("Expecting IExpressionSource at end of parameters");
            IExpressionSource body = (IExpressionSource)(argList.Head);
            argList = argList.Tail;
            if (argList != null) throw new SchemeRuntimeException("Expecting IExpressionSource to be last parameter");
            return new RunnableReturn(k, new LetLoopSource(loopName, clauseList, body));
        }
    }

    [SchemeSingleton("xom-eval")]
    public class XomEvalProc : IProcedure
    {
        public XomEvalProc() { }

        public int Arity { get { return 2; } }

        public bool More { get { return false; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            IExpression expr;
            Environment env;
            if (argList == null) throw new SchemeRuntimeException("Expected expr");
            if (!(argList.Head is IExpression)) throw new SchemeRuntimeException("Expected expr to be an IExpression");
            expr = (IExpression)(argList.Head);
            argList = argList.Tail;
            if (argList == null) throw new SchemeRuntimeException("Expected env after expr");
            if (!(argList.Head is Environment)) throw new SchemeRuntimeException("Expected env to be an Environment");
            env = (Environment)(argList.Head);
            argList = argList.Tail;
            if (argList != null) throw new SchemeRuntimeException("Don't know what to do with extra parameters");

            return new RunnableEval(expr, env, k);
        }
    }

    public class PlaceholderSource : IExpressionSource
    {
        private IExpressionSource content;

        public PlaceholderSource()
        {
            content = null;
        }

        public IExpressionSource Content { get { return content; } set { content = value; } }

        #region IExpressionSource Members

        public EnvSpec GetRequirements()
        {
            if (content == null) throw new SchemeRuntimeException("Unset placeholder");
            return content.GetRequirements();
        }

        public IExpression Compile(EnvDesc ed)
        {
            if (content == null) throw new SchemeRuntimeException("Unset placeholder");
            return content.Compile(ed);
        }

        #endregion
    }
}