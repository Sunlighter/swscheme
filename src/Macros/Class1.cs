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

namespace Macros
{
    public interface IReadOnlyArray<T> : IEnumerable<T>
    {
        int Count { get; }
        T this[int index] { get; }
    }

    public class ArrayWrapper<T> : IReadOnlyArray<T>
    {
        private T[] array;

        public ArrayWrapper(T[] array)
        {
            this.array = array;
        }

        public int Count { get { return array.Length; } }

        public T this[int index] { get { return array[index]; } }

        public IEnumerator<T> GetEnumerator()
        {
            return array.AsEnumerable().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return array.GetEnumerator();
        }
    }

    public class VariantType
    {
        private string rootName;
        private string access;
        private Variation[] variations;

        public VariantType(string rootName, string access, Variation[] variations)
        {
            this.rootName = rootName;
            this.access = access;
            this.variations = variations;
        }

        public string RootName { get { return rootName; } }

        public string Access { get { return access; } }

        public IReadOnlyArray<Variation> Variations { get { return new ArrayWrapper<Variation>(variations); } }
    }

    public class Variation
    {
        private string name;
        private Field[] fields;

        public Variation(string name, Field[] fields)
        {
            this.name = name;
            this.fields = fields;
        }

        public string Name { get { return name; } }

        public IReadOnlyArray<Field> Fields { get { return new ArrayWrapper<Field>(fields); } }
    }

    public class Field
    {
        private string privateName;
        private string propertyName;
        private string type;

        public Field(string privateName, string propertyName, string type)
        {
            this.privateName = privateName;
            this.propertyName = propertyName;
            this.type = type;
        }

        public string PrivateName { get { return privateName; } }

        public string PropertyName { get { return propertyName; } }

        public string Type { get { return type; } }
    }

    public class CodeBuilder
    {
        private List<string> lines;
        private StringBuilder currentLine;
        private bool isIndented;
        private int indent;

        public CodeBuilder(int indent)
        {
            lines = new List<string>();
            currentLine = new StringBuilder();
            this.indent = indent;
        }

        public void Newline()
        {
            lines.Add(currentLine.ToString());
            currentLine.Clear();
            isIndented = false;
        }

        public void Append(string str)
        {
            if (!isIndented)
            {
                currentLine.Append(new string(' ', indent));
                isIndented = true;
            }
            currentLine.Append(str);
        }

        public void AppendLine(string str)
        {
            Append(str);
            Newline();
        }

        public void Begin()
        {
            indent += 4;
        }

        public void End()
        {
            indent = Math.Max(0, indent - 4);
        }

        public IEnumerable<string> Lines
        {
            get
            {
                return lines.AsEnumerable();
            }
        }
    }

    public static partial class MacroUtils
    {
        public static IEnumerable<Tuple<T, bool>> WithLastMark<T>(this IEnumerable<T> items)
        {
            T prevItem = default(T);
            bool havePrevItem = false;
            foreach (T item in items)
            {
                if (havePrevItem) yield return new Tuple<T, bool>(prevItem, false);
                prevItem = item;
                havePrevItem = true;
            }
            if (havePrevItem) yield return new Tuple<T, bool>(prevItem, true);
        }

        public static IEnumerable<string> Emit(int indent, VariantType vt)
        {
            CodeBuilder cb = new CodeBuilder(indent);
            Emit(cb, vt);
            return cb.Lines;
        }

        public static void Emit(CodeBuilder cb, VariantType vt)
        {
            if (!string.IsNullOrEmpty(vt.Access))
            {
                cb.Append(vt.Access);
                cb.Append(" ");
            }
            cb.Append("abstract class ");
            cb.Append(vt.RootName);
            cb.Newline();
            cb.AppendLine("{");
            cb.AppendLine("}");
            
            foreach (Variation v in vt.Variations)
            {
                cb.Newline();
                Emit(cb, vt, v);
            }
        }

        public static void Emit(CodeBuilder cb, VariantType vt, Variation v)
        {
            if (!string.IsNullOrEmpty(vt.Access))
            {
                cb.Append(vt.Access);
                cb.Append(" ");
            }
            cb.Append("class ");
            cb.Append(v.Name);
            cb.Append(" : ");
            cb.Append(vt.RootName);
            cb.Newline();
            cb.AppendLine("{");
            cb.Begin();
            foreach (Field f in v.Fields)
            {
                cb.Append("private ");
                cb.Append(f.Type);
                cb.Append(" ");
                cb.Append(f.PrivateName);
                cb.AppendLine(";");
            }
            if (v.Fields.Count > 0) cb.Newline();
            cb.Append("public ");
            cb.AppendLine(v.Name);
            cb.AppendLine("(");
            cb.Begin();
            foreach (Tuple<Field, bool> f in v.Fields.WithLastMark())
            {
                cb.Append(f.Item1.Type);
                cb.Append(" ");
                cb.Append(f.Item1.PrivateName);
                if (!f.Item2) cb.Append(",");
                cb.Newline();
            }
            cb.End();
            cb.AppendLine(")");
            cb.AppendLine("{");
            cb.Begin();
            foreach (Field f in v.Fields)
            {
                cb.Append("this.");
                cb.Append(f.PrivateName);
                cb.Append(" = ");
                cb.Append(f.PrivateName);
                cb.AppendLine(";");
            }
            cb.End();
            cb.AppendLine("}");

            if (v.Fields.Count > 0) cb.Newline();

            foreach (Field f in v.Fields)
            {
                cb.Append("public ");
                cb.Append(f.Type);
                cb.Append(" ");
                cb.Append(f.PropertyName);
                cb.Append(" { get { return ");
                cb.Append(f.PrivateName);
                cb.AppendLine("; } }");
            }
            cb.End();
            cb.AppendLine("}");
        }
    }
}
