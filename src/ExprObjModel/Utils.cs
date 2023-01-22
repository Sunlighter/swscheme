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
using System.Text;
using System.IO;

namespace ExprObjModel
{
    public static partial class Utils
    {
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> list)
        {
            HashSet<T> result = new HashSet<T>();
            result.UnionWith(list);
            return result;
        }

        public static T[] ToArray<T>(this ICountedEnumerable<T> list)
        {
            T[] arr = new T[list.Count];
            int i = 0;
            foreach (T item in list)
            {
                arr[i] = item;
                ++i;
            }
            return arr;
        }

        public static Dictionary<K, V> ToDictionaryFavorFirst<T, K, V>(this IEnumerable<T> list, Func<T, K> getKey, Func<T, V> getValue)
        {
            Dictionary<K, V> dict = new Dictionary<K, V>();
            foreach (T item in list)
            {
                K key = getKey(item);
                V value = getValue(item);
                if (!(dict.ContainsKey(key))) dict.Add(key, value);
            }
            return dict;
        }

        public static FList<T> ToFList<T>(this IEnumerable<T> items, bool reverse)
        {
            FList<T> results = null;
            foreach (T item in items)
            {
                results = new FList<T>(item, results);
            }
            return reverse ? results : FListUtils.Reverse(results);
        }

        public static SchemeHashSet ToSchemeHashSet(this IEnumerable<object> list)
        {
            SchemeHashSet u = new SchemeHashSet();
            foreach (object obj in list)
            {
                if (!(Procedures.ProxyDiscovery.IsHashable(obj))) throw new SchemeRuntimeException("Non-hashable object");
                u.Add(obj);
            }
            return u;
        }

        public static int Find(this Symbol[] array, Symbol target)
        {
            int i = 0;
            int iend = array.Length;
            while (i < iend)
            {
                if (array[i] == target) break;
                ++i;
            }
            if (i == iend) throw new SchemeRuntimeException("Symbol not found!");
            return i;
        }

        public static T[] GetCustomAttributes<T>(this Type t, bool inherit)
        {
            object[] objArr = t.GetCustomAttributes(typeof(T), inherit);
            T[] arr = objArr.OfType<T>().ToArray();
            return arr;
        }

        [Obsolete("Use MemberInfo.IsDefined")]
        public static bool HasCustomAttributes<T>(this Type t, bool inherit)
        {
            return t.IsDefined(typeof(T), inherit);
        }

        public static T[] GetCustomAttributes<T>(this System.Reflection.MemberInfo mi, bool inherit)
        {
            object[] objArr = mi.GetCustomAttributes(inherit);
            T[] arr = objArr.OfType<T>().ToArray();
            return arr;
        }

        [Obsolete("Use MemberInfo.IsDefined")]
        public static bool HasCustomAttributes<T>(this System.Reflection.MemberInfo mi, bool inherit)
        {
            return mi.IsDefined(typeof(T),inherit);
        }

        public static T[] GetCustomAttributes<T>(this System.Reflection.ParameterInfo pi, bool inherit)
        {
            object[] objArr = pi.GetCustomAttributes(inherit);
            T[] arr = objArr.OfType<T>().ToArray();
            return arr;
        }

        [Obsolete("Use MemberInfo.IsDefined")]
        public static bool HasCustomAttributes<T>(this System.Reflection.ParameterInfo pi, bool inherit)
        {
            return pi.IsDefined(typeof(T), inherit);
        }

        public static HashSet<T> Closure<T>(this HashSet<T> set, Func<T, HashSet<T>> reachableFrom)
        {
            Queue<T> q = new Queue<T>();
            foreach (T item in set)
            {
                q.Enqueue(item);
            }
            HashSet<T> results = new HashSet<T>();
            results.UnionWith(set);

            while (!(q.Count == 0))
            {
                T item = q.Dequeue();
                HashSet<T> candidates = reachableFrom(item);
                candidates.ExceptWith(results);
                foreach (T item2 in candidates) q.Enqueue(item2);
                results.UnionWith(candidates);
            }

            return results;
        }

        public static string Concatenate(this IEnumerable<string> strings, string delimiter)
        {
            StringBuilder sb = new StringBuilder();
            bool needDelim = false;
            foreach (string str in strings)
            {
                if (needDelim) sb.Append(delimiter);
                sb.Append(str);
                needDelim = true;
            }
            return sb.ToString();
        }

        public static void WriteCompressedUInt32(this Stream str, uint u)
        {
            while (true)
            {
                if ((u & ~0x7Fu) == 0)
                {
                    str.WriteByte(unchecked((byte)u));
                    break;
                }
                else
                {
                    str.WriteByte(unchecked((byte)((u & 0x7Fu) | 0x80u)));
                    u = (u >> 7);
                }
            }
        }

        public static uint ReadCompressedUInt32(this Stream str)
        {
            uint result = 0u;
            int shift = 0;

            while (true)
            {
                int x = str.ReadByte();
                if (x == -1) throw new EndOfStreamException("Expected byte of a compressed UInt32");
                result |= unchecked((uint)(x & 0x7F)) << shift;
                if ((x & 0x80) == 0) break;
                shift += 7;
            }

            return result;
        }

        public static void WriteCompressedInt32(this Stream str, int i)
        {
            str.WriteCompressedUInt32(unchecked((uint)i));
        }

        public static int ReadCompressedInt32(this Stream str)
        {
            return unchecked((int)(str.ReadCompressedUInt32()));
        }

        public static void WriteByteArray(this Stream str, byte[] b)
        {
            str.WriteCompressedInt32(b.Length);
            str.Write(b, 0, b.Length);
        }

        public static byte[] ReadByteArray(this Stream str)
        {
            int len = str.ReadCompressedInt32();
            return str.ReadFixedByteArray(len);
        }

        public static byte[] ReadFixedByteArray(this Stream str, int len)
        {
            byte[] b = new byte[len];
            int actuallyRead = str.Read(b, 0, b.Length);
            if (actuallyRead != len) throw new Exception("Could not read entire byte array");
            return b;
        }

        public static byte ReadByteOrDie(this Stream str)
        {
            int b = str.ReadByte();
            if (b == -1) throw new EndOfStreamException("Byte expected");
            return unchecked((byte)b);
        }

        public static IEnumerable<T> SingleItem<T>(T item)
        {
            yield return item;
        }

        public static Dictionary<T, U> Clone<T, U>(this Dictionary<T, U> dict)
        {
            Dictionary<T, U> dict2 = new Dictionary<T, U>();
            foreach(KeyValuePair<T, U> kvp in dict)
            {
                dict2.Add(kvp.Key, kvp.Value);
            }
            return dict2;
        }

        public static IEnumerable<Tuple<int, T>> Numbered<T>(this IEnumerable<T> items)
        {
            int i = 0;
            foreach (T item in items)
            {
                yield return new Tuple<int, T>(i, item);
                ++i;
            }
        }

        public static void Reverse<T>(this List<T> list)
        {
            int i = 0;
            int j = list.Count;
            while (j > i)
            {
                --j;
                T item = list[i]; list[i] = list[j]; list[j] = item;
                ++i;
            }
        }

        public static Func<T, T, bool> CacheMapLessThan<T, U>(Func<T, U> map, Func<U, U, bool> lessThan)
        {
            Dictionary<T, U> cache = new Dictionary<T, U>();

            Func<T, T, bool> result = delegate(T a, T b)
            {
                U aa;
                if (cache.ContainsKey(a))
                {
                    aa = cache[a];
                }
                else
                {
                    aa = map(a);
                    cache.Add(a, aa);
                }

                U bb;
                if (cache.ContainsKey(b))
                {
                    bb = cache[b];
                }
                else
                {
                    bb = map(b);
                    cache.Add(b, bb);
                }

                return lessThan(aa, bb);
            };

            return result;
        }
    }
}