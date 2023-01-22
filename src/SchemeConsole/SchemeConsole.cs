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
using System.Reflection;
using System.IO;
using System.Reflection.Emit;
using ExprObjModel;
using ExprObjModel.SyntaxAnalysis;
using ControlledWindowLib.Scheduling;

namespace SchemeConsole
{
    public class ConsoleStringSource : IStringSource
    {
        private IConsole console;

        public ConsoleStringSource(IConsole console)
        {
            this.console = console;
        }

        #region IStringSource Members

        private string current;

        public bool Next(int parenDepth)
        {
            console.Write(parenDepth.ToString("00") + "> ");
            current = console.ReadLine();
            //return !(parenDepth == 0 && current.Trim().Length == 0);
            return true;
        }

        public string Current
        {
            get { return current; }
        }

        #endregion
    }

    public class SchemeConsole
    {
        static void DefineTestProcedure(TopLevel t)
        {
            AssemblyName a = new AssemblyName("temp.dll");
            AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly(a, AssemblyBuilderAccess.RunAndCollect);
            ModuleBuilder mb = ab.DefineDynamicModule("temp.dll");

            Action<int> test = delegate(int i)
            {
                Console.WriteLine("The int you passed was: " + i);
            };

            IProcedure i1 = ProxyGenerator.GenerateProxyFromDelegate(mb, "ActionIntClass", "ActionIntFactory", test);

            t.Define(new Symbol("print-int"), i1);

            Action licenseInfo = delegate()
            {
                Console.WriteLine();
                Console.WriteLine("This program is free software; you can redistribute it and/or modify");
                Console.WriteLine("it under the terms of the GNU General Public License as published by");
                Console.WriteLine("the Free Software Foundation; either version 2 of the License, or");
                Console.WriteLine("(at your option) any later version.");
                Console.WriteLine();
                Console.WriteLine("This program is distributed in the hope that it will be useful,");
                Console.WriteLine("but WITHOUT ANY WARRANTY; without even the implied warranty of");
                Console.WriteLine("MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the");
                Console.WriteLine("GNU General Public License for more details.");
                Console.WriteLine();
                Console.WriteLine("You should have received a copy of the GNU General Public License along");
                Console.WriteLine("with this program; if not, write to the Free Software Foundation, Inc.,");
                Console.WriteLine("51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.");
                Console.WriteLine();
            };

            IProcedure licenseInfoProc = ProxyGenerator.GenerateProxyFromDelegate(mb, "ActionClass", "ActionFactory", licenseInfo);

            t.Define(new Symbol("license-info"), licenseInfoProc);
        }

        [STAThread]
        static void Main(string[] args)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.Clear();
                Console.SetBufferSize(120, 300);
                Console.SetWindowSize(120, 60);
            }
            try
            {
                using (ControlledWindowLib.Scheduling.Scheduler s = new ControlledWindowLib.Scheduling.Scheduler())
                {
                    IConsole c = new PlainConsole();
                    using (GlobalState gs = new GlobalState(s, c))
                    {
                        Tuple<int, int> oldColor = null;
                        Action beginGreen = delegate()
                        {
                            oldColor = c.GetColor();
                            c.SetColor((int)(ConsoleColor.Green), (int)(ConsoleColor.Black));
                        };
                        Action endGreen = delegate()
                        {
                            c.SetColor(oldColor.Item1, oldColor.Item2);
                        };
                        TopLevel t = new TopLevel(gs, true);
                        c.WriteLine("Sunlit World Scheme");
                        c.WriteLine("http://swscheme.codeplex.com/");
                        c.WriteLine("Copyright (c) 2010 by Edward Kiser");
                        c.Write("Enter ");
                        beginGreen();
                        c.Write("(license-info)");
                        endGreen();
                        c.WriteLine(" for licensing information.");
                        c.Write("Enter ");
                        beginGreen();
                        c.Write("exit");
                        endGreen();
                        c.WriteLine(" to quit.");
                        c.WriteLine();

                        DefineTestProcedure(t);

                        SchemeDataReader sdr = new SchemeDataReader(new LexemeSource(new ConsoleStringSource(c)));
                        while (true)
                        {
                            try
                            {
                                object obj = sdr.ReadItem();
                                if (obj is Symbol && ((Symbol)obj).ToString() == "exit") break;
                                Tuple<SignalID, IContinuation> th = Doer.CreateThread(gs);
                                t.PostEval(obj, th.Item2);
                                Tuple<SignalID, object, bool> result = gs.Scheduler.BlockingWaitAny(new SignalID[] { th.Item1 });
                                if (result.Item3)
                                {
                                    if (result.Item2 is Exception)
                                    {
                                        throw ((Exception)result.Item2);
                                    }
                                    Console.Write("** Exception: ");
                                    SchemeDataWriter.WriteItem(result.Item2, ConsoleAppendable.Instance);
                                    Console.Write(" **");
                                    Console.WriteLine();
                                }
                                else
                                {
                                    SchemeDataWriter.WriteItem(result.Item2, ConsoleAppendable.Instance);
                                    Console.WriteLine();
                                }
                            }
                            catch (SchemeRuntimeException sre)
                            {
                                Console.WriteLine("Scheme Runtime Exception:");
                                Console.WriteLine(sre.Message);
                            }
                            catch (UndefinedVariableException uve)
                            {
                                Console.WriteLine("Undefined Variable: " + uve.Message);
                            }
                            catch (SchemeSyntaxException sse)
                            {
                                Console.WriteLine("Scheme Syntax Exception:");
                                Console.WriteLine(sse.Message);
                            }
                            catch (ParsingException pe)
                            {
                                Console.WriteLine("Parsing Exception:");
                                Console.WriteLine(pe.Message);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("General Exception:");
                                Console.WriteLine(e.Message);
                            }
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine();
                Console.WriteLine("***** Exception! *****");
                Console.WriteLine();
                Console.WriteLine(exc.Message);
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    Console.ReadKey(true);
                }
            }
        }
    }
}
