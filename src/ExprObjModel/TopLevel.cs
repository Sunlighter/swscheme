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
using ExprObjModel.Procedures;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;
using ExprObjModel.SyntaxAnalysis;
using System.Linq;
using ControlledWindowLib.Scheduling;

namespace ExprObjModel
{
    [Serializable]
    public sealed class UndefinedVariableException : Exception
    {
        public UndefinedVariableException() { }
        public UndefinedVariableException(string message) : base(message) { }
        public UndefinedVariableException(string message, Exception inner) : base(message, inner) { }
    }

    public class TopLevelEnvironment
    {
        private Stack<Dictionary<Symbol, Box>> stack;
        private Dictionary<Symbol, Box> dict;

        public TopLevelEnvironment()
        {
            dict = new Dictionary<Symbol, Box>();
            stack = new Stack<Dictionary<Symbol, Box>>();
        }

        public void Define(Symbol sym, object value)
        {
            Box b = new Box();
            b.Contents = value;
            if (dict.ContainsKey(sym))
            {
                dict[sym] = b;
            }
            else
            {
                dict.Add(sym, b);
            }
        }

        public bool IsDefined(Symbol sym)
        {
            return dict.ContainsKey(sym);
        }

        public void Undefine(Symbol sym)
        {
            if (dict.ContainsKey(sym))
            {
                dict.Remove(sym);
            }
        }

        public void BeginModule()
        {
            stack.Push(dict.Clone());
        }

        public void EndModule(IEnumerable<Symbol> exports)
        {
            if (stack.Count == 0) throw new SchemeRuntimeException("Attempt to end a module without beginning one");

            Dictionary<Symbol, Box> oldDict = dict;
            dict = stack.Pop();
            foreach (Symbol export in exports)
            {
                if (dict.ContainsKey(export) && oldDict.ContainsKey(export))
                {
                    dict[export] = oldDict[export];
                }
                else if (dict.ContainsKey(export))
                {
                    dict.Remove(export);
                }
                else if (oldDict.ContainsKey(export))
                {
                    dict.Add(export, oldDict[export]);
                }
                // else do nothing
            }
        }

        public IEnumerable<Symbol> Keys { get { return dict.Keys; } }

        public void CreateEnvironment(EnvSpec spec, out EnvDesc envDesc, out Environment environment)
        {
            Symbol[] syms = spec.ToArray();
            EnvDesc.Empty.Extend(syms, out envDesc);
            int iend = spec.Count;
            Box[] boxes = new Box[iend];
            for (int i = 0; i < iend; ++i)
            {
                if (!dict.ContainsKey(syms[i])) throw new UndefinedVariableException(syms[i].ToString());
                boxes[i] = dict[syms[i]];
            }
            environment = Environment.Empty.Extend(boxes);
        }

        private bool ImportGeneratorMethod(ModuleBuilder mb, NameConverter nc, MethodBase mi)
        {
            SchemeFunctionGeneratorAttribute[] schemeFunctionGeneratorAttributes = mi.GetCustomAttributes<SchemeFunctionGeneratorAttribute>(false);

            if (schemeFunctionGeneratorAttributes.Length > 0)
            {
                Action<string> wrong = delegate(string err)
                {
                    Console.WriteLine("Error importing " + mi.DeclaringType.FullName + "." + mi.Name);
                    Console.WriteLine(err);
                };

                if (mi.DeclaringType.IsGenericTypeDefinition) { wrong("Method belongs to a generic type"); return false; }
                if (mi.IsGenericMethodDefinition) { wrong("Method is generic"); return false; }

                if (!(mi is MethodInfo)) { wrong("Method is a constructor"); return false; }

                MethodInfo mii = (MethodInfo)mi;

                if (mii.ReturnType != typeof(Dictionary<MethodBase, List<string>>)) { wrong("Method has wrong return type"); return false; }

                ParameterInfo[] pii = mi.GetParameters();

                if (pii.Length != 0) { wrong("Method requires parameters"); return false; }

                Dictionary<MethodBase, List<string>> d = (Dictionary<MethodBase, List<string>>)(mii.Invoke(null, null));

                foreach (KeyValuePair<MethodBase, List<string>> kvp in d)
                {
                    ImportAnyMethod(mb, nc, kvp.Key, kvp.Value);
                }
                return true;
            }
            else return false;
        }

        private void ImportMethod(ModuleBuilder mb, NameConverter nc, MethodBase mi)
        {
            if (ImportGeneratorMethod(mb, nc, mi)) return;

            SchemeFunctionAttribute[] schemeFunctionAttributes = mi.GetCustomAttributes<SchemeFunctionAttribute>(false);

            if (schemeFunctionAttributes.Length > 0)
            {
                List<string> schemeNames = new List<string>();
                foreach (SchemeFunctionAttribute sfa in schemeFunctionAttributes)
                {
                    schemeNames.Add(sfa.Name);
                }
                ImportAnyMethod(mb, nc, mi, schemeNames);
            }
        }

