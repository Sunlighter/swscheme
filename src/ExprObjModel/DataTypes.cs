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
using System.Runtime.Serialization;
using ExprObjModel.SyntaxAnalysis;
using System.Linq;
using ControlledWindowLib;

namespace ExprObjModel
{
    public class SchemeRuntimeException : ApplicationException
    {
        public SchemeRuntimeException() : base() { }
        public SchemeRuntimeException(string excuse) : base(excuse) { }
        public SchemeRuntimeException(string excuse, Exception cause) : base(excuse, cause) { }
    }

    public enum SpecialValue
    {
        EMPTY_LIST,
        UNSPECIFIED,
        EOF
    }

    [Serializable]
    public class Symbol : IComparable, BigMath.IHashable
    {
        private string name;
        private bool interned;
        private long gensymIndex;

        private static long nextGensymIndex = 0L;

        public Symbol(string x) { name = x; interned = true; gensymIndex = 0L; }

        [SchemeFunction("gensym")]
        public Symbol()
        {
            name = null;
            interned = false;
            gensymIndex = System.Threading.Interlocked.Increment(ref nextGensymIndex);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Symbol)) return false;
            Symbol s = (Symbol)obj;
            if (interned != s.interned) return false;
            if (interned) return name.Equals(s.name);
            else return gensymIndex == s.gensymIndex;
        }

        public override int GetHashCode()
        {
            if (interned) return name.GetHashCode() ^ 0x5A5A5A5A;
            else return gensymIndex.GetHashCode() ^ unchecked((int)0xA5A5A5A5);
        }

        public static bool operator <(Symbol one, Symbol two)
        {
            if (one.interned)
            {
                if (two.interned)
                {
                    return one.name.CompareTo(two.name) < 0;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                if (two.interned)
                {
                    return false;
                }
                else
                {
                    return one.gensymIndex < two.gensymIndex;
                }
            }
        }

        public static bool operator >(Symbol one, Symbol two)
        {
            if (one.interned)
            {
                if (two.interned)
                {
                    return one.name.CompareTo(two.name) > 0;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (two.interned)
                {
                    return true;
                }
                else
                {
                    return one.gensymIndex > two.gensymIndex;
                }
            }
        }

        public static bool operator ==(Symbol one, Symbol two)
        {
            if (object.ReferenceEquals(one, null) && object.ReferenceEquals(two, null)) return true;
            if (object.ReferenceEquals(one, null)) return false;
            if (object.ReferenceEquals(two, null)) return false;
            if (one.interned != two.interned) return false;
            if (one.interned)
            {
                if (one.name.CompareTo(two.name) != 0) return false;
            }
            else
            {
                if (one.gensymIndex != two.gensymIndex) return false;
            }
            return true;
        }

        public static bool operator !=(Symbol one, Symbol two)
        {
            if (object.ReferenceEquals(one, null) && object.ReferenceEquals(two, null)) return false;
            if (object.ReferenceEquals(one, null)) return true;
            if (object.ReferenceEquals(two, null)) return true;
            if (one.interned != two.interned) return true;
            if (one.interned)
            {
                if (one.name.CompareTo(two.name) != 0) return true;
            }
            else
            {
                if (one.gensymIndex != two.gensymIndex) return true;
            }
            return false;
        }

        public int CompareTo(object obj)
        {
            Symbol other = (Symbol)obj;
            return (this < other) ? -1 : (this > other) ? 1 : 0;
        }

        public string Name { get { if (interned) return name; else return "g$" + gensymIndex; } }

        public bool IsInterned { [SchemeFunction("interned?")] get { return interned; } }

        public override string ToString()
        {
            if (interned)
                return name;
            else return "g$" + gensymIndex;
        }

        public bool IsSymbol(string name)
        {
            return interned && (this.name == name);
        }

        #region IHashable Members

        private static byte[] internedHeaderHashBytes = new byte[] { 0x1A, 0x2B, 0x3F };
        private static byte[] uninternedHeaderHashBytes = new byte[] { 0x3E, 0xAD, 0x0E };

        public void AddToHash(BigMath.IHashGenerator hg)
        {
            if (interned)
            {
                hg.Add(internedHeaderHashBytes);
                hg.Add(name);
            }
            else
            {
                hg.Add(uninternedHeaderHashBytes);
                hg.Add(BitConverter.GetBytes(gensymIndex));
            }
        }

        #endregion
    }

    [Serializable]
    public class ConsCell
    {
        public ConsCell() { car = null; cdr = null; }

        [SchemeFunction("cons")]
        public ConsCell(object car, object cdr) { this.car = car; this.cdr = cdr; }

        public object car;
        public object cdr;

        [SchemeFunction("null?")]
        public static bool IsEmptyList(object list)
        {
            return (list is SpecialValue) && ((SpecialValue)list == SpecialValue.EMPTY_LIST);
        }

        [SchemeFunction("pair?")]
        public static bool IsPair(object list) { return (list is ConsCell); }

        [SchemeFunction("car")]
        public static object Car(object list) { return ((ConsCell)list).car; }

        //[SchemeFunction("car?")]
        //public static bool CanCar(object list) { return (list is ConsCell); }

        [SchemeFunction("set-car!")]
        public void SetCar(object val) { car = val; }

        [SchemeFunction("cdr")]
        public static object Cdr(object list) { return ((ConsCell)list).cdr; }

        //[SchemeFunction("cdr?")]
        //public static bool CanCdr(object list) { return (list is ConsCell); }

        [SchemeFunction("set-cdr!")]
        public void SetCdr(object val) { cdr = val; }

        //[SchemeFunction("caar?")]
        //public static bool CanCaar(object list) { return (CanCar(list) && CanCar(Car(list))); }

        //[SchemeFunction("caar")]
        //public static object Caar(object list) { return Car(Car(list)); }

        //[SchemeFunction("cadr?")]
        //public static bool CanCadr(object list) { return (CanCdr(list) && CanCar(Cdr(list))); }

        //[SchemeFunction("cadr")]
        //public static object Cadr(object list) { return Car(Cdr(list)); }

        //[SchemeFunction("cdar?")]
        //public static bool CanCdar(object list) { return (CanCar(list) && CanCdr(Car(list))); }

        //[SchemeFunction("cdar")]
        //public static object Cdar(object list) { return Cdr(Car(list)); }

        //[SchemeFunction("cddr?")]
        //public static bool CanCddr(object list) { return (CanCdr(list) && CanCdr(Cdr(list))); }

        //[SchemeFunction("cddr")]
        //public static object Cddr(object list) { return Cdr(Cdr(list)); }

        //[SchemeFunction("caddr?")]
        //public static bool CanCaddr(object list) { return (CanCddr(list) && CanCar(Cddr(list))); }

        //[SchemeFunction("caddr")]
        //public static object Caddr(object list) { return Car(Cddr(list)); }

        [SchemeFunction("list?")]
        public static bool IsList(object list)
        {
            if (IsEmptyList(list)) return true;

            object ptr = list;
            ObjectIDGenerator g = new ObjectIDGenerator();

            while (ptr is ConsCell)
            {
                ConsCell cPtr = (ConsCell)ptr;
                bool firstTime;
                g.GetId(ptr, out firstTime);
                if (!firstTime) return false;
                ptr = cPtr.cdr;
            }
            if (!IsEmptyList(ptr)) return false;

            return true;
        }

        public static void ForEachInList(object list, Action<object> f)
        {
            if (IsEmptyList(list)) return;

            object ptr = list;
            ObjectIDGenerator g = new ObjectIDGenerator();

            while (ptr is ConsCell)
            {
                ConsCell cPtr = (ConsCell)ptr;
                bool firstTime;
                g.GetId(ptr, out firstTime);
                if (!firstTime) throw new Exception("Circular list!");
                f(cPtr.car);
                ptr = cPtr.cdr;
            }
            if (!IsEmptyList(ptr)) throw new Exception("Improper list!");
        }

        public static void Reverse(ref object list)
        {
            object result = SpecialValue.EMPTY_LIST;
            while (list is ConsCell)
            {
                ConsCell lc = (ConsCell)list;
                list = lc.cdr; lc.cdr = result; result = lc;
            }
            if (!IsEmptyList(list))
                throw new SystemException("Reverse: Improper List");
            list = result;
        }

        [SchemeFunction("reverse")]
        public static object ReverseCopy(object list)
        {
            object result = SpecialValue.EMPTY_LIST;
            ForEachInList
            (
                list, delegate(object obj)
                {
                    result = new ConsCell(obj, result);
                }
            );
            return result;
        }

        [SchemeFunction("length")]
        public static int ListLength(object list)
        {
            int result = 0;
            ForEachInList
            (
                list, delegate(object obj)
                {
                    ++result;
                }
            );
            return result;
        }

        [SchemeFunction("length-up-to")]
        public static int ListLengthUpTo(int limit, object list)
        {
            if (IsEmptyList(list)) return 0;

            int length = 0;

            object ptr = list;

            while (ptr is ConsCell && limit > 0)
            {
                ConsCell cPtr = (ConsCell)ptr;
                ptr = cPtr.cdr;
                ++length; --limit;
            }
            return length;
        }

        [SchemeFunction("list-copy")]
        public static object ListCopy(object list)
        {
            object l2 = ReverseCopy(list);
            Reverse(ref l2);
            return l2;
        }

        [SchemeFunction("list->vector")]
        public static Deque<object> ListToVector(object list)
        {
            Deque<object> v = new Deque<object>();
            ForEachInList
            (
                list, delegate(object obj)
                {
                    v.PushBack(obj);
                }
            );
            return v;
        }

        [SchemeFunction("vector->list")]
        public static object VectorToList(object v)
        {
            object result = SpecialValue.EMPTY_LIST;
            Deque<object> vec = (Deque<object>)v;
            int iend = vec.Count;
            for (int i = iend - 1; i >= 0; --i)
            {
                result = new ConsCell(vec[i], result);
            }
            return result;
        }

        public static object ReverseMap(object list, Func<object, object> transformFunc)
        {
            object result = SpecialValue.EMPTY_LIST;
            ForEachInList
            (
                list, delegate(object obj)
                {
                    result = new ConsCell(transformFunc(obj), result);
                }
            );
            return result;
        }

        public static object Map(object list, Func<object, object> transformFunc)
        {
            object result = ReverseMap(list, transformFunc);
            Reverse(ref result);
            return result;
        }

        public static object ReverseMapFList<T>(FList<T> list, Func<T, object> transformFunc)
        {
            object result = SpecialValue.EMPTY_LIST;
            foreach (T item in FListUtils.ToEnumerable(list))
            {
                result = new ConsCell(transformFunc(item), result);
            }
            return result;
        }

        public static object MapFromFList<T>(FList<T> list, Func<T, object> transformFunc)
        {
            object result = ReverseMapFList(list, transformFunc);
            Reverse(ref result);
            return result;
        }

        public static object ReverseMapEnumerable<T>(IEnumerable<T> list, Func<T, object> transformFunc)
        {
            object result = SpecialValue.EMPTY_LIST;
            foreach (T item in list)
            {
                result = new ConsCell(transformFunc(item), result);
            }
            return result;
        }

        public static object MapEnumerable<T>(IEnumerable<T> list, Func<T, object> transformFunc)
        {
            object result = ReverseMapEnumerable(list, transformFunc);
            Reverse(ref result);
            return result;
        }

        public static IEnumerable<object> Enumerate(object list)
        {
            while (list is ConsCell)
            {
                ConsCell c = (ConsCell)list;
                yield return c.car;
                list = c.cdr;
            }
        }

        public static object Fold(object list, object initialResult, Func<object, object, object> func)
        {
            object result = initialResult;
            ForEachInList
            (
                list, delegate(object obj)
                {
                    result = func(result, obj);
                }
            );
            return result;
        }

        [SchemeFunction("$$append2")]
        public static object Append(object l1, object l2)
        {
            object soFar = l2;
            l1 = ListCopy(l1);
            object l1end = l1;
            while (IsPair(Cdr(l1end))) l1end = Cdr(l1end);
            ((ConsCell)l1end).cdr = l2;
            return l1;
        }

        public static EnvSpec GetRequirements(FList<IExpressionSource> exprSourceList)
        {
            return FListUtils.ToEnumerable(exprSourceList).Select(i => i.GetRequirements()).EnvSpecUnion();
        }

        public static FList<IExpression> CompileList(FList<IExpressionSource> exprList, EnvDesc ed)
        {
            return FListUtils.Map
            (
                exprList,
                delegate(IExpressionSource src)
                {
                    return src.Compile(ed);
                }
            );
        }

        public static FList<T> MapToFList<T>(object consCellList, Func<object, T> func)
        {
            FList<T> result = null;
            ForEachInList
            (
                consCellList,
                delegate(object obj)
                {
                    result = new FList<T>(func(obj), result);
                }
            );
            return FListUtils.Reverse(result);
        }
    }
}
