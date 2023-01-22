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
using System.Runtime.Serialization;
using System.Linq;
using BigMath;
using ControlledWindowLib;

namespace ExprObjModel
{
    public interface IAppendable
    {
        void Append(char ch);
        void Append(string s);
    }

    public class StringAppendable : IAppendable
    {
        public StringAppendable()
        {
            sb = new System.Text.StringBuilder();
        }

        private System.Text.StringBuilder sb;

        public void Append(char ch)
        {
            sb.Append(ch);
        }

        public void Append(string s)
        {
            sb.Append(s);
        }

        public override string ToString()
        {
            return sb.ToString();
        }
    }

    public class ConsoleAppendable : IAppendable
    {
        private ConsoleAppendable() { }
        private static ConsoleAppendable instance;
        static ConsoleAppendable() { instance = new ConsoleAppendable(); }
        public static ConsoleAppendable Instance { get { return instance; } }

        public void Append(char ch)
        {
            Console.Write(ch);
        }

        public void Append(string s)
        {
            Console.Write(s);
        }
    }

    public class TextWriterAppendable : IAppendable
    {
        public TextWriterAppendable(System.IO.TextWriter dest)
        {
            this.dest = dest;
        }

        private System.IO.TextWriter dest;

        public void Append(char ch)
        {
            dest.Write(ch);
        }

        public void Append(string s)
        {
            dest.Write(s);
        }
    }

    public class EntryCountKeeper
    {
        public EntryCountKeeper()
        {
            idgen = new ObjectIDGenerator();
            dict = new Dictionary<long, int>();
        }

        private ObjectIDGenerator idgen;
        private Dictionary<long, int> dict;

        private long GetId(object obj)
        {
            bool firstTime;
            return idgen.GetId(obj, out firstTime);
        }

        public int Enter(object obj)
        {
            long key = GetId(obj);
            if (dict.ContainsKey(key))
            {
                return ++dict[key];
            }
            else
            {
                dict.Add(key, 1);
                return 1;
            }
        }

        public int Leave(object obj)
        {
            long key = GetId(obj);
            System.Diagnostics.Debug.Assert(dict.ContainsKey(key));
            int i = dict[key];
            --i;
            if (i == 0)
            {
                dict.Remove(key);
            }
            else
            {
                dict[key] = i;
            }
            return i;
        }
    }

    public class SchemeDataWriter
    {
        private IAppendable sink;
        private EntryCountKeeper eck;

        public SchemeDataWriter(IAppendable sink)
        {
            this.sink = sink;
            eck = new EntryCountKeeper();
        }

        private static char Digit(int val)
        {
            if (val <= 9) return (char)('0' + val);
            else return (char)(('A' - 10) + val);
        }

        private void WriteString(string str)
        {
            sink.Append('"');
            foreach (char ch in str)
            {
                if (ch == '"') sink.Append("\\\"");
                else if (ch == '\\') sink.Append("\\\\");
                else if (ch == '\a') sink.Append("\\a");
                else if (ch == '\b') sink.Append("\\b");
                else if (ch == '\t') sink.Append("\\t");
                else if (ch == '\n') sink.Append("\\n");
                else if (ch == '\v') sink.Append("\\v");
                else if (ch == '\f') sink.Append("\\f");
                else if (ch == '\r') sink.Append("\\r");
                else if ((ch < ' ' || ch > '~') && ((int)ch) < 256)
                {
                    int i = (int)ch;
                    sink.Append("\\x");
                    sink.Append(Digit((i >> 4) & 15));
                    sink.Append(Digit(i & 15));
                }
                else if (ch >= 256)
                {
                    int i = (int)ch;
                    sink.Append("\\u");
                    sink.Append(Digit((i >> 12) & 15));
                    sink.Append(Digit((i >> 8) & 15));
                    sink.Append(Digit((i >> 4) & 15));
                    sink.Append(Digit(i & 15));
                }
                else sink.Append(ch);
            }
            sink.Append('"');
        }

        private void WriteDouble(double d)
        {
            string str = d.ToString("R");
            sink.Append(str);
            if (!str.Contains(".")) sink.Append(".");
        }

        private void WriteSymbol(Symbol sym)
        {
            if (sym.IsInterned)
            {
                string name = sym.Name;
                SchemeScanner ss = new SchemeScanner();
                ScanResult sr;
                int newPos;
                ss.Scan(name, 0, out sr, out newPos);
                if (sr.type == LexemeType.Symbol && newPos == name.Length)
                {
                    sink.Append(name);
                }
                else
                {
                    sink.Append('|');
                    foreach (char ch in name)
                    {
                        if (ch == '|') sink.Append("\\|");
                        else if (ch == '\\') sink.Append("\\\\");
                        else if (ch == '\a') sink.Append("\\a");
                        else if (ch == '\b') sink.Append("\\b");
                        else if (ch == '\t') sink.Append("\\t");
                        else if (ch == '\n') sink.Append("\\n");
                        else if (ch == '\v') sink.Append("\\v");
                        else if (ch == '\f') sink.Append("\\f");
                        else if (ch == '\r') sink.Append("\\r");
                        else if ((ch < ' ' || ch > '~') && ((int)ch < 256))
                        {
                            int i = (int)ch;
                            sink.Append("\\x");
                            sink.Append(Digit((i >> 4) & 15));
                            sink.Append(Digit(i & 15));
                        }
                        else if (ch >= 256)
                        {
                            int i = (int)ch;
                            sink.Append("\\u");
                            sink.Append(Digit((i >> 12) & 15));
                            sink.Append(Digit((i >> 8) & 15));
                            sink.Append(Digit((i >> 4) & 15));
                            sink.Append(Digit(i & 15));
                        }
                        else sink.Append(ch);
                    }
                    sink.Append('|');
                }
            }
            else
            {
                sink.Append("#<" + sym.Name + ">");
            }
        }

        private void WriteGuid(Guid g)
        {
            sink.Append("#g{");
            sink.Append(g.ToString("d"));
            sink.Append("}");
        }

        private void WriteIPAddress(System.Net.IPAddress ipAddr)
        {
            if (ipAddr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                sink.Append("#ipv4[");
                sink.Append(ipAddr.ToString());
                sink.Append("]");
            }
            else
            {
                System.Diagnostics.Debug.Assert(ipAddr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6);
                sink.Append("#ipv6[");
                sink.Append(ipAddr.ToString());
                sink.Append("]");
            }
        }

        private void WriteIPEndPoint(System.Net.IPEndPoint ipEndPoint)
        {
            WriteIPAddress(ipEndPoint.Address);
            sink.Append(":");
            sink.Append(ipEndPoint.Port.ToString());
        }

        private void WriteCharacter(char ch)
        {
            if (ch >= '!' && ch <= '~')
            {
                sink.Append("#\\");
                sink.Append(ch);
            }
            else if (ch == (char)0) sink.Append("#\\nul");
            else if (ch == '\a') sink.Append("#\\bel");
            else if (ch == '\b') sink.Append("#\\backspace");
            else if (ch == '\t') sink.Append("#\\tab");
            else if (ch == '\n') sink.Append("#\\newline");
            else if (ch == '\v') sink.Append("#\\vt");
            else if (ch == '\f') sink.Append("#\\page");
            else if (ch == '\r') sink.Append("#\\return");
            else if (ch == ' ') sink.Append("#\\space");
            else
            {
                int i = (int)ch;
                sink.Append("#\\x");
                sink.Append(Digit((i >> 12) & 15));
                sink.Append(Digit((i >> 8) & 15));
                sink.Append(Digit((i >> 4) & 15));
                sink.Append(Digit(i & 15));
            }
        }

        private void WriteBool(bool b)
        {
            sink.Append(b ? "#t" : "#f");
        }

        private void WriteVector(Deque<object> vec)
        {
            int i = eck.Enter(vec);
            if (i > 1)
            {
                sink.Append("#<...>");
            }
            else
            {
                sink.Append("#(");
                bool needSpace = false;
                foreach (object item in vec)
                {
                    if (needSpace) sink.Append(' ');
                    WriteItem(item);
                    needSpace = true;
                }
                sink.Append(")");
            }
            eck.Leave(vec);
        }

        private void WriteList(ConsCell c, bool isFirst)
        {
            int i = eck.Enter(c);
            if (i > 1)
            {
                sink.Append("#<...>");
            }
            else
            {
                if (isFirst) sink.Append("(");
                WriteItem(c.car);
                if (ConsCell.IsEmptyList(c.cdr))
                {
                    // do nothing
                }
                else if (c.cdr is ConsCell)
                {
                    sink.Append(' ');
                    WriteList((ConsCell)c.cdr, false);
                }
                else
                {
                    sink.Append(" . ");
                    WriteItem(c.cdr);
                }
                if (isFirst) sink.Append(")");
            }
            eck.Leave(c);
        }

        private void WriteHashSet(SchemeHashSet hs)
        {
            int i = eck.Enter(hs);
            if (i > 1)
            {
                sink.Append("#<...>");
            }
            else
            {
                sink.Append("#s(");
                bool needSpace = false;
                foreach (object obj in hs)
                {
                    if (needSpace) sink.Append(' ');
                    WriteItem(obj);
                    needSpace = true;
                }
                sink.Append(")");
            }
            eck.Leave(hs);
        }

        private void WriteHashMap(SchemeHashMap hm)
        {
            int i = eck.Enter(hm);
            if (i > 1)
            {
                sink.Append("#<...>");
            }
            else
            {
                sink.Append("#m(");
                bool needSpace = false;
                foreach (KeyValuePair<object, object> kvp in hm)
                {
                    if (needSpace) sink.Append(' ');
                    ConsCell c = new ConsCell(kvp.Key, kvp.Value);
                    WriteItem(c);
                    needSpace = true;
                }
                sink.Append(")");
            }
            eck.Leave(hm);
        }

        private void WriteSignature(ExprObjModel.ObjectSystem.Signature s)
        {
            sink.Append("#sig(");
            WriteSymbol(s.Type);
            if (s.Parameters.Count > 0)
            {
                sink.Append(" .");
                foreach (Symbol p in s.Parameters)
                {
                    sink.Append(" ");
                    WriteSymbol(p);
                }
            }
            sink.Append(")");
        }

        private void WriteMessage(ExprObjModel.ObjectSystem.Message<object> m)
        {
            int i = eck.Enter(m);
            if (i > 1)
            {
                sink.Append("#<...>");
            }
            else
            {
                sink.Append("#msg(");
                WriteSymbol(m.Type);
                if (m.Arguments.Count > 0)
                {
                    sink.Append(" .");
                    foreach (Tuple<Symbol, object> a in m.Arguments)
                    {
                        sink.Append(" ");
                        WriteSymbol(a.Item1);
                        sink.Append(" ");
                        WriteItem(a.Item2);
                    }
                }
                sink.Append(")");
            }
            eck.Leave(m);
        }

        private void WriteVec3Part(BigRational r)
        {
            if (r.Denominator == BigInteger.One)
            {
                sink.Append(BigInteger.ToString(r.Numerator, 10u));
            }
            else
            {
                sink.Append(BigInteger.ToString(r.Numerator, 10u));
                sink.Append("/");
                sink.Append(BigInteger.ToString(r.Denominator, 10u));
            }
        }

        private void WriteVector2(Vector2 v)
        {
            sink.Append("#vec2(");
            WriteVec3Part(v.X);
            sink.Append(" ");
            WriteVec3Part(v.Y);
            sink.Append(")");
        }

        private void WriteVertex2(Vertex2 v)
        {
            sink.Append("#vtx2(");
            WriteVec3Part(v.X);
            sink.Append(" ");
            WriteVec3Part(v.Y);
            sink.Append(")");
        }
        
        private void WriteVector3(Vector3 v)
        {
            sink.Append("#vec3(");
            WriteVec3Part(v.X);
            sink.Append(" ");
            WriteVec3Part(v.Y);
            sink.Append(" ");
            WriteVec3Part(v.Z);
            sink.Append(")");
        }

        private void WriteVertex3(Vertex3 v)
        {
            sink.Append("#vtx3(");
            WriteVec3Part(v.X);
            sink.Append(" ");
            WriteVec3Part(v.Y);
            sink.Append(" ");
            WriteVec3Part(v.Z);
            sink.Append(")");
        }

        private void WriteQuaternion(Quaternion q)
        {
            sink.Append("#quat(");
            WriteVec3Part(q.W);
            sink.Append(" ");
            WriteVec3Part(q.X);
            sink.Append(" ");
            WriteVec3Part(q.Y);
            sink.Append(" ");
            WriteVec3Part(q.Z);
            sink.Append(")");
        }

        public void WriteTypeName(Type t)
        {
            if (t.IsGenericType)
            {
                sink.Append(t.GetGenericTypeDefinition().FullName);
                sink.Append("{");
                Type[] t0 = t.GetGenericArguments();
                int iEnd = t0.Length;
                int i = 0;
                while (i < iEnd)
                {
                    WriteTypeName(t0[i]);
                    ++i;
                    if (i < iEnd) sink.Append(", ");
                }
                sink.Append("}");
            }
            else
            {
                sink.Append(t.FullName);
            }
        }

        public void WriteItem(object obj)
        {
            if (object.ReferenceEquals(obj, null))
            {
                sink.Append("#< NULL >");
            }
            else if (obj is SchemeString)
            {
                WriteString(((SchemeString)obj).TheString);
            }
            else if (obj is Symbol)
            {
                WriteSymbol((Symbol)obj);
            }
            else if (obj is bool)
            {
                WriteBool((bool)obj);
            }
            else if (obj is char)
            {
                WriteCharacter((char)obj);
            }
            else if (obj is BigInteger)
            {
                BigInteger b = (BigInteger)obj;
                sink.Append(BigInteger.ToString(b, 10u));
            }
            else if (obj is BigRational)
            {
                BigRational r = (BigRational)obj;
                sink.Append(BigInteger.ToString(r.Numerator, 10u));
                sink.Append('/');
                sink.Append(BigInteger.ToString(r.Denominator, 10u));
            }
            else if (obj is double)
            {
                WriteDouble((double)obj);
            }
            else if (obj is Vector2)
            {
                WriteVector2((Vector2)obj);
            }
            else if (obj is Vertex2)
            {
                WriteVertex2((Vertex2)obj);
            }
            else if (obj is Vector3)
            {
                WriteVector3((Vector3)obj);
            }
            else if (obj is Vertex3)
            {
                WriteVertex3((Vertex3)obj);
            }
            else if (obj is Quaternion)
            {
                WriteQuaternion((Quaternion)obj);
            }
            else if (obj is Guid)
            {
                WriteGuid((Guid)obj);
            }
            else if (obj is DisposableID)
            {
                sink.Append("#<disposable-id ");
                sink.Append(((DisposableID)obj).id.ToString());
                sink.Append(">");
            }
            else if (obj is AsyncID)
            {
                sink.Append("#<async-id ");
                sink.Append(((AsyncID)obj).id.ToString());
                sink.Append(">");
            }
            else if (obj is ExprObjModel.ObjectSystem.OldObjectID)
            {
                sink.Append("#<old-object-id ");
                sink.Append(((ExprObjModel.ObjectSystem.OldObjectID)obj).id.ToString());
                sink.Append(">");
            }
            else if (obj is ControlledWindowLib.Scheduling.SignalID)
            {
                sink.Append("#<signal-id ");
                sink.Append(((ControlledWindowLib.Scheduling.SignalID)obj).id.ToString());
                sink.Append(">");
            }
            else if (obj is ControlledWindowLib.Scheduling.ObjectID)
            {
                sink.Append("#<object-id ");
                sink.Append(((ControlledWindowLib.Scheduling.ObjectID)obj).id.ToString());
                sink.Append(">");
            }
            else if (obj is System.Net.IPAddress)
            {
                WriteIPAddress((System.Net.IPAddress)obj);
            }
            else if (obj is System.Net.IPEndPoint)
            {
                WriteIPEndPoint((System.Net.IPEndPoint)obj);
            }
            else if (obj is Deque<object>)
            {
                WriteVector((Deque<object>)obj);
            }
            else if (obj is ConsCell)
            {
                WriteList((ConsCell)obj, true);
            }
            else if (obj is SpecialValue)
            {
                SpecialValue s = (SpecialValue)obj;
                if (s == SpecialValue.EMPTY_LIST)
                {
                    sink.Append("()");
                }
                else if (s == SpecialValue.UNSPECIFIED)
                {
                    sink.Append("#<unspecified>");
                }
                else if (s == SpecialValue.EOF)
                {
                    sink.Append("#<EOF>");
                }
            }
            else if (obj is Type)
            {
                Type t = (Type)obj;
                sink.Append("#<type of ");
                WriteTypeName(t);
                sink.Append(">");
            }
            else if (obj is System.Reflection.Assembly)
            {
                System.Reflection.Assembly a = (System.Reflection.Assembly)obj;
                sink.Append("#<assembly ");
                sink.Append(a.FullName);
                sink.Append(">");
            }
            else if (obj is System.Reflection.MemberInfo)
            {
                System.Reflection.MemberInfo m = (System.Reflection.MemberInfo)obj;
                sink.Append("#<");
                sink.Append(m.MemberType.ToString());
                sink.Append(" ");
                sink.Append(m.Name);
                sink.Append(" ");
                sink.Append(m.MetadataToken.ToString());
                sink.Append(">");
            }
            else if (obj.GetType().IsEnum)
            {
                sink.Append("#<enum ");
                sink.Append(obj.GetType().ToString());
                sink.Append(".");
                sink.Append(obj.ToString());
                sink.Append(">");
            }
            else if (obj is SchemeHashSet)
            {
                WriteHashSet((SchemeHashSet)obj);
            }
            else if (obj is SchemeHashMap)
            {
                WriteHashMap((SchemeHashMap)obj);
            }
            else if (obj is ExprObjModel.ObjectSystem.Signature)
            {
                WriteSignature((ExprObjModel.ObjectSystem.Signature)obj);
            }
            else if (obj is ExprObjModel.ObjectSystem.Message<object>)
            {
                WriteMessage((ExprObjModel.ObjectSystem.Message<object>)obj);
            }
            else if (obj is byte)
            {
                sink.Append("#<byte = " + (byte)obj + ">");
            }
            else if (obj is sbyte)
            {
                sink.Append("#<sbyte = " + (sbyte)obj + ">");
            }
            else if (obj is ushort)
            {
                sink.Append("#<ushort = " + (ushort)obj + ">");
            }
            else if (obj is short)
            {
                sink.Append("#<short = " + (short)obj + ">");
            }
            else if (obj is uint)
            {
                sink.Append("#<uint = " + (uint)obj + ">");
            }
            else if (obj is int)
            {
                sink.Append("#<int = " + (int)obj + ">");
            }
            else if (obj is ulong)
            {
                sink.Append("#<ulong = " + (ulong)obj + ">");
            }
            else if (obj is long)
            {
                sink.Append("#<long = " + (long)obj + ">");
            }
            else if (obj is float)
            {
                sink.Append("#<float = " + (float)obj + ">");
            }
            else if (obj is string)
            {
                sink.Append("#<string = ");
                WriteString((string)obj);
                sink.Append(">");
            }
            else if (Pascalesque.Utils.IsTupleType(obj.GetType()))
            {
                sink.Append("#< tuple{");
                bool needComma = false;
                foreach (Type t in obj.GetType().GetGenericArguments())
                {
                    if (needComma) sink.Append(", ");
                    WriteTypeName(t);
                    needComma = true;
                }
                sink.Append("} = { ");
                needComma = false;
                foreach (object o2 in Enumerable.Range(0, Pascalesque.Utils.TupleElements(obj.GetType())).Select(x => Pascalesque.Utils.GetTupleProperty(obj.GetType(), x).GetGetMethod().Invoke(obj, null)))
                {
                    if (needComma) sink.Append(", ");
                    WriteItem(o2);
                    needComma = true;
                }
                sink.Append(" }>");
            }
            else if (obj is ExprObjModel.Procedures.UserType)
            {
                sink.Append("#<User type ");
                sink.Append(((ExprObjModel.Procedures.UserType)obj).ID.ToString());
                sink.Append(">");
            }
            else if (obj is System.Reflection.Emit.OpCode)
            {
                sink.Append("#<OpCode ");
                sink.Append(((System.Reflection.Emit.OpCode)obj).Name);
                sink.Append(">");
            }
            else
            {
                sink.Append("#<object of type ");
                sink.Append(obj.GetType().ToString());
                sink.Append(">");
            }
        }

        public static void WriteItem(object obj, IAppendable sink)
        {
            SchemeDataWriter sdw = new SchemeDataWriter(sink);
            sdw.WriteItem(obj);
        }

        [SchemeFunction("object->string")]
        public static string ItemToString(object obj)
        {
            StringAppendable sb = new StringAppendable();
            WriteItem(obj, sb);
            return sb.ToString();
        }
    }
}
