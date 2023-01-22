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
using System.Linq;
using System.Text;
using ExprObjModel;
using System.IO;

namespace SchemeRun
{
#if false
    public class FileStringSource : IStringSource
    {
        private string[] lines;
        int index;

        public FileStringSource(string pathName)
        {
            lines = File.ReadAllLines(pathName);
            index = -1;
        }

        #region IStringSource Members

        public bool Next(int parenDepth)
        {
            ++index;
            return (index < lines.Length);
        }

        public string Current
        {
            get { return lines[index]; }
        }

        #endregion
    }
#endif

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                IConsole c = new PlainConsole();
                using (ControlledWindowLib.Scheduling.Scheduler s = new ControlledWindowLib.Scheduling.Scheduler())
                {
                    using (GlobalState gs = new GlobalState(s, c))
                    {
                        if (args.Length == 0)
                        {
                            throw new ArgumentException("What script file should I run?");
                        }

                        TopLevel t = new TopLevel(gs, true);
                        Deque<object> d = new Deque<object>();
                        for (int i = 1; i < args.Length; ++i)
                        {
                            d.PushBack(new SchemeString(args[i]));
                        }
                        t.Define(new Symbol("args"), d);

                        using (FileStringSource fs = new FileStringSource(args[0]))
                        {
                            SchemeDataReader sdr = new SchemeDataReader(new LexemeSource(fs));

                            while (true)
                            {
                                object obj = sdr.ReadItem();
                                if (obj == null) break;
                                DoerResult result = t.Eval(obj);
                                if (result.IsException)
                                {
                                    if (result.Result is Exception) throw (Exception)(result.Result);
                                    else
                                    {
                                        throw new SchemeRuntimeException("Value Exception: " + SchemeDataWriter.ItemToString(result.Result));
                                    }
                                }
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
                Console.WriteLine(exc);
            }
        }
    }
}
