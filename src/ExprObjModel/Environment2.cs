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
using System.Linq;

namespace ExprObjModel
{
    [Serializable]
    public class Box
    {
        public Box() { inited = false; }
        public Box(object contents) { inited = true; this.contents = contents; }

        private bool inited;
        private object contents;

        public bool HasContents { get { return inited; } }

        public object Contents
        {
            get
            {
                if (!inited) throw new SchemeRuntimeException("Attempt to read from uninitialized variable!");
                return contents;
            }
            set
            {
                inited = true; contents = value;
            }
        }
    }

    // EnvSpec is a set of symbols.
    [Serializable]
    public class EnvSpec : IEnumerable<Symbol>
    {
        // this is supposed to be a set of symbols, with members such as union,
        // intersection, is empty, etc.

        private HashSet<Symbol> s;

        private EnvSpec()
        {
            s = new HashSet<Symbol>();
        }

        private EnvSpec(HashSet<Symbol> s)
        {
            this.s = s;
        }

        public static EnvSpec FromEnumerable(IEnumerable<Symbol> symList)
        {
            HashSet<Symbol> s = new HashSet<Symbol>();
            s.UnionWith(symList);
            return new EnvSpec(s);
        }

        public static EnvSpec FromArray(Symbol[] symList)
        {
            HashSet<Symbol> s = new HashSet<Symbol>();
            s.UnionWith(symList);
            return new EnvSpec(s);
        }

        private static EnvSpec emptySet = new EnvSpec();

        public static EnvSpec EmptySet { get { return emptySet; } }

        private EnvSpec(Symbol sym)
        {
            s = new HashSet<Symbol>();
            s.Add(sym);
        }

        public static EnvSpec Only(Symbol sym)
        {
            return new EnvSpec(sym);
        }

        public Symbol[] ToArray()
        {
            return s.ToArray();
        }

        public object ToSchemeList()
        {
            return ConsCell.MapEnumerable(s, x => x);
        }

        public static EnvSpec operator |(EnvSpec a, EnvSpec b)
        {
            HashSet<Symbol> r = new HashSet<Symbol>();
            r.UnionWith(a.s);
            r.UnionWith(b.s);
            return new EnvSpec(r);
        }

        public static EnvSpec operator |(EnvSpec a, Symbol b)
        {
            HashSet<Symbol> r = new HashSet<Symbol>();
            r.UnionWith(a.s);
            r.Add(b);
            return new EnvSpec(r);
        }

        public static EnvSpec operator &(EnvSpec a, EnvSpec b)
        {
            HashSet<Symbol> r = new HashSet<Symbol>();
            r.UnionWith(a.s);
            r.IntersectWith(b.s);
            return new EnvSpec(r);
        }

        public static EnvSpec operator -(EnvSpec a, EnvSpec b)
        {
            HashSet<Symbol> r = new HashSet<Symbol>();
            r.UnionWith(a.s);
            r.ExceptWith(b.s);
            return new EnvSpec(r);
        }

        public static EnvSpec operator -(EnvSpec a, Symbol b)
        {
            HashSet<Symbol> r = new HashSet<Symbol>();
            r.UnionWith(a.s);
            r.Remove(b);
            return new EnvSpec(r);
        }

        public static EnvSpec operator ^(EnvSpec a, EnvSpec b)
        {
            HashSet<Symbol> r = new HashSet<Symbol>();
            r.UnionWith(a.s);
            r.SymmetricExceptWith(b.s);
            return new EnvSpec(r);
        }

        public bool IsEmpty { get { return s.Count == 0; } }

        public bool Contains(Symbol a)
        {
            return s.Contains(a);
        }

        public int Count { get { return s.Count; } }

        #region IEnumerable<Symbol> Members

        public IEnumerator<Symbol> GetEnumerator()
        {
            return s.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return s.GetEnumerator();
        }

        #endregion
    }

    public class VarSpec
    {
        private Symbol name;
        private object value;

        public VarSpec(Symbol name, object value)
        {
            this.name = name;
            this.value = value;
        }

        public Symbol Name { get { return name; } }
        public object Value { get { return value; } }
    }

    public static partial class Utils
    {
        public static EnvSpec EnvSpecUnion(this IEnumerable<EnvSpec> e)
        {
            EnvSpec r = EnvSpec.EmptySet;
            foreach (EnvSpec f in e)
            {
                r |= f;
            }
            return r;
        }

