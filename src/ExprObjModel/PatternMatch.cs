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
using ExprObjModel;

namespace ExprObjModel.SyntaxAnalysis
{
    /*

    There are four types of pattern matchers:

    [1] Match against an atom -- this succeeds or fails.
    
    [2] Match into a variable -- this always succeeds, and binds the variable.
    
    [3] Structural composite -- match against a structure (such as a ConsCell); has subpatterns
        which must match against the parts of the structure.

    [4] Alternative -- match against either pattern A or pattern B. (However, you have to
        call different routines because the variables are likely to be different.)

    */

    public class MatchCaptureSet
    {
        private MatchCaptureSet()
        {
            vars = new Dictionary<Symbol, object>();
        }

        private static MatchCaptureSet emptySet = new MatchCaptureSet();

        public static MatchCaptureSet EmptySet { get { return emptySet; } }

        public MatchCaptureSet(Symbol k, object v)
        {
            vars = new Dictionary<Symbol, object>();
            vars.Add(k, v);
        }

        private MatchCaptureSet(MatchCaptureSet other)
        {
            vars = new Dictionary<Symbol, object>();
            foreach(KeyValuePair<Symbol, object> pair in other.vars)
            {
                vars.Add(pair.Key, pair.Value);
            }
        }

        private Dictionary<Symbol, object> vars;

        private void Add(Symbol k, object v)
        {
            if (vars.ContainsKey(k))
            {
                vars.Remove(k);
            }
            vars.Add(k, v);
        }

        public bool Contains(string s)
        {
            Symbol sym = new Symbol(s);
            return vars.ContainsKey(sym);
        }

        public bool Contains(Symbol sym)
        {
            return vars.ContainsKey(sym);
        }

        public object this[string s]
        {
            get
            {
                Symbol sym = new Symbol(s);
                return vars[sym];
            }
        }

        public object this[Symbol sym]
        {
            get
            {
                return vars[sym];
            }
        }

        public MatchCaptureSet Union(Symbol sym, object value)
        {
            MatchCaptureSet m = new MatchCaptureSet(this);
            m.Add(sym, value);
            return m;
        }

        public MatchCaptureSet Union(MatchCaptureSet m)
        {
            if (m == EmptySet) return this;
            MatchCaptureSet result = new MatchCaptureSet(this);
            foreach (KeyValuePair<Symbol, object> pair in m.vars)
            {
                result.Add(pair.Key, pair.Value);
            }
            return result;
        }
    }

    public interface IPattern
    {
        MatchCaptureSet Match(object data);
    }

    public class AtomPattern<T> : IPattern
    {
        private static IEqualityComparer<T> eqTester = EqualityComparer<T>.Default;

        public static void SetEqualityComparer(IEqualityComparer<T> et)
        {
            eqTester = et;
        }

        public AtomPattern(T value)
        {
            this.value = value;
        }

        private T value;

        public MatchCaptureSet Match(object data)
        {
            if (data is T)
            {
                T tdata = (T)data;
                if (eqTester.Equals(tdata, value))
                {
                    return MatchCaptureSet.EmptySet;
                }
            }
            return null;
        }
    }

    public class VarPattern : IPattern
    {
        public VarPattern(Symbol var)
        {
            this.var = var;
        }

        private Symbol var;

        public MatchCaptureSet Match(object data)
        {
            return new MatchCaptureSet(var, data);
        }
    }

    public class ConsCellPattern : IPattern
    {
        public ConsCellPattern(IPattern carPattern, IPattern cdrPattern)
        {
            this.carPattern = carPattern;
            this.cdrPattern = cdrPattern;
        }

        private IPattern carPattern;
        private IPattern cdrPattern;

        public MatchCaptureSet Match(object data)
        {
            if (!(data is ConsCell)) return null;
            ConsCell cData = (ConsCell)data;

            MatchCaptureSet m1 = carPattern.Match(cData.car);
            if (m1 == null) return null;

            MatchCaptureSet m2 = cdrPattern.Match(cData.cdr);
            if (m2 == null) return null;

            return m1.Union(m2);
        }
    }

    public class PatternSpecificationException : ApplicationException
    {
        public PatternSpecificationException() : base() { }
        public PatternSpecificationException(string msg) : base(msg) { }
        public PatternSpecificationException(string msg, Exception cause) : base(msg, cause) { }
    }

    public class PatternBuilder
    {
        public static bool IsPatternVariable(Symbol sym)
        {
            string str = sym.ToString();
            return (str.StartsWith("<") && str.EndsWith(">"));
        }

        public static IPattern BuildPattern(object desc)
        {
            if (desc is Symbol)
            {
                Symbol sDesc = (Symbol)desc;
                if (IsPatternVariable(sDesc))
                {
                    return new VarPattern(sDesc);
                }
                else
                {
                    return new AtomPattern<Symbol>(sDesc);
                }
            }
            else if (desc is ConsCell)
            {
                ConsCell cDesc = (ConsCell)desc;
                IPattern carPattern = BuildPattern(cDesc.car);
                IPattern cdrPattern = BuildPattern(cDesc.cdr);
                return new ConsCellPattern(carPattern, cdrPattern);
            }
            else if (desc is SpecialValue)
            {
                SpecialValue sv = (SpecialValue)desc;
                return new AtomPattern<SpecialValue>(sv);
            }
            else if (desc is int)
            {
                int i = (int)desc;
                return new AtomPattern<int>(i);
            }
            else if (desc is string)
            {
                string s = (string)desc;
                return new AtomPattern<string>(s);
            }
            else if (desc is bool)
            {
                bool b = (bool)desc;
                return new AtomPattern<bool>(b);
            }
            else if (desc is char)
            {
                char ch = (char)desc;
                return new AtomPattern<char>(ch);
            }
            else throw new PatternSpecificationException("Unknown pattern specification: " + desc.GetType());
        }

        public static IPattern BuildPattern(string desc)
        {
            return BuildPattern(SchemeDataReader.ReadItem(desc));
        }
    }
}