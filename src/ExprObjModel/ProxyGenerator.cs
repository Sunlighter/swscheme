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
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using ExprObjModel.CodeGeneration;
using BigMath;

namespace ExprObjModel
{
    [Serializable]
    public class ProxyException : Exception
    {
        public ProxyException() { }
        public ProxyException(string message) : base(message) { }
        public ProxyException(string message, Exception inner) : base(message, inner) { }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = true)]
    public class SchemeFunctionAttribute : Attribute
    {
        private string name;

        public SchemeFunctionAttribute(string name)
        {
            this.name = name;
        }

        public string Name { get { return name; } }
    }

    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Class, AllowMultiple = true)]
    public class SchemeSingletonAttribute : Attribute
    {
        private string name;

        public SchemeSingletonAttribute(string name)
        {
            this.name = name;
        }

        public string Name { get { return name; } }
    }

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = true)]
    public class SchemeIsAFunctionAttribute : Attribute
    {
        private string name;

        public SchemeIsAFunctionAttribute(string name)
        {
            this.name = name;
        }

        public string Name { get { return name; } }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SchemeFunctionGeneratorAttribute : Attribute
    {
        public SchemeFunctionGeneratorAttribute() { }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = false)]
    public class SchemeDisposableAttribute : Attribute
    {
        private string descr;

        public SchemeDisposableAttribute(string descr)
        {
            this.descr = descr;
        }

        public string Description { get { return descr; } }
    }

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class OverflowModeAttribute : Attribute
    {
        private BigMath.OverflowBehavior beh;

        public OverflowModeAttribute(BigMath.OverflowBehavior behavior)
        {
            this.beh = behavior;
        }

        public BigMath.OverflowBehavior Behavior { get { return beh; } }
    }

    public class NameConverter
    {
        Random r;

        public NameConverter()
        {
            r = new Random();
        }

        public string ConvertName(string name)
        {
            System.Text.StringBuilder result = new System.Text.StringBuilder();
            bool first = true;
            foreach (char ch in name)
            {
                if (first && char.IsLetter(ch) || ((!first) && char.IsLetterOrDigit(ch)))
                {
                    result.Append(ch);
                }
                else
                {
                    result.Append('_');
                    result.Append(Convert.ToInt32(ch).ToString("X"));
                    //result.Append('_');
                }
                first = false;
            }
            result.Append('_');
            for (int i = 0; i < 16; ++i)
            {
                result.AppendFormat(r.Next(16).ToString("X"));
            }
            return result.ToString();
        }
    }

    public interface IProxyProcedure : IProcedure
    {
        MethodBase WrappedMethod { get; }
    }

    public interface IDelegateProcedureFactory
    {
        Type DelegateType { get; }
        IProcedure NewProcedure(Delegate d);
    }

    public static partial class ProxyGenerator
    {
        private static void PopObjectOffList(CodeGenerator cg, LocalBuilder theList)
        {
            cg.LoadLocal(theList);
            cg.IfNot(OpCodes.Brfalse_S);
                cg.LoadLocal(theList);
                cg.Call(typeof(FList<object>).GetMethod("get_Head"));
                cg.LoadLocal(theList);
                cg.Call(typeof(FList<object>).GetMethod("get_Tail"));
                cg.StoreLocal(theList);
            cg.Else(false);
                ThrowSchemeException(cg, "Too few arguments");
            cg.Then();
        }

        private static void VerifyListEmpty(CodeGenerator cg, LocalBuilder theList)
        {
            cg.LoadLocal(theList);
            cg.IfNot(OpCodes.Brfalse_S);
                ThrowSchemeException(cg, "Too many arguments");
            cg.Then();
        }

        private static void ThrowSchemeException(CodeGenerator cg, string msg)
        {
            cg.LoadString(cg.Context + ": " + msg);
            cg.NewObj(typeof(SchemeRuntimeException).GetConstructor(new Type[] { typeof(string) }));
            cg.Throw();
        }

        private static void NewRunnableReturn(CodeGenerator cg)
        {
            cg.NewObj(typeof(RunnableReturn).GetConstructor(new Type[] { typeof(IContinuation), typeof(object) }));
        }

        private static void NewRunnableThrow(CodeGenerator cg)
        {
            cg.NewObj(typeof(RunnableThrow).GetConstructor(new Type[] { typeof(IContinuation), typeof(object) }));
        }

        public static void MarshalToIDisposable(CodeGenerator cg, LocalBuilder localDest, int globalStateIndex)
        {
            LocalBuilder l1 = cg.DeclareLocal(typeof(DisposableID));

            cg.Dup();
            cg.IsInst(typeof(DisposableID));
            cg.IfNot(OpCodes.Brfalse_S);
                cg.Unbox(typeof(DisposableID));
                cg.LoadObjIndirect(typeof(DisposableID));
                cg.StoreLocal(l1);
                cg.LoadArg(globalStateIndex);
                cg.LoadLocal(l1);
                cg.CallVirt(typeof(IGlobalState).GetMethod("GetDisposableByID", new Type[] { typeof(DisposableID) }));
                cg.Dup();
                cg.IsInst(localDest.LocalType);
                cg.IfNot(OpCodes.Brfalse_S);
                    cg.CastClass(localDest.LocalType);
                    cg.StoreLocal(localDest);
                cg.Else(false);
                    cg.Drop();
                    ThrowSchemeException(cg, "Disposable type mismatch: expected " + localDest.LocalType.FullName);
                cg.Then();
            cg.Else(false);
                cg.Drop();
                ThrowSchemeException(cg, "DisposableID expected");
            cg.Then();
        }

        private static void MarshalToChar(CodeGenerator cg, LocalBuilder localCharDest)
        {
            cg.Dup();
            cg.IsInst(typeof(char));
            cg.IfNot(OpCodes.Brfalse_S);
                cg.Unbox(typeof(char));
                cg.LoadObjIndirect(typeof(char));
                cg.StoreLocal(localCharDest);
            cg.Else(false);
                cg.Drop();
                ThrowSchemeException(cg, "char expected");
            cg.Then();
        }

        private static void MarshalToBool(CodeGenerator cg, LocalBuilder localBoolDest)
        {
            cg.Dup();
            cg.IsInst(typeof(bool));
            cg.IfNot(OpCodes.Brfalse_S);
                cg.Unbox(typeof(bool));
                cg.LoadObjIndirect(typeof(bool));
                cg.StoreLocal(localBoolDest);
            cg.Else(false);
                cg.Drop();
                ThrowSchemeException(cg, "bool expected");
            cg.Then();
        }

        private static void MarshalToString(CodeGenerator cg, LocalBuilder localStringDest)
        {
            cg.Dup();
            cg.IsInst(typeof(SchemeString));
            cg.IfNot(OpCodes.Brfalse_S);
                cg.CastClass(typeof(SchemeString));
                cg.Call(typeof(SchemeString).GetMethod("get_TheString"));
                cg.StoreLocal(localStringDest);
            cg.Else(false);
                cg.Drop();
                ThrowSchemeException(cg, "SchemeString expected");
            cg.Then();
        }

        private static void MarshalToSchemeString(CodeGenerator cg, LocalBuilder localSchemeStringDest)
        {
            cg.Dup();
            cg.IsInst(typeof(SchemeString));
            cg.IfNot(OpCodes.Brfalse_S);
                cg.CastClass(typeof(SchemeString));
                cg.StoreLocal(localSchemeStringDest);
            cg.Else(false);
                cg.Drop();
                ThrowSchemeException(cg, "SchemeString expected");
            cg.Then();
        }

        private static void MarshalToBigRational(CodeGenerator cg, LocalBuilder localBigRationalDest)
        {
            cg.Dup();
            cg.IsInst(typeof(BigRational));
            cg.IfNot(OpCodes.Brfalse_S);
                cg.CastClass(typeof(BigRational));
                cg.StoreLocal(localBigRationalDest);
            cg.Else(false);
                cg.Dup();
                cg.IsInst(typeof(BigInteger));
                cg.IfNot(OpCodes.Brfalse_S);
                    cg.Call(typeof(BigInteger).GetProperty("One").GetGetMethod());
                    cg.NewObj(typeof(BigRational).GetConstructor(new Type[] { typeof(BigInteger), typeof(BigInteger) }));
                    cg.StoreLocal(localBigRationalDest);
                cg.Else(false);
                    cg.Drop();
                    ThrowSchemeException(cg, "BigInteger or BigRational expected");
                cg.Then();
            cg.Then();        
        }

        [SchemeFunction("exact->inexact")]
        public static double NumberToDouble(object obj)
        {
            if (obj is BigRational)
            {
                return ((BigRational)obj).GetDoubleValue(RoundingMode.Round);
            }
            else if (obj is BigInteger)
            {
                return (new BigRational((BigInteger)obj, BigInteger.One)).GetDoubleValue(RoundingMode.Round);
            }
            else if (obj is double) return (double)obj;
            else if (obj is float) return (double)(float)obj;
            else throw new SchemeRuntimeException("number->double type mismatch error");
        }

        private static void MarshalToDouble(CodeGenerator cg, LocalBuilder localDoubleDest)
        {
            cg.Call(typeof(ProxyGenerator).GetMethod("NumberToDouble", new Type[] { typeof(object) }));
            cg.StoreLocal(localDoubleDest);
        }

        public static float NumberToFloat(object obj)
        {
            if (obj is BigRational)
            {
                return ((BigRational)obj).GetSingleValue(RoundingMode.Round);
            }
            else if (obj is BigInteger)
            {
                return (new BigRational((BigInteger)obj, BigInteger.One)).GetSingleValue(RoundingMode.Round);
            }
            else if (obj is double) return (float)(double)obj;
            else if (obj is float) return (float)obj;
            else throw new SchemeRuntimeException("number->float type mismatch error");
        }

        private static void MarshalToFloat(CodeGenerator cg, LocalBuilder localFloatDest)
        {
            cg.Call(typeof(ProxyGenerator).GetMethod("NumberToFloat", new Type[] { typeof(object) }));
            cg.StoreLocal(localFloatDest);
        }

        private static void MarshalToObject(CodeGenerator cg, LocalBuilder localObjectDest)
        {
            cg.StoreLocal(localObjectDest);
        }

        private static void MarshalToSpecificClass(CodeGenerator cg, LocalBuilder localSpecificClassDest)
        {
            cg.Dup();
            cg.IsInst(localSpecificClassDest.LocalType);
            cg.IfNot(OpCodes.Brfalse_S);
                cg.CastClass(localSpecificClassDest.LocalType);
                cg.StoreLocal(localSpecificClassDest);
            cg.Else(false);
                cg.Drop();
                ThrowSchemeException(cg, localSpecificClassDest.LocalType.ToString() + " expected");
            cg.Then();
        }

        private static void MarshalToValueType(CodeGenerator cg, LocalBuilder localValueTypeDest)
        {
            cg.Dup();
            cg.IsInst(localValueTypeDest.LocalType);
            cg.IfNot(OpCodes.Brfalse_S);
                cg.Unbox(localValueTypeDest.LocalType);
                cg.LoadObjIndirect(localValueTypeDest.LocalType);
                cg.StoreLocal(localValueTypeDest);
            cg.Else(false);
                cg.Drop();
                ThrowSchemeException(cg, localValueTypeDest.LocalType.ToString() + " expected");
            cg.Then();
        }

        private static void UnmarshalFromVoid(CodeGenerator cg)
        {
            cg.LoadInt((int)SpecialValue.UNSPECIFIED);
            cg.Box(typeof(SpecialValue));
        }

        private static void UnmarshalFromString(CodeGenerator cg)
        {
            cg.NewObj(typeof(SchemeString).GetConstructor(new Type[] { typeof(string) }));
        }

        private static void UnmarshalFromSchemeString(CodeGenerator cg)
        {
            //cg.CastClass(typeof(object));
        }

        private static void UnmarshalFromBool(CodeGenerator cg)
        {
            cg.Box(typeof(bool));
        }

        private static void UnmarshalFromChar(CodeGenerator cg)
        {
            cg.Box(typeof(char));
        }

        private static void UnmarshalFromFloat(CodeGenerator cg)
        {
            cg.ILGenerator.Emit(OpCodes.Conv_R8);
            cg.Box(typeof(double));
        }

        private static void UnmarshalFromBigRational(CodeGenerator cg)
        {
            cg.Dup();
            cg.Call(typeof(BigRational).GetProperty("Denominator").GetGetMethod());
            cg.Call(typeof(BigInteger).GetProperty("One").GetGetMethod());
            cg.Call(typeof(BigInteger).GetMethod("op_Equality"));
            cg.IfNot(OpCodes.Brfalse_S);
                cg.Call(typeof(BigRational).GetProperty("Numerator").GetGetMethod());
            cg.Then();
        }

        private static void UnmarshalFromIDisposable(CodeGenerator cg, string name, int globalStateIndex)
        {
            LocalBuilder l1 = cg.DeclareLocal(typeof(IDisposable));

            cg.StoreLocal(l1);
            cg.LoadArg(globalStateIndex);
            cg.LoadLocal(l1);
            cg.LoadString(name);
            cg.CallVirt(typeof(IGlobalState).GetMethod("RegisterDisposable", new Type[] { typeof(IDisposable), typeof(string) }));
            cg.Box(typeof(DisposableID));
        }

        [Serializable]
        private class ProxyProxy
        {
            private MethodBase mb;

            public ProxyProxy(MethodBase mb)
            {
                this.mb = mb;
            }

            public MethodBase MethodBase { get { return mb; } }
        }

        [Serializable]
        private class SingletonProxy
        {
            private Type t;

            public SingletonProxy(Type t)
            {
                this.t = t;
            }

            public Type Type { get { return t; } }
        }

        [Serializable]
        private class IsAProxy
        {
            private Type t;

            public IsAProxy(Type t)
            {
                this.t = t;
            }

            public Type Type { get { return t; } }
        }

        private class ProxySurrogate : ISerializationSurrogate
        {
            public ProxySurrogate()
            {
            }

            #region ISerializationSurrogate Members

            public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
            {
                IProxyProcedure pp = obj as IProxyProcedure;
                if (pp == null) throw new Exception("ProxySurrogate used on non-IProxyProcedure!");

                info.SetType(typeof(ProxyProxy));
                info.AddValue("mb", pp.WrappedMethod);
            }

            public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
            {
                if (proxyDictionary == null) throw new Exception("No proxy dictionary!");

                if (obj is ProxyProxy)
                {
                    ProxyProxy pp = (ProxyProxy)obj;
                    MethodBase mb = (MethodBase)info.GetValue("mb", typeof(MethodBase));
                    return proxyDictionary[mb];
                }
                else if (obj is SingletonProxy)
                {
                    SingletonProxy sp = (SingletonProxy)obj;
                    Type t = (Type)info.GetValue("t", typeof(Type));
                    return singletonDictionary[t];
                }
                else if (obj is IsAProxy)
                {
                    IsAProxy ip = (IsAProxy)obj;
                    Type t = (Type)info.GetValue("t", typeof(Type));
                    return isADictionary[t];
                }
                else throw new Exception("Unknown type in ProxySurrogate!");
            }

            #endregion
        }

        private class SingletonSurrogate : ISerializationSurrogate
        {
            private Type t;

            public SingletonSurrogate(Type t)
            {
                this.t = t;
            }

            #region ISerializationSurrogate Members

            public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
            {
                info.SetType(typeof(SingletonProxy));
                info.AddValue("t", t);
            }

            public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
            {
                throw new Exception("Not implemented.");
            }

            #endregion
        }

        private class IsASurrogate : ISerializationSurrogate
        {
            private Type t;

            public IsASurrogate(Type t)
            {
                this.t = t;
            }

            #region ISerializationSurrogate Members

            public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
            {
                info.SetType(typeof(IsAProxy));
                info.AddValue("t", t);
            }

            public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
            {
                throw new Exception("Not implemented.");
            }

            #endregion
        }

        public static void AddProxySurrogates(SurrogateSelector ss)
        {
            if (proxyDictionary == null) throw new Exception("No proxy dictionary!");

            ProxySurrogate ps = new ProxySurrogate();
            StreamingContext sc = new StreamingContext(StreamingContextStates.All);

            foreach (KeyValuePair<MethodBase, IProxyProcedure> kvp in proxyDictionary)
            {
                ss.AddSurrogate(kvp.Value.GetType(), sc, ps);
            }
            foreach (KeyValuePair<Type, IProcedure> kvp in singletonDictionary)
            {
                ss.AddSurrogate(kvp.Key, sc, new SingletonSurrogate(kvp.Key));
            }
            foreach (KeyValuePair<Type, IProcedure> kvp in isADictionary)
            {
                ss.AddSurrogate(kvp.Key, sc, new IsASurrogate(kvp.Key));
            }
            ss.AddSurrogate(typeof(ProxyProxy), sc, ps);
            ss.AddSurrogate(typeof(SingletonProxy), sc, ps);
            ss.AddSurrogate(typeof(IsAProxy), sc, ps);
        }

        private static Dictionary<MethodBase, IProxyProcedure> proxyDictionary = null;
        private static Dictionary<Type, IProcedure> singletonDictionary = null;
        private static Dictionary<Type, IProcedure> isADictionary = null;
        private static Dictionary<Type, IDelegateProcedureFactory> proxyFromDelegateDictionary = null;

        private static bool ImplementsInterface(Type theType, Type theInterface)
        {
            Type[] interfaces = theType.GetInterfaces();
            foreach (Type t in interfaces)
            {
                if (t == theInterface) return true;
            }
            return false;
        }

        public static IProcedure GenerateSingleton(string name, Type t)
        {
            if (singletonDictionary == null) singletonDictionary = new Dictionary<Type, IProcedure>();

            if (singletonDictionary.ContainsKey(t)) return singletonDictionary[t];

            if (t.IsGenericTypeDefinition) throw new Exception("Singleton classes cannot be generic!");

            if (!ImplementsInterface(t, typeof(IProcedure))) throw new Exception("Singleton class must implement IProcedure!");

            ConstructorInfo ci = t.GetConstructor(System.Type.EmptyTypes);
            if (ci == null) throw new Exception("Singleton class must have a no-argument constructor!");

            IProcedure proc = (IProcedure)ci.Invoke(null);
            singletonDictionary.Add(ci.DeclaringType, proc);
            return proc;
        }

        private static string GetDisposableDescription(MethodBase mb)
        {
            SchemeDisposableAttribute[] sda = mb.GetCustomAttributes<SchemeDisposableAttribute>(false);
            if (sda.Length == 1) return sda[0].Description;
            if (mb is ConstructorInfo)
            {
                return ((ConstructorInfo)mb).DeclaringType.FullName;

            }
            else
            {
                return ((MethodInfo)mb).ReturnType.FullName;
            }
        }

        public static IProxyProcedure GenerateProxy(ModuleBuilder bModule, string className, MethodBase mi, string context)
        {
            if (proxyDictionary == null) proxyDictionary = new Dictionary<MethodBase, IProxyProcedure>();

            if (proxyDictionary.ContainsKey(mi))
            {
                return proxyDictionary[mi];
            }

            System.Diagnostics.Trace.WriteLine("Generating Proxy: " + className);

            TypeBuilder bType = bModule.DefineType
            (
                className,
                TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed,
                null,
                new Type[] { typeof(IProxyProcedure) }
            );

            FieldBuilder bWrappedMethodField = bType.DefineField
            (
                "methodBase",
                typeof(MethodBase),
                FieldAttributes.Private
            );

            PropertyBuilder bArity = bType.DefineProperty("Arity", PropertyAttributes.None, typeof(int), null);

            PropertyBuilder bMore = bType.DefineProperty("More", PropertyAttributes.None, typeof(bool), null);

            PropertyBuilder bWrappedMethod = bType.DefineProperty("WrappedMethod", PropertyAttributes.None, typeof(MethodBase), null);

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

            MethodBuilder bWrappedMethodGet = bType.DefineMethod
            (
                "get_WrappedMethod",
                MethodAttributes.Virtual | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                typeof(MethodBase),
                null
            );

            bWrappedMethod.SetGetMethod(bWrappedMethodGet);

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

            CodeGenerator cg = new CodeGenerator(bCall.GetILGenerator(), context);
            LocalBuilder lException = cg.DeclareLocal(typeof(Exception));
            LocalBuilder lRunnableStep = cg.DeclareLocal(typeof(IRunnableStep));

            ParameterInfo[] pi = mi.GetParameters();

            bool passThis = ((mi.CallingConvention & CallingConventions.HasThis) != 0) && !mi.IsConstructor;

            int iend = pi.Length;

            LocalBuilder thisVar = null;
                
            if (passThis)
            {
                thisVar = cg.DeclareLocal(mi.DeclaringType);
            }

            LocalBuilder[] lb = null;

            if (iend > 0)
            {
                lb = new LocalBuilder[iend];
            }

            LocalBuilder theList = cg.DeclareLocal(typeof(FList<object>));

            bool handledGlobalState = false;
            for (int i1 = 0; i1 < iend; ++i1)
            {
                if (pi[i1].IsOut) throw new Exception("Cannot handle ref or out parameters");
                if (pi[i1].ParameterType == typeof(IGlobalState))
                {
                    if (handledGlobalState) throw new Exception("Only one IGlobalState allowed");
                    lb[i1] = null;
                    handledGlobalState = true;
                }
                else
                {
                    lb[i1] = cg.DeclareLocal(pi[i1].ParameterType);
                }
            }

            cg.Try();

            //MethodInfo miBreak = typeof(System.Diagnostics.Debugger).GetMethod("Break");
            //cg.Call(miBreak);

            int ARG_GLOBALSTATE = 1;
            int ARG_ARGS = 2;
            int ARG_CONTINUATION = 3;

            cg.LoadArg(ARG_CONTINUATION);

            cg.LoadArg(ARG_ARGS);
            cg.StoreLocal(theList);

            if (passThis)
            {
                System.Diagnostics.Trace.WriteLine("Argument this: " + thisVar.LocalType);
                PopObjectOffList(cg, theList);
                MarshalToTypeAndStore(cg, OverflowBehavior.Wraparound, thisVar, ARG_GLOBALSTATE);
            }

            for (int i2 = 0; i2 < iend; ++i2)
            {
                System.Diagnostics.Trace.WriteLine("Argument " + i2 + ": " + ((lb[i2] == null) ? "NULL (IGlobalState)" : lb[i2].LocalType.ToString()));
                if (pi[i2].ParameterType == typeof(IGlobalState))
                {
                    // do nothing
                }
                else
                {
                    PopObjectOffList(cg, theList);
                    OverflowBehavior beh = OverflowBehavior.Wraparound;
                    OverflowModeAttribute[] obj = pi[i2].GetCustomAttributes<OverflowModeAttribute>(false);
                    if (obj.Length == 1)
                    {
                        beh = obj[0].Behavior;
                    }

                    MarshalToTypeAndStore(cg, beh, lb[i2], ARG_GLOBALSTATE);
                }
            }

            System.Diagnostics.Trace.WriteLine("Verify List Empty");

            VerifyListEmpty(cg, theList);

            if (passThis)
            {
                cg.LoadLocal(thisVar);
            }

            for (int i3 = 0; i3 < iend; ++i3)
            {
                if (pi[i3].ParameterType == typeof(IGlobalState))
                {
                    cg.LoadArg(ARG_GLOBALSTATE);
                }
                else
                {
                    cg.LoadLocal(lb[i3]);
                }
            }

            if (mi.IsConstructor)
            {
                cg.NewObj((ConstructorInfo)mi);
                UnmarshalFromType(cg, mi.DeclaringType, GetDisposableDescription(mi), ARG_GLOBALSTATE);
            }
            else
            {
                MethodInfo mmi = (MethodInfo)mi;
                if (mmi.IsVirtual)
                {
                    cg.CallVirt(mmi);
                }
                else
                {
                    cg.Call(mmi);
                }
                UnmarshalFromType(cg, mmi.ReturnType, GetDisposableDescription(mi), ARG_GLOBALSTATE);
            }

            NewRunnableReturn(cg);
            cg.StoreLocal(lRunnableStep);

            cg.Catch(typeof(Exception));
            
            cg.StoreLocal(lException);
            cg.LoadArg(ARG_CONTINUATION);
            cg.LoadLocal(lException);
            NewRunnableThrow(cg);
            cg.StoreLocal(lRunnableStep);

            cg.EndTryCatchFinally();

            cg.LoadLocal(lRunnableStep);
            cg.Return();

            CodeGenerator cgArityGet = new CodeGenerator(bArityGet.GetILGenerator(), className);
            cgArityGet.LoadInt(iend + (passThis ? 1 : 0));
            cgArityGet.Return();

            CodeGenerator cgMoreGet = new CodeGenerator(bMoreGet.GetILGenerator(), className);
            cgMoreGet.LoadInt(0);
            cgMoreGet.Return();

            CodeGenerator cgWrappedMethodGet = new CodeGenerator(bWrappedMethodGet.GetILGenerator(), className);
            cgWrappedMethodGet.LoadArg(0);
            cgWrappedMethodGet.LoadField(bWrappedMethodField);
            cgWrappedMethodGet.Return();

            Type[] constructorParams = new Type[] { typeof(MethodBase) };

            ConstructorBuilder bConstructor = bType.DefineConstructor
            (
                MethodAttributes.Public,
                CallingConventions.Standard,
                constructorParams
            );

            ParameterBuilder pb = bConstructor.DefineParameter(1, ParameterAttributes.None, "methodBase");

            CodeGenerator cgConstructor = new CodeGenerator(bConstructor.GetILGenerator(), className);
            cgConstructor.LoadArg(0);
            cgConstructor.LoadArg(1);
            cgConstructor.StoreField(bWrappedMethodField);
            cgConstructor.Return();

            IProxyProcedure proc = (IProxyProcedure)(bType.CreateType().GetConstructor(constructorParams).Invoke(new object[] { mi }));

            proxyDictionary.Add(mi, proc);

            return proc;
        }

        public static IProcedure GenerateProxyFromDelegate(ModuleBuilder bModule, string className, string factoryClassName, Delegate del)
        {
            if (proxyFromDelegateDictionary == null) proxyFromDelegateDictionary = new Dictionary<Type, IDelegateProcedureFactory>();

            Type delegateType = del.GetType();

            if (proxyFromDelegateDictionary.ContainsKey(delegateType))
            {
                return proxyFromDelegateDictionary[delegateType].NewProcedure(del);
            }

            System.Diagnostics.Trace.WriteLine("Generating Proxy for Delegate Type: " + delegateType.FullName);

            TypeBuilder bFactoryType = bModule.DefineType
            (
                factoryClassName,
                TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed,
                null,
                new Type[] { typeof(IDelegateProcedureFactory) }
            );

            TypeBuilder bType = bModule.DefineType
            (
                className,
                TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed,
                null,
                new Type[] { typeof(IProcedure) }
            );

            PropertyBuilder bDelegateType = bFactoryType.DefineProperty("DelegateType", PropertyAttributes.None, typeof(Type), null);

            MethodBuilder bDelegateTypeGet = bFactoryType.DefineMethod
            (
                "get_DelegateType",
                MethodAttributes.Virtual | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                typeof(Type),
                null
            );

            bDelegateType.SetGetMethod(bDelegateTypeGet);

            string context = className + " (" + factoryClassName + ")";
            CodeGenerator cg_dtg = new CodeGenerator(bDelegateTypeGet.GetILGenerator(), context);
            cg_dtg.LoadToken(delegateType);
            cg_dtg.Call(typeof(Type).GetMethod("GetTypeFromHandle", new Type[] { typeof(RuntimeTypeHandle) }));
            cg_dtg.Return();

            MethodBuilder bDelegateNewProcedure = bFactoryType.DefineMethod
            (
                "NewProcedure",
                MethodAttributes.Virtual | MethodAttributes.Public | MethodAttributes.HideBySig,
                typeof(IProcedure),
                new Type[] { typeof(Delegate) }
            );

            FieldBuilder bDelegateField = bType.DefineField
            (
                "theDelegate",
                delegateType,
                FieldAttributes.Private
            );

            Type[] constructorParams = new Type[] { delegateType };

            ConstructorBuilder bConstructor = bType.DefineConstructor
            (
                MethodAttributes.Public,
                CallingConventions.Standard,
                constructorParams
            );

            ParameterBuilder pb = bConstructor.DefineParameter(1, ParameterAttributes.None, "theDelegate");

            CodeGenerator cgConstructor = new CodeGenerator(bConstructor.GetILGenerator(), context);
            cgConstructor.LoadArg(0);
            cgConstructor.LoadArg(1);
            cgConstructor.StoreField(bDelegateField);
            cgConstructor.Return();

            CodeGenerator cg_dnp = new CodeGenerator(bDelegateNewProcedure.GetILGenerator(), context);
            cg_dnp.LoadArg(1);
            cg_dnp.CastClass(delegateType);
            cg_dnp.NewObj(bConstructor);
            cg_dnp.Return();

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

            CodeGenerator cg = new CodeGenerator(bCall.GetILGenerator(), context);
            LocalBuilder lException = cg.DeclareLocal(typeof(Exception));
            LocalBuilder lRunnableStep = cg.DeclareLocal(typeof(IRunnableStep));

            MethodInfo mi = delegateType.GetMethod("Invoke");

            ParameterInfo[] pi = mi.GetParameters();

            int iend = pi.Length;

            LocalBuilder[] lb = null;

            if (iend > 0)
            {
                lb = new LocalBuilder[iend];
            }

            LocalBuilder theList = cg.DeclareLocal(typeof(FList<object>));

            bool handledGlobalState = false;
            for (int i1 = 0; i1 < iend; ++i1)
            {
                if (pi[i1].IsOut) throw new Exception("Cannot handle ref or out parameters");
                if (pi[i1].ParameterType == typeof(IGlobalState))
                {
                    if (handledGlobalState) throw new Exception("Only one IGlobalState allowed");
                    lb[i1] = null;
                    handledGlobalState = true;
                }
                else
                {
                    lb[i1] = cg.DeclareLocal(pi[i1].ParameterType);
                }
            }

            cg.Try();

            //MethodInfo miBreak = typeof(System.Diagnostics.Debugger).GetMethod("Break");
            //cg.Call(miBreak);

            int ARG_GLOBALSTATE = 1;
            int ARG_ARGS = 2;
            int ARG_CONTINUATION = 3;

            cg.LoadArg(ARG_CONTINUATION);

            cg.LoadArg(ARG_ARGS);
            cg.StoreLocal(theList);

            for (int i2 = 0; i2 < iend; ++i2)
            {
                System.Diagnostics.Trace.WriteLine("Argument " + i2 + ": " + ((lb[i2] == null) ? "NULL (IGlobalState)" : lb[i2].LocalType.ToString()));
                if (pi[i2].ParameterType == typeof(IGlobalState))
                {
                    // do nothing
                }
                else
                {
                    PopObjectOffList(cg, theList);
                    OverflowBehavior beh = OverflowBehavior.Wraparound;
                    object[] obj = pi[i2].GetCustomAttributes(typeof(OverflowModeAttribute), false);
                    if (obj.Length == 1)
                    {
                        OverflowModeAttribute oma = (OverflowModeAttribute)obj[0];
                        beh = oma.Behavior;
                    }

                    MarshalToTypeAndStore(cg, beh, lb[i2], ARG_GLOBALSTATE);
                }
            }

            System.Diagnostics.Trace.WriteLine("Verify List Empty");

            VerifyListEmpty(cg, theList);

            cg.LoadArg(0); // this
            cg.LoadField(bDelegateField);

            for (int i3 = 0; i3 < iend; ++i3)
            {
                if (pi[i3].ParameterType == typeof(IGlobalState))
                {
                    cg.LoadArg(ARG_GLOBALSTATE);
                }
                else
                {
                    cg.LoadLocal(lb[i3]);
                }
            }

            cg.CallVirt(mi);

            UnmarshalFromType(cg, mi.ReturnType, GetDisposableDescription(mi), ARG_GLOBALSTATE);

            NewRunnableReturn(cg);
            cg.StoreLocal(lRunnableStep);

            cg.Catch(typeof(Exception));

            cg.StoreLocal(lException);
            cg.LoadArg(ARG_CONTINUATION);
            cg.LoadLocal(lException);
            NewRunnableThrow(cg);
            cg.StoreLocal(lRunnableStep);

            cg.EndTryCatchFinally();

            cg.LoadLocal(lRunnableStep);
            cg.Return();

            CodeGenerator cgArityGet = new CodeGenerator(bArityGet.GetILGenerator(), context);
            cgArityGet.LoadInt(iend);
            cgArityGet.Return();

            CodeGenerator cgMoreGet = new CodeGenerator(bMoreGet.GetILGenerator(), context);
            cgMoreGet.LoadInt(0);
            cgMoreGet.Return();

            Type bCreatedType = bType.CreateType();

            IDelegateProcedureFactory factory = (IDelegateProcedureFactory)(bFactoryType.CreateType().GetConstructor(Type.EmptyTypes).Invoke(new object[] { }));

            proxyFromDelegateDictionary.Add(delegateType, factory);

            return factory.NewProcedure(del);
        }

        public static IProcedure GenerateIsAFunction(ModuleBuilder bModule, string className, Type t)
        {
            if (isADictionary == null) isADictionary = new Dictionary<Type, IProcedure>();

            if (isADictionary.ContainsKey(t))
            {
                return isADictionary[t];
            }

            System.Diagnostics.Trace.WriteLine("Generating Is-a: " + className);

            TypeBuilder bType = bModule.DefineType
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

            CodeGenerator cgArityGet = new CodeGenerator(bArityGet.GetILGenerator(), context);
            cgArityGet.LoadInt(1);
            cgArityGet.Return();

            CodeGenerator cgMoreGet = new CodeGenerator(bMoreGet.GetILGenerator(), context);
            cgMoreGet.LoadInt(0);
            cgMoreGet.Return();

            Type[] constructorParams = new Type[] { };

            ConstructorBuilder bConstructor = bType.DefineConstructor
            (
                MethodAttributes.Public,
                CallingConventions.Standard,
                constructorParams
            );

            CodeGenerator cgConstructor = new CodeGenerator(bConstructor.GetILGenerator(), context);
            cgConstructor.Return();

            IProcedure proc = (IProcedure)(bType.CreateType().GetConstructor(constructorParams).Invoke(null));
            
            isADictionary.Add(t, proc);

            return proc;
        }

        [SchemeFunction("generate-exe")]
        public static void GenerateExe(string exeName, IProcedure mainFunc)
        {
            AssemblyName aName = new AssemblyName(exeName);
            AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly(aName, AssemblyBuilderAccess.Save);
            ModuleBuilder mb = ab.DefineDynamicModule(exeName, exeName);

            ExprObjModel.Procedures.SchemeByteArray b = ExprObjModel.Procedures.ProxyDiscovery.SerializeToBytes(mainFunc);

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(b.Bytes))
            {
                mb.DefineManifestResource("VMCode", ms, ResourceAttributes.Public);

                TypeBuilder tb = mb.DefineType("Program", TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed);

                MethodBuilder meb = tb.DefineMethod("Main", MethodAttributes.Static | MethodAttributes.Public, typeof(void), new Type[] { typeof(string[]) });

                CodeGenerator cg = new CodeGenerator(meb.GetILGenerator(), "generate-exe");

                LocalBuilder l_scheduler = cg.DeclareLocal(typeof(ControlledWindowLib.Scheduling.Scheduler));
                LocalBuilder l_topLevelEnvironment = cg.DeclareLocal(typeof(TopLevelEnvironment));
                LocalBuilder l_binaryFormatter = cg.DeclareLocal(typeof(System.Runtime.Serialization.Formatters.Binary.BinaryFormatter));
                LocalBuilder l_surrogateSelector = cg.DeclareLocal(typeof(System.Runtime.Serialization.SurrogateSelector));
                LocalBuilder l_doerResult = cg.DeclareLocal(typeof(DoerResult));
                LocalBuilder l_globalState = cg.DeclareLocal(typeof(GlobalState));

                cg.NewObj(typeof(TopLevelEnvironment).GetConstructor(new Type[] { }));
                cg.StoreLocal(l_topLevelEnvironment);

                cg.LoadLocal(l_topLevelEnvironment);
                cg.LoadToken(typeof(ExprObjModel.Procedures.ProxyDiscovery));
                cg.Call(typeof(Type).GetMethod("GetTypeFromHandle", new Type[] { typeof(RuntimeTypeHandle) }));
                cg.CallVirt(typeof(Type).GetProperty("Assembly").GetGetMethod());
                cg.Call(typeof(TopLevelEnvironment).GetMethod("ImportSchemeFunctions"));

                cg.NewObj(typeof(System.Runtime.Serialization.Formatters.Binary.BinaryFormatter).GetConstructor(new Type[] { }));
                cg.StoreLocal(l_binaryFormatter);

                cg.NewObj(typeof(System.Runtime.Serialization.SurrogateSelector).GetConstructor(new Type[] { }));
                cg.Dup();
                cg.StoreLocal(l_surrogateSelector);
                cg.Call(typeof(ProxyGenerator).GetMethod("AddProxySurrogates", new Type[] { typeof(System.Runtime.Serialization.SurrogateSelector) }));

                cg.LoadLocal(l_binaryFormatter);
                cg.LoadLocal(l_surrogateSelector);
                cg.Call(typeof(System.Runtime.Serialization.Formatters.Binary.BinaryFormatter).GetProperty("SurrogateSelector").GetSetMethod());

                cg.NewObj(typeof(ControlledWindowLib.Scheduling.Scheduler).GetConstructor(Type.EmptyTypes));
                cg.StoreLocal(l_scheduler);

                cg.Try();

                cg.LoadLocal(l_scheduler);
                cg.NewObj(typeof(PlainConsole).GetConstructor(Type.EmptyTypes));
                cg.CastClass(typeof(IConsole));
                cg.NewObj(typeof(GlobalState).GetConstructor(new Type[] { typeof(ControlledWindowLib.Scheduling.Scheduler), typeof(IConsole) }));
                cg.StoreLocal(l_globalState);

                cg.Try();

                cg.LoadLocal(l_globalState);
                cg.CastClass(typeof(IGlobalState));

                cg.LoadLocal(l_binaryFormatter);
                cg.LoadToken(tb.TypeToken);
                cg.Call(typeof(Type).GetMethod("GetTypeFromHandle", new Type[] { typeof(RuntimeTypeHandle) }));
                cg.CallVirt(typeof(Type).GetProperty("Assembly").GetGetMethod());
                cg.LoadString("VMCode");
                cg.CallVirt(typeof(Assembly).GetMethod("GetManifestResourceStream", new Type[] { typeof(string) }));
                cg.Call(typeof(System.Runtime.Serialization.Formatters.Binary.BinaryFormatter).GetMethod("Deserialize", new Type[] { typeof(System.IO.Stream) }));

                cg.CastClass(typeof(IProcedure));

                cg.LoadArg(0);

                cg.Call(typeof(Doer).GetMethod("ApplyToStrings", new Type[] { typeof(IGlobalState), typeof(IProcedure), typeof(string[]) }));

                cg.StoreLocal(l_doerResult);

                cg.LoadLocal(l_doerResult);
                cg.Call(typeof(DoerResult).GetProperty("IsException").GetGetMethod());
                cg.IfNot(OpCodes.Brfalse_S);
                cg.Call(typeof(Console).GetMethod("WriteLine", Type.EmptyTypes));
                cg.LoadString("***** Exception *****");
                cg.Call(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }));
                cg.Call(typeof(Console).GetMethod("WriteLine", new Type[] { }));
                cg.LoadLocal(l_doerResult);
                cg.Call(typeof(DoerResult).GetProperty("Result").GetGetMethod());
                //cg.CallVirt(typeof(object).GetMethod("ToString", new Type[] { }));
                cg.Call(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(object) }));
                cg.Then();

                cg.Leave();

                cg.Finally();

                cg.LoadLocal(l_globalState);
                cg.CastClass(typeof(IDisposable));
                cg.CallVirt(typeof(IDisposable).GetMethod("Dispose", Type.EmptyTypes));

                cg.EndTryCatchFinally();

                cg.Finally();

                cg.LoadLocal(l_scheduler);
                cg.CastClass(typeof(IDisposable));
                cg.CallVirt(typeof(IDisposable).GetMethod("Dispose", Type.EmptyTypes));

                cg.EndTryCatchFinally();

                cg.Return();

                Type t = tb.CreateType();

                ab.SetEntryPoint(t.GetMethod("Main"));

                ab.Save(exeName);
            }
        }

#if false
        private static bool IsAssignableFrom(Type dest, Type src)
        {
            if (dest.IsAssignableFrom(src)) return true;

            
        }

        private class ParamInfo
        {
            public Type[] paramTypes;
            public bool repeatingTail;

            public static bool LessThan(ParamInfo a, ParamInfo b)
            {
                if (a.repeatingTail != b.repeatingTail)
                {
                    if (b.repeatingTail) return true;
                    if (a.repeatingTail) return false;
                }
                else
                {
                    if (a.repeatingTail == false)
                    {
                        if (a.paramTypes.Length != b.paramTypes.Length)
                        {
                            return a.paramTypes.Length < b.paramTypes.Length;
                        }
                        else
                        {
                            int iEnd = a.paramTypes.Length;
                            for (int i = 0; i < iEnd; ++i)
                            {
                                if (LessThan(a.paramTypes[i], b.paramTypes[i])) return true;
                                if (LessThan(b.paramTypes[i], a.paramTypes[i])) return false;
                            }
                            return false;
                        }
                    }
                    else
                    {

                    }
                }
            }

            private static Tuple<Type, Type[]>[] charts = new Tuple<Type, Type[]>[]
            {
                new Tuple<Type, Type[]>(typeof(byte), new Type[] { typeof(sbyte), typeof(ushort), typeof(short), typeof(uint), typeof(int), typeof(ulong), typeof(long), typeof(BigInteger), typeof(BigRational), typeof(float), typeof(double) }),
                new Tuple<Type, Type[]>(typeof(sbyte), new Type[] { typeof(ushort), typeof(short), typeof(uint), typeof(int), typeof(ulong), typeof(long), typeof(BigInteger), typeof(BigRational), typeof(float), typeof(double) }),
                new Tuple<Type, Type[]>(typeof(ushort), new Type[] { typeof(short), typeof(uint), typeof(int), typeof(ulong), typeof(long), typeof(BigInteger), typeof(BigRational), typeof(float), typeof(double) }),
                new Tuple<Type, Type[]>(typeof(short), new Type[] { typeof(uint), typeof(int), typeof(ulong), typeof(long), typeof(BigInteger), typeof(BigRational), typeof(float), typeof(double) }),
                new Tuple<Type, Type[]>(typeof(uint), new Type[] { typeof(int), typeof(ulong), typeof(long), typeof(BigInteger), typeof(BigRational), typeof(float), typeof(double) }),
                new Tuple<Type, Type[]>(typeof(int), new Type[] { typeof(ulong), typeof(long), typeof(BigInteger), typeof(BigRational), typeof(float), typeof(double) }),
                new Tuple<Type, Type[]>(typeof(ulong), new Type[] { typeof(long), typeof(BigInteger), typeof(BigRational), typeof(float), typeof(double) }),
                new Tuple<Type, Type[]>(typeof(long), new Type[] { typeof(BigInteger), typeof(BigRational), typeof(float), typeof(double) }),
                new Tuple<Type, Type[]>(typeof(BigInteger), new Type[] { typeof(BigRational), typeof(float), typeof(double) }),
                new Tuple<Type, Type[]>(typeof(BigRational), new Type[] { typeof(float), typeof(double) }),
                new Tuple<Type, Type[]>(typeof(float), new Type[] { typeof(double) }),
                new Tuple<Type, Type[]>(typeof(double), new Type[] { typeof(float) }),

            };

            private static int Score(Type t)
            {
                if (t == typeof(bool)) return 1;
                if (t == typeof(char)) return 2;
                if (t == typeof(Symbol)) return 3;
                
                if (t == typeof(string)) return 4;
                if (t == typeof(SchemeString)) return 4;

                if (t == typeof(Deque<object>)) return 5;

                if (t == typeof(ExprObjModel.ObjectSystem.Message<object>)) return 6;
                if (t == typeof(ExprObjModel.ObjectSystem.Signature)) return 7;

                if (t == typeof(System.Net.IPAddress)) return 8;
                if (t == typeof(System.Net.IPEndPoint)) return 9;

                if (t == typeof(Guid)) return 10;

                if (t == typeof(byte)) return 100;
                if (t == typeof(sbyte)) return 101;
                if (t == typeof(ushort)) return 102;
                if (t == typeof(short)) return 103;
                if (t == typeof(uint)) return 104;
                if (t == typeof(int)) return 105;
                if (t == typeof(UIntPtr)) return 106;
                if (t == typeof(IntPtr)) return 107;
                if (t == typeof(ulong)) return 108;
                if (t == typeof(long)) return 109;
                if (t == typeof(BigInteger)) return 110;
                if (t == typeof(BigRational)) return 111;
                if (t == typeof(float)) return 112;
                if (t == typeof(double)) return 112;

                if (t == typeof(IProcedure)) return 201;
                if (typeof(IProcedure).IsAssignableFrom(t)) return 200;

                if (t == typeof(IDisposable)) return 301;
                if (typeof(IDisposable).IsAssignableFrom(t)) return 300;

                if (t == typeof(object)) return 1000;
                return 999; // specific object but not "object"
            }

            private static bool LessThan(Type a, Type b)
            {
                if (b == typeof(object)) return true;

                if (a == typeof(object)) return false;

                if (b == typeof(IDisposable) && typeof(IDisposable).IsAssignableFrom(a)) return true;

                if (a == typeof(IDisposable) && typeof(IDisposable).IsAssignableFrom(b)) return false;

                if (b == typeof(IHashable) && typeof(IHashable).IsAssignableFrom(a)) return true;

                if (a == typeof(IHashable) && typeof(IHashable).IsAssignableFrom(b)) return false;


            }
        }

        private static ParamInfo GetParamInfo(MethodBase b)
        {
            List<Type> sParams = new List<Type>();
            bool extras = false;

            if (b is ConstructorInfo)
            {
                sParams.Add(b.DeclaringType);
            }

            bool globalState = false;
            foreach (ParameterInfo pi in b.GetParameters())
            {
                if (pi.ParameterType == typeof(IGlobalState))
                {
                    if (globalState) throw new Exception("IGlobalState can appear only once in parameter list");
                    globalState = true;
                }
                else
                {
                    if (pi.GetCustomAttributes<
                }
            }

        }

        private static IEnumerable<MethodBase> SortOverloads(IEnumerable<MethodBase> methods)
        {
            return Utils.TopologicalSort
            (
                methods,
                Utils.MakeChildrenGetter
                (
                    methods,
                    Utils.CacheMapLessThan
                    (
                        new Func<MethodBase, ParamInfo>(GetParamInfo),
                        new Func<ParamInfo, ParamInfo, bool>(ParamInfo.LessThan)
                    )
                )
            );
        }
#endif
    }
}