        public static Tuple<IExpression, Environment> Compile(IEnumerable<VarSpec> vars, IExpressionSource expr)
        {
            TopLevelEnvironment topEnv = new TopLevelEnvironment();
            foreach (VarSpec vs in vars)
            {
                topEnv.Define(vs.Name, vs.Value);
            }

            EnvSpec envSpec = expr.GetRequirements();
            Environment env;
            EnvDesc envDesc;
            topEnv.CreateEnvironment(envSpec, out envDesc, out env);
            IExpression exp = expr.Compile(envDesc);

            return new Tuple<IExpression, Environment>(exp, env);
        }
    }

    public class EnvDesc: IEnumerable<KeyValuePair<Symbol, int>>
    {
        private static EnvDesc empty = null;
        private static void InitEmpty() { if (empty == null) empty = new EnvDesc(); }
        public static EnvDesc Empty { get { InitEmpty(); return empty; } }
        
        private Dictionary<Symbol, int> dict;
        private int lastIndex;

        private EnvDesc()
        {
            dict = new Dictionary<Symbol, int>();
            lastIndex = 0;
        }

        private EnvDesc(EnvDesc src)
        {
            dict = new Dictionary<Symbol, int>();
            foreach (KeyValuePair<Symbol, int> kvp in src.dict)
            {
                dict.Add(kvp.Key, kvp.Value);
            }
            lastIndex = src.lastIndex;
        }

        private int Add(Symbol s)
        {
            int i = lastIndex;
            dict.Add(s, i);
            ++lastIndex;
            return i;
        }

        public static explicit operator EnvSpec(EnvDesc d)
        {
            return EnvSpec.FromEnumerable(d.dict.Select(x => x.Key));
        }

        [Obsolete]
        public EnvSpec GetEnvSpec()
        {
            Symbol[] s = new Symbol[dict.Count];
            int index = 0;
            foreach (KeyValuePair<Symbol, int> kvp in dict)
            {
                s[index] = kvp.Key;
                ++index;
            }
            return EnvSpec.FromArray(s);
        }

        public bool Defines(Symbol s) { return dict.ContainsKey(s); }

        public int Count { get { return lastIndex; } }

        public int this[Symbol s] { get { return dict[s]; } }

        public void Extend(Symbol[] names, out EnvDesc result)
        {
            EnvSpec e = EnvSpec.FromArray(names);
            if (e.Count != names.Length) throw new Exception("EnvDesc.Extend: name used more than once!");

            result = new EnvDesc(this);
            foreach (Symbol s1 in names)
            {
                result.Add(s1);
            }
        }

        public void ShadowExtend(Symbol[] names, out EnvDesc result, out int[] mapping)
        {
            EnvSpec e = EnvSpec.FromArray(names);
            if (e.Count != names.Length) throw new Exception("EnvDesc.ShadowExtend: name used more than once!");

            EnvSpec captures = ((EnvSpec)this) - e;

            result = new EnvDesc();
            mapping = new int[captures.Count];
            foreach (Symbol s in captures)
            {
                int i = result.Add(s);
                mapping[i] = this[s];
            }
            foreach (Symbol s1 in names)
            {
                result.Add(s1);
            }
        }

        public void Subset(EnvSpec requirements, out EnvDesc subsetted, out int[] mapping)
        {
            if ((requirements - ((EnvSpec)this)).Count > 0) throw new Exception("EnvDesc.Subset: requirement for nonexistent name!");

            subsetted = new EnvDesc();
            mapping = new int[requirements.Count];
            foreach (Symbol s in requirements)
            {
                int i = subsetted.Add(s);
                mapping[i] = this[s];
            }
        }

        public void SubsetShadowExtend(EnvSpec requirements, Symbol[] names, out EnvDesc subsettedShadowedExtended, out int[] mapping)
        {
            EnvSpec e = EnvSpec.FromArray(names);
            if (e.Count != names.Length) throw new Exception("EnvDesc.SubsetShadowExtend: name used more than once!");

            if ((requirements - ((EnvSpec)this)).Count > 0) throw new Exception("EnvDesc.SubsetShadowExtend: requirement for nonexistent name!");
            EnvSpec captures = requirements - e;

            subsettedShadowedExtended = new EnvDesc();
            mapping = new int[captures.Count];
            foreach (Symbol s in captures)
            {
                int i = subsettedShadowedExtended.Add(s);
                mapping[i] = this[s];
            }
            foreach (Symbol s1 in names)
            {
                subsettedShadowedExtended.Add(s1);
            }
        }

        public void SubsetShadowExtend(EnvSpec requirements, Symbol loopName, Symbol[] names, out EnvDesc subsettedShadowedExtended, out int[] mapping)
        {
            EnvSpec e = EnvSpec.FromArray(names);
            if (e.Count != names.Length) throw new Exception("EnvDesc.SubsetShadowExtend: name used more than once!");

            if ((requirements - ((EnvSpec)this)).Count > 0) throw new Exception("EnvDesc.SubsetShadowExtend: requirement for nonexistent name!");
            
            EnvSpec captures = requirements - e;
            captures = requirements - loopName;

            subsettedShadowedExtended = new EnvDesc();
            mapping = new int[captures.Count];
            foreach (Symbol s in captures)
            {
                int i = subsettedShadowedExtended.Add(s);
                mapping[i] = this[s];
            }
            subsettedShadowedExtended.Add(loopName);
            foreach (Symbol s1 in names)
            {
                subsettedShadowedExtended.Add(s1);
            }
        }

        #region IEnumerable<KeyValuePair<Symbol,int>> Members

        public IEnumerator<KeyValuePair<Symbol, int>> GetEnumerator()
        {
            return dict.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)dict).GetEnumerator();
        }

        #endregion
    }

    [Serializable]
    public class Environment
    {
        private static Environment empty = null;
        private static void InitEmpty() { if (empty == null) empty = new Environment(null); }
        public static Environment Empty { get { InitEmpty(); return empty; } }
        
        private Box[] vars;

        private Environment(Box[] vars)
        {
            this.vars = vars;
        }

        public Box GetBox(int i)
        {
            return vars[i];
        }

        public object this[int i] { get { return GetBox(i).Contents; } set { GetBox(i).Contents = value; } }

        public Environment Extend(int[] mapping, Box[] boxes)
        {
            int iend = mapping.Length;
            int jend = boxes.Length;
            Box[] b2 = new Box[iend + jend];
            for (int i = 0; i < iend; ++i)
            {
                b2[i] = vars[mapping[i]];
            }
            for (int j = 0; j < jend; ++j)
            {
                b2[iend + j] = boxes[j];
            }
            return new Environment(b2);
        }

        public Environment Extend(int[] mapping, object[] values)
        {
            int iend = mapping.Length;
            int jend = values.Length;
            Box[] b2 = new Box[iend + jend];
            for (int i = 0; i < iend; ++i)
            {
                b2[i] = vars[mapping[i]];
            }
            for (int j = 0; j < jend; ++j)
            {
                b2[iend + j] = new Box(values[j]);
            }
            return new Environment(b2);
        }

        public Environment Extend(int[] mapping, int nValues)
        {
            int iend = mapping.Length;
            Box[] b2 = new Box[iend + nValues];
            for (int i = 0; i < iend; ++i)
            {
                b2[i] = vars[mapping[i]];
            }
            for (int j = 0; j < nValues; ++j)
            {
                b2[iend + j] = new Box();
            }
            return new Environment(b2);
        }

        public Environment Extend(Box[] boxes)
        {
            int iend = (vars == null) ? 0 : vars.Length;
            int jend = boxes.Length;
            Box[] b2 = new Box[iend + jend];
            if (vars != null) Array.Copy(vars, 0, b2, 0, iend);
            for (int j = 0; j < jend; ++j)
            {
                b2[iend + j] = boxes[j];
            }
            return new Environment(b2);
        }

        public Environment Extend(object[] values)
        {
            int iend = (vars == null) ? 0 : vars.Length;
            int jend = values.Length;
            Box[] b2 = new Box[iend + jend];
            if (vars != null) Array.Copy(vars, 0, b2, 0, vars.Length);
            for (int j = 0; j < jend; ++j)
            {
                b2[iend + j] = new Box(values[j]);
            }
            return new Environment(b2);
        }

        public Environment Extend(IProcedure loopBody, object[] values)
        {
            int iend = (vars == null) ? 0 : vars.Length;
            int jend = values.Length;
            Box[] b2 = new Box[iend + 1 + jend];
            if (vars != null) Array.Copy(vars, 0, b2, 0, vars.Length);
            b2[iend] = new Box(loopBody);
            for (int j = 0; j < jend; ++j)
            {
                b2[iend + 1 + j] = new Box(values[j]);
            }
            return new Environment(b2);
        }
    }
}