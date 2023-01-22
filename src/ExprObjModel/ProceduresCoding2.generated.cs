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
using BigMath;

namespace ExprObjModel.Coding
{
    public static partial class CodingUtils
    {
        
        public static void WriteInt16(this IWriter w, short s)
        {
            byte[] b1 = BitConverter.GetBytes(s);
            w.WriteBytes(b1, 0, 2);
        }

        public static short ReadInt16(this IReader r)
        {
            byte[] b1 = new byte[2];
            r.ReadBytes(b1, 0, 2);
            return BitConverter.ToInt16(b1, 0);
        }

        
        public static void WriteInt32(this IWriter w, int i)
        {
            byte[] b1 = BitConverter.GetBytes(i);
            w.WriteBytes(b1, 0, 4);
        }

        public static int ReadInt32(this IReader r)
        {
            byte[] b1 = new byte[4];
            r.ReadBytes(b1, 0, 4);
            return BitConverter.ToInt32(b1, 0);
        }

        
        public static void WriteInt64(this IWriter w, long l)
        {
            byte[] b1 = BitConverter.GetBytes(l);
            w.WriteBytes(b1, 0, 8);
        }

        public static long ReadInt64(this IReader r)
        {
            byte[] b1 = new byte[8];
            r.ReadBytes(b1, 0, 8);
            return BitConverter.ToInt64(b1, 0);
        }

        
        public static void WriteUInt16(this IWriter w, ushort us)
        {
            byte[] b1 = BitConverter.GetBytes(us);
            w.WriteBytes(b1, 0, 2);
        }

        public static ushort ReadUInt16(this IReader r)
        {
            byte[] b1 = new byte[2];
            r.ReadBytes(b1, 0, 2);
            return BitConverter.ToUInt16(b1, 0);
        }

        
        public static void WriteUInt32(this IWriter w, uint ui)
        {
            byte[] b1 = BitConverter.GetBytes(ui);
            w.WriteBytes(b1, 0, 4);
        }

        public static uint ReadUInt32(this IReader r)
        {
            byte[] b1 = new byte[4];
            r.ReadBytes(b1, 0, 4);
            return BitConverter.ToUInt32(b1, 0);
        }

        
        public static void WriteUInt64(this IWriter w, ulong ul)
        {
            byte[] b1 = BitConverter.GetBytes(ul);
            w.WriteBytes(b1, 0, 8);
        }

        public static ulong ReadUInt64(this IReader r)
        {
            byte[] b1 = new byte[8];
            r.ReadBytes(b1, 0, 8);
            return BitConverter.ToUInt64(b1, 0);
        }

        
        public static void WriteSingle(this IWriter w, float f)
        {
            byte[] b1 = BitConverter.GetBytes(f);
            w.WriteBytes(b1, 0, 4);
        }

        public static float ReadSingle(this IReader r)
        {
            byte[] b1 = new byte[4];
            r.ReadBytes(b1, 0, 4);
            return BitConverter.ToSingle(b1, 0);
        }

        
        public static void WriteDouble(this IWriter w, double d)
        {
            byte[] b1 = BitConverter.GetBytes(d);
            w.WriteBytes(b1, 0, 8);
        }

        public static double ReadDouble(this IReader r)
        {
            byte[] b1 = new byte[8];
            r.ReadBytes(b1, 0, 8);
            return BitConverter.ToDouble(b1, 0);
        }

        
    }

    
    public class ByteCodec : ICodec
    {
        public ByteCodec() { }

        public bool IsByRef { get { return false; } }

        public void Read(IReader r, Action<object> dest)
        {
            byte j = r.ReadByte();
            BigInteger jj = BigInteger.FromByte(j);
            dest(jj);
        }

        public void Test(ITester t, object obj, Action<bool> dest)
        {
            if (!(obj is BigInteger))
            {
                dest(false);
            }
            else
            {
                BigInteger jj = (BigInteger)obj;
                dest(jj.FitsInByte);
            }
        }

        public void Write(IWriter w, object obj)
        {
            BigInteger jj = (BigInteger)obj;
            byte j = jj.GetByteValue(OverflowBehavior.Wraparound);
            w.WriteByte(j);
        }
    }

    [Pattern("byte")]
    public class ByteCodecDesc : CodecDesc
    {
        public override ICodec Compile(Environment env)
        {
            return new ByteCodec();
        }
    }

    
    public class Int16Codec : ICodec
    {
        public Int16Codec() { }

