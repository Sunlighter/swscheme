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
    public class Deque<T> : IEnumerable<T>
    {
        protected T[] arr;
        protected int offset;
        protected int length;

        public Deque()
        {
            arr = new T[10];
            offset = 0;
            length = 0;
        }

        public Deque(T[] obj)
        {
            arr = new T[PreferredCapacity(obj.Length)];
            offset = 0;
            length = obj.Length;
            Array.Copy(obj, 0, arr, 0, length);
        }

        public Deque(Deque<T> src)
        {
            arr = new T[src.arr.Length];
            offset = 0;
            length = src.length;
            Utils.CopyUp(src.GetSegs().First(src.length), this.GetSegs().First(src.length));
        }

        public Deque(IEnumerable<T> src)
            : this()
        {
            foreach (T item in src) PushBack(item);
        }

        public void Clear()
        {
            length = 0;
        }

        private static int Modulo(int ptr, int size)
        {
            ptr %= size; if (ptr < 0) ptr += size;
            return ptr;
        }

        private FList<ArraySegment<T>> GetSegs()
        {
            if (offset == 0)
            {
                return new FList<ArraySegment<T>>(new ArraySegment<T>(arr, 0, arr.Length));
            }
            else
            {
                ArraySegment<T> a = new ArraySegment<T>(arr, offset, arr.Length - offset);
                ArraySegment<T> b = new ArraySegment<T>(arr, 0, offset);
                return new FList<ArraySegment<T>>(a, new FList<ArraySegment<T>>(b));
            }
        }

        private static FList<ArraySegment<T>> GetSegs(T[] arr)
        {
            return new FList<ArraySegment<T>>(new ArraySegment<T>(arr, 0, arr.Length));
        }

        private void Realloc(int newSize)
        {
            if (arr.Length == newSize) return;

            T[] newArr = new T[newSize];
            Utils.CopyUp(GetSegs().First(length), GetSegs(newArr).First(length));
            arr = newArr;
            offset = 0;
        }

        private void ReallocUp(int newSize)
        {
            if (arr.Length >= newSize) return;

            T[] newArr = new T[newSize];
            Utils.CopyUp(GetSegs().First(length), GetSegs(newArr).First(length));
            arr = newArr;
            offset = 0;
        }

        private int PreferredCapacity(int size)
        {
            int capacity = 10;
            while (capacity < size) capacity <<= 1;
            return capacity;
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= length) throw new IndexOutOfRangeException();
                int i = offset + index;
                int iEnd = arr.Length;
                if (i >= iEnd) i -= iEnd;
                return arr[i];
            }
            set
            {
                if (index < 0 || index >= length) throw new IndexOutOfRangeException();
                int i = offset + index;
                int iEnd = arr.Length;
                if (i >= iEnd) i -= iEnd;
                arr[i] = value;
            }
        }

        public int Count { get { return length; } }

        public int Capacity
        {
            get
            {
                return arr.Length;
            }
            set
            {
                if (value < length) throw new ArgumentOutOfRangeException("Can't set deque capacity below size!");
                Realloc(value);
            }
        }

        public bool IsEmpty { get { return length == 0; } }

        private void AllocBack(int count)
        {
            ReallocUp(PreferredCapacity(length + count));
            length += count;
        }

        public void PushBack(T obj)
        {
            AllocBack(1);
            this[length - 1] = obj;
        }

        public void PushBack(T[] src)
        {
            AllocBack(src.Length);
            Utils.CopyUp(GetSegs(src), GetSegs().Skip(length - src.Length).First(src.Length));
        }

        public void PushBack(T[] src, int sOff, int sLen)
        {
            if (sOff < 0 || (sOff + sLen) > src.Length) throw new ArgumentOutOfRangeException();
            AllocBack(sLen);
            Utils.CopyUp(GetSegs(src).Skip(sOff).First(sLen), GetSegs().Skip(length - src.Length).First(src.Length));
        }

        public void PushBack(Deque<T> src)
        {
            AllocBack(src.length);
            Utils.CopyUp(src.GetSegs().First(src.length), GetSegs().Skip(length - src.length).First(src.length));
        }

        public void PushBack(Deque<T> src, int sOff, int sLen)
        {
            if (sOff < 0 || (sOff + sLen) > src.Count) throw new ArgumentOutOfRangeException();
            AllocBack(sLen);
            Utils.CopyUp(src.GetSegs().Skip(sOff).First(sLen), GetSegs().Skip(length - src.length).First(sLen));
        }

        public void PushBack(T src, int count)
        {
            AllocBack(count);
            Utils.Fill(src, GetSegs().Skip(length - count).First(count));
        }

        public T PopBack()
        {
            if (length == 0) throw new InvalidOperationException("Attempt to pop back of an empty deque");
            T result = this[length - 1];
            this[length - 1] = default(T);
            --length;
            Realloc(PreferredCapacity(length));
            return result;
        }

        public void PopBack(int count)
        {
            if (length < count) throw new ArgumentOutOfRangeException("Attempt to pop " + count + " items off a deque with only " + length + " items");
            Utils.Fill(default(T), GetSegs().Skip(length - count).First(count));
            length -= count;
            Realloc(PreferredCapacity(length));
        }

        private void AllocFront(int count)
        {
            ReallocUp(PreferredCapacity(length + count));
            length += count;
            offset -= count;
            if (offset < 0) offset += arr.Length;
        }

        public void PushFront(T src)
        {
            AllocFront(1);
            this[0] = src;
        }

        public void PushFront(T[] src)
        {
            AllocFront(src.Length);
            Utils.CopyUp(GetSegs(src), GetSegs().First(src.Length));
        }

        public void PushFront(T[] src, int sOff, int sLen)
        {
            if (sOff < 0 || (sOff + sLen) > src.Length) throw new ArgumentOutOfRangeException();
            AllocFront(sLen);
            Utils.CopyUp(GetSegs(src).Skip(sOff).First(sLen), GetSegs().First(sLen));
        }

        public void PushFront(Deque<T> src)
        {
            AllocFront(src.length);
            Utils.CopyUp(src.GetSegs().First(src.length), GetSegs().First(src.length));
        }

        public void PushFront(Deque<T> src, int sOff, int sLen)
        {
            if (sOff < 0 || (sOff + sLen) > src.Count) throw new ArgumentOutOfRangeException();
            AllocFront(sLen);
            Utils.CopyUp(src.GetSegs().Skip(sOff).First(sLen), GetSegs().First(sLen));
        }

        public void PushFront(T src, int count)
        {
            AllocFront(count);
            Utils.Fill(src, GetSegs().First(count));
        }

        public T PopFront()
        {
            if (length == 0) throw new InvalidOperationException("Attempt to pop front of an empty deque");
            T result = this[0];
            this[0] = default(T);
            --length; ++offset;
            if (offset >= arr.Length) offset -= arr.Length;
            Realloc(PreferredCapacity(length));
            return result;
        }

        public void PopFront(int count)
        {
            if (length < count) throw new ArgumentOutOfRangeException("Attempt to pop " + count + " items off a deque with only " + length + " items");
            Utils.Fill(default(T), GetSegs().First(count));
            length -= count;
            offset = Modulo(offset + count, arr.Length);
            Realloc(PreferredCapacity(length));
        }

        public T Front
        {
            get
            {
                if (length == 0) throw new InvalidOperationException("Empty deque has no front");
                return this[0];
            }
            set
            {
                if (length == 0) throw new InvalidOperationException("Empty deque has no front");
                this[0] = value;
            }
        }

        public T Back
        {
            get
            {
                if (length == 0) throw new InvalidOperationException("Empty deque has no back");
                return this[length - 1];
            }
            set
            {
                if (length == 0) throw new InvalidOperationException("Empty deque has no back");
                this[length - 1] = value;
            }
        }

        public T[] ToArray()
        {
            T[] obj = new T[length];
            Utils.CopyUp(GetSegs().First(length), GetSegs(obj));
            return obj;
        }

        public void AllocMiddle(int index, int count)
        {
            int itemsAfter = length - index;
            if (index < itemsAfter)
            {
                AllocFront(count);
                Utils.CopyUp(GetSegs().Skip(index).First(count), GetSegs().First(count));
            }
            else
            {
                AllocBack(count);
                Utils.CopyDown(GetSegs().Skip(index).First(itemsAfter), GetSegs().Skip(index + count).First(itemsAfter));
            }
        }

        public void AddAt(int index, T item)
        {
            if (index < 0 || index > length) throw new IndexOutOfRangeException();
            AllocMiddle(index, 1);
            this[index] = item;
        }

        public void AddAt(int index, T[] src)
        {
            if (index < 0 || index > length) throw new IndexOutOfRangeException();
            AllocMiddle(index, src.Length);
            Utils.CopyUp(GetSegs(src), GetSegs().Skip(index).First(src.Length));
        }

        public void AddAt(int index, T[] src, int sOff, int sLen)
        {
            if (index < 0 || index > length) throw new IndexOutOfRangeException();
            if (sOff < 0 || (sOff + sLen) < src.Length) throw new ArgumentOutOfRangeException();
            AllocMiddle(index, sLen);
            Utils.CopyUp(GetSegs(src).Skip(sOff).First(sLen), GetSegs().Skip(index).First(sLen));
        }

        public void AddAt(int index, Deque<T> src)
        {
            if (index < 0 || index > length) throw new IndexOutOfRangeException();
            AllocMiddle(index, src.length);
            Utils.CopyUp(src.GetSegs().First(src.length), GetSegs().Skip(index).First(src.length));
        }

        public void AddAt(int index, Deque<T> src, int sOff, int sLen)
        {
            if (index < 0 || index > length) throw new IndexOutOfRangeException();
            if (sOff < 0 || (sOff + sLen) < src.length) throw new ArgumentOutOfRangeException();
            AllocMiddle(index, sLen);
            Utils.CopyUp(src.GetSegs().Skip(sOff).First(sLen), GetSegs().Skip(index).First(sLen));
        }

        public void RemoveAt(int index)
        {
            RemoveAt(index, 1);
        }

        public void RemoveAt(int index, int count)
        {
            if (count < 0) throw new ArgumentException("Count cannot be negative");
            if (index < 0 || (index + count) > length) throw new IndexOutOfRangeException();
            int itemsAfter = (length - count) - index;
            if (index < itemsAfter)
            {
                Utils.CopyDown(GetSegs().First(count), GetSegs().Skip(index).First(count));
                PopFront(count);
            }
            else
            {
                Utils.CopyUp(GetSegs().Skip(index + count).First(itemsAfter), GetSegs().Skip(index).First(itemsAfter));
                PopBack(count);
            }
        }

        private IEnumerator<T> InternalGetEnumerator()
        {
            int p = offset;
            int pEnd = arr.Length;
            int pLen = length;
            while (pLen > 0)
            {
                yield return arr[p];
                ++p;
                if (p >= pEnd) p = 0;
                --pLen;
            }
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return InternalGetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return InternalGetEnumerator();
        }

        #endregion
    }

    internal static partial class Utils
    {
        public static void CopyUp<T>(ArraySegment<T> src, ArraySegment<T> dest)
        {
            if (src.Count != dest.Count) throw new ArgumentException("Array segments must be the same length");
            T[] srcArr = src.Array;
            T[] destArr = dest.Array;
            int srcPtr = src.Offset;
            int destPtr = dest.Offset;
            int i = src.Count;
            if (srcPtr + i < i) throw new OverflowException();
            if (destPtr + i < i) throw new OverflowException();
            while (i > 0)
            {
                --i;
                destArr[destPtr] = srcArr[srcPtr];
                ++srcPtr;
                ++destPtr;
            }
        }

        private static int NextSegLength<T>(FList<ArraySegment<T>> segs)
        {
            if (segs == null) return 0;
            return segs.Head.Count;
        }

        public static FList<ArraySegment<T>> First<T>(this FList<ArraySegment<T>> segs, int count)
        {
            FList<ArraySegment<T>> result = null;
            while (true)
            {
                if (count == 0 || segs == null) return FListUtils.Reverse(result);
                if (count < segs.Head.Count)
                {
                    result = new FList<ArraySegment<T>>(new ArraySegment<T>(segs.Head.Array, segs.Head.Offset, count), result);
                    return FListUtils.Reverse(result);
                }
                else
                {
                    result = new FList<ArraySegment<T>>(segs.Head);
                    count -= segs.Head.Count;
                    segs = segs.Tail;
                }
            }
        }

        public static FList<ArraySegment<T>> Skip<T>(this FList<ArraySegment<T>> segs, int count)
        {
            while (true)
            {
                if (count == 0 || segs == null) return segs;
                if (count < segs.Head.Count)
                {
                    return new FList<ArraySegment<T>>(new ArraySegment<T>(segs.Head.Array, segs.Head.Offset + count, segs.Head.Count - count), segs.Tail);
                }
                else
                {
                    count -= segs.Head.Count;
                    segs = segs.Tail;
                }
            }
        }

        private static FList<ArraySegment<T>> RemoveLast<T>(FList<ArraySegment<T>> segsReversed, int count)
        {
            while (true)
            {
                if (count == 0 || segsReversed == null) return segsReversed;
                if (count < segsReversed.Head.Count)
                {
                    return new FList<ArraySegment<T>>(new ArraySegment<T>(segsReversed.Head.Array, segsReversed.Head.Offset, segsReversed.Head.Count - count), segsReversed.Tail);
                }
                else
                {
                    count -= segsReversed.Head.Count;
                    segsReversed = segsReversed.Tail;
                }
            }
        }

        private static int TotalLength<T>(FList<ArraySegment<T>> segs)
        {
            int count = 0;
            while (true)
            {
                if (segs == null) return count;
                count = checked(count + segs.Head.Count);
                segs = segs.Tail;
            }
        }

        public static void CopyUp<T>(FList<ArraySegment<T>> src, FList<ArraySegment<T>> dest)
        {
            if (TotalLength(src) != TotalLength(dest)) throw new ArgumentException("Array segment lists must be of equal length");
            while (true)
            {
                if (src == null && dest == null) break;
                if (src == null || dest == null) { System.Diagnostics.Debug.Assert(false); break; }
                if (src.Head.Count < dest.Head.Count)
                {
                    CopyUp(src.Head, new ArraySegment<T>(dest.Head.Array, dest.Head.Offset, src.Head.Count));
                    dest = new FList<ArraySegment<T>>(new ArraySegment<T>(dest.Head.Array, dest.Head.Offset + src.Head.Count, dest.Head.Count - src.Head.Count), dest.Tail);
                    src = src.Tail;
                }
                else if (src.Head.Count > dest.Head.Count)
                {
                    CopyUp(new ArraySegment<T>(src.Head.Array, src.Head.Offset, dest.Head.Count), dest.Head);
                    src = new FList<ArraySegment<T>>(new ArraySegment<T>(src.Head.Array, src.Head.Offset + dest.Head.Count, src.Head.Count - dest.Head.Count), src.Tail);
                    dest = dest.Tail;
                }
                else
                {
                    CopyUp(src.Head, dest.Head);
                    src = src.Tail;
                    dest = dest.Tail;
                }
            }
        }

        public static void CopyDown<T>(ArraySegment<T> src, ArraySegment<T> dest)
        {
            if (src.Count != dest.Count) throw new ArgumentException("Array segments must be the same length");
            T[] srcArr = src.Array;
            T[] destArr = dest.Array;
            int srcPtr = checked(src.Offset + src.Count);
            int destPtr = checked(dest.Offset + dest.Count);
            int i = src.Count;
            while (i > 0)
            {
                --i;
                --destPtr;
                --srcPtr;
                destArr[destPtr] = srcArr[srcPtr];
            }
        }

        public static void CopyDown<T>(FList<ArraySegment<T>> src, FList<ArraySegment<T>> dest)
        {
            if (TotalLength(src) != TotalLength(dest)) throw new ArgumentException("Array segment lists must be of equal length");
            src = FListUtils.Reverse(src);
            dest = FListUtils.Reverse(dest);
            while (true)
            {
                if (src == null && dest == null) break;
                if (src == null || dest == null) { System.Diagnostics.Debug.Assert(false); break; }
                if (src.Head.Count < dest.Head.Count)
                {
                    CopyDown(src.Head, new ArraySegment<T>(dest.Head.Array, dest.Head.Offset + (dest.Head.Count - src.Head.Count), src.Head.Count));
                    dest = new FList<ArraySegment<T>>(new ArraySegment<T>(dest.Head.Array, dest.Head.Offset, dest.Head.Count - src.Head.Count), dest.Tail);
                    src = src.Tail;
                }
                else if (dest.Head.Count < src.Head.Count)
                {
                    CopyDown(new ArraySegment<T>(src.Head.Array, src.Head.Offset + (src.Head.Count - dest.Head.Count), dest.Head.Count), dest.Head);
                    src = new FList<ArraySegment<T>>(new ArraySegment<T>(src.Head.Array, src.Head.Offset, src.Head.Count - dest.Head.Count), src.Tail);
                    dest = dest.Tail;
                }
                else
                {
                    CopyDown(src.Head, dest.Head);
                    src = src.Tail;
                    dest = dest.Tail;
                }
            }
        }

        public static void Fill<T>(T value, ArraySegment<T> dest)
        {
            int iEnd = checked(dest.Offset + dest.Count);
            T[] arr = dest.Array;
            for (int i = dest.Offset; i < iEnd; ++i)
            {
                arr[i] = value;
            }
        }

        public static void Fill<T>(T value, FList<ArraySegment<T>> dest)
        {
            while (dest != null)
            {
                Fill(value, dest.Head);
                dest = dest.Tail;
            }
        }
    }
}
