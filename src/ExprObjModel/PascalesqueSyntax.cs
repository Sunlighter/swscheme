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

namespace Pascalesque.One.Syntax
{
    [DescendantsWithPatterns]
    public abstract class ExprSyntax
    {
        public abstract IExpression GetExpr();
    }

    [Pattern("#t")]
    public class LiteralBooleanTrueSyntax : ExprSyntax
    {
        public override IExpression GetExpr()
        {
            return new LiteralExpr(true);
        }
    }

    [Pattern("#f")]
    public class LiteralBooleanFalseSyntax : ExprSyntax
    {
        public override IExpression GetExpr()
        {
            return new LiteralExpr(false);
        }
    }

    [Pattern("(pi)")]
    public class LiteralPiSyntax : ExprSyntax
    {
        public override IExpression GetExpr()
        {
            return new LiteralExpr(Math.PI);
        }
    }

    [Pattern("(e)")]
    public class LiteralESyntax : ExprSyntax
    {
        public override IExpression GetExpr()
        {
            return new LiteralExpr(Math.E);
        }
    }

    [Pattern("$ch")]
    public class LiteralCharSyntax : ExprSyntax
    {
        [Bind("$ch")]
        public char ch;

        public override IExpression GetExpr()
        {
            return new LiteralExpr(ch);
        }
    }

    [Pattern("$str")]
    public class LiteralStringSyntax : ExprSyntax
    {
        [Bind("$str")]
        public string str;

        public override IExpression GetExpr()
        {
            return new LiteralExpr(str);
        }
    }

    [Pattern("$var")]
    public class VarRefSyntax : ExprSyntax
    {
        [Bind("$var")]
        public Symbol v;

        public override IExpression GetExpr()
        {
            return new VarRefExpr(v);
        }
    }

    [Pattern("(set! $v $val)")]
    public class VarSetSyntax : ExprSyntax
    {
        [Bind("$v")]
        public Symbol v;

        [Bind("$val")]
        public ExprSyntax val;

        public override IExpression GetExpr()
        {
            return new VarSetExpr(v, val.GetExpr());
        }
    }

    [Pattern("(begin . $exprs)")]
    public class BeginSyntax : ExprSyntax
    {
        [Bind("$exprs")]
        public List<ExprSyntax> exprs;

        public override IExpression GetExpr()
        {
            return BeginExpr.FromList(exprs.Select(x => x.GetExpr()));
        }
    }

    [Pattern("(nop)")]
    public class VoidSyntax : ExprSyntax
    {
        public override IExpression GetExpr()
        {
            return new EmptyExpr();
        }
    }

    [Pattern("(if $test $then $else)")]
    public class IfThenElseSyntax : ExprSyntax
    {
        [Bind("$test")]
        public ExprSyntax test;

        [Bind("$then")]
        public ExprSyntax aThen;

        [Bind("$else")]
        public ExprSyntax aElse;

        public override IExpression GetExpr()
        {
            return new IfThenElseExpr(test.GetExpr(), aThen.GetExpr(), aElse.GetExpr());
        }
    }

    [Pattern("(while $pre $test $post)")]
    public class BeginWhileRepeatSyntax : ExprSyntax
    {
        [Bind("$pre")]
        public ExprSyntax pre;

        [Bind("$test")]
        public ExprSyntax test;

        [Bind("$post")]
        public ExprSyntax post;

        public override IExpression GetExpr()
        {
            return new BeginWhileRepeatExpr(pre.GetExpr(), test.GetExpr(), post.GetExpr());
        }
    }

    [DescendantsWithPatterns]
    public abstract class TypeSyntax
    {
        public abstract Type GetTheType();
    }

    [Pattern("bool")]
    public class BoolTypeSyntax : TypeSyntax
    {
        public override Type GetTheType()
        {
 	        return typeof(bool);
        }
    }

    [Pattern("intptr")]
    public class IntPtrTypeSyntax : TypeSyntax
    {
        public override Type GetTheType()
        {
 	        return typeof(IntPtr);
        }
    }

    [Pattern("uintptr")]
    public class UIntPtrTypeSyntax : TypeSyntax
    {
        public override Type GetTheType()
        {
 	        return typeof(UIntPtr);
        }
    }

    [Pattern("void")]
    public class VoidTypeSyntax : TypeSyntax
    {
        public override Type GetTheType()
        {
            return typeof(void);
        }
    }

    [Pattern("bitmapdata")]
    public class BitmapDataTypeSyntax : TypeSyntax
    {
        public override Type GetTheType()
        {
            return typeof(System.Drawing.Imaging.BitmapData);
        }
    }

    [Pattern("byterect")]
    public class ByteRectangleTypeSyntax : TypeSyntax
    {
        public override Type GetTheType()
        {
            return typeof(ExprObjModel.Procedures.ByteRectangle);
        }
    }

    [Pattern("sba")]
    public class SbaTypeSyntax : TypeSyntax
    {
        public override Type GetTheType()
        {
            return typeof(ExprObjModel.Procedures.SchemeByteArray);
        }
    }

    [Pattern("char")]
    public class CharTypeSyntax : TypeSyntax
    {
        public override Type GetTheType()
        {
            return typeof(char);
        }
    }

    [Pattern("object")]
    public class ObjectTypeSyntax : TypeSyntax
    {
        public override Type GetTheType()
        {
            return typeof(object);
        }
    }

    [Pattern("string")]
    public class StringTypeSyntax : TypeSyntax
    {
        public override Type GetTheType()
        {
            return typeof(string);
        }
    }

    [Pattern("exception")]
    public class ExceptionTypeSyntax : TypeSyntax
    {
        public override Type GetTheType()
        {
            return typeof(Exception);
        }
    }

    [Pattern("idisposable")]
    public class DisposableTypeSyntax : TypeSyntax
    {
        public override Type GetTheType()
        {
            return typeof(IDisposable);
        }
    }

    [Pattern("bitmap")]
    public class BitmapTypeSyntax : TypeSyntax
    {
        public override Type GetTheType()
        {
            return typeof(System.Drawing.Bitmap);
        }
    }

    [Pattern("(type-named $name . $types)")]
    public class TypeNamedSyntax : TypeSyntax
    {
        [Bind("$name")]
        public string name;

        [Bind("$types")]
        public List<TypeSyntax> types;

        public override Type GetTheType()
        {
            Type t1 = Type.GetType(name, true);
            if (t1.IsGenericTypeDefinition)
            {
                return t1.MakeGenericType(types.Select(x => x.GetTheType()).ToArray());
            }
            else if (types.Count > 0)
            {
                throw new PascalesqueException("The type \"" + name + "\" doesn't take any type parameters");
            }
            else
            {
                return Type.GetType(name, true);
            }
        }
    }

    [Pattern("(array-of $type)")]
    public class ArrayOfTypeSyntax : TypeSyntax
    {
        [Bind("$type")]
        public TypeSyntax element;

        public override Type GetTheType()
        {
            return element.GetTheType().MakeArrayType();
        }
    }

    [Pattern("(action . $types)")]
    public class ActionTypeSyntax : TypeSyntax
    {
        [Bind("$types")]
        public List<TypeSyntax> types;

        public override Type GetTheType()
        {
            return System.Linq.Expressions.Expression.GetActionType(types.Select(x => x.GetTheType()).ToArray());
        }
    }

    [Pattern("(tuple . $types)")]
    public class TupleTypeSyntax : TypeSyntax
    {
        [Bind("$types")]
        public List<TypeSyntax> types;

        public override Type GetTheType()
        {
            Type[] paramTypes = types.Select(x => x.GetTheType()).ToArray();

            Type baseType = null;
            switch (paramTypes.Length)
            {
                case 1:
                    baseType = typeof(Tuple<>);
                    break;
                case 2:
                    baseType = typeof(Tuple<,>);
                    break;
                case 3:
                    baseType = typeof(Tuple<,,>);
                    break;
                case 4:
                    baseType = typeof(Tuple<,,,>);
                    break;
                case 5:
                    baseType = typeof(Tuple<,,,,>);
                    break;
                case 6:
                    baseType = typeof(Tuple<,,,,,>);
                    break;
                case 7:
                    baseType = typeof(Tuple<,,,,,,>);
                    break;
                default:
                    throw new PascalesqueException("Tuples with more than 7 parts not supported");
            }

            return baseType.MakeGenericType(paramTypes);
        }
    }

    [Pattern("(func . $types)")]
    public class FuncTypeSyntax : TypeSyntax
    {
        [Bind("$types")]
        public List<TypeSyntax> types;

        public override Type GetTheType()
        {
            return System.Linq.Expressions.Expression.GetFuncType(types.Select(x => x.GetTheType()).ToArray());
        }
    }

    [Pattern("($type $var $body)")]
    public class LetClauseSyntax
    {
        [Bind("$type")]
        public TypeSyntax varType;

        [Bind("$var")]
        public Symbol var;

        [Bind("$body")]
        public ExprSyntax body;

        public LetClause GetLetClause()
        {
            return new LetClause(var, varType.GetTheType(), body.GetExpr());
        }
    }

    [Pattern("(let $vars . $body)")]
    public class LetSyntax : ExprSyntax
    {
        [Bind("$vars")]
        public List<LetClauseSyntax> vars;

        [Bind("$body")]
        public List<ExprSyntax> body;

        public override IExpression GetExpr()
        {
            return new LetExpr
            (
                vars.Select(x => x.GetLetClause()),
                BeginExpr.FromList(body.Select(x => x.GetExpr()))
            );
        }
    }

    [Pattern("(let* $vars . $body)")]
    public class LetStarSyntax : ExprSyntax
    {
        [Bind("$vars")]
        public List<LetClauseSyntax> vars;

        [Bind("$body")]
        public List<ExprSyntax> body;

        public override IExpression GetExpr()
        {
            return new LetStarExpr
            (
                vars.Select(x => x.GetLetClause()),
                BeginExpr.FromList(body.Select(x => x.GetExpr()))
            );
        }
    }

    [Pattern("(letrec $vars . $body)")]
    public class LetRecSyntax : ExprSyntax
    {
        [Bind("$vars")]
        public List<LetClauseSyntax> vars;

        [Bind("$body")]
        public List<ExprSyntax> body;

        public override IExpression GetExpr()
        {
            return new LetRecExpr
            (
                vars.Select(x => x.GetLetClause()),
                BeginExpr.FromList(body.Select(x => x.GetExpr()))
            );
        }
    }

    [Pattern("($type $name)")]
    public class ParamSyntax
    {
        [Bind("$type")]
        public TypeSyntax type;

        [Bind("$name")]
        public Symbol name;

        public ParamInfo GetParamInfo()
        {
            return new ParamInfo(name, type.GetTheType());
        }
    }

    [Pattern("(lambda $params . $body)")]
    public class LambdaSyntax : ExprSyntax
    {
        [Bind("$params")]
        public List<ParamSyntax> aParams;

        [Bind("$body")]
        public List<ExprSyntax> body;

        public override IExpression GetExpr()
        {
            return new LambdaExpr
            (
                aParams.Select(x => x.GetParamInfo()),
                BeginExpr.FromList(body.Select(x => x.GetExpr()))
            );
        }
    }

    [Pattern("(invoke $func . $args)")]
    public class InvokeSyntax : ExprSyntax
    {
        [Bind("$func")]
        public ExprSyntax func;

        [Bind("$args")]
        public List<ExprSyntax> args;

        public override IExpression GetExpr()
        {
            return new InvokeExpr(func.GetExpr(), args.Select(x => x.GetExpr()));
        }
    }

    [Pattern("(let-loop $lreturntype $lname $vars . $body)")]
    public class LetLoopSyntax : ExprSyntax
    {
        [Bind("$lreturntype")]
        public TypeSyntax loopReturnType;

        [Bind("$lname")]
        public Symbol loopName;

        [Bind("$vars")]
        public List<LetClauseSyntax> vars;

        [Bind("$body")]
        public List<ExprSyntax> body;

        public override IExpression GetExpr()
        {
            return new LetLoopExpr
            (
                loopName,
                loopReturnType.GetTheType(),
                vars.Select(x => x.GetLetClause()),
                BeginExpr.FromList(body.Select(x => x.GetExpr()))
            );
        }
    }

    [Pattern("($name $val)")]
    public class PinClauseSyntax
    {
        [Bind("$name")]
        public Symbol name;

        [Bind("$val")]
        public ExprSyntax val;

        public PinClause GetPinClause()
        {
            return new PinClause(name, val.GetExpr());
        }
    }

    [Pattern("(pin $clauses . $body)")]
    public class PinSyntax : ExprSyntax
    {
        [Bind("$clauses")]
        public List<PinClauseSyntax> clauses;

        [Bind("$body")]
        public List<ExprSyntax> body;

        public override IExpression GetExpr()
        {
            return new PinExpr(clauses.Select(x => x.GetPinClause()), BeginExpr.FromList(body.Select(x => x.GetExpr())));
        }
    }

    [Pattern("(and . $body)")]
    public class AndSyntax : ExprSyntax
    {
        [Bind("$body")]
        public List<ExprSyntax> body;

        public override IExpression GetExpr()
        {
            return new AndExpr(body.Select(x => x.GetExpr()));
        }
    }

    [Pattern("(or . $body)")]
    public class OrSyntax : ExprSyntax
    {
        [Bind("$body")]
        public List<ExprSyntax> body;

        public override IExpression GetExpr()
        {
            return new OrExpr(body.Select(x => x.GetExpr()));
        }
    }

    [Pattern("(pfor $s $e $proc)")]
    public class ParallelForSyntax : ExprSyntax
    {
        [Bind("$s")]
        public ExprSyntax start;

        [Bind("$e")]
        public ExprSyntax end;

        [Bind("$proc")]
        public ExprSyntax proc;

        public override IExpression GetExpr()
        {
            return new MethodCallExpr
            (
                typeof(System.Threading.Tasks.Parallel).GetMethod("For", new Type[] { typeof(int), typeof(int), typeof(Action<int>) }),
                new IExpression[]
                {
                    start.GetExpr(),
                    end.GetExpr(),
                    proc.GetExpr()
                }
            );
        }
    }

    [Pattern("(sba-get-bytes $sba)")]
    public class SbaGetBytesSyntax : ExprSyntax
    {
        [Bind("$sba")]
        public ExprSyntax expr;

        public override IExpression GetExpr()
        {
            return new MethodCallExpr
            (
                typeof(ExprObjModel.Procedures.SchemeByteArray).GetMethod("get_Bytes", Type.EmptyTypes),
                new IExpression[]
                {
                    expr.GetExpr()
                }
            );
        }
    }

    [Pattern("(byterect-get-sba $byterect)")]
    public class ByteRectangleGetArraySyntax : ExprSyntax
    {
        [Bind("$byterect")]
        public ExprSyntax expr;

        public override IExpression GetExpr()
        {
            return new MethodCallExpr
            (
                typeof(ExprObjModel.Procedures.ByteRectangle).GetMethod("get_Array", Type.EmptyTypes),
                new IExpression[]
                {
                    expr.GetExpr()
                }
            );
        }
    }

    [Pattern("(byterect-get-offset $byterect)")]
    public class ByteRectangleGetOffsetSyntax : ExprSyntax
    {
        [Bind("$byterect")]
        public ExprSyntax expr;

        public override IExpression GetExpr()
        {
            return new MethodCallExpr
            (
                typeof(ExprObjModel.Procedures.ByteRectangle).GetMethod("get_Offset", Type.EmptyTypes),
                new IExpression[]
                {
                    expr.GetExpr()
                }
            );
        }
    }

    [Pattern("(byterect-get-width $byterect)")]
    public class ByteRectangleGetWidthSyntax : ExprSyntax
    {
        [Bind("$byterect")]
        public ExprSyntax expr;

        public override IExpression GetExpr()
        {
            return new MethodCallExpr
            (
                typeof(ExprObjModel.Procedures.ByteRectangle).GetMethod("get_Width", Type.EmptyTypes),
                new IExpression[]
                {
                    expr.GetExpr()
                }
            );
        }
    }

    [Pattern("(byterect-get-height $byterect)")]
    public class ByteRectangleGetHeightSyntax : ExprSyntax
    {
        [Bind("$byterect")]
        public ExprSyntax expr;

        public override IExpression GetExpr()
        {
            return new MethodCallExpr
            (
                typeof(ExprObjModel.Procedures.ByteRectangle).GetMethod("get_Height", Type.EmptyTypes),
                new IExpression[]
                {
                    expr.GetExpr()
                }
            );
        }
    }

    [Pattern("(byterect-get-stride $byterect)")]
    public class ByteRectangleGetStrideSyntax : ExprSyntax
    {
        [Bind("$byterect")]
        public ExprSyntax expr;

        public override IExpression GetExpr()
        {
            return new MethodCallExpr
            (
                typeof(ExprObjModel.Procedures.ByteRectangle).GetMethod("get_Stride", Type.EmptyTypes),
                new IExpression[]
                {
                    expr.GetExpr()
                }
            );
        }
    }

    [Pattern("(bitmapdata-get-height $bitmapdata)")]
    public class BitmapDataGetHeightSyntax : ExprSyntax
    {
        [Bind("$bitmapdata")]
        public ExprSyntax bitmapData;

        public override IExpression GetExpr()
        {
            return new MethodCallExpr
            (
                typeof(System.Drawing.Imaging.BitmapData).GetMethod("get_Height", Type.EmptyTypes),
                new IExpression[]
                {
                    bitmapData.GetExpr()
                }
            );
        }
    }

    [Pattern("(bitmapdata-get-width $bitmapdata)")]
    public class BitmapDataGetWidthSyntax : ExprSyntax
    {
        [Bind("$bitmapdata")]
        public ExprSyntax bitmapData;

        public override IExpression GetExpr()
        {
            return new MethodCallExpr
            (
                typeof(System.Drawing.Imaging.BitmapData).GetMethod("get_Width", Type.EmptyTypes),
                new IExpression[]
                {
                    bitmapData.GetExpr()
                }
            );
        }
    }

    [Pattern("(bitmapdata-get-stride $bitmapdata)")]
    public class BitmapDataGetStrideSyntax : ExprSyntax
    {
        [Bind("$bitmapdata")]
        public ExprSyntax bitmapData;

        public override IExpression GetExpr()
        {
            return new MethodCallExpr
            (
                typeof(System.Drawing.Imaging.BitmapData).GetMethod("get_Stride", Type.EmptyTypes),
                new IExpression[]
                {
                    bitmapData.GetExpr()
                }
            );
        }
    }

    [Pattern("(bitmapdata-get-scan0 $bitmapdata)")]
    public class BitmapDataGetScan0Syntax : ExprSyntax
    {
        [Bind("$bitmapdata")]
        public ExprSyntax bitmapData;

        public override IExpression GetExpr()
        {
            return new MethodCallExpr
            (
                typeof(System.Drawing.Imaging.BitmapData).GetMethod("get_Scan0", Type.EmptyTypes),
                new IExpression[]
                {
                    bitmapData.GetExpr()
                }
            );
        }
    }

    [Pattern("(new-array $type $size)")]
    public class NewArraySyntax : ExprSyntax
    {
        [Bind("$type")]
        public TypeSyntax elementType;

        [Bind("$size")]
        public ExprSyntax arraySize;

        public override IExpression GetExpr()
        {
            return new NewArrayExpr
            (
                elementType.GetTheType(),
                arraySize.GetExpr()
            );
        }
    }

    [Pattern("(array-ref $array $index)")]
    public class ArrayRefSyntax : ExprSyntax
    {
        [Bind("$array")]
        public ExprSyntax array;

        [Bind("$index")]
        public ExprSyntax index;

        public override IExpression GetExpr()
        {
            return new ArrayRefExpr(array.GetExpr(), index.GetExpr());
        }
    }

    [Pattern("(array-set! $array $index $val)")]
    public class ArraySetSyntax : ExprSyntax
    {
        [Bind("$array")]
        public ExprSyntax array;

        [Bind("$index")]
        public ExprSyntax index;

        [Bind("$val")]
        public ExprSyntax val;

        public override IExpression GetExpr()
        {
            return new ArraySetExpr(array.GetExpr(), index.GetExpr(), val.GetExpr());
        }
    }

    [Pattern("(array-length $array)")]
    public class ArrayLengthSyntax : ExprSyntax
    {
        [Bind("$array")]
        public ExprSyntax array;

        public override IExpression GetExpr()
        {
            return new ArrayLenExpr(array.GetExpr());
        }
    }

    [Pattern("(poke! $addr $val)")]
    public class PokeSyntax : ExprSyntax
    {
        [Bind("$addr")]
        public ExprSyntax addr;

        [Bind("$val")]
        public ExprSyntax val;

        public override IExpression GetExpr()
        {
            return new PokeExpr(addr.GetExpr(), val.GetExpr());
        }
    }

    [Pattern("(peek $type $addr)")]
    public class PeekSyntax : ExprSyntax
    {
        [Bind("$type")]
        public TypeSyntax peekType;

        [Bind("$addr")]
        public ExprSyntax addr;

        public override IExpression GetExpr()
        {
            return new PeekExpr(addr.GetExpr(), peekType.GetTheType());
        }
    }

    [Pattern("(memset! $dest $val $count)")]
    public class MemSetSyntax : ExprSyntax
    {
        [Bind("$dest")]
        public ExprSyntax dest;

        [Bind("$val")]
        public ExprSyntax val;

        [Bind("$count")]
        public ExprSyntax count;

        public override IExpression GetExpr()
        {
            return new MemSetExpr(dest.GetExpr(), val.GetExpr(), count.GetExpr());
        }
    }

    [Pattern("(memcpy! $dest $src $count)")]
    public class MemCpySyntax : ExprSyntax
    {
        [Bind("$dest")]
        public ExprSyntax dest;

        [Bind("$src")]
        public ExprSyntax src;

        [Bind("$count")]
        public ExprSyntax count;

        public override IExpression GetExpr()
        {
            return new MemCpyExpr(dest.GetExpr(), src.GetExpr(), count.GetExpr());
        }
    }

    [DescendantsWithPatterns]
    public abstract class SwitchClause
    {
    }

    [Pattern("($items . $exprs)")]
    public class SwitchClauseRegular : SwitchClause
    {
        [Bind("$items")]
        public List<uint> keys;

        [Bind("$exprs")]
        public List<ExprSyntax> exprs;
    }

    [Pattern("(else . $exprs)")]
    public class SwitchClauseElse : SwitchClause
    {
        [Bind("$exprs")]
        public List<ExprSyntax> exprs;
    }

    [Pattern("(switch $expr . $clauses)")]
    public class SwitchSyntax : ExprSyntax
    {
        [Bind("$expr")]
        public ExprSyntax expr;

        [Bind("$clauses")]
        public List<SwitchClause> clauses;

        public override IExpression GetExpr()
        {
            if (clauses.Count == 0) throw new PascalesqueException("Switch must have at least one clause");

            int elseCount = clauses.Where(x => x is SwitchClauseElse).Count();
            if (elseCount > 1) throw new PascalesqueException("Too many else clauses in switch");
            if (elseCount == 1 && !(clauses.Last() is SwitchClauseElse)) throw new PascalesqueException("In switch, else clause must be last");

            IExpression elseExpr;
            if (elseCount == 0)
            {
                elseExpr = new EmptyExpr();
            }
            else
            {
                elseExpr = BeginExpr.FromList(((SwitchClauseElse)(clauses.Last())).exprs.Select(y => y.GetExpr()));
            }
            return new SwitchExpr
            (
                expr.GetExpr(),
                elseExpr,
                clauses.OfType<SwitchClauseRegular>().Select(x => new Tuple<IEnumerable<uint>, IExpression>(x.keys, BeginExpr.FromList(x.exprs.Select(y => y.GetExpr()))))
            );
        }
    }

    [DescendantsWithPatterns]
    public abstract class CondClause
    {
    }

    [Pattern("(if $cond . $exprs)")]
    public class CondClauseRegular : CondClause
    {
        [Bind("$cond")]
        public ExprSyntax cond;

        [Bind("$exprs")]
        public List<ExprSyntax> exprs;
    }

    [Pattern("(else . $exprs)")]
    public class CondClauseElse : CondClause
    {
        [Bind("$exprs")]
        public List<ExprSyntax> exprs;
    }

    [Pattern("(cond . $clauses)")]
    public class CondSyntax : ExprSyntax
    {
        [Bind("$clauses")]
        public List<CondClause> clauses;

        public override IExpression GetExpr()
        {
            if (clauses.Count == 0) throw new PascalesqueException("Cond must have at least one clause");

            int elseCount = clauses.Where(x => x is CondClauseElse).Count();
            if (elseCount > 1) throw new PascalesqueException("Too many else clauses in cond");
            if (elseCount == 1 && !(clauses.Last() is CondClauseElse)) throw new PascalesqueException("In cond, else clause must be last");

            IExpression elseExpr;
            if (elseCount == 0)
            {
                elseExpr = new EmptyExpr();
            }
            else
            {
                elseExpr = BeginExpr.FromList(((CondClauseElse)(clauses.Last())).exprs.Select(y => y.GetExpr()));
            }

            int i = clauses.Count;
            while (i > 0)
            {
                --i;
                if (clauses[i] is CondClauseRegular)
                {
                    CondClauseRegular ccr = (CondClauseRegular)(clauses[i]);
                    elseExpr = new IfThenElseExpr(ccr.cond.GetExpr(), BeginExpr.FromList(ccr.exprs.Select(x => x.GetExpr())), elseExpr);
                }
            }

            return elseExpr;
        }
    }

    [DescendantsWithPatterns]
    public abstract class TryClause
    {
    }

    [Pattern("(try . $body)")]
    public class TryBodyClause : TryClause
    {
        [Bind("$body")]
        public List<ExprSyntax> body;
    }

    [Pattern("(catch ($type $var) . $body)")]
    public class CatchBodyClause : TryClause
    {
        [Bind("$type")]
        public TypeSyntax exceptionType;

        [Bind("$var")]
        public Symbol exceptionName;

        [Bind("$body")]
        public List<ExprSyntax> body;
    }

    [Pattern("(finally . $body)")]
    public class FinallyBodyClause : TryClause
    {
        [Bind("$body")]
        public List<ExprSyntax> body;
    }

    [Pattern("(try-block . $clauses)")]
    public class TrySyntax : ExprSyntax
    {
        [Bind("$clauses")]
        public List<TryClause> clauses;

        public override IExpression GetExpr()
        {
            TryClause body1 = Enumerable.Single(clauses.Where(x => x is TryBodyClause));
            TryClause finally1 = Enumerable.SingleOrDefault(clauses.Where(x => x is FinallyBodyClause));

            // this is sloppy because it allows you to put a catch before the body or after the finally...

            return new TryCatchFinallyExpr
            (
                BeginExpr.FromList(((TryBodyClause)body1).body.Select(x => x.GetExpr())),
                clauses.OfType<CatchBodyClause>().Select(x => new CatchClause(x.exceptionType.GetTheType(), x.exceptionName, BeginExpr.FromList(x.body.Select(y => y.GetExpr())))),
                (finally1 == null) ? new EmptyExpr() : BeginExpr.FromList(((FinallyBodyClause)finally1).body.Select(x => x.GetExpr()))
            );
        }
    }

    [Pattern("(using* $vars . $body)")]
    public class UsingSyntax : ExprSyntax
    {
        [Bind("$vars")]
        public List<LetClauseSyntax> vars;

        [Bind("$body")]
        public List<ExprSyntax> body;

        public override IExpression GetExpr()
        {
            IExpression i = BeginExpr.FromList(body.Select(x => x.GetExpr()));
            int j = vars.Count;
            while (j > 0)
            {
                --j;
                Symbol temp = new Symbol();
                i = new LetExpr
                (
                    ExprObjModel.Utils.SingleItem<LetClause>(vars[j].GetLetClause()),
                    new TryCatchFinallyExpr
                    (
                        i,
                        Enumerable.Empty<CatchClause>(),
                        new LetExpr
                        (
                            new LetClause[]
                            {
                                new LetClause(temp, typeof(IDisposable), new CastClassExpr(typeof(IDisposable), new VarRefExpr(vars[j].var)))
                            },
                            new IfThenElseExpr
                            (
                                new IsNullExpr
                                (
                                    new VarRefExpr(temp)
                                ),
                                new EmptyExpr(),
                                new MethodCallExpr
                                (
                                    typeof(IDisposable).GetMethod("Dispose", Type.EmptyTypes),
                                    new IExpression[]
                                    {
                                        new VarRefExpr(temp)
                                    }
                                )
                            )
                        )
                    )
                );
            }
            return i;
        }
    }

    [Pattern("(throw $type $exp)")]
    public class ThrowSyntax : ExprSyntax
    {
        [Bind("$type")]
        public TypeSyntax type;

        [Bind("$exp")]
        public ExprSyntax exp;

        public override IExpression GetExpr()
        {
            return new ThrowExpr(type.GetTheType(), exp.GetExpr());
        }
    }

    [Pattern("(new-exception $msg)")]
    public class NewExceptionSyntax : ExprSyntax
    {
        [Bind("$msg")]
        public string msg;

        public override IExpression GetExpr()
        {
            return new MethodCallExpr
            (
                typeof(Exception).GetConstructor(new Type[] { typeof(string) }),
                new IExpression[]
                {
                    new LiteralExpr(msg)
                }
            );
        }
    }

    [Pattern("(null? $var)")]
    public class IsNullSyntax : ExprSyntax
    {
        [Bind("$var")]
        public ExprSyntax body;

        public override IExpression GetExpr()
        {
            return new IsNullExpr(body.GetExpr());
        }
    }

    [Pattern("(is? $type $body)")]
    public class IsOfTypeSyntax : ExprSyntax
    {
        [Bind("$type")]
        public TypeSyntax type;

        [Bind("$body")]
        public ExprSyntax body;

        public override IExpression GetExpr()
        {
            return new IsInstanceExpr(body.GetExpr(), type.GetTheType());
        }
    }

    [Pattern("(cast-to $type $body)")]
    public class CastToTypeSyntax : ExprSyntax
    {
        [Bind("$type")]
        public TypeSyntax type;

        [Bind("$body")]
        public ExprSyntax body;

        public override IExpression GetExpr()
        {
            return new CastClassExpr(type.GetTheType(), body.GetExpr());
        }
    }

    [Pattern("(box $expr)")]
    public class BoxSyntax : ExprSyntax
    {
        [Bind("$expr")]
        public ExprSyntax expr;

        public override IExpression GetExpr()
        {
            return new BoxExpr(expr.GetExpr());
        }
    }

    [Pattern("(unbox $type $expr)")]
    public class UnboxSyntax : ExprSyntax
    {
        [Bind("$type")]
        public TypeSyntax type;

        [Bind("$expr")]
        public ExprSyntax expr;

        public override IExpression GetExpr()
        {
            return new UnboxExpr(expr.GetExpr(), type.GetTheType());
        }
    }

    [Pattern("(tuple-first $expr)")]
    public class TupleFirstSyntax : ExprSyntax
    {
        [Bind("$expr")]
        public ExprSyntax expr;

        public override IExpression GetExpr()
        {
            return new TupleItemExpr(expr.GetExpr(), 0);
        }
    }

    [Pattern("(tuple-second $expr)")]
    public class TupleSecondSyntax : ExprSyntax
    {
        [Bind("$expr")]
        public ExprSyntax expr;

        public override IExpression GetExpr()
        {
            return new TupleItemExpr(expr.GetExpr(), 1);
        }
    }

    [Pattern("(tuple-third $expr)")]
    public class TupleThirdSyntax : ExprSyntax
    {
        [Bind("$expr")]
        public ExprSyntax expr;

        public override IExpression GetExpr()
        {
            return new TupleItemExpr(expr.GetExpr(), 2);
        }
    }

    [Pattern("(tuple-fourth $expr)")]
    public class TupleFourthSyntax : ExprSyntax
    {
        [Bind("$expr")]
        public ExprSyntax expr;

        public override IExpression GetExpr()
        {
            return new TupleItemExpr(expr.GetExpr(), 3);
        }
    }

    [Pattern("(tuple-fifth $expr)")]
    public class TupleFifthSyntax : ExprSyntax
    {
        [Bind("$expr")]
        public ExprSyntax expr;

        public override IExpression GetExpr()
        {
            return new TupleItemExpr(expr.GetExpr(), 4);
        }
    }

    [Pattern("(tuple-sixth $expr)")]
    public class TupleSixthSyntax : ExprSyntax
    {
        [Bind("$expr")]
        public ExprSyntax expr;

        public override IExpression GetExpr()
        {
            return new TupleItemExpr(expr.GetExpr(), 5);
        }
    }

    [Pattern("(tuple-seventh $expr)")]
    public class TupleSeventhSyntax : ExprSyntax
    {
        [Bind("$expr")]
        public ExprSyntax expr;

        public override IExpression GetExpr()
        {
            return new TupleItemExpr(expr.GetExpr(), 6);
        }
    }

    [DescendantsWithPatterns]
    public abstract class CriterionSyntax
    {
        public abstract MethodCriteria GetMethodCriterion();

        public abstract PropertyCriteria GetPropertyCriterion();
    }

    [Pattern("static")]
    public class StaticCriterion : CriterionSyntax
    {
        public override MethodCriteria GetMethodCriterion()
        {
            return MethodCriteria.IsStatic | MethodCriteria.IsNotVirtual;
        }

        public override PropertyCriteria GetPropertyCriterion()
        {
            return PropertyCriteria.IsStatic;
        }
    }

    [Pattern("instance")]
    public class InstanceCriterion : CriterionSyntax
    {
        public override MethodCriteria GetMethodCriterion()
        {
            return MethodCriteria.IsNotStatic;
        }

        public override PropertyCriteria GetPropertyCriterion()
        {
            return PropertyCriteria.IsNotStatic;
        }
    }

    [Pattern("public")]
    public class PublicCriterion : CriterionSyntax
    {
        public override MethodCriteria GetMethodCriterion()
        {
            return MethodCriteria.IsPublic;
        }

        public override PropertyCriteria GetPropertyCriterion()
        {
            return PropertyCriteria.IsPublic;
        }
    }

    [Pattern("nonpublic")]
    public class NonPublicCriterion : CriterionSyntax
    {
        public override MethodCriteria GetMethodCriterion()
        {
            return MethodCriteria.IsNotPublic;
        }

        public override PropertyCriteria GetPropertyCriterion()
        {
            return PropertyCriteria.IsNotPublic;
        }
    }

    [Pattern("special")]
    public class SpecialCriterion : CriterionSyntax
    {
        public override MethodCriteria GetMethodCriterion()
        {
            return MethodCriteria.IsSpecialName;
        }

        public override PropertyCriteria GetPropertyCriterion()
        {
            return PropertyCriteria.IsSpecialName;
        }
    }

    [Pattern("nonspecial")]
    public class NonSpecialCriterion : CriterionSyntax
    {
        public override MethodCriteria GetMethodCriterion()
        {
            return MethodCriteria.IsNotSpecialName;
        }

        public override PropertyCriteria GetPropertyCriterion()
        {
            return PropertyCriteria.IsNotSpecialName;
        }
    }

    [Pattern("(call $criteria $type $name $params . $exprs)")]
    public class CallStaticSyntax : ExprSyntax
    {
        [Bind("$criteria")]
        public List<CriterionSyntax> criteria;

        [Bind("$type")]
        public TypeSyntax type;

        [Bind("$name")]
        public string name;

        [Bind("$params")]
        public List<TypeSyntax> theParams;

        [Bind("$exprs")]
        public List<ExprSyntax> theArgs;

        public override IExpression GetExpr()
        {
            Type t = type.GetTheType();
            MethodCriteria criteria2 = criteria.Select(x => x.GetMethodCriterion()).Combine();
            MethodInfo mi = t.GetMethod(name, criteria2, theParams.Select(x => x.GetTheType()).ToArray());
            int extraArgs = mi.IsStatic ? 0 : 1;
            if (theParams.Count + extraArgs != theArgs.Count) throw new PascalesqueException("call-static: parameter count doesn't match");
            return new MethodCallExpr(mi, theArgs.Select(x => x.GetExpr()));
        }
    }

    [Pattern("get-property $criteria $type $name $params . $exprs)")]
    public class GetStaticPropertySyntax : ExprSyntax
    {
        [Bind("$criteria")]
        public List<CriterionSyntax> criteria;

        [Bind("$type")]
        public TypeSyntax type;

        [Bind("$name")]
        public string name;

        [Bind("$params")]
        public List<TypeSyntax> theParams;

        [Bind("$exprs")]
        public List<ExprSyntax> theArgs;

        public override IExpression GetExpr()
        {
            Type t = type.GetTheType();
            PropertyCriteria criteria2 = criteria.Select(x => x.GetPropertyCriterion()).Combine();
            PropertyInfo pi = t.GetProperty(name, criteria2, theParams.Select(x => x.GetTheType()).ToArray());
            MethodInfo mi = pi.GetGetMethod();
            if (mi == null) throw new PascalesqueException("get-property: property is not gettable");
            int extra = mi.IsStatic ? 0 : 1;
            if (theParams.Count + extra != theArgs.Count) throw new PascalesqueException("get-property: parameter count doesn't match");
            return new MethodCallExpr(mi, theArgs.Select(x => x.GetExpr()));
        }
    }

    [Pattern("set-property! $criteria $type $name $params . $exprs)")]
    public class SetStaticPropertySyntax : ExprSyntax
    {
        [Bind("$criteria")]
        public List<CriterionSyntax> criteria;

        [Bind("$type")]
        public TypeSyntax type;

        [Bind("$name")]
        public string name;

        [Bind("$params")]
        public List<TypeSyntax> theParams;

        [Bind("$exprs")]
        public List<ExprSyntax> theArgs;

        public override IExpression GetExpr()
        {
            Type t = type.GetTheType();
            PropertyCriteria criteria2 = criteria.Select(x => x.GetPropertyCriterion()).Combine();
            PropertyInfo pi = t.GetProperty(name, criteria2, theParams.Select(x => x.GetTheType()).ToArray());
            MethodInfo mi = pi.GetSetMethod();
            if (mi == null) throw new PascalesqueException("set-property!: property is not settable");
            int extra = mi.IsStatic ? 0 : 1;
            if (theParams.Count + extra != theArgs.Count) throw new PascalesqueException("set-property!: parameter count doesn't match");
            return new MethodCallExpr(mi, theArgs.Select(x => x.GetExpr()));
        }
    }

    [Pattern("(new $type $params . $exprs)")]
    public class NewObjSyntax : ExprSyntax
    {
        [Bind("$type")]
        public TypeSyntax type;

        [Bind("$params")]
        public List<TypeSyntax> theParams;

        [Bind("$exprs")]
        public List<ExprSyntax> theArgs;

        public override IExpression GetExpr()
        {
            Type t = type.GetTheType();
            ConstructorInfo ci = t.GetConstructor(theParams.Select(x => x.GetTheType()).ToArray());
            return new MethodCallExpr(ci, theArgs.Select(x => x.GetExpr()));
        }
    }

    [Pattern("(create-bitmap $x $y)")]
    public class CreateBitmapSyntax : ExprSyntax
    {
        [Bind("$x")]
        public ExprSyntax x;

        [Bind("$y")]
        public ExprSyntax y;

        public override IExpression GetExpr()
        {
            return new MethodCallExpr
            (
                typeof(System.Drawing.Bitmap).GetConstructor(new Type[] { typeof(int), typeof(int), typeof(System.Drawing.Imaging.PixelFormat) }),
                new IExpression[]
                {
                    x.GetExpr(),
                    y.GetExpr(),
                    new LiteralExpr(System.Drawing.Imaging.PixelFormat.Format32bppRgb)
                }
            );
        }
    }

    [DescendantsWithPatterns]
    public abstract class LockModeSyntax
    {
        public abstract System.Drawing.Imaging.ImageLockMode GetLockMode();
    }

    [Pattern("read")]
    public class ReadLockModeSyntax : LockModeSyntax
    {
        public override System.Drawing.Imaging.ImageLockMode GetLockMode()
        {
            return System.Drawing.Imaging.ImageLockMode.ReadOnly;
        }
    }

    [Pattern("write")]
    public class WriteLockModeSyntax : LockModeSyntax
    {
        public override System.Drawing.Imaging.ImageLockMode GetLockMode()
        {
            return System.Drawing.Imaging.ImageLockMode.WriteOnly;
        }
    }

    [Pattern("read-write")]
    public class ReadWriteLockModeSyntax : LockModeSyntax
    {
        public override System.Drawing.Imaging.ImageLockMode GetLockMode()
        {
            return System.Drawing.Imaging.ImageLockMode.ReadWrite;
        }
    }

    [Pattern("(lock-bits! $bmp $mode)")]
    public class LockBitmapSyntax : ExprSyntax
    {
        [Bind("$mode")]
        public LockModeSyntax lockMode;

        [Bind("$bmp")]
        public ExprSyntax bmp;

        public override IExpression GetExpr()
        {
            Symbol bmpvar = new Symbol();
            return new LetExpr
            (
                new LetClause[]
                {
                    new LetClause(bmpvar, typeof(System.Drawing.Bitmap), bmp.GetExpr())
                },
                new MethodCallExpr
                (
                    typeof(System.Drawing.Bitmap).GetMethod
                    (
                        "LockBits",
                        MethodCriteria.IsPublic | MethodCriteria.IsNotStatic | MethodCriteria.IsNotSpecialName,
                        new Type[]
                        {
                            typeof(System.Drawing.Rectangle),
                            typeof(System.Drawing.Imaging.ImageLockMode),
                            typeof(System.Drawing.Imaging.PixelFormat)
                        }
                    
                    ),
                    new IExpression[]
                    {
                        new VarRefExpr(bmpvar),
                        new MethodCallExpr
                        (
                            typeof(System.Drawing.Rectangle).GetConstructor
                            (
                                new Type[]
                                {
                                    typeof(System.Drawing.Point),
                                    typeof(System.Drawing.Size)
                                }
                            ),
                            new IExpression[]
                            {
                                new MethodCallExpr
                                (
                                    typeof(System.Drawing.Point).GetMethod
                                    (
                                        "get_Empty",
                                        MethodCriteria.IsSpecialName | MethodCriteria.IsPublic | MethodCriteria.IsStatic,
                                        Type.EmptyTypes
                                    ),
                                    new IExpression[] { }
                                ),
                                new MethodCallExpr
                                (
                                    typeof(System.Drawing.Image).GetMethod
                                    (
                                        "get_Size",
                                        MethodCriteria.IsSpecialName | MethodCriteria.IsPublic | MethodCriteria.IsNotStatic,
                                        Type.EmptyTypes
                                    ),
                                    new IExpression[]
                                    {
                                        new VarRefExpr(bmpvar)
                                    }
                                )
                            }
                        ),
                        new LiteralExpr(lockMode.GetLockMode()),
                        new LiteralExpr(System.Drawing.Imaging.PixelFormat.Format32bppRgb)
                    }
                )
            );
        }
    }

    [Pattern("(unlock-bits! $bitmap $bitmapdata)")]
    public class UnlockBitmapSyntax : ExprSyntax
    {
        [Bind("$bitmap")]
        public ExprSyntax bitmap;

        [Bind("$bitmapdata")]
        public ExprSyntax bitmapData;

        public override IExpression GetExpr()
        {
            return new MethodCallExpr
            (
                typeof(System.Drawing.Bitmap).GetMethod
                (
                    "UnlockBits",
                    MethodCriteria.IsPublic | MethodCriteria.IsNotStatic | MethodCriteria.IsNotSpecialName,
                    new Type[]
                    {
                        typeof(System.Drawing.Imaging.BitmapData)
                    }
                ),
                new IExpression[]
                {
                    bitmap.GetExpr(),
                    bitmapData.GetExpr()
                }
            );
        }
    }

    public static class SyntaxAnalyzer
    {
        private static object syncRoot = new object();
        private static Func<object, ExprObjModel.Option<object>> parserExpr = null;
        private static Func<object, ExprObjModel.Option<object>> parserType = null;

        public static IExpression AnalyzeExpr(object obj)
        {
            if (parserExpr == null)
            {
                lock(syncRoot)
                {
                    if (parserExpr == null)
                    {
                        parserExpr = ExprObjModel.Utils.MakeParser(typeof(ExprSyntax));
                    }
                }
            }
            ExprObjModel.Option<object> k = parserExpr(obj);
            if (k is ExprObjModel.Some<object>)
            {
                ExprSyntax es = (ExprSyntax)(((ExprObjModel.Some<object>)k).value);
                IExpression t = es.GetExpr();
                return t;
            }
            else
            {
                return null;
            }
        }

        public static Type AnalyzeType(object obj)
        {
            if (parserType == null)
            {
                lock (syncRoot)
                {
                    if (parserType == null)
                    {
                        parserType = ExprObjModel.Utils.MakeParser(typeof(TypeSyntax));
                    }
                }
            }
            ExprObjModel.Option<object> k = parserType(obj);
            if (k is ExprObjModel.Some<object>)
            {
                TypeSyntax ts = (TypeSyntax)(((ExprObjModel.Some<object>)k).value);
                Type t = ts.GetTheType();
                return t;
            }
            else
            {
                return null;
            }
        }

#if false
        public static Func<object, ExprObjModel.Option<object>> letClauseParser = null;

        public static LetClause TestParseLetClause(object obj)
        {
            if (letClauseParser == null)
            {
                letClauseParser = ExprObjModel.Utils.MakeParser(typeof(LetClauseSyntax));
            }
            ExprObjModel.Option<object> k = letClauseParser(obj);
            if (k is ExprObjModel.Some<object>)
            {
                LetClauseSyntax lcs = (LetClauseSyntax)(((ExprObjModel.Some<object>)k).value);
                LetClause lc = lcs.GetLetClause();
                return lc;
            }
            else
            {
                return null;
            }
        }

        public static Func<object, ExprObjModel.Option<object>> typeParser;

        public static Type TestParseType(object obj)
        {
            if (typeParser == null)
            {
                typeParser = ExprObjModel.Utils.MakeParser(typeof(TypeSyntax));
            }
            ExprObjModel.Option<object> k = typeParser(obj);
            if (k is ExprObjModel.Some<object>)
            {
                TypeSyntax ts = (TypeSyntax)(((ExprObjModel.Some<object>)k).value);
                Type t = ts.GetTheType();
                return t;
            }
            else
            {
                return null;
            }
        }
#endif
    }
}
