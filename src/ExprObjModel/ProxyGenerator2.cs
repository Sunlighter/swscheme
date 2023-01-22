using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Pascalesque;
using System.Runtime.Serialization;
using System.Reflection.Emit;

namespace ExprObjModel
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = true)]
    public class SchemeOverloadAttribute : Attribute
    {
        private string name;

        public SchemeOverloadAttribute(string name)
        {
            this.name = name;
        }

        public string Name { get { return name; } }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class SchemeOperatorAttribute : Attribute
    {
        private string name;

        public SchemeOperatorAttribute(string name)
        {
            this.name = name;
        }

        public string Name { get { return name; } }
    }

    public interface IProxyManager
    {
        IProcedure GetProcedure(Symbol s);
        IEnumerable<Tuple<Symbol, IProcedure>> Procedures { get; }
        void AddProxySurrogates(SurrogateSelector ss);
    }

    public abstract class ProxyDescription
    {
        private Symbol name;

        protected ProxyDescription(Symbol name)
        {
            this.name = name;
        }

        public Symbol Name { get { return name; } }

        public abstract ClassToBuild GenerateProxy(NameConverter nc, ModuleBuilder mb);
    }

    public class OverloadProxyDescription : ProxyDescription
    {
        private List<MethodBase> methods;

        public OverloadProxyDescription(Symbol name, MethodBase method)
            : base(name)
        {
            this.methods = new List<MethodBase>();
            this.methods.Add(method);
        }

        public ICountedEnumerable<MethodBase> Methods { get { return new CountedEnumerable<MethodBase>(methods, methods.Count); } }

        public void Add(MethodBase m)
        {
            methods.Add(m);
        }

        public override ClassToBuild GenerateProxy(NameConverter nc, ModuleBuilder mb)
        {
            throw new NotImplementedException();
        }
    }

    public class OperatorProxyDescription : ProxyDescription
    {
        private List<MethodInfo> methods;

        public OperatorProxyDescription(Symbol name, MethodInfo method)
            : base(name)
        {
            this.methods = new List<MethodInfo>();
            this.methods.Add(method);
        }

        public ICountedEnumerable<MethodInfo> Methods { get { return new CountedEnumerable<MethodInfo>(methods, methods.Count); } }

        public void Add(MethodInfo m)
        {
            methods.Add(m);
        }

        public override ClassToBuild GenerateProxy(NameConverter nc, ModuleBuilder mb)
        {
            throw new NotImplementedException();
        }
    }

    public class IsAProxyDescription : ProxyDescription
    {
        private Type type;

        public IsAProxyDescription(Symbol name, Type type)
            : base(name)
        {
            this.type = type;
        }

        public Type Type { get { return type; } }

        public override Pascalesque.IExpression GenerateProxy(NameConverter nc, ModuleBuilder mb)
        {
            string className = nc.ConvertName("IsA" + this.Name.ToString());

            System.Diagnostics.Trace.WriteLine("Generating Is-a: " + className);

            TypeBuilder bType = mb.DefineType
            (
                className,
                TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed,
                null,
                new Type[] { typeof(IProcedure) }
            );

            PropertyBuilder bArity = bType.DefineProperty("Arity", PropertyAttributes.None, typeof(int), null);

            PropertyBuilder bMore = bType.DefineProperty("More", PropertyAttributes.None, typeof(bool), null);

            MethodBuilder bArityGet = bType.DefineMethod
            (
                "get_Arity",
                MethodAttributes.Virtual | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                typeof(int),
                null
            );

            bArity.SetGetMethod(bArityGet);

            MethodBuilder bMoreGet = bType.DefineMethod
            (
                "get_More",
                MethodAttributes.Virtual | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                typeof(bool),
                null
            );

            bMore.SetGetMethod(bMoreGet);

            Pascalesque.IExpression callMethodBody = new IfThenElseExpr
            (
                new IsNullExpr
                (
                    new VarRefExpr(new Symbol("args"))
                ),
                new RegardAsClassExpr
                (
                    typeof(IRunnableStep),
                    new MethodCallExpr
                    (
                        typeof(RunnableThrow).GetConstructor(new Type[] { typeof(IContinuation), typeof(object) }),
                        new Pascalesque.IExpression[]
                        {
                            new VarRefExpr(new Symbol("k")),
                            new MethodCallExpr
                            (
                                typeof(SchemeRuntimeException).GetConstructor(new Type[] { typeof(string) }),
                                new Pascalesque.IExpression[]
                                {
                                    new LiteralExpr(this.Name.ToString() + ": insufficient arguments")
                                }
                            )
                        }
                    )
                ),
                new LetExpr
                (
                    new Pascalesque.LetClause[]
                    {
                        new Pascalesque.LetClause
                        (
                            new Symbol("obj"),
                            typeof(object),
                            new MethodCallExpr
                            (
                                typeof(FList<object>).GetMethod("get_Head", Type.EmptyTypes),
                                new Pascalesque.IExpression[]
                                {
                                    new VarRefExpr(new Symbol("args"))
                                }
                            )
                        ),
                        new Pascalesque.LetClause
                        (
                            new Symbol("args2"),
                            typeof(FList<object>),
                            new MethodCallExpr
                            (
                                typeof(FList<object>).GetMethod("get_Tail", Type.EmptyTypes),
                                new Pascalesque.IExpression[]
                                {
                                    new VarRefExpr(new Symbol("args"))
                                }
                            )
                        )
                    },
                    new IfThenElseExpr
                    (
                        new IsNullExpr
                        (
                            new VarRefExpr(new Symbol("args2"))
                        ),
                        new RegardAsClassExpr
                        (
                            typeof(IRunnableStep),
                            new MethodCallExpr
                            (
                                typeof(RunnableReturn).GetConstructor(new Type[] { typeof(IContinuation), typeof(object) }),
                                new Pascalesque.IExpression[]
                                {
                                    new VarRefExpr(new Symbol("k")),
                                    new BoxExpr
                                    (
                                        new IsInstanceExpr
                                        (
                                            new VarRefExpr(new Symbol("obj")),
                                            type
                                        )
                                    )
                                }
                            )
                        ),
                        new RegardAsClassExpr
                        (
                            typeof(IRunnableStep),
                            new MethodCallExpr
                            (
                                typeof(RunnableThrow).GetConstructor(new Type[] { typeof(IContinuation), typeof(object) }),
                                new Pascalesque.IExpression[]
                                {
                                    new VarRefExpr(new Symbol("k")),
                                    new MethodCallExpr
                                    (
                                        typeof(SchemeRuntimeException).GetConstructor(new Type[] { typeof(string) }),
                                        new Pascalesque.IExpression[]
                                        {
                                            new LiteralExpr(this.Name.ToString() + ": too many arguments")
                                        }
                                    )
                                }
                            )
                        )
                    )
                )
            );

            bType.DefineMethod
            (
                mb,
                "Call",
                MethodAttributes.Virtual | MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                typeof(IRunnableStep),
                new ParamInfo[]
                {
                    new ParamInfo(new Symbol("this"), bType),
                    new ParamInfo(new Symbol("gs"), typeof(IGlobalState)),
                    new ParamInfo(new Symbol("args"), typeof(FList<object>)),
                    new ParamInfo(new Symbol("k"), typeof(IContinuation))
                },
                callMethodBody
            );

#if false
            MethodBuilder bCall = bType.DefineMethod
            (
                "Call",
                MethodAttributes.Virtual | MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                typeof(IRunnableStep),
                new Type[]
                {
                    typeof(IGlobalState),
                    typeof(FList<object>),
                    typeof(IContinuation)
                }
            );

            string context = className + " (is-a)";
            CodeGenerator cg = new CodeGenerator(bCall.GetILGenerator(), context);
            LocalBuilder theList = cg.DeclareLocal(typeof(FList<object>));
            LocalBuilder theObj = cg.DeclareLocal(typeof(object));

            int GLOBAL_STATE_ARG = 1;
            int CONTINUATION_ARG = 3;
            int ARGS_ARG = 2;

            cg.LoadArg(CONTINUATION_ARG);

            cg.LoadArg(ARGS_ARG);
            cg.StoreLocal(theList);

            PopObjectOffList(cg, theList);
            cg.StoreLocal(theObj);
            VerifyListEmpty(cg, theList);

            cg.LoadLocal(theObj);
            if (typeof(IDisposable).IsAssignableFrom(t))
            {
                cg.IsInst(typeof(DisposableID));
                cg.IfNot(OpCodes.Brtrue_S);
                cg.LoadInt(0);
                cg.Else(false);
                cg.LoadArg(GLOBAL_STATE_ARG);
                cg.LoadLocal(theObj);
                cg.Unbox(typeof(DisposableID));
                cg.LoadObjIndirect(typeof(DisposableID));
                cg.CallVirt(typeof(IGlobalState).GetMethod("GetDisposableByID", new Type[] { typeof(DisposableID) }));
                cg.IsInst(t);
                cg.Then();
                cg.Box(typeof(bool));
                NewRunnableReturn(cg);
            }
            else
            {
                cg.IsInst(t);
                cg.Box(typeof(bool));
                NewRunnableReturn(cg);
            }
            cg.Return();
#endif

            ILGenerator ilgArityGet = bArityGet.GetILGenerator();
            ilgArityGet.LoadInt(1);
            ilgArityGet.Return();

            ILGenerator ilgMoreGet = bMoreGet.GetILGenerator();
            ilgMoreGet.LoadInt(0);
            ilgMoreGet.Return();

            Type[] constructorParams = new Type[] { };

            ConstructorBuilder bConstructor = bType.DefineConstructor
            (
                MethodAttributes.Public,
                CallingConventions.Standard,
                constructorParams
            );

            ILGenerator ilgConstructor = bConstructor.GetILGenerator();
            ilgConstructor.Return();

            //IProcedure proc = (IProcedure)(bType.CreateType().GetConstructor(constructorParams).Invoke(null));

            return new MethodCallExpr
            (
                bConstructor,
                new Pascalesque.IExpression[]
                {
                    // empty
                }
            );
        }
    }

    public class SingletonProxyDescription : ProxyDescription
    {
        private Type type;

        public SingletonProxyDescription(Symbol name, Type type)
            : base(name)
        {
            this.type = type;
        }

        public Type Type { get { return type; } }

        public override Pascalesque.IExpression GenerateProxy(NameConverter nc, ModuleBuilder mb)
        {
            throw new NotImplementedException();
        }
    }

    public static class ProxyGenerator2
    {
        public static void GetProxies(Assembly a, Dictionary<Symbol, ProxyDescription> dest)
        {
            foreach (Module m in a.GetModules())
            {
                foreach (Type t in m.GetTypes())
                {
                    if (t.IsDefined(typeof(SchemeIsAFunctionAttribute), false))
                    {
                        SchemeIsAFunctionAttribute[] arr = t.GetCustomAttributes<SchemeIsAFunctionAttribute>(false);
                        foreach (SchemeIsAFunctionAttribute aa in arr)
                        {
                            Symbol name = new Symbol(aa.Name);
                            if (dest.ContainsKey(name))
                            {
                                throw new ProxyException("Non-overloadable symbol " + SchemeDataWriter.ItemToString(name));
                            }
                            else
                            {
                                dest.Add(name, new IsAProxyDescription(name, t));
                            }
                        }
                    }

                    if (t.IsDefined(typeof(SchemeSingletonAttribute),false))
                    {
                        ConstructorInfo ci = t.GetConstructor(Type.EmptyTypes);
                        if (ci == null || ci.IsPrivate) throw new ProxyException("Singleton " + t.FullName + " has no public default constructor");
                        SchemeSingletonAttribute[] arr = t.GetCustomAttributes<SchemeSingletonAttribute>(false);
                        foreach (SchemeSingletonAttribute aa in arr)
                        {
                            Symbol name = new Symbol(aa.Name);
                            if (dest.ContainsKey(name))
                            {
                                throw new ProxyException("Non-overloadable symbol " + SchemeDataWriter.ItemToString(name));
                            }
                            else
                            {
                                dest.Add(name, new SingletonProxyDescription(name, t));
                            }
                        }
                    }

                    foreach (MethodBase mi in t.GetMethods().AsEnumerable<MethodBase>().Concat(t.GetConstructors().AsEnumerable<MethodBase>()))
                    {
                        if (mi.IsPrivate) continue;

                        if (mi.IsDefined(typeof(SchemeFunctionAttribute), false))
                        {
                            SchemeFunctionAttribute[] arr = mi.GetCustomAttributes<SchemeFunctionAttribute>(false);
                            foreach (SchemeFunctionAttribute aa in arr)
                            {
                                Symbol name = new Symbol(aa.Name);
                                if (dest.ContainsKey(name))
                                {
                                    if (dest[name] is OverloadProxyDescription)
                                    {
                                        OverloadProxyDescription opd = (OverloadProxyDescription)(dest[name]);
                                        opd.Add(mi);
                                    }
                                    else
                                    {
                                        throw new ProxyException("Non-overloadable symbol " + SchemeDataWriter.ItemToString(name));
                                    }
                                }
                                else
                                {
                                    dest.Add(name, new OverloadProxyDescription(name, mi));
                                }
                            }
                        }

                        if (mi is MethodInfo)
                        {
                            if (mi.IsDefined(typeof(SchemeOperatorAttribute), false))
                            {
                                SchemeOperatorAttribute[] arr = mi.GetCustomAttributes<SchemeOperatorAttribute>(false);
                                foreach (SchemeOperatorAttribute aa in arr)
                                {
                                    Symbol name = new Symbol(aa.Name);
                                    if (dest.ContainsKey(name))
                                    {
                                        if (dest[name] is OperatorProxyDescription)
                                        {
                                            OperatorProxyDescription opd = (OperatorProxyDescription)(dest[name]);
                                            opd.Add((MethodInfo)mi);
                                        }
                                        else
                                        {
                                            throw new ProxyException("Non-overloadable symbol " + SchemeDataWriter.ItemToString(name));
                                        }
                                    }
                                    else
                                    {
                                        dest.Add(name, new OperatorProxyDescription(name, (MethodInfo)mi));
                                    }
                                }
                            }
                        }
                    } // method
                } // type
            } // module
        }

        public static IProxyManager GenerateProxies(Dictionary<Symbol, ProxyDescription> descriptions)
        {
            Symbol assemblyNameSymbol = new Symbol();
            AssemblyName assemblyName = new AssemblyName(assemblyNameSymbol.ToString() + ".dll");
            AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            ModuleBuilder mb = ab.DefineDynamicModule(assemblyNameSymbol.ToString() + ".dll");

            NameConverter nc = new NameConverter();

            List<Tuple<Symbol, Pascalesque.IExpression>> generators = new List<Tuple<Symbol, Pascalesque.IExpression>>();

            foreach (KeyValuePair<Symbol, ProxyDescription> kvp in descriptions)
            {
                try
                {
                    Pascalesque.IExpression expr = kvp.Value.GenerateProxy(nc, mb);
                    generators.Add(new Tuple<Symbol, Pascalesque.IExpression>(kvp.Key, expr));
                }
                catch (NotImplementedException)
                {
                    // ignore it
                }
            }

            TypeBuilder t = mb.DefineType(nc.ConvertName("ProxyManager"), TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed, typeof(object), new Type[] { typeof(IProxyManager) });

            ConstructorBuilder cb = t.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, Type.EmptyTypes);
            ILGenerator cbilg = cb.GetILGenerator();
            cbilg.Return();

            //MethodBuilder mb2 = t.DefineMethod("GetProcedure", MethodAttributes.Public | MethodAttributes.Virtual, CallingConventions.HasThis);

            throw new NotImplementedException();
        }
    }
}