        public bool IsByRef { get { return false; } }

        public void Read(IReader r, Action<object> dest)
        {
            short j = r.ReadInt16();
            BigInteger jj = BigInteger.FromInt16(j);
            dest(jj);
        }

        public void Test(ITester t, object obj, Action<bool> dest)
        {
            if (!(obj is BigInteger))
            {
                dest(false);
            }
            else
            {
                BigInteger jj = (BigInteger)obj;
                dest(jj.FitsInInt16);
            }
        }

        public void Write(IWriter w, object obj)
        {
            BigInteger jj = (BigInteger)obj;
            short j = jj.GetInt16Value(OverflowBehavior.Wraparound);
            w.WriteInt16(j);
        }
    }

    [Pattern("short")]
    public class Int16CodecDesc : CodecDesc
    {
        public override ICodec Compile(Environment env)
        {
            return new Int16Codec();
        }
    }

    
    public class Int32Codec : ICodec
    {
        public Int32Codec() { }

        public bool IsByRef { get { return false; } }

        public void Read(IReader r, Action<object> dest)
        {
            int j = r.ReadInt32();
            BigInteger jj = BigInteger.FromInt32(j);
            dest(jj);
        }

        public void Test(ITester t, object obj, Action<bool> dest)
        {
            if (!(obj is BigInteger))
            {
                dest(false);
            }
            else
            {
                BigInteger jj = (BigInteger)obj;
                dest(jj.FitsInInt32);
            }
        }

        public void Write(IWriter w, object obj)
        {
            BigInteger jj = (BigInteger)obj;
            int j = jj.GetInt32Value(OverflowBehavior.Wraparound);
            w.WriteInt32(j);
        }
    }

    [Pattern("int")]
    public class Int32CodecDesc : CodecDesc
    {
        public override ICodec Compile(Environment env)
        {
            return new Int32Codec();
        }
    }

    
    public class Int64Codec : ICodec
    {
        public Int64Codec() { }

        public bool IsByRef { get { return false; } }

        public void Read(IReader r, Action<object> dest)
        {
            long j = r.ReadInt64();
            BigInteger jj = BigInteger.FromInt64(j);
            dest(jj);
        }

        public void Test(ITester t, object obj, Action<bool> dest)
        {
            if (!(obj is BigInteger))
            {
                dest(false);
            }
            else
            {
                BigInteger jj = (BigInteger)obj;
                dest(jj.FitsInInt64);
            }
        }

        public void Write(IWriter w, object obj)
        {
            BigInteger jj = (BigInteger)obj;
            long j = jj.GetInt64Value(OverflowBehavior.Wraparound);
            w.WriteInt64(j);
        }
    }

    [Pattern("long")]
    public class Int64CodecDesc : CodecDesc
    {
        public override ICodec Compile(Environment env)
        {
            return new Int64Codec();
        }
    }

    
    public class SByteCodec : ICodec
    {
        public SByteCodec() { }

        public bool IsByRef { get { return false; } }

        public void Read(IReader r, Action<object> dest)
        {
            sbyte j = r.ReadSByte();
            BigInteger jj = BigInteger.FromSByte(j);
            dest(jj);
        }

        public void Test(ITester t, object obj, Action<bool> dest)
        {
            if (!(obj is BigInteger))
            {
                dest(false);
            }
            else
            {
                BigInteger jj = (BigInteger)obj;
                dest(jj.FitsInSByte);
            }
        }

        public void Write(IWriter w, object obj)
        {
            BigInteger jj = (BigInteger)obj;
            sbyte j = jj.GetSByteValue(OverflowBehavior.Wraparound);
            w.WriteSByte(j);
        }
    }

    [Pattern("sbyte")]
    public class SByteCodecDesc : CodecDesc
    {
        public override ICodec Compile(Environment env)
        {
            return new SByteCodec();
        }
    }

    
    public class UInt16Codec : ICodec
    {
        public UInt16Codec() { }

        public bool IsByRef { get { return false; } }

        public void Read(IReader r, Action<object> dest)
        {
            ushort j = r.ReadUInt16();
            BigInteger jj = BigInteger.FromUInt16(j);
            dest(jj);
        }

        public void Test(ITester t, object obj, Action<bool> dest)
        {
            if (!(obj is BigInteger))
            {
                dest(false);
            }
            else
            {
                BigInteger jj = (BigInteger)obj;
                dest(jj.FitsInUInt16);
            }
        }

