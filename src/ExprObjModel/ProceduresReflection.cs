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
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using BigMath;

namespace ExprObjModel.Procedures
{
    static partial class ProxyDiscovery
    {
        [SchemeFunction("type")]
        public static Type GetType(string str)
        {
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Module m in a.GetModules())
                {
                    foreach (Type t2 in m.GetTypes())
                    {
                        if (string.CompareOrdinal(t2.FullName, str) == 0) return t2;
                    }
                }
            }
            throw new Exception("Type not found");
        }

        [SchemeFunction("is-open-generic-type?")]
        public static bool IsOpenGeneric(Type t)
        {
            return t.IsGenericTypeDefinition;
        }

        [SchemeFunction("is-open-generic-method?")]
        public static bool IsOpenGeneric(MethodInfo m)
        {
            return m.IsGenericMethodDefinition;
        }

        [SchemeFunction("count-generic-type-parameters")]
        public static int CountGenericTypeParameters(Type t)
        {
            return t.GetGenericArguments().Where(x => x.IsGenericParameter).Count();
        }

        [SchemeFunction("count-generic-method-parameters")]
        public static int CountGenericMethodParameters(MethodInfo m)
        {
            return m.GetGenericArguments().Where(x => x.IsGenericParameter).Count();
        }

        [SchemeFunction("is-enum?")]
        public static bool IsEnum(Type t)
        {
            return t.IsEnum;
        }

        [SchemeFunction("is-enum-flags?")]
        public static bool IsEnumFlags(Type t)
        {
            return t.IsEnum && (t.GetCustomAttributes<FlagsAttribute>(false).Length > 0);
        }

        [SchemeFunction("field")]
        public static FieldInfo GetField(Type t, string name)
        {
            FieldInfo fi = t.GetField(name);
            if (fi == null) throw new SchemeRuntimeException("Field not found");
            return fi;
        }

        [SchemeFunction("is-static-method?")]
        public static bool IsStaticMethod(MethodBase mb)
        {
            if (mb is ConstructorInfo) return true;
            return ((MethodInfo)mb).IsStatic;
        }

        [SchemeFunction("is-static-field?")]
        public static bool IsStaticField(FieldInfo fi)
        {
            return fi.IsStatic;
        }

        [SchemeFunction("static-field-ref")]
        public static object StaticFieldRef(FieldInfo fi)
        {
            if (!fi.IsStatic) throw new SchemeRuntimeException("Field is not static");
            object obj = fi.GetValue(null);
            return Unmarshal(obj);
        }

        [SchemeFunction("static-field-set!")]
        public static void StaticFieldSet(FieldInfo fi, object val)
        {
            if (!fi.IsStatic) throw new SchemeRuntimeException("Field is not static");
            object obj = Marshal(val, fi.FieldType);
            fi.SetValue(null, obj);
        }

        [SchemeFunction("field-ref")]
        public static object FieldRef(object obj1, FieldInfo fi)
        {
            if (!fi.IsStatic) throw new SchemeRuntimeException("Field is not static");
            object obj = fi.GetValue(obj1);
            return Unmarshal(obj);
        }

        [SchemeFunction("field-set!")]
        public static void FieldSet(object obj1, FieldInfo fi, object val)
        {
            if (!fi.IsStatic) throw new SchemeRuntimeException("Field is not static");
            object obj = Marshal(val, fi.FieldType);
            fi.SetValue(obj1, obj);
        }

        [SchemeFunction("get-enum-values")]
        public static object GetEnumValues(Type t)
        {
            if (!(t.IsEnum)) throw new SchemeRuntimeException("Operand must be an enum type");

            List<Tuple<string, BigInteger>> list = new List<Tuple<string, BigInteger>>();
            foreach (string str in Enum.GetNames(t))
            {
                list.Add(new Tuple<string, BigInteger>(str, EnumToInteger(Enum.Parse(t, str))));
            }

            object r = SpecialValue.EMPTY_LIST;
            foreach (Tuple<string, BigInteger> i in list.OrderBy(x => x.Item1))
            {
                r = new ConsCell(new ConsCell(new Symbol(i.Item1), i.Item2), r);
            }

            ConsCell.Reverse(ref r);

            return r;
        }

        [SchemeFunction("enum-value")]
        public static BigInteger EnumValue(Type t, Symbol s)
        {
            try
            {
                return EnumToInteger(Enum.Parse(t, s.Name));
            }
            catch (FormatException)
            {
                throw new SchemeRuntimeException("Value does not exist");
            }
        }

        [SchemeFunction("enum->integer")]
        public static BigInteger EnumToInteger(object obj)
        {
            if (!(obj is Enum)) throw new SchemeRuntimeException("Operand must be an enumeration");

            Type tEnum = obj.GetType();
            Type tUnderlying = Enum.GetUnderlyingType(tEnum);

            object o2 = Convert.ChangeType(obj, tUnderlying);

            if (o2 is char)
            {
                o2 = Convert.ChangeType(o2, typeof(ushort));
            }

            if (o2 is IntPtr)
            {
                o2 = Convert.ChangeType(o2, typeof(long));
            }

            if (o2 is UIntPtr)
            {
                o2 = Convert.ChangeType(o2, typeof(ulong));
            }

            if (o2 is bool)
            {
                bool b = (bool)o2;
                if (b) return BigInteger.One;
                else return BigInteger.Zero;
            }
            else if (o2 is byte)
            {
                byte b = (byte)o2;
                return BigInteger.FromByte(b);
            }
            else if (o2 is short)
            {
                short s = (short)o2;
                return BigInteger.FromInt16(s);
            }
            else if (o2 is int)
            {
                int i = (int)o2;
                return BigInteger.FromInt32(i);
            }
            else if (o2 is long)
            {
                long l = (long)o2;
                return BigInteger.FromInt64(l);
            }
            else if (o2 is sbyte)
            {
                sbyte sb = (sbyte)o2;
                return BigInteger.FromSByte(sb);
            }
            else if (o2 is ushort)
            {
                ushort us = (ushort)o2;
                return BigInteger.FromUInt16(us);
            }
            else if (o2 is uint)
            {
                uint ui = (uint)o2;
                return BigInteger.FromUInt32(ui);
            }
            else if (o2 is ulong)
            {
                ulong ul = (ulong)o2;
                return BigInteger.FromUInt64(ul);
            }

            throw new SchemeRuntimeException("Unknown underlying type");
        }

        [SchemeFunction("integer->enum")]
        public static object IntegerToEnum(Type eType, BigInteger i)
        {
            if (!(eType.IsEnum)) throw new SchemeRuntimeException("Type must be an enum");

            Type tUnderlying = Enum.GetUnderlyingType(eType);

            if (tUnderlying == typeof(bool))
            {
                return Convert.ChangeType(i.IsOdd, eType);
            }
            else if (tUnderlying == typeof(byte))
            {
                return Enum.ToObject(eType, i.GetByteValue(OverflowBehavior.Wraparound));
            }
            else if (tUnderlying == typeof(short))
            {
                return Enum.ToObject(eType, i.GetInt16Value(OverflowBehavior.Wraparound));
            }
            else if (tUnderlying == typeof(int))
            {
                return Enum.ToObject(eType, i.GetInt32Value(OverflowBehavior.Wraparound));
            }
            else if (tUnderlying == typeof(long))
            {
                return Enum.ToObject(eType, i.GetInt64Value(OverflowBehavior.Wraparound));
            }
            else if (tUnderlying == typeof(sbyte))
            {
                return Enum.ToObject(eType, i.GetSByteValue(OverflowBehavior.Wraparound));
            }
            else if (tUnderlying == typeof(ushort))
            {
                return Enum.ToObject(eType, i.GetUInt16Value(OverflowBehavior.Wraparound));
            }
            else if (tUnderlying == typeof(uint))
            {
                return Enum.ToObject(eType, i.GetUInt32Value(OverflowBehavior.Wraparound));
            }
            else if (tUnderlying == typeof(ulong))
            {
                return Enum.ToObject(eType, i.GetUInt64Value(OverflowBehavior.Wraparound));
            }
            else if (tUnderlying == typeof(IntPtr))
            {
                return Enum.ToObject(eType, i.GetInt64Value(OverflowBehavior.Wraparound));
            }
            else if (tUnderlying == typeof(UIntPtr))
            {
                return Enum.ToObject(eType, i.GetUInt64Value(OverflowBehavior.Wraparound));
            }
            else if (tUnderlying == typeof(char))
            {
                return Enum.ToObject(eType, i.GetUInt16Value(OverflowBehavior.Wraparound));
            }
            else throw new SchemeRuntimeException("Unknown underlying type of enum");
        }

        public static object Marshal(object obj, Type t)
        {
            if (t == typeof(string))
            {
                if (obj is SchemeString)
                {
                    return ((SchemeString)obj).TheString;
                }
                else throw new SchemeRuntimeException("Type mismatch");
            }
            else if (t == typeof(byte))
            {
                if (obj is BigInteger)
                {
                    BigInteger b = (BigInteger)obj;
                    return b.GetByteValue(OverflowBehavior.ThrowException);
                }
                else throw new SchemeRuntimeException("Type mismatch");
            }
            else if (t == typeof(short))
            {
                if (obj is BigInteger)
                {
                    BigInteger b = (BigInteger)obj;
                    return b.GetInt16Value(OverflowBehavior.ThrowException);
                }
                else throw new SchemeRuntimeException("Type mismatch");
            }
            else if (t == typeof(int))
            {
                if (obj is BigInteger)
                {
                    BigInteger b = (BigInteger)obj;
                    return b.GetInt32Value(OverflowBehavior.ThrowException);
                }
                else throw new SchemeRuntimeException("Type mismatch");
            }
            else if (t == typeof(long))
            {
                if (obj is BigInteger)
                {
                    BigInteger b = (BigInteger)obj;
                    return b.GetInt64Value(OverflowBehavior.ThrowException);
                }
                else throw new SchemeRuntimeException("Type mismatch");
            }
            else if (t == typeof(sbyte))
            {
                if (obj is BigInteger)
                {
                    BigInteger b = (BigInteger)obj;
                    return b.GetSByteValue(OverflowBehavior.ThrowException);
                }
                else throw new SchemeRuntimeException("Type mismatch");
            }
            else if (t == typeof(ushort))
            {
                if (obj is BigInteger)
                {
                    BigInteger b = (BigInteger)obj;
                    return b.GetUInt16Value(OverflowBehavior.ThrowException);
                }
                else throw new SchemeRuntimeException("Type mismatch");
            }
            else if (t == typeof(uint))
            {
                if (obj is BigInteger)
                {
                    BigInteger b = (BigInteger)obj;
                    return b.GetUInt32Value(OverflowBehavior.ThrowException);
                }
                else throw new SchemeRuntimeException("Type mismatch");
            }
            else if (t == typeof(ulong))
            {
                if (obj is BigInteger)
                {
                    BigInteger b = (BigInteger)obj;
                    return b.GetUInt64Value(OverflowBehavior.ThrowException);
                }
                else throw new SchemeRuntimeException("Type mismatch");
            }
            else if (t == typeof(float))
            {
                if (obj is double)
                {
                    return (float)(double)obj;
                }
                else if (obj is BigRational)
                {
                    return ((BigRational)obj).GetSingleValue(RoundingMode.Round);
                }
                else if (obj is BigInteger)
                {
                    return new BigRational((BigInteger)obj, BigInteger.One).GetSingleValue(RoundingMode.Round);
                }
                else throw new SchemeRuntimeException("Type mismatch");
            }
            else if (t == typeof(double))
            {
                if (obj is double)
                {
                    return obj;
                }
                else if (obj is BigRational)
                {
                    return ((BigRational)obj).GetDoubleValue(RoundingMode.Round);
                }
                else if (obj is BigInteger)
                {
                    return new BigRational((BigInteger)obj, BigInteger.One).GetDoubleValue(RoundingMode.Round);
                }
                else throw new SchemeRuntimeException("Type mismatch");
            }
            else if (t.IsAssignableFrom(obj.GetType()))
            {
                return obj;
            }
            else throw new SchemeRuntimeException("Type mismatch");
        }

        public static object Unmarshal(object obj)
        {
            Type t = obj.GetType();
            if (t == typeof(string))
            {
                return new SchemeString((string)obj);
            }
            else if (t == typeof(byte))
            {
                return BigInteger.FromByte((byte)obj);
            }
            else if (t == typeof(short))
            {
                return BigInteger.FromInt16((short)obj);
            }
            else if (t == typeof(int))
            {
                return BigInteger.FromInt32((int)obj);
            }
            else if (t == typeof(long))
            {
                return BigInteger.FromInt64((long)obj);
            }
            else if (t == typeof(sbyte))
            {
                return BigInteger.FromSByte((sbyte)obj);
            }
            else if (t == typeof(ushort))
            {
                return BigInteger.FromUInt16((ushort)obj);
            }
            else if (t == typeof(uint))
            {
                return BigInteger.FromUInt32((uint)obj);
            }
            else if (t == typeof(ulong))
            {
                return BigInteger.FromUInt64((ulong)obj);
            }
            else if (t == typeof(byte[]))
            {
                return new SchemeByteArray((byte[])obj, DigitOrder.LBLA);
            }
            else if (t == typeof(float))
            {
                return (double)(float)obj;
            }
            else
            {
                return obj;
            }
        }
    }

    [SchemeSingleton("generic-type")]
    public class GenericType : IProcedure
    {
        public GenericType() { }

        public int Arity { get { return 2; } }

        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            FList<object> rest = argList;
            Type g = (Type)(rest.Head);
            rest = rest.Tail;

            List<Type> t = new List<Type>();
            while (rest != null)
            {
                t.Add((Type)(rest.Head));
                rest = rest.Tail;
            }

            Type[] t1 = t.ToArray();

            Type t0 = g.MakeGenericType(t1);

            return new RunnableReturn(k, t0);
        }
    }

    [SchemeSingleton("generic-method")]
    public class GenericMethod : IProcedure
    {
        public GenericMethod() { }

        public int Arity { get { return 2; } }

        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            FList<object> rest = argList;
            MethodInfo g = (MethodInfo)(rest.Head);
            rest = rest.Tail;

            List<Type> t = new List<Type>();
            while (rest != null)
            {
                t.Add((Type)(rest.Head));
                rest = rest.Tail;
            }

            Type[] t1 = t.ToArray();

            MethodInfo t0 = g.MakeGenericMethod(t1);

            return new RunnableReturn(k, t0);
        }
    }

    [SchemeSingleton("constructor")]
    public class GetConstructor : IProcedure
    {
        public GetConstructor() { }

        public int Arity { get { return 1; } }

        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            FList<object> rest = argList;
            Type g = (Type)(rest.Head);
            rest = rest.Tail;

            List<Type> t = new List<Type>();
            while (rest != null)
            {
                t.Add((Type)(rest.Head));
                rest = rest.Tail;
            }

            Type[] t1 = t.ToArray();

            ConstructorInfo ci = g.GetConstructor(t1);

            if (ci == null) throw new SchemeRuntimeException("Constructor not found");

            return new RunnableReturn(k, ci);
        }
    }

    [SchemeSingleton("method")]
    public class GetMethod : IProcedure
    {
        public GetMethod() { }

        public int Arity { get { return 2; } }

        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            FList<object> rest = argList;
            Type g = (Type)(rest.Head);
            rest = rest.Tail;

            SchemeString methodName = (SchemeString)rest.Head;
            rest = rest.Tail;

            List<Type> t = new List<Type>();
            while (rest != null)
            {
                t.Add((Type)(rest.Head));
                rest = rest.Tail;
            }

            MethodInfo mi = g.GetMethod(methodName.TheString, t.ToArray());

            if (mi == null) throw new SchemeRuntimeException("Method not found");

            return new RunnableReturn(k, mi);
        }
    }

    [SchemeSingleton("invoke")]
    public class ReflectionInvoke : IProcedure
    {
        public ReflectionInvoke() { }

        public int Arity { get { return 2; } }

        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            FList<object> rest = argList;

            object obj = rest.Head;
            rest = rest.Tail;

            if (rest.Head is ConstructorInfo) throw new SchemeRuntimeException("Use static-invoke for constructors");
            MethodInfo mi = (MethodInfo)(rest.Head);
            if (mi.IsStatic) throw new SchemeRuntimeException("Use static-invoke for static methods");
            rest = rest.Tail;

            ParameterInfo[] pi = mi.GetParameters();
            int index = 0;
            List<object> t = new List<object>();
            while (rest != null)
            {
                if (index >= pi.Length) throw new SchemeRuntimeException("Too many arguments");
                t.Add(ProxyDiscovery.Marshal(rest.Head, pi[index].ParameterType));
                rest = rest.Tail;
                ++index;
            }

            if (index != pi.Length) throw new SchemeRuntimeException("Too few arguments");

            object result = mi.Invoke(obj, t.ToArray());

            if (mi.ReturnType == typeof(void))
            {
                return new RunnableReturn(k, SpecialValue.UNSPECIFIED);
            }
            else
            {
                return new RunnableReturn(k, ProxyDiscovery.Unmarshal(result));
            }
        }
    }

    [SchemeSingleton("static-invoke")]
    public class ReflectionStaticInvoke : IProcedure
    {
        public ReflectionStaticInvoke() { }

        public int Arity { get { return 1; } }

        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            FList<object> rest = argList;

            MethodBase mi = (MethodBase)(rest.Head);
            if (mi is MethodInfo && !(mi.IsStatic)) throw new SchemeRuntimeException("Use invoke for instance methods");
            rest = rest.Tail;

            ParameterInfo[] pi = mi.GetParameters();
            int index = 0;
            List<object> t = new List<object>();
            while (rest != null)
            {
                if (index >= pi.Length) throw new SchemeRuntimeException("Too many arguments");
                t.Add(ProxyDiscovery.Marshal(rest.Head, pi[index].ParameterType));
                rest = rest.Tail;
                ++index;
            }

            if (index != pi.Length) throw new SchemeRuntimeException("Too few arguments");

            object result = (mi is ConstructorInfo) ? ((ConstructorInfo)mi).Invoke(t.ToArray()) : ((MethodInfo)mi).Invoke(null, t.ToArray());

            if ((mi is MethodInfo) && (((MethodInfo)mi).ReturnType == typeof(void)))
            {
                return new RunnableReturn(k, SpecialValue.UNSPECIFIED);
            }
            else
            {
                return new RunnableReturn(k, ProxyDiscovery.Unmarshal(result));
            }
        }
    }
}