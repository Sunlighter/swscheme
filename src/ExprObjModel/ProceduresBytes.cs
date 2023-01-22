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
using BigMath;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace ExprObjModel.Procedures
{
    [Serializable]
    [SchemeIsAFunction("bytes?")]
    public class SchemeByteArray
    {
        private DigitOrder digitOrder;
        private byte[] bytes;

        [SchemeFunction("make-bytes")]
        public SchemeByteArray(int i)
        {
            bytes = new byte[i];
            digitOrder = DigitOrder.LBLA;
        }

        public SchemeByteArray(byte[] b, DigitOrder d)
        {
            bytes = b;
            digitOrder = d;
        }

        public int Length { get { return bytes.Length; } }

        public byte[] Bytes { get { return bytes; } }

        public bool BigEndian
        {
            [SchemeFunction("byte-get-hbla")]
            get { return digitOrder == DigitOrder.HBLA; }
            [SchemeFunction("byte-set-hbla!")]
            set { digitOrder = value ? DigitOrder.HBLA : DigitOrder.LBLA; }
        }

        [SchemeFunction("big-endian")]
        public static SchemeByteArray SetBigEndian(SchemeByteArray b0)
        {
            return new SchemeByteArray(b0.Bytes, DigitOrder.HBLA);
        }

        [SchemeFunction("little-endian")]
        public static SchemeByteArray SetLittleEndian(SchemeByteArray b0)
        {
            return new SchemeByteArray(b0.Bytes, DigitOrder.LBLA);
        }
        
        public byte ByteRef(int off) { return bytes[off]; }

        public byte ByteRef(long off)
        {
            if (off < 0L || off > bytes.LongLength) throw new IndexOutOfRangeException();
            return bytes[(int)off];
        }

        public void ByteSet(int off, byte val) { bytes[off] = val; }

        public void ByteSet(long off, byte val)
        {
            if (off < 0L || off > bytes.LongLength) throw new IndexOutOfRangeException();
            bytes[(int)off] = val;
        }

        public BigInteger ByteRefInt(int off, int len)
        {
            return BigInteger.FromByteArray(bytes, off, len, true, digitOrder);
        }

        public BigInteger ByteRefInt(int off, int len, bool signed, DigitOrder? dord)
        {
            return BigInteger.FromByteArray(bytes, off, len, signed, dord ?? digitOrder);
        }

        public BigInteger ByteRefUInt(int off, int len)
        {
            return BigInteger.FromByteArray(bytes, off, len, false, digitOrder);
        }

        public void ByteSetInt(int off, int len, BigInteger val)
        {
            val.WriteBytesToArray(bytes, off, len, false, OverflowBehavior.Wraparound, digitOrder);
        }

        public void ByteSetIntWithSaturation(int off, int len, BigInteger val)
        {
            val.WriteBytesToArray(bytes, off, len, true, OverflowBehavior.Saturate, digitOrder);
        }

        public void ByteSetUintWithSaturation(int off, int len, BigInteger val)
        {
            val.WriteBytesToArray(bytes, off, len, false, OverflowBehavior.Saturate, digitOrder);
        }

        [SchemeFunction("byte-ref-double")]
        public double ByteRefDouble(int off)
        {
            if (digitOrder == DigitOrder.HBLA)
            {
                byte[] x = new byte[8];
                int x0 = 8;
                int x1 = off;
                while (x0 > 0)
                {
                    --x0;
                    x[x0] = bytes[x1];
                    ++x1;
                }
                return BitConverter.ToDouble(x, 0);
            }
            else
            {
                return BitConverter.ToDouble(bytes, off);
            }
        }

        [SchemeFunction("byte-set-double!")]
        public void ByteSetDouble(int off, double val)
        {
            byte[] x = BitConverter.GetBytes(val);
            if (digitOrder == DigitOrder.HBLA)
            {
                byte[] y = new byte[8];
                int x0 = 8;
                int y0 = 0;
                while (x0 > 0)
                {
                    --x0;
                    y[y0] = x[x0];
                    ++y0;
                }
                x = y;
            }
            Array.Copy(x, 0, bytes, off, 8);
        }

        [SchemeFunction("byte-ref-float")]
        public float ByteRefFloat(int off)
        {
            if (digitOrder == DigitOrder.HBLA)
            {
                byte[] x = new byte[4];
                int x0 = 4;
                int x1 = off;
                while (x0 > 0)
                {
                    --x0;
                    x[x0] = bytes[x1];
                    ++x1;
                }
                return BitConverter.ToSingle(x, 0);
            }
            else
            {
                return BitConverter.ToSingle(bytes, off);
            }
        }

        [SchemeFunction("byte-set-float!")]
        public void ByteSetFloat(int off, float val)
        {
            byte[] x = BitConverter.GetBytes(val);
            if (digitOrder == DigitOrder.HBLA)
            {
                byte[] y = new byte[4];
                int x0 = 4;
                int y0 = 0;
                while (x0 > 0)
                {
                    --x0;
                    y[y0] = x[x0];
                    ++y0;
                }
                x = y;
            }
            Array.Copy(x, 0, bytes, off, 4);
        }

        [SchemeFunction("byte-ref-guid")]
        public Guid ByteRefGuid(int off)
        {
            byte[] b2 = new byte[16];
            Buffer.BlockCopy(bytes, off, b2, 0, 16);
            return new Guid(b2);
        }

        [SchemeFunction("byte-set-guid!")]
        public void ByteSetGuid(int off, Guid g)
        {
            byte[] b2 = g.ToByteArray();
            Buffer.BlockCopy(b2, 0, bytes, off, 16);
        }

        public string ByteRefString(int off, int len)
        {
            StringBuilder sb = new StringBuilder();
            while (len > 0)
            {
                --len;
                if (bytes[off] == 0) break;
                sb.Append((char)(bytes[off++]));
            }
            return sb.ToString();
        }

        public void ByteSetString(int off, int len, string str)
        {
            int j = 0;
            int sLen = str.Length;
            int offEnd = off + len;
            while (j < sLen && off < offEnd)
            {
                bytes[off] = unchecked((byte)str[j]);
                ++off; ++j;
            }
            while (off < offEnd)
            {
                bytes[off++] = 0;
            }
        }

        public bool IsValidRange(int off, int len)
        {
            return (off >= 0) && ((off + len) >= off) && ((off + len) <= bytes.Length);
        }

        public bool IsValidRange(long off, int len)
        {
            return (off >= 0L) && ((off + (long)len) >= off) && ((off + (long)len) <= bytes.LongLength);
        }

        public bool IsValidRange(long off, long len)
        {
            return (off >= 0L) && ((off + len) >= off) && ((off + len) < bytes.LongLength);
        }
    }

    [SchemeIsAFunction("byterange?")]
    public abstract class AbstractByteRange
    {
        protected AbstractByteRange() { }

        public abstract long Length { [SchemeFunction("byterange-length")] get; }

        public abstract AbstractByteRange GetSubRange(long off, long len);
    }
    
    [SchemeIsAFunction("byterange-array-backed?")]
    public class ByteRange : AbstractByteRange
    {
        private SchemeByteArray array;
        private int off;
        private int len;

        public ByteRange(SchemeByteArray array, int off, int len)
        {
            this.array = array;
            this.off = off;
            this.len = len;
        }

        public SchemeByteArray Array { [SchemeFunction("byterange-bytes")] get { return array; } }

        public int Offset { [SchemeFunction("byterange-offset")] get { return off; } }

        public override long Length { get { return (long)len; } }

        public int LengthInt32 { get { return len; } }

        public override AbstractByteRange GetSubRange(long off2, long len2)
        {
            if ((off2 > (long)len) || (off2 + len2 > (long)len)) throw new ArgumentException("Out of bounds");

            return new ByteRange(array, (int)(off + off2), (int)len2);
        }

        public bool IsValid { [SchemeFunction("byterange-valid?")] get { return array != null && array.IsValidRange(off, len); } }
    }

    [SchemeIsAFunction("byterange-native?")]
    public class NativeByteRange : AbstractByteRange
    {
        private IntPtr ptr;
        private UIntPtr len;

        public NativeByteRange(IntPtr ptr, UIntPtr len)
        {
            this.ptr = ptr;
            this.len = len;
        }

        public IntPtr Ptr
        {
            get { return ptr; }
        }

        public override long Length
        {
            get { return (long)len; }
        }

        public override AbstractByteRange GetSubRange(long off2, long len2)
        {
            if ((off2 > (long)len) || (off2 + len2 > (long)len)) throw new ArgumentException("Out of bounds");

            return new NativeByteRange((IntPtr)(((long)ptr) + off2), (UIntPtr)len2);
        }
    }

    [SchemeSingleton("byterange")]
    public class MakeByteRangeProc : IProcedure
    {
        public MakeByteRangeProc() { }

        public int Arity { get { return 1; } }

        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            try
            {
                if (argList == null) throw new SchemeRuntimeException("Insufficient arguments");
                if (argList.Head is SchemeByteArray)
                {
                    SchemeByteArray a = (SchemeByteArray)(argList.Head);
                    argList = argList.Tail;
                    if (argList != null)
                    {
                        if (!(argList.Head is BigInteger)) throw new SchemeRuntimeException("Offset must be an integer");
                        BigInteger off = (BigInteger)argList.Head;
                        if (off.IsNegative) throw new SchemeRuntimeException("Can't make a byte range with a negative offset");
                        if (!(off.FitsInInt32)) throw new SchemeRuntimeException("Offset of byte range too large");

                        int off32 = off.GetInt32Value(OverflowBehavior.Wraparound);

                        argList = argList.Tail;
                        if (argList != null)
                        {
                            if (!(argList.Head is BigInteger)) throw new SchemeRuntimeException("Range must be an integer");
                            BigInteger len = (BigInteger)argList.Head;
                            if (len.IsNegative) throw new SchemeRuntimeException("Can't make a byte range with a negative length");
                            if (!(len.FitsInInt32)) throw new SchemeRuntimeException("Length of byte range too large");

                            int len32 = len.GetInt32Value(OverflowBehavior.Wraparound);

                            argList = argList.Tail;
                            if (argList != null) throw new SchemeRuntimeException("Too many arguments");

                            return new RunnableReturn(k, new ByteRange(a, off32, len32));
                        }

                        return new RunnableReturn(k, new ByteRange(a, off32, Math.Max(0, a.Bytes.Length - off32)));
                    }

                    return new RunnableReturn(k, new ByteRange(a, 0, a.Bytes.Length));
                }
                else if (argList.Head is DisposableID)
                {
                    IDisposable d1 = gs.GetDisposableByID((DisposableID)(argList.Head));
                    if (d1 is NativeMemory)
                    {
                        NativeMemory a = (NativeMemory)d1;

                        argList = argList.Tail;
                        if (argList != null)
                        {
                            if (!(argList.Head is BigInteger)) throw new SchemeRuntimeException("Offset must be an integer");
                            BigInteger off = (BigInteger)argList.Head;
                            if (off.IsNegative) throw new SchemeRuntimeException("Can't make a byte range with a negative offset");
                            if (!(off.FitsInInt64)) throw new SchemeRuntimeException("Offset of byte range too large");

                            long off64 = off.GetInt64Value(OverflowBehavior.Wraparound);

                            if (off64 > (long)(a.Length)) throw new SchemeRuntimeException("Offset of byte range too large");

                            argList = argList.Tail;
                            if (argList != null)
                            {
                                if (!(argList.Head is BigInteger)) throw new SchemeRuntimeException("Range must be an integer");
                                BigInteger len = (BigInteger)argList.Head;
                                if (len.IsNegative) throw new SchemeRuntimeException("Can't make a byte range with a negative length");
                                if (!(len.FitsInInt64)) throw new SchemeRuntimeException("Length of byte range too large");

                                long len64 = len.GetInt64Value(OverflowBehavior.Wraparound);

                                if (off64 + len64 < off64) throw new SchemeRuntimeException("Length of byte range too large");
                                if (off64 + len64 > (long)(a.Length)) throw new SchemeRuntimeException("Length of byte range too large");

                                argList = argList.Tail;
                                if (argList != null) throw new SchemeRuntimeException("Too many arguments");

                                return new RunnableReturn(k, new NativeByteRange((IntPtr)((long)(a.Ptr) + off64), (UIntPtr)len64));
                            }

                            return new RunnableReturn(k, new NativeByteRange((IntPtr)((long)(a.Ptr) + off64), (UIntPtr)Math.Max(0L, (long)(a.Length) - off64)));
                        }

                        return new RunnableReturn(k, new NativeByteRange(a.Ptr, a.Length));
                    }
                    else throw new SchemeRuntimeException("First argument must be a byte array");
                }
                else throw new SchemeRuntimeException("First argument must be a byte array");
            }
            catch (Exception exc)
            {
                return new RunnableThrow(k, exc);
            }
        }
    }

    public static partial class ProxyDiscovery
    {
        private static Action<IntPtr, IntPtr, UIntPtr> memcpy = null;
        private static Action<IntPtr, byte, UIntPtr> memset = null;
        private static Action<byte[], int, byte, int> memset2 = null;

        private static void InitMemFunctions()
        {
            if (memcpy == null)
            {
                lock (typeof(ProxyDiscovery))
                {
                    if (memcpy == null)
                    {
                        InitMemFunctions2();
                    }
                }
            }
        }

        private static void InitMemFunctions2()
        {
            string memCpySrc = @"
(while (nop) (> count (to-uintptr (uint 0)))
  (let* ((uint count1 (if (> count (to-uintptr (uint #x7FFFFF00))) (uint #x7FFFFF00) (to-uint count))))
    (memcpy! dest src count1)
    (set! dest (+ dest (to-intptr count1)))
    (set! src (+ src (to-intptr count1)))
    (set! count (- count (to-uintptr count1)))))
";

            string memSetSrc = @"
(while (nop) (> count (to-uintptr (uint 0)))
  (let* ((uint count1 (if (> count (to-uintptr (uint #x7FFFFF00))) (uint #x7FFFFF00) (to-uint count))))
    (memset! dest b count1)
    (set! dest (+ dest (to-intptr count1)))
    (set! count (- count (to-uintptr count1)))))
";

            string memSet2Src = @"
(pin ((dest dest1))
  (let* ((intptr dest (+ dest (to-intptr off))))
    (memset! dest b (as-uint count))))
";
            object memCpyObj = SchemeDataReader.ReadItem(memCpySrc);

            Pascalesque.One.IExpression memCpyExpr = Pascalesque.One.Syntax.SyntaxAnalyzer.AnalyzeExpr(memCpyObj);

            object memSetObj = SchemeDataReader.ReadItem(memSetSrc);

            Pascalesque.One.IExpression memSetExpr = Pascalesque.One.Syntax.SyntaxAnalyzer.AnalyzeExpr(memSetObj);

            object memSet2Obj = SchemeDataReader.ReadItem(memSet2Src);

            Pascalesque.One.IExpression memSet2Expr = Pascalesque.One.Syntax.SyntaxAnalyzer.AnalyzeExpr(memSet2Obj);

            List<Delegate> ds = Pascalesque.One.Compiler.CompileRunAndCollect
            (
                new Pascalesque.One.MethodToBuild[]
                {
                    new Pascalesque.One.MethodToBuild
                    (
                        typeof(Action<IntPtr, IntPtr, UIntPtr>),
                        new Symbol("memcpy"),
                        new Pascalesque.One.ParamInfo[]
                        {
                            new Pascalesque.One.ParamInfo(new Symbol("dest"), typeof(IntPtr)),
                            new Pascalesque.One.ParamInfo(new Symbol("src"), typeof(IntPtr)),
                            new Pascalesque.One.ParamInfo(new Symbol("count"), typeof(UIntPtr))
                        },
                        typeof(void),
                        memCpyExpr
                    ),
                    new Pascalesque.One.MethodToBuild
                    (
                        typeof(Action<IntPtr, byte, UIntPtr>),
                        new Symbol("memset"),
                        new Pascalesque.One.ParamInfo[]
                        {
                            new Pascalesque.One.ParamInfo(new Symbol("dest"), typeof(IntPtr)),
                            new Pascalesque.One.ParamInfo(new Symbol("b"), typeof(byte)),
                            new Pascalesque.One.ParamInfo(new Symbol("count"), typeof(UIntPtr))
                        },
                        typeof(void),
                        memSetExpr
                    ),
                    new Pascalesque.One.MethodToBuild
                    (
                        typeof(Action<byte[], int, byte, int>),
                        new Symbol("memset2"),
                        new Pascalesque.One.ParamInfo[]
                        {
                            new Pascalesque.One.ParamInfo(new Symbol("dest1"), typeof(byte[])),
                            new Pascalesque.One.ParamInfo(new Symbol("off"), typeof(int)),
                            new Pascalesque.One.ParamInfo(new Symbol("b"), typeof(byte)),
                            new Pascalesque.One.ParamInfo(new Symbol("count"), typeof(int))
                        },
                        typeof(void),
                        memSet2Expr
                    )
                }
            );

            memcpy = (Action<IntPtr, IntPtr, UIntPtr>)(ds[0]);
            memset = (Action<IntPtr, byte, UIntPtr>)(ds[1]);
            memset2 = (Action<byte[], int, byte, int>)(ds[2]);
        }

        public static void MemCpy(IntPtr dest, IntPtr src, UIntPtr count)
        {
            InitMemFunctions();
            memcpy(dest, src, count);
        }

        public static void MemSet(IntPtr dest, byte b, UIntPtr count)
        {
            InitMemFunctions();
            memset(dest, b, count);
        }

        public static void MemSet(byte[] dest, int off, byte b, int count)
        {
            InitMemFunctions();
            memset2(dest, off, b, count);
        }

        [SchemeFunction("byte-copy!")]
        public static void ByteCopy(AbstractByteRange src, AbstractByteRange dest)
        {
            if (src is ByteRange)
            {
                ByteRange brSrc = (ByteRange)src;
                if (dest is ByteRange)
                {
                    ByteRange brDest = (ByteRange)dest;

                    if (!(brSrc.IsValid)) throw new SchemeRuntimeException("byte-copy!: src range is invalid");
                    if (!(brDest.IsValid)) throw new SchemeRuntimeException("byte-copy!: dest range is invalid");

                    int len = Math.Min((int)(brSrc.Length), (int)(brDest.Length));
                    Buffer.BlockCopy(brSrc.Array.Bytes, brSrc.Offset, brDest.Array.Bytes, brDest.Offset, len);
                }
                else if (dest is NativeByteRange)
                {
                    NativeByteRange nbrDest = (NativeByteRange)dest;

                    if (!(brSrc.IsValid)) throw new SchemeRuntimeException("byte-copy!: src range is invalid");

                    int len = (int)Math.Min((long)(brSrc.Length), nbrDest.Length);
                    System.Runtime.InteropServices.Marshal.Copy(brSrc.Array.Bytes, brSrc.Offset, nbrDest.Ptr, len);
                }
                else throw new ArgumentException("Unknown type of byte range: " + dest.GetType().FullName);
            }
            else if (src is NativeByteRange)
            {
                NativeByteRange nbrSrc = (NativeByteRange)src;
                if (dest is ByteRange)
                {
                    ByteRange brDest = (ByteRange)dest;

                    if (!(brDest.IsValid)) throw new SchemeRuntimeException("byte-copy!: dest range is invalid");
                    int len = (int)Math.Min(nbrSrc.Length, (long)(brDest.Length));
                    System.Runtime.InteropServices.Marshal.Copy(nbrSrc.Ptr, brDest.Array.Bytes, brDest.Offset, len);
                }
                else if (dest is NativeByteRange)
                {
                    NativeByteRange nbrDest = (NativeByteRange)dest;

                    long len = Math.Min(nbrSrc.Length, nbrDest.Length);

                    MemCpy(nbrDest.Ptr, nbrSrc.Ptr, (UIntPtr)len);
                }
                else throw new ArgumentException("Unknown type of byte range: " + dest.GetType().FullName);
            }
            else throw new ArgumentException("Unknown type of byte range: " + src.GetType().FullName);            
        }

        [SchemeFunction("byte-length")]
        public static long ByteLength(IGlobalState gs, object obj)
        {
            if (obj is SchemeByteArray)
            {
                SchemeByteArray sba = (SchemeByteArray)obj;
                return (long)(sba.Length);
            }
            else if (obj is DisposableID)
            {
                DisposableID d = (DisposableID)obj;
                IDisposable d2 = gs.GetDisposableByID(d);
                if (d2 is NativeMemory)
                {
                    NativeMemory mb = (NativeMemory)d2;
                    return (long)(mb.Length);
                }
            }
            throw new SchemeRuntimeException("Unknown type of byte array: " + obj.GetType().FullName);
        }

        [SchemeFunction("byte-ref")]
        public static byte ByteRef(IGlobalState gs, object obj, long off)
        {
            if (obj is SchemeByteArray)
            {
                SchemeByteArray sba = (SchemeByteArray)obj;
                return sba.ByteRef(off);
            }
            else if (obj is DisposableID)
            {
                DisposableID d = (DisposableID)obj;
                IDisposable d2 = gs.GetDisposableByID(d);
                if (d2 is NativeMemory)
                {
                    NativeMemory d3 = (NativeMemory)d2;
                    return System.Runtime.InteropServices.Marshal.ReadByte((IntPtr)(((long)d3.Ptr) + off));
                }
                else
                {
                    throw new SchemeRuntimeException("byte-ref: unknown type of byte array: " + d2.GetType().FullName);
                }
            }
            else if (obj is ByteRange)
            {
                ByteRange br = (ByteRange)obj;
                if (off < 0L || off >= br.Length) throw new SchemeRuntimeException("byte-ref: offset out of range");
                return br.Array.ByteRef(br.Offset + off);
            }
            else if (obj is NativeByteRange)
            {
                NativeByteRange nbr = (NativeByteRange)obj;
                if (off < 0L || off >= nbr.Length) throw new SchemeRuntimeException("byte-ref: offset out of range");
                return System.Runtime.InteropServices.Marshal.ReadByte((IntPtr)((long)(nbr.Ptr) + off));
            }
            else
            {
                throw new SchemeRuntimeException("byte-ref: unknown type of byte array: " + obj.GetType().FullName);
            }
        }

        [SchemeFunction("byte-set!")]
        public static void ByteSet(IGlobalState gs, object obj, long off, byte val)
        {
            if (obj is SchemeByteArray)
            {
                SchemeByteArray sba = (SchemeByteArray)obj;
                sba.ByteSet(off, val);
            }
            else if (obj is DisposableID)
            {
                DisposableID d = (DisposableID)obj;
                IDisposable d2 = gs.GetDisposableByID(d);
                if (d2 is NativeMemory)
                {
                    NativeMemory d3 = (NativeMemory)d2;
                    System.Runtime.InteropServices.Marshal.WriteByte((IntPtr)(((long)d3.Ptr) + off), val);
                }
                else
                {
                    throw new SchemeRuntimeException("byte-set!: unknown type of byte array: " + d2.GetType().FullName);
                }
            }
            else if (obj is ByteRange)
            {
                ByteRange br = (ByteRange)obj;
                if (off < 0L || off >= br.Length) throw new SchemeRuntimeException("byte-set!: offset out of range");
                br.Array.ByteSet(br.Offset + off, val);
            }
            else if (obj is NativeByteRange)
            {
                NativeByteRange nbr = (NativeByteRange)obj;
                if (off < 0L || off >= nbr.Length) throw new SchemeRuntimeException("byte-set!: offset out of range");
                System.Runtime.InteropServices.Marshal.WriteByte((IntPtr)((long)(nbr.Ptr) + off), val);
            }
            else
            {
                throw new SchemeRuntimeException("byte-set!: unknown type of byte array: " + obj.GetType().FullName);
            }
        }

        [SchemeFunction("byte-fill!")]
        public static void ByteFill(IGlobalState gs, object dest, byte fill)
        {
            if (dest is ByteRange)
            {
                ByteRange brDest = (ByteRange)dest;
                if (!(brDest.IsValid)) throw new SchemeRuntimeException("byte-fill!: dest range is invalid");

                MemSet(brDest.Array.Bytes, brDest.Offset, fill, brDest.LengthInt32);
            }
            else if (dest is NativeByteRange)
            {
                NativeByteRange nbrDest = (NativeByteRange)dest;
                MemSet(nbrDest.Ptr, fill, (UIntPtr)(nbrDest.Length));
            }
            else
            {
                throw new SchemeRuntimeException("byte-fill!: unknown type of byte range: " + dest.GetType().FullName);
            }
        }

        [SchemeFunction("byte-ref-int")]
        public static BigInteger ByteRefInt(ByteRange src)
        {
            if (!(src.IsValid)) throw new SchemeRuntimeException("byte-ref-int: range is invalid");
            return src.Array.ByteRefInt(src.Offset, src.LengthInt32);
        }

        [SchemeFunction("byte-ref-uint")]
        public static BigInteger ByteRefUInt(ByteRange src)
        {
            if (!(src.IsValid)) throw new SchemeRuntimeException("byte-ref-uint: range is invalid");
            return src.Array.ByteRefUInt(src.Offset, src.LengthInt32);
        }

        [SchemeFunction("byte-set-int!")]
        public static void ByteSetInt(ByteRange dest, BigInteger val)
        {
            if (!(dest.IsValid)) throw new SchemeRuntimeException("byte-set-int!: range is invalid");
            dest.Array.ByteSetInt(dest.Offset, dest.LengthInt32, val);
        }

        [SchemeFunction("byte-set-int-s!")]
        public static void ByteSetIntWithSaturation(ByteRange dest, BigInteger val)
        {
            if (!(dest.IsValid)) throw new SchemeRuntimeException("byte-set-int-s!: range is invalid");
            dest.Array.ByteSetIntWithSaturation(dest.Offset, dest.LengthInt32, val);
        }

        [SchemeFunction("byte-set-uint-s!")]
        public static void ByteSetUIntWithSaturation(ByteRange dest, BigInteger val)
        {
            if (!(dest.IsValid)) throw new SchemeRuntimeException("byte-set-uint-s!: range is invalid");
            dest.Array.ByteSetUintWithSaturation(dest.Offset, dest.LengthInt32, val);
        }

        [SchemeFunction("byte-ref-string")]
        public static string ByteRefString(ByteRange src)
        {
            if (!(src.IsValid)) throw new SchemeRuntimeException("byte-ref-string: range is invalid");
            return src.Array.ByteRefString(src.Offset, src.LengthInt32);
        }

        [SchemeFunction("byte-set-string!")]
        public static void ByteSetString(ByteRange dest, string str)
        {
            if (!(dest.IsValid)) throw new SchemeRuntimeException("byte-set-string!: range is invalid");
            dest.Array.ByteSetString(dest.Offset, dest.LengthInt32, str);
        }

        [SchemeFunction("integer->bytes")]
        public static SchemeByteArray IntegerToBytes(BigInteger b, bool useHbla)
        {
            DigitOrder d = useHbla ? DigitOrder.HBLA : DigitOrder.LBLA;
            return new SchemeByteArray(b.GetByteArray(d), d);
        }

        [SchemeFunction("string->utf8")]
        public static SchemeByteArray StringToUtf8(string str)
        {
            byte[] b = Encoding.UTF8.GetBytes(str);
            return new SchemeByteArray(b, DigitOrder.LBLA);
        }

        [SchemeFunction("utf8->string")]
        public static string Utf8ToString(SchemeByteArray b, int off, int len)
        {
            return Encoding.UTF8.GetString(b.Bytes, off, len);
        }

        [SchemeFunction("byterange-left")]
        public static ByteRange ByteRangeLeft(ByteRange b, int l)
        {
            if (b.Length < l) return b;
            return new ByteRange(b.Array, b.Offset, l);
        }

        [SchemeFunction("byterange-right")]
        public static ByteRange ByteRangeRight(ByteRange b, int l)
        {
            if (b.Length < l) return b;
            return new ByteRange(b.Array, b.Offset + b.LengthInt32 - l, l);
        }

        [SchemeFunction("byterange-trim-left")]
        public static ByteRange ByteRangeTrimLeft(ByteRange b, int l)
        {
            if (b.Length < l) return new ByteRange(b.Array, b.Offset + b.LengthInt32, 0);
            return new ByteRange(b.Array, b.Offset + l, b.LengthInt32 - l);
        }

        [SchemeFunction("byterange-trim-right")]
        public static ByteRange ByteRangeTrimRight(ByteRange b, int l)
        {
            if (b.Length < l) return new ByteRange(b.Array, b.Offset, 0);
            return new ByteRange(b.Array, b.Offset, b.LengthInt32 - l);
        }

        public static byte[] ByteRangeToBytes1(ByteRange b)
        {
            byte[] b1 = new byte[b.Length];
            Buffer.BlockCopy(b.Array.Bytes, b.Offset, b1, 0, b.LengthInt32);
            return b1;
        }

        [SchemeFunction("byterange->bytes")]
        public static SchemeByteArray ByteRangeToBytes(ByteRange b)
        {
            if (!(b.IsValid)) throw new SchemeRuntimeException("Invalid byte range");

            SchemeByteArray s = new SchemeByteArray(b.LengthInt32);
            Buffer.BlockCopy(b.Array.Bytes, b.Offset, s.Bytes, 0, b.LengthInt32);
            return s;
        }
    }

    [SchemeSingleton("bytes")]
    public class ByteProc : IProcedure
    {
        public ByteProc() { }

        public int Arity { get { return 0; } }

        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            int count = 0;
            foreach (object obj in FListUtils.ToEnumerable(argList))
            {
                if (!(obj is BigInteger)) throw new SchemeRuntimeException("bytes: arg is not an integer");
                ++count;
            }

            SchemeByteArray arr = new SchemeByteArray(count);
            int i = 0;
            foreach (object obj in FListUtils.ToEnumerable(argList))
            {
                arr.Bytes[i] = ((BigInteger)obj).GetByteValue(OverflowBehavior.Wraparound);
                ++i;
            }

            return new RunnableReturn(k, arr);
        }
    }

    [SchemeIsAFunction("native-memory?")]
    public class NativeMemory : IDisposable
    {
        public enum MemoryBlockSecurity
        {
            ReadOnly,
            ReadWrite,
            ExecuteRead,
            ExecuteReadWrite
        }

        private IntPtr location;
        private UIntPtr size;
        private MemoryBlockSecurity security;

        [SchemeFunction("make-native-memory")]
        //[SchemeDisposable("native-memory")]
        public NativeMemory(UIntPtr size)
        {
            size = (UIntPtr)(((long)size + 0xFFFFL) & ~0xFFFFL);
            this.size = size;
            location = VirtualAlloc((IntPtr)0, size, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
            security = MemoryBlockSecurity.ReadWrite;
        }

        public MemoryBlockSecurity Security
        {
            get
            {
                return security;
            }
            set
            {
                security = value;
                uint oldProtect;
                VirtualProtect(location, size, GetSecurityFlags(security), out oldProtect);
            }
        }

        public IntPtr Ptr { get { return location; } }

        public UIntPtr Length { get { return size; } }

        public void Dispose()
        {
            VirtualFree(location, (UIntPtr)0, MEM_RELEASE);
        }

        private static uint GetSecurityFlags(MemoryBlockSecurity mbs)
        {
            switch (mbs)
            {
                case MemoryBlockSecurity.ReadOnly: return PAGE_READONLY;
                case MemoryBlockSecurity.ReadWrite: return PAGE_READWRITE;
                case MemoryBlockSecurity.ExecuteRead: return PAGE_EXECUTE_READ;
                case MemoryBlockSecurity.ExecuteReadWrite: return PAGE_EXECUTE_READWRITE;
                default: goto case MemoryBlockSecurity.ReadWrite;
            }
        }

        private static readonly uint MEM_COMMIT = 0x1000;
        private static readonly uint MEM_RESERVE = 0x2000;
        //private static readonly uint MEM_RESET = 0x80000;
        //private static readonly uint MEM_TOP_DOWN = 0x100000;
        //private static readonly uint MEM_DECOMMIT = 0x4000;
        private static readonly uint MEM_RELEASE = 0x8000;

        private static readonly uint PAGE_READONLY = 0x02;
        private static readonly uint PAGE_READWRITE = 0x04;
        private static readonly uint PAGE_EXECUTE_READ = 0x20;
        private static readonly uint PAGE_EXECUTE_READWRITE = 0x40;

        //private static readonly uint PAGE_WRITECOMBINE = 0x400;

        [DllImport("Kernel32.dll", EntryPoint = "VirtualAlloc", SetLastError = true)]
        private static extern IntPtr VirtualAlloc(IntPtr lpAddress, UIntPtr dwSize, uint flAllocationType, uint flProtect);

        [DllImport("Kernel32.dll", EntryPoint = "VirtualFree", SetLastError = true)]
        private static extern bool VirtualFree(IntPtr lpAddress, UIntPtr dwSize, uint dwFreeType);

        [DllImport("Kernel32.dll", EntryPoint = "VirtualProtect", SetLastError = true)]
        private static extern bool VirtualProtect(IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        public static bool Is64Bit
        {
            get
            {
                return IntPtr.Size == 8;
            }
        }

        [DllImport("Kernel32.dll", EntryPoint = "RtlAddFunctionTable")]
        public static extern bool RtlAddFunctionTable(IntPtr functionTable, uint entryCount, IntPtr baseAddress);
    }

}