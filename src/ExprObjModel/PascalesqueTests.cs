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
using System.Collections.Generic;
using Pascalesque.One;

namespace ExprObjModel.Procedures
{
    public static partial class ProxyDiscovery
    {
        [SchemeFunction("test-Pascalesque.One")]
        public static void TestPascalesque(IGlobalState gs)
        {
#if false
            Pascalesque.One.IExpression expr = new Pascalesque.One.BinaryOpExpr
            (
                Pascalesque.One.BinaryOp.Add,
                new Pascalesque.One.VarRefExpr(new Symbol("x")),
                new Pascalesque.One.LiteralExpr(8)
            );
#endif

            Pascalesque.One.IExpression expr = new Pascalesque.One.LetExpr
            (
                new Pascalesque.One.LetClause[]
                {
                    new Pascalesque.One.LetClause(new Symbol("i"), typeof(int), new Pascalesque.One.VarRefExpr(new Symbol("x"))),
                    new Pascalesque.One.LetClause(new Symbol("sum"), typeof(int), new Pascalesque.One.LiteralExpr(0)),
                },
                new Pascalesque.One.BeginExpr
                (
                    new Pascalesque.One.IExpression[]
                    {
                        new Pascalesque.One.BeginWhileRepeatExpr
                        (
                            new Pascalesque.One.EmptyExpr(),
                            new Pascalesque.One.ComparisonExpr(Pascalesque.Comparison.GreaterThan, new Pascalesque.One.VarRefExpr(new Symbol("i")), new Pascalesque.One.LiteralExpr(0)),
                            new Pascalesque.One.BeginExpr
                            (
                                new Pascalesque.One.IExpression[]
                                {
                                    new Pascalesque.One.VarSetExpr
                                    (
                                        new Symbol("sum"),
                                        new Pascalesque.One.BinaryOpExpr
                                        (
                                            Pascalesque.BinaryOp.Add,
                                            new Pascalesque.One.VarRefExpr(new Symbol("sum")),
                                            new Pascalesque.One.BinaryOpExpr
                                            (
                                                Pascalesque.BinaryOp.Mul,
                                                new Pascalesque.One.VarRefExpr(new Symbol("i")),
                                                new Pascalesque.One.VarRefExpr(new Symbol("i"))
                                            )
                                        )
                                    ),
                                    new Pascalesque.One.VarSetExpr
                                    (
                                        new Symbol("i"),
                                        new Pascalesque.One.BinaryOpExpr
                                        (
                                            Pascalesque.BinaryOp.Sub,
                                            new Pascalesque.One.VarRefExpr(new Symbol("i")),
                                            new Pascalesque.One.LiteralExpr(1)
                                        )
                                    )
                                }
                            )
                        ),
                        new Pascalesque.One.VarRefExpr(new Symbol("sum"))
                    }
                )
            );

            Func<int, int> func = (Func<int, int>)Pascalesque.One.Compiler.CompileRunAndCollect
            (
                typeof(Func<int, int>),
                new Pascalesque.One.ParamInfo[]
                {
                    new Pascalesque.One.ParamInfo(new Symbol("x"), typeof(int))
                },
                typeof(int),
                expr
            );

            for (int q = 0; q < 5; ++q)
            {
                gs.Console.WriteLine("" + q + " -> " + func(q));
            }
        }

        public static unsafe void UnsafeTest(byte[] b, int off, Action<IntPtr, double> func, double dv)
        {
            fixed (byte* bptr = b)
            {
                func((IntPtr)(bptr + off), dv);
            }
        }

        [SchemeFunction("test-pascalesque-2")]
        public static void TestPascalesqueTwo(IGlobalState gs)
        {
            Pascalesque.One.IExpression expr = new Pascalesque.One.PokeExpr
            (
                new Pascalesque.One.VarRefExpr(new Symbol("ptr")),
                new Pascalesque.One.VarRefExpr(new Symbol("val"))
            );

            Action<IntPtr, double> poke = (Action<IntPtr, double>)Pascalesque.One.Compiler.CompileRunAndCollect
            (
                typeof(Action<IntPtr, double>),
                new Pascalesque.One.ParamInfo[]
                {
                    new Pascalesque.One.ParamInfo(new Symbol("ptr"), typeof(IntPtr)),
                    new Pascalesque.One.ParamInfo(new Symbol("val"), typeof(double))
                },
                typeof(void),
                expr
            );

            byte[] b0 = new byte[64];
            for (int i = 0; i < 8; ++i)
            {
                UnsafeTest(b0, i * 8, poke, 1.0 / (i + 1.0));
            }

            Dump(gs, new ByteRange(new SchemeByteArray(b0, DigitOrder.LBLA), 0, b0.Length));
        }

        [SchemeFunction("test-pascalesque-3")]
        public static void TestPascalesqueThree(IGlobalState gs)
        {
            Pascalesque.One.IExpression expr3 = new Pascalesque.One.LambdaExpr
            (
                new Pascalesque.One.ParamInfo[]
                {
                    new Pascalesque.One.ParamInfo(new Symbol("i"), typeof(int))
                },
                new Pascalesque.One.BinaryOpExpr
                (
                    Pascalesque.BinaryOp.Add,
                    new Pascalesque.One.VarRefExpr(new Symbol("i")),
                    new Pascalesque.One.VarRefExpr(new Symbol("j"))
                )
            );

            Func<int, Func<int, int>> makeAdder = (Func<int, Func<int, int>>)Pascalesque.One.Compiler.CompileRunAndCollect
            (
                typeof(Func<int, Func<int, int>>),
                new Pascalesque.One.ParamInfo[]
                {
                    new Pascalesque.One.ParamInfo(new Symbol("j"), typeof(int))
                },
                typeof(Func<int, int>),
                expr3
            );

            Func<int, int> adder5 = makeAdder(5);
            Func<int, int> adder8 = makeAdder(8);
            for (int i = 0; i < 8; ++i)
            {
                gs.Console.WriteLine("" + i + "  " + adder5(i) + "  " + adder8(i));
            }
        }

        [SchemeFunction("test-pascalesque-4")]
        public static void TestPascalesqueFour(IGlobalState gs)
        {
            Pascalesque.One.IExpression expr4 = new Pascalesque.One.LetRecExpr
            (
                new Pascalesque.One.LetClause[]
                {
                    new Pascalesque.One.LetClause
                    (
                        new Symbol("odd?"),
                        typeof(Func<int, bool>),
                        new Pascalesque.One.LambdaExpr
                        (
                            new Pascalesque.One.ParamInfo[]
                            {
                                new Pascalesque.One.ParamInfo(new Symbol("i"), typeof(int))
                            },
                            new Pascalesque.One.IfThenElseExpr
                            (
                                new Pascalesque.One.ComparisonExpr
                                (
                                    Pascalesque.Comparison.LessEqual,
                                    new Pascalesque.One.VarRefExpr(new Symbol("i")),
                                    new Pascalesque.One.LiteralExpr(0)
                                ),
                                new Pascalesque.One.LiteralExpr(false),
                                new Pascalesque.One.InvokeExpr
                                (
                                    new Pascalesque.One.VarRefExpr(new Symbol("even?")),
                                    new Pascalesque.One.IExpression[]
                                    {
                                        new Pascalesque.One.BinaryOpExpr
                                        (
                                            Pascalesque.BinaryOp.Sub,
                                            new Pascalesque.One.VarRefExpr(new Symbol("i")),
                                            new Pascalesque.One.LiteralExpr(1)
                                        )
                                    }
                                )
                            )
                        )
                    ),
                    new Pascalesque.One.LetClause
                    (
                        new Symbol("even?"),
                        typeof(Func<int, bool>),
                        new Pascalesque.One.LambdaExpr
                        (
                            new Pascalesque.One.ParamInfo[]
                            {
                                new Pascalesque.One.ParamInfo(new Symbol("i"), typeof(int))
                            },
                            new Pascalesque.One.IfThenElseExpr
                            (
                                new Pascalesque.One.ComparisonExpr
                                (
                                    Pascalesque.Comparison.LessEqual,
                                    new Pascalesque.One.VarRefExpr(new Symbol("i")),
                                    new Pascalesque.One.LiteralExpr(0)
                                ),
                                new Pascalesque.One.LiteralExpr(true),
                                new Pascalesque.One.InvokeExpr
                                (
                                    new Pascalesque.One.VarRefExpr(new Symbol("odd?")),
                                    new Pascalesque.One.IExpression[]
                                    {
                                        new Pascalesque.One.BinaryOpExpr
                                        (
                                            Pascalesque.BinaryOp.Sub,
                                            new Pascalesque.One.VarRefExpr(new Symbol("i")),
                                            new Pascalesque.One.LiteralExpr(1)
                                        )
                                    }
                                )
                            )
                        )
                    )
                },
                new Pascalesque.One.InvokeExpr
                (
                    new Pascalesque.One.VarRefExpr(new Symbol("odd?")),
                    new Pascalesque.One.IExpression[]
                    {
                        new Pascalesque.One.VarRefExpr(new Symbol("i"))
                    }
                )
            );

            Func<int, bool> f = (Func<int, bool>)Pascalesque.One.Compiler.CompileRunAndCollect
            (
                typeof(Func<int, bool>),
                new Pascalesque.One.ParamInfo[]
                {
                    new Pascalesque.One.ParamInfo(new Symbol("i"), typeof(int))
                },
                typeof(bool),
                expr4
            );

            for (int i = 0; i < 8; ++i)
            {
                gs.Console.WriteLine("" + i + "  " + f(i));
            }
        }

        [SchemeFunction("test-pascalesque-5")]
        public static void TestPascalesqueFive()
        {
            Pascalesque.One.IExpression expr = new Pascalesque.One.LetLoopExpr
            (
                new Symbol("loop"),
                typeof(void),
                new Pascalesque.One.LetClause[]
                {
                    new Pascalesque.One.LetClause
                    (
                        new Symbol("i"),
                        typeof(int),
                        new Pascalesque.One.LiteralExpr(0)
                    )
                },
                new Pascalesque.One.IfThenElseExpr
                (
                    new Pascalesque.One.ComparisonExpr
                    (
                        Pascalesque.Comparison.GreaterEqual,
                        new Pascalesque.One.VarRefExpr(new Symbol("i")),
                        new Pascalesque.One.LiteralExpr(20)
                    ),
                    new Pascalesque.One.EmptyExpr(),
                    new Pascalesque.One.BeginExpr
                    (
                        new Pascalesque.One.IExpression[]
                        {
                            new Pascalesque.One.MethodCallExpr
                            (
                                typeof(Console).GetMethod("WriteLine", new Type[] { typeof(int) }),
                                new Pascalesque.One.IExpression[]
                                {
                                    new Pascalesque.One.VarRefExpr(new Symbol("i"))
                                }
                            ),
                            new Pascalesque.One.InvokeExpr
                            (
                                new VarRefExpr(new Symbol("loop")),
                                new Pascalesque.One.IExpression[]
                                {
                                    new Pascalesque.One.BinaryOpExpr
                                    (
                                        Pascalesque.BinaryOp.Add,
                                        new Pascalesque.One.VarRefExpr(new Symbol("i")),
                                        new Pascalesque.One.LiteralExpr(1)
                                    )
                                }
                            )
                        }
                    )
                )
            );

            Action a = (Action)Pascalesque.One.Compiler.CompileRunAndCollect
            (
                typeof(Action),
                new ParamInfo[0],
                typeof(void),
                expr
            );

            a();
        }

        [SchemeFunction("test-pascalesque-6")]
        public static void TestPascalesqueSix(IGlobalState gs)
        {
            Pascalesque.One.IExpression expr = new Pascalesque.One.SwitchExpr
            (
                new Pascalesque.One.RegardAsExpr(Pascalesque.ConvertTo.UInt, new Pascalesque.One.VarRefExpr(new Symbol("a"))),
                new Pascalesque.One.LiteralExpr(0),
                new Tuple<IEnumerable<uint>, Pascalesque.One.IExpression>[]
                {
                    new Tuple<IEnumerable<uint>, Pascalesque.One.IExpression>
                    (
                        new uint[] { 1u, 3u },
                        new Pascalesque.One.LiteralExpr(100)
                    ),
                    new Tuple<IEnumerable<uint>, Pascalesque.One.IExpression>
                    (
                        new uint[] { 4u, 6u },
                        new Pascalesque.One.LiteralExpr(200)
                    ),
                    new Tuple<IEnumerable<uint>, Pascalesque.One.IExpression>
                    (
                        new uint[] { 7u },
                        new Pascalesque.One.LiteralExpr(300)
                    )
                }
            );

            Func<int, int> f1 = (Func<int, int>)Pascalesque.One.Compiler.CompileRunAndCollect
            (
                typeof(Func<int, int>),
                new Pascalesque.One.ParamInfo[]
                {
                    new Pascalesque.One.ParamInfo(new Symbol("a"), typeof(int))
                },
                typeof(int),
                expr
            );

            for (int i = 0; i < 10; ++i)
            {
                gs.Console.WriteLine("" + i + " -> " + f1(i));
            }
        }
        [SchemeFunction("pascalesque-parse")]
        public static Pascalesque.One.IExpression PascalesqueParse(object obj)
        {
            return Pascalesque.One.Syntax.SyntaxAnalyzer.AnalyzeExpr(obj);
        }

#if false
        [SchemeFunction("pascalesque-parse-let-clause")]
        public static Pascalesque.One.LetClause Pascalesque.OneParseLetClause(object obj)
        {
            return Pascalesque.One.SyntaxAnalyzer.TestParseLetClause(obj);
        }

        [SchemeFunction("Pascalesque.One-parse-type")]
        public static Type Pascalesque.OneParseType(object obj)
        {
            return Pascalesque.One.SyntaxAnalyzer.TestParseType(obj);
        }
#endif
    }
}