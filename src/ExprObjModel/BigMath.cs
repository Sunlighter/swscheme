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
using System.Text;
using ExprObjModel;

namespace BigMath
{
    // DivideOverflow can only be thrown by a low-level primitive
    [Serializable]
    public class DivideOverflowException : Exception
    {
        public DivideOverflowException() { }
        public DivideOverflowException(string message) : base(message) { }
        public DivideOverflowException(string message, Exception inner) : base(message, inner) { }
        protected DivideOverflowException
        (
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context
        )
            : base(info, context)
        {
        }
    }

    public static class MixedPrecision
    {
        public static ulong Combine(uint hi, uint lo)
        {
            return (((ulong)hi) << 32) + (ulong)lo;
        }

        public static void Split(ulong val, out uint hi, out uint lo)
        {
            hi = (uint)(val >> 32);
            lo = (uint)val;
        }

        public static void Add(uint addend1, uint addend2, bool carryIn, out uint sum, out bool carryOut)
        {
            ulong lSum = (ulong)addend1 + (ulong)addend2;
            if (carryIn) lSum++;
            sum = (uint)lSum;
            carryOut = (lSum >= 0x100000000ul);
        }

        public static void Subtract(uint minuend, uint subtrahend, bool borrowIn, out uint diff, out bool borrowOut)
        {
            ulong lDiff = (ulong)minuend + 0x100000000ul - (ulong)subtrahend;
            if (borrowIn) lDiff--;
            diff = (uint)lDiff;
            borrowOut = (lDiff < 0x100000000ul);
        }

        public static void Multiply(uint fac1, uint fac2, uint carryIn, out uint prod, out uint carryOut)
        {
            ulong lProd = (ulong)fac1 * (ulong)fac2 + (ulong)carryIn;
            Split(lProd, out carryOut, out prod);
        }

        public static void Divide(uint dividendHi, uint dividendLo, uint divisor, out uint quotient, out uint remainder)
        {
            if (dividendHi >= divisor) throw new DivideOverflowException();
            ulong lDividend = Combine(dividendHi, dividendLo);
            ulong lQuotient = lDividend / (ulong)divisor;
            ulong lRemainder = lDividend % (ulong)divisor;
            quotient = (uint)lQuotient;
            remainder = (uint)lRemainder;
        }

        public static void ShiftRight(uint hi, uint lo, uint shiftCount, out uint outLo)
        {
            ulong lValue = Combine(hi, lo);
            outLo = (uint)(lValue >> (int)shiftCount);
        }

        public static void ShiftLeft(uint hi, uint lo, uint shiftCount, out uint outHi)
        {
            ulong lValue = Combine(hi, lo);
            outHi = (uint)(lValue >> (32 - (int)shiftCount));
        }

        public static uint QHat(uint Un, uint Unm1, uint Unm2, uint Vnm1, uint Vnm2)
        {
            // an opcode-for-opcode translation of Knuth's MIX code
            uint qhat;
            uint rhat;
            uint rA;
            uint rX;
            bool carry;

            if (Un >= Vnm1) goto L1;
            Divide(Un, Unm1, Vnm1, out rA, out rX);
            qhat = rA;
            rhat = rX;
            goto L2;
        L1:
            rX = 0xFFFFFFFFu;
            rA = Unm1;
            goto L4;
        L3:
            rX = qhat;
            --rX;
            rA = rhat;
        L4:
            qhat = rX;
            Add(rA, Vnm1, false, out rA, out carry);
            if (carry) goto END;
            rhat = rA;
            rA = qhat;
        L2:
            Multiply(rA, Vnm2, 0u, out rX, out rA);
            if (rA < rhat) goto END;
            if (rA > rhat) goto L3;
            if (rX > Unm2) goto L3;
        END:
            return qhat;
        }

        public static uint NormFactor(uint u)
        {
            if (u == 0) return 0;
            if (u == ~0u) return 1u;
            uint q;
            uint r;
            Divide(1u, 0u, u + 1u, out q, out r);
            return q;
        }

        public static void HighestPower(uint numericBase, out uint power, out uint exponent)
        {
            uint carry = 0;
            uint value = numericBase;
            uint carry2;
            uint value2;
            uint exp2 = 1;
            while (true)
            {
                Multiply(value, numericBase, carry, out value2, out carry2);
                if (carry2 > 0u) break;
                carry = carry2;
                value = value2;
                ++exp2;
            }
            power = value;
            exponent = exp2;
        }

        public static int SignificantBits(uint u)
        {
            if (u == 0u) return -1;
            int lsb = 0;
            if ((u & 0xFFFF0000u) != 0) { lsb += 16; u >>= 16; }
            if ((u & 0xFF00u) != 0) { lsb += 8; u >>= 8; }
            if ((u & 0xF0u) != 0) { lsb += 4; u >>= 4; }
            if ((u & 0xCu) != 0) { lsb += 2; u >>= 2; }
            if ((u & 0x2u) != 0) { lsb += 1; u >>= 1; }

            return lsb;
        }

        public static void AddL(ulong addend1, ulong addend2, bool carryIn, out ulong sum, out bool carryOut)
        {
            uint a1hi;
            uint a1lo;
            Split(addend1, out a1hi, out a1lo);
            uint a2hi;
            uint a2lo;
            Split(addend2, out a2hi, out a2lo);
            bool carry = carryIn;
            uint slo;
            uint shi;
            Add(a1lo, a2lo, carry, out slo, out carry);
            Add(a1hi, a2hi, carry, out shi, out carry);
            sum = Combine(shi, slo);
            carryOut = carry;
        }

        public static void SubtractL(ulong minuend, ulong subtrahend, bool borrowIn, out ulong diff, out bool borrowOut)
        {
            uint minuendHi;
            uint minuendLo;
            Split(minuend, out minuendHi, out minuendLo);
            uint subtrahendHi;
            uint subtrahendLo;
            Split(subtrahend, out subtrahendHi, out subtrahendLo);
            bool borrow = borrowIn;
            uint diffHi;
            uint diffLo;
            Subtract(minuendLo, subtrahendLo, borrow, out diffLo, out borrow);
            Subtract(minuendHi, subtrahendHi, borrow, out diffHi, out borrow);
            diff = Combine(diffHi, diffLo);
            borrowOut = borrow;
        }

        public static void MultiplyL(ulong fac1, ulong fac2, ulong carryIn, out ulong prod, out ulong carryOut)
        {
            uint fac1Hi;
            uint fac1Lo;
            Split(fac1, out fac1Hi, out fac1Lo);
            uint fac2Hi;
            uint fac2Lo;
            Split(fac2, out fac2Hi, out fac2Lo);
            uint r3 = 0u;
            uint r2 = 0u;
            uint r1;
            uint r0;
            Split(carryIn, out r1, out r0);
            uint t2;
            uint t1;
            uint t0;
            Multiply(fac1Lo, fac2Lo, 0u, out t0, out t1);
            Multiply(fac1Hi, fac2Lo, t1, out t1, out t2);
            bool carry = false;
            Add(r0, t0, carry, out r0, out carry);
            Add(r1, t1, carry, out r1, out carry);
            Add(r2, t2, carry, out r2, out carry);
            System.Diagnostics.Debug.Assert(!carry);
            Multiply(fac1Lo, fac2Hi, 0u, out t0, out t1);
            Multiply(fac1Hi, fac2Hi, t1, out t1, out t2);
            carry = false;
            Add(r1, t0, carry, out r1, out carry);
            Add(r2, t1, carry, out r2, out carry);
            Add(r3, t2, carry, out r3, out carry);
            System.Diagnostics.Debug.Assert(!carry);
            prod = Combine(r1, r0);
            carryOut = Combine(r3, r2);
        }

        public static void DivideL(ulong dividendHi, ulong dividendLo, ulong divisor, out ulong quotient, out ulong remainder)
        {
            uint temp2;
            uint temp1;
            uint temp0;
            uint normFactor;
            uint divisor1;
            uint divisor0;
            uint dividend4;
            uint dividend3;
            uint dividend2;
            uint dividend1;
            uint dividend0;
            uint quotient1;
            uint quotient0;
            bool borrow;
            Split(divisor, out divisor1, out divisor0);
            if (divisor1 == 0)
            {
                if (divisor0 == 0)
                {
                    throw new DivideByZeroException();
                }
                else
                {
                    // short division (by divisorLo)
                    Split(dividendHi, out dividend3, out dividend2);
                    Split(dividendLo, out dividend1, out dividend0);
                    if (dividend3 != 0u) throw new DivideOverflowException();
                    if (dividend2 >= divisor0) throw new DivideOverflowException();
                    Divide(dividend2, dividend1, divisor0, out quotient1, out dividend1);
                    Divide(dividend1, dividend0, divisor0, out quotient0, out dividend0);
                    quotient = Combine(quotient1, quotient0);
                    remainder = Combine(0u, dividend0);
                }
            }
            else
            {
                Split(dividendHi, out dividend3, out dividend2);
                Split(dividendLo, out dividend1, out dividend0);
                if (divisor0 == 0)
                {
                    // short division (by divisorHi)
                    if (dividend3 >= divisor1) throw new DivideOverflowException();
                    Divide(dividend3, dividend2, divisor1, out quotient1, out dividend2);
                    Divide(dividend2, dividend1, divisor1, out quotient0, out dividend1);
                    quotient = Combine(quotient1, quotient0);
                    remainder = Combine(dividend1, dividend0);
                }
                else
                {
                    if (dividend3 >= divisor1) throw new DivideOverflowException();
                    normFactor = NormFactor(divisor1);
                    Multiply(divisor0, normFactor, 0u, out divisor0, out temp0);
                    Multiply(divisor1, normFactor, temp0, out divisor1, out temp0);
                    System.Diagnostics.Debug.Assert(temp0 == 0);
                    Multiply(dividend0, normFactor, 0u, out dividend0, out dividend4);
                    Multiply(dividend1, normFactor, dividend4, out dividend1, out dividend4);
                    Multiply(dividend2, normFactor, dividend4, out dividend2, out dividend4);
                    Multiply(dividend3, normFactor, dividend4, out dividend3, out dividend4);

                    System.Diagnostics.Debug.Assert(dividend4 == 0u); // undetected divide overflow?

                    if (dividend3 != 0u)
                    {
                        quotient1 = QHat(dividend3, dividend2, dividend1, divisor1, divisor0);
                        Multiply(divisor0, quotient1, 0u, out temp0, out temp2);
                        Multiply(divisor1, quotient1, temp2, out temp1, out temp2);

                        Subtract(dividend1, temp0, false, out dividend1, out borrow);
                        Subtract(dividend2, temp1, borrow, out dividend2, out borrow);
                        Subtract(dividend3, temp2, borrow, out dividend3, out borrow);
                        if (borrow)
                        {
                            System.Diagnostics.Debug.Assert(dividend3 == 0xFFFFFFFFu);
                            Add(dividend1, divisor0, false, out dividend1, out borrow);
                            Add(dividend2, divisor1, borrow, out dividend2, out borrow);
                            System.Diagnostics.Debug.Assert(borrow);
                            quotient1--;
                        }
                    }
                    else
                    {
                        quotient1 = 0u;
                    }

                    quotient0 = QHat(dividend2, dividend1, dividend0, divisor1, divisor0);
                    Multiply(divisor0, quotient0, 0u, out temp0, out temp2);
                    Multiply(divisor1, quotient0, temp2, out temp1, out temp2);

                    Subtract(dividend0, temp0, false, out dividend0, out borrow);
                    Subtract(dividend1, temp1, borrow, out dividend1, out borrow);
                    Subtract(dividend2, temp2, borrow, out dividend2, out borrow);
                    if (borrow)
                    {
                        System.Diagnostics.Debug.Assert(dividend2 == 0xFFFFFFFFu);
                        Add(dividend0, divisor0, false, out dividend0, out borrow);
                        Add(dividend1, divisor1, borrow, out dividend1, out borrow);
                        System.Diagnostics.Debug.Assert(borrow);
                        quotient0--;
                    }

                    Divide(0u, dividend1, normFactor, out dividend2, out dividend1);
                    Divide(dividend1, dividend0, normFactor, out dividend1, out dividend0);
                    System.Diagnostics.Debug.Assert(dividend0 == 0);
                    quotient = Combine(quotient1, quotient0);
                    remainder = Combine(dividend2, dividend1);
                }
            }
        }

        public static void ShiftRightL(ulong hi, ulong lo, ulong shiftCount, out ulong outLo)
        {
            uint in3;
            uint in2;
            uint in1;
            uint in0;
            uint shhi;
            uint shlo;
            uint out1;
            uint out0;
            if (shiftCount == 0ul)
            {
                outLo = lo;
            }
            else if (shiftCount > 127ul)
            {
                outLo = 0ul;
            }
            else
            {
                Split(hi, out in3, out in2);
                Split(lo, out in1, out in0);
                Split(shiftCount, out shhi, out shlo);
                if (shlo > 95u)
                {
                    out1 = 0u;
                    ShiftRight(0u, in3, shlo & 31, out out0);
                }
                else if (shlo > 63u)
                {
                    ShiftRight(0u, in3, shlo & 31u, out out1);
                    ShiftRight(in3, in2, shlo & 31u, out out0);
                }
                else if (shlo > 31u)
                {
                    ShiftRight(in3, in2, shlo & 31u, out out1);
                    ShiftRight(in2, in1, shlo & 31u, out out0);
                }
                else
                {
                    ShiftRight(in2, in1, shlo, out out1);
                    ShiftRight(in1, in0, shlo, out out0);
                }
                outLo = Combine(out1, out0);
            }
        }

        public static void ShiftLeftL(ulong hi, ulong lo, ulong shiftCount, out ulong outHi)
        {
            uint in3;
            uint in2;
            uint in1;
            uint in0;
            uint shhi;
            uint shlo;
            uint out1;
            uint out0;
            if (shiftCount == 0ul)
            {
                outHi = hi;
            }
            else if (shiftCount > 127ul)
            {
                outHi = 0ul;
            }
            else
            {
                Split(hi, out in3, out in2);
                Split(lo, out in1, out in0);
                Split(shiftCount, out shhi, out shlo);
                if (shlo > 95u)
                {
                    ShiftLeft(in0, 0u, shlo & 31, out out1);
                    out0 = 0u;
                }
                else if (shlo > 63u)
                {
                    ShiftLeft(in1, in0, shlo & 31, out out1);
                    ShiftLeft(in0, 0u, shlo & 31, out out0);
                }
                else if (shlo > 31u)
                {
                    ShiftLeft(in2, in1, shlo & 31, out out1);
                    ShiftLeft(in1, in0, shlo & 31, out out0);
                }
                else
                {
                    ShiftLeft(in3, in2, shlo, out out1);
                    ShiftLeft(in2, in1, shlo, out out0);
                }
                outHi = Combine(out1, out0);
            }
        }

        public static ulong QHatL(ulong Un, ulong Unm1, ulong Unm2, ulong Vnm1, ulong Vnm2)
        {
            // an opcode-for-opcode translation of Knuth's MIX code
            ulong qhat;
            ulong rhat;
            ulong rA;
            ulong rX;
            bool carry;

            if (Un >= Vnm1) goto L1;
            DivideL(Un, Unm1, Vnm1, out rA, out rX);
            qhat = rA;
            rhat = rX;
            goto L2;
        L1:
            rX = 0xFFFFFFFFFFFFFFFFul;
            rA = Unm1;
            goto L4;
        L3:
            rX = qhat;
            --rX;
            rA = rhat;
        L4:
            qhat = rX;
            AddL(rA, Vnm1, false, out rA, out carry);
            if (carry) goto END;
            rhat = rA;
            rA = qhat;
        L2:
            MultiplyL(rA, Vnm2, 0ul, out rX, out rA);
            if (rA < rhat) goto END;
            if (rA > rhat) goto L3;
            if (rX > Unm2) goto L3;
        END:
            return qhat;
        }

        public static ulong NormFactorL(ulong u)
        {
            if (u == 0ul) return 0;
            if (u == ~0ul) return 1ul;
            ulong q;
            ulong r;
            DivideL(1ul, 0ul, u + 1ul, out q, out r);
            return q;
        }

        public static void HighestPowerL(ulong numericBase, out ulong power, out ulong exponent)
        {
            ulong carry = 0;
            ulong value = numericBase;
            ulong carry2;
            ulong value2;
            ulong exp2 = 1;
            while (true)
            {
                MultiplyL(value, numericBase, carry, out value2, out carry2);
                if (carry2 > 0ul) break;
                carry = carry2;
                value = value2;
                ++exp2;
            }
            power = value;
            exponent = exp2;
        }

        public static int SignificantBitsL(ulong u)
        {

            if (u == 0ul) return -1;
            int lsb = 0;
            if ((u & 0xFFFFFFFF00000000ul) != 0) { lsb += 32; u >>= 32; }
            if ((u & 0xFFFF0000ul) != 0) { lsb += 16; u >>= 16; }
            if ((u & 0xFF00ul) != 0) { lsb += 8; u >>= 8; }
            if ((u & 0xF0ul) != 0) { lsb += 4; u >>= 4; }
            if ((u & 0xCul) != 0) { lsb += 2; u >>= 2; }
            if ((u & 0x2ul) != 0) { lsb += 1; u >>= 1; }

            return lsb;
        }
    }

    public interface IHashGenerator
    {
        void Add(byte b);
        void Add(byte[] b);
        void Add(byte[] b, int off, int len);
        void Add(char ch);
        void Add(char[] ch);
        void Add(string s);
        int Hash { get; }
    }

    public interface IHashable
    {
        void AddToHash(IHashGenerator hg);
    }

    public class HashGenerator : IHashGenerator
    {
        private int hash;

        public HashGenerator()
        {
            hash = 0x23F3071B;
        }

        public void Add(byte b)
        {
            hash = unchecked((hash * 729) + b + 1);
        }

        public void Add(byte[] b)
        {
            int iend = b.Length;
            for (int i = 0; i < iend; ++i)
            {
                Add(b[i]);
            }
        }

        public void Add(byte[] b, int off, int len)
        {
            int iEnd = off + len;
            for (int i = off; i < iEnd; ++i)
            {
                Add(b[i]);
            }
        }

        public void Add(char ch)
        {
            Add(BitConverter.GetBytes(ch));
        }

        public void Add(char[] ch)
        {
            int iend = ch.Length;
            for (int i = 0; i < iend; ++i)
            {
                Add(ch[i]);
            }
        }

        public void Add(string s)
        {
            int iend = s.Length;
            for (int i = 0; i < iend; ++i)
            {
                Add(s[i]);
            }
        }

        public int Hash { get { return unchecked(hash * 0x3A5E4215); } }
    }

    public enum OverflowBehavior
    {
        Wraparound,
        Saturate,
        ThrowException
    }

    public enum FloatingOverflowBehavior
    {
        SaturateToInfinity,
        ThrowException
    }

    public enum RoundingMode
    {
        Floor,
        Round,
        Ceiling,
        TruncateTowardZero
    }

    public enum DigitOrder
    {
        HBLA,
        LBLA
    }

    [Serializable]
    public class BigInteger : IHashable
    {
        private uint[] digits;
        private bool isNegative;

        public BigInteger(uint[] digits, bool isNegative)
        {
            this.digits = digits;
            this.isNegative = isNegative;
            System.Diagnostics.Debug.Assert((digits.Length == 0) || (digits[digits.Length - 1] != SignExtend));
        }

        private BigInteger(uint digit0, bool isNegative)
        {
            if (digit0 == GetSignExtend(isNegative))
            {
                this.digits = new uint[0];
                this.isNegative = isNegative;
            }
            else
            {
                this.digits = new uint[1];
                digits[0] = digit0;
                this.isNegative = isNegative;
            }
        }

        private BigInteger(uint digit0, uint digit1, bool isNegative)
        {
            if (digit1 == GetSignExtend(isNegative))
            {
                if (digit0 == GetSignExtend(isNegative))
                {
                    this.digits = new uint[0];
                    this.isNegative = isNegative;
                }
                else
                {
                    this.digits = new uint[1];
                    digits[0] = digit0;
                    this.isNegative = isNegative;
                }
            }
            else
            {
                this.digits = new uint[2];
                digits[0] = digit0;
                digits[1] = digit1;
                this.isNegative = isNegative;
            }
        }

        public bool IsZero { get { return digits.Length == 0 && !isNegative; } }

        public bool IsNegative { get { return isNegative; } }

        public bool IsPositive { get { return !IsZero && !IsNegative; } }

        public bool IsOdd { get { return (digits.Length == 0 && isNegative) || (digits.Length > 0 && ((digits[0] & 1) != 0)); } }

        private uint SignExtend { get { return isNegative ? 0xFFFFFFFFu : 0u; } }

        private static uint GetSignExtend(bool isNegative) { return isNegative ? 0xFFFFFFFFu : 0u; }

        private static BigInteger minusOne = null;
        public static BigInteger MinusOne
        {
            get
            {
                if (object.ReferenceEquals(minusOne, null))
                {
                    lock (typeof(BigInteger))
                    {
                        if (object.ReferenceEquals(minusOne, null))
                        {
                            minusOne = new BigInteger(new uint[0], true);
                        }
                    }
                }
                return minusOne;
            }
        }

        private static BigInteger zero = null;
        public static BigInteger Zero
        {
            get
            {
                if (object.ReferenceEquals(zero, null))
                {
                    lock (typeof(BigInteger))
                    {
                        if (object.ReferenceEquals(zero, null))
                        {
                            zero = new BigInteger(new uint[0], false);
                        }
                    }
                }
                return zero;
            }
        }

        private static BigInteger one = null;
        public static BigInteger One
        {
            get
            {
                if (object.ReferenceEquals(one, null))
                {
                    lock (typeof(BigInteger))
                    {
                        if (object.ReferenceEquals(one, null))
                        {
                            one = new BigInteger(new uint[] { 1u }, false);
                        }
                    }
                }
                return one;
            }
        }

        private static BigInteger two = null;
        public static BigInteger Two
        {
            get
            {
                if (object.ReferenceEquals(two, null))
                {
                    lock (typeof(BigInteger))
                    {
                        if (object.ReferenceEquals(two, null))
                        {
                            two = new BigInteger(new uint[] { 2u }, false);
                        }
                    }
                } 
                return two;
            }
        }

        #region Conversion to Various Integer Types

        public bool FitsInByte
        {
            get
            {
                return (!isNegative) && ((digits.Length == 0) || ((digits.Length == 1) && (digits[0] < 256u)));
            }
        }

        public byte GetByteValue(OverflowBehavior ob)
        {
            if (IsZero) return (byte)0u;
            if (FitsInByte) return unchecked((byte)digits[0]);
            if (ob == OverflowBehavior.Wraparound)
            {
                return unchecked((byte)(digits[0] & 0xFFu));
            }
            else if (ob == OverflowBehavior.Saturate)
            {
                if (isNegative) return (byte)0u;
                return byte.MaxValue;
            }
            else throw new OverflowException();
        }

        public static BigInteger FromByte(byte b)
        {
            if (b == 0) return new BigInteger(new uint[0], false);
            else return new BigInteger((uint)b, false);
        }

        public bool FitsInUInt16
        {
            get
            {
                return (!isNegative) && ((digits.Length == 0) || ((digits.Length == 1) && (digits[0] < 0x10000u)));
            }
        }

        public ushort GetUInt16Value(OverflowBehavior ob)
        {
            if (IsZero) return (ushort)0u;
            if (FitsInUInt16) return unchecked((ushort)digits[0]);
            if (ob == OverflowBehavior.Wraparound)
            {
                return unchecked((ushort)(digits[0] & 0xFFFFu));
            }
            else if (ob == OverflowBehavior.Saturate)
            {
                if (isNegative) return (ushort)0u;
                return ushort.MaxValue;
            }
            else throw new OverflowException();
        }

        public static BigInteger FromUInt16(ushort u)
        {
            return new BigInteger((uint)u, false);
        }

        public bool FitsInUInt32
        {
            get
            {
                return (!isNegative) && (digits.Length <= 1);
            }
        }

        public uint GetUInt32Value(OverflowBehavior ob)
        {
            if (IsZero) return 0u;
            if (FitsInUInt32) return digits[0];
            if (ob == OverflowBehavior.Wraparound)
            {
                return digits[0];
            }
            else if (ob == OverflowBehavior.Saturate)
            {
                if (isNegative) return 0u;
                else return uint.MaxValue;
            }
            else throw new OverflowException();
        }

        public static BigInteger FromUInt32(uint u)
        {
            return new BigInteger(u, false);
        }

        public bool FitsInUIntPtr
        {
            get
            {
                return (IntPtr.Size == 4) ? FitsInUInt32 : FitsInUInt64;
            }
        }

        public UIntPtr GetUIntPtrValue(OverflowBehavior ob)
        {
            if (UIntPtr.Size == 4) return (UIntPtr)(GetUInt32Value(ob));
            else return (UIntPtr)(GetUInt64Value(ob));
        }

        public static BigInteger FromUIntPtr(UIntPtr ip)
        {
            if (UIntPtr.Size == 4) return FromUInt32((uint)ip);
            else return FromUInt64((ulong)ip);
        }

        public bool FitsInUInt64
        {
            get
            {
                return (!isNegative) && (digits.Length <= 2);
            }
        }

        public ulong GetUInt64Value(OverflowBehavior ob)
        {
            if (IsZero) return 0ul;
            if (FitsInUInt64 || ob == OverflowBehavior.Wraparound)
            {
                if (digits.Length == 1)
                {
                    return (ulong)digits[0];
                }
                else
                {
                    return ((ulong)digits[1] << 32) | (ulong)digits[0];
                }
            }
            if (ob == OverflowBehavior.Saturate)
            {
                if (isNegative) return 0ul;
                else return ulong.MaxValue;
            }
            else throw new OverflowException();
        }

        public static BigInteger FromUInt64(ulong u)
        {
            if (u < 0x100000000ul)
            {
                return new BigInteger(unchecked((uint)u), false);
            }
            else
            {
                return new BigInteger(unchecked((uint)u), unchecked((uint)(u >> 32)), false);
            }
        }

        public bool FitsInSByte
        {
            get
            {
                return (isNegative && (digits.Length == 0 || ((digits.Length == 1) && (digits[0] >= 0xFFFFFF80u)))) ||
                    (!isNegative && (digits.Length == 0 || ((digits.Length == 1) && (digits[0] <= 0x7Fu))));
            }
        }

        public sbyte GetSByteValue(OverflowBehavior ob)
        {
            if (isNegative)
            {
                if (digits.Length == 0) return (sbyte)-1;
                if (ob == OverflowBehavior.Wraparound || ((digits.Length == 1) && (digits[0] >= 0xFFFFFF80u)))
                {
                    return unchecked((sbyte)(digits[0] & 0xFFu));
                }
                if (ob == OverflowBehavior.Saturate)
                {
                    return sbyte.MinValue;
                }
                throw new OverflowException();
            }
            else
            {
                if (digits.Length == 0) return (sbyte)0;
                if (ob == OverflowBehavior.Wraparound || ((digits.Length == 1) && (digits[0] <= 0x7Fu)))
                {
                    return unchecked((sbyte)(digits[0] & 0xFFu));
                }
                if (ob == OverflowBehavior.Saturate)
                {
                    return sbyte.MaxValue;
                }
                throw new OverflowException();
            }
        }

        public static BigInteger FromSByte(sbyte s)
        {
            if (s < 0)
            {
                return new BigInteger(unchecked((uint)s), true);
            }
            else
            {
                return new BigInteger(0x7Fu & unchecked((uint)s), false);
            }
        }

        public bool FitsInInt16
        {
            get
            {
                return (isNegative && (digits.Length == 0 || ((digits.Length == 1) && digits[0] >= 0xFFFF8000u))) ||
                    (!isNegative && (digits.Length == 0 || ((digits.Length == 1) && digits[0] <= 0x7FFFu)));
            }
        }

        public short GetInt16Value(OverflowBehavior ob)
        {
            if (isNegative)
            {
                if (digits.Length == 0) return (short)-1;
                if (ob == OverflowBehavior.Wraparound || ((digits.Length == 1) && (digits[0] >= 0xFFFF8000u)))
                {
                    return unchecked((short)(digits[0] & 0xFFFFu));
                }
                if (ob == OverflowBehavior.Saturate)
                {
                    return short.MinValue;
                }
                throw new OverflowException();
            }
            else
            {
                if (digits.Length == 0) return (short)0;
                if (ob == OverflowBehavior.Wraparound || ((digits.Length == 1) && (digits[0] <= 0x7FFFu)))
                {
                    return unchecked((short)(digits[0] & 0xFFFFu));
                }
                if (ob == OverflowBehavior.Saturate)
                {
                    return short.MaxValue;
                }
                throw new OverflowException();
            }
        }

        public static BigInteger FromInt16(short s)
        {
            if (s < 0)
            {
                return new BigInteger(unchecked((uint)s), true);
            }
            else
            {
                return new BigInteger(0x7FFFu & unchecked((uint)s), false);
            }
        }

        public bool FitsInInt32
        {
            get
            {
                return (isNegative && (digits.Length == 0 || ((digits.Length == 1) && digits[0] >= 0x80000000u))) ||
                    (!isNegative && (digits.Length == 0 || ((digits.Length == 1) && digits[0] <= 0x7FFFFFFFu)));
            }
        }

        public int GetInt32Value(OverflowBehavior ob)
        {
            if (isNegative)
            {
                if (digits.Length == 0) return -1;
                if (ob == OverflowBehavior.Wraparound || ((digits.Length == 1) && (digits[0] >= 0x80000000u)))
                {
                    return unchecked((int)(digits[0]));
                }
                if (ob == OverflowBehavior.Saturate)
                {
                    return int.MinValue;
                }
                throw new OverflowException();
            }
            else
            {
                if (digits.Length == 0) return (short)0;
                if (ob == OverflowBehavior.Wraparound || ((digits.Length == 1) && (digits[0] <= 0x7FFFFFFFu)))
                {
                    return unchecked((int)(digits[0]));
                }
                if (ob == OverflowBehavior.Saturate)
                {
                    return int.MaxValue;
                }
                throw new OverflowException();
            }
        }

        public static BigInteger FromInt32(int i)
        {
            return new BigInteger(unchecked((uint)i), i < 0);
        }

        public bool FitsInIntPtr
        {
            get
            {
                return (IntPtr.Size == 4) ? FitsInInt32 : FitsInInt64;
            }
        }

        public IntPtr GetIntPtrValue(OverflowBehavior ob)
        {
            if (IntPtr.Size == 4) return (IntPtr)(GetInt32Value(ob));
            else return (IntPtr)(GetInt64Value(ob));
        }

        public static BigInteger FromIntPtr(IntPtr ip)
        {
            if (IntPtr.Size == 4) return FromInt32((int)ip);
            else return FromInt64((long)ip);
        }

        public bool FitsInInt64
        {
            get
            {
                return (isNegative && (digits.Length < 2 || ((digits.Length == 2) && digits[1] >= 0x80000000u))) ||
                    (!isNegative && (digits.Length < 2 || ((digits.Length == 2) && digits[1] <= 0x7FFFFFFFu)));
            }
        }

        public long GetInt64Value(OverflowBehavior ob)
        {
            if (isNegative)
            {
                if (digits.Length == 0) return -1L;
                if (digits.Length == 1)
                {
                    return unchecked((long)0xFFFFFFFF00000000L | (long)(digits[0]));
                }
                if (ob == OverflowBehavior.Wraparound || ((digits.Length == 2) && (digits[1] >= 0x80000000u)))
                {
                    return unchecked(((long)(digits[1]) << 32) | (long)(digits[0]));
                }
                if (ob == OverflowBehavior.Saturate)
                {
                    return long.MinValue;
                }
                throw new OverflowException();
            }
            else
            {
                if (digits.Length == 0) return 0L;
                if (digits.Length == 1)
                {
                    return unchecked((long)(digits[0]));
                }
                if (ob == OverflowBehavior.Wraparound || ((digits.Length == 2) && (digits[1] <= 0x7FFFFFFFu)))
                {
                    return unchecked(((long)(digits[1]) << 32) | (long)(digits[0]));
                }
                if (ob == OverflowBehavior.Saturate)
                {
                    return long.MaxValue;
                }
                throw new OverflowException();
            }
        }

        public static BigInteger FromInt64(long l)
        {
            return new BigInteger(unchecked((uint)l), unchecked((uint)(l >> 32)), (l < 0));
        }

        [SchemeFunction("byte-size-of-integer")]
        public int MinByteArraySize(bool signed)
        {
            int count;
            uint top;
            uint top1;
            bool signBit;

            if (signed)
            {
                if (digits.Length == 0) return 1;
                count = digits.Length * 4;
                top = digits[digits.Length - 1];
                signBit = ((top & 0x80000000u) != 0);
                if (signBit != isNegative) return count + 1;
                signBit = ((top & 0x800000u) != 0);
                top1 = (top & 0xFFFFFFu);
                if (signBit) top1 |= 0xFF000000u;
                if (top != top1) return count;
                signBit = ((top & 0x8000u) != 0);
                top1 = (top & 0xFFFFu);
                if (signBit) top1 |= 0xFFFF0000u;
                if (top != top1) return count - 1;
                signBit = ((top & 0x80u) != 0);
                top1 = (top & 0xFFu);
                if (signBit) top1 |= 0xFFFFFF00u;
                if (top != top1) return count - 2;
                return count - 3;
            }
            else
            {
                if (digits.Length == 0) return 0;
                count = digits.Length * 4;
                top = digits[digits.Length - 1];
                top1 = top & 0xFFFFFFu;
                if (isNegative) top1 |= 0xFF000000u;
                if (top != top1) return count;
                top1 = top & 0xFFFFu;
                if (isNegative) top1 |= 0xFFFF0000u;
                if (top != top1) return count - 1;
                top1 = top & 0xFFu;
                if (isNegative) top1 |= 0xFFFFFF00u;
                if (top != top1) return count - 2;
                return count - 3;
            }
        }

        private class ByteEnumerator
        {
            private BigInteger parent;

            private int ptr;
            private int ptrEnd;
            private byte current;
            private uint currentDigit;
            private int bytesLeft;

            public ByteEnumerator(BigInteger parent)
            {
                this.parent = parent;
                ptr = 0;
                ptrEnd = parent.digits.Length;
                bytesLeft = 0;
            }

            private void LoadDigit()
            {
                if (ptr < ptrEnd)
                {
                    currentDigit = parent.digits[ptr];
                    ++ptr;
                }
                else
                {
                    currentDigit = parent.SignExtend;
                }
                bytesLeft = 4;
            }

            public bool Next()
            {
                if (bytesLeft == 0) LoadDigit();
                current = (byte)(currentDigit & 0xFFu);
                currentDigit >>= 8;
                --bytesLeft;
                return true;
            }

            public byte Current { get { return current; } }
        }

        private delegate void LoopBody(int i);

        private static void LoopLowToHigh(int offset, int len, DigitOrder ord, LoopBody body)
        {
            if (ord == DigitOrder.LBLA)
            {
                int i = offset;
                int iend = offset + len;
                while (i < iend)
                {
                    body(i);
                    ++i;
                }
            }
            else
            {
                int i = offset + len;
                int iend = offset;
                while (i > iend)
                {
                    --i;
                    body(i);
                }
            }
        }

        private void WriteSpecialValue(byte[] array, int offset, int len, bool negative, bool signed, DigitOrder ord)
        {
            byte fillByte = negative ? (byte)0u : (byte)0xFFu;
            byte topByte = signed ? (byte)(fillByte ^ (byte)0x80u) : fillByte;

            LoopLowToHigh
            (
                offset, len, ord,
                delegate(int outPtr)
                {
                    array[outPtr] = fillByte;
                }
            );

            if (ord == DigitOrder.HBLA)
            {
                array[offset] = topByte;
            }
            else
            {
                array[offset + len - 1] = topByte;
            }
        }

        public void WriteBytesToArray(byte[] array, int offset, int len, bool signed, OverflowBehavior ob, DigitOrder ord)
        {
            if (ob != OverflowBehavior.Wraparound)
            {
                int wantedLen = MinByteArraySize(signed);
                if (wantedLen > len || (isNegative && !signed))
                {
                    if (ob == OverflowBehavior.ThrowException) throw new OverflowException();
                    WriteSpecialValue(array, offset, len, isNegative, signed, ord);
                    return;
                }
            }

            ByteEnumerator bytes = new ByteEnumerator(this);

            bytes.Next();

            LoopLowToHigh
            (
                offset, len, ord,
                delegate(int outPtr)
                {
                    array[outPtr] = bytes.Current;
                    bytes.Next();
                }
            );
        }

        public static BigInteger FromByteArray(byte[] array, int offset, int len, bool signed, DigitOrder ord)
        {
            int lenUInts = (len + 3) / 4;
            List<uint> digits = new List<uint>();
            uint currentUint = 0u;
            int pos = 0;
            bool isNegative = false;

            LoopLowToHigh
            (
                offset, len, ord,
                delegate(int i)
                {
                    byte element = array[i];
                    isNegative = (element & 0x80) != 0;
                    currentUint |= (uint)element << pos;
                    pos += 8;
                    if (pos == 32)
                    {
                        digits.Add(currentUint);
                        currentUint = 0u;
                        pos = 0;
                    }
                }
            );

            if (signed && isNegative) currentUint |= 0xFFFFFFFFu << pos; // sign extend

            if (pos != 0) digits.Add(currentUint);

            RemoveLeadingZeros(digits, GetSignExtend(signed && isNegative));

            return new BigInteger(digits.ToArray(), signed && isNegative);
        }

        public byte[] GetByteArray(DigitOrder ord)
        {
            int bSize = MinByteArraySize(true);
            byte[] b = new byte[bSize];
            WriteBytesToArray(b, 0, bSize, true, OverflowBehavior.Wraparound, ord);
            return b;
        }

        #endregion

        [SchemeFunction("lognot")]
        public static BigInteger operator ~ (BigInteger a)
        {
            int i = 0;
            int iEnd = a.digits.Length;
            uint[] resultDigits = new uint[iEnd];
            while (i < iEnd)
            {
                resultDigits[i] = ~a.digits[i];
                ++i;
            }
            return new BigInteger(resultDigits, !a.isNegative);
        }

        public static BigInteger operator - (BigInteger a)
        {
            bool carry = true;
            int i = 0;
            int iEnd = a.digits.Length;
            List<uint> resultDigits = new List<uint>(iEnd + 1);
            uint nDigit;
            while (i < iEnd)
            {
                MixedPrecision.Add(~a.digits[i], 0u, carry, out nDigit, out carry);
                resultDigits.Add(nDigit);
                ++i;
            }
            MixedPrecision.Add(~a.SignExtend, 0u, carry, out nDigit, out carry);
            if (nDigit != 0u && nDigit != 0xFFFFFFFFu)
            {
                resultDigits.Add(nDigit);
                MixedPrecision.Add(~a.SignExtend, 0u, carry, out nDigit, out carry);
            }
            System.Diagnostics.Debug.Assert(nDigit == 0u || nDigit == 0xFFFFFFFFu);
            RemoveLeadingZeros(resultDigits, nDigit);
            return new BigInteger(resultDigits.ToArray(), nDigit != 0u);
        }

        private static void RemoveLeadingZeros(List<uint> resultDigits, uint signExtend)
        {
            while (resultDigits.Count > 0 && resultDigits[resultDigits.Count - 1] == signExtend) resultDigits.RemoveAt(resultDigits.Count - 1);
        }

        public static BigInteger operator + (BigInteger a, BigInteger b)
        {
            bool carry = false;
            int i = 0;
            int aEnd = a.digits.Length;
            int bEnd = b.digits.Length;
            List<uint> resultDigits = new List<uint>(Math.Max(aEnd, bEnd) + 1);
            uint sDigit;
            while (i < aEnd && i < bEnd)
            {
                MixedPrecision.Add(a.digits[i], b.digits[i], carry, out sDigit, out carry);
                resultDigits.Add(sDigit);
                ++i;
            }
            while (i < aEnd)
            {
                MixedPrecision.Add(a.digits[i], b.SignExtend, carry, out sDigit, out carry);
                resultDigits.Add(sDigit);
                ++i;
            }
            while (i < bEnd)
            {
                MixedPrecision.Add(a.SignExtend, b.digits[i], carry, out sDigit, out carry);
                resultDigits.Add(sDigit);
                ++i;
            }
            MixedPrecision.Add(a.SignExtend, b.SignExtend, carry, out sDigit, out carry);
            if (sDigit != 0u && sDigit != 0xFFFFFFFFu)
            {
                resultDigits.Add(sDigit);
                MixedPrecision.Add(a.SignExtend, b.SignExtend, carry, out sDigit, out carry);
            }
            System.Diagnostics.Debug.Assert(sDigit == 0u || sDigit == 0xFFFFFFFFu);
            RemoveLeadingZeros(resultDigits, sDigit);
            return new BigInteger(resultDigits.ToArray(), sDigit != 0u);
        }

        public static BigInteger operator - (BigInteger a, BigInteger b)
        {
            bool borrow = false;
            int i = 0;
            int aEnd = a.digits.Length;
            int bEnd = b.digits.Length;
            List<uint> resultDigits = new List<uint>(Math.Max(aEnd, bEnd) + 1);
            uint dDigit;
            while (i < aEnd && i < bEnd)
            {
                MixedPrecision.Subtract(a.digits[i], b.digits[i], borrow, out dDigit, out borrow);
                resultDigits.Add(dDigit);
                ++i;
            }
            while (i < aEnd)
            {
                MixedPrecision.Subtract(a.digits[i], b.SignExtend, borrow, out dDigit, out borrow);
                resultDigits.Add(dDigit);
                ++i;
            }
            while (i < bEnd)
            {
                MixedPrecision.Subtract(a.SignExtend, b.digits[i], borrow, out dDigit, out borrow);
                resultDigits.Add(dDigit);
                ++i;
            }
            MixedPrecision.Subtract(a.SignExtend, b.SignExtend, borrow, out dDigit, out borrow);
            if (dDigit != 0u && dDigit != 0xFFFFFFFFu)
            {
                resultDigits.Add(dDigit);
                MixedPrecision.Subtract(a.SignExtend, b.SignExtend, borrow, out dDigit, out borrow);
            }
            System.Diagnostics.Debug.Assert(dDigit == 0u || dDigit == 0xFFFFFFFFu);
            RemoveLeadingZeros(resultDigits, dDigit);
            return new BigInteger(resultDigits.ToArray(), dDigit != 0u);
        }

        public static BigInteger operator & (BigInteger a, BigInteger b)
        {
            int i = 0;
            int aEnd = a.digits.Length;
            int bEnd = b.digits.Length;
            List<uint> resultDigits = new List<uint>(Math.Max(aEnd, bEnd));
            while (i < aEnd && i < bEnd)
            {
                resultDigits.Add(a.digits[i] & b.digits[i]);
                ++i;
            }
            while (i < aEnd)
            {
                resultDigits.Add(a.digits[i] & b.SignExtend);
                ++i;
            }
            while (i < bEnd)
            {
                resultDigits.Add(a.SignExtend & b.digits[i]);
                ++i;
            }
            uint finalSign = a.SignExtend & b.SignExtend;
            RemoveLeadingZeros(resultDigits, finalSign);
            return new BigInteger(resultDigits.ToArray(), finalSign != 0u);
        }

        public static BigInteger operator | (BigInteger a, BigInteger b)
        {
            int i = 0;
            int aEnd = a.digits.Length;
            int bEnd = b.digits.Length;
            List<uint> resultDigits = new List<uint>(Math.Max(aEnd, bEnd));
            while (i < aEnd && i < bEnd)
            {
                resultDigits.Add(a.digits[i] | b.digits[i]);
                ++i;
            }
            while (i < aEnd)
            {
                resultDigits.Add(a.digits[i] | b.SignExtend);
                ++i;
            }
            while (i < bEnd)
            {
                resultDigits.Add(a.SignExtend | b.digits[i]);
                ++i;
            }
            uint finalSign = a.SignExtend | b.SignExtend;
            RemoveLeadingZeros(resultDigits, finalSign);
            return new BigInteger(resultDigits.ToArray(), finalSign != 0u);
        }

        public static BigInteger operator ^ (BigInteger a, BigInteger b)
        {
            int i = 0;
            int aEnd = a.digits.Length;
            int bEnd = b.digits.Length;
            List<uint> resultDigits = new List<uint>(Math.Max(aEnd, bEnd));
            while (i < aEnd && i < bEnd)
            {
                resultDigits.Add(a.digits[i] ^ b.digits[i]);
                ++i;
            }
            while (i < aEnd)
            {
                resultDigits.Add(a.digits[i] ^ b.SignExtend);
                ++i;
            }
            while (i < bEnd)
            {
                resultDigits.Add(a.SignExtend ^ b.digits[i]);
                ++i;
            }
            uint finalSign = a.SignExtend ^ b.SignExtend;
            RemoveLeadingZeros(resultDigits, finalSign);
            return new BigInteger(resultDigits.ToArray(), finalSign != 0u);
        }

        public static explicit operator double(BigInteger a)
        {
            return (double)((BigRational)a);
        }

        public static explicit operator float(BigInteger a)
        {
            return (float)((BigRational)a);
        }

        public static BigInteger ShiftLeft(BigInteger a, uint b, bool ones)
        {
            uint shiftInBits = ones ? 0xFFFFFFFFu : 0u;

            List<uint> rDigits = new List<uint>();
            rDigits.Capacity = a.digits.Length + (int)((uint)(b >> 5)) + 1;
            while (b >= 32)
            {
                rDigits.Add(shiftInBits);
                b -= 32;
            }
            if (b == 0)
            {
                int pos = 0;
                int posend = a.digits.Length;
                while (pos < posend)
                {
                    rDigits.Add(a.digits[pos]);
                    ++pos;
                }
            }
            else
            {
                uint shiftLo = shiftInBits;
                int pos = 0;
                int posend = a.digits.Length;
                uint result;
                while (pos < posend)
                {
                    uint shiftHi = a.digits[pos];
                    MixedPrecision.ShiftLeft(shiftHi, shiftLo, b, out result);
                    rDigits.Add(result);
                    shiftLo = shiftHi;
                    ++pos;
                }
                MixedPrecision.ShiftLeft(0u, shiftLo, b, out result);
                rDigits.Add(result);
            }
            RemoveLeadingZeros(rDigits, a.SignExtend);
            return new BigInteger(rDigits.ToArray(), a.isNegative);
        }

        [SchemeFunction("shl")]
        public static BigInteger operator << (BigInteger a, int b)
        {
            if (b == -0x80000000) throw new ArgumentException();
            if (b < 0) return a >> -b;
            return ShiftLeft(a, (uint)b, false);
        }

        [SchemeFunction("shl1")]
        public static BigInteger Shl1(BigInteger a, int b)
        {
            if (b == -0x80000000) throw new ArgumentException();
            if (b < 0) return Shr1(a, -b);
            return ShiftLeft(a, (uint)b, true);
        }

        public static BigInteger ShiftRight(BigInteger a, uint b)
        {
            List<uint> rDigits = new List<uint>();
            int proposedCapacity = a.digits.Length - (int)((uint)(b >> 5)) + 1;
            if (proposedCapacity < 1) proposedCapacity = 1;
            rDigits.Capacity = proposedCapacity;

            int pos = 0;
            int posEnd = a.digits.Length;
            while (b >= 32 && pos < posEnd)
            {
                ++pos;
                b -= 32;
            }
            if (pos == posEnd) return a.isNegative ? BigInteger.MinusOne : BigInteger.Zero;
            if (b == 0)
            {
                while (pos < posEnd)
                {
                    rDigits.Add(a.digits[pos]);
                    ++pos;
                }
            }
            else
            {
                uint dLow = a.digits[pos];
                ++pos;
                uint result;
                while (pos < posEnd)
                {
                    uint dHigh = a.digits[pos];
                    MixedPrecision.ShiftRight(dHigh, dLow, b, out result);
                    rDigits.Add(result);
                    dLow = dHigh;
                    ++pos;
                }
                MixedPrecision.ShiftRight(a.SignExtend, dLow, b, out result);
                rDigits.Add(result);
            }
            RemoveLeadingZeros(rDigits, a.SignExtend);
            return new BigInteger(rDigits.ToArray(), a.isNegative);
        }

        [SchemeFunction("shr")]
        public static BigInteger operator >>(BigInteger a, int b)
        {
            if (b == -0x80000000) throw new ArgumentException();
            if (b < 0) return a << -b;
            return ShiftRight(a, (uint)b);
        }

        [SchemeFunction("shr1")]
        public static BigInteger Shr1(BigInteger a, int b)
        {
            if (b == -0x80000000) throw new ArgumentException();
            if (b < 0) return Shl1(a, -b);
            return ShiftRight(a, (uint)b);
        }

        public static BigInteger operator * (BigInteger a, uint b)
        {
            bool resultNegative = a.IsNegative;
            if (a.IsNegative) a = -a;
            int i = 0;
            int aEnd = a.digits.Length;
            uint carry = 0u;
            uint pDigit;
            List<uint> resultDigits = new List<uint>(aEnd + 1);
            while (i < aEnd)
            {
                MixedPrecision.Multiply(a.digits[i], b, carry, out pDigit, out carry);
                resultDigits.Add(pDigit);
                ++i;
            }
            if (carry != 0u) resultDigits.Add(carry);
            RemoveLeadingZeros(resultDigits, 0u);
            BigInteger r = new BigInteger(resultDigits.ToArray(), false);
            return (resultNegative) ? -r : r;
        }

        public static BigInteger operator * (uint a, BigInteger b)
        {
            bool resultNegative = b.IsNegative;
            if (b.IsNegative) b = -b;
            int i = 0;
            int bEnd = b.digits.Length;
            uint carry = 0u;
            uint pDigit;
            List<uint> resultDigits = new List<uint>(bEnd + 1);
            while (i < bEnd)
            {
                MixedPrecision.Multiply(a, b.digits[i], carry, out pDigit, out carry);
                resultDigits.Add(pDigit);
                ++i;
            }
            if (carry != 0u) resultDigits.Add(carry);
            RemoveLeadingZeros(resultDigits, 0u);
            BigInteger r = new BigInteger(resultDigits.ToArray(), false);
            return resultNegative ? -r : r;
        }

        public static BigInteger operator * (BigInteger a, BigInteger b)
        {
            bool resultNegative = a.IsNegative ^ b.IsNegative;
            if (a.IsNegative) a = -a;
            if (b.IsNegative) b = -b;
            int i = 0;
            int j = 0;
            int aEnd = a.digits.Length;
            int bEnd = b.digits.Length;
            int rEnd = aEnd + bEnd;
            uint[] resultDigits = new uint[rEnd];
            while (i < aEnd)
            {
                uint mulCarry = 0u;
                uint pDigit = 0u;
                bool addCarry = false;
                uint sDigit = 0u;
                j = 0;
                while (j < bEnd)
                {
                    MixedPrecision.Multiply(a.digits[i], b.digits[j], mulCarry, out pDigit, out mulCarry);
                    MixedPrecision.Add(resultDigits[i + j], pDigit, addCarry, out sDigit, out addCarry);
                    resultDigits[i + j] = sDigit;
                    ++j;
                }
                MixedPrecision.Add(resultDigits[i + j], mulCarry, addCarry, out sDigit, out addCarry);
                resultDigits[i + j] = sDigit;
                ++j;
                while (addCarry && (j < (rEnd - i)))
                {
                    MixedPrecision.Add(resultDigits[i + j], 0u, addCarry, out sDigit, out addCarry);
                    ++j;
                }
                System.Diagnostics.Debug.Assert(!addCarry);
                ++i;
            }
            while (rEnd > 0 && resultDigits[rEnd - 1] == 0u) --rEnd;
            if (rEnd < (aEnd + bEnd))
            {
                uint[] rd2 = new uint[rEnd];
                Array.Copy(resultDigits, 0, rd2, 0, rEnd);
                resultDigits = rd2;
            }
            BigInteger r = new BigInteger(resultDigits, false);
            return resultNegative ? -r : r;
        }

        public static void DivModFloored(BigInteger dividend, uint divisor, out BigInteger quotient, out uint remainder)
        {
            bool remainderNegative = dividend.IsNegative;
            if (dividend.IsNegative) dividend = -dividend;
            int dividendEnd = dividend.digits.Length;
            int rEnd = dividendEnd;
            uint[] resultDigits = new uint[rEnd];
            int i = dividendEnd;
            uint highPart = 0u;
            uint qDigit;
            while (i > 0)
            {
                --i;
                MixedPrecision.Divide(highPart, dividend.digits[i], divisor, out qDigit, out highPart);
                resultDigits[i] = qDigit;
            }
            while (rEnd > 0 && resultDigits[rEnd - 1] == 0u) --rEnd;
            if (rEnd < dividendEnd)
            {
                uint[] rd2 = new uint[rEnd];
                Array.Copy(resultDigits, 0, rd2, 0, rEnd);
                resultDigits = rd2;
            }
            BigInteger q = new BigInteger(resultDigits, dividend.IsNegative);
            if (remainderNegative)
            {
                if (highPart == 0u)
                {
                    q = -q;
                }
                else
                {
                    q = ~q;
                    highPart = divisor - highPart;
                }
            }
            quotient = q;
            remainder = highPart;
        }

        public static void DivModSymmetric(BigInteger dividend, BigInteger divisor, out BigInteger quotient, out BigInteger scaledRemainder, out uint normFactor)
        {
            bool dividendNegative = dividend.IsNegative;
            if (dividendNegative) dividend = -dividend;

            bool divisorNegative = divisor.IsNegative;
            if (divisorNegative) divisor = -divisor;

            if (divisor.IsZero) throw new DivideByZeroException();

            if (divisor.digits.Length == 1)
            {
                BigInteger q1;
                uint r1;
                DivModFloored(dividend, divisor.digits[0], out q1, out r1);
             
                BigInteger r2 = new BigInteger(r1, false);
                
                if (divisorNegative != dividendNegative) q1 = -q1;
                if (dividendNegative) r2 = -r2;

                quotient = q1;
                scaledRemainder = r2;
                normFactor = 1u;
                return;
            }

            normFactor = MixedPrecision.NormFactor(divisor.digits[divisor.digits.Length - 1]);

            divisor = divisor * normFactor;
            dividend = dividend * normFactor;

            int dividendLength = Math.Max(dividend.digits.Length, divisor.digits.Length) + 1;
            int divisorLength = 1 + divisor.digits.Length;
            int shift = dividendLength - divisorLength;
            int topEnd = dividendLength - 1;
            int divisorTopEnd = divisor.digits.Length - 1;

            uint[] dividendReg = new uint[dividendLength];
            Array.Copy(dividend.digits, 0, dividendReg, 0, dividend.digits.Length);

            uint[] qDigits = new uint[shift + 1];

            while (shift >= 0)
            {
                uint qhat = MixedPrecision.QHat
                (
                    dividendReg[topEnd], dividendReg[topEnd - 1], dividendReg[topEnd - 2],
                    divisor.digits[divisorTopEnd], (divisorTopEnd == 0) ? 0u : divisor.digits[divisorTopEnd - 1]
                );

                int pos = 0;
                int posEnd = divisor.digits.Length;
                int posPlusShift = pos + shift;

                uint mulCarry = 0u;
                uint mulResult;
                bool subBorrow = false;
                uint subResult;
                while (pos < posEnd)
                {
                    MixedPrecision.Multiply(divisor.digits[pos], qhat, mulCarry, out mulResult, out mulCarry);
                    MixedPrecision.Subtract(dividendReg[posPlusShift], mulResult, subBorrow, out subResult, out subBorrow);
                    dividendReg[posPlusShift] = subResult;

                    ++pos;
                    ++posPlusShift;
                }
                MixedPrecision.Subtract(dividendReg[pos + shift], mulCarry, subBorrow, out subResult, out subBorrow);
                dividendReg[posPlusShift] = subResult;

                if (subBorrow)
                {
                    pos = 0;
                    posPlusShift = pos + shift;

                    bool addCarry = false;
                    uint addResult;
                    while (pos < posEnd)
                    {
                        MixedPrecision.Add(divisor.digits[pos], dividendReg[posPlusShift], addCarry, out addResult, out addCarry);
                        dividendReg[posPlusShift] = addResult;
                        ++pos;
                        ++posPlusShift;
                    }
                    MixedPrecision.Add(0u, dividendReg[posPlusShift], addCarry, out addResult, out addCarry);
                    dividendReg[posPlusShift] = addResult;
                    System.Diagnostics.Debug.Assert(addCarry); // if this assertion fails, qhat was two too big; we could go round again...

                    --qhat;
                }

                qDigits[shift] = qhat;

                --shift;
                --topEnd;
            }

            int qEnd = qDigits.Length;
            while (qEnd > 0 && qDigits[qEnd - 1] == 0) --qEnd;
            if (qEnd != qDigits.Length)
            {
                uint[] qDigits2 = new uint[qEnd];
                Array.Copy(qDigits, 0, qDigits2, 0, qEnd);
                qDigits = qDigits2;
            }

            int rEnd = dividendReg.Length;
            while (rEnd > 0 && dividendReg[rEnd - 1] == 0) --rEnd;
            if (rEnd != dividendReg.Length)
            {
                uint[] rdigits = new uint[rEnd];
                Array.Copy(dividendReg, 0, rdigits, 0, rEnd);
                dividendReg = rdigits;
            }
            else
            {
                System.Diagnostics.Debug.Assert(false); // surprised
            }

            BigInteger q = new BigInteger(qDigits, false);
            BigInteger r = new BigInteger(dividendReg, false);

            if (divisorNegative != dividendNegative) q = -q;
            if (dividendNegative) r = -r;

            quotient = q;
            scaledRemainder = r;
        }

        [SchemeFunction("quotient")]
        public static BigInteger operator / (BigInteger dividend, BigInteger divisor)
        {
            BigInteger quotient;
            BigInteger remainder;
            uint normFactor;

            DivModSymmetric(dividend, divisor, out quotient, out remainder, out normFactor);
            return quotient;
        }

        [SchemeFunction("remainder")]
        public static BigInteger operator % (BigInteger dividend, BigInteger divisor)
        {
            BigInteger quotient;
            BigInteger remainder;
            uint normFactor;

            DivModSymmetric(dividend, divisor, out quotient, out remainder, out normFactor);
            if (remainder.IsNegative && normFactor > 1)
            {
                BigInteger r = -remainder;
                BigInteger q2;
                uint r2;
                DivModFloored(r, normFactor, out q2, out r2);
                System.Diagnostics.Debug.Assert(r2 == 0);
                System.Diagnostics.Debug.Assert(!(q2.digits.Length == 1 && q2.digits[0] == 0u));
                remainder = -q2;
            }
            else if (normFactor > 1u)
            {
                BigInteger q3;
                uint r3;
                DivModFloored(remainder, normFactor, out q3, out r3);
                System.Diagnostics.Debug.Assert(r3 == 0);
                System.Diagnostics.Debug.Assert(!(q3.digits.Length == 1 && q3.digits[0] == 0u));
                remainder = q3;
            }
            return remainder;
        }

        [SchemeFunction("modulo")]
        public static BigInteger Modulo(BigInteger dividend, BigInteger divisor)
        {
            BigInteger quotient;
            BigInteger remainder;
            uint normFactor;

            DivModSymmetric(dividend, divisor, out quotient, out remainder, out normFactor);

            if (normFactor > 1u)
            {
                BigInteger r2;
                uint r3;
                DivModFloored(remainder, normFactor, out r2, out r3);
                System.Diagnostics.Debug.Assert(r3 == 0);
                System.Diagnostics.Debug.Assert(!(r2.digits.Length == 1 && r2.digits[0] == 0u));
                remainder = r2;
            }

            if (dividend.IsNegative != divisor.IsNegative)
            {
                remainder += divisor;
            }

            return remainder;
        }

        public static BigInteger Gcd(BigInteger a, BigInteger b)
        {
            bool isNegative = a.IsNegative | b.IsNegative;
            if (a.IsNegative) a = -a;
            if (b.IsNegative) b = -b;
            if (a.IsZero) return b;
            if (b.IsZero) return a;
            if (a < b) { BigInteger temp = a; a = b; b = temp; }

            while (true)
            {
                BigInteger r = a % b;
                if (r.IsZero) return b;
                a = b; b = r;
            }
        }

        public static BigInteger Lcm(BigInteger a, BigInteger b)
        {
            return a * b / Gcd(a, b);
        }

        public static BigInteger Pow(BigInteger @base, uint exponent)
        {
            BigInteger result = BigInteger.One;
            BigInteger fromBit = @base;
            while (exponent != 0)
            {
                if ((exponent & 1u) != 0)
                {
                    result *= fromBit;
                }
                fromBit = fromBit * fromBit;
                exponent >>= 1;
            }
            return result;
        }

        [SchemeFunction("lsb")]
        public int SignificantBits()
        {
            if (digits.Length == 0) return -1;
            uint mdigit = digits[digits.Length - 1];
            if (isNegative) mdigit = ~mdigit;
            return ((digits.Length - 1) << 5) + MixedPrecision.SignificantBits(mdigit);
        }

        public static bool operator == (BigInteger a, BigInteger b)
        {
            if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null)) return true;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null)) return false;
            return (a - b).IsZero;
        }

        public static bool operator != (BigInteger a, BigInteger b)
        {
            if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null)) return false;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null)) return true;
            return !((a - b).IsZero);
        }

        public static bool operator < (BigInteger a, BigInteger b)
        {
            if (a.IsNegative && !(b.IsNegative)) return true;
            if (b.IsNegative && !(a.IsNegative)) return false;
            return (a - b).IsNegative;
        }

        public static bool operator > (BigInteger a, BigInteger b)
        {
            if (a.IsNegative && !(b.IsNegative)) return false;
            if (b.IsNegative && !(a.IsNegative)) return true;
            return (b - a).IsNegative;
        }

        public static bool operator <= (BigInteger a, BigInteger b)
        {
            if (a.IsNegative && !(b.IsNegative)) return true;
            if (b.IsNegative && !(a.IsNegative)) return false;
            return !((b - a).IsNegative);
        }

        public static bool operator >= (BigInteger a, BigInteger b)
        {
            if (a.IsNegative && !(b.IsNegative)) return false;
            if (b.IsNegative && !(a.IsNegative)) return true;
            return !((a - b).IsNegative);
        }

        public static BigInteger Min(BigInteger a, BigInteger b)
        {
            return (a < b) ? a : b;
        }

        public static BigInteger Max(BigInteger a, BigInteger b)
        {
            return (a > b) ? a : b;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is BigInteger)) return false;
            return this == (BigInteger)obj;
        }

        public override int GetHashCode()
        {
            HashGenerator hg = new HashGenerator();
            AddToHash(hg);
            return hg.Hash;
        }

        public void AddToHash(IHashGenerator hg)
        {
            int iend = digits.Length;
            for (int i = 0; i < iend; ++i)
            {
                hg.Add(BitConverter.GetBytes(digits[i]));
            }
            hg.Add(isNegative ? (byte)1 : (byte)0);
        }

        private static char[] stringDigits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        private static double logDigitSize = Math.Log(2.0) * 32.0;

        [SchemeFunction("bigint->string")]
        public static string ToString(BigInteger b, uint @base)
        {
            System.Diagnostics.Debug.Assert(@base >= 2 && @base <= 36);
            if (b.IsZero) return "0";
            bool wasNegative = b.IsNegative;
            if (b.IsNegative) b = -b;
            uint power;
            uint exponent;
            MixedPrecision.HighestPower(@base, out power, out exponent);
            double dDigitsRequired = (b.digits.Length * logDigitSize) / Math.Log(@base);
            int iDigitsRequired = (int)((Math.Ceiling(dDigitsRequired / (double)exponent)) * (double)exponent);
            ++iDigitsRequired;
            char[] ch = new char[iDigitsRequired + 1];
            int chPtr = iDigitsRequired + 1;
            while (!b.IsZero)
            {
                uint remainder;
                BigInteger b2;
                DivModFloored(b, power, out b2, out remainder);
                int j = 0;
                while (j < exponent)
                {
                    System.Diagnostics.Debug.Assert(chPtr > 0);
                    --chPtr;
                    ch[chPtr] = stringDigits[remainder % @base];
                    remainder /= @base;
                    ++j;
                }
                b = b2;
            }
            while (chPtr < (iDigitsRequired + 1) && ch[chPtr] == '0') ++chPtr;
            if (wasNegative)
            {
                --chPtr;
                ch[chPtr] = '-';
            }
            return new string(ch, chPtr, (iDigitsRequired + 1) - chPtr);
        }

        [SchemeFunction("string->bigint")]
        public static BigInteger Parse(string str, uint @base)
        {
            System.Diagnostics.Debug.Assert(@base >= 2 && @base <= 36);
            uint power;
            uint exponent;
            MixedPrecision.HighestPower(@base, out power, out exponent);
            uint accum = 0u;
            uint accumPower = 1u;
            BigInteger b = BigInteger.Zero;
            int strPos = 0;
            int strEnd = str.Length;
            bool isNegative = false;
            if (str[strPos] == '-')
            {
                ++strPos;
                isNegative = true;
            }
            while (strPos < strEnd)
            {
                char ch = str[strPos];
                uint digit;
                if (ch >= '0' && ch <= '9') digit = (uint)(ch - '0');
                else if (ch >= 'A' && ch <= 'Z') digit = (uint)(ch - 'A' + 10);
                else if (ch >= 'a' && ch <= 'z') digit = (uint)(ch - 'a' + 10);
                else digit = 0u;
                accum = (accum * @base) + digit;
                accumPower *= @base;
                ++strPos;

                if (accumPower == power || strPos == strEnd)
                {
                    b = (b * accumPower) + new BigInteger(accum, false);
                    accum = 0u;
                    accumPower = 1u;
                }
            }
            if (isNegative) b = -b;
            return b;
        }

        public string DebugView { get { return ToString(this, 10) + " (0x" + ToString(this, 16) + ")"; } }
    }

    [Serializable]
    public class BigRational : IHashable
    {
        private BigInteger numerator;
        private BigInteger denominator;

        [SchemeFunction("make-rational")]
        public BigRational(BigInteger numerator, BigInteger denominator)
        {
            if (denominator.IsZero) throw new DivideByZeroException("Denominator of a rational number cannot be zero");

            if (denominator.IsNegative)
            {
                numerator = -numerator;
                denominator = -denominator;
            }

            BigInteger gcd = BigInteger.Gcd(numerator, denominator);

            if (gcd != BigInteger.One)
            {
                numerator = numerator / gcd;
                denominator = denominator / gcd;
            }

            this.numerator = numerator;
            this.denominator = denominator;
        }

        public bool IsNegative { get { return numerator.IsNegative; } }
        public bool IsZero { get { return numerator.IsZero; } }

        public BigInteger Numerator { [SchemeFunction("numerator")] get { return numerator; } }
        public BigInteger Denominator { [SchemeFunction("denominator")] get { return denominator; } }

        public static BigRational operator +(BigRational a, BigRational b)
        {
            return new BigRational(a.numerator * b.denominator + b.numerator * a.denominator, a.denominator * b.denominator);
        }

        public static BigRational operator -(BigRational a)
        {
            return new BigRational(-a.numerator, a.denominator);
        }

        public static BigRational operator -(BigRational a, BigRational b)
        {
            return new BigRational(a.numerator * b.denominator - b.numerator * a.denominator, a.denominator * b.denominator);
        }

        public static BigRational operator *(BigRational a, BigRational b)
        {
            return new BigRational(a.numerator * b.numerator, a.denominator * b.denominator);
        }

        public static BigRational operator /(BigRational a, BigRational b)
        {
            return new BigRational(a.numerator * b.denominator, a.denominator * b.numerator);
        }

        public static BigRational operator +(BigRational a, BigInteger b)
        {
            return new BigRational(a.numerator + b * a.denominator, a.denominator);
        }

        public static BigRational operator +(BigInteger a, BigRational b)
        {
            return new BigRational(a * b.denominator + b.numerator, b.denominator);
        }

        public static BigRational operator -(BigRational a, BigInteger b)
        {
            return new BigRational(a.numerator - b * a.denominator, a.denominator);
        }

        public static BigRational operator -(BigInteger a, BigRational b)
        {
            return new BigRational(a * b.denominator - b.numerator, b.denominator);
        }

        public static BigRational operator *(BigRational a, BigInteger b)
        {
            return new BigRational(a.numerator * b, a.denominator);
        }
        
        public static BigRational operator *(BigInteger a, BigRational b)
        {
            return new BigRational(a * b.numerator, b.denominator);
        }

        public static BigRational operator /(BigRational a, BigInteger b)
        {
            return new BigRational(a.numerator, a.denominator * b);
        }

        public static BigRational operator /(BigInteger a, BigRational b)
        {
            return new BigRational(a * b.denominator, b.numerator);
        }

        public static explicit operator BigRational(BigInteger i)
        {
            return new BigRational(i, BigInteger.One);
        }

        public static explicit operator double(BigRational a)
        {
            return a.GetDoubleValue(RoundingMode.Round);
        }

        public static explicit operator float(BigRational a)
        {
            return a.GetSingleValue(RoundingMode.Round);
        }

        public BigInteger Floor()
        {
            BigInteger b = numerator / denominator;
            if (numerator < BigInteger.Zero) b = b - BigInteger.One;
            return b;
        }

        public BigInteger Round()
        {
            BigRational r = this + new BigRational(BigInteger.One, BigInteger.FromInt32(2));
            if (r.Denominator == BigInteger.One)
            {
                if (r.Numerator.IsOdd)
                {
                    return r.Numerator - BigInteger.One;
                }
                else return r.Numerator;
            }
            else return r.Floor();
        }

        public BigInteger Ceiling()
        {
            BigInteger b = numerator / denominator;
            if (numerator >= BigInteger.Zero) b = b + BigInteger.One;
            return b;
        }

        public BigInteger TruncateTowardZero()
        {
            BigInteger b = numerator / denominator;
            return b;
        }

        public BigInteger RoundingOp(RoundingMode m)
        {
            switch (m)
            {
                case RoundingMode.Ceiling: return Ceiling();
                case RoundingMode.Floor: return Floor();
                case RoundingMode.Round: return Round();
                case RoundingMode.TruncateTowardZero: return TruncateTowardZero();
                default: goto case RoundingMode.Round;
            }
        }

        public static bool operator <(BigRational a, BigRational b)
        {
            return a.numerator * b.denominator < b.numerator * a.denominator;
        }

        public static bool operator >(BigRational a, BigRational b)
        {
            return a.numerator * b.denominator > b.numerator * a.denominator;
        }

        public static bool operator <=(BigRational a, BigRational b)
        {
            return a.numerator * b.denominator <= b.numerator * a.denominator;
        }

        public static bool operator >=(BigRational a, BigRational b)
        {
            return a.numerator * b.denominator >= b.numerator * a.denominator;
        }

        public static bool operator ==(BigRational a, BigRational b)
        {
            if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null)) return true;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null)) return false;
            return (a.numerator == b.numerator) && (a.denominator == b.denominator);
        }

        public static bool operator !=(BigRational a, BigRational b)
        {
            if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null)) return false;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null)) return true;
            return (a.numerator != b.numerator) || (a.denominator != b.denominator);
        }

        public override bool Equals(object obj)
        {
            return (obj is BigRational) && (this == (BigRational)obj);
        }

        public override int GetHashCode()
        {
            HashGenerator hg = new HashGenerator();
            AddToHash(hg);
            return hg.Hash;
        }

        public void AddToHash(IHashGenerator hg)
        {
            hg.Add((byte)37);
            numerator.AddToHash(hg);
            hg.Add((byte)37);
            denominator.AddToHash(hg);
            hg.Add((byte)37);
        }

        public static BigRational Min(BigRational a, BigRational b)
        {
            return (a < b) ? a : b;
        }

        public static BigRational Max(BigRational a, BigRational b)
        {
            return (a > b) ? a : b;
        }

        public BigRational Reciprocal()
        {
            return new BigRational(denominator, numerator);
        }

        public static BigRational Gcd(BigRational a, BigRational b)
        {
            return new BigRational
            (
                BigInteger.Gcd(a.Numerator * b.Denominator, b.Numerator * a.Denominator),
                a.Denominator * b.Denominator
            );
        }

        public static BigRational Lcm(BigRational a, BigRational b)
        {
            return new BigRational
            (
                BigInteger.Lcm(a.Numerator * b.Denominator, b.Numerator * a.Denominator),
                a.Denominator * b.Denominator
            );
        }

        public static BigRational Pow(BigRational @base, int expt)
        {
            if (expt < 0) return Pow(@base.Reciprocal(), -expt);
            return new BigRational(BigInteger.Pow(@base.Numerator, (uint)expt), BigInteger.Pow(@base.Denominator, (uint)expt));
        }

        private static BigRational zero = null;
        private static void InitZero() { if (zero == null) lock (typeof(BigRational)) { if (zero == null) { zero = new BigRational(BigInteger.Zero, BigInteger.One); } } }
        public static BigRational Zero { get { InitZero(); return zero; } }

        private static BigRational one = null;
        private static void InitOne() { if (one == null) lock (typeof(BigRational)) { if (one == null) { one = new BigRational(BigInteger.One, BigInteger.One); } } }
        public static BigRational One { get { InitOne(); return one; } }

        private static BigRational two = null;
        private static void InitTwo() { if (two == null) lock (typeof(BigRational)) { if (two == null) { two = new BigRational(BigInteger.Two, BigInteger.One); } } }
        public static BigRational Two { get { InitTwo(); return two; } }

        private static BigRational oneHalf = null;
        private static void InitOneHalf() { if (oneHalf == null) lock (typeof(BigRational)) { if (oneHalf == null) { oneHalf = new BigRational(BigInteger.One, BigInteger.Two); } } }
        public static BigRational OneHalf { get { InitOneHalf(); return oneHalf; } }

        private static BigRational minusOne = null;
        private static void InitMinusOne() { if (minusOne == null) lock (typeof(BigRational)) { if (minusOne == null) { minusOne = new BigRational(BigInteger.MinusOne, BigInteger.One); } } }
        public static BigRational MinusOne { get { InitMinusOne(); return minusOne; } }

        public static Tuple<BigRational, int> Normalize(BigRational r)
        {
            if (r.IsNegative)
            {
                Tuple<BigRational, int> result = Normalize(-r);
                return new Tuple<BigRational, int>(-result.Item1, result.Item2);
            }

            Stack<BigRational> powers = new Stack<BigRational>();
            Stack<int> exponents = new Stack<int>();

            BigRational currentPower = null;
            int currentExponent = 0;

            int finalExponent = 0;

            if (r < BigRational.One)
            {
                currentPower = BigRational.OneHalf;
                currentExponent = -1;

                while (r < currentPower)
                {
                    powers.Push(currentPower);
                    exponents.Push(currentExponent);
                    currentPower *= currentPower;
                    currentExponent *= 2;
                }

                while (powers.Count > 0)
                {
                    currentPower = powers.Pop();
                    currentExponent = exponents.Pop();
                    if (r < currentPower)
                    {
                        r /= currentPower;
                        finalExponent += currentExponent;
                    }
                }
            }
            else
            {
                currentPower = BigRational.Two;
                currentExponent = 1;

                while (r > currentPower)
                {
                    powers.Push(currentPower);
                    exponents.Push(currentExponent);
                    currentPower *= currentPower;
                    currentExponent *= 2;
                }

                while (powers.Count > 0)
                {
                    currentPower = powers.Pop();
                    currentExponent = exponents.Pop();
                    if (r > currentPower)
                    {
                        r /= currentPower;
                        finalExponent += currentExponent;
                    }
                }
            }

            while (r >= BigRational.Two)
            {
                r /= BigRational.Two;
                finalExponent += 1;
            }

            while (r < BigRational.One)
            {
                r *= BigRational.Two;
                finalExponent -= 1;
            }

            return new Tuple<BigRational, int>(r, finalExponent);
        }

        [SchemeFunction("normalize")]
        public static object Normalize1(object obj)
        {
            BigRational target = null;

            if (obj is BigInteger)
            {
                target = new BigRational((BigInteger)obj, BigInteger.One);
            }
            else if (obj is BigRational)
            {
                target = (BigRational)obj;
            }
            else throw new SchemeRuntimeException("normalize expects an integer or rational");

            Tuple<BigRational, int> result;

            if (target.IsZero) result = new Tuple<BigRational, int>(BigRational.Zero, 0);

            else result = Normalize(target);

            object r1 = SpecialValue.EMPTY_LIST;
            r1 = new ConsCell(BigInteger.FromInt32(result.Item2), r1);
            r1 = new ConsCell((result.Item1.Denominator == BigInteger.One) ? ((object)result.Item1.Numerator) : ((object)result.Item1), r1);
            return r1;
        }

        private static BigRational doubleFractionScale = new BigRational(BigInteger.FromInt64(0x10000000000000), BigInteger.One);

        public double GetDoubleValue(RoundingMode m)
        {
            if (this.IsZero) return 0.0;
            Tuple<BigRational, int> normalized = Normalize(this);
            BigRational frac = normalized.Item1;
            int expt = normalized.Item2;

            expt += 1023;
            int loops = 53;
            while (expt < 0 && loops > 0)
            {
                frac /= BigRational.Two;
                expt += 1;
                loops -= 1;
            }

            if (expt <= 0) { expt = 0; frac /= BigRational.Two; }

            if (expt > 2046) return (frac < BigRational.Zero) ? double.NegativeInfinity : double.PositiveInfinity;

            long bits = (frac * doubleFractionScale).RoundingOp(m).GetInt64Value(OverflowBehavior.Saturate);
            if (bits < 0) bits = (-bits) | unchecked((long)0x8000000000000000L);
            bits &= unchecked((long)0x800FFFFFFFFFFFFFL);
            bits |= (long)expt << 52;

            return BitConverter.Int64BitsToDouble(bits);
        }

        private static BigRational singleFractionScale = new BigRational(BigInteger.FromInt32(0x800000), BigInteger.One);

        public float GetSingleValue(RoundingMode m)
        {
            if (this.IsZero) return 0.0f;
            Tuple<BigRational, int> normalized = Normalize(this);
            BigRational frac = normalized.Item1;
            int expt = normalized.Item2;

            expt += 127;
            int loops = 24;
            while (expt < 0 && loops > 0)
            {
                frac /= BigRational.Two;
                expt += 1;
                loops -= 1;
            }

            if (expt <= 0) { expt = 0; frac /= BigRational.Two; }

            if (expt > 254) return (frac < BigRational.Zero) ? float.NegativeInfinity : float.PositiveInfinity;

            int bits = (frac * singleFractionScale).RoundingOp(m).GetInt32Value(OverflowBehavior.Saturate);
            if (bits < 0) bits = (-bits) | unchecked((int)0x80000000L);
            bits &= unchecked((int)0x807FFFFFL);
            bits |= expt << 23;

            return BitConverter.ToSingle(BitConverter.GetBytes(bits), 0);
        }

        private static void DecomposeSingle(float f, out bool isNegative, out int exponent, out int fraction)
        {
            int i = BitConverter.ToInt32(BitConverter.GetBytes(f), 0);
            fraction = i & 0x7FFFFF;
            exponent = ((i >> 23) & 0xFF) - 127;
            if (exponent == 128) throw new SchemeRuntimeException("Float not representable as a rational");
            if (exponent == -127) { exponent = -126; } else { fraction |= 0x800000; }
            isNegative = ((i >> 31) != 0);
        }

        private static void DecomposeDouble(double d, out bool isNegative, out long exponent, out long fraction)
        {
            long l = BitConverter.DoubleToInt64Bits(d);
            fraction = l & 0xFFFFFFFFFFFFFL;
            exponent = ((l >> 52) & 0x7FFL) - 1023;
            if (exponent == 1024) throw new SchemeRuntimeException("Double not representable as a rational");
            if (exponent == -1023) { exponent = -1022; } else { fraction |= 0x10000000000000L; }
            isNegative = ((l >> 63) != 0);
        }

        public static BigRational GetRationalValue(float f)
        {
            bool isNegative;
            int exponent;
            int fraction;
            DecomposeSingle(f, out isNegative, out exponent, out fraction);
            return Pow(BigRational.Two, exponent - 23) * new BigRational(BigInteger.FromInt32(fraction), BigInteger.One);
        }

        public static BigRational GetRationalValue(double d)
        {
            bool isNegative;
            long exponent;
            long fraction;
            DecomposeDouble(d, out isNegative, out exponent, out fraction);
            return Pow(BigRational.Two, unchecked((int)(exponent - 52))) * new BigRational(BigInteger.FromInt64(fraction), BigInteger.One);
        }

        public string DebugView
        {
            get
            {
                return BigInteger.ToString(numerator, 10) + "/" + BigInteger.ToString(denominator, 10) +
                    " (0x" + BigInteger.ToString(numerator, 16) + "/0x" + BigInteger.ToString(denominator, 16) + ")";
            }
        }
    }
}
