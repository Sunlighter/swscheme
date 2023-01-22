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
using System.Text;
using System.IO;
using System.ComponentModel;
using ControlledWindowLib;

namespace ExprObjModel.Procedures
{
    public static partial class ProxyDiscovery
    {
        [SchemeFunction("make-vector")]
        public static Deque<object> MakeDeque(int count, object value)
        {
            Deque<object> d = new Deque<object>();
            for (int i = 0; i < count; ++i) d.PushBack(value);
            return d;
        }

        [SchemeFunction("vector?")]
        public static bool IsVector(object obj)
        {
            return (obj is Deque<object>);
        }

        [SchemeFunction("vector-ref")]
        public static object DequeRef(Deque<object> d, int index)
        {
            return d[index];
        }

        [SchemeFunction("vector-set!")]
        public static void DequeSet(Deque<object> d, int index, object val)
        {
            d[index] = val;
        }

        [SchemeFunction("vector-length")]
        public static int DequeLength(Deque<object> d)
        {
            return d.Count;
        }

        [SchemeFunction("vector-push-front!")]
        public static void DequePushFront(Deque<object> d, object f)
        {
            d.PushFront(f);
        }

        [SchemeFunction("vector-push-back!")]
        public static void DequePushBack(Deque<object> d, object f)
        {
            d.PushBack(f);
        }

        [SchemeFunction("vector-pop-front!")]
        public static object DequePopFront(Deque<object> d)
        {
            return d.PopFront();
        }

        [SchemeFunction("vector-pop-back!")]
        public static object DequePopBack(Deque<object> d)
        {
            return d.PopBack();
        }

        [SchemeFunction("vector-front-ref")]
        public static object DequeFrontRef(Deque<object> d)
        {
            return d.Front;
        }

        [SchemeFunction("vector-front-set!")]
        public static void DequeFrontSet(Deque<object> d, object f)
        {
            d.Front = f;
        }

        [SchemeFunction("vector-back-ref")]
        public static object DequeBackRef(Deque<object> d)
        {
            return d.Back;
        }

        [SchemeFunction("vector-back-set!")]
        public static void DequeBackSet(Deque<object> d, object f)
        {
            d.Back = f;
        }

    }

    [SchemeSingleton("vector")]
    public class VectorProc : IProcedure
    {
        public VectorProc() { }

        public int Arity { get { return 0; } }
        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            Deque<object> result = new Deque<object>();
            while (argList != null)
            {
                result.PushBack(argList.Head);
                argList = argList.Tail;
            }
            return new RunnableReturn(k, result);
        }
    }
}