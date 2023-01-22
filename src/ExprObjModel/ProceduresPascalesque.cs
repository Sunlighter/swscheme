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
using Pascalesque.One;

namespace ExprObjModel.Procedures
{
    public static partial class ProxyDiscovery
    {
        [SchemeFunction("pascalesque")]
        public static IProcedure MakePascalesqueProcedure(object theProc)
        {
            Pascalesque.One.IExpression expr = Pascalesque.One.Syntax.SyntaxAnalyzer.AnalyzeExpr(theProc);
            
            if (expr == null) throw new SchemeRuntimeException("Unable to parse procedure body");

            if (!(expr is Pascalesque.One.LambdaExpr)) throw new SchemeRuntimeException("Pascalesque procedure body must be a lambda expression");

            return Compiler.CompileAsProcedure((Pascalesque.One.LambdaExpr)expr);
        }
    }
}