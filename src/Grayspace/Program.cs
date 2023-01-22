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
using System.IO;
using ExprObjModel;
using ExprObjModel.Lexing;

namespace Grayspace
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                foreach (string str in args)
                {
                    Grayspace(str);
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

        static void Help()
        {
            Console.WriteLine("Syntax: grayspace <infile>");
        }

        static void Grayspace(string inFile)
        {
            string outFile = inFile + ".html";
            using (FileStream fs = new FileStream(outFile, FileMode.Create, FileAccess.Write, FileShare.None, 32768, FileOptions.SequentialScan))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    PreTextTemplate1 ptt = new PreTextTemplate1();
                    ptt.Title = inFile;
                    ptt.SchemeLines = File.ReadLines(inFile);
                    sw.Write(ptt.TransformText());
                }
            }
        }
    }
}
