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

namespace ExprObjModel
{
    public interface ICountedEnumerable<T> : IEnumerable<T>
    {
        int Count { get; }
    }

    public class CountedEnumerable<T> : ICountedEnumerable<T>
    {
        private IEnumerable<T> items;
        private int count;

        public CountedEnumerable(IEnumerable<T> items, int count)
        {
            this.items = items;
            this.count = count;
        }

        public int Count
        {
            get { return count; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.IEnumerable)items).GetEnumerator();
        }
    }

    public interface IReadOnlyArray<T> : IEnumerable<T>
    {
        int Count { get; }
        T this[int index] { get; }
    }

    public class ReadOnlyArrayFunc<T> : IReadOnlyArray<T>
    {
        private int count;
        private Func<int, T> func;

        public ReadOnlyArrayFunc(int count, Func<int, T> func)
        {
            this.count = count;
            this.func = func;
        }

        public int Count { get { return count; } }

        public T this[int index]
        {
            get
            {
                index %= count;
                if (index < 0) index += count;
                return func(index);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < count; ++i) yield return func(i);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (System.Collections.IEnumerator)(this.GetEnumerator());
        }
    }

    public static partial class Utils
    {
        public static T[] ToArray<T>(this IReadOnlyArray<T> items)
        {
            int iEnd = items.Count;
            T[] arr = new T[iEnd];
            for (int i = 0; i < iEnd; ++i)
            {
                arr[i] = items[i];
            }
            return arr;
        }

        public static IReadOnlyArray<T> AsReadOnlyArray<T>(this T[] array)
        {
            return new ReadOnlyArrayFunc<T>(array.Length, x => array[x]);
        }

        public static IReadOnlyArray<U> Select<T, U>(this IReadOnlyArray<T> items, Func<T, U> selectFunc)
        {
            return new ReadOnlyArrayFunc<U>(items.Count, x => selectFunc(items[x]));
        }

        public static int? FirstIndexWhere<T>(this IReadOnlyArray<T> array, Func<T, bool> predicate)
        {
            int iEnd = array.Count;
            for (int i = 0; i < iEnd; ++i)
            {
                if (predicate(array[i])) return i;
            }
            return null;
        }
    }

    public class IndexedSet<T> : IEnumerable<KeyValuePair<T, int>>
    {
        private List<T> items;
        private Dictionary<T, int> indices;

        public IndexedSet()
        {
            items = new List<T>();
            indices = new Dictionary<T, int>();
        }

        public int Count { get { return items.Count; } }

        public bool Contains(T item) { return indices.ContainsKey(item); }

        public int IndexOf(T item) { return indices[item]; }

        public T this[int index] { get { return items[index]; } }

        public T[] ToArray() { return items.ToArray(); }

        public int Add(T item)
        {
            if (indices.ContainsKey(item)) throw new ArgumentException("Item already exists in IndexedSet");
            int index = items.Count;
            items.Add(item);
            indices.Add(item, index);
            return index;
        }

        public int EnsureAdded(T item)
        {
            if (indices.ContainsKey(item)) return indices[item];
            int index = items.Count;
            items.Add(item);
            indices.Add(item, index);
            return index;
        }

        public IEnumerator<KeyValuePair<T, int>> GetEnumerator()
        {
            return indices.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return indices.GetEnumerator();
        }
    }

    public class DualIndexedSet<T>
    {
        private List<T> items;
        private Dictionary<T, int> indices;
        private Func<T, T> getDual;

        public DualIndexedSet(Func<T, T> getDual)
        {
            items = new List<T>();
            indices = new Dictionary<T, int>();
            this.getDual = getDual;
        }

        public int Count { get { return items.Count; } }

        public bool Contains(T item) { return indices.ContainsKey(item) || indices.ContainsKey(getDual(item)); }

        public int IndexOf(T item)
        {
            if (indices.ContainsKey(item)) return indices[item];
            else if (indices.ContainsKey(getDual(item))) return ~indices[item];
            else throw new KeyNotFoundException();
        }

        public T this[int index]
        {
            get
            {
                if (index < 0) return getDual(items[~index]);
                else return items[index];
            }
        }

        public int EnsureAdded(T item)
        {
            if (indices.ContainsKey(item)) return indices[item];
            else if (indices.ContainsKey(getDual(item))) return ~indices[getDual(item)];
            else
            {
                int index = items.Count;
                items.Add(item);
                indices.Add(item, index);
                return index;
            }
        }
    }
}