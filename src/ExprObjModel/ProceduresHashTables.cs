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
using BigMath;
using ControlledWindowLib;

namespace ExprObjModel
{
    [Serializable]
    public class HashEqualityTester : IEqualityComparer<object>
    {
        public HashEqualityTester() { }

        #region IEqualityComparer<object> Members

        bool IEqualityComparer<object>.Equals(object x, object y)
        {
            return ExprObjModel.Procedures.ProxyDiscovery.FastEqual(x, y);
        }

        public int GetHashCode(object obj)
        {
            return unchecked((int)(ExprObjModel.Procedures.ProxyDiscovery.Hash(obj)));
        }

        #endregion
    }

    [Serializable]
    public class SchemeHashSet : IEnumerable<object>
    {
        private HashSet<object> dict;

        public SchemeHashSet()
        {
            dict = new HashSet<object>(new HashEqualityTester());
        }

        public int Count
        {
            [SchemeFunction("hashset-length")]
            get
            {
                return dict.Count;
            }
        }

        [SchemeFunction("hashset-contains?")]
        public bool Contains(object obj)
        {
            if (!Procedures.ProxyDiscovery.IsHashable(obj)) return false;
            return dict.Contains(obj);
        }

        [SchemeFunction("hashset-add!")]
        public void Add(object obj)
        {
            if (!Procedures.ProxyDiscovery.IsHashable(obj)) throw new SchemeRuntimeException("Cannot add non-hashable object to hash set");
            if (!dict.Contains(obj)) dict.Add(obj);
        }

        [SchemeFunction("hashset-remove!")]
        public void Remove(object obj)
        {
            if (!Procedures.ProxyDiscovery.IsHashable(obj)) return;
            if (dict.Contains(obj)) dict.Remove(obj);
        }

        [SchemeFunction("hashset->list")]
        public object ToSchemeList()
        {
            object result = SpecialValue.EMPTY_LIST;
            foreach (object key in dict)
            {
                result = new ConsCell(key, result);
            }
            return result;
        }

        [SchemeFunction("hashset->vector")]
        public Deque<object> ToSchemeVector()
        {
            Deque<object> result = new Deque<object>();
            foreach (object key in dict)
            {
                result.PushFront(key);
            }
            return result;
        }

        public static SchemeHashSet Union(SchemeHashSet a, SchemeHashSet b)
        {
            SchemeHashSet c = new SchemeHashSet();
            c.dict.UnionWith(a.dict);
            c.dict.UnionWith(b.dict);
            return c;
        }

        public static SchemeHashSet Intersection(SchemeHashSet a, SchemeHashSet b)
        {
            SchemeHashSet c = new SchemeHashSet();
            c.dict.UnionWith(a.dict);
            c.dict.IntersectWith(b.dict);
            return c;
        }

        public static SchemeHashSet Difference(SchemeHashSet a, SchemeHashSet b)
        {
            SchemeHashSet c = new SchemeHashSet();
            c.dict.UnionWith(a.dict);
            c.dict.ExceptWith(b.dict);
            return c;
        }

        public static SchemeHashSet SymmetricDifference(SchemeHashSet a, SchemeHashSet b)
        {
            SchemeHashSet c = new SchemeHashSet();
            c.dict.UnionWith(a.dict);
            c.dict.SymmetricExceptWith(b.dict);
            return c;
        }

        public IEnumerator<object> GetEnumerator()
        {
            return dict.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return dict.GetEnumerator();
        }

        public static SchemeHashSet FromEnumerable(IEnumerable<object> items)
        {
            SchemeHashSet h = new SchemeHashSet();
            foreach(object i in items)
            {
                if (!(ExprObjModel.Procedures.ProxyDiscovery.IsHashable(i))) throw new ArgumentException("Object cannot be included in set");
                h.dict.Add(i);
            }
            return h;
        }
    }

    [Serializable]
    public class SchemeHashMap : IEnumerable<KeyValuePair<object, object>>
    {
        private Dictionary<object, object> dict;

        [SchemeFunction("make-hashmap")]
        public SchemeHashMap()
        {
            dict = new Dictionary<object, object>(new HashEqualityTester());
        }

        public int Count
        {
            [SchemeFunction("hashmap-length")]
            get
            {
                return dict.Count;
            }
        }

        [SchemeFunction("hashmap-contains-key?")]
        public bool ContainsKey(object key)
        {
            if (!Procedures.ProxyDiscovery.IsHashable(key)) return false;
            return dict.ContainsKey(key);
        }

        public object this[object key]
        {
            [SchemeFunction("hashmap-ref")]
            get
            {
                if (!Procedures.ProxyDiscovery.IsHashable(key)) throw new SchemeRuntimeException("Cannot find non-hashable key in hash map");
                if (!dict.ContainsKey(key)) return false;
                return dict[key];
            }
            [SchemeFunction("hashmap-set!")]
            set
            {
                if (!Procedures.ProxyDiscovery.IsHashable(key)) throw new SchemeRuntimeException("Cannot add non-hashable key to hash map");
                if (dict.ContainsKey(key))
                {
                    dict[key] = value;
                }
                else
                {
                    dict.Add(key, value);
                }
            }
        }

        [Obsolete]
        public void Set(object key, object value)
        {
            this[key] = value;
        }

        [SchemeFunction("hashmap-remove!")]
        public void Remove(object key)
        {
            if (!Procedures.ProxyDiscovery.IsHashable(key)) throw new SchemeRuntimeException("Cannot remove non-hashable key from hash map");
            if (dict.ContainsKey(key)) dict.Remove(key);
        }

        [SchemeFunction("hashmap-keys")]
        public SchemeHashSet GetKeys()
        {
            SchemeHashSet h = new SchemeHashSet();
            foreach (object obj in dict.Keys)
            {
                h.Add(obj);
            }
            return h;
        }

        public IEnumerator<KeyValuePair<object, object>> GetEnumerator()
        {
            return dict.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return dict.GetEnumerator();
        }
    }

    namespace Procedures
    {
        [SchemeSingleton("make-hashset")]
        public class MakeHashSetProc : IProcedure
        {
            public MakeHashSetProc() { }

            #region IProcedure Members

            public int Arity { get { return 0; } }

            public bool More { get { return true; } }

            public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
            {
                SchemeHashSet result = new SchemeHashSet();
                while (argList != null)
                {
                    if (ProxyDiscovery.IsHashable(argList.Head))
                    {
                        result.Add(argList.Head);
                    }
                    else throw new SchemeRuntimeException("make-hashset requires all arguments to be hashable");
                    argList = argList.Tail;
                }
                return new RunnableReturn(k, result);
            }

            #endregion
        }

        [SchemeSingleton("hashset-union")]
        public class HashSetUnionProc : IProcedure
        {
            public HashSetUnionProc() { }

            #region IProcedure Members

            public int Arity { get { return 1; } }

            public bool More { get { return true; } }

            public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
            {
                if (argList == null) throw new SchemeRuntimeException("hashset-union requires at least one argument");
                if (!(argList.Head is SchemeHashSet)) throw new SchemeRuntimeException("hashset-union requires arguments to be hash sets");
                SchemeHashSet result = (SchemeHashSet)(argList.Head);
                argList = argList.Tail;
                while (argList != null)
                {
                    if (!(argList.Head is SchemeHashSet)) throw new SchemeRuntimeException("hashset-union requires arguments to be hash sets");
                    result = SchemeHashSet.Union(result, ((SchemeHashSet)(argList.Head)));
                    argList = argList.Tail;
                }
                return new RunnableReturn(k, result);
            }

            #endregion
        }

        [SchemeSingleton("hashset-intersection")]
        public class HashSetIntersectionProc : IProcedure
        {
            public HashSetIntersectionProc() { }

            #region IProcedure Members

            public int Arity { get { return 1; } }

            public bool More { get { return true; } }

            public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
            {
                if (argList == null) throw new SchemeRuntimeException("hashset-intersection requires at least one argument");
                if (!(argList.Head is SchemeHashSet)) throw new SchemeRuntimeException("hashset-union requires arguments to be hash sets");
                SchemeHashSet result = (SchemeHashSet)(argList.Head);
                argList = argList.Tail;
                while (argList != null)
                {
                    if (!(argList.Head is SchemeHashSet)) throw new SchemeRuntimeException("hashset-union requires arguments to be hash sets");
                    result = SchemeHashSet.Intersection(result, ((SchemeHashSet)(argList.Head)));
                    argList = argList.Tail;
                }
                return new RunnableReturn(k, result);
            }

            #endregion
        }

        [SchemeSingleton("hashset-difference")]
        public class HashSetDifferenceProc : IProcedure
        {
            public HashSetDifferenceProc() { }

            #region IProcedure Members

            public int Arity { get { return 1; } }

            public bool More { get { return true; } }

            public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
            {
                if (argList == null) throw new SchemeRuntimeException("hashset-intersection requires at least one argument");
                if (!(argList.Head is SchemeHashSet)) throw new SchemeRuntimeException("hashset-union requires arguments to be hash sets");
                SchemeHashSet result = (SchemeHashSet)(argList.Head);
                argList = argList.Tail;
                while (argList != null)
                {
                    if (!(argList.Head is SchemeHashSet)) throw new SchemeRuntimeException("hashset-union requires arguments to be hash sets");
                    result = SchemeHashSet.Difference(result, ((SchemeHashSet)(argList.Head)));
                    argList = argList.Tail;
                }
                return new RunnableReturn(k, result);
            }

            #endregion
        }

        [SchemeSingleton("hashset-symmetric-difference")]
        public class HashSetSymmetricDifferenceProc : IProcedure
        {
            public HashSetSymmetricDifferenceProc() { }

            #region IProcedure Members

            public int Arity { get { return 1; } }

            public bool More { get { return true; } }

            public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
            {
                if (argList == null) throw new SchemeRuntimeException("hashset-intersection requires at least one argument");
                if (!(argList.Head is SchemeHashSet)) throw new SchemeRuntimeException("hashset-union requires arguments to be hash sets");
                SchemeHashSet result = (SchemeHashSet)(argList.Head);
                argList = argList.Tail;
                while (argList != null)
                {
                    if (!(argList.Head is SchemeHashSet)) throw new SchemeRuntimeException("hashset-union requires arguments to be hash sets");
                    result = SchemeHashSet.SymmetricDifference(result, ((SchemeHashSet)(argList.Head)));
                    argList = argList.Tail;
                }
                return new RunnableReturn(k, result);
            }

            #endregion
        }
        public static partial class ProxyDiscovery
        {
            [SchemeFunction("hashset?")]
            public static bool IsHashSet(object obj)
            {
                return obj is SchemeHashSet;
            }

            [SchemeFunction("hashmap?")]
            public static bool IsHashMap(object obj)
            {
                return obj is SchemeHashMap;
            }
        }
    }
}