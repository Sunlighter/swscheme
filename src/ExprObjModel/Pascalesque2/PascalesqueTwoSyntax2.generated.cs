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
using System.Reflection;
using System.Linq;

using Symbol = ExprObjModel.Symbol;
using DescendantsWithPatterns = ExprObjModel.DescendantsWithPatternsAttribute;
using Pattern = ExprObjModel.PatternAttribute;
using Bind = ExprObjModel.BindAttribute;

namespace Pascalesque.Two.Syntax
{
    
    [Pattern("(byte $b)")]
    public class LiteralByteSyntax : ExprSyntax
    {
        [Bind("$b")]
        public byte b;

        public override IExpression2 GetExpr()
        {
 	        return new LiteralExpr2(b);
        }
    }

    [Pattern("byte")]
    public class ByteTypeSyntax : TypeSyntax
    {
        public override TypeReference GetTheType()
        {
 	        return ExistingTypeReference.Byte;
        }
    }
    
    [Pattern("(short $s)")]
    public class LiteralInt16Syntax : ExprSyntax
    {
        [Bind("$s")]
        public short s;

        public override IExpression2 GetExpr()
        {
 	        return new LiteralExpr2(s);
        }
    }

    [Pattern("short")]
    public class Int16TypeSyntax : TypeSyntax
    {
        public override TypeReference GetTheType()
        {
 	        return ExistingTypeReference.Int16;
        }
    }
    
    [Pattern("(int $i)")]
    public class LiteralInt32Syntax : ExprSyntax
    {
        [Bind("$i")]
        public int i;

        public override IExpression2 GetExpr()
        {
 	        return new LiteralExpr2(i);
        }
    }

    [Pattern("int")]
    public class Int32TypeSyntax : TypeSyntax
    {
        public override TypeReference GetTheType()
        {
 	        return ExistingTypeReference.Int32;
        }
    }
    
    [Pattern("(long $l)")]
    public class LiteralInt64Syntax : ExprSyntax
    {
        [Bind("$l")]
        public long l;

        public override IExpression2 GetExpr()
        {
 	        return new LiteralExpr2(l);
        }
    }

    [Pattern("long")]
    public class Int64TypeSyntax : TypeSyntax
    {
        public override TypeReference GetTheType()
        {
 	        return ExistingTypeReference.Int64;
        }
    }
    
    [Pattern("(sbyte $sb)")]
    public class LiteralSByteSyntax : ExprSyntax
    {
        [Bind("$sb")]
        public sbyte sb;

        public override IExpression2 GetExpr()
        {
 	        return new LiteralExpr2(sb);
        }
    }

    [Pattern("sbyte")]
    public class SByteTypeSyntax : TypeSyntax
    {
        public override TypeReference GetTheType()
        {
 	        return ExistingTypeReference.SByte;
        }
    }
    
    [Pattern("(ushort $us)")]
    public class LiteralUInt16Syntax : ExprSyntax
    {
        [Bind("$us")]
        public ushort us;

        public override IExpression2 GetExpr()
        {
 	        return new LiteralExpr2(us);
        }
    }

    [Pattern("ushort")]
    public class UInt16TypeSyntax : TypeSyntax
    {
        public override TypeReference GetTheType()
        {
 	        return ExistingTypeReference.UInt16;
        }
    }
    
    [Pattern("(uint $ui)")]
    public class LiteralUInt32Syntax : ExprSyntax
    {
        [Bind("$ui")]
        public uint ui;

        public override IExpression2 GetExpr()
        {
 	        return new LiteralExpr2(ui);
        }
    }

    [Pattern("uint")]
    public class UInt32TypeSyntax : TypeSyntax
    {
        public override TypeReference GetTheType()
        {
 	        return ExistingTypeReference.UInt32;
        }
    }
    
    [Pattern("(ulong $ul)")]
    public class LiteralUInt64Syntax : ExprSyntax
    {
        [Bind("$ul")]
        public ulong ul;

        public override IExpression2 GetExpr()
        {
 	        return new LiteralExpr2(ul);
        }
    }

    [Pattern("ulong")]
    public class UInt64TypeSyntax : TypeSyntax
    {
        public override TypeReference GetTheType()
        {
 	        return ExistingTypeReference.UInt64;
        }
    }
    
    [Pattern("(double $d)")]
    public class LiteralDoubleSyntax : ExprSyntax
    {
        [Bind("$d")]
        public double d;

        public override IExpression2 GetExpr()
        {
 	        return new LiteralExpr2(d);
        }
    }

    [Pattern("double")]
    public class DoubleTypeSyntax : TypeSyntax
    {
        public override TypeReference GetTheType()
        {
 	        return ExistingTypeReference.Double;
        }
    }
    
    [Pattern("(float $f)")]
    public class LiteralSingleSyntax : ExprSyntax
    {
        [Bind("$f")]
        public float f;

        public override IExpression2 GetExpr()
        {
 	        return new LiteralExpr2(f);
        }
    }

    [Pattern("float")]
    public class SingleTypeSyntax : TypeSyntax
    {
        public override TypeReference GetTheType()
        {
 	        return ExistingTypeReference.Single;
        }
    }
    
    [Pattern("(+ $a $b)")]
    public class AddSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax arg1;

        [Bind("$b")]
        public ExprSyntax arg2;

        public override IExpression2 GetExpr()
        {
            return new BinaryOpExpr2(BinaryOp.Add, arg1.GetExpr(), arg2.GetExpr());
        }
    }
    
    [Pattern("(- $a $b)")]
    public class SubSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax arg1;

        [Bind("$b")]
        public ExprSyntax arg2;

        public override IExpression2 GetExpr()
        {
            return new BinaryOpExpr2(BinaryOp.Sub, arg1.GetExpr(), arg2.GetExpr());
        }
    }
    
    [Pattern("(* $a $b)")]
    public class MulSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax arg1;

        [Bind("$b")]
        public ExprSyntax arg2;

        public override IExpression2 GetExpr()
        {
            return new BinaryOpExpr2(BinaryOp.Mul, arg1.GetExpr(), arg2.GetExpr());
        }
    }
    
    [Pattern("(/ $a $b)")]
    public class DivSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax arg1;

        [Bind("$b")]
        public ExprSyntax arg2;

        public override IExpression2 GetExpr()
        {
            return new BinaryOpExpr2(BinaryOp.Div, arg1.GetExpr(), arg2.GetExpr());
        }
    }
    
    [Pattern("(% $a $b)")]
    public class RemSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax arg1;

        [Bind("$b")]
        public ExprSyntax arg2;

        public override IExpression2 GetExpr()
        {
            return new BinaryOpExpr2(BinaryOp.Rem, arg1.GetExpr(), arg2.GetExpr());
        }
    }
    
    [Pattern("(logand $a $b)")]
    public class LogAndSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax arg1;

        [Bind("$b")]
        public ExprSyntax arg2;

        public override IExpression2 GetExpr()
        {
            return new BinaryOpExpr2(BinaryOp.And, arg1.GetExpr(), arg2.GetExpr());
        }
    }
    
    [Pattern("(logior $a $b)")]
    public class LogOrSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax arg1;

        [Bind("$b")]
        public ExprSyntax arg2;

        public override IExpression2 GetExpr()
        {
            return new BinaryOpExpr2(BinaryOp.Or, arg1.GetExpr(), arg2.GetExpr());
        }
    }
    
    [Pattern("(logxor $a $b)")]
    public class LogXorSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax arg1;

        [Bind("$b")]
        public ExprSyntax arg2;

        public override IExpression2 GetExpr()
        {
            return new BinaryOpExpr2(BinaryOp.Xor, arg1.GetExpr(), arg2.GetExpr());
        }
    }
    
    [Pattern("(shl $a $b)")]
    public class ShlSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax arg1;

        [Bind("$b")]
        public ExprSyntax arg2;

        public override IExpression2 GetExpr()
        {
            return new BinaryOpExpr2(BinaryOp.Shl, arg1.GetExpr(), arg2.GetExpr());
        }
    }
    
    [Pattern("(shr $a $b)")]
    public class ShrSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax arg1;

        [Bind("$b")]
        public ExprSyntax arg2;

        public override IExpression2 GetExpr()
        {
            return new BinaryOpExpr2(BinaryOp.Shr, arg1.GetExpr(), arg2.GetExpr());
        }
    }
    
    [Pattern("(atan2 $a $b)")]
    public class Atan2Syntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax arg1;

        [Bind("$b")]
        public ExprSyntax arg2;

        public override IExpression2 GetExpr()
        {
            return new BinaryOpExpr2(BinaryOp.Atan2, arg1.GetExpr(), arg2.GetExpr());
        }
    }
    
    [Pattern("(ieeeremainder $a $b)")]
    public class IEEERemainderSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax arg1;

        [Bind("$b")]
        public ExprSyntax arg2;

        public override IExpression2 GetExpr()
        {
            return new BinaryOpExpr2(BinaryOp.IEEERemainder, arg1.GetExpr(), arg2.GetExpr());
        }
    }
    
    [Pattern("(logbase $a $b)")]
    public class LogBaseSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax arg1;

        [Bind("$b")]
        public ExprSyntax arg2;

        public override IExpression2 GetExpr()
        {
            return new BinaryOpExpr2(BinaryOp.LogBase, arg1.GetExpr(), arg2.GetExpr());
        }
    }
    
    [Pattern("(max $a $b)")]
    public class MaxSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax arg1;

        [Bind("$b")]
        public ExprSyntax arg2;

        public override IExpression2 GetExpr()
        {
            return new BinaryOpExpr2(BinaryOp.Max, arg1.GetExpr(), arg2.GetExpr());
        }
    }
    
    [Pattern("(min $a $b)")]
    public class MinSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax arg1;

        [Bind("$b")]
        public ExprSyntax arg2;

        public override IExpression2 GetExpr()
        {
            return new BinaryOpExpr2(BinaryOp.Min, arg1.GetExpr(), arg2.GetExpr());
        }
    }
    
    [Pattern("(pow $a $b)")]
    public class PowSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax arg1;

        [Bind("$b")]
        public ExprSyntax arg2;

        public override IExpression2 GetExpr()
        {
            return new BinaryOpExpr2(BinaryOp.Pow, arg1.GetExpr(), arg2.GetExpr());
        }
    }
    
    [Pattern("(invert $a)")]
    public class InvertSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new UnaryOpExpr2(UnaryOp.Invert, expr.GetExpr());
        }
    }
    
    [Pattern("(negate $a)")]
    public class NegateSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new UnaryOpExpr2(UnaryOp.Negate, expr.GetExpr());
        }
    }
    
    [Pattern("(not $a)")]
    public class NotSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new UnaryOpExpr2(UnaryOp.Not, expr.GetExpr());
        }
    }
    
    [Pattern("(abs $a)")]
    public class AbsSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new UnaryOpExpr2(UnaryOp.Abs, expr.GetExpr());
        }
    }
    
    [Pattern("(acos $a)")]
    public class AcosSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new UnaryOpExpr2(UnaryOp.Acos, expr.GetExpr());
        }
    }
    
    [Pattern("(asin $a)")]
    public class AsinSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new UnaryOpExpr2(UnaryOp.Asin, expr.GetExpr());
        }
    }
    
    [Pattern("(atan $a)")]
    public class AtanSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new UnaryOpExpr2(UnaryOp.Atan, expr.GetExpr());
        }
    }
    
    [Pattern("(ceil $a)")]
    public class CeilSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new UnaryOpExpr2(UnaryOp.Ceil, expr.GetExpr());
        }
    }
    
    [Pattern("(cos $a)")]
    public class CosSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new UnaryOpExpr2(UnaryOp.Cos, expr.GetExpr());
        }
    }
    
    [Pattern("(cosh $a)")]
    public class CoshSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new UnaryOpExpr2(UnaryOp.Cosh, expr.GetExpr());
        }
    }
    
    [Pattern("(exp $a)")]
    public class ExpSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new UnaryOpExpr2(UnaryOp.Exp, expr.GetExpr());
        }
    }
    
    [Pattern("(floor $a)")]
    public class FloorSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new UnaryOpExpr2(UnaryOp.Floor, expr.GetExpr());
        }
    }
    
    [Pattern("(log $a)")]
    public class LogSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new UnaryOpExpr2(UnaryOp.Log, expr.GetExpr());
        }
    }
    
    [Pattern("(log10 $a)")]
    public class Log10Syntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new UnaryOpExpr2(UnaryOp.Log10, expr.GetExpr());
        }
    }
    
    [Pattern("(round $a)")]
    public class RoundSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new UnaryOpExpr2(UnaryOp.Round, expr.GetExpr());
        }
    }
    
    [Pattern("(sign $a)")]
    public class SignSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new UnaryOpExpr2(UnaryOp.Sign, expr.GetExpr());
        }
    }
    
    [Pattern("(sin $a)")]
    public class SinSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new UnaryOpExpr2(UnaryOp.Sin, expr.GetExpr());
        }
    }
    
    [Pattern("(sinh $a)")]
    public class SinhSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new UnaryOpExpr2(UnaryOp.Sinh, expr.GetExpr());
        }
    }
    
    [Pattern("(sqrt $a)")]
    public class SqrtSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new UnaryOpExpr2(UnaryOp.Sqrt, expr.GetExpr());
        }
    }
    
    [Pattern("(tan $a)")]
    public class TanSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new UnaryOpExpr2(UnaryOp.Tan, expr.GetExpr());
        }
    }
    
    [Pattern("(tanh $a)")]
    public class TanhSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new UnaryOpExpr2(UnaryOp.Tanh, expr.GetExpr());
        }
    }
    
    [Pattern("(trunc $a)")]
    public class TruncSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new UnaryOpExpr2(UnaryOp.Trunc, expr.GetExpr());
        }
    }
    
    [Pattern("(< $a $b)")]
    public class LessThanSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax a;

        [Bind("$b")]
        public ExprSyntax b;

        public override IExpression2 GetExpr()
        {
            return new ComparisonExpr2(Comparison.LessThan, a.GetExpr(), b.GetExpr());
        }
    }
    
    [Pattern("(<= $a $b)")]
    public class LessEqualSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax a;

        [Bind("$b")]
        public ExprSyntax b;

        public override IExpression2 GetExpr()
        {
            return new ComparisonExpr2(Comparison.LessEqual, a.GetExpr(), b.GetExpr());
        }
    }
    
    [Pattern("(> $a $b)")]
    public class GreaterThanSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax a;

        [Bind("$b")]
        public ExprSyntax b;

        public override IExpression2 GetExpr()
        {
            return new ComparisonExpr2(Comparison.GreaterThan, a.GetExpr(), b.GetExpr());
        }
    }
    
    [Pattern("(>= $a $b)")]
    public class GreaterEqualSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax a;

        [Bind("$b")]
        public ExprSyntax b;

        public override IExpression2 GetExpr()
        {
            return new ComparisonExpr2(Comparison.GreaterEqual, a.GetExpr(), b.GetExpr());
        }
    }
    
    [Pattern("(= $a $b)")]
    public class EqualSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax a;

        [Bind("$b")]
        public ExprSyntax b;

        public override IExpression2 GetExpr()
        {
            return new ComparisonExpr2(Comparison.Equal, a.GetExpr(), b.GetExpr());
        }
    }
    
    [Pattern("(<> $a $b)")]
    public class NotEqualSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax a;

        [Bind("$b")]
        public ExprSyntax b;

        public override IExpression2 GetExpr()
        {
            return new ComparisonExpr2(Comparison.NotEqual, a.GetExpr(), b.GetExpr());
        }
    }
    
    [Pattern("(to-byte $a)")]
    public class ConvertToByteSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new ConvertExpr2(ConvertTo.Byte, expr.GetExpr());
        }
    }

    [Pattern("(as-byte $a)")]
    public class RegardAsByteSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new RegardAsExpr2(ConvertTo.Byte, expr.GetExpr());
        }
    }
    
    [Pattern("(to-short $a)")]
    public class ConvertToInt16Syntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new ConvertExpr2(ConvertTo.Short, expr.GetExpr());
        }
    }

    [Pattern("(as-short $a)")]
    public class RegardAsInt16Syntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new RegardAsExpr2(ConvertTo.Short, expr.GetExpr());
        }
    }
    
    [Pattern("(to-int $a)")]
    public class ConvertToInt32Syntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new ConvertExpr2(ConvertTo.Int, expr.GetExpr());
        }
    }

    [Pattern("(as-int $a)")]
    public class RegardAsInt32Syntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new RegardAsExpr2(ConvertTo.Int, expr.GetExpr());
        }
    }
    
    [Pattern("(to-long $a)")]
    public class ConvertToInt64Syntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new ConvertExpr2(ConvertTo.Long, expr.GetExpr());
        }
    }

    [Pattern("(as-long $a)")]
    public class RegardAsInt64Syntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new RegardAsExpr2(ConvertTo.Long, expr.GetExpr());
        }
    }
    
    [Pattern("(to-sbyte $a)")]
    public class ConvertToSByteSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new ConvertExpr2(ConvertTo.SByte, expr.GetExpr());
        }
    }

    [Pattern("(as-sbyte $a)")]
    public class RegardAsSByteSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new RegardAsExpr2(ConvertTo.SByte, expr.GetExpr());
        }
    }
    
    [Pattern("(to-ushort $a)")]
    public class ConvertToUInt16Syntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new ConvertExpr2(ConvertTo.UShort, expr.GetExpr());
        }
    }

    [Pattern("(as-ushort $a)")]
    public class RegardAsUInt16Syntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new RegardAsExpr2(ConvertTo.UShort, expr.GetExpr());
        }
    }
    
    [Pattern("(to-uint $a)")]
    public class ConvertToUInt32Syntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new ConvertExpr2(ConvertTo.UInt, expr.GetExpr());
        }
    }

    [Pattern("(as-uint $a)")]
    public class RegardAsUInt32Syntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new RegardAsExpr2(ConvertTo.UInt, expr.GetExpr());
        }
    }
    
    [Pattern("(to-ulong $a)")]
    public class ConvertToUInt64Syntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new ConvertExpr2(ConvertTo.ULong, expr.GetExpr());
        }
    }

    [Pattern("(as-ulong $a)")]
    public class RegardAsUInt64Syntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new RegardAsExpr2(ConvertTo.ULong, expr.GetExpr());
        }
    }
    
    [Pattern("(to-intptr $a)")]
    public class ConvertToIntPtrSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new ConvertExpr2(ConvertTo.IntPtr, expr.GetExpr());
        }
    }

    [Pattern("(as-intptr $a)")]
    public class RegardAsIntPtrSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new RegardAsExpr2(ConvertTo.IntPtr, expr.GetExpr());
        }
    }
    
    [Pattern("(to-uintptr $a)")]
    public class ConvertToUIntPtrSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new ConvertExpr2(ConvertTo.UIntPtr, expr.GetExpr());
        }
    }

    [Pattern("(as-uintptr $a)")]
    public class RegardAsUIntPtrSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new RegardAsExpr2(ConvertTo.UIntPtr, expr.GetExpr());
        }
    }
    
    [Pattern("(to-float $a)")]
    public class ConvertToSingleSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new ConvertExpr2(ConvertTo.Float, expr.GetExpr());
        }
    }

    [Pattern("(as-float $a)")]
    public class RegardAsSingleSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new RegardAsExpr2(ConvertTo.Float, expr.GetExpr());
        }
    }
    
    [Pattern("(to-double $a)")]
    public class ConvertToDoubleSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new ConvertExpr2(ConvertTo.Double, expr.GetExpr());
        }
    }

    [Pattern("(as-double $a)")]
    public class RegardAsDoubleSyntax : ExprSyntax
    {
        [Bind("$a")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new RegardAsExpr2(ConvertTo.Double, expr.GetExpr());
        }
    }
    
}
