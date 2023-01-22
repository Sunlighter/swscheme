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

namespace ExprObjModel.ObjectSystem
{
    [Serializable]
    [SchemeIsAFunction("signature?")]
    public class Signature : BigMath.IHashable, IEquatable<Signature>
    {
        private Symbol type;
        private HashSet<Symbol> parameters;

        public Signature(Symbol type, IEnumerable<Symbol> parameters)
        {
            this.type = type;
            this.parameters = parameters.ToHashSet();
        }

        public Symbol Type { [SchemeFunction("signature-get-type")] get { return type; } }

        public ICountedEnumerable<Symbol> Parameters
        {
            get
            {
                return new CountedEnumerable<Symbol>
                (
                    parameters.OrderBy(x => x.IsInterned).ThenBy(x => x.Name),
                    parameters.Count
                );
            }
        }

        public void AddToHash(BigMath.IHashGenerator hg)
        {
            type.AddToHash(hg);
            hg.Add((byte)2);
            hg.Add(BitConverter.GetBytes(parameters.Count));
            foreach (Symbol s in parameters.OrderBy(x => x.IsInterned ? 1 : 0).ThenBy(x => x.Name))
            {
                s.AddToHash(hg);
                hg.Add((byte)11);
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Signature)) return false;
            return Equals((Signature)obj);
        }

        public override int GetHashCode()
        {
            BigMath.HashGenerator h = new BigMath.HashGenerator();
            AddToHash(h);
            return h.Hash;
        }

        public bool Equals(Signature other)
        {
            if (type != other.type) return false;
            return parameters.SetEquals(other.parameters);
        }

        [SchemeFunction("signature-get-parameters")]
        public SchemeHashSet SchemeGetParameters()
        {
            return SchemeHashSet.FromEnumerable(parameters);
        }

        [SchemeFunction("signature-has-parameter?")]
        public bool HasParameter(Symbol s)
        {
            return parameters.Contains(s);
        }

        [SchemeFunction("make-signature")]
        public static Signature MakeSignature(Symbol type, SchemeHashSet parameters)
        {
            if (parameters.Any(x => !(x is Symbol))) throw new SchemeRuntimeException("make-signature: parameters must be symbols");
            return new Signature(type, parameters.Cast<Symbol>());
        }

        [SchemeFunction("signature=?")]
        public static bool operator ==(Signature a, Signature b)
        {
            if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null)) return true;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null)) return false;
            return a.Equals(b);
        }

        public static bool operator !=(Signature a, Signature b)
        {
            if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null)) return false;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null)) return true;
            return !(a.Equals(b));
        }
    }

    [Serializable]
    public class Message<T>
    {
        private Symbol type;
        private Dictionary<Symbol, T> arguments;

        public Message(Symbol type, IEnumerable<Tuple<Symbol, T>> arguments)
        {
            this.type = type;
            this.arguments = arguments.ToDictionaryFavorFirst(x => x.Item1, x => x.Item2);
        }

        public Symbol Type { get { return type; } }

        public ICountedEnumerable<Tuple<Symbol, T>> Arguments
        {
            get
            {
                return new CountedEnumerable<Tuple<Symbol, T>>
                (
                    (arguments.OrderBy(x => x.Key.IsInterned).ThenBy(x => x.Key.Name)).Select(x => new Tuple<Symbol, T>(x.Key, x.Value)),
                    arguments.Count
                );
            }
        }

        public Signature Signature
        {
            get
            {
                return new Signature(type, arguments.Keys);
            }
        }

        public bool Matches(Signature s)
        {
            if (type != s.Type) return false;
            HashSet<Symbol> p1 = s.Parameters.ToHashSet();
            HashSet<Symbol> p2 = arguments.Keys.ToHashSet();
            return p1.SetEquals(p2);
        }

        public bool HasArgument(Symbol s)
        {
            return arguments.ContainsKey(s);
        }

        public T this[Symbol s]
        {
            get
            {
                return arguments[s];
            }
        }

        public Message<U> Map<U>(Func<T, U> func)
        {
            return new Message<U>(type, arguments.Select(x => new Tuple<Symbol, U>(x.Key, func(x.Value))));
        }

        public ICountedEnumerable<Symbol> Keys
        {
            get
            {
                return new CountedEnumerable<Symbol>
                (
                    arguments.Select(x => x.Key).OrderBy(x => x.IsInterned).ThenBy(x => x.Name),
                    arguments.Count
                );
            }
        }

        public ICountedEnumerable<T> Values
        {
            get
            {
                return new CountedEnumerable<T>
                (
                    (arguments.OrderBy(x => x.Key.IsInterned).ThenBy(x => x.Key.Name)).Select(x => x.Value),
                    arguments.Count
                );
            }
        }
    }
}