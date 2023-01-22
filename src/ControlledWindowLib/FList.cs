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

namespace ExprObjModel
{
    [Serializable]
    public class FList<T>
    {
        private T head;
        private FList<T> tail;

        public FList(T head): this(head, null)
        {
            // empty
        }

        public FList(T head, FList<T> tail)
        {
            this.head = head;
            this.tail = tail;
        }

        public T Head { get { return head; } }
        public FList<T> Tail { get { return tail; } }
    }

    public static partial class FListUtils
    {
        public static IEnumerable<T> ToEnumerable<T>(FList<T> list)
        {
            FList<T> ptr = list;
            while (ptr != null)
            {
                yield return ptr.Head;
                ptr = ptr.Tail;
            }
        }

        public static T[] ToArray<T>(FList<T> list)
        {
            List<T> items = new List<T>();
            while (list != null)
            {
                items.Add(list.Head);
                list = list.Tail;
            }
            return items.ToArray();
        }

        public static FList<U> ReverseMap<T, U>(FList<T> list, Func<T, U> mapFunc)
        {
            FList<U> result = null;
            FList<T> ptr = list;
            while (ptr != null)
            {
                result = new FList<U>(mapFunc(ptr.Head), result);
                ptr = ptr.Tail;
            }
            return result;
        }

        public static T Fold<T>(FList<T> list, Func<T, T, T> foldFunc, T initVal)
        {
            T val = initVal;
            FList<T> ptr = list;
            while (ptr != null)
            {
                val = foldFunc(ptr.Head, val);
                ptr = ptr.Tail;
            }
            return val;
        }

        public static U MapFold<T, U>(FList<T> list, Func<T, U> mapFunc, Func<U, U, U> foldFunc, U initVal)
        {
            U val = initVal;
            FList<T> ptr = list;
            while (ptr != null)
            {
                val = foldFunc(val, mapFunc(ptr.Head));
                ptr = ptr.Tail;
            }
            return val;
        }

        public static FList<T> Reverse<T>(FList<T> list)
        {
            FList<T> result = null;
            FList<T> ptr = list;
            while (ptr != null)
            {
                result = new FList<T>(ptr.Head, result);
                ptr = ptr.Tail;
            }
            return result;
        }

        public static int Count<T>(FList<T> list)
        {
            FList<T> ptr = list;
            int count = 0;
            while (ptr != null)
            {
                ++count;
                ptr = ptr.Tail;
            }
            return count;
        }

        public static int CountUpTo<T>(FList<T> list, int limit)
        {
            FList<T> ptr = list;
            int count = 0;
            while (ptr != null && count < limit)
            {
                ++count;
                ptr = ptr.Tail;
            }
            return count;
        }

        public static T Nth<T>(FList<T> list, int n)
        {
            FList<T> ptr = list;
            while (ptr != null)
            {
                if (n == 0) return ptr.Head;
                ptr = ptr.Tail;
                --n;
            }
            throw new ArgumentOutOfRangeException("Index " + n + " doesn't exist");
        }

        public static FList<T> NthTail<T>(FList<T> list, int n)
        {
            FList<T> ptr = list;
            if (ptr != null)
            {
                ptr = ptr.Tail;
                while (ptr != null)
                {
                    if (n == 0) return ptr;
                    ptr = ptr.Tail;
                    --n;
                }
            }
            throw new ArgumentOutOfRangeException("Tail " + n + " doesn't exist");
        }

        public static FList<T> Append<T>(FList<T> a, FList<T> b)
        {
            FList<T> ptr = b;
            FList<T> ptr2 = Reverse(a);
            while (ptr2 != null)
            {
                ptr = new FList<T>(ptr2.Head, ptr);
                ptr2 = ptr2.Tail;
            }
            return ptr;
        }

        public static FList<U> Map<T, U>(FList<T> list, Func<T, U> mapFunc)
        {
            return Reverse(ReverseMap(list, mapFunc));
        }

        public static FList<T> Filter<T>(FList<T> list, Func<T, bool> where)
        {
            FList<T> results = null;
            while (list != null)
            {
                T head = list.Head;
                list = list.Tail;
                if (where(head))
                {
                    results = new FList<T>(head, results);
                }
            }
            return Reverse(results);
        }

        public static FList<T> FilterDispose<T>(FList<T> list, Func<T, bool> where, Action<T> dispose)
        {
            FList<T> results = null;
            while (list != null)
            {
                T head = list.Head;
                list = list.Tail;
                if (where(head))
                {
                    results = new FList<T>(head, results);
                }
                else
                {
                    dispose(head);
                }
            }
            return Reverse(results);
        }

        public static FList<T> ToFList<T>(IEnumerable<T> src)
        {
            FList<T> results = null;
            foreach (T item in src)
            {
                results = new FList<T>(item, results);
            }
            return Reverse(results);
        }
    }
}