        public void Write(IWriter w, object obj)
        {
            BigInteger jj = (BigInteger)obj;
            ushort j = jj.GetUInt16Value(OverflowBehavior.Wraparound);
            w.WriteUInt16(j);
        }
    }

    [Pattern("ushort")]
    public class UInt16CodecDesc : CodecDesc
    {
        public override ICodec Compile(Environment env)
        {
            return new UInt16Codec();
        }
    }

    
    public class UInt32Codec : ICodec
    {
        public UInt32Codec() { }

        public bool IsByRef { get { return false; } }

        public void Read(IReader r, Action<object> dest)
        {
            uint j = r.ReadUInt32();
            BigInteger jj = BigInteger.FromUInt32(j);
            dest(jj);
        }

        public void Test(ITester t, object obj, Action<bool> dest)
        {
            if (!(obj is BigInteger))
            {
                dest(false);
            }
            else
            {
                BigInteger jj = (BigInteger)obj;
                dest(jj.FitsInUInt32);
            }
        }

        public void Write(IWriter w, object obj)
        {
            BigInteger jj = (BigInteger)obj;
            uint j = jj.GetUInt32Value(OverflowBehavior.Wraparound);
            w.WriteUInt32(j);
        }
    }

    [Pattern("uint")]
    public class UInt32CodecDesc : CodecDesc
    {
        public override ICodec Compile(Environment env)
        {
            return new UInt32Codec();
        }
    }

    
    public class UInt64Codec : ICodec
    {
        public UInt64Codec() { }

        public bool IsByRef { get { return false; } }

        public void Read(IReader r, Action<object> dest)
        {
            ulong j = r.ReadUInt64();
            BigInteger jj = BigInteger.FromUInt64(j);
            dest(jj);
        }

        public void Test(ITester t, object obj, Action<bool> dest)
        {
            if (!(obj is BigInteger))
            {
                dest(false);
            }
            else
            {
                BigInteger jj = (BigInteger)obj;
                dest(jj.FitsInUInt64);
            }
        }

        public void Write(IWriter w, object obj)
        {
            BigInteger jj = (BigInteger)obj;
            ulong j = jj.GetUInt64Value(OverflowBehavior.Wraparound);
            w.WriteUInt64(j);
        }
    }

    [Pattern("ulong")]
    public class UInt64CodecDesc : CodecDesc
    {
        public override ICodec Compile(Environment env)
        {
            return new UInt64Codec();
        }
    }

        
    public class SingleCodec : ICodec
    {
        public SingleCodec() { }

        public bool IsByRef { get { return false; } }

        public void Read(IReader r, Action<object> dest)
        {
            float j = r.ReadSingle();
            dest(j);
        }

        public void Test(ITester t, object obj, Action<bool> dest)
        {
            if (obj is BigInteger || obj is BigRational || obj is double)
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
            w.WriteSingle((float)ProxyGenerator.NumberToDouble(obj));
        }
    }

    [Pattern("float")]
    public class SingleCodecDesc : CodecDesc
    {
        public override ICodec Compile(Environment env)
        {
            return new SingleCodec();
        }
    }

        
    public class DoubleCodec : ICodec
    {
        public DoubleCodec() { }

        public bool IsByRef { get { return false; } }

        public void Read(IReader r, Action<object> dest)
        {
            double j = r.ReadDouble();
            dest(j);
        }

        public void Test(ITester t, object obj, Action<bool> dest)
        {
            if (obj is BigInteger || obj is BigRational || obj is double)
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
            w.WriteDouble(ProxyGenerator.NumberToDouble(obj));
        }
    }

    [Pattern("double")]
    public class DoubleCodecDesc : CodecDesc
    {
        public override ICodec Compile(Environment env)
        {
            return new DoubleCodec();
        }
    }

    
    public class Vector2Codec : ICodec
    {
        public Vector2Codec() { }

        #region ICodec Members

        public bool IsByRef
        {
            get { return false; }
        }

        public void Read(IReader r, Action<object> dest)
        {
            dest(r.ReadVector2());
        }

        public void Test(ITester t, object src, Action<bool> dest)
        {
            dest(src is Vector2);
        }

        public void Write(IWriter w, object obj)
        {
            w.WriteVector2((Vector2)obj);
        }

        #endregion
    }

    [Pattern("vector2")]
    public class Vector2CodecDesc : CodecDesc
    {
        public override ICodec Compile(Environment env)
        {
            return new Vector2Codec();
        }
    }

    
    public class Vector3Codec : ICodec
    {
        public Vector3Codec() { }

        #region ICodec Members

        public bool IsByRef
        {
            get { return false; }
        }

        public void Read(IReader r, Action<object> dest)
        {
            dest(r.ReadVector3());
        }

        public void Test(ITester t, object src, Action<bool> dest)
        {
            dest(src is Vector3);
        }

        public void Write(IWriter w, object obj)
        {
            w.WriteVector3((Vector3)obj);
        }

        #endregion
    }

    [Pattern("vector3")]
    public class Vector3CodecDesc : CodecDesc
    {
        public override ICodec Compile(Environment env)
        {
            return new Vector3Codec();
        }
    }

    
    public class Vertex2Codec : ICodec
    {
        public Vertex2Codec() { }

        #region ICodec Members

        public bool IsByRef
        {
            get { return false; }
        }

        public void Read(IReader r, Action<object> dest)
        {
            dest(r.ReadVertex2());
        }

        public void Test(ITester t, object src, Action<bool> dest)
        {
            dest(src is Vertex2);
        }

        public void Write(IWriter w, object obj)
        {
            w.WriteVertex2((Vertex2)obj);
        }

        #endregion
    }

    [Pattern("vertex2")]
    public class Vertex2CodecDesc : CodecDesc
    {
        public override ICodec Compile(Environment env)
        {
            return new Vertex2Codec();
        }
    }

    
    public class Vertex3Codec : ICodec
    {
        public Vertex3Codec() { }

        #region ICodec Members

        public bool IsByRef
        {
            get { return false; }
        }

        public void Read(IReader r, Action<object> dest)
        {
            dest(r.ReadVertex3());
        }

        public void Test(ITester t, object src, Action<bool> dest)
        {
            dest(src is Vertex3);
        }

        public void Write(IWriter w, object obj)
        {
            w.WriteVertex3((Vertex3)obj);
        }

        #endregion
    }

    [Pattern("vertex3")]
    public class Vertex3CodecDesc : CodecDesc
    {
        public override ICodec Compile(Environment env)
        {
            return new Vertex3Codec();
        }
    }

    
    public class QuaternionCodec : ICodec
    {
        public QuaternionCodec() { }

        #region ICodec Members

        public bool IsByRef
        {
            get { return false; }
        }

        public void Read(IReader r, Action<object> dest)
        {
            dest(r.ReadQuaternion());
        }

        public void Test(ITester t, object src, Action<bool> dest)
        {
            dest(src is Quaternion);
        }

        public void Write(IWriter w, object obj)
        {
            w.WriteQuaternion((Quaternion)obj);
        }

        #endregion
    }

    [Pattern("quat")]
    public class QuaternionCodecDesc : CodecDesc
    {
        public override ICodec Compile(Environment env)
        {
            return new QuaternionCodec();
        }
    }

    
    public class Line3Codec : ICodec
    {
        public Line3Codec() { }

        #region ICodec Members

        public bool IsByRef
        {
            get { return false; }
        }

        public void Read(IReader r, Action<object> dest)
        {
            dest(r.ReadLine3());
        }

        public void Test(ITester t, object src, Action<bool> dest)
        {
            dest(src is Line3);
        }

        public void Write(IWriter w, object obj)
        {
            w.WriteLine3((Line3)obj);
        }

        #endregion
    }

    [Pattern("line3")]
    public class Line3CodecDesc : CodecDesc
    {
        public override ICodec Compile(Environment env)
        {
            return new Line3Codec();
        }
    }

    
    public class Plane3Codec : ICodec
    {
        public Plane3Codec() { }

        #region ICodec Members

        public bool IsByRef
        {
            get { return false; }
        }

        public void Read(IReader r, Action<object> dest)
        {
            dest(r.ReadPlane3());
        }

        public void Test(ITester t, object src, Action<bool> dest)
        {
            dest(src is Plane3);
        }

        public void Write(IWriter w, object obj)
        {
            w.WritePlane3((Plane3)obj);
        }

        #endregion
    }

    [Pattern("plane3")]
    public class Plane3CodecDesc : CodecDesc
    {
        public override ICodec Compile(Environment env)
        {
            return new Plane3Codec();
        }
    }

    
}