        private void ImportAnyMethod(ModuleBuilder mb, NameConverter nc, MethodBase mi, List<string> schemeNames)
        {
            IProcedure proc = null;
            try
            {
                proc = ProxyGenerator.GenerateProxy(mb, nc.ConvertName(schemeNames[0]), mi, schemeNames[0]);
            }
            catch (Exception exc)
            {
                Console.WriteLine("Error importing " + schemeNames[0]);
                Console.WriteLine(exc);
            }

            if (proc != null)
            {
                foreach (string str in schemeNames)
                {
                    Define(new Symbol(str), proc);
                }
            }
            else
            {
                Console.WriteLine("Failure importing " + schemeNames[0]);
            }
        }

        private void ImportSingleton(Type t)
        {
            SchemeSingletonAttribute[] schemeSingletonAttributes = t.GetCustomAttributes<SchemeSingletonAttribute>(false);

            if (schemeSingletonAttributes.Length > 0)
            {
                List<string> schemeNames = new List<string>();
                foreach (SchemeSingletonAttribute ssa in schemeSingletonAttributes)
                {
                    schemeNames.Add(ssa.Name);
                }
                IProcedure proc = null;
                try
                {
                    proc = ProxyGenerator.GenerateSingleton(schemeNames[0], t);
                }
                catch (Exception exc)
                {
                    Console.WriteLine("Error importing " + schemeNames[0]);
                    Console.WriteLine(exc);
                }

                if (proc != null)
                {
                    foreach (string str in schemeNames)
                    {
                        Define(new Symbol(str), proc);
                    }
                }
                else
                {
                    Console.WriteLine("Failure importing " + schemeNames[0]);
                }
            }
        }

        private void ImportIsAFunction(ModuleBuilder mb, NameConverter nc, Type t)
        {
            object[] schemeIsAFunctionAttributes = t.GetCustomAttributes(typeof(SchemeIsAFunctionAttribute), false);

            if (schemeIsAFunctionAttributes.Length > 0)
            {
                List<string> schemeNames = new List<string>();
                foreach (SchemeIsAFunctionAttribute sia in schemeIsAFunctionAttributes)
                {
                    schemeNames.Add(sia.Name);
                }
                IProcedure proc = null;
                try
                {
                    proc = ProxyGenerator.GenerateIsAFunction(mb, nc.ConvertName(t.Name), t);
                }
                catch (Exception exc)
                {
                    Console.WriteLine("Error creating is-a function " + schemeNames[0]);
                    Console.WriteLine(exc);
                }

                if (proc != null)
                {
                    foreach (string str in schemeNames)
                    {
                        Define(new Symbol(str), proc);
                    }
                }
                else
                {
                    Console.WriteLine("Failure importing " + schemeNames[0]);
                }
            }
        }

        public void ImportSchemeFunctions(Assembly a)
        {
            NameConverter nc = new NameConverter();
            string convertedName = nc.ConvertName(a.GetName().Name);
            AssemblyName tempAssemblyName = new AssemblyName(convertedName);
            //AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly(tempAssemblyName, AssemblyBuilderAccess.RunAndSave);
            //ModuleBuilder mb = ab.DefineDynamicModule(convertedName, convertedName + ".dll", true);
            AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly(tempAssemblyName, AssemblyBuilderAccess.Run/*AndSave*/);

            foreach (Module m in a.GetModules())
            {
                string convertedModuleName = nc.ConvertName(m.Name);
                ModuleBuilder mb = ab.DefineDynamicModule(convertedModuleName/*, convertedModuleName + ".dll", true*/);
                foreach (Type t in m.GetTypes())
                {
                    ImportSingleton(t);
                    ImportIsAFunction(mb, nc, t);

                    foreach (MethodInfo mi in t.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
                    {
                        ImportMethod(mb, nc, mi);
                    }

                    foreach (ConstructorInfo ci in t.GetConstructors())
                    {
                        ImportMethod(mb, nc, ci);
                    }
                }
            }

            // ab.Save(convertedName + ".dll", PortableExecutableKinds.ILOnly, ImageFileMachine.I386);
        }

#if false
        public IProxyManager ImportSchemeFunctions2(Assembly a)
        {
            Dictionary<Symbol, ProxyDescription> proxies = new Dictionary<Symbol, ProxyDescription>();
            ProxyGenerator2.GetProxies(a, proxies);
            IProxyManager pm = ProxyGenerator2.GenerateProxies(proxies);
            return pm;
        }
#endif
    }

    public class DoerResult
    {
        private object result;
        private bool exception;
        private int cycles;

        public DoerResult(object result, bool exception, int cycles)
        {
            this.result = result;
            this.exception = exception;
            this.cycles = cycles;
        }

        public object Result { get { return result; } }
        public bool IsException { get { return exception; } }
        public int Cycles { get { return cycles; } }
    }

    public static class Doer
    {
        [Obsolete]
        private class FinalRunnableStep : IRunnableStep
        {
            private object val;
            private bool wasException;

            public FinalRunnableStep(object val, bool wasException)
            {
                this.val = val;
                this.wasException = wasException;
            }

            public object Value { get { return val; } }
            public bool WasException { get { return wasException; } }

            public IRunnableStep Run(IGlobalState gs)
            {
                throw new Exception("Ran amok past final runnable step!");
            }
        }

        private class FinalRunnableStep2 : IRunnableStep
        {
            private SignalID sid;
            private object val;
            private bool wasException;

            public FinalRunnableStep2(SignalID sid, object val, bool wasException)
            {
                this.sid = sid;
                this.val = val;
                this.wasException = wasException;
            }

            public SignalID SignalID { get { return sid; } }
            public object Value { get { return val; } }
            public bool WasException { get { return wasException; } }

            public IRunnableStep Run(IGlobalState gs)
            {
                throw new Exception("Ran amok past final runnable step!");
            }
        }

        [Obsolete]
        [Serializable]
        private class FinalContinuation : IContinuation
        {
            private FinalContinuation() { }
            private static FinalContinuation instance = new FinalContinuation();
            public static FinalContinuation Instance { get { return instance; } }

            public IRunnableStep Return(IGlobalState gs, object v)
            {
                return new FinalRunnableStep(v, false);
            }

            public IRunnableStep Throw(IGlobalState gs, object exc)
            {
                return new FinalRunnableStep(exc, true);
            }

            public IContinuation Parent { get { return null; } }
            public IProcedure EntryProc { get { return null; } }
            public IProcedure ExitProc { get { return null; } }

            public IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
            {
                throw new SchemeRuntimeException("PartialCapture ran amok past FinalContinuation");
            }

            public Box DynamicLookup(Symbol s)
            {
                return null;
            }

            public EnvSpec DynamicEnv
            {
                get { return EnvSpec.EmptySet; }
            }
        }

        private class FinalContinuation2 : IContinuation
        {
            private SignalID sid;

            public FinalContinuation2(SignalID sid)
            {
                this.sid = sid;
            }

            public IRunnableStep Return(IGlobalState gs, object v)
            {
                return new FinalRunnableStep2(sid, v, false);
            }

            public IRunnableStep Throw(IGlobalState gs, object exc)
            {
                return new FinalRunnableStep2(sid, exc, true);
            }

            public IContinuation Parent { get { return null; } }
            public IProcedure EntryProc { get { return null; } }
            public IProcedure ExitProc { get { return null; } }

            public IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
            {
                throw new SchemeRuntimeException("PartialCapture ran amok past FinalContinuation");
            }

            public Box DynamicLookup(Symbol s)
            {
                return null;
            }

            public EnvSpec DynamicEnv
            {
                get { return EnvSpec.EmptySet; }
            }
        }

        [Obsolete]
        private static DoerResult RunToConclusion(IGlobalState gs, IRunnableStep r)
        {
            int cycles = 0;
            while (true)
            {
                //Console.WriteLine(r);
                if (r is FinalRunnableStep) break;
                r = r.Run(gs);
                ++cycles;
            }
            FinalRunnableStep frs = (FinalRunnableStep)r;
            return new DoerResult(frs.Value, frs.WasException, cycles);
        }

        [Obsolete]
        public static DoerResult Eval(IGlobalState gs, IExpression expr, Environment env)
        {
            return RunToConclusion(gs, new RunnableEval(expr, env, FinalContinuation.Instance));
        }

        [Obsolete]
        public static DoerResult Apply(IGlobalState gs, IProcedure proc, FList<object> argList)
        {
            return RunToConclusion(gs, new RunnableCall(proc, argList, FinalContinuation.Instance));
        }

        [Obsolete]
        public static DoerResult Apply(IGlobalState gs, ExprObjModel.ObjectSystem.IMsgProcedure msgProc, ExprObjModel.ObjectSystem.Message<object> msg)
        {
            return RunToConclusion(gs, new ExprObjModel.ObjectSystem.RunnableMsgCall(msgProc, msg, FinalContinuation.Instance));
        }

#if false
        [Obsolete]
        public static DoerResult ApplyToStrings(IGlobalState gs, IProcedure proc, string[] args)
        {
            FList<object> argList = null;
            int i = args.Length;
            while (i > 0)
            {
                --i;
                argList = new FList<object>(new SchemeString(args[i]), argList);
            }
            if ((proc.Arity == args.Length) || (proc.More && (proc.Arity < args.Length)))
            {
                return Apply(gs, proc, argList);
            }
            else
            {
                return new DoerResult(new SchemeRuntimeException("Incorrect number of arguments"), true, 0);
            }
        }
#endif

        public static DoerResult ApplyToStrings(IGlobalState gs, IProcedure proc, string[] args)
        {
            Tuple<SignalID, IContinuation> tk = CreateThread(gs);
            PostApplyToStrings(gs, proc, args, tk.Item2);
            Tuple<SignalID, object, bool> result = gs.Scheduler.BlockingWaitAny(Utils.SingleItem(tk.Item1));
            return new DoerResult(result.Item2, result.Item3, 0);
        }

        private static void RunToCompletion2(IGlobalState gs, IRunnableStep r)
        {
            //int cycles = 0;
            while (true)
            {
                //Console.WriteLine(r);
                if (r is FinalRunnableStep2)
                {
                    FinalRunnableStep2 frs2 = (FinalRunnableStep2)r;
                    gs.Scheduler.PostSignal(frs2.SignalID, frs2.Value, frs2.WasException);
                    return;
                }
                else if (object.ReferenceEquals(r, null))
                {
                    return;
                }
                else
                {
                    r = r.Run(gs);
                    //++cycles;
                }
            }
        }

        public static Tuple<SignalID, IContinuation> CreateThread(IGlobalState gs)
        {
            SignalID sid = gs.Scheduler.GetNewSignalID();
            FinalContinuation2 k = new FinalContinuation2(sid);
            return new Tuple<SignalID, IContinuation>(sid, k);
        }

        public static IContinuation CreateThreadNoReturn(IGlobalState gs)
        {
            return FinalContinuation.Instance;
        }

        public static void PostEval(IGlobalState gs, IExpression expr, Environment env, IContinuation k)
        {
            gs.Scheduler.PostAction
            (
                delegate()
                {
                    RunToCompletion2(gs, new RunnableEval(expr, env, k));
                }
            );
        }

        public static void PostApply(IGlobalState gs, IProcedure proc, FList<object> argList, IContinuation k)
        {
            gs.Scheduler.PostAction
            (
                delegate()
                {
                    RunToCompletion2(gs, new RunnableCall(proc, argList, k));
                }
            );
        }

        public static void PostApplyToStrings(IGlobalState gs, IProcedure proc, string[] args, IContinuation k)
        {
            gs.Scheduler.PostAction
            (
                delegate()
                {
                    FList<object> argList = null;
                    int i = args.Length;
                    while (i > 0)
                    {
                        --i;
                        argList = new FList<object>(new SchemeString(args[i]), argList);
                    }
                    if ((proc.Arity == args.Length) || (proc.More && (proc.Arity < args.Length)))
                    {
                        RunToCompletion2(gs, new RunnableCall(proc, argList, k));
                    }
                    else
                    {
                        RunToCompletion2(gs, new RunnableThrow(k, new SchemeRuntimeException("Incorrect number of arguments")));
                    }
                }
            );
        }

        public static void PostReturn(IGlobalState gs, IContinuation k, object val)
        {
            gs.Scheduler.PostAction
            (
                delegate()
                {
                    RunToCompletion2(gs, new RunnableReturn(k, val));
                }
            );
        }

        public static void PostThrow(IGlobalState gs, IContinuation k, object exc)
        {
            gs.Scheduler.PostAction
            (
                delegate()
                {
                    RunToCompletion2(gs, new RunnableThrow(k, exc));
                }
            );
        }
    }

    public class TextReaderStringSource : IStringSource, IDisposable
    {
        private TextReader tr;
        private string current;

        public TextReaderStringSource(TextReader tr)
        {
            this.tr = tr;
        }

        public void Dispose()
        {
            // do nothing
        }

        public bool Next(int parenDepth)
        {
            current = tr.ReadLine();
            return (current != null);
        }

        public string Current
        {
            get { return current; }
        }
    }

    public class FileStringSource : IStringSource, IDisposable
    {
        private StreamReader sr;
        private string current;

        public FileStringSource(string filename)
        {
            sr = new StreamReader(filename, System.Text.Encoding.UTF8);
            current = null;
        }

        public void Close()
        {
            if (sr != null)
            {
                sr.Close();
                sr.Dispose();
                sr = null;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (sr != null)
            {
                sr.Dispose();
                sr = null;
            }
        }

        #endregion

        #region IStringSource Members

        public bool Next(int parenDepth)
        {
            current = sr.ReadLine();
            return (current != null);
        }

        public string Current
        {
            get { return current; }
        }

        #endregion
    }

    public class TopLevel
    {
        private IGlobalState gs;
        private TopLevelEnvironment topEnv;

        public TopLevel(IGlobalState gs, bool loadRtl)
        {
            this.gs = gs;

            topEnv = new TopLevelEnvironment();

            topEnv.Define(new Symbol("wraparound"), BigMath.OverflowBehavior.Wraparound);
            topEnv.Define(new Symbol("saturate"), BigMath.OverflowBehavior.Saturate);
            topEnv.Define(new Symbol("hbla"), BigMath.DigitOrder.HBLA);
            topEnv.Define(new Symbol("lbla"), BigMath.DigitOrder.LBLA);
            topEnv.Define(new Symbol("fsaturate"), BigMath.FloatingOverflowBehavior.SaturateToInfinity);
            topEnv.Define(new Symbol("apropos-list"), new AproposListProc(this));
            topEnv.Define(new Symbol("eval"), new EvalProc());
            topEnv.Define(new Symbol("pi"), Math.PI);
            topEnv.Define(new Symbol("e"), Math.E);
            topEnv.Define(new Symbol("convex-hull-empty"), new CH_Empty());
            topEnv.Define(new Symbol("$$this"), this);
            topEnv.Define(new Symbol("current-top-level"), this);

            topEnv.ImportSchemeFunctions(Assembly.GetExecutingAssembly());

            if (loadRtl)
            {
#if false
                Func<IGlobalState, object> gmrs = delegate(IGlobalState gs2)
                {
                    Stream str = typeof(TopLevel).Assembly.GetManifestResourceStream("ExprObjModel.rtl.scm");
                    return gs2.RegisterDisposable(str, "RTL Manifest Resource Stream");
                };

                IProcedure procGMRS = Utils.CreateProcedure("create-manifest-resource-stream", gmrs);
                AssemblyName a = new AssemblyName("toplevelparts.dll");
                AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly(a, AssemblyBuilderAccess.RunAndCollect);
                ModuleBuilder mb = ab.DefineDynamicModule("toplevelparts.dll");

                Func<Stream> getManifestResourceStream = delegate()
                {
                    return typeof(TopLevel).Assembly.GetManifestResourceStream("ExprObjModel.rtl.scm");
                };

                IProcedure procGMRS = ProxyGenerator.GenerateProxyFromDelegate(mb, "FuncStream", "FuncStreamFactory", getManifestResourceStream);

                Func<Stream, StreamReader> newStreamReader = delegate(Stream stream)
                {
                    return new StreamReader(stream);
                };

                IProcedure procNSR = ProxyGenerator.GenerateProxyFromDelegate(mb, "FuncStreamReader", "FuncStreamReaderFactory", newStreamReader);

                Tuple<IExpression, Environment> ex = Utils.Compile
                (
                    new VarSpec[]
                    {
                        new VarSpec(new Symbol("gmrs"), procGMRS),
                        new VarSpec(new Symbol("nsr"), procNSR),
                    },
                    new UsingStarSource
                    (
                        FList<LetClause>.Create
                        (
                            new LetClause[]
                            {
                                new LetClause
                                (
                                    new Symbol("stream"),
                                    new InvocationSource
                                    (
                                        FList<IExpressionSource>.Create
                                        (
                                            new IExpressionSource[]
                                            {
                                                new VarRefSource(new Symbol("gmrs"))
                                            }
                                        )
                                    )
                                ),
                                new LetClause
                                (
                                    new Symbol("streamReader"),
                                    new InvocationSource
                                    (
                                        FList<IExpressionSource>.Create
                                        (
                                            new IExpressionSource[]
                                            {
                                                new VarRefSource(new Symbol("nsr")),
                                                new VarRefSource(new Symbol("stream"))
                                            }
                                        )
                                    )
                                )
                            }
                        ),
#endif
                
                using (Stream stream = typeof(TopLevel).Assembly.GetManifestResourceStream("ExprObjModel.rtl.scm"))
                {
                    using (StreamReader streamReader = new StreamReader(stream))
                    {
                        using (TextReaderStringSource trss = new TextReaderStringSource(streamReader))
                        {
                            SchemeDataReader sdr = new SchemeDataReader(new LexemeSource(trss));

                            while (true)
                            {
                                object obj = sdr.ReadItem();
                                if (obj == null) break;
                                Tuple<SignalID, IContinuation> th = Doer.CreateThread(gs);
                                PostEval(obj, th.Item2);
                                Tuple<SignalID, object, bool> result = gs.Scheduler.BlockingWaitAny(new SignalID[] { th.Item1 });
                                if (result.Item3)
                                {
                                    if (result.Item2 is Exception) throw (Exception)(result.Item2);
                                    else
                                    {
                                        throw new SchemeRuntimeException("Value Exception: " + SchemeDataWriter.ItemToString(result.Item2));
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (Symbol s in topEnv.Keys.Where(x => x.IsInterned && x.Name.StartsWith("$$")).ToList())
                {
                    topEnv.Undefine(s);
                }
            }

            try
            {
                //IProxyManager pm = topEnv.ImportSchemeFunctions2(Assembly.GetExecutingAssembly());
            }
            catch (NotImplementedException)
            {
                // do nothing
            }
        }

        private static IPattern pDefine = PatternBuilder.BuildPattern("(define <var> <expr>)");
        private static IPattern pUndefine = PatternBuilder.BuildPattern("(undefine <var>)");
        private static IPattern pLoad = PatternBuilder.BuildPattern("(load <filespec>)");
        private static IPattern pLoadInteractive = PatternBuilder.BuildPattern("(load-interactive)");
        private static IPattern pLoadLibrary = PatternBuilder.BuildPattern("(load-library <filespec>)");
        private static IPattern pBeginModule = PatternBuilder.BuildPattern("(begin-module)");
        private static IPattern pEndModule = PatternBuilder.BuildPattern("(end-module . <exports>)");

        private class DefinePartialContinuation : IPartialContinuation
        {
            private TopLevel parent;
            private Symbol sVar;
            private IPartialContinuation k;

            public DefinePartialContinuation(TopLevel parent, Symbol sVar, IPartialContinuation k)
            {
                this.parent = parent;
                this.sVar = sVar;
                this.k = k;
            }

            #region IPartialContinuation Members

            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<DefinePartialContinuation, DefineContinuation>(this, delegate() { return new DefineContinuation(parent, sVar, k.Attach(theNewBase, a)); });
            }

            #endregion
        }

        private class DefineContinuation : IContinuation
        {
            private TopLevel parent;
            private Symbol sVar;
            private IContinuation k;

            public DefineContinuation(TopLevel parent, Symbol sVar, IContinuation k)
            {
                this.parent = parent;
                this.sVar = sVar;
                this.k = k;
            }

            #region IContinuation Members

            public IRunnableStep Return(IGlobalState gs, object v)
            {
                parent.topEnv.Define(sVar, v);
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
                return a.Assoc<DefineContinuation, DefinePartialContinuation>(this, delegate() { return new DefinePartialContinuation(parent, sVar, k.PartialCapture(baseMark, a)); });
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }
            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }

            #endregion
        }

        private void PostDefineRec(object var, object expr, IContinuation k)
        {
            if (!(var is Symbol))
            {
                throw new SchemeSyntaxException("Ill-formed define");
            }
            Symbol sVar = (Symbol)var;

            IExpressionSource es = SyntaxAnalyzer.Analyze(expr);
            es = new LetrecSource(new FList<LetClause>(new LetClause(sVar, es)), new VarRefSource(sVar));

            EnvSpec envSpec = es.GetRequirements();
            Environment env;
            EnvDesc envDesc;
            topEnv.CreateEnvironment(envSpec, out envDesc, out env);
            IExpression exp = es.Compile(envDesc);

            Doer.PostEval(gs, exp, env, new DefineContinuation(this, sVar, k));
        }

        [Obsolete]
        private DoerResult DefineRec(object var, object expr)
        {
            if (!(var is Symbol))
            {
                throw new SchemeSyntaxException("Ill-formed define");
            }
            Symbol sVar = (Symbol)var;

            IExpressionSource es = SyntaxAnalyzer.Analyze(expr);
            es = new LetrecSource(new FList<LetClause>(new LetClause(sVar, es)), new VarRefSource(sVar));

            EnvSpec envSpec = es.GetRequirements();
            Environment env;
            EnvDesc envDesc;
            topEnv.CreateEnvironment(envSpec, out envDesc, out env);
            IExpression exp = es.Compile(envDesc);

            DoerResult result = Doer.Eval(gs, exp, env);
            if (!result.IsException)
            {
                topEnv.Define((Symbol)var, result.Result);
                return new DoerResult(SpecialValue.UNSPECIFIED, false, result.Cycles + 1);
            }
            else return result;
        }

        [Obsolete]
        private DoerResult Define(object var, object expr)
        {
            if (!(var is Symbol))
            {
                throw new SchemeSyntaxException("Ill-formed define");
            }
            DoerResult result = EvalExpression(expr);
            if (!result.IsException)
            {
                topEnv.Define((Symbol)var, result.Result);
                return new DoerResult(SpecialValue.UNSPECIFIED, false, result.Cycles + 1);
            }
            else return result;
        }

        private DoerResult Undefine(object var)
        {
            if (!(var is Symbol))
            {
                throw new SchemeSyntaxException("Ill-formed undefine");
            }
            else
            {
                topEnv.Undefine((Symbol)var);
                return new DoerResult(SpecialValue.UNSPECIFIED, false, 0);
            }
        }

        private static string ConvertSchemeString(object inString, string command)
        {
            string outString = null;
            if (inString is string)
            {
                outString = (string)inString;
            }
            else if (inString is SchemeString)
            {
                outString = ((SchemeString)inString).TheString;
            }
            else
            {
                throw new SchemeSyntaxException("Ill-formed " + command);
            }
            return outString;
        }

        private void PostLoad(object filespec, IContinuation k)
        {
            string properFilespec = ConvertSchemeString(filespec, "load");

            using (FileStringSource fss = new FileStringSource(properFilespec))
            {
                LexemeSource ls = new LexemeSource(fss);
                SchemeDataReader sdr = new SchemeDataReader(ls);

                ls.Next();
                while (true)
                {
                    while (ls.Current.type == LexemeType.Whitespace) ls.Next();
                    if (ls.Current.type == LexemeType.EndOfInput) break;
                    object obj = sdr.ReadItem();
                    try
                    {
                        Tuple<SignalID, IContinuation> t = Doer.CreateThread(gs);
                        PostEval(obj, t.Item2);
                        Tuple<SignalID, object, bool> result = gs.Scheduler.BlockingWaitAny(new SignalID[] { t.Item1 });

                        if (result.Item3)
                        {
                            Doer.PostThrow(gs, k, result.Item2);
                            return;
                        }
                    }
                    catch (Exception exc)
                    {
                        Doer.PostThrow(gs, k, exc);
                        return;
                    }
                }
                Doer.PostReturn(gs, k, true);
            }
        }

        [Obsolete]
        private DoerResult Load(object filespec)
        {
            string properFilespec = ConvertSchemeString(filespec, "load");

            using (FileStringSource fss = new FileStringSource(properFilespec))
            {
                LexemeSource ls = new LexemeSource(fss);
                SchemeDataReader sdr = new SchemeDataReader(ls);
                int count = 0;
                bool success = true;

                ls.Next();
                while (true)
                {
                    while (ls.Current.type == LexemeType.Whitespace) ls.Next();
                    if (ls.Current.type == LexemeType.EndOfInput) break;
                    object obj = sdr.ReadItem();
                    try
                    {
                        DoerResult result = Eval(obj);
                        count += result.Cycles;
                        if (result.IsException)
                        {
                            success = false;
                            if (result.Result is Exception)
                            {
                                throw ((Exception)result.Result);
                            }
                            else
                            {
                                success = false;
                                Console.WriteLine("Exception loading " + SchemeDataWriter.ItemToString(filespec) + ": " + SchemeDataWriter.ItemToString(result.Result));
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        success = false;
                        Console.WriteLine("Exception loading " + SchemeDataWriter.ItemToString(filespec) + ": " + exc.Message);
                        break;
                    }
                }
                return new DoerResult(success, false, count);
            }
        }

        private void PostLoadLibrary(object filespec, IContinuation k)
        {
            string properFilespec = ConvertSchemeString(filespec, "load-library");

            if (!Path.IsPathRooted(properFilespec))
            {
                properFilespec = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + "\\" + properFilespec;
            }
            Assembly a = System.Reflection.Assembly.LoadFile(properFilespec);
            topEnv.ImportSchemeFunctions(a);

            Doer.PostReturn(gs, k, true);
        }

        [Obsolete]
        private DoerResult LoadLibrary(object filespec)
        {
            string properFilespec = ConvertSchemeString(filespec, "load-library");

            if (!Path.IsPathRooted(properFilespec))
            {
                properFilespec = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + "\\" + properFilespec;
            }
            Assembly a = System.Reflection.Assembly.LoadFile(properFilespec);
            topEnv.ImportSchemeFunctions(a);

            return new DoerResult(true, false, 0);
        }

        public void Define(Symbol var, object value)
        {
            topEnv.Define(var, value);
        }

        public void Undefine(Symbol var)
        {
            topEnv.Undefine(var);
        }

        public void BeginModule()
        {
            topEnv.BeginModule();
        }

        public void EndModule(IEnumerable<Symbol> exports)
        {
            topEnv.EndModule(exports);
        }

        private void PostEvalExpression(object expr, IContinuation k)
        {
            IExpressionSource es = SyntaxAnalyzer.Analyze(expr);
            EnvSpec envSpec = es.GetRequirements();
            Environment env;
            EnvDesc envDesc;
            topEnv.CreateEnvironment(envSpec, out envDesc, out env);
            IExpression exp = es.Compile(envDesc);
            Doer.PostEval(gs, exp, env, k);
        }

        [Obsolete]
        private DoerResult EvalExpression(object expr)
        {
            IExpressionSource es = SyntaxAnalyzer.Analyze(expr);
            EnvSpec envSpec = es.GetRequirements();
            Environment env;
            EnvDesc envDesc;
            topEnv.CreateEnvironment(envSpec, out envDesc, out env);
            IExpression exp = es.Compile(envDesc);
            return Doer.Eval(gs, exp, env);
        }

        public void PostEval(object expr, IContinuation k)
        {
            if (expr is ConsCell)
            {
                MatchCaptureSet m = null;

                m = pDefine.Match(expr);
                if (m != null)
                {
                    PostDefineRec(m["<var>"], m["<expr>"], k);
                    return;
                }

                m = pUndefine.Match(expr);
                if (m != null)
                {
                    Undefine(m["<var>"]);
                    Doer.PostReturn(gs, k, SpecialValue.UNSPECIFIED);
                    return;
                }

                m = pLoad.Match(expr);
                if (m != null)
                {
                    PostLoad(m["<filespec>"], k);
                    return;
                }

                m = pLoadLibrary.Match(expr);
                if (m != null)
                {
                    PostLoadLibrary(m["<filespec>"], k);
                    return;
                }

                m = pLoadInteractive.Match(expr);
                if (m != null)
                {
                    System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
                    ofd.CheckFileExists = true;
                    ofd.InitialDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory);
                    ofd.Filter = "Scheme Files (*.scm)|*.scm|Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                    System.Windows.Forms.DialogResult dr = ofd.ShowDialog();
                    if (dr != System.Windows.Forms.DialogResult.OK)
                    {
                        Doer.PostReturn(gs, k, false);
                        return;
                    }
                    else
                    {
                        PostLoad(ofd.FileName, k);
                        return;
                    }
                }

                m = pBeginModule.Match(expr);
                if (m != null)
                {
                    BeginModule();
                    Doer.PostReturn(gs, k, SpecialValue.UNSPECIFIED);
                    return;
                }

                m = pEndModule.Match(expr);
                if (m != null)
                {
                    List<object> s = ConsCell.Enumerate(m["<exports>"]).ToList();
                    if (s.Any(x => !(x is Symbol))) throw new SchemeRuntimeException("end-module: exports must be symbols");
                    List<Symbol> s2 = s.Cast<Symbol>().ToList();
                    if (Pascalesque.ExtMethods.HasDuplicates<Symbol>(s2)) throw new SchemeRuntimeException("end-module: symbols must not be repeated");
                    EndModule(s2);
                    Doer.PostReturn(gs, k, SpecialValue.UNSPECIFIED);
                    return;
                }
            }

            PostEvalExpression(expr, k);
        }

        [Obsolete]
        public DoerResult Eval(object expr)
        {
            if (expr is ConsCell)
            {
                MatchCaptureSet m = null;

                m = pDefine.Match(expr);
                if (m != null)
                {
                    return DefineRec(m["<var>"], m["<expr>"]);
                }

                m = pUndefine.Match(expr);
                if (m != null)
                {
                    return Undefine(m["<var>"]);
                }

                m = pLoad.Match(expr);
                if (m != null)
                {
                    return Load(m["<filespec>"]);
                }

                m = pLoadLibrary.Match(expr);
                if (m != null)
                {
                    return LoadLibrary(m["<filespec>"]);
                }

                m = pLoadInteractive.Match(expr);
                if (m != null)
                {
                    System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
                    ofd.CheckFileExists = true;
                    ofd.InitialDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory);
                    ofd.Filter = "Scheme Files (*.scm)|*.scm|Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                    System.Windows.Forms.DialogResult dr = ofd.ShowDialog();
                    if (dr != System.Windows.Forms.DialogResult.OK)
                    {
                        return new DoerResult(false, true, 1);
                    }
                    else
                    {
                        return Load(ofd.FileName);
                    }
                }

                m = pBeginModule.Match(expr);
                if (m != null)
                {
                    BeginModule();
                    return new DoerResult(SpecialValue.UNSPECIFIED, false, 1);
                }

                m = pEndModule.Match(expr);
                if (m != null)
                {
                    List<object> s = ConsCell.Enumerate(m["<exports>"]).ToList();
                    if (s.Any(x => !(x is Symbol))) throw new SchemeRuntimeException("end-module: exports must be symbols");
                    List<Symbol> s2 = s.Cast<Symbol>().ToList();
                    if (Pascalesque.ExtMethods.HasDuplicates<Symbol>(s2)) throw new SchemeRuntimeException("end-module: symbols must not be repeated");
                    EndModule(s2);
                    return new DoerResult(SpecialValue.UNSPECIFIED, false, 1);
                }
            }

            return EvalExpression(expr);
        }

        private class AproposListProc : IProcedure
        {
            private TopLevel parent;

            public AproposListProc(TopLevel parent)
            {
                this.parent = parent;
            }

            #region IProcedure Members

            public int Arity
            {
                get { return 0; }
            }

            public bool More
            {
                get { return false; }
            }

            public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
            {
                object result = SpecialValue.EMPTY_LIST;

                List<Symbol> l = new List<Symbol>();
                foreach(Symbol s in parent.topEnv.Keys) { l.Add(s); }

                Comparison<Symbol> sortFunc = delegate(Symbol x, Symbol y) { return (x < y) ? 1 : (x > y) ? -1 : 0; };

                l.Sort(sortFunc);

                foreach (Symbol s in l)
                {
                    result = new ConsCell(s, result);
                }

                return new RunnableReturn(k, result);
            }

            #endregion
        }

        private class DefineIntoPartialContinuation : IPartialContinuation
        {
            private TopLevel topLevel;
            private Symbol var;
            private IPartialContinuation k;

            public DefineIntoPartialContinuation(TopLevel topLevel, Symbol var, IPartialContinuation k)
            {
                this.topLevel = topLevel;
                this.var = var;
                this.k = k;
            }

            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<DefineIntoPartialContinuation, DefineIntoContinuation>(this, delegate() { return new DefineIntoContinuation(topLevel, var, k.Attach(theNewBase, a)); });
            }
        }

        private class DefineIntoContinuation : IContinuation
        {
            private TopLevel topLevel;
            private Symbol var;
            private IContinuation k;

            public DefineIntoContinuation(TopLevel topLevel, Symbol var, IContinuation k)
            {
                this.topLevel = topLevel;
                this.var = var;
                this.k = k;
            }

            public IRunnableStep Return(IGlobalState gs, object v)
            {
                topLevel.Define(var, v);
                return new RunnableReturn(k, SpecialValue.UNSPECIFIED);
            }

            public IRunnableStep Throw(IGlobalState gs, object exc) { return new RunnableThrow(k, exc); }

            public IContinuation Parent { get { return k; } }
            public IProcedure EntryProc { get { return null; } }
            public IProcedure ExitProc { get { return null; } }

            public IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
            {
                return a.Assoc<DefineIntoContinuation, DefineIntoPartialContinuation>(this, delegate() { return new DefineIntoPartialContinuation(topLevel, var, k.PartialCapture(baseMark, a)); });
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }

            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }
        }

        private static IRunnableStep CompileAndEval(object expr, TopLevel tl, IContinuation k)
        {
            IExpressionSource es = SyntaxAnalyzer.Analyze(expr);
            EnvSpec envSpec = es.GetRequirements();
            Environment env;
            EnvDesc envDesc;
            tl.topEnv.CreateEnvironment(envSpec, out envDesc, out env);
            IExpression exp = es.Compile(envDesc);

            return new RunnableEval(exp, env, k);
        }

        private class EvalProc : IProcedure
        {
            public EvalProc()
            {
            }

            public int Arity { get { return 2; } }
            public bool More { get { return false; } }

            public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
            {
                if (argList == null || argList.Tail == null || argList.Tail.Tail != null) throw new SchemeRuntimeException("eval: wrong number of parameters");
                if (argList.Head as TopLevel == null) throw new SchemeRuntimeException("eval: first arg must be a top-level");
                
                TopLevel topLevel = (TopLevel)argList.Head;
                object expr = argList.Tail.Head;

                if (expr is ConsCell)
                {
                    MatchCaptureSet m = null;

                    m = pDefine.Match(expr);
                    if (m != null)
                    {
                        object var = m["<var>"];
                        object expr2 = m["<expr>"];
                        if (var is Symbol)
                        {
                            return CompileAndEval(expr2, topLevel, new DefineIntoContinuation(topLevel, (Symbol)var, k));
                        }
                    }
                }
                return CompileAndEval(expr, topLevel, k);
            }
        }
    }
}
