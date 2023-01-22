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
using System.IO;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using BigMath;
using ControlledWindowLib;

namespace ExprObjModel.Coding
{
    public interface IReader
    {
        byte ReadByte();
        void ReadBytes(byte[] buf, int off, int len);
        void ReadReference(ICodec codec, Action<object> dest);
    }

    public interface IWriter
    {
        void WriteByte(byte b);
        void WriteBytes(byte[] buf, int off, int len);
        void WriteReference(ICodec codec, object src);
    }

    public interface ITester
    {
        void TestReference(ICodec codec, object src, Action<bool> dest);
    }

    public interface ICodec
    {
        bool IsByRef { get; }
        void Read(IReader r, Action<object> dest);
        void Test(ITester t, object src, Action<bool> dest);
        void Write(IWriter w, object obj);
    }

    public static partial class CodingUtils
    {
        public static long GetId(this ObjectIDGenerator idgen, object obj)
        {
            bool firstTime;
            long id = idgen.GetId(obj, out firstTime);
            return id;
        }

        public static void WriteSByte(this IWriter w, sbyte sb)
        {
            w.WriteByte(unchecked((byte)sb));
        }

        public static sbyte ReadSByte(this IReader r)
        {
            return unchecked((sbyte)(r.ReadByte()));
        }

        public static void WriteChar(this IWriter w, char c)
        {
            byte[] b = BitConverter.GetBytes(c);
            w.WriteBytes(b, 0, 2);
        }

        public static char ReadChar(this IReader r)
        {
            byte[] b = new byte[2];
            r.ReadBytes(b, 0, 2);
            return BitConverter.ToChar(b, 0);
        }

        public static void WriteBoolean(this IWriter w, bool b)
        {
            w.WriteByte(b ? (byte)1 : (byte)0);
        }

        public static bool ReadBoolean(this IReader r)
        {
            byte b = r.ReadByte();
            return (b != 0);
        }

        public static void WriteByteArray(this IWriter w, byte[] b, int off, int len)
        {
            w.WriteInt32(len);
            w.WriteBytes(b, off, len);
        }

        public static byte[] ReadByteArray(this IReader r)
        {
            int len = r.ReadInt32();
            byte[] b0 = new byte[len];
            r.ReadBytes(b0, 0, len);
            return b0;
        }

        public static void WriteBigInteger(this IWriter w, BigInteger b, bool signed)
        {
            int bsize = b.MinByteArraySize(signed);
            byte[] b0 = new byte[bsize];
            b.WriteBytesToArray(b0, 0, bsize, signed, OverflowBehavior.ThrowException, DigitOrder.LBLA);
            w.WriteByteArray(b0, 0, bsize);
        }

        public static BigInteger ReadBigInteger(this IReader r, bool signed)
        {
            byte[] b0 = r.ReadByteArray();
            BigInteger b = BigInteger.FromByteArray(b0, 0, b0.Length, signed, DigitOrder.LBLA);
            return b;
        }

        public static void WriteVector2(this IWriter w, Vector2 v)
        {
            BigInteger denom = BigInteger.Lcm(v.X.Denominator, v.Y.Denominator);
            w.WriteBigInteger((v.X * denom).Numerator, true);
            w.WriteBigInteger((v.Y * denom).Numerator, true);
            w.WriteBigInteger(denom, false);
        }

        public static Vector2 ReadVector2(this IReader r)
        {
            BigInteger x = r.ReadBigInteger(true);
            BigInteger y = r.ReadBigInteger(true);
            BigInteger denom = r.ReadBigInteger(false);

            return new Vector2(new BigRational(x, denom), new BigRational(y, denom));
        }

        public static void WriteVertex2(this IWriter w, Vertex2 v)
        {
            BigInteger denom = BigInteger.Lcm(v.X.Denominator, v.Y.Denominator);
            w.WriteBigInteger((v.X * denom).Numerator, true);
            w.WriteBigInteger((v.Y * denom).Numerator, true);
            w.WriteBigInteger(denom, false);
        }

        public static Vertex2 ReadVertex2(this IReader r)
        {
            BigInteger x = r.ReadBigInteger(true);
            BigInteger y = r.ReadBigInteger(true);
            BigInteger denom = r.ReadBigInteger(false);

            return new Vertex2(new BigRational(x, denom), new BigRational(y, denom));
        }

        public static void WriteVector3(this IWriter w, Vector3 v)
        {
            BigInteger denom = BigInteger.Lcm(v.X.Denominator, BigInteger.Lcm(v.Y.Denominator, v.Z.Denominator));
            w.WriteBigInteger((v.X * denom).Numerator, true);
            w.WriteBigInteger((v.Y * denom).Numerator, true);
            w.WriteBigInteger((v.Z * denom).Numerator, true);
            w.WriteBigInteger(denom, false);
        }

        public static Vector3 ReadVector3(this IReader r)
        {
            BigInteger x = r.ReadBigInteger(true);
            BigInteger y = r.ReadBigInteger(true);
            BigInteger z = r.ReadBigInteger(true);
            BigInteger denom = r.ReadBigInteger(false);

            return new Vector3(new BigRational(x, denom), new BigRational(y, denom), new BigRational(z, denom));
        }

        public static void WriteVertex3(this IWriter w, Vertex3 v)
        {
            BigInteger denom = BigInteger.Lcm(v.X.Denominator, BigInteger.Lcm(v.Y.Denominator, v.Z.Denominator));
            w.WriteBigInteger((v.X * denom).Numerator, true);
            w.WriteBigInteger((v.Y * denom).Numerator, true);
            w.WriteBigInteger((v.Z * denom).Numerator, true);
            w.WriteBigInteger(denom, false);
        }

        public static Vertex3 ReadVertex3(this IReader r)
        {
            BigInteger x = r.ReadBigInteger(true);
            BigInteger y = r.ReadBigInteger(true);
            BigInteger z = r.ReadBigInteger(true);
            BigInteger denom = r.ReadBigInteger(false);

            return new Vertex3(new BigRational(x, denom), new BigRational(y, denom), new BigRational(z, denom));
        }

        public static void WriteQuaternion(this IWriter w, Quaternion q)
        {
            BigInteger denom = BigInteger.Lcm(BigInteger.Lcm(q.W.Denominator, q.X.Denominator), BigInteger.Lcm(q.Y.Denominator, q.Z.Denominator));
            w.WriteBigInteger((q.W * denom).Numerator, true);
            w.WriteBigInteger((q.X * denom).Numerator, true);
            w.WriteBigInteger((q.Y * denom).Numerator, true);
            w.WriteBigInteger((q.Z * denom).Numerator, true);
            w.WriteBigInteger(denom, false);
        }

        public static Quaternion ReadQuaternion(this IReader r)
        {
            BigInteger w = r.ReadBigInteger(true);
            BigInteger x = r.ReadBigInteger(true);
            BigInteger y = r.ReadBigInteger(true);
            BigInteger z = r.ReadBigInteger(true);
            BigInteger denom = r.ReadBigInteger(false);

            return new Quaternion(new BigRational(w, denom), new BigRational(x, denom), new BigRational(y, denom), new BigRational(z, denom));
        }

        public static void WriteLine3(this IWriter w, Line3 l)
        {
            w.WriteVertex3(l.Origin);
            w.WriteVector3(l.Direction);
        }

        public static Line3 ReadLine3(this IReader r)
        {
            Vertex3 origin = r.ReadVertex3();
            Vector3 direction = r.ReadVector3();
            return new Line3(origin, direction);
        }

        public static void WritePlane3(this IWriter w, Plane3 p)
        {
            w.WriteVertex3(p.Origin);
            w.WriteVector3(p.Normal);
        }

        public static Plane3 ReadPlane3(this IReader r)
        {
            Vertex3 origin = r.ReadVertex3();
            Vector3 normal = r.ReadVector3();
            return new Plane3(origin, normal);
        }

        public static void WriteGuid(this IWriter w, Guid g)
        {
            byte[] b0 = g.ToByteArray();
            w.WriteBytes(b0, 0, 16);
        }

        public static Guid ReadGuid(this IReader r)
        {
            byte[] b0 = new byte[16];
            r.ReadBytes(b0, 0, 16);
            return new Guid(b0);
        }

        private static void WriteAddress(this Stream str, long addr)
        {
            if (addr < -1 || addr >= (long)(uint.MaxValue)) throw new ArgumentOutOfRangeException("addr");
            byte[] b = BitConverter.GetBytes(unchecked((uint)addr));
            str.Write(b, 0, 4);
        }

        private static long ReadAddress(this Stream str)
        {
            byte[] b = new byte[4];
            int count = str.Read(b, 0, 4);
            if (count != 4) throw new EndOfStreamException("Failed to read address");
            uint u = BitConverter.ToUInt32(b, 0);
            long addr = (long)u;
            if (addr == (long)(uint.MaxValue)) addr = -1L;
            return addr;
        }

        private class SpecialIdGenerator
        {
            private ObjectIDGenerator idgen;

            private Dictionary<Symbol, Symbol> properSymbols;

            public SpecialIdGenerator()
            {
                idgen = new ObjectIDGenerator();
                properSymbols = new Dictionary<Symbol, Symbol>();
            }

            public long GetId(object obj)
            {
                if (obj is Symbol)
                {
                    Symbol s = (Symbol)obj;
                    if (properSymbols.ContainsKey(s))
                    {
                        return idgen.GetId(properSymbols[s]);
                    }
                    else
                    {
                        properSymbols.Add(s, s);
                        return idgen.GetId(s);
                    }
                }
                else return idgen.GetId(obj);
            }
        }

        private class InternalReader : IReader
        {
            private Stream r;
            private SpecialIdGenerator idgen;
            private Dictionary<Tuple<long, long>, ObjectTrackingInfo> unreadObjects;
            private Dictionary<Tuple<long, long>, object> readObjects;
            private Queue<Tuple<long, long>> q;

            private class ObjectTrackingInfo
            {
                public ICodec codec;
                public FList<Action<object>> fixups;
            }

            public InternalReader(Stream r)
            {
                this.r = r;
                this.idgen = new SpecialIdGenerator();
                this.unreadObjects = new Dictionary<Tuple<long, long>, ObjectTrackingInfo>();
                this.readObjects = new Dictionary<Tuple<long, long>, object>();
                this.q = new Queue<Tuple<long, long>>();
            }

            public Tuple<long, long> GetKey(long address, ICodec codec)
            {
                return new Tuple<long, long>(address, idgen.GetId(codec));
            }

            public byte ReadByte()
            {
                int i = r.ReadByte();
                if (i == -1) throw new EndOfStreamException();
                return unchecked((byte)i);
            }

            public void ReadBytes(byte[] buf, int off, int len)
            {
                int count = r.Read(buf, off, len);
                if (count != len) throw new EndOfStreamException();
            }

            public void ReadReference(ICodec codec, Action<object> dest)
            {
                long addr = r.ReadAddress();
                if (addr == -1L)
                {
                    dest(null);
                }
                else
                {
                    Tuple<long, long> key = GetKey(addr, codec);
                    if (readObjects.ContainsKey(key))
                    {
                        dest(readObjects[key]);
                    }
                    else if (unreadObjects.ContainsKey(key))
                    {
                        ObjectTrackingInfo oti = unreadObjects[key];
                        oti.fixups = new FList<Action<object>>(dest, oti.fixups);
                    }
                    else
                    {
                        ObjectTrackingInfo oti = new ObjectTrackingInfo();
                        oti.codec = codec;
                        oti.fixups = new FList<Action<object>>(dest);
                        unreadObjects.Add(key, oti);
                        q.Enqueue(key);
                    }
                }
            }

            public void ReadDirect(ICodec codec, Action<object> dest)
            {
                long addr = r.Position;
                Tuple<long, long> key = GetKey(addr, codec);
                if (readObjects.ContainsKey(key))
                {
                    throw new InvalidOperationException("Cannot read the same address with the same codec twice");
                }

                Action<object> store = delegate(object finalObj)
                {
                    dest(finalObj);
                    if (unreadObjects.ContainsKey(key))
                    {
                        ObjectTrackingInfo oti = unreadObjects[key];
                        FList<Action<object>> fixups = oti.fixups;
                        while (fixups != null)
                        {
                            fixups.Head(finalObj);
                            fixups = fixups.Tail;
                        }
                        unreadObjects.Remove(key);
                    }
                    readObjects.Add(key, finalObj);
                };

                codec.Read(this, store);
            }

            public void PlayOutQueue()
            {
                while (q.Count > 0)
                {
                    Tuple<long, long> key = q.Dequeue();
                    
                    if (unreadObjects.ContainsKey(key))
                    {
                        ObjectTrackingInfo oti = unreadObjects[key];
                        r.Position = key.Item1;
                        ReadDirect(oti.codec, delegate(object obj) { });
                    }
                }
            }
        }

        private class InternalWriter : IWriter
        {
            private Stream w;
            private SpecialIdGenerator idgen;
            private Dictionary<Tuple<long, long>, ObjectTrackingInfo> unwrittenObjects;
            private Dictionary<Tuple<long, long>, long> writtenObjects;
            private Queue<Tuple<long, long>> q;

            private class ObjectTrackingInfo
            {
                public object obj;
                public ICodec codec;
                public FList<long> fixups;
            }

            public InternalWriter(Stream w)
            {
                this.w = w;
                this.idgen = new SpecialIdGenerator();
                this.unwrittenObjects = new Dictionary<Tuple<long, long>, ObjectTrackingInfo>();
                this.writtenObjects = new Dictionary<Tuple<long, long>, long>();
                this.q = new Queue<Tuple<long, long>>();
            }

            private Tuple<long, long> GetKey(object obj, ICodec codec)
            {
                return new Tuple<long, long>(idgen.GetId(obj), idgen.GetId(codec));
            }

            public void WriteByte(byte b)
            {
                w.WriteByte(b);
            }

            public void WriteBytes(byte[] buf, int off, int len)
            {
                w.Write(buf, off, len);
            }

            public void WriteReference(ICodec codec, object src)
            {
                if (src == null)
                {
                    w.WriteAddress(-1L);
                }
                else
                {
                    Tuple<long, long> id = GetKey(src, codec);
                    if (writtenObjects.ContainsKey(id))
                    {
                        long addr = writtenObjects[id];
                        w.WriteAddress(addr);
                    }
                    else if (unwrittenObjects.ContainsKey(id))
                    {
                        ObjectTrackingInfo oti = unwrittenObjects[id];
                        long fixupAddr = w.Position;
                        oti.fixups = new FList<long>(fixupAddr, oti.fixups);
                        w.WriteAddress(-1L);
                    }
                    else
                    {
                        q.Enqueue(id);
                        ObjectTrackingInfo oti = new ObjectTrackingInfo();
                        oti.obj = src;
                        oti.codec = codec;
                        long fixupAddr = w.Position;
                        oti.fixups = new FList<long>(fixupAddr);
                        unwrittenObjects.Add(id, oti);
                        w.WriteAddress(-1L);
                    }
                }
            }

            public void WriteDirect(ICodec codec, object src)
            {
                if (src == null)
                {
                    throw new ArgumentNullException("Cannot write a null object directly");
                }

                Tuple<long, long> id = GetKey(src, codec);
                long addr = w.Position;
                if (writtenObjects.ContainsKey(id))
                {
                    throw new ArgumentException("Cannot write the same object with the same codec twice");
                }
                else if (unwrittenObjects.ContainsKey(id))
                {
                    ObjectTrackingInfo oti = unwrittenObjects[id];
                    FList<long> fixups = oti.fixups;
                    while (fixups != null)
                    {
                        w.Position = fixups.Head;
                        w.WriteAddress(addr);
                        fixups = fixups.Tail;
                    }
                    w.Position = addr;
                    unwrittenObjects.Remove(id);
                }
                
                writtenObjects.Add(id, addr);
                codec.Write(this, src);
            }

            public void PlayOutQueue()
            {
                while (q.Count > 0)
                {
                    Tuple<long, long> id = q.Dequeue();
                    if (unwrittenObjects.ContainsKey(id))
                    {
                        ObjectTrackingInfo oti = unwrittenObjects[id];
                        WriteDirect(oti.codec, oti.obj);
                    }
                }
            }
        }

        private class InternalTester : ITester
        {
            private SpecialIdGenerator idgen;
            private Dictionary<Tuple<long, long>, ObjectTrackingInfo> untestedObjects;
            private Dictionary<Tuple<long, long>, bool> testedObjects;
            private Queue<Tuple<long, long>> q;

            private class ObjectTrackingInfo
            {
                public ICodec codec;
                public object obj;
                public FList<Action<bool>> fixups;
            }

            public InternalTester()
            {
                this.idgen = new SpecialIdGenerator();
                this.untestedObjects = new Dictionary<Tuple<long, long>, ObjectTrackingInfo>();
                this.testedObjects = new Dictionary<Tuple<long, long>, bool>();
                this.q = new Queue<Tuple<long, long>>();
            }

            private Tuple<long, long> GetKey(object obj, ICodec codec)
            {
                return new Tuple<long, long>(idgen.GetId(obj), idgen.GetId(codec));
            }

            public void TestReference(ICodec codec, object src, Action<bool> dest)
            {
                if (src == null)
                {
                    dest(true);
                }
                else
                {
                    Tuple<long, long> id = GetKey(src, codec);
                    if (testedObjects.ContainsKey(id))
                    {
                        dest(testedObjects[id]);
                    }
                    else if (untestedObjects.ContainsKey(id))
                    {
                        ObjectTrackingInfo oti = untestedObjects[id];
                        oti.fixups = new FList<Action<bool>>(dest, oti.fixups);
                    }
                    else
                    {
                        q.Enqueue(id);
                        ObjectTrackingInfo oti = new ObjectTrackingInfo();
                        oti.codec = codec;
                        oti.obj = src;
                        oti.fixups = new FList<Action<bool>>(dest);
                        untestedObjects.Add(id, oti);
                    }
                }
            }

            public void TestDirect(ICodec codec, object src, Action<bool> dest)
            {
                if (src == null)
                {
                    dest(false);
                }

                Tuple<long, long> id = GetKey(src, codec);
                if (testedObjects.ContainsKey(id))
                {
                    dest(testedObjects[id]);
                }
                else
                {
                    Action<bool> store = delegate(bool finalResult)
                    {
                        dest(finalResult);
                        if (untestedObjects.ContainsKey(id))
                        {
                            ObjectTrackingInfo oti = untestedObjects[id];
                            FList<Action<bool>> fixups = oti.fixups;
                            while (fixups != null)
                            {
                                fixups.Head(finalResult);
                                fixups = fixups.Tail;
                            }
                            untestedObjects.Remove(id);
                        }
                        testedObjects.Add(id, finalResult);
                    };

                    codec.Test(this, src, store);
                }
            }

            public void PlayOutQueue()
            {
                while (q.Count > 0)
                {
                    Tuple<long, long> id = q.Dequeue();

                    if (untestedObjects.ContainsKey(id))
                    {
                        ObjectTrackingInfo oti = untestedObjects[id];
                        TestDirect(oti.codec, oti.obj, delegate(bool b) { });
                    }
                }
            }
        }

        public static object Read(byte[] buf, ICodec readFunc)
        {
            using (MemoryStream ms = new MemoryStream(buf))
            {
                InternalReader ir = new InternalReader(ms);
                object obj = null;
                bool written = false;
                ir.ReadDirect(readFunc, delegate(object obj2) { obj = obj2; written = true; });
                ir.PlayOutQueue();

                System.Diagnostics.Debug.Assert(written);
                return obj;
            }
        }

        public static byte[] Write(object obj, ICodec writeFunc)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                InternalWriter iw = new InternalWriter(ms);
                iw.WriteDirect(writeFunc, obj);
                iw.PlayOutQueue();

                return ms.ToArray();
            }
        }

        public static bool Test(object obj, ICodec codec)
        {
            bool result = false;
            InternalTester t = new InternalTester();
            t.TestDirect(codec, obj, delegate(bool b) { result = b; });
            t.PlayOutQueue();

            return result;
        }
    }

    public class Environment
    {
        private Dictionary<Symbol, Pascalesque.Box<ICodec>> env;
        private Environment parent;

        private static Lazy<Environment> empty = new Lazy<Environment>(() => new Environment(), true);

        private Environment()
        {
            env = new Dictionary<Symbol, Pascalesque.Box<ICodec>>();
            parent = null;
        }

        private Environment(Dictionary<Symbol, Pascalesque.Box<ICodec>> env, Environment parent)
        {
            this.env = env;
            this.parent = parent;
        }

        public static Environment Empty { get { return empty.Value; } }

        public Environment Shadow(IEnumerable<Symbol> symbols)
        {
            Dictionary<Symbol, Pascalesque.Box<ICodec>> e2 = new Dictionary<Symbol, Pascalesque.Box<ICodec>>();
            foreach (Symbol s in symbols)
            {
                e2.Add(s, new Pascalesque.Box<ICodec>());
            }
            
            return new Environment(e2, this);
        }

        public Pascalesque.Box<ICodec> this[Symbol s]
        {
            get
            {
                Environment e = this;
                while (e != null)
                {
                    if (e.env.ContainsKey(s)) return e.env[s];
                    e = e.parent;
                }

                throw new ArgumentException("Undefined variable");
            }
        }
    }

    [DescendantsWithPatterns]
    public abstract class CodecDesc
    {
        public abstract ICodec Compile(Environment env);
    }

    [Pattern("($var $value)")]
    public class LetClause
    {
        [Bind("$var")]
        public Symbol var;

        [Bind("$value")]
        public CodecDesc val;
    }

    [Pattern("(letrec $defns $body)")]
    public class LetRecCoderDesc : CodecDesc
    {
        [Bind("$defns")]
        public List<LetClause> defns;

        [Bind("$body")]
        public CodecDesc body;

        public override ICodec Compile(Environment env)
        {
            Environment e2 = env.Shadow(defns.Select(x => x.var));
            foreach (LetClause defn in defns)
            {
                ICodec c = defn.val.Compile(e2);
                e2[defn.var].Value = c;
            }
            return body.Compile(e2);
        }
    }

    public class RefCodec : ICodec
    {
        private Pascalesque.Box<ICodec> referent;

        public RefCodec(Pascalesque.Box<ICodec> referent)
        {
            this.referent = referent;
        }

        public bool IsByRef { get { return referent.Value.IsByRef; } }

        public void Read(IReader r, Action<object> dest)
        {
            referent.Value.Read(r, dest);
        }

        public void Test(ITester t, object obj, Action<bool> dest)
        {
            referent.Value.Test(t, obj, dest);
        }

        public void Write(IWriter w, object obj)
        {
            referent.Value.Write(w, obj);
        }
    }

    [Pattern("(ref $var)")]
    public class RefCodecDesc : CodecDesc
    {
        [Bind("$var")]
        public Symbol var;

        public override ICodec Compile(Environment env)
        {
            return new RefCodec(env[var]);
        }
    }

    public class MsgCodec : ICodec
    {
        private ExprObjModel.ObjectSystem.Message<ICodec> format;

        public MsgCodec(ExprObjModel.ObjectSystem.Message<ICodec> format)
        {
            this.format = format;
        }

        #region ICoder Members

        public bool IsByRef { get { return false; } }

        public void Read(IReader r, Action<object> dest)
        {
            int iEnd = format.Arguments.Count;
            object[] items = new object[iEnd];
            BigInteger b = (BigInteger.One << iEnd) - BigInteger.One;
            Action finalStore = delegate()
            {
                object result = new ExprObjModel.ObjectSystem.Message<object>(format.Type, format.Arguments.Numbered().Select(x => new Tuple<Symbol, object>(x.Item2.Item1, items[x.Item1])));
                dest(result);
            };
            foreach(Tuple<int, Tuple<Symbol, ICodec>> k in format.Arguments.Numbered())
            {
                Action<object> destItem = delegate(object obj2)
                {
                    items[k.Item1] = obj2;
                    b = b & ~(BigInteger.One << k.Item1);
                    if (b.IsZero) finalStore();
                };
                k.Item2.Item2.Read(r, destItem);
            }
        }

        public void Test(ITester t, object obj, Action<bool> dest)
        {
            if (!(obj is ExprObjModel.ObjectSystem.Message<object>))
            {
                dest(false);
            }
            else
            {
                ExprObjModel.ObjectSystem.Message<object> data = (ExprObjModel.ObjectSystem.Message<object>)obj;
                if (format.Signature != data.Signature)
                {
                    dest(false);
                }
                else
                {
                    BigInteger b = (BigInteger.One << format.Arguments.Count) - BigInteger.One;
                    bool x = true;
                    foreach (Tuple<int, Tuple<Symbol, ICodec>> ikvt in format.Arguments.Numbered())
                    {
                        int index = ikvt.Item1;
                        Action<bool> dest2 = delegate(bool r)
                        {
                            x &= r;
                            b &= ~(BigInteger.One << index);
                            if (b.IsZero)
                            {
                                dest(x);
                            }
                        };

                        t.TestReference(ikvt.Item2.Item2, data[ikvt.Item2.Item1], dest2);
                    }
                }
            }
        }

        public void Write(IWriter w, object obj)
        {
            ExprObjModel.ObjectSystem.Message<object> data = (ExprObjModel.ObjectSystem.Message<object>)obj;
            foreach (Tuple<Symbol, ICodec> kvp in format.Arguments)
            {
                kvp.Item2.Write(w, data[kvp.Item1]);
            }
        }

        #endregion
    }

    [Pattern("$msg")]
    public class MsgCodecDesc : CodecDesc
    {
        [Bind("$msg")]
        public ExprObjModel.ObjectSystem.Message<CodecDesc> msg;

        public override ICodec Compile(Environment env)
        {
            return new MsgCodec(msg.Map<ICodec>(x => x.Compile(env)));
        }
    }

    public class ByteCaseCodec : ICodec
    {
        private List<Tuple<byte, ICodec>> clauses;

        public ByteCaseCodec(IEnumerable<Tuple<byte, ICodec>> clauses)
        {
            this.clauses = clauses.ToList();
        }

        public bool IsByRef { get { return false; } }

        public void Read(IReader r, Action<object> dest)
        {
            byte b1 = r.ReadByte();
            foreach(Tuple<Byte, ICodec> clause in clauses)
            {
                if (clause.Item1 == b1)
                {
                    if (clause.Item2.IsByRef)
                    {
                        r.ReadReference(clause.Item2, dest);
                        return;
                    }
                    else
                    {
                        clause.Item2.Read(r, dest);
                        return;
                    }
                }
            }
            throw new FormatException("Byte case not found");
        }

        public void Test(ITester t, object obj, Action<bool> dest)
        {
            BigInteger b = (BigInteger.One << clauses.Count) - BigInteger.One;
            bool x = false;
            foreach (Tuple<int, Tuple<byte, ICodec>> iclause in clauses.Numbered())
            {
                int index = iclause.Item1;

                Action<bool> dest2 = delegate(bool r)
                {
                    x |= r;
                    b &= ~(BigInteger.One << index);
                    if (b.IsZero)
                    {
                        dest(x);
                    }

                };

                t.TestReference(iclause.Item2.Item2, obj, dest2);
            }
        }

        public void Write(IWriter w, object obj)
        {
            foreach(Tuple<byte, ICodec> clause in clauses)
            {
                if (CodingUtils.Test(obj, clause.Item2))
                {
                    w.WriteByte(clause.Item1);
                    if (clause.Item2.IsByRef)
                    {
                        w.WriteReference(clause.Item2, obj);
                    }
                    else
                    {
                        clause.Item2.Write(w, obj);
                    }
                    return;
                }
            }
            throw new ArgumentException("No case to encode this object");
        }
    }

    [Pattern("($byte $body)")]
    public class ByteCaseClauseDesc
    {
        [Bind("$byte")]
        public byte theByte;

        [Bind("$body")]
        public CodecDesc theBody;
    }

    [Pattern("(byte-case . $clauses)")]
    public class ByteCaseCodecDesc : CodecDesc
    {
        [Bind("$clauses")]
        public List<ByteCaseClauseDesc> clauses;

        public override ICodec Compile(Environment env)
        {
            return new ByteCaseCodec(clauses.Select(x => new Tuple<byte, ICodec>(x.theByte, x.theBody.Compile(env))));
        }
    }

    public class ListOfCodec : ICodec
    {
        private ICodec element;

        public ListOfCodec(ICodec element)
        {
            this.element = element;
        }

        #region ICoder Members

        public bool IsByRef { get { return true; } }

        public void Read(IReader r, Action<object> dest)
        {
            int iEnd = r.ReadInt32();
            object result = SpecialValue.EMPTY_LIST;
            
            for (int i = 0; i < iEnd; ++i)
            {
                ConsCell c = new ConsCell(false, result);
                result = c;
            }

            object iter = result;
            for (int i = 0; i < iEnd; ++i)
            {
                ConsCell x = (ConsCell)iter;
                if (element.IsByRef)
                {
                    r.ReadReference(element, delegate(object obj) { x.car = obj; });
                }
                else
                {
                    element.Read(r, delegate(object obj) { x.car = obj; });
                }
                iter = x.cdr;
            }

            dest(result);
        }

        public void Test(ITester t, object obj, Action<bool> dest)
        {
            if (!(ConsCell.IsList(obj)))
            {
                dest(false);
            }
            else
            {
                int iEnd = ConsCell.ListLength(obj);
                BigInteger b = (BigInteger.One << iEnd) - BigInteger.One;
                object j = obj;
                bool x = true;
                int i = 0;
                while (!ConsCell.IsEmptyList(j))
                {
                    int k = i;
                    Action<bool> dest2 = delegate(bool r)
                    {
                        x &= r;
                        b &= ~(BigInteger.One << k);
                        if (b.IsZero)
                        {
                            dest(x);
                        }
                    };

                    t.TestReference(element, ConsCell.Car(j), dest2);
                    j = ConsCell.Cdr(j);
                    ++i;
                }
            }
        }

        public void Write(IWriter w, object obj)
        {
            if (!(ConsCell.IsList(obj))) throw new ArgumentException("incorrect element type");
            
            int iEnd = ConsCell.ListLength(obj);

            w.WriteInt32(iEnd);

            for (int i = 0; i < iEnd; ++i)
            {
                ConsCell c = (ConsCell)obj;
                if (element.IsByRef)
                {
                    w.WriteReference(element, c.car);
                }
                else
                {
                    element.Write(w, c.car);
                }
                obj = c.cdr;
            }

            System.Diagnostics.Debug.Assert(ConsCell.IsEmptyList(obj));
        }

        #endregion
    }

    [Pattern("(list-of $body)")]
    public class ListOfCodecDesc : CodecDesc
    {
        [Bind("$body")]
        public CodecDesc element;

        public override ICodec Compile(Environment env)
        {
            return new ListOfCodec(element.Compile(env));
        }
    }

    public class VectorOfCodec : ICodec
    {
        private ICodec element;

        public VectorOfCodec(ICodec element)
        {
            this.element = element;
        }

        #region ICoder Members

        public bool IsByRef { get { return true; } }

        public void Read(IReader r, Action<object> dest)
        {
            int iEnd = r.ReadInt32();
            Deque<object> result = new Deque<object>();
            result.Capacity = iEnd;
            for (int i = 0; i < iEnd; ++i)
            {
                result.PushBack(false);
            }

            for (int i = 0; i < iEnd; ++i)
            {
                if (element.IsByRef)
                {
                    r.ReadReference(element, delegate(object obj) { result[i] = obj; });
                }
                else
                {
                    element.Read(r, delegate(object obj) { result[i] = obj; });
                }
            }

            dest(result);
        }

        public void Test(ITester t, object obj, Action<bool> dest)
        {
            if (obj is Deque<object>)
            {
                Deque<object> d = (Deque<object>)obj;
                int iEnd = d.Count;
                BigInteger b = (BigInteger.One << iEnd) - BigInteger.One;
                bool x = true;
                for(int i = 0; i < iEnd; ++i)
                {
                    int j = i;
                    Action<bool> dest2 = delegate(bool r)
                    {
                        x &= r;
                        b &= ~(BigInteger.One << j);
                        if (b.IsZero)
                        {
                            dest(x);
                        }
                    };

                    t.TestReference(element, d[i], dest2);
                }
            }
            else
            {
                dest(false);
            }
        }

        public void Write(IWriter w, object obj)
        {
            if (obj is Deque<object>)
            {
                Deque<object> d = (Deque<object>)obj;
                int iEnd = d.Count;
                w.WriteInt32(iEnd);

                for (int i = 0; i < iEnd; ++i)
                {
                    if (element.IsByRef)
                    {
                        w.WriteReference(element, d[i]);
                    }
                    else
                    {
                        element.Write(w, d[i]);
                    }
                }
            }
            else
            {
                throw new ArgumentException("Vector expected");
            }
        }

        #endregion
    }

    [Pattern("(vector-of $element)")]
    public class VectorOfCodecDesc : CodecDesc
    {
        [Bind("$element")]
        public CodecDesc element;

        public override ICodec Compile(Environment env)
        {
            return new VectorOfCodec(element.Compile(env));
        }
    }

    public class ByteArrayCodec : ICodec
    {
        public ByteArrayCodec() { }

        #region ICodec Members

        public bool IsByRef { get { return true; } }

        public void Read(IReader r, Action<object> dest)
        {
            byte[] b0 = r.ReadByteArray();
            dest(b0);
        }

        public void Test(ITester t, object src, Action<bool> dest)
        {
            if (src is byte[])
            {
                dest(true);
            }
            else if (src is ExprObjModel.Procedures.AbstractByteRange)
            {
                ExprObjModel.Procedures.AbstractByteRange abr = (ExprObjModel.Procedures.AbstractByteRange)src;
                if (abr.Length > (long)(int.MaxValue))
                {
                    dest(false);
                }
                else
                {
                    dest(true);
                }
            }
            else
            {
                dest(false);
            }
        }

        public void Write(IWriter w, object obj)
        {
            if (obj is byte[])
            {
                byte[] b0 = (byte[])obj;
                w.WriteByteArray(b0, 0, b0.Length);
            }
            else if (obj is ExprObjModel.Procedures.AbstractByteRange)
            {
                ExprObjModel.Procedures.AbstractByteRange abr = (ExprObjModel.Procedures.AbstractByteRange)obj;
                if (abr.Length > (long)(int.MaxValue))
                {
                    throw new ArgumentException("Byte array too long");
                }
                else
                {
                    int len = unchecked((int)(abr.Length));
                    ExprObjModel.Procedures.SchemeByteArray sbr = new Procedures.SchemeByteArray(len);
                    ExprObjModel.Procedures.ByteRange br = new ExprObjModel.Procedures.ByteRange(sbr, 0, sbr.Length);
                    ExprObjModel.Procedures.ProxyDiscovery.ByteCopy(abr, br);
                    w.WriteByteArray(sbr.Bytes, 0, sbr.Length);
                }
            }
            else
            {
                throw new ArgumentException("Byte array or byte range expected");
            }
        }

        #endregion
    }

    [Pattern("byte-array")]
    public class ByteArrayCodecDesc : CodecDesc
    {
        public override ICodec Compile(Environment env)
        {
            return new ByteArrayCodec();
        }
    }

    public class StringCodec : ICodec
    {
        public StringCodec() { }

        #region ICodec Members

        public bool IsByRef { get { return true; } }

        public void Read(IReader r, Action<object> dest)
        {
            byte[] br = r.ReadByteArray();
            string str = System.Text.Encoding.UTF8.GetString(br);
            SchemeString sstr = new SchemeString(str);
            dest(sstr);
        }

        public void Test(ITester t, object src, Action<bool> dest)
        {
            if (src is SchemeString)
            {
                dest(true);
            }
            else
            {
                dest(false);
            }
        }

        public void Write(IWriter w, object obj)
        {
            if (obj is SchemeString)
            {
                byte[] br = System.Text.Encoding.UTF8.GetBytes(((SchemeString)obj).TheString);
                w.WriteByteArray(br, 0, br.Length);
            }
            else
            {
                throw new ArgumentException("String expected");
            }
        }

        #endregion
    }

    [Pattern("string")]
    public class StringCodecDesc : CodecDesc
    {
        public override ICodec Compile(Environment env)
        {
            return new StringCodec();
        }
    }

    public class SymbolCodec : ICodec
    {
        public SymbolCodec() { }

        #region ICodec Members

        public bool IsByRef
        {
            get { return true; }
        }

        public void Read(IReader r, Action<object> dest)
        {
            byte type = r.ReadByte();
            if (type == 0)
            {
                byte[] br = r.ReadByteArray();
                string str = System.Text.Encoding.UTF8.GetString(br);
                dest(new Symbol(str));
            }
            else
            {
                dest(new Symbol());
            }
        }

        public void Test(ITester t, object src, Action<bool> dest)
        {
            if (src is Symbol)
            {
                dest(true);
            }
            else
            {
                dest(false);
            }
        }

        public void Write(IWriter w, object obj)
        {
            Symbol s = (Symbol)obj;
            if (s.IsInterned)
            {
                w.WriteByte(0);
                byte[] br = System.Text.Encoding.UTF8.GetBytes(s.Name);
                w.WriteByteArray(br, 0, br.Length);
            }
            else
            {
                w.WriteByte(1);
            }
        }

        #endregion
    }

    [Pattern("symbol")]
    public class SymbolCodecDesc : CodecDesc
    {
        public override ICodec Compile(Environment env)
        {
            return new SymbolCodec();
        }
    }

    public class EmptyListCodec : ICodec
    {
        public EmptyListCodec() { }

        #region ICodec Members

        public bool IsByRef { get { return false; } }

        public void Read(IReader r, Action<object> dest)
        {
            dest(SpecialValue.EMPTY_LIST);
        }

        public void Test(ITester t, object src, Action<bool> dest)
        {
            dest(ConsCell.IsEmptyList(src));
        }

        public void Write(IWriter w, object obj)
        {
            if (!(ConsCell.IsEmptyList(obj))) throw new ArgumentException("Empty list expected");
        }

        #endregion
    }

    [Pattern("empty-list")]
    public class EmptyListCodecDesc : CodecDesc
    {
        public override ICodec Compile(Environment env)
        {
            return new EmptyListCodec();
        }
    }

    public class BigIntCodec : ICodec
    {
        public BigIntCodec() { }

        #region ICodec Members

        public bool IsByRef { get { return false; } }

        public void Read(IReader r, Action<object> dest)
        {
            BigInteger b = r.ReadBigInteger(true);
            dest(b);
        }

        public void Test(ITester t, object src, Action<bool> dest)
        {
            dest(src is BigInteger);
        }

        public void Write(IWriter w, object obj)
        {
            if (obj is BigInteger)
            {
                w.WriteBigInteger((BigInteger)obj, true);
            }
            else
            {
                throw new ArgumentException("BigInteger expected");
            }
        }

        #endregion
    }

    [Pattern("bigint")]
    public class BigIntCodecDesc : CodecDesc
    {
        public override ICodec Compile(Environment env)
        {
            return new BigIntCodec();
        }
    }

    public class RationalCodec : ICodec
    {
        public RationalCodec() { }

        #region ICodec Members

        public bool IsByRef { get { return false; } }

        public void Read(IReader r, Action<object> dest)
        {
            byte b = r.ReadByte();
            if (b == 0)
            {
                BigInteger n = r.ReadBigInteger(true);
                dest(n);
            }
            else
            {
                BigInteger n = r.ReadBigInteger(true);
                BigInteger d = r.ReadBigInteger(false);
                dest(new BigRational(n, d));
            }
        }

        public void Test(ITester t, object src, Action<bool> dest)
        {
            dest(src is BigInteger || src is BigRational);
        }

        public void Write(IWriter w, object obj)
        {
            if (obj is BigInteger)
            {
                BigInteger b = (BigInteger)obj;
                w.WriteByte(0);
                w.WriteBigInteger(b, true);
            }
            else if (obj is BigRational)
            {
                w.WriteByte(1);
                BigRational b = (BigRational)obj;
                w.WriteBigInteger(b.Numerator, true);
                w.WriteBigInteger(b.Denominator, false);
            }
            else
            {
                throw new ArgumentException("BigInteger or BigRational expected");
            }
        }

        #endregion
    }

    [Pattern("rational")]
    public class RationalCodecDesc : CodecDesc
    {
        public override ICodec Compile(Environment env)
        {
            return new RationalCodec();
        }
    }

    public class NumberCodec : ICodec
    {
        public NumberCodec() { }

        #region ICodec Members

        public bool IsByRef { get { return false; } }

        public void Read(IReader r, Action<object> dest)
        {
            byte t = r.ReadByte();
            if (t == 0)
            {
                BigInteger b = r.ReadBigInteger(true);
                dest(b);
            }
            else if (t == 1)
            {
                BigInteger n = r.ReadBigInteger(true);
                BigInteger d = r.ReadBigInteger(false);
                BigRational rat = new BigRational(n, d);
                dest(rat);
            }
            else if (t == 2)
            {
                double d = r.ReadDouble();
                dest(d);
            }
            else
            {
                throw new FormatException("Unknown type of Scheme number");
            }
        }

        public void Test(ITester t, object src, Action<bool> dest)
        {
            dest(src is BigInteger || src is BigRational || src is double);
        }

        public void Write(IWriter w, object obj)
        {
            if (obj is BigInteger)
            {
                w.WriteByte(0);
                w.WriteBigInteger((BigInteger)obj, true);
            }
            else if (obj is BigRational)
            {
                w.WriteByte(1);
                BigRational rat = (BigRational)obj;
                w.WriteBigInteger(rat.Numerator, true);
                w.WriteBigInteger(rat.Denominator, false);
            }
            else if (obj is double)
            {
                w.WriteByte(2);
                w.WriteDouble((double)obj);
            }
            else
            {
                throw new ArgumentException("Scheme number expected");
            }
        }

        #endregion
    }

    [Pattern("number")]
    public class NumberCodecDesc : CodecDesc
    {
        public override ICodec Compile(Environment env)
        {
            return new NumberCodec();
        }
    }

    public class ConsCellCodec : ICodec
    {
        private ICodec cCar;
        private ICodec cCdr;

        public ConsCellCodec(ICodec cCar, ICodec cCdr) { this.cCar = cCar; this.cCdr = cCdr; }

        #region ICodec Members

        public bool IsByRef { get { return true; } }

        public void Read(IReader r, Action<object> dest)
        {
            ConsCell c = new ConsCell();
            int flags = 3;

            if (cCar.IsByRef)
            {
                r.ReadReference
                (
                    cCar,
                    delegate(object obj)
                    {
                        c.car = obj;
                        flags &= ~1;
                        if (flags == 0) dest(c);
                    }
                );
            }
            else
            {
                cCar.Read
                (
                    r,
                    delegate(object obj)
                    {
                        c.car = obj;
                        flags &= ~1;
                        if (flags == 0) dest(c);
                    }
                );
            }

            if (cCdr.IsByRef)
            {
                r.ReadReference
                (
                    cCdr,
                    delegate(object obj)
                    {
                        c.cdr = obj;
                        flags &= ~2;
                        if (flags == 0) dest(c);
                    }
                );
            }
            else
            {
                cCdr.Read
                (
                    r,
                    delegate(object obj)
                    {
                        c.cdr = obj;
                        flags &= ~2;
                        if (flags == 0) dest(c);
                    }
                );
            }
        }

        public void Test(ITester t, object src, Action<bool> dest)
        {
            if (src is ConsCell)
            {
                ConsCell c = (ConsCell)src;
                bool result = true;
                int flags = 3;

                t.TestReference
                (
                    cCar,
                    c.car,
                    delegate(bool r)
                    {
                        result &= r;
                        flags &= ~1;
                        if (flags == 0) dest(result);
                    }
                );
                t.TestReference
                (
                    cCdr,
                    c.cdr,
                    delegate(bool r)
                    {
                        result &= r;
                        flags &= ~2;
                        if (flags == 0) dest(result);
                    }
                );
            }
            else
            {
                dest(false);
            }
        }

        public void Write(IWriter w, object obj)
        {
            if (obj is ConsCell)
            {
                ConsCell c = (ConsCell)obj;
                if (cCar.IsByRef)
                {
                    w.WriteReference(cCar, c.car);
                }
                else
                {
                    cCar.Write(w, c.car);
                }
                if (cCdr.IsByRef)
                {
                    w.WriteReference(cCdr, c.cdr);
                }
                else
                {
                    cCdr.Write(w, c.cdr);
                }
            }
            else
            {
                throw new ArgumentException("ConsCell expected");
            }
        }

        #endregion
    }

    [Pattern("(cons $car $cdr)")]
    public class ConsCellCodecDesc : CodecDesc
    {
        [Bind("$car")]
        public CodecDesc cCar;

        [Bind("$cdr")]
        public CodecDesc cCdr;

        public override ICodec Compile(Environment env)
        {
            return new ConsCellCodec(cCar.Compile(env), cCdr.Compile(env));
        }
    }

    public class EofCodec : ICodec
    {
        public EofCodec() { }

        #region ICodec Members

        public bool IsByRef { get { return false; } }

        public void Read(IReader r, Action<object> dest)
        {
            dest(SpecialValue.EOF);
        }

        public void Test(ITester t, object src, Action<bool> dest)
        {
            dest(ExprObjModel.Procedures.ProxyDiscovery.IsEof(src));
        }

        public void Write(IWriter w, object obj)
        {
            if (!(ExprObjModel.Procedures.ProxyDiscovery.IsEof(obj))) throw new ArgumentException("EOF object expected");
        }

        #endregion
    }

    [Pattern("eof")]
    public class EofCodecDesc : CodecDesc
    {
        public override ICodec Compile(Environment env)
        {
            return new EofCodec();
        }
    }

    public class CharCodec : ICodec
    {
        public CharCodec() { }

        #region ICodec Members

        public bool IsByRef { get { return false; } }

        public void Read(IReader r, Action<object> dest)
        {
            dest(r.ReadChar());
        }

        public void Test(ITester t, object src, Action<bool> dest)
        {
            dest(src is char);
        }

        public void Write(IWriter w, object obj)
        {
            if (obj is char)
            {
                w.WriteChar((char)obj);
            }
            else
            {
                throw new ArgumentException("Char expected");
            }
        }

        #endregion
    }

    [Pattern("char")]
    public class CharCodecDesc : CodecDesc
    {
        public override ICodec Compile(Environment env)
        {
            return new CharCodec();
        }
    }

    public class BooleanCodec : ICodec
    {
        public BooleanCodec() { }

        #region ICodec Members

        public bool IsByRef { get { return false; } }

        public void Read(IReader r, Action<object> dest)
        {
            dest(r.ReadBoolean());
        }

        public void Test(ITester t, object src, Action<bool> dest)
        {
            dest(src is bool);
        }

        public void Write(IWriter w, object obj)
        {
            if (obj is bool)
            {
                w.WriteBoolean((bool)obj);
            }
            else
            {
                throw new ArgumentException("Boolean expected");
            }
        }

        #endregion
    }

    [Pattern("bool")]
    public class BooleanCodecDesc : CodecDesc
    {
        public override ICodec Compile(Environment env)
        {
            return new BooleanCodec();
        }
    }

    public class GuidCodec : ICodec
    {
        public GuidCodec() { }

        #region ICodec Members

        public bool IsByRef { get { return false; } }

        public void Read(IReader r, Action<object> dest)
        {
            dest(r.ReadGuid());
        }

        public void Test(ITester t, object src, Action<bool> dest)
        {
            dest(src is Guid);
        }

        public void Write(IWriter w, object obj)
        {
            if (obj is Guid)
            {
                w.WriteGuid((Guid)obj);
            }
            else
            {
                throw new ArgumentException("Guid expected");
            }
        }

        #endregion
    }

    [Pattern("guid")]
    public class GuidCodecDesc : CodecDesc
    {
        public override ICodec Compile(Environment env)
        {
            return new GuidCodec();
        }
    }

    public class TheSymbolCodec : ICodec
    {
        private Symbol theSymbol;

        public TheSymbolCodec(Symbol theSymbol)
        {
            this.theSymbol = theSymbol;
        }

        #region ICodec Members

        public bool IsByRef { get { return false; } }

        public void Read(IReader r, Action<object> dest)
        {
            dest(theSymbol);
        }

        public void Test(ITester t, object src, Action<bool> dest)
        {
            dest(src is Symbol && (((Symbol)src) == theSymbol));
        }

        public void Write(IWriter w, object obj)
        {
            if (!(obj is Symbol && (((Symbol)obj) == theSymbol)))
            {
                throw new ArgumentException("Expecting the symbol " + theSymbol);
            }
        }

        #endregion
    }

    [Pattern("(the-symbol $sym)")]
    public class TheSymbolCodecDesc : CodecDesc
    {
        [Bind("$sym")]
        public Symbol theSymbol;

        public override ICodec Compile(Environment env)
        {
            return new TheSymbolCodec(theSymbol);
        }
    }

    public class TheBoolCodec : ICodec
    {
        private bool theBool;

        public TheBoolCodec(bool theBool)
        {
            this.theBool = theBool;
        }

        #region ICodec Members

        public bool IsByRef { get { return false; } }

        public void Read(IReader r, Action<object> dest)
        {
            dest(theBool);
        }

        public void Test(ITester t, object src, Action<bool> dest)
        {
            dest(src is bool && (((bool)src) == theBool));
        }

        public void Write(IWriter w, object obj)
        {
            if (!(obj is bool && (((bool)obj) == theBool)))
            {
                throw new ArgumentException("Expecting the bool " + (theBool ? "#t" : "#f"));
            }
        }

        #endregion
    }

    [Pattern("#t")]
    public class TheTrueBoolDesc : CodecDesc
    {
        public override ICodec Compile(Environment env)
        {
            return new TheBoolCodec(true);
        }
    }

    [Pattern("#f")]
    public class TheFalseBoolDesc : CodecDesc
    {
        public override ICodec Compile(Environment env)
        {
            return new TheBoolCodec(false);
        }
    }

    public class CanWriteSProc : IProcedure
    {
        private ICodec c;

        public CanWriteSProc(ICodec c)
        {
            this.c = c;
        }

        public int Arity { get { return 1; } }
        public bool More { get { return false; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> args, IContinuation k)
        {
            try
            {
                if (args == null) throw new SchemeRuntimeException("can-write: Insufficient arguments");
                if (args.Tail != null) throw new SchemeRuntimeException("can-write: Too many arguments");

                bool canWrite = CodingUtils.Test(args.Head, c);

                return new RunnableReturn(k, canWrite);
            }
            catch (Exception exc)
            {
                return new RunnableThrow(k, exc);
            }
        }
    }

    public class WriteSProc : IProcedure
    {
        private ICodec c;

        public WriteSProc(ICodec c)
        {
            this.c = c;
        }

        public int Arity { get { return 1; } }
        public bool More { get { return false; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> args, IContinuation k)
        {
            try
            {
                if (args == null) throw new SchemeRuntimeException("write: Insufficient arguments");
                if (args.Tail != null) throw new SchemeRuntimeException("write: Too many arguments");

                byte[] b0 = CodingUtils.Write(args.Head, c);

                return new RunnableReturn(k, new ExprObjModel.Procedures.SchemeByteArray(b0, DigitOrder.LBLA));
            }
            catch (Exception exc)
            {
                return new RunnableThrow(k, exc);
            }
        }
    }

    public class ReadSProc : IProcedure
    {
        private ICodec c;

        public ReadSProc(ICodec c)
        {
            this.c = c;
        }

        public int Arity { get { return 1; } }
        public bool More { get { return false; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> args, IContinuation k)
        {
            try
            {
                if (args == null) throw new SchemeRuntimeException("read: Insufficient arguments");
                if (args.Tail != null) throw new SchemeRuntimeException("read: Too many arguments");

                byte[] b0;

                if (args.Head is ExprObjModel.Procedures.SchemeByteArray)
                {
                    b0 = ((ExprObjModel.Procedures.SchemeByteArray)(args.Head)).Bytes;
                }
                else if (args.Head is ExprObjModel.Procedures.AbstractByteRange)
                {
                    ExprObjModel.Procedures.AbstractByteRange br = (ExprObjModel.Procedures.AbstractByteRange)(args.Head);
                    if (br.Length > (long)(int.MaxValue)) throw new SchemeRuntimeException("read: Byte array too long");

                    ExprObjModel.Procedures.SchemeByteArray sb0 = new ExprObjModel.Procedures.SchemeByteArray((int)(br.Length));
                    ExprObjModel.Procedures.ByteRange br2 = new ExprObjModel.Procedures.ByteRange(sb0, 0, sb0.Length);

                    ExprObjModel.Procedures.ProxyDiscovery.ByteCopy(br, br2);
                    b0 = br2.Array.Bytes;
                }
                else 
                {
                    throw new SchemeRuntimeException("read: Byte array or byte range expected");
                }

                object r = CodingUtils.Read(b0, c);

                return new RunnableReturn(k, r);
            }
            catch (Exception exc)
            {
                return new RunnableThrow(k, exc);
            }
        }
    }

    [SchemeSingleton("make-codec")]
    public class MakeCodecProc : IProcedure
    {
        private static Lazy<Func<object, Option<object>>> coderParser = new Lazy<Func<object, Option<object>>>(() => Utils.MakeParser(typeof(CodecDesc)), true);

        [SchemeFunction("test-parse-codec")]
        public static object TestParser(object obj)
        {
            Option<object> k = coderParser.Value(obj);
            if (k is Some<object>)
            {
                CodecDesc t = (CodecDesc)(((Some<object>)k).value);

                return t.Compile(Environment.Empty);
            }
            else
            {
                throw new SchemeRuntimeException("parse failed");
            }
        }

        public MakeCodecProc() { }

        public int Arity { get { return 2; } }
        public bool More { get { return false; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> args, IContinuation k)
        {
            try
            {
                if (args == null) throw new SchemeRuntimeException("make-codec: insufficient arguments");
                object form = args.Head;
                args = args.Tail;

                if (args == null) throw new SchemeRuntimeException("make-codec: insufficient arguments");
                if (!(args.Head is IProcedure)) throw new SchemeRuntimeException("make-codec: second argument must be a procedure");
                IProcedure proc = (IProcedure)(args.Head);
                if (!(proc.AcceptsParameterCount(3))) throw new SchemeRuntimeException("make-codec: procedure must accept 3 arguments");

                Option<object> opt = coderParser.Value(form);
                if (opt is Some<object>)
                {
                    CodecDesc t = (CodecDesc)(((Some<object>)opt).value);

                    ICodec ic = t.Compile(Environment.Empty);

                    FList<object> par = new FList<object>(new WriteSProc(ic));
                    par = new FList<object>(new CanWriteSProc(ic), par);
                    par = new FList<object>(new ReadSProc(ic), par);

                    return new RunnableCall(proc, par, k);
                }
                else
                {
                    throw new SchemeRuntimeException("make-codec: parse failed");
                }
            }
            catch (Exception exc)
            {
                return new RunnableThrow(k, exc);
            }
        }
    }
}
