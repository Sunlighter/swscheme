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
    [DescendantsWithPatterns]
    public abstract class ExprSyntax
    {
        public abstract IExpression2 GetExpr();
    }

    [Pattern("#t")]
    public class LiteralBooleanTrueSyntax : ExprSyntax
    {
        public override IExpression2 GetExpr()
        {
            return new LiteralExpr2(true);
        }
    }

    [Pattern("#f")]
    public class LiteralBooleanFalseSyntax : ExprSyntax
    {
        public override IExpression2 GetExpr()
        {
            return new LiteralExpr2(false);
        }
    }

    [Pattern("(pi)")]
    public class LiteralPiSyntax : ExprSyntax
    {
        public override IExpression2 GetExpr()
        {
            return new LiteralExpr2(Math.PI);
        }
    }

    [Pattern("(e)")]
    public class LiteralESyntax : ExprSyntax
    {
        public override IExpression2 GetExpr()
        {
            return new LiteralExpr2(Math.E);
        }
    }

    [Pattern("$ch")]
    public class LiteralCharSyntax : ExprSyntax
    {
        [Bind("$ch")]
        public char ch;

        public override IExpression2 GetExpr()
        {
            return new LiteralExpr2(ch);
        }
    }

    [Pattern("$str")]
    public class LiteralStringSyntax : ExprSyntax
    {
        [Bind("$str")]
        public string str;

        public override IExpression2 GetExpr()
        {
            return new LiteralExpr2(str);
        }
    }

    [Pattern("$var")]
    public class VarRefSyntax : ExprSyntax
    {
        [Bind("$var")]
        public Symbol v;

        public override IExpression2 GetExpr()
        {
            return new VarRefExpr2(v);
        }
    }

    [Pattern("(set! $v $val)")]
    public class VarSetSyntax : ExprSyntax
    {
        [Bind("$v")]
        public Symbol v;

        [Bind("$val")]
        public ExprSyntax val;

        public override IExpression2 GetExpr()
        {
            return new VarSetExpr2(v, val.GetExpr());
        }
    }

    [Pattern("(begin . $exprs)")]
    public class BeginSyntax : ExprSyntax
    {
        [Bind("$exprs")]
        public List<ExprSyntax> exprs;

        public override IExpression2 GetExpr()
        {
            return BeginExpr2.FromList(exprs.Select(x => x.GetExpr()));
        }
    }

    [Pattern("(nop)")]
    public class VoidSyntax : ExprSyntax
    {
        public override IExpression2 GetExpr()
        {
            return new EmptyExpr2();
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

        public override IExpression2 GetExpr()
        {
            return new IfThenElseExpr2(test.GetExpr(), aThen.GetExpr(), aElse.GetExpr());
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

        public override IExpression2 GetExpr()
        {
            return new BeginWhileRepeatExpr2(pre.GetExpr(), test.GetExpr(), post.GetExpr());
        }
    }

    [DescendantsWithPatterns]
    public abstract class TypeSyntax
    {
        public abstract TypeReference GetTheType();
    }

    [Pattern("bool")]
    public class BoolTypeSyntax : TypeSyntax
    {
        public override TypeReference GetTheType()
        {
            return ExistingTypeReference.Boolean;
        }
    }

    [Pattern("intptr")]
    public class IntPtrTypeSyntax : TypeSyntax
    {
        public override TypeReference GetTheType()
        {
            return ExistingTypeReference.IntPtr;
        }
    }

    [Pattern("uintptr")]
    public class UIntPtrTypeSyntax : TypeSyntax
    {
        public override TypeReference GetTheType()
        {
            return ExistingTypeReference.UIntPtr;
        }
    }

    [Pattern("void")]
    public class VoidTypeSyntax : TypeSyntax
    {
        public override TypeReference GetTheType()
        {
            return ExistingTypeReference.Void;
        }
    }

    [Pattern("bitmapdata")]
    public class BitmapDataTypeSyntax : TypeSyntax
    {
        public override TypeReference GetTheType()
        {
            return new ExistingTypeReference(typeof(System.Drawing.Imaging.BitmapData));
        }
    }

    [Pattern("byterect")]
    public class ByteRectangleTypeSyntax : TypeSyntax
    {
        public override TypeReference GetTheType()
        {
            return new ExistingTypeReference(typeof(ExprObjModel.Procedures.ByteRectangle));
        }
    }

    [Pattern("sba")]
    public class SbaTypeSyntax : TypeSyntax
    {
        public override TypeReference GetTheType()
        {
            return new ExistingTypeReference(typeof(ExprObjModel.Procedures.SchemeByteArray));
        }
    }

    [Pattern("char")]
    public class CharTypeSyntax : TypeSyntax
    {
        public override TypeReference GetTheType()
        {
            return ExistingTypeReference.Char;
        }
    }

    [Pattern("object")]
    public class ObjectTypeSyntax : TypeSyntax
    {
        public override TypeReference GetTheType()
        {
            return ExistingTypeReference.Object;
        }
    }

    [Pattern("string")]
    public class StringTypeSyntax : TypeSyntax
    {
        public override TypeReference GetTheType()
        {
            return ExistingTypeReference.String;
        }
    }

    [Pattern("exception")]
    public class ExceptionTypeSyntax : TypeSyntax
    {
        public override TypeReference GetTheType()
        {
            return new ExistingTypeReference(typeof(Exception));
        }
    }

    [Pattern("idisposable")]
    public class DisposableTypeSyntax : TypeSyntax
    {
        public override TypeReference GetTheType()
        {
            return new ExistingTypeReference(typeof(IDisposable));
        }
    }

    [Pattern("bitmap")]
    public class BitmapTypeSyntax : TypeSyntax
    {
        public override TypeReference GetTheType()
        {
            return new ExistingTypeReference(typeof(System.Drawing.Bitmap));
        }
    }

    [Pattern("(existing-type-named $name . $types)")]
    public class ExistingTypeNamedSyntax : TypeSyntax
    {
        [Bind("$name")]
        public string name;

        [Bind("$types")]
        public List<TypeSyntax> types;

        public override TypeReference GetTheType()
        {
            Type t1 = Type.GetType(name, true);
            if (t1.IsGenericTypeDefinition)
            {
                return new ExistingGenericTypeReference(t1, types.Select(x => x.GetTheType()));
                //return t1.MakeGenericType(types.Select(x => x.GetTheType()).ToArray());
            }
            else if (types.Count > 0)
            {
                throw new PascalesqueException("The type \"" + name + "\" doesn't take any type parameters");
            }
            else
            {
                return new ExistingTypeReference(Type.GetType(name, true));
            }
        }
    }

    [Pattern("(new-type-named $name)")]
    public class NewTypeNamedSyntax : TypeSyntax
    {
        [Bind("$name")]
        public Symbol name;

        public override TypeReference GetTheType()
        {
            return new TypeKeyReference(new TypeKey(name));
        }
    }

    [Pattern("(array-of $type)")]
    public class ArrayOfTypeSyntax : TypeSyntax
    {
        [Bind("$type")]
        public TypeSyntax element;

        public override TypeReference GetTheType()
        {
            return element.GetTheType().MakeArrayType();
        }
    }

    [Pattern("(action . $types)")]
    public class ActionTypeSyntax : TypeSyntax
    {
        [Bind("$types")]
        public List<TypeSyntax> types;

        public override TypeReference GetTheType()
        {
            return ExistingGenericTypeReference.GetActionType(types.Select(x => x.GetTheType()).ToArray());
        }
    }

    [Pattern("(tuple . $types)")]
    public class TupleTypeSyntax : TypeSyntax
    {
        [Bind("$types")]
        public List<TypeSyntax> types;

        public override TypeReference GetTheType()
        {
            TypeReference[] paramTypes = types.Select(x => x.GetTheType()).ToArray();

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

            return new ExistingGenericTypeReference(baseType, paramTypes);
        }
    }

    [Pattern("(func . $types)")]
    public class FuncTypeSyntax : TypeSyntax
    {
        [Bind("$types")]
        public List<TypeSyntax> types;

        public override TypeReference GetTheType()
        {
            return ExistingGenericTypeReference.GetFuncType(types.Select(x => x.GetTheType()).ToArray());
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

        public LetClause2 GetLetClause()
        {
            return new LetClause2(var, varType.GetTheType(), body.GetExpr());
        }
    }

    [Pattern("(let $vars . $body)")]
    public class LetSyntax : ExprSyntax
    {
        [Bind("$vars")]
        public List<LetClauseSyntax> vars;

        [Bind("$body")]
        public List<ExprSyntax> body;

        public override IExpression2 GetExpr()
        {
            return new LetExpr2
            (
                vars.Select(x => x.GetLetClause()),
                BeginExpr2.FromList(body.Select(x => x.GetExpr()))
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

        public override IExpression2 GetExpr()
        {
            return new LetStarExpr2
            (
                vars.Select(x => x.GetLetClause()),
                BeginExpr2.FromList(body.Select(x => x.GetExpr()))
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

        public override IExpression2 GetExpr()
        {
            return new LetRecExpr2
            (
                vars.Select(x => x.GetLetClause()),
                BeginExpr2.FromList(body.Select(x => x.GetExpr()))
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

        public ParamInfo2 GetParamInfo()
        {
            return new ParamInfo2(name, type.GetTheType());
        }
    }

    [Pattern("(lambda $params . $body)")]
    public class LambdaSyntax : ExprSyntax
    {
        [Bind("$params")]
        public List<ParamSyntax> aParams;

        [Bind("$body")]
        public List<ExprSyntax> body;

        public override IExpression2 GetExpr()
        {
            return new LambdaExpr2
            (
                aParams.Select(x => x.GetParamInfo()),
                BeginExpr2.FromList(body.Select(x => x.GetExpr()))
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

        public override IExpression2 GetExpr()
        {
            return new InvokeExpr2(func.GetExpr(), args.Select(x => x.GetExpr()));
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

        public override IExpression2 GetExpr()
        {
            return new LetLoopExpr2
            (
                loopName,
                loopReturnType.GetTheType(),
                vars.Select(x => x.GetLetClause()),
                BeginExpr2.FromList(body.Select(x => x.GetExpr()))
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

        public PinClause2 GetPinClause()
        {
            return new PinClause2(name, val.GetExpr());
        }
    }

    [Pattern("(pin $clauses . $body)")]
    public class PinSyntax : ExprSyntax
    {
        [Bind("$clauses")]
        public List<PinClauseSyntax> clauses;

        [Bind("$body")]
        public List<ExprSyntax> body;

        public override IExpression2 GetExpr()
        {
            return new PinExpr2(clauses.Select(x => x.GetPinClause()), BeginExpr2.FromList(body.Select(x => x.GetExpr())));
        }
    }

    [Pattern("(and . $body)")]
    public class AndSyntax : ExprSyntax
    {
        [Bind("$body")]
        public List<ExprSyntax> body;

        public override IExpression2 GetExpr()
        {
            return new AndExpr2(body.Select(x => x.GetExpr()));
        }
    }

    [Pattern("(or . $body)")]
    public class OrSyntax : ExprSyntax
    {
        [Bind("$body")]
        public List<ExprSyntax> body;

        public override IExpression2 GetExpr()
        {
            return new OrExpr2(body.Select(x => x.GetExpr()));
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

        public override IExpression2 GetExpr()
        {
            return new MethodCallExpr2
            (
                new ExistingMethodReference(typeof(System.Threading.Tasks.Parallel).GetMethod("For", new Type[] { typeof(int), typeof(int), typeof(Action<int>) })),
                false,
                new IExpression2[]
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

        public override IExpression2 GetExpr()
        {
            return new MethodCallExpr2
            (
                new ExistingMethodReference(typeof(ExprObjModel.Procedures.SchemeByteArray).GetMethod("get_Bytes", Type.EmptyTypes)),
                true,
                new IExpression2[]
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

        public override IExpression2 GetExpr()
        {
            return new MethodCallExpr2
            (
                new ExistingMethodReference(typeof(ExprObjModel.Procedures.ByteRectangle).GetMethod("get_Array", Type.EmptyTypes)),
                true,
                new IExpression2[]
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

        public override IExpression2 GetExpr()
        {
            return new MethodCallExpr2
            (
                new ExistingMethodReference(typeof(ExprObjModel.Procedures.ByteRectangle).GetMethod("get_Offset", Type.EmptyTypes)),
                true,
                new IExpression2[]
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

        public override IExpression2 GetExpr()
        {
            return new MethodCallExpr2
            (
                new ExistingMethodReference(typeof(ExprObjModel.Procedures.ByteRectangle).GetMethod("get_Width", Type.EmptyTypes)),
                true,
                new IExpression2[]
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

        public override IExpression2 GetExpr()
        {
            return new MethodCallExpr2
            (
                new ExistingMethodReference(typeof(ExprObjModel.Procedures.ByteRectangle).GetMethod("get_Height", Type.EmptyTypes)),
                true,
                new IExpression2[]
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

        public override IExpression2 GetExpr()
        {
            return new MethodCallExpr2
            (
                new ExistingMethodReference(typeof(ExprObjModel.Procedures.ByteRectangle).GetMethod("get_Stride", Type.EmptyTypes)),
                true,
                new IExpression2[]
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

        public override IExpression2 GetExpr()
        {
            return new MethodCallExpr2
            (
                new ExistingMethodReference(typeof(System.Drawing.Imaging.BitmapData).GetMethod("get_Height", Type.EmptyTypes)),
                true,
                new IExpression2[]
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

        public override IExpression2 GetExpr()
        {
            return new MethodCallExpr2
            (
                new ExistingMethodReference(typeof(System.Drawing.Imaging.BitmapData).GetMethod("get_Width", Type.EmptyTypes)),
                true,
                new IExpression2[]
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

        public override IExpression2 GetExpr()
        {
            return new MethodCallExpr2
            (
                new ExistingMethodReference(typeof(System.Drawing.Imaging.BitmapData).GetMethod("get_Stride", Type.EmptyTypes)),
                true,
                new IExpression2[]
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

        public override IExpression2 GetExpr()
        {
            return new MethodCallExpr2
            (
                new ExistingMethodReference(typeof(System.Drawing.Imaging.BitmapData).GetMethod("get_Scan0", Type.EmptyTypes)),
                true,
                new IExpression2[]
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

        public override IExpression2 GetExpr()
        {
            return new NewArrayExpr2
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

        public override IExpression2 GetExpr()
        {
            return new ArrayRefExpr2(array.GetExpr(), index.GetExpr());
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

        public override IExpression2 GetExpr()
        {
            return new ArraySetExpr2(array.GetExpr(), index.GetExpr(), val.GetExpr());
        }
    }

    [Pattern("(array-length $array)")]
    public class ArrayLengthSyntax : ExprSyntax
    {
        [Bind("$array")]
        public ExprSyntax array;

        public override IExpression2 GetExpr()
        {
            return new ArrayLenExpr2(array.GetExpr());
        }
    }

    [Pattern("(poke! $addr $val)")]
    public class PokeSyntax : ExprSyntax
    {
        [Bind("$addr")]
        public ExprSyntax addr;

        [Bind("$val")]
        public ExprSyntax val;

        public override IExpression2 GetExpr()
        {
            return new PokeExpr2(addr.GetExpr(), val.GetExpr());
        }
    }

    [Pattern("(peek $type $addr)")]
    public class PeekSyntax : ExprSyntax
    {
        [Bind("$type")]
        public TypeSyntax peekType;

        [Bind("$addr")]
        public ExprSyntax addr;

        public override IExpression2 GetExpr()
        {
            return new PeekExpr2(addr.GetExpr(), peekType.GetTheType());
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

        public override IExpression2 GetExpr()
        {
            return new MemSetExpr2(dest.GetExpr(), val.GetExpr(), count.GetExpr());
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

        public override IExpression2 GetExpr()
        {
            return new MemCpyExpr2(dest.GetExpr(), src.GetExpr(), count.GetExpr());
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

        public override IExpression2 GetExpr()
        {
            if (clauses.Count == 0) throw new PascalesqueException("Switch must have at least one clause");

            int elseCount = clauses.Where(x => x is SwitchClauseElse).Count();
            if (elseCount > 1) throw new PascalesqueException("Too many else clauses in switch");
            if (elseCount == 1 && !(clauses.Last() is SwitchClauseElse)) throw new PascalesqueException("In switch, else clause must be last");

            IExpression2 elseExpr;
            if (elseCount == 0)
            {
                elseExpr = new EmptyExpr2();
            }
            else
            {
                elseExpr = BeginExpr2.FromList(((SwitchClauseElse)(clauses.Last())).exprs.Select(y => y.GetExpr()));
            }
            return new SwitchExpr2
            (
                expr.GetExpr(),
                elseExpr,
                clauses.OfType<SwitchClauseRegular>().Select(x => new Tuple<IEnumerable<uint>, IExpression2>(x.keys, BeginExpr2.FromList(x.exprs.Select(y => y.GetExpr()))))
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

        public override IExpression2 GetExpr()
        {
            if (clauses.Count == 0) throw new PascalesqueException("Cond must have at least one clause");

            int elseCount = clauses.Where(x => x is CondClauseElse).Count();
            if (elseCount > 1) throw new PascalesqueException("Too many else clauses in cond");
            if (elseCount == 1 && !(clauses.Last() is CondClauseElse)) throw new PascalesqueException("In cond, else clause must be last");

            IExpression2 elseExpr;
            if (elseCount == 0)
            {
                elseExpr = new EmptyExpr2();
            }
            else
            {
                elseExpr = BeginExpr2.FromList(((CondClauseElse)(clauses.Last())).exprs.Select(y => y.GetExpr()));
            }

            int i = clauses.Count;
            while (i > 0)
            {
                --i;
                if (clauses[i] is CondClauseRegular)
                {
                    CondClauseRegular ccr = (CondClauseRegular)(clauses[i]);
                    elseExpr = new IfThenElseExpr2(ccr.cond.GetExpr(), BeginExpr2.FromList(ccr.exprs.Select(x => x.GetExpr())), elseExpr);
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

        public override IExpression2 GetExpr()
        {
            TryClause body1 = Enumerable.Single(clauses.Where(x => x is TryBodyClause));
            TryClause finally1 = Enumerable.SingleOrDefault(clauses.Where(x => x is FinallyBodyClause));

            // this is sloppy because it allows you to put a catch before the body or after the finally...

            return new TryCatchFinallyExpr2
            (
                BeginExpr2.FromList(((TryBodyClause)body1).body.Select(x => x.GetExpr())),
                clauses.OfType<CatchBodyClause>().Select(x => new CatchClause2(x.exceptionType.GetTheType(), x.exceptionName, BeginExpr2.FromList(x.body.Select(y => y.GetExpr())))),
                (finally1 == null) ? new EmptyExpr2() : BeginExpr2.FromList(((FinallyBodyClause)finally1).body.Select(x => x.GetExpr()))
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

        public override IExpression2 GetExpr()
        {
            IExpression2 i = BeginExpr2.FromList(body.Select(x => x.GetExpr()));
            int j = vars.Count;
            while (j > 0)
            {
                --j;
                Symbol temp = new Symbol();
                i = new LetExpr2
                (
                    ExprObjModel.Utils.SingleItem<LetClause2>(vars[j].GetLetClause()),
                    new TryCatchFinallyExpr2
                    (
                        i,
                        Enumerable.Empty<CatchClause2>(),
                        new LetExpr2
                        (
                            new LetClause2[]
                            {
                                new LetClause2
                                (
                                    temp,
                                    new ExistingTypeReference(typeof(IDisposable)),
                                    new CastClassExpr2(new ExistingTypeReference(typeof(IDisposable)), new VarRefExpr2(vars[j].var))
                                )
                            },
                            new IfThenElseExpr2
                            (
                                new IsNullExpr2
                                (
                                    new VarRefExpr2(temp)
                                ),
                                new EmptyExpr2(),
                                new MethodCallExpr2
                                (
                                    new ExistingMethodReference(typeof(IDisposable).GetMethod("Dispose", Type.EmptyTypes)),
                                    true,
                                    new IExpression2[]
                                    {
                                        new VarRefExpr2(temp)
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

        public override IExpression2 GetExpr()
        {
            return new ThrowExpr2(type.GetTheType(), exp.GetExpr());
        }
    }

    [Pattern("(new-exception $msg)")]
    public class NewExceptionSyntax : ExprSyntax
    {
        [Bind("$msg")]
        public string msg;

        public override IExpression2 GetExpr()
        {
            return new NewObjExpr2
            (
                new ExistingConstructorReference(typeof(Exception).GetConstructor(new Type[] { typeof(string) })),
                new IExpression2[]
                {
                    new LiteralExpr2(msg)
                }
            );
        }
    }

    [Pattern("(null? $var)")]
    public class IsNullSyntax : ExprSyntax
    {
        [Bind("$var")]
        public ExprSyntax body;

        public override IExpression2 GetExpr()
        {
            return new IsNullExpr2(body.GetExpr());
        }
    }

    [Pattern("(is? $type $body)")]
    public class IsOfTypeSyntax : ExprSyntax
    {
        [Bind("$type")]
        public TypeSyntax type;

        [Bind("$body")]
        public ExprSyntax body;

        public override IExpression2 GetExpr()
        {
            return new IsInstanceExpr2(body.GetExpr(), type.GetTheType());
        }
    }

    [Pattern("(cast-to $type $body)")]
    public class CastToTypeSyntax : ExprSyntax
    {
        [Bind("$type")]
        public TypeSyntax type;

        [Bind("$body")]
        public ExprSyntax body;

        public override IExpression2 GetExpr()
        {
            return new CastClassExpr2(type.GetTheType(), body.GetExpr());
        }
    }

    [Pattern("(box $expr)")]
    public class BoxSyntax : ExprSyntax
    {
        [Bind("$expr")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new BoxExpr2(expr.GetExpr());
        }
    }

    [Pattern("(unbox $type $expr)")]
    public class UnboxSyntax : ExprSyntax
    {
        [Bind("$type")]
        public TypeSyntax type;

        [Bind("$expr")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new UnboxExpr2(expr.GetExpr(), type.GetTheType());
        }
    }

    [Pattern("(tuple-first $expr)")]
    public class TupleFirstSyntax : ExprSyntax
    {
        [Bind("$expr")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new TupleItemExpr2(expr.GetExpr(), 0);
        }
    }

    [Pattern("(tuple-second $expr)")]
    public class TupleSecondSyntax : ExprSyntax
    {
        [Bind("$expr")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new TupleItemExpr2(expr.GetExpr(), 1);
        }
    }

    [Pattern("(tuple-third $expr)")]
    public class TupleThirdSyntax : ExprSyntax
    {
        [Bind("$expr")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new TupleItemExpr2(expr.GetExpr(), 2);
        }
    }

    [Pattern("(tuple-fourth $expr)")]
    public class TupleFourthSyntax : ExprSyntax
    {
        [Bind("$expr")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new TupleItemExpr2(expr.GetExpr(), 3);
        }
    }

    [Pattern("(tuple-fifth $expr)")]
    public class TupleFifthSyntax : ExprSyntax
    {
        [Bind("$expr")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new TupleItemExpr2(expr.GetExpr(), 4);
        }
    }

    [Pattern("(tuple-sixth $expr)")]
    public class TupleSixthSyntax : ExprSyntax
    {
        [Bind("$expr")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new TupleItemExpr2(expr.GetExpr(), 5);
        }
    }

    [Pattern("(tuple-seventh $expr)")]
    public class TupleSeventhSyntax : ExprSyntax
    {
        [Bind("$expr")]
        public ExprSyntax expr;

        public override IExpression2 GetExpr()
        {
            return new TupleItemExpr2(expr.GetExpr(), 6);
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

#if false
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

        public override IExpression2 GetExpr()
        {
            TypeReference t = type.GetTheType();
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

#endif

    [Pattern("(create-bitmap $x $y)")]
    public class CreateBitmapSyntax : ExprSyntax
    {
        [Bind("$x")]
        public ExprSyntax x;

        [Bind("$y")]
        public ExprSyntax y;

        public override IExpression2 GetExpr()
        {
            return new NewObjExpr2
            (
                new ExistingConstructorReference
                (
                    typeof(System.Drawing.Bitmap).GetConstructor(new Type[] { typeof(int), typeof(int), typeof(System.Drawing.Imaging.PixelFormat) })
                ),
                new IExpression2[]
                {
                    x.GetExpr(),
                    y.GetExpr(),
                    new LiteralExpr2(System.Drawing.Imaging.PixelFormat.Format32bppRgb)
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

        public override IExpression2 GetExpr()
        {
            Symbol bmpvar = new Symbol();
            return new LetExpr2
            (
                new LetClause2[]
                {
                    new LetClause2(bmpvar, new ExistingTypeReference(typeof(System.Drawing.Bitmap)), bmp.GetExpr())
                },
                new MethodCallExpr2
                (
                    new ExistingMethodReference
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
                        )
                    ),
                    true,
                    new IExpression2[]
                    {
                        new VarRefExpr2(bmpvar),
                        new NewObjExpr2
                        (
                            new ExistingConstructorReference
                            (   typeof(System.Drawing.Rectangle).GetConstructor
                                (
                                    new Type[]
                                    {
                                        typeof(System.Drawing.Point),
                                        typeof(System.Drawing.Size)
                                    }
                                )
                            ),
                            new IExpression2[]
                            {
                                new MethodCallExpr2
                                (
                                    new ExistingMethodReference
                                    (
                                        typeof(System.Drawing.Point).GetMethod
                                        (
                                            "get_Empty",
                                            MethodCriteria.IsSpecialName | MethodCriteria.IsPublic | MethodCriteria.IsStatic,
                                            Type.EmptyTypes
                                        )
                                    ),
                                    true,
                                    new IExpression2[] { }
                                ),
                                new MethodCallExpr2
                                (
                                    new ExistingMethodReference
                                    (
                                        typeof(System.Drawing.Image).GetMethod
                                        (
                                            "get_Size",
                                            MethodCriteria.IsSpecialName | MethodCriteria.IsPublic | MethodCriteria.IsNotStatic,
                                            Type.EmptyTypes
                                        )
                                    ),
                                    true,
                                    new IExpression2[]
                                    {
                                        new VarRefExpr2(bmpvar)
                                    }
                                )
                            }
                        ),
                        new LiteralExpr2(lockMode.GetLockMode()),
                        new LiteralExpr2(System.Drawing.Imaging.PixelFormat.Format32bppRgb)
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

        public override IExpression2 GetExpr()
        {
            return new MethodCallExpr2
            (
                new ExistingMethodReference
                (
                    typeof(System.Drawing.Bitmap).GetMethod
                    (
                        "UnlockBits",
                        MethodCriteria.IsPublic | MethodCriteria.IsNotStatic | MethodCriteria.IsNotSpecialName,
                        new Type[]
                        {
                            typeof(System.Drawing.Imaging.BitmapData)
                        }
                    ) 
                ),
                true,
                new IExpression2[]
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
        private static Func<object, ExprObjModel.Option<object>> parserModule = null;

        public static IExpression2 AnalyzeExpr(object obj)
        {
            if (parserExpr == null)
            {
                lock (syncRoot)
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
                IExpression2 t = es.GetExpr();
                return t;
            }
            else
            {
                return null;
            }
        }

        public static TypeReference AnalyzeType(object obj)
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
                TypeReference t = ts.GetTheType();
                return t;
            }
            else
            {
                return null;
            }
        }

        public static ModuleToBuild AnalyzeModule(object obj)
        {
            if (parserModule == null)
            {
                lock (syncRoot)
                {
                    if (parserModule == null)
                    {
                        parserModule = ExprObjModel.Utils.MakeParser(typeof(ModuleSyntax));
                    }
                }
            }
            ExprObjModel.Option<object> k = parserModule(obj);
            if (k is ExprObjModel.Some<object>)
            {
                ModuleSyntax ms = (ModuleSyntax)(((ExprObjModel.Some<object>)k).value);
                ModuleToBuild mtb = ms.GetModuleToBuild();
                return mtb;
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
