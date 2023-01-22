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
using BigMath;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ControlledWindowLib;

namespace ExprObjModel
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class DescendantsWithPatternsAttribute : Attribute
    {
        public DescendantsWithPatternsAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class PatternAttribute : Attribute
    {
        private string pattern;

        public PatternAttribute(string pattern)
        {
            this.pattern = pattern;
        }

        public string Pattern { get { return pattern; } }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class BindAttribute : Attribute
    {
        private string var;

        public BindAttribute(string var)
        {
            this.var = var;
        }

        public string Var { get { return var; } }
    }

    /// <summary>
    /// For use with enumerations
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
    public class SymbolAttribute : Attribute
    {
        private string sym;

        public SymbolAttribute(string sym)
        {
            this.sym = sym;
        }

        public string Symbol { get { return sym; } }
    }

    public abstract class Option<T>
    {
    }

    public class Some<T> : Option<T>
    {
        public T value;
    }

    public class None<T> : Option<T>
    {
    }

    /// <summary>
    /// Single-Assignment Box
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SaBox<T>
    {
        private T theValue;
        private bool hasValue;

        public SaBox()
        {
            theValue = default(T);
            hasValue = false;
        }

        public SaBox(T initValue)
        {
            theValue = initValue;
            hasValue = true;
        }

        public T Value
        {
            get
            {
                if (hasValue) return theValue;
                else throw new InvalidOperationException("Uninitialized Box");
            }
            set
            {
                if (hasValue) throw new InvalidOperationException("Single assignment violation");
                theValue = value;
                hasValue = true;
            }
        }

        public bool HasValue { get { return hasValue; } }
    }

    public static partial class Utils
    {
        public static Type[] GetDescendants(Type t)
        {
            List<Type> l = new List<Type>();
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Module m in a.GetModules())
                {
                    foreach (Type t2 in m.GetTypes())
                    {
                        if (t.IsAssignableFrom(t2) && t != t2) l.Add(t2);
                    }
                }
            }
            return l.ToArray();
        }

        private static Option<object> ParseDouble(object input)
        {
            if (input is double)
            {
                return new Some<object>() { value = input };
            }
            else if (input is BigRational)
            {
                return new Some<object>() { value = ((BigRational)input).GetDoubleValue(RoundingMode.Round) };
            }
            else if (input is BigInteger)
            {
                BigRational br = new BigRational((BigInteger)input, BigInteger.One);
                return new Some<object>() { value = ((BigRational)input).GetDoubleValue(RoundingMode.Round) };
            }
            return new None<object>();
        }

        private static Option<object> ParseSingle(object input)
        {
            if (input is double)
            {
                return new Some<object>() { value = (float)(double)input };
            }
            else if (input is BigRational)
            {
                return new Some<object>() { value = ((BigRational)input).GetSingleValue(RoundingMode.Round) };
            }
            else if (input is BigInteger)
            {
                BigRational br = new BigRational((BigInteger)input, BigInteger.One);
                return new Some<object>() { value = br.GetSingleValue(RoundingMode.Round) };
            }
            return new None<object>();
        }

        private static Option<object> ParseChar(object input)
        {
            if (input is char)
            {
                char bInput = (char)input;
                return new Some<object>() { value = bInput };
            }
            return new None<object>();
        }
    
        private static Option<object> ParseString(object input)
        {
            if (input is SchemeString)
            {
                SchemeString sInput = (SchemeString)input;
                return new Some<object>() { value = sInput.TheString };
            }
            return new None<object>();
        }

        private static Option<object> ParseBool(object input)
        {
            if (input is bool)
            {
                return new Some<object>() { value = input };
            }
            return new None<object>();
        }

        private static Option<object> ParseSymbol(object input)
        {
            if (input is Symbol)
            {
                return new Some<object>() { value = input };
            }
            return new None<object>();
        }

        private static Option<object> ParseType(object input)
        {
            if (input is Type)
            {
                return new Some<object>() { value = input };
            }
            return new None<object>();
        }

        private static Option<object> ParseMethodBase(object input)
        {
            if (input is MethodBase)
            {
                return new Some<object>() { value = input };
            }
            return new None<object>();
        }

        private static Option<object> ParseObject(object input)
        {
            return new Some<object>() { value = input };
        }

        private static Func<object, Option<object>> MakeParseEnum(Type enumType)
        {
            System.Diagnostics.Debug.Assert(enumType.IsEnum);

            Dictionary<string, object> dict = new Dictionary<string, object>();

            foreach (FieldInfo fi in enumType.GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                object enumValue = fi.GetValue(null);
                System.Diagnostics.Debug.Assert(enumValue != null && enumValue.GetType() == enumType);
                SymbolAttribute[] sa = fi.GetCustomAttributes<SymbolAttribute>(false);
                foreach (SymbolAttribute s in sa)
                {
                    dict.Add(s.Symbol, enumValue);
                }
            }

            Func<object, Option<object>> func = delegate(object input)
            {
                if (input is Symbol)
                {
                    Symbol s = (Symbol)input;
                    if (s.IsInterned)
                    {
                        if (dict.ContainsKey(s.Name))
                        {
                            return new Some<object>() { value = dict[s.Name] };
                        }
                    }
                }
                return new None<object>();
            };

            return func;
        }

#if false
        private static int recLevel = 0;
        private static int lineCount = 0;

        private static Func<object, Option<object>> Stamp(Type t, Func<object, Option<object>> unstamped)
        {
            Func<object, Option<object>> func2 = delegate(object obj)
            {
                ++recLevel;
                //Console.WriteLine(new string(' ', recLevel * 2) + "Entering " + t.FullName);
                //++lineCount;
                Option<object> result = unstamped(obj);
                if (result is Some<object>)
                {
                    Console.WriteLine(new string(' ', recLevel * 2) + "Exiting " + t.FullName + ": " + ((result is Some<object>) ? "Succeeded" : "Failed"));
                    ++lineCount;
                    if (lineCount >= 40)
                    {
                        Console.WriteLine("Press [Enter]...");
                        Console.ReadLine();
                        lineCount = 0;
                    }
                }
                --recLevel;
                return result;
            };
            return func2;
        }
#endif

        public static Func<object, Option<object>> MakeParser(Type t)
        {
            Dictionary<Type, SaBox<Func<object, Option<object>>>> dict = new Dictionary<Type,SaBox<Func<object,Option<object>>>>();
            Queue<Type> parsersToMake = new Queue<Type>();

            parsersToMake.Enqueue(t);

            Func<Type, SaBox<Func<object, Option<object>>>> getBox = delegate(Type t2)
            {
                if (dict.ContainsKey(t2))
                {
                    //System.Diagnostics.Debug.WriteLine("getBox, found, type = " + t2.FullName);
                    return dict[t2];
                }
                else
                {
                    //System.Diagnostics.Debug.WriteLine("getBox, not found, type = " + t2.FullName);
                    SaBox<Func<object, Option<object>>> box = new SaBox<Func<object, Option<object>>>();
                    dict.Add(t2, box);
                    parsersToMake.Enqueue(t2);
                    return box;
                }
            };

            while (parsersToMake.Count > 0)
            {
                Type t1 = parsersToMake.Dequeue();
                ForceMakeParser(t1, getBox);
            }

#if false
            foreach (KeyValuePair<Type, Box<Func<object, Option<object>>>> kvp in dict)
            {
                dict[kvp.Key].Value = Stamp(kvp.Key, kvp.Value.Value);
            }
#endif

            return dict[t].Value;
        }

        private static void ForceMakeParser(Type t, Func<Type, SaBox<Func<object, Option<object>>>> getBox)
        {
            //System.Diagnostics.Debug.WriteLine("ForceMakeParser, type = " + t.FullName);
            if (getBox(t).HasValue) return;

            if (t == typeof(byte))
            {
                getBox(t).Value = new Func<object, Option<object>>(ParseByte);
            }
            else if (t == typeof(short))
            {
                getBox(t).Value = new Func<object, Option<object>>(ParseInt16);
            }
            else if (t == typeof(int))
            {
                getBox(t).Value = new Func<object, Option<object>>(ParseInt32);
            }
            else if (t == typeof(long))
            {
                getBox(t).Value = new Func<object, Option<object>>(ParseInt64);
            }
            else if (t == typeof(sbyte))
            {
                getBox(t).Value = new Func<object, Option<object>>(ParseSByte);
            }
            else if (t == typeof(ushort))
            {
                getBox(t).Value = new Func<object, Option<object>>(ParseUInt16);
            }
            else if (t == typeof(uint))
            {
                getBox(t).Value = new Func<object, Option<object>>(ParseUInt32);
            }
            else if (t == typeof(ulong))
            {
                getBox(t).Value = new Func<object, Option<object>>(ParseUInt64);
            }
            else if (t == typeof(char))
            {
                getBox(t).Value = new Func<object, Option<object>>(ParseChar);
            }
            else if (t == typeof(string))
            {
                getBox(t).Value = new Func<object, Option<object>>(ParseString);
            }
            else if (t == typeof(bool))
            {
                getBox(t).Value = new Func<object, Option<object>>(ParseBool);
            }
            else if (t == typeof(double))
            {
                getBox(t).Value = new Func<object, Option<object>>(ParseDouble);
            }
            else if (t == typeof(float))
            {
                getBox(t).Value = new Func<object, Option<object>>(ParseSingle);
            }
            else if (t == typeof(Symbol))
            {
                getBox(t).Value = new Func<object, Option<object>>(ParseSymbol);
            }
            else if (t == typeof(Type))
            {
                getBox(t).Value = new Func<object, Option<object>>(ParseType);
            }
            else if (t == typeof(MethodBase))
            {
                getBox(t).Value = new Func<object, Option<object>>(ParseMethodBase);
            }
            else if (t == typeof(object))
            {
                getBox(t).Value = new Func<object, Option<object>>(ParseObject);
            }
            else if (t.IsEnum)
            {
                getBox(t).Value = MakeParseEnum(t);
            }
            else
            {
                PatternAttribute[] p = t.GetCustomAttributes<PatternAttribute>(false);

                if (p.Length == 0)
                {
                    DescendantsWithPatternsAttribute[] d = t.GetCustomAttributes<DescendantsWithPatternsAttribute>(false);
                    if (d.Length == 0)
                    {
                        throw new Exception("Type does not have a [Pattern] or [DescendantsWithPatterns] attribute");
                    }
                    else
                    {
                        Type[] tArr = GetDescendants(t).OrderBy(u => u.FullName).ToArray();
                        SaBox<Func<object, Option<object>>>[] pArr = tArr.Select(x1 => getBox(x1)).ToArray();

                        getBox(t).Value = MakeParseAlternatives(pArr, getBox);
                    }
                }
                else
                {
                    object pattern = SchemeDataReader.ReadItem(p[0].Pattern);

                    Dictionary<Symbol, FieldInfo> patternEnv = new Dictionary<Symbol, FieldInfo>();
                    foreach (FieldInfo f in t.GetFields())
                    {
                        BindAttribute[] b = f.GetCustomAttributes<BindAttribute>(false);
                        if (b.Length == 1)
                        {
                            Symbol s = new Symbol(b[0].Var);
                            patternEnv.Add(s, f);
                        }
                    }

                    ConstructorInfo ci = t.GetConstructor(new Type[0]);
                    if (ci == null) throw new Exception("Type being parsed must have a default constructor");

                    Func<object, object, bool> match = MakeMatch(pattern, patternEnv, getBox);

                    Func<object, Option<object>> parser = delegate(object datum)
                    {
                        object result = ci.Invoke(new object[] { });
                        bool success = match(datum, result);
                        if (success) return new Some<object>() { value = result };
                        else return new None<object>();
                    };

                    getBox(t).Value = parser;
                }
            }
        }

        private static Func<object, object, bool> MakeMatch(object pattern, Dictionary<Symbol, FieldInfo> patternEnv, Func<Type, SaBox<Func<object, Option<object>>>> getBox)
        {
            if (pattern is Deque<object>)
            {
                return MakeVectorMatch((Deque<object>)pattern, patternEnv, getBox);
            }
            else if (pattern is ConsCell)
            {
                return MakeConsCellMatch((ConsCell)pattern, patternEnv, getBox);
            }
            else if (pattern is SpecialValue)
            {
                return MakeSpecialValueMatch((SpecialValue)pattern);
            }
            else if (pattern is Symbol)
            {
                Symbol sPattern = (Symbol)pattern;
                if (patternEnv.ContainsKey(sPattern))
                {
                    return MakeFieldStore(patternEnv[sPattern], getBox);
                }
                else
                {
                    return MakeSymbolMatch(sPattern);
                }
            }
            else if (pattern is bool)
            {
                return MakeBoolMatch((bool)pattern);
            }
            else throw new Exception("Unknown pattern type");
        }

        private static Func<object, object, bool> MakeVectorMatch(Deque<object> pattern, Dictionary<Symbol, FieldInfo> patternEnv, Func<Type, SaBox<Func<object, Option<object>>>> getBox)
        {
            Func<object, object, bool>[] matchElement = pattern.Select(x => MakeMatch(x, patternEnv, getBox)).ToArray();

            Func<object, object, bool> match = delegate(object input, object dest)
            {
                if (!(input is Deque<object>)) return false;
                Deque<object> dInput = (Deque<object>)input;
                if (dInput.Count != matchElement.Length) return false;
                int iEnd = dInput.Count;
                for (int i = 0; i < iEnd; ++i)
                {
                    if (!(matchElement[i](dInput[i], dest))) return false;
                }
                return true;
            };

            return match;
        }

        private static Func<object, object, bool> MakeConsCellMatch(ConsCell pattern, Dictionary<Symbol, FieldInfo> patternEnv, Func<Type, SaBox<Func<object, Option<object>>>> getBox)
        {
            Func<object, object, bool> matchCar = MakeMatch(pattern.car, patternEnv, getBox);
            Func<object, object, bool> matchCdr = MakeMatch(pattern.cdr, patternEnv, getBox);

            Func<object, object, bool> match = delegate(object input, object dest)
            {
                if (!(input is ConsCell)) return false;
                ConsCell ccInput = (ConsCell)input;
                if (!(matchCar(ccInput.car, dest))) return false;
                if (!(matchCdr(ccInput.cdr, dest))) return false;
                return true;
            };

            return match;
        }

        private static Func<object, object, bool> MakeSymbolMatch(Symbol s)
        {
            Func<object, object, bool> symbolMatch = delegate(object input, object dest)
            {
                if (!(input is Symbol)) return false;
                Symbol sInput = (Symbol)input;
                return (sInput == s);
            };
            return symbolMatch;
        }

        private static Func<object, object, bool> MakeBoolMatch(bool b)
        {
            Func<object, object, bool> boolMatch = delegate(object input, object dest)
            {
                if (!(input is bool)) return false;
                bool bInput = (bool)input;
                return (bInput == b);
            };
            return boolMatch;
        }

        private static Func<object, object, bool> MakeSpecialValueMatch(SpecialValue sv)
        {
            Func<object, object, bool> specialValueMatch = delegate(object input, object dest)
            {
                if (!(input is SpecialValue)) return false;
                SpecialValue svInput = (SpecialValue)input;
                return (svInput == sv);
            };
            return specialValueMatch;
        }

        public static IEnumerable<Tuple<T, V>> MapSecond<T, U, V>(IEnumerable<Tuple<T, U>> sequence, Func<U, V> func)
        {
            return sequence.Select(x => new Tuple<T, V>(x.Item1, func(x.Item2)));
        }

        public static U CastFunc<T, U>(T t) where U : T
        {
            return (U)t;
        }

        public static Delegate MakeCastFunc(Type t, Type u)
        {
            MethodInfo mi = typeof(Utils).GetMethod("CastFunc").MakeGenericMethod(new Type[] { t, u });
            Type delegateType = typeof(Func<,>).MakeGenericType(new Type[] { t, u });
            return Delegate.CreateDelegate(delegateType, mi);
        }

        private static Func<object, object, bool> MakeFieldStore(FieldInfo f, Func<Type, SaBox<Func<object, Option<object>>>> getBox)
        {
            if (f.FieldType.IsGenericType && f.FieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                #region handle List<T>

                Type element = f.FieldType.GetGenericArguments()[0];

                SaBox<Func<object, Option<object>>> elParser = getBox(element);

                Func<object, object, bool> fieldStore = delegate(object input, object dest)
                {
                    if (ConsCell.IsList(input))
                    {
                        object i2 = input;
                        object result = f.FieldType.GetConstructor(Type.EmptyTypes).Invoke(new object[] { });
                        MethodInfo resultAdd = f.FieldType.GetMethod("Add", new Type[] { element });

                        while (!(ConsCell.IsEmptyList(i2)))
                        {
                            object carInput = ((ConsCell)i2).car;
                            object cdrInput = ((ConsCell)i2).cdr;

                            // if elParser.Value ends up null in the line below,
                            // it might be because you have a [DescendantsWithPatterns]
                            // with no descendants...
                            Option<object> parseResult = elParser.Value(carInput); 
                            if (parseResult is Some<object>)
                            {
                                resultAdd.Invoke(result, new object[] { ((Some<object>)parseResult).value });
                            }
                            else
                            {
                                return false;
                            }

                            i2 = cdrInput;
                        }

                        f.SetValue(dest, result);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                };

                return fieldStore;

                #endregion
            }
            else if (f.FieldType.IsGenericType && f.FieldType.GetGenericTypeDefinition() == typeof(FList<>))
            {
                #region handle FList<T>

                Type element = f.FieldType.GetGenericArguments()[0];

                SaBox<Func<object, Option<object>>> elParser = getBox(element);

                ConstructorInfo newFList = f.FieldType.GetConstructor(new Type[] { element, f.FieldType });

                MethodInfo reverse = f.FieldType.GetMethod("Reverse", BindingFlags.Static | BindingFlags.Public, null, new Type[] { f.FieldType }, null);

                Func<object, object, bool> fieldStore = delegate(object input, object dest)
                {
                    if (ConsCell.IsList(input))
                    {
                        object i2 = input;
                        object result = null;

                        while (!(ConsCell.IsEmptyList(i2)))
                        {
                            object carInput = ((ConsCell)i2).car;
                            object cdrInput = ((ConsCell)i2).cdr;

                            Option<object> parseResult = elParser.Value(carInput);
                            if (parseResult is Some<object>)
                            {
                                result = newFList.Invoke(new object[] { ((Some<object>)parseResult).value, result });
                            }
                            else
                            {
                                return false;
                            }
                            i2 = cdrInput;
                        }

                        result = reverse.Invoke(null, new object[] { result });

                        f.SetValue(dest, result);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                };

                return fieldStore;

                #endregion
            }
            else if (f.FieldType.IsGenericType && f.FieldType.GetGenericTypeDefinition() == typeof(ExprObjModel.ObjectSystem.Message<>))
            {
                #region handle Message<T>

                Type element = f.FieldType.GetGenericArguments()[0];

                SaBox<Func<object, Option<object>>> elParser = getBox(element);

                Type tupleSymbolElement = typeof(Tuple<,>).MakeGenericType(typeof(Symbol), element);

                Type enumerableTupleSymbolElement = typeof(IEnumerable<>).MakeGenericType(tupleSymbolElement);

                ConstructorInfo newMessage = f.FieldType.GetConstructor(new Type[] { typeof(Symbol), enumerableTupleSymbolElement });

                MethodInfo mapSecond = typeof(Utils).GetMethod("MapSecond").MakeGenericMethod(new Type[] { typeof(Symbol), typeof(object), element });

                Delegate castFunc = MakeCastFunc(typeof(object), element);

                Func<object, object, bool> fieldStore = delegate(object input, object dest)
                {
                    if (input is ExprObjModel.ObjectSystem.Message<object>)
                    {
                        ExprObjModel.ObjectSystem.Message<Option<object>> m2 = ((ExprObjModel.ObjectSystem.Message<object>)(input)).Map<Option<object>>(x => elParser.Value(x));
                        if (m2.Values.All(x => x is Some<object>))
                        {
                            IEnumerable<Tuple<Symbol, object>> parseResults = m2.Arguments.Select(x => new Tuple<Symbol, object>(x.Item1, ((Some<object>)(x.Item2)).value));
                            object parseResults2 = mapSecond.Invoke(null, new object[] { parseResults, castFunc });
                            object result = newMessage.Invoke(new object[] { m2.Type, parseResults2 });
                            f.SetValue(dest, result);
                            return true;
                        }
                        else return false;
                    }
                    else
                    {
                        return false;
                    }
                };

                return fieldStore;

                #endregion
            }
            else
            {
                SaBox<Func<object, Option<object>>> parser = getBox(f.FieldType);

                Func<object, object, bool> fieldStore = delegate(object input, object dest)
                {
                    Option<object> q = parser.Value(input);

                    if (q is Some<object>)
                    {
                        f.SetValue(dest, ((Some<object>)q).value);
                        return true;
                    }
                    else return false;
                };

                return fieldStore;
            }

            throw new NotImplementedException("Unknown type");
        }

        private static Func<object, Option<object>> MakeParseAlternatives(SaBox<Func<object, Option<object>>>[] alts, Func<Type, SaBox<Func<object, Option<object>>>> getBox)
        {
            int iEnd = alts.Length;

            Func<object, Option<object>> result = null;
            while (iEnd > 0)
            {
                --iEnd;

                if (result == null)
                {
                    SaBox<Func<object, Option<object>>> f1 = alts[iEnd];
                    result = delegate(object input)
                    {
                        Option<object> s1 = f1.Value(input);
                        if (!(s1 is Some<object>))
                        {
                            throw new SchemeRuntimeException("Failed to parse: " + SchemeDataWriter.ItemToString(input));
                        }
                        return s1;
                    };
                }
                else
                {
                    SaBox<Func<object, Option<object>>> f1 = alts[iEnd];
                    Func<object, Option<object>> f2 = result;
                    result = delegate(object input)
                    {
                        Option<object> x = f1.Value(input);
                        if (x is None<object>) x = f2(input);
                        return x;
                    };
                }
            }
            return result;
        }
    }

    namespace TestPatternBind
    {
        [DescendantsWithPatterns]
        public abstract class Tree
        {
            public abstract void AppendTo(StringBuilder sb);

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                AppendTo(sb);
                return sb.ToString();
            }
        }

        [Pattern("(node . $a)")]
        public class Node : Tree
        {
            [Bind("$a")]
            public List<Tree> children;

            public override void AppendTo(StringBuilder sb)
            {
                sb.Append("(Node: ");
                bool needSpace = false;
                foreach (Tree t in children)
                {
                    if (needSpace) sb.Append(" ");
                    t.AppendTo(sb);
                    needSpace = true;
                }
                sb.Append(")");
            }
        }

        [Pattern("(string $b)")]
        public class StringLeaf : Tree
        {
            [Bind("$b")]
            public string data;

            public override void AppendTo(StringBuilder sb)
            {
                sb.Append("(String: \"");
                sb.Append(data);
                sb.Append("\")");
            }
        }

        [Pattern("(int $i)")]
        public class IntLeaf : Tree
        {
            [Bind("$i")]
            public int i;

            public override void AppendTo(StringBuilder sb)
            {
                sb.Append("(Int: ");
                sb.Append(i);
                sb.Append(")");
            }
        }

        public enum TestItem
        {
            [Symbol("movl->")]
            Move32Right,

            [Symbol("movl<-")]
            Move32Left,
        }

        [Pattern("(enum $e)")]
        public class EnumLeaf : Tree
        {
            [Bind("$e")]
            public TestItem e;

            public override void AppendTo(StringBuilder sb)
            {
                sb.Append("(Enum: ");
                sb.Append(e);
                sb.Append(")");
            }
        }

        [Pattern("(two-ints $i $j)")]
        public class TwoIntsLeaf : Tree
        {
            [Bind("$i")]
            public int i;

            [Bind("$j")]
            public int j;

            public override void AppendTo(StringBuilder sb)
            {
                sb.Append("(Two-Ints: ");
                sb.Append(i);
                sb.Append(" ");
                sb.Append(j);
                sb.Append(")");
            }
        }

        [Pattern("(int-list . $i)")]
        public class IntListLeaf : Tree
        {
            [Bind("$i")]
            public List<int> iList;

            public override void AppendTo(StringBuilder sb)
            {
                sb.Append("(Int-List: ");
                bool needSpace = false;
                foreach (int i in iList)
                {
                    if (needSpace) sb.Append(" ");
                    sb.Append(i);
                    needSpace = true;
                }
                sb.Append(")");
            }
        }

        [Pattern("$msg")]
        public class MsgLeaf : Tree
        {
            [Bind("$msg")]
            public ExprObjModel.ObjectSystem.Message<Tree> msg;

            public override void AppendTo(StringBuilder sb)
            {
                sb.Append("(Message ");
                sb.Append(msg.Type);
                sb.Append(": ");
                bool needSpace = false;
                foreach (Tuple<Symbol, Tree> st in msg.Arguments)
                {
                    if (needSpace) sb.Append(", ");
                    sb.Append(st.Item1);
                    sb.Append("=");
                    st.Item2.AppendTo(sb);
                    needSpace = true;
                }
                sb.Append(")");
            }
        }
    }

    namespace Procedures
    {
        static partial class ProxyDiscovery
        {
            private static Func<object, Option<object>> parser = Utils.MakeParser(typeof(ExprObjModel.TestPatternBind.Tree));

            [SchemeFunction("test-parser")]
            public static string TestParser(object obj)
            {
                Option<object> k = parser(obj);
                if (k is Some<object>)
                {
                    ExprObjModel.TestPatternBind.Tree t = (ExprObjModel.TestPatternBind.Tree)(((Some<object>)k).value);
                    return t.ToString();
                }
                else
                {
                    return "";
                }
            }
        }
    }
}