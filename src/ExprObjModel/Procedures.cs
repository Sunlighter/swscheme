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
using System.Collections.Generic;
using System.Net;
using ControlledWindowLib;
using ControlledWindowLib.Scheduling;

namespace ExprObjModel.Procedures
{
    public static class MathUtils
    {
        public static object Add2(object a, object b)
        {
            if (a is BigInteger)
            {
                BigInteger aa = (BigInteger)a;
                if (b is BigInteger)
                {
                    BigInteger bb = (BigInteger)b;
                    return aa + bb;
                }
                else if (b is BigRational)
                {
                    BigRational bb = (BigRational)b;
                    return aa + bb;
                }
                else if (b is double)
                {
                    double bb = (double)b;
                    return (double)aa + bb;
                }
                else if (b is Vector2 || b is Vertex2 || b is Vector3 || b is Vertex3 || b is Quaternion)
                {
                    throw new SchemeRuntimeException("Type mismatch");
                }
                else
                {
                    throw new SchemeRuntimeException("Unknown type");
                }
            }
            else if (a is BigRational)
            {
                BigRational aa = (BigRational)a;
                if (b is BigInteger)
                {
                    BigInteger bb = (BigInteger)b;
                    return aa + bb;
                }
                else if (b is BigRational)
                {
                    BigRational bb = (BigRational)b;
                    return aa + bb;
                }
                else if (b is double)
                {
                    double bb = (double)b;
                    return (double)aa + bb;
                }
                else if (b is Vector2 || b is Vertex2 || b is Vector3 || b is Vertex3 || b is Quaternion)
                {
                    throw new SchemeRuntimeException("Type mismatch");
                }
                else
                {
                    throw new SchemeRuntimeException("Unknown type");
                }
            }
            else if (a is double)
            {
                double aa = (double)a;
                if (b is BigInteger)
                {
                    BigInteger bb = (BigInteger)b;
                    return aa + (double)bb;
                }
                else if (b is BigRational)
                {
                    BigRational bb = (BigRational)b;
                    return aa + (double)bb;
                }
                else if (b is double)
                {
                    double bb = (double)b;
                    return aa + bb;
                }
                else if (b is Vector2 || b is Vertex2 || b is Vector3 || b is Vertex3 || b is Quaternion)
                {
                    throw new SchemeRuntimeException("Type mismatch");
                }
                else
                {
                    throw new SchemeRuntimeException("Unknown type");
                }
            }
            else if (a is Vector2)
            {
                Vector2 aa = (Vector2)a;
                if (b is Vector2)
                {
                    Vector2 bb = (Vector2)b;
                    return aa + bb;
                }
                else if (b is Vertex2)
                {
                    Vertex2 bb = (Vertex2)b;
                    return aa + bb;
                }
                else if (b is BigInteger || b is BigRational || b is double || b is Vector3 || b is Vertex3 || b is Quaternion)
                {
                    throw new SchemeRuntimeException("Type mismatch");
                }
                else
                {
                    throw new SchemeRuntimeException("Unknown type");
                }
            }
            else if (a is Vertex2)
            {
                Vertex2 aa = (Vertex2)a;
                if (b is Vector2)
                {
                    Vector2 bb = (Vector2)b;
                    return aa + bb;
                }
                else if (b is Vertex2 || b is BigInteger || b is BigRational || b is double || b is Vector3 || b is Vertex3 || b is Quaternion)
                {
                    throw new SchemeRuntimeException("Type mismatch");
                }
                else
                {
                    throw new SchemeRuntimeException("Unknown type");
                }
            }
            else if (a is Vector3)
            {
                Vector3 aa = (Vector3)a;
                if (b is Vector3)
                {
                    Vector3 bb = (Vector3)b;
                    return aa + bb;
                }
                else if (b is Vertex3)
                {
                    Vertex3 bb = (Vertex3)b;
                    return aa + bb;
                }
                else if (b is BigInteger || b is BigRational || b is double || b is Vector2 || b is Quaternion)
                {
                    throw new SchemeRuntimeException("Type mismatch");
                }
                else
                {
                    throw new SchemeRuntimeException("Unknown type");
                }
            }
            else if (a is Vertex3)
            {
                Vertex3 aa = (Vertex3)a;
                if (b is Vector3)
                {
                    Vector3 bb = (Vector3)b;
                    return aa + bb;
                }
                else if (b is BigInteger || b is BigRational || b is double || b is Vertex3 || b is Vector2 || b is Quaternion)
                {
                    throw new SchemeRuntimeException("Type mismatch");
                }
                else
                {
                    throw new SchemeRuntimeException("Unknown type");
                }
            }
            else if (a is Quaternion)
            {
                Quaternion aa = (Quaternion)a;
                if (b is Quaternion)
                {
                    Quaternion bb = (Quaternion)b;
                    return aa + bb;
                }
                else if (b is BigInteger || b is BigRational || b is double || b is Vector3 || b is Vertex3 || b is Vector2)
                {
                    throw new SchemeRuntimeException("Type mismatch");
                }
                else
                {
                    throw new SchemeRuntimeException("Unknown type");
                }
            }
            else
            {
                throw new SchemeRuntimeException("Unknown type");
            }
        }

        public static object Subtract2(object a, object b)
        {
            if (a is BigInteger)
            {
                BigInteger aa = (BigInteger)a;
                if (b is BigInteger)
                {
                    BigInteger bb = (BigInteger)b;
                    return aa - bb;
                }
                else if (b is BigRational)
                {
                    BigRational bb = (BigRational)b;
                    return aa - bb;
                }
                else if (b is double)
                {
                    double bb = (double)b;
                    return (double)aa - bb;
                }
                else if (b is Vector2 || b is Vertex2 || b is Vector3 || b is Vertex3 || b is Quaternion)
                {
                    throw new SchemeRuntimeException("Type mismatch");
                }
                else
                {
                    throw new SchemeRuntimeException("Unknown type");
                }
            }
            else if (a is BigRational)
            {
                BigRational aa = (BigRational)a;
                if (b is BigInteger)
                {
                    BigInteger bb = (BigInteger)b;
                    return aa - bb;
                }
                else if (b is BigRational)
                {
                    BigRational bb = (BigRational)b;
                    return aa - bb;
                }
                else if (b is double)
                {
                    double bb = (double)b;
                    return (double)aa - bb;
                }
                else if (b is Vector2 || b is Vertex2 || b is Vector3 || b is Vertex3 || b is Quaternion)
                {
                    throw new SchemeRuntimeException("Type mismatch");
                }
                else
                {
                    throw new SchemeRuntimeException("Unknown type");
                }
            }
            else if (a is double)
            {
                double aa = (double)a;
                if (b is BigInteger)
                {
                    BigInteger bb = (BigInteger)b;
                    return aa - (double)bb;
                }
                else if (b is BigRational)
                {
                    BigRational bb = (BigRational)b;
                    return aa - (double)bb;
                }
                else if (b is double)
                {
                    double bb = (double)b;
                    return aa - bb;
                }
                else if (b is Vector2 || b is Vertex2 || b is Vector3 || b is Vertex3 || b is Quaternion)
                {
                    throw new SchemeRuntimeException("Type mismatch");
                }
                else
                {
                    throw new SchemeRuntimeException("Unknown type");
                }
            }
            else if (a is Vector2)
            {
                Vector2 aa = (Vector2)a;
                if (b is Vector2)
                {
                    Vector2 bb = (Vector2)b;
                    return aa - bb;
                }
                else if (b is BigInteger || b is BigRational || b is double || b is Vertex2 || b is Vector3 || b is Vertex3 || b is Quaternion)
                {
                    throw new SchemeRuntimeException("Type mismatch");
                }
                else
                {
                    throw new SchemeRuntimeException("Unknown type");
                }
            }
            else if (a is Vertex2)
            {
                Vertex2 aa = (Vertex2)a;
                if (b is Vector2)
                {
                    Vector2 bb = (Vector2)b;
                    return aa - bb;
                }
                else if (b is Vertex2)
                {
                    Vertex2 bb = (Vertex2)b;
                    return aa - bb;
                }
                else if (b is BigInteger || b is BigRational || b is double || b is Vector3 || b is Vertex3 || b is Quaternion)
                {
                    throw new SchemeRuntimeException("Type mismatch");
                }
                else
                {
                    throw new SchemeRuntimeException("Unknown type");
                }
            }
            else if (a is Vector3)
            {
                Vector3 aa = (Vector3)a;
                if (b is Vector3)
                {
                    Vector3 bb = (Vector3)b;
                    return aa - bb;
                }
                else if (b is BigInteger || b is BigRational || b is double || b is Vertex3 || b is Vector2 || b is Vertex2 || b is Quaternion)
                {
                    throw new SchemeRuntimeException("Type mismatch");
                }
                else
                {
                    throw new SchemeRuntimeException("Unknown type");
                }
            }
            else if (a is Vertex3)
            {
                Vertex3 aa = (Vertex3)a;
                if (b is Vector3)
                {
                    Vector3 bb = (Vector3)b;
                    return aa - bb;
                }
                else if (b is Vertex3)
                {
                    Vertex3 bb = (Vertex3)b;
                    return aa - bb;
                }
                else if (b is BigInteger || b is BigRational || b is double || b is Vector2 || b is Vertex2 || b is Quaternion)
                {
                    throw new SchemeRuntimeException("Type mismatch");
                }
                else
                {
                    throw new SchemeRuntimeException("Unknown type");
                }
            }
            else if (a is Quaternion)
            {
                Quaternion aa = (Quaternion)a;
                if (b is Quaternion)
                {
                    Quaternion bb = (Quaternion)b;
                    return aa - bb;
                }
                else if (b is BigInteger || b is BigRational || b is double || b is Vector2 || b is Vertex2 || b is Vector3 || b is Vertex3)
                {
                    throw new SchemeRuntimeException("Type mismatch");
                }
                else
                {
                    throw new SchemeRuntimeException("Unknown type");
                }
            }
            else
            {
                throw new SchemeRuntimeException("Unknown type");
            }
        }

        public static object Multiply2(object a, object b)
        {
            if (a is BigInteger)
            {
                BigInteger aa = (BigInteger)a;
                if (b is BigInteger)
                {
                    BigInteger bb = (BigInteger)b;
                    return aa * bb;
                }
                else if (b is BigRational)
                {
                    BigRational bb = (BigRational)b;
                    return aa * bb;
                }
                else if (b is double)
                {
                    double bb = (double)b;
                    return (double)aa * bb;
                }
                else if (b is Vector2)
                {
                    Vector2 bb = (Vector2)b;
                    return aa * bb;
                }
                else if (b is Vector3)
                {
                    Vector3 bb = (Vector3)b;
                    return aa * bb;
                }
                else if (b is Quaternion)
                {
                    Quaternion bb = (Quaternion)b;
                    return aa * bb;
                }
                else
                {
                    throw new SchemeRuntimeException("Unknown type");
                }
            }
            else if (a is BigRational)
            {
                BigRational aa = (BigRational)a;
                if (b is BigInteger)
                {
                    BigInteger bb = (BigInteger)b;
                    return aa * bb;
                }
                else if (b is BigRational)
                {
                    BigRational bb = (BigRational)b;
                    return aa * bb;
                }
                else if (b is double)
                {
                    double bb = (double)b;
                    return (double)aa * bb;
                }
                else if (b is Vector2)
                {
                    Vector2 bb = (Vector2)b;
                    return aa * bb;
                }
                else if (b is Vector3)
                {
                    Vector3 bb = (Vector3)b;
                    return aa * bb;
                }
                else if (b is Quaternion)
                {
                    Quaternion bb = (Quaternion)b;
                    return aa * bb;
                }
                else
                {
                    throw new SchemeRuntimeException("Unknown type");
                }
            }
            else if (a is double)
            {
                double aa = (double)a;
                if (b is BigInteger)
                {
                    BigInteger bb = (BigInteger)b;
                    return aa * (double)bb;
                }
                else if (b is BigRational)
                {
                    BigRational bb = (BigRational)b;
                    return aa * (double)bb;
                }
                else if (b is double)
                {
                    double bb = (double)b;
                    return aa * bb;
                }
                else if (b is Vector2 || b is Vector3 || b is Quaternion)
                {
                    throw new SchemeRuntimeException("Type mismatch");
                }
                else
                {
                    throw new SchemeRuntimeException("Unknown type");
                }
            }
            else if (a is Vector2)
            {
                Vector2 aa = (Vector2)a;
                if (b is BigInteger)
                {
                    BigInteger bb = (BigInteger)b;
                    return aa * bb;
                }
                else if (b is BigRational)
                {
                    BigRational bb = (BigRational)b;
                    return aa * bb;
                }
                else if (b is double || b is Vector2 || b is Vector3 || b is Quaternion)
                {
                    throw new SchemeRuntimeException("Type mismatch");
                }
                else
                {
                    throw new SchemeRuntimeException("Unknown type");
                }
            }
            else if (a is Vector3)
            {
                Vector3 aa = (Vector3)a;
                if (b is BigInteger)
                {
                    BigInteger bb = (BigInteger)b;
                    return aa * bb;
                }
                else if (b is BigRational)
                {
                    BigRational bb = (BigRational)b;
                    return aa * bb;
                }
                else if (b is double || b is Vector2 || b is Vector3 || b is Quaternion)
                {
                    throw new SchemeRuntimeException("Type mismatch");
                }
                else
                {
                    throw new SchemeRuntimeException("Unknown type");
                }
            }
            else if (a is Quaternion)
            {
                Quaternion aa = (Quaternion)a;
                if (b is BigInteger)
                {
                    BigInteger bb = (BigInteger)b;
                    return aa * bb;
                }
                else if (b is BigRational)
                {
                    BigRational bb = (BigRational)b;
                    return aa * bb;
                }
                else if (b is Quaternion)
                {
                    Quaternion bb = (Quaternion)b;
                    return aa * bb;
                }
                else if (b is double || b is Vector2 || b is Vector3)
                {
                    throw new SchemeRuntimeException("Type mismatch");
                }
                else
                {
                    throw new SchemeRuntimeException("Unknown type");
                }
            }
            else
            {
                throw new SchemeRuntimeException("Unknown type");
            }
        }

        public static object Divide2(object a, object b)
        {
            if (a is BigInteger)
            {
                BigInteger aa = (BigInteger)a;
                if (b is BigInteger)
                {
                    BigInteger bb = (BigInteger)b;
                    return new BigRational(aa, bb);
                }
                else if (b is BigRational)
                {
                    BigRational bb = (BigRational)b;
                    return aa / bb;
                }
                else if (b is double)
                {
                    double bb = (double)b;
                    return (double)aa / bb;
                }
                else if (b is Quaternion)
                {
                    Quaternion bb = (Quaternion)b;
                    return aa / bb;
                }
                else if (b is Vector2 || b is Vector3)
                {
                    throw new SchemeRuntimeException("Type mismatch");
                }
                else
                {
                    throw new SchemeRuntimeException("Unknown type");
                }
            }
            else if (a is BigRational)
            {
                BigRational aa = (BigRational)a;
                if (b is BigInteger)
                {
                    BigInteger bb = (BigInteger)b;
                    return aa / bb;
                }
                else if (b is BigRational)
                {
                    BigRational bb = (BigRational)b;
                    return aa / bb;
                }
                else if (b is double)
                {
                    double bb = (double)b;
                    return (double)aa / bb;
                }
                else if (b is Quaternion)
                {
                    Quaternion bb = (Quaternion)b;
                    return aa / bb;
                }
                else if (b is Vector2 || b is Vector3)
                {
                    throw new SchemeRuntimeException("Type mismatch");
                }
                else
                {
                    throw new SchemeRuntimeException("Unknown type");
                }
            }
            else if (a is double)
            {
                double aa = (double)a;
                if (b is BigInteger)
                {
                    BigInteger bb = (BigInteger)b;
                    return aa / (double)bb;
                }
                else if (b is BigRational)
                {
                    BigRational bb = (BigRational)b;
                    return aa / (double)bb;
                }
                else if (b is double)
                {
                    double bb = (double)b;
                    return aa / bb;
                }
                else if (b is Vector2 || b is Vector3 || b is Quaternion)
                {
                    throw new SchemeRuntimeException("Type mismatch");
                }
                else
                {
                    throw new SchemeRuntimeException("Unknown type");
                }
            }
            else if (a is Vector2)
            {
                Vector2 aa = (Vector2)a;
                if (b is BigInteger)
                {
                    BigInteger bb = (BigInteger)b;
                    return aa / bb;
                }
                else if (b is BigRational)
                {
                    BigRational bb = (BigRational)b;
                    return aa / bb;
                }
                else if (b is double || b is Vector2 || b is Vector3 || b is Quaternion)
                {
                    throw new SchemeRuntimeException("Type mismatch");
                }
                else
                {
                    throw new SchemeRuntimeException("Unknown type");
                }
            }
            else if (a is Vector3)
            {
                Vector3 aa = (Vector3)a;
                if (b is BigInteger)
                {
                    BigInteger bb = (BigInteger)b;
                    return aa / bb;
                }
                else if (b is BigRational)
                {
                    BigRational bb = (BigRational)b;
                    return aa / bb;
                }
                else if (b is double || b is Vector2 || b is Vector3 || b is Quaternion)
                {
                    throw new SchemeRuntimeException("Type mismatch");
                }
                else
                {
                    throw new SchemeRuntimeException("Unknown type");
                }
            }
            else if (a is Quaternion)
            {
                Quaternion aa = (Quaternion)a;
                if (b is BigInteger)
                {
                    BigInteger bb = (BigInteger)b;
                    return aa / bb;
                }
                else if (b is BigRational)
                {
                    BigRational bb = (BigRational)b;
                    return aa / bb;
                }
                else if (b is Quaternion)
                {
                    Quaternion bb = (Quaternion)b;
                    return aa / bb;
                }
                else if (b is double || b is Vector2 || b is Vector3)
                {
                    throw new SchemeRuntimeException("Type mismatch");
                }
                else
                {
                    throw new SchemeRuntimeException("Unknown type");
                }
            }
            else
            {
                throw new SchemeRuntimeException("Unknown type");
            }
        }

        public static object Normalize(object a)
        {
            if (a is BigRational)
            {
                BigRational aa = (BigRational)a;
                if (aa.Denominator == BigInteger.One)
                {
                    return aa.Numerator;
                }
            }
            return a;
        }

        public static object Negate(object a)
        {
            if (a is BigInteger)
            {
                BigInteger aa = (BigInteger)a;
                return -aa;
            }
            else if (a is BigRational)
            {
                BigRational aa = (BigRational)a;
                return -aa;
            }
            else if (a is double)
            {
                double aa = (double)a;
                return -aa;
            }
            else if (a is Vector2)
            {
                Vector2 aa = (Vector2)a;
                return -aa;
            }
            else if (a is Vector3)
            {
                Vector3 aa = (Vector3)a;
                return -aa;
            }
            else if (a is Quaternion)
            {
                Quaternion aa = (Quaternion)a;
                return -aa;
            }
            else
            {
                throw new SchemeRuntimeException("Unknown type");
            }
        }

        public static object Reciprocate(object a)
        {
            if (a is BigInteger)
            {
                BigInteger aa = (BigInteger)a;
                return new BigRational(BigInteger.One, aa);
            }
            else if (a is BigRational)
            {
                BigRational aa = (BigRational)a;
                BigRational aaa = aa.Reciprocal();
                if (aaa.Denominator == BigInteger.One)
                {
                    return aaa.Numerator;
                }
                else return aaa;
            }
            else if (a is double)
            {
                double aa = (double)a;
                return 1.0 / aa;
            }
            else if (a is Quaternion)
            {
                Quaternion aa = (Quaternion)a;
                return aa.Reciprocal();
            }
            else if (a is Vector2 || a is Vector3)
            {
                throw new SchemeRuntimeException("Type mismatch");
            }
            else
            {
                throw new SchemeRuntimeException("Unknown type");
            }
        }

        public static object Gcd(object a, object b)
        {
            if (a is BigInteger)
            {
                BigInteger aa = (BigInteger)a;
                if (b is BigInteger)
                {
                    BigInteger bb = (BigInteger)b;
                    return BigInteger.Gcd(aa, bb);
                }
                else if (b is BigRational)
                {
                    BigRational bb = (BigRational)b;
                    return BigRational.Gcd((BigRational)aa, bb);
                }
                else
                {
                    throw new SchemeRuntimeException("GCD not supported for " + b.GetType());
                }
            }
            else if (a is BigRational)
            {
                BigRational aa = (BigRational)a;
                if (b is BigInteger)
                {
                    BigInteger bb = (BigInteger)b;
                    return BigRational.Gcd(aa, (BigRational)bb);
                }
                else if (b is BigRational)
                {
                    BigRational bb = (BigRational)b;
                    return BigRational.Gcd(aa, bb);
                }
                else
                {
                    throw new SchemeRuntimeException("GCD not supported for " + b.GetType());
                }
            }
            else
            {
                throw new SchemeRuntimeException("GCD not supported for " + a.GetType());
            }
        }

        public static object Lcm(object a, object b)
        {
            if (a is BigInteger)
            {
                BigInteger aa = (BigInteger)a;
                if (b is BigInteger)
                {
                    BigInteger bb = (BigInteger)b;
                    return BigInteger.Lcm(aa, bb);
                }
                else if (b is BigRational)
                {
                    BigRational bb = (BigRational)b;
                    return BigRational.Lcm((BigRational)aa, bb);
                }
                else
                {
                    throw new SchemeRuntimeException("LCM not supported for " + b.GetType());
                }
            }
            else if (a is BigRational)
            {
                BigRational aa = (BigRational)a;
                if (b is BigInteger)
                {
                    BigInteger bb = (BigInteger)b;
                    return BigRational.Lcm(aa, (BigRational)bb);
                }
                else if (b is BigRational)
                {
                    BigRational bb = (BigRational)b;
                    return BigRational.Lcm(aa, bb);
                }
                else
                {
                    throw new SchemeRuntimeException("LCM not supported for " + b.GetType());
                }
            }
            else
            {
                throw new SchemeRuntimeException("LCM not supported for " + a.GetType());
            }
        }

        public static bool LessThan(object a, object b)
        {
            if (a is BigInteger)
            {
                BigInteger aa = (BigInteger)a;
                if (b is BigInteger)
                {
                    BigInteger bb = (BigInteger)b;
                    return aa < bb;
                }
                else if (b is BigRational)
                {
                    BigRational bb = (BigRational)b;
                    return ((BigRational)aa) < bb;
                }
                else if (b is double)
                {
                    double bb = (double)b;
                    if (double.IsNaN(bb)) throw new SchemeRuntimeException("Comparison not supported for NaNs");
                    return ((double)aa) < bb;
                }
                else
                {
                    throw new SchemeRuntimeException("Comparison not supported for " + b.GetType());
                }
            }
            else if (a is BigRational)
            {
                BigRational aa = (BigRational)a;
                if (b is BigInteger)
                {
                    BigInteger bb = (BigInteger)b;
                    return aa < ((BigRational)bb);
                }
                else if (b is BigRational)
                {
                    BigRational bb = (BigRational)b;
                    return aa < bb;
                }
                else if (b is double)
                {
                    double bb = (double)b;
                    if (double.IsNaN(bb)) throw new SchemeRuntimeException("Comparison not supported for NaNs");
                    return ((double)aa) < bb;
                }
                else
                {
                    throw new SchemeRuntimeException("Comparison not supported for " + b.GetType());
                }
            }
            else if (a is double)
            {
                double aa = (double)a;
                if (double.IsNaN(aa)) throw new SchemeRuntimeException("Comparison not supported for NaNs");
                if (b is BigInteger)
                {
                    BigInteger bb = (BigInteger)b;
                    return aa < ((double)bb);
                }
                else if (b is BigRational)
                {
                    BigRational bb = (BigRational)b;
                    return aa < ((double)bb);
                }
                else if (b is double)
                {
                    double bb = (double)b;
                    if (double.IsNaN(bb)) throw new SchemeRuntimeException("Comparison not supported for NaNs");
                    return aa < bb;
                }
                else
                {
                    throw new SchemeRuntimeException("Comparison not supported for " + b.GetType());
                }
            }
            else
            {
                throw new SchemeRuntimeException("Comparison not supported for " + a.GetType());
            }
        }

        public static object Min(object a, object b)
        {
            return LessThan(a, b) ? a : b;
        }

        public static object Max(object a, object b)
        {
            return LessThan(a, b) ? b : a;
        }

        [SchemeFunction("dot")]
        public static object Dot(object a, object b)
        {
            if (a is Vector2)
            {
                Vector2 aa = (Vector2)a;
                if (b is Vector2)
                {
                    Vector2 bb = (Vector2)b;
                    return Normalize(aa.Dot(bb));
                }
                else if (b is Vector3)
                {
                    throw new SchemeRuntimeException("Type mismatch");
                }
                else
                {
                    throw new SchemeRuntimeException("Unknown type");
                }
            }
            else if (a is Vector3)
            {
                Vector3 aa = (Vector3)a;
                if (b is Vector3)
                {
                    Vector3 bb = (Vector3)b;
                    return Normalize(aa.Dot(bb));
                }
                else if (b is Vector2)
                {
                    throw new SchemeRuntimeException("Type mismatch");
                }
                else
                {
                    throw new SchemeRuntimeException("Unknown type");
                }
            }
            else
            {
                throw new SchemeRuntimeException("Unknown type");
            }
        }

        [SchemeFunction("cross")]
        public static object Cross(object a, object b)
        {
            if (a is Vector2)
            {
                Vector2 aa = (Vector2)a;
                if (b is Vector2)
                {
                    Vector2 bb = (Vector2)b;
                    return Normalize(aa.Cross(bb));
                }
                else if (b is Vector3)
                {
                    throw new SchemeRuntimeException("Type mismatch");
                }
                else
                {
                    throw new SchemeRuntimeException("Unknown type");
                }
            }
            else if (a is Vector3)
            {
                Vector3 aa = (Vector3)a;
                if (b is Vector3)
                {
                    Vector3 bb = (Vector3)b;
                    return aa.Cross(bb);
                }
                else if (b is Vector2)
                {
                    throw new SchemeRuntimeException("Type mismatch");
                }
                else
                {
                    throw new SchemeRuntimeException("Unknown type");
                }
            }
            else
            {
                throw new SchemeRuntimeException("Unknown type");
            }
        }
    }

    [Serializable]
    [SchemeSingleton("+")]
    public class AddProc : IProcedure
    {
        public AddProc() { }

        public int Arity { get { return 0; } }
        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            try
            {
                if (argList == null)
                {
                    return new RunnableReturn(k, BigInteger.Zero);
                }
                else
                {
                    object sum = argList.Head;
                    foreach (object arg in FListUtils.ToEnumerable(argList.Tail))
                    {
                        sum = MathUtils.Add2(sum, arg);
                    }
                    return new RunnableReturn(k, MathUtils.Normalize(sum));
                }
            }
            catch (Exception exc)
            {
                return new RunnableThrow(k, exc);
            }
        }
    }

    [Serializable]
    [SchemeSingleton("*")]
    public class MultiplyProc : IProcedure
    {
        public MultiplyProc() { }

        public int Arity { get { return 0; } }
        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            try
            {
                if (argList == null)
                {
                    return new RunnableReturn(k, BigInteger.One);
                }
                else
                {
                    object product = argList.Head;
                    foreach (object arg in FListUtils.ToEnumerable(argList.Tail))
                    {
                        product = MathUtils.Multiply2(product, arg);
                    }
                    return new RunnableReturn(k, MathUtils.Normalize(product));
                }
            }
            catch (Exception exc)
            {
                return new RunnableThrow(k, exc);
            }
        }
    }

    [Serializable]
    [SchemeSingleton("-")]
    public class SubtractProc : IProcedure
    {
        public SubtractProc() { }

        public int Arity { get { return 0; } }
        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            try
            {
                if (argList == null)
                {
                    return new RunnableReturn(k, BigInteger.Zero);
                }
                else
                {
                    object diff = argList.Head;
                    if (argList.Tail == null)
                    {
                        return new RunnableReturn(k, MathUtils.Negate(diff));
                    }
                    else
                    {
                        foreach (object arg in FListUtils.ToEnumerable(argList.Tail))
                        {
                            diff = MathUtils.Subtract2(diff, arg);
                        }
                        return new RunnableReturn(k, MathUtils.Normalize(diff));
                    }
                }
            }
            catch (Exception exc)
            {
                return new RunnableThrow(k, exc);
            }
        }
    }

    [Serializable]
    [SchemeSingleton("/")]
    public class DivideProc : IProcedure
    {
        public DivideProc() { }

        public int Arity { get { return 0; } }
        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            try
            {
                if (argList == null)
                {
                    return new RunnableReturn(k, BigInteger.One);
                }
                else
                {
                    object quotient = argList.Head;
                    if (argList.Tail == null)
                    {
                        return new RunnableReturn(k, MathUtils.Normalize(MathUtils.Reciprocate(quotient)));
                    }
                    else
                    {
                        foreach (object arg in FListUtils.ToEnumerable(argList.Tail))
                        {
                            quotient = MathUtils.Divide2(quotient, arg);
                        }
                        return new RunnableReturn(k, MathUtils.Normalize(quotient));
                    }
                }
            }
            catch (Exception exc)
            {
                return new RunnableThrow(k, exc);
            }
        }
    }

    [Serializable]
    [SchemeSingleton("gcd")]
    public class GcdProc : IProcedure
    {
        public GcdProc() { }

        public int Arity { get { return 1; } }
        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            try
            {
                if (argList == null)
                {
                    return new RunnableReturn(k, BigInteger.One);
                }
                else
                {
                    object result = argList.Head;
                    foreach (object arg in FListUtils.ToEnumerable(argList.Tail))
                    {
                        result = MathUtils.Gcd(result, arg);
                    }
                    return new RunnableReturn(k, MathUtils.Normalize(result));
                }
            }
            catch (Exception exc)
            {
                return new RunnableThrow(k, exc);
            }
        }
    }

    [Serializable]
    [SchemeSingleton("lcm")]
    public class LcmProc : IProcedure
    {
        public LcmProc() { }

        public int Arity { get { return 1; } }
        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            try
            {
                if (argList == null)
                {
                    return new RunnableReturn(k, BigInteger.One);
                }
                else
                {
                    object result = argList.Head;
                    foreach (object arg in FListUtils.ToEnumerable(argList.Tail))
                    {
                        result = MathUtils.Lcm(result, arg);
                    }
                    return new RunnableReturn(k, MathUtils.Normalize(result));
                }
            }
            catch (Exception exc)
            {
                return new RunnableThrow(k, exc);
            }
        }
    }

    [Serializable]
    [SchemeSingleton("min")]
    public class MinProc : IProcedure
    {
        public MinProc() { }

        public int Arity { get { return 1; } }
        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            try
            {
                if (argList == null)
                {
                    return new RunnableReturn(k, BigInteger.One);
                }
                else
                {
                    object min = argList.Head;
                    foreach (object arg in FListUtils.ToEnumerable(argList.Tail))
                    {
                        min = MathUtils.Min(min, arg);
                    }
                    return new RunnableReturn(k, min);
                }
            }
            catch (Exception exc)
            {
                return new RunnableThrow(k, exc);
            }
        }
    }

    [Serializable]
    [SchemeSingleton("max")]
    public class MaxProc : IProcedure
    {
        public MaxProc() { }

        public int Arity { get { return 1; } }
        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            try
            {
                if (argList == null)
                {
                    return new RunnableReturn(k, BigInteger.One);
                }
                else
                {
                    object max = argList.Head;
                    foreach (object arg in FListUtils.ToEnumerable(argList.Tail))
                    {
                        max = MathUtils.Max(max, arg);
                    }
                    return new RunnableReturn(k, max);
                }
            }
            catch (Exception exc)
            {
                return new RunnableThrow(k, exc);
            }
        }
    }

    [Serializable]
    [SchemeSingleton("<")]
    public class AscendingProc : IProcedure
    {
        public AscendingProc() { }

        public int Arity { get { return 1; } }
        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            try
            {
                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException("< : Insufficient arguments"));
                }
                else
                {
                    object basis = argList.Head;
                    foreach(object arg in FListUtils.ToEnumerable(argList.Tail))
                    {
                        if (MathUtils.LessThan(basis, arg))
                        {
                            basis = arg;
                        }
                        else
                        {
                            return new RunnableReturn(k, false);
                        }
                    }
                    return new RunnableReturn(k, true);
                }
            }
            catch (Exception exc)
            {
                return new RunnableThrow(k, exc);
            }
        }
    }

    [Serializable]
    [SchemeSingleton(">=")]
    public class NonAscendingProc : IProcedure
    {
        public NonAscendingProc() { }

        public int Arity { get { return 2; } }
        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            try
            {
                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException("< : Insufficient arguments"));
                }
                else
                {
                    object basis = argList.Head;
                    foreach (object arg in FListUtils.ToEnumerable(argList.Tail))
                    {
                        if (!MathUtils.LessThan(basis, arg))
                        {
                            basis = arg;
                        }
                        else
                        {
                            return new RunnableReturn(k, false);
                        }
                    }
                    return new RunnableReturn(k, true);
                }
            }
            catch (Exception exc)
            {
                return new RunnableThrow(k, exc);
            }
        }
    }

    [Serializable]
    [SchemeSingleton(">")]
    public class DescendingProc : IProcedure
    {
        public DescendingProc() { }

        public int Arity { get { return 2; } }
        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            try
            {
                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException("< : Insufficient arguments"));
                }
                else
                {
                    object basis = argList.Head;
                    foreach (object arg in FListUtils.ToEnumerable(argList.Tail))
                    {
                        if (MathUtils.LessThan(arg, basis))
                        {
                            basis = arg;
                        }
                        else
                        {
                            return new RunnableReturn(k, false);
                        }
                    }
                    return new RunnableReturn(k, true);
                }
            }
            catch (Exception exc)
            {
                return new RunnableThrow(k, exc);
            }
        }
    }

    [Serializable]
    [SchemeSingleton("<=")]
    public class NonDescendingProc : IProcedure
    {
        public NonDescendingProc() { }

        public int Arity { get { return 2; } }
        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            try
            {
                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException("< : Insufficient arguments"));
                }
                else
                {
                    object basis = argList.Head;
                    foreach (object arg in FListUtils.ToEnumerable(argList.Tail))
                    {
                        if (!MathUtils.LessThan(arg, basis))
                        {
                            basis = arg;
                        }
                        else
                        {
                            return new RunnableReturn(k, false);
                        }
                    }
                    return new RunnableReturn(k, true);
                }
            }
            catch (Exception exc)
            {
                return new RunnableThrow(k, exc);
            }
        }
    }

    [SchemeSingleton("logand")]
    public class LogAnd : IProcedure
    {
        public LogAnd() { }

        public int Arity { get { return 0; } }
        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            BigInteger z = BigInteger.MinusOne;
            foreach (object obj in FListUtils.ToEnumerable(argList))
            {
                if (!(obj is BigInteger)) throw new SchemeRuntimeException("Type mismatch: logand requires integers");
                z = z & ((BigInteger)obj);
            }
            return new RunnableReturn(k, z);
        }
    }

    [SchemeSingleton("logior")]
    public class LogIor : IProcedure
    {
        public LogIor() { }

        public int Arity { get { return 0; } }
        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            BigInteger z = BigInteger.Zero;
            foreach (object obj in FListUtils.ToEnumerable(argList))
            {
                if (!(obj is BigInteger)) throw new SchemeRuntimeException("Type mismatch: logand requires integers");
                z = z | ((BigInteger)obj);
            }
            return new RunnableReturn(k, z);
        }
    }

    [SchemeSingleton("logxor")]
    public class LogXor : IProcedure
    {
        public LogXor() { }

        public int Arity { get { return 0; } }
        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            BigInteger z = BigInteger.Zero;
            foreach (object obj in FListUtils.ToEnumerable(argList))
            {
                if (!(obj is BigInteger)) throw new SchemeRuntimeException("Type mismatch: logand requires integers");
                z = z ^ ((BigInteger)obj);
            }
            return new RunnableReturn(k, z);
        }
    }

    [SchemeSingleton("call-with-current-continuation")]
    public class CallWithCurrentContinuation : IProcedure
    {
        public CallWithCurrentContinuation() { }

        private class ContinuationProcedure : IProcedure
        {
            private IContinuation kInner;

            public ContinuationProcedure(IContinuation kInner)
            {
                this.kInner = kInner;
            }

            public int Arity { get { return 1; } }
            public bool More { get { return false; } }

            public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
            {
                return ContinuationUtilities.MoveToAndReturn(k, kInner, argList.Head);
            }

#if false
            public IRunnableStep InjectException(object exc, IContinuation k)
            {
                return ContinuationUtilities.MoveToAndThrow(k, kInner, exc);
            }
#endif
        }

        #region IProcedure Members

        public int Arity { get { return 1; } }
        public bool More { get { return false; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            IProcedure proc1 = (IProcedure)argList.Head;
            return new RunnableCall(proc1, new FList<object>(new ContinuationProcedure(k)), k);
        }

        #endregion
    }

    [SchemeSingleton("call-with-current-exception-handler")]
    public class CallWithCurrentExceptionHandler : IProcedure
    {
        public CallWithCurrentExceptionHandler() { }

        private class ThrowProcedure : IProcedure
        {
            private IContinuation kInner;

            public ThrowProcedure(IContinuation kInner)
            {
                this.kInner = kInner;
            }

            public int Arity { get { return 1; } }
            public bool More { get { return false; } }

            public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
            {
                return ContinuationUtilities.MoveToAndThrow(k, kInner, argList.Head);
            }
        }

        #region IProcedure Members

        public int Arity { get { return 1; } }
        public bool More { get { return false; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            IProcedure proc1 = (IProcedure)(argList.Head);
            return new RunnableCall(proc1, new FList<object>(new ThrowProcedure(k)), k);
        }

        #endregion
    }

    [SchemeSingleton("throw")]
    public class Throw : IProcedure
    {
        public Throw() { }

        public int Arity { get { return 1; } }
        public bool More { get { return false; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            return new RunnableThrow(k, argList.Head);
        }
    }

    [Serializable]
    [SchemeSingleton("map")]
    public class Map : IProcedure
    {
        public Map() { }

        private static void BuildCall(FList<object> procArgLists, out FList<object> argList, out FList<object> procArgListsRest)
        {
            argList = null;
            procArgListsRest = null;
            while (procArgLists != null)
            {
                ConsCell hcc = (ConsCell)procArgLists.Head;

                argList = new FList<object>(hcc.car, argList);
                procArgListsRest = new FList<object>(hcc.cdr, procArgListsRest);
                procArgLists = procArgLists.Tail;
            }
            argList = FListUtils.Reverse(argList);
            procArgListsRest = FListUtils.Reverse(procArgListsRest);
        }

        private class MapPartialContinuation : IPartialContinuation
        {
            private IPartialContinuation k;
            private IProcedure proc;
            private FList<object> procArgLists;
            private object resultList;

            public MapPartialContinuation(IPartialContinuation k, IProcedure proc, FList<object> procArgLists, object resultList)
            {
                this.k = k;
                this.proc = proc;
                this.procArgLists = procArgLists;
                this.resultList = resultList;
            }

            #region IPartialContinuation Members

            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<MapPartialContinuation, MapContinuation>(this, delegate() { return new MapContinuation(k.Attach(theNewBase, a), proc, procArgLists, resultList); });
            }

            #endregion
        }

        private class MapContinuation : IContinuation
        {
            private IContinuation k;
            private IProcedure proc;
            private FList<object> procArgLists;
            private object resultList;

            public MapContinuation(IContinuation k, IProcedure proc, FList<object> procArgLists, object resultList)
            {
                this.k = k;
                this.proc = proc;
                this.procArgLists = procArgLists;
                this.resultList = resultList;
            }

            #region IContinuation Members

            public IRunnableStep Return(IGlobalState gs, object v)
            {
                object resultList2 = new ConsCell(v, resultList);
                if (ConsCell.IsEmptyList(procArgLists.Head))
                {
                    ConsCell.Reverse(ref resultList2);
                    return new RunnableReturn(k, resultList2);
                }
                else
                {
                    FList<object> argList = null;
                    FList<object> procArgListsRest = null;
                    BuildCall(procArgLists, out argList, out procArgListsRest);
                    return new RunnableCall(proc, argList, new MapContinuation(k, proc, procArgListsRest, resultList2));
                }
            }

            public IRunnableStep Throw(IGlobalState gs, object exc) { return new RunnableThrow(k, exc); }

            public IContinuation Parent { get { return k; } }
            public IProcedure EntryProc { get { return null; } }
            public IProcedure ExitProc { get { return null; } }

            public IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
            {
                return a.Assoc<MapContinuation, MapPartialContinuation>(this, delegate() { return new MapPartialContinuation(k.PartialCapture(baseMark, a), proc, procArgLists, resultList); });
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }
            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }

            #endregion
        }

        #region IProcedure Members

        public int Arity
        {
            get { return 2; }
        }

        public bool More
        {
            get { return true; }
        }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            IProcedure proc = (IProcedure)argList.Head;
            FList<object> procArgLists = argList.Tail;
            if (ConsCell.IsEmptyList(procArgLists.Head))
            {
                return new RunnableReturn(k, SpecialValue.EMPTY_LIST);
            }
            else
            {
                FList<object> argList2 = null;
                FList<object> procArgListsRest = null;
                BuildCall(procArgLists, out argList2, out procArgListsRest);
                return new RunnableCall(proc, argList2, new MapContinuation(k, proc, procArgListsRest, SpecialValue.EMPTY_LIST));
            }
        }

        #endregion
    }

    [Serializable]
    [SchemeSingleton("apply")]
    public class Apply : IProcedure
    {
        public Apply() { }

        #region IProcedure Members

        public int Arity { get { return 2; } }

        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            IProcedure proc = (IProcedure)argList.Head;
            FList<object> args = argList.Tail;

            if (args == null) throw new SchemeRuntimeException("apply requires two arguments");

            FList<object> args2 = null;
            while (args.Tail != null)
            {
                args2 = new FList<object>(args.Head, args2);
                args = args.Tail;
            }

            object list = args.Head;
            while (!ConsCell.IsEmptyList(list))
            {
                if (list is ConsCell)
                {
                    args2 = new FList<object>(ConsCell.Car(list), args2);
                    list = ConsCell.Cdr(list);
                }
                else throw new SchemeRuntimeException("apply expects a proper list");
            }

            args2 = FListUtils.Reverse(args2);

            return new RunnableCall(proc, args2, k);
        }

        #endregion
    }

    [Serializable]
    [SchemeSingleton("string-append")]
    public class StringAppend : IProcedure
    {
        public StringAppend() { }

        #region IProcedure Members

        public int Arity { get { return 0; } }

        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            StringBuilder sb = new StringBuilder();
            while (argList != null)
            {
                if (argList.Head is SchemeString)
                {
                    sb.Append(((SchemeString)argList.Head).TheString);
                }
                else throw new SchemeRuntimeException("string-append expects strings");
                argList = argList.Tail;
            }
            return new RunnableReturn(k, new SchemeString(sb.ToString()));
        }

        #endregion
    }

    [SchemeSingleton("string<?")]
    public class StringLessThan : IProcedure
    {
        public StringLessThan() { }

        public int Arity { get { return 2; } }
        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            if (!(argList.Head is SchemeString)) throw new SchemeRuntimeException("string<? expects string arguments");
            SchemeString a = (SchemeString)(argList.Head);
            FList<object> f = argList.Tail;
            while (f != null)
            {
                if (!(f.Head is SchemeString)) throw new SchemeRuntimeException("string<? expects string arguments");
                SchemeString b = (SchemeString)(f.Head);
                if (!(a < b)) return new RunnableReturn(k, false);
                a = b;                
                f = f.Tail;
            }
            return new RunnableReturn(k, true);
        }
    }

    [SchemeSingleton("string-ci<?")]
    public class StringCiLessThan : IProcedure
    {
        public StringCiLessThan() { }

        public int Arity { get { return 2; } }
        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            if (!(argList.Head is SchemeString)) throw new SchemeRuntimeException("string<? expects string arguments");
            SchemeString a = (SchemeString)(argList.Head);
            FList<object> f = argList.Tail;
            while (f != null)
            {
                if (!(f.Head is SchemeString)) throw new SchemeRuntimeException("string<? expects string arguments");
                SchemeString b = (SchemeString)(f.Head);
                if (!(string.Compare(a.TheString, b.TheString, true) < 0)) return new RunnableReturn(k, false);
                a = b;
                f = f.Tail;
            }
            return new RunnableReturn(k, true);
        }
    }

    [SchemeSingleton("string>?")]
    public class StringGreaterThan : IProcedure
    {
        public StringGreaterThan() { }

        public int Arity { get { return 2; } }
        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            if (!(argList.Head is SchemeString)) throw new SchemeRuntimeException("string<? expects string arguments");
            SchemeString a = (SchemeString)(argList.Head);
            FList<object> f = argList.Tail;
            while (f != null)
            {
                if (!(f.Head is SchemeString)) throw new SchemeRuntimeException("string<? expects string arguments");
                SchemeString b = (SchemeString)(f.Head);
                if (!(a > b)) return new RunnableReturn(k, false);
                a = b;
                f = f.Tail;
            }
            return new RunnableReturn(k, true);
        }
    }

    [SchemeSingleton("string-ci>?")]
    public class StringCiGreaterThan : IProcedure
    {
        public StringCiGreaterThan() { }

        public int Arity { get { return 2; } }
        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            if (!(argList.Head is SchemeString)) throw new SchemeRuntimeException("string<? expects string arguments");
            SchemeString a = (SchemeString)(argList.Head);
            FList<object> f = argList.Tail;
            while (f != null)
            {
                if (!(f.Head is SchemeString)) throw new SchemeRuntimeException("string<? expects string arguments");
                SchemeString b = (SchemeString)(f.Head);
                if (!(string.Compare(a.TheString, b.TheString, true) > 0)) return new RunnableReturn(k, false);
                a = b;
                f = f.Tail;
            }
            return new RunnableReturn(k, true);
        }
    }

    [SchemeSingleton("string<=?")]
    public class StringNotGreaterThan : IProcedure
    {
        public StringNotGreaterThan() { }

        public int Arity { get { return 2; } }
        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            if (!(argList.Head is SchemeString)) throw new SchemeRuntimeException("string<? expects string arguments");
            SchemeString a = (SchemeString)(argList.Head);
            FList<object> f = argList.Tail;
            while (f != null)
            {
                if (!(f.Head is SchemeString)) throw new SchemeRuntimeException("string<? expects string arguments");
                SchemeString b = (SchemeString)(f.Head);
                if (!(a <= b)) return new RunnableReturn(k, false);
                a = b;
                f = f.Tail;
            }
            return new RunnableReturn(k, true);
        }
    }

    [SchemeSingleton("string-ci<=?")]
    public class StringCiNotGreaterThan : IProcedure
    {
        public StringCiNotGreaterThan() { }

        public int Arity { get { return 2; } }
        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            if (!(argList.Head is SchemeString)) throw new SchemeRuntimeException("string<? expects string arguments");
            SchemeString a = (SchemeString)(argList.Head);
            FList<object> f = argList.Tail;
            while (f != null)
            {
                if (!(f.Head is SchemeString)) throw new SchemeRuntimeException("string<? expects string arguments");
                SchemeString b = (SchemeString)(f.Head);
                if (!(string.Compare(a.TheString, b.TheString, true) <= 0)) return new RunnableReturn(k, false);
                a = b;
                f = f.Tail;
            }
            return new RunnableReturn(k, true);
        }
    }

    [SchemeSingleton("string>=?")]
    public class StringNotLessThan : IProcedure
    {
        public StringNotLessThan() { }

        public int Arity { get { return 2; } }
        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            if (!(argList.Head is SchemeString)) throw new SchemeRuntimeException("string<? expects string arguments");
            SchemeString a = (SchemeString)(argList.Head);
            FList<object> f = argList.Tail;
            while (f != null)
            {
                if (!(f.Head is SchemeString)) throw new SchemeRuntimeException("string<? expects string arguments");
                SchemeString b = (SchemeString)(f.Head);
                if (!(a <= b)) return new RunnableReturn(k, false);
                a = b;
                f = f.Tail;
            }
            return new RunnableReturn(k, true);
        }
    }

    [SchemeSingleton("string-ci>=?")]
    public class StringCiNotLessThan : IProcedure
    {
        public StringCiNotLessThan() { }

        public int Arity { get { return 2; } }
        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            if (!(argList.Head is SchemeString)) throw new SchemeRuntimeException("string<? expects string arguments");
            SchemeString a = (SchemeString)(argList.Head);
            FList<object> f = argList.Tail;
            while (f != null)
            {
                if (!(f.Head is SchemeString)) throw new SchemeRuntimeException("string<? expects string arguments");
                SchemeString b = (SchemeString)(f.Head);
                if (!(string.Compare(a.TheString, b.TheString, true) >= 0)) return new RunnableReturn(k, false);
                a = b;
                f = f.Tail;
            }
            return new RunnableReturn(k, true);
        }
    }

    [Serializable]
    [SchemeSingleton("list")]
    public class ListProc : IProcedure
    {
        public ListProc() { }

        #region IProcedure Members

        public int Arity { get { return 0; } }

        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            object result = SpecialValue.EMPTY_LIST;
            argList = FListUtils.Reverse(argList);

            while (argList != null)
            {
                result = new ConsCell(argList.Head, result);
                argList = argList.Tail;
            }

            return new RunnableReturn(k, result);
        }

        #endregion
    }

    [Serializable]
    [SchemeSingleton("filter")]
    public class FilterProc : IProcedure
    {
        public FilterProc() { }

        #region IProcedure Members

        public int Arity { get { return 2; } }

        public bool More { get { return false; } }

        private class FilterPartialContinuation : IPartialContinuation
        {
            private object itemUnderConsideration;
            private object remainingInputList;
            private object outputList;
            private IProcedure filterProc;
            private IPartialContinuation k;

            public FilterPartialContinuation(object itemUnderConsideration, object remainingInputList, object outputList, IProcedure filterProc, IPartialContinuation k)
            {
                this.itemUnderConsideration = itemUnderConsideration;
                this.remainingInputList = remainingInputList;
                this.outputList = outputList;
                this.filterProc = filterProc;
                this.k = k;
            }

            #region IPartialContinuation Members

            public IContinuation Attach(IContinuation theNewBase, ItemAssociation a)
            {
                return a.Assoc<FilterPartialContinuation, FilterContinuation>(this, delegate() { return new FilterContinuation(itemUnderConsideration, remainingInputList, outputList, filterProc, k.Attach(theNewBase, a)); });
            }

            #endregion
        }

        private class FilterContinuation : IContinuation
        {
            private object itemUnderConsideration;
            private object remainingInputList;
            private object outputList;
            private IProcedure filterProc;
            private IContinuation k;

            public FilterContinuation(object itemUnderConsideration, object remainingInputList, object outputList, IProcedure filterProc, IContinuation k)
            {
                this.itemUnderConsideration = itemUnderConsideration;
                this.remainingInputList = remainingInputList;
                this.outputList = outputList;
                this.filterProc = filterProc;
                this.k = k;
            }

            #region IContinuation Members

            public IRunnableStep Return(IGlobalState gs, object v)
            {
                if (remainingInputList is ConsCell)
                {
                    ConsCell ccRemainingInputList = (ConsCell)remainingInputList;
                    object newInputList = ccRemainingInputList.cdr;

                    object newOutputList = outputList;
                    if (IfThenElseSource.IsTrue(v))
                    {
                        newOutputList = new ConsCell(itemUnderConsideration, newOutputList);
                    }
                    object newItemUnderConsideration = ccRemainingInputList.car;
                    return new RunnableCall
                    (
                        filterProc,
                        new FList<object>(newItemUnderConsideration),
                        new FilterContinuation(newItemUnderConsideration, newInputList, newOutputList, filterProc, k)
                    );
                }
                else if (ConsCell.IsEmptyList(remainingInputList))
                {
                    object newOutputList = outputList;
                    if (IfThenElseSource.IsTrue(v))
                    {
                        newOutputList = new ConsCell(itemUnderConsideration, newOutputList);
                    }

                    ConsCell.Reverse(ref newOutputList);

                    return new RunnableReturn(k, newOutputList);
                }
                else
                {
                    return new RunnableThrow(k, new SchemeRuntimeException("Attempt to filter improper list!"));
                }
            }

            public IRunnableStep Throw(IGlobalState gs, object exc) { return new RunnableThrow(k, exc); }

            public IContinuation Parent { get { return k; } }
            public IProcedure EntryProc { get { return null; } }
            public IProcedure ExitProc { get { return null; } }

            public IPartialContinuation PartialCapture(Symbol baseMark, ItemAssociation a)
            {
                return a.Assoc<FilterContinuation, FilterPartialContinuation>(this, delegate() { return new FilterPartialContinuation(itemUnderConsideration, remainingInputList, outputList, filterProc, k.PartialCapture(baseMark, a)); });
            }

            public Box DynamicLookup(Symbol s) { return k.DynamicLookup(s); }
            public EnvSpec DynamicEnv { get { return k.DynamicEnv; } }


            #endregion
        }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            try
            {
                IProcedure filterProc = (IProcedure)argList.Head;
                object list = argList.Tail.Head;
                if (argList.Tail.Tail != null) throw new SchemeRuntimeException("filter: Too many arguments");

                if (ConsCell.IsEmptyList(list))
                {
                    return new RunnableReturn(k, list);
                }
                else if (list is ConsCell)
                {
                    ConsCell ccList = (ConsCell)list;

                    return new RunnableCall
                    (
                        filterProc,
                        new FList<object>(ccList.car),
                        new FilterContinuation(ccList.car, ccList.cdr, SpecialValue.EMPTY_LIST, filterProc, k)
                    );
                }
                else
                {
                    return new RunnableThrow(k, new SchemeRuntimeException("filter: Argument 2 is not a list"));
                }
            }
            catch (Exception exc)
            {
                return new RunnableThrow(k, exc);
            }
        }

        #endregion
    }

    public static partial class ProxyDiscovery
    {
        [SchemeFunction("symbol->string")]
        public static string SymbolToString(Symbol s)
        {
            return s.ToString();
        }

        [SchemeFunction("string->symbol")]
        public static Symbol StringToSymbol(string s)
        {
            return new Symbol(s);
        }

        [SchemeFunction("symbol?")]
        public static bool IsSymbol(object obj)
        {
            return (obj is Symbol);
        }

        [SchemeFunction("char->integer")]
        public static int CharToInteger(char ch)
        {
            return (int)ch;
        }

        [SchemeFunction("integer->char")]
        public static char IntegerToChar(int i)
        {
            return (char)i;
        }

        [SchemeFunction("char?")]
        public static bool IsChar(object obj)
        {
            return obj is char;
        }

        [SchemeFunction("integer?")]
        public static bool IsInteger(object obj)
        {
            return obj is BigInteger;
        }

        [SchemeFunction("=")]
        public static bool IsNumericEqual(object a, object b)
        {
            if (a is BigInteger && b is BigInteger)
            {
                return ((BigInteger)a) == ((BigInteger)b);
            }
            else if (a is BigRational && b is BigRational)
            {
                return ((BigRational)a) == ((BigRational)b);
            }
            else if (a is double && b is double)
            {
                return ((double)a) == ((double)b);
            }
            else if (a is DisposableID && b is DisposableID)
            {
                return ((DisposableID)a) == ((DisposableID)b);
            }
            else if (a is AsyncID && b is AsyncID)
            {
                return ((AsyncID)a) == ((AsyncID)b);
            }
            else return false;
        }

        [SchemeFunction("procedure?")]
        public static bool IsProcedure(object obj)
        {
            return (obj is IProcedure);
        }

        [SchemeFunction("arity")]
        public static int Arity(IProcedure proc)
        {
            return proc.Arity;
        }

        [SchemeFunction("more-arity?")]
        public static bool MoreArity(IProcedure proc)
        {
            return proc.More;
        }

        [SchemeFunction("this-exe")]
        public static string ProxyTest6()
        {
            return System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
        }

        [SchemeFunction("string?")]
        public static bool IsString(object obj)
        {
            return (obj is SchemeString);
        }

        [SchemeFunction("string-length")]
        public static int StringLength(SchemeString str)
        {
            return str.Length;
        }

        [SchemeFunction("string-ref")]
        public static char StringRef(SchemeString str, [OverflowMode(BigMath.OverflowBehavior.ThrowException)] int pos)
        {
            return str[pos];
        }

        [SchemeFunction("string-set!")]
        public static void StringSet(SchemeString str, [OverflowMode(BigMath.OverflowBehavior.ThrowException)] int pos, char ch)
        {
            str[pos] = ch;
        }

        [SchemeFunction("make-string")]
        public static SchemeString MakeString([OverflowMode(BigMath.OverflowBehavior.ThrowException)] int len, char init)
        {
            char[] ch = new char[len];
            for (int i = 0; i < len; ++i) ch[i] = init;
            return new SchemeString(ch);
        }

        [SchemeFunction("substring")]
        public static SchemeString Substring(SchemeString str, int begin, int end)
        {
            if (str.Length <= begin) return new SchemeString("");
            int len = end - begin;
            if ((str.Length - begin) < len) return new SchemeString(str.TheString.Substring(begin));
            return new SchemeString(str.TheString.Substring(begin, end - begin));
        }

        [SchemeFunction("string-copy!")]
        public static void StringCopy(SchemeString src, int off, int len, SchemeString dest, int destoff)
        {
            Array.Copy(src.TheCharArray, off, dest.TheCharArray, destoff, len);
        }

        [SchemeFunction("string-fill!")]
        public static void StringFill(SchemeString src, int off, int len, char ch)
        {
            int iend = off + len;
            for (int i = off; i < iend; ++i) src[i] = ch;
        }

        [SchemeFunction("left$")]
        public static string LeftDollar(string src, int count)
        {
            if (src.Length <= count) return src;
            return src.Substring(0, count);
        }

        [SchemeFunction("right$")]
        public static string RightDollar(string src, int count)
        {
            if (src.Length <= count) return src;
            return src.Substring(src.Length - count, count);
        }

        [SchemeFunction("mid$")]
        public static string MidDollar(string src, int off, int len)
        {
            if (src.Length <= off) return "";
            if ((src.Length - off) < len) return src.Substring(off);
            return src.Substring(off, len);
        }

        [SchemeFunction("trim")]
        public static string Trim(string str) { return str.Trim(); }

        [SchemeFunction("hex$")]
        public static string HexDollar(BigInteger i)
        {
            if (i.IsNegative)
            {
                return "-#x" + BigInteger.ToString(-i, 16u);
            }
            else
            {
                return "#x" + BigInteger.ToString(i, 16u);
            }
        }

#if false
        [SchemeFunction("vector-length")]
        public static int VectorLength(object[] vec)
        {
            return vec.Length;
        }

        [SchemeFunction("make-vector")]
        public static object[] MakeVector([OverflowMode(BigMath.OverflowBehavior.ThrowException)] int len, object init)
        {
            object[] arr = new object[len];
            for (int i = 0; i < len; ++i) arr[i] = init;
            return arr;
        }

        [SchemeFunction("vector-ref")]
        public static object VectorRef
        (
            object[] vec,
            [OverflowMode(BigMath.OverflowBehavior.ThrowException)] int i
        )
        {
            return vec[i];
        }

        [SchemeFunction("vector-set!")]
        public static void VectorSet
        (
            object[] vec,
            [OverflowMode(BigMath.OverflowBehavior.ThrowException)] int i,
            object val
        )
        {
            vec[i] = val;
        }

        [SchemeFunction("vector-copy!")]
        public static void VectorCopy
        (
            object[] src,
            [OverflowMode(BigMath.OverflowBehavior.ThrowException)] int srcOff,
            [OverflowMode(BigMath.OverflowBehavior.ThrowException)] int len,
            object[] dest,
            [OverflowMode(BigMath.OverflowBehavior.ThrowException)] int destOff
        )
        {
            bool copyBackwards = false;
            if (src == dest && destOff > srcOff) copyBackwards = true;

            if (copyBackwards)
            {
                for (int j = len - 1; j >= 0; --j)
                {
                    dest[destOff + j] = src[srcOff + j];
                }
            }
            else
            {
                for (int i = 0; i < len; ++i)
                {
                    dest[destOff + i] = src[srcOff + i];
                }
            }
        }

        [SchemeFunction("vector-fill!")]
        public static void VectorFill(object[] dest, int off, int len, object val)
        {
            int iend = off + len;
            for (int i = off; i < iend; ++i) dest[i] = val;
        }

        [SchemeFunction("vector?")]
        public static bool IsVector(object obj)
        {
            return (obj is object[]);
        }
#endif

        [SchemeFunction("int->decimal")]
        public static Decimal IntToDecimal(int i)
        {
            return Convert.ToDecimal(i);
        }

        [SchemeFunction("decimal->int")]
        public static int DecimalToInt(Decimal d)
        {
            return Convert.ToInt32(d);
        }

        [SchemeFunction("make-regex")]
        public static object MakeRegex(string s)
        {
            try
            {
                return new System.Text.RegularExpressions.Regex(s);
            }
            catch (Exception exc)
            {
                return exc;
            }
        }

        [SchemeFunction("regex?")]
        public static bool IsRegex(object obj)
        {
            return (obj is System.Text.RegularExpressions.Regex);
        }

        [SchemeFunction("regex-matches?")]
        public static bool RegexMatches(System.Text.RegularExpressions.Regex r, string s)
        {
            System.Text.RegularExpressions.MatchCollection mc = r.Matches(s);
            return (mc.Count == 1 && mc[0].Length == s.Length);
        }

        [SchemeFunction("exception?")]
        public static bool IsException(object obj)
        {
            return (obj is Exception);
        }

        [SchemeFunction("exception->string")]
        public static string ExceptionToString(Exception exc)
        {
            return exc.Message;
        }

        public static System.Runtime.Serialization.Formatters.Binary.BinaryFormatter NewFormatter()
        {
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            System.Runtime.Serialization.SurrogateSelector ss = new System.Runtime.Serialization.SurrogateSelector();
            ProxyGenerator.AddProxySurrogates(ss);
            bf.SurrogateSelector = ss;
            return bf;
        }

        [SchemeFunction("serialize-to-file")]
        public static void SerializeToFile(string filename, object obj)
        {
            System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Create);
            try
            {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = NewFormatter();
                bf.Serialize(fs, obj);
            }
            finally
            {
                fs.Close();
            }
        }

        [SchemeFunction("deserialize-from-file")]
        public static object DeserializeFromFile(string filename)
        {
            System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Open);
            object obj = null;
            try
            {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = NewFormatter();
                obj = bf.Deserialize(fs);
            }
            finally
            {
                fs.Close();
            }
            return obj;
        }

        [SchemeFunction("serialize-to-bytes")]
        public static SchemeByteArray SerializeToBytes(object obj)
        {
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = NewFormatter();
                bf.Serialize(ms, obj);
                return new SchemeByteArray(ms.ToArray(), DigitOrder.LBLA);
            }
        }

        [SchemeFunction("deserialize-from-bytes")]
        public static object DeserializeFromBytes(SchemeByteArray b)
        {
            object obj = null;
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(b.Bytes))
            {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = NewFormatter();
                obj = bf.Deserialize(ms);
            }
            return obj;
        }

#if false
        [SchemeFunction("make-bytes")]
        public static byte[] MakeBytes(int size) { return new byte[size]; }

        [SchemeFunction("byte-length")]
        public static int ByteLength(byte[] bytes) { return bytes.Length; }

        [SchemeFunction("byte-copy!")]
        public static void ByteCopy(byte[] src, int srcOff, int len, byte[] dest, int destOff)
        {
            Array.Copy(src, srcOff, dest, destOff, len);
        }

        [SchemeFunction("byte-fill!")]
        public static void ByteFill(byte[] dest, int off, int len, byte value)
        {
            int offEnd = off + len;
            while (off < offEnd)
            {
                dest[off++] = value;
            }
        }

        [SchemeFunction("byte-ref")]
        public static byte ByteRef(byte[] bytes, int off) { return bytes[off]; }

        [SchemeFunction("byte-set!")]
        public static void ByteSet(byte[] bytes, int off, byte val) { bytes[off] = val; }

        [SchemeFunction("byte-ref-int")]
        public static BigInteger ByteRefInt(byte[] bytes, int off, int len, bool signed, bool hbf)
        {
            return BigInteger.FromByteArray(bytes, off, len, signed, hbf ? DigitOrder.HBLA : DigitOrder.LBLA);
        }

        [SchemeFunction("byte-set-int!")]
        public static void ByteSetInt(byte[] bytes, int off, int len, bool signed, bool hbf, bool saturate, BigInteger value)
        {
            value.WriteBytesToArray
            (
                bytes, off, len, signed,
                saturate ? OverflowBehavior.Saturate : OverflowBehavior.Wraparound,
                hbf ? DigitOrder.HBLA : DigitOrder.LBLA
            );
        }

        public static double ByteRefDouble(byte[] bytes, int off, bool hbf)
        {
            if (hbf)
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

        public static void ByteSetDouble(byte[] bytes, int off, bool hbf, double value)
        {
            byte[] x = BitConverter.GetBytes(value);
            if (hbf)
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

        public static float ByteRefFloat(byte[] bytes, int off, bool hbf)
        {
            if (hbf)
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

        public static void ByteSetFloat(byte[] bytes, int off, bool hbf, float value)
        {
            byte[] x = BitConverter.GetBytes(value);
            if (hbf)
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

        public static string ByteRefString(byte[] bytes, int off, int len)
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

        public static void ByteSetString(byte[] bytes, int off, int len, string str)
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
#endif

        [SchemeFunction("page!")]
        public static void Page()
        {
            Console.Clear();
        }

        [SchemeFunction("set-consize!")]
        public static void SetConSize(IGlobalState gs, int x, int y)
        {
            gs.Console.SetSize(x, y);
        }

        [SchemeFunction("display-string!")]
        public static void Display(IGlobalState gs, string s)
        {
            gs.Console.Write(s);
        }

        [SchemeFunction("getkey")]
        public static object GetKey(IGlobalState gs)
        {
            return gs.Console.ReadKey();
        }

        [SchemeFunction("set-color!")]
        public static void SetColor(IGlobalState gs, int fg, int bg)
        {
            gs.Console.SetColor(fg, bg);
        }

        [SchemeFunction("goto-xy!")]
        public static void GotoXY(IGlobalState gs, int x, int y)
        {
            gs.Console.MoveTo(x, y);
        }

        [SchemeFunction("readline")]
        public static string ReadLine(IGlobalState gs)
        {
            return gs.Console.ReadLine();
        }

        [SchemeFunction("begin-readline")]
        public static SignalID BeginReadLine(IGlobalState gs)
        {
            SignalID sid = gs.Scheduler.GetNewSignalID();
            Func<string> f = new Func<string>(gs.Console.ReadLine);
            IAsyncResult iar = f.BeginInvoke(null, null);
            gs.Scheduler.PostActionOnCompletion
            (
                iar.AsyncWaitHandle,
                delegate()
                {
                    try
                    {
                        string x = f.EndInvoke(iar);
                        gs.Scheduler.PostSignal(sid, new SchemeString(x), false);
                    }
                    catch (Exception exc)
                    {
                        gs.Scheduler.PostSignal(sid, exc, true);
                    }
                }
            );
            gs.RegisterSignal(sid, "readline", false);
            return sid;
        }

        [SchemeFunction("not")]
        public static bool Not(object obj)
        {
            return ((obj is bool) && ((bool)obj) == false);
        }

        [SchemeFunction("boolean?")]
        public static bool IsBoolean(object obj)
        {
            return (obj is bool);
        }

        public static char ToHexChar(int value)
        {
            if (value < 10) return (char)('0' + value);
            else return (char)('A' + value - 10);
        }

        private static string ToHex(int value, int digits)
        {
            char[] d = new char[digits];
            int pos = digits;
            while (pos > 0)
            {
                --pos;
                d[pos] = ToHexChar(value & 0x0F);
                value >>= 4;
            }
            return new string(d);
        }

        private static string ToHex(long value, int digits)
        {
            char[] d = new char[digits];
            int pos = digits;
            while (pos > 0)
            {
                --pos;
                d[pos] = ToHexChar((int)(value & 0x0FL));
                value >>= 4;
            }
            return new string(d);
        }

        public static string DumpLine(byte[] bytes, int lBegin, int dBegin, int dEnd)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(ToHex(lBegin, 6));
            sb.Append(" : ");
            int lEnd = lBegin + 16;
            for (int i = lBegin; i < lEnd; ++i)
            {
                if (i < dBegin || i >= dEnd) sb.Append("   ");
                else
                {
                    sb.Append(ToHex(bytes[i], 2));
                    sb.Append(" ");
                }
            }
            sb.Append(": ");
            for (int i = lBegin; i < lEnd; ++i)
            {
                if (i < dBegin || i >= dEnd) sb.Append(" ");
                else
                {
                    byte b = bytes[i];
                    if (b < 32 || b > 126) sb.Append(".");
                    else sb.Append((char)b);
                }
            }
            return sb.ToString();
        }

        public static string DumpLine(long lBegin, long dBegin, long dEnd)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(ToHex(lBegin, 16));
            sb.Append(" : ");
            long lEnd = lBegin + 16L;
            for (long i = lBegin; i < lEnd; ++i)
            {
                if (i < dBegin || i >= dEnd) sb.Append("   ");
                else
                {
                    sb.Append(ToHex(System.Runtime.InteropServices.Marshal.ReadByte((IntPtr)i), 2));
                    sb.Append(" ");
                }
            }
            sb.Append(": ");
            for (long i = lBegin; i < lEnd; ++i)
            {
                if (i < dBegin || i >= dEnd) sb.Append(" ");
                else
                {
                    byte b = System.Runtime.InteropServices.Marshal.ReadByte((IntPtr)i);
                    if (b < 32 || b > 126) sb.Append(".");
                    else sb.Append((char)b);
                }
            }
            return sb.ToString();
        }

        [SchemeFunction("dump")]
        public static void Dump(IGlobalState gs, object obj)
        {
            if (obj is ByteRange)
            {
                ByteRange b = (ByteRange)obj;
                int lPos = (b.Offset & ~0x0F);
                int lEnd = b.Offset + b.LengthInt32;
                while (lPos < lEnd)
                {
                    gs.Console.WriteLine(DumpLine(b.Array.Bytes, lPos, b.Offset, lEnd));
                    lPos += 16;
                }
            }
            else if (obj is NativeByteRange)
            {
                NativeByteRange nb = (NativeByteRange)obj;
                long lPos = (long)(nb.Ptr) & ~0x0FL;
                long lBegin = (long)(nb.Ptr);
                long lEnd = (long)(nb.Ptr) + (long)(nb.Length);
                while (lPos < lEnd)
                {
                    gs.Console.WriteLine(DumpLine(lPos, lBegin, lEnd));
                    lPos += 16L;
                }
            }
            else if (obj is SchemeByteArray)
            {
                SchemeByteArray sba = (SchemeByteArray)obj;
                int lPos = 0;
                int lEnd = sba.Length;
                while (lPos < lEnd)
                {
                    gs.Console.WriteLine(DumpLine(sba.Bytes, lPos, 0, lEnd));
                    lPos += 16;
                }
            }
            else if (obj is DisposableID)
            {
                DisposableID d = (DisposableID)obj;
                IDisposable d2 = gs.GetDisposableByID(d);
                if (d2 is NativeMemory)
                {
                    NativeMemory nm = (NativeMemory)d2;
                    long lPos = (long)(nm.Ptr) & ~0x0FL;
                    long lBegin = (long)(nm.Ptr);
                    long lEnd = (long)(nm.Ptr) + (long)(nm.Length);
                    while (lPos < lEnd)
                    {
                        gs.Console.WriteLine(DumpLine(lPos, lBegin, lEnd));
                        lPos += 16L;
                    }
                }
                else
                {
                    gs.Console.WriteLine("dump: unknown data type " + d2.GetType().FullName);
                }
            }
            else
            {
                gs.Console.WriteLine("dump: unknown data type " + obj.GetType().FullName);
            }
        }

        [SchemeFunction("floor")]
        public static object Floor(object obj)
        {
            if (obj is BigInteger) return obj;
            else if (obj is BigRational)
            {
                return ((BigRational)obj).Floor();
            }
            else if (obj is double)
            {
                return Math.Floor((double)obj);
            }
            else throw new SchemeRuntimeException("Type mismatch during floor");
        }

        [SchemeFunction("round")]
        public static object Round(object obj)
        {
            if (obj is BigInteger) return obj;
            else if (obj is BigRational)
            {
                return ((BigRational)obj).Round();
            }
            else if (obj is double)
            {
                return Math.Round((double)obj);
            }
            else throw new SchemeRuntimeException("Type mismatch during round");
        }

        [SchemeFunction("ceiling")]
        public static object Ceiling(object obj)
        {
            if (obj is BigInteger) return obj;
            else if (obj is BigRational)
            {
                return ((BigRational)obj).Ceiling();
            }
            else if (obj is double)
            {
                return Math.Ceiling((double)obj);
            }
            else throw new SchemeRuntimeException("Type mismatch during ceiling");
        }

        [SchemeFunction("truncate")]
        public static object Truncate(object obj)
        {
            if (obj is BigInteger) return obj;
            else if (obj is BigRational)
            {
                return ((BigRational)obj).TruncateTowardZero();
            }
            else if (obj is double)
            {
                return Math.Truncate((double)obj);
            }
            else throw new SchemeRuntimeException("Type mismatch during truncate");
        }

        [SchemeFunction("odd?")]
        public static bool IsOdd(object obj)
        {
            if (obj is BigInteger) return ((BigInteger)obj).IsOdd;
            else return false;
        }

        [SchemeFunction("even?")]
        public static bool IsEven(object obj)
        {
            if (obj is BigInteger) return !((BigInteger)obj).IsOdd;
            else return false;
        }

        [SchemeFunction("$$char=?")]
        public static bool CharEqual(char c1, char c2)
        {
            return c1 == c2;
        }

        [SchemeFunction("$$char<?")]
        public static bool CharLessThan(char c1, char c2)
        {
            return c1 < c2;
        }

        [SchemeFunction("$$char>?")]
        public static bool CharGreaterThan(char c1, char c2)
        {
            return c1 > c2;
        }

        [SchemeFunction("$$char<=?")]
        public static bool CharLessEqual(char c1, char c2)
        {
            return c1 <= c2;
        }

        [SchemeFunction("$$char>=?")]
        public static bool CharGreaterEqual(char c1, char c2)
        {
            return c1 >= c2;
        }

        [SchemeFunction("$$char-ci=?")]
        public static bool CharCiEqual(char c1, char c2)
        {
            return char.ToUpperInvariant(c1) == char.ToUpperInvariant(c2);
        }

        [SchemeFunction("$$char-ci<?")]
        public static bool CharCiLessThan(char c1, char c2)
        {
            return char.ToUpperInvariant(c1) < char.ToUpperInvariant(c2);
        }

        [SchemeFunction("$$char-ci>?")]
        public static bool CharCiGreaterThan(char c1, char c2)
        {
            return char.ToUpperInvariant(c1) > char.ToUpperInvariant(c2);
        }

        [SchemeFunction("$$char-ci<=?")]
        public static bool CharCiLessEqual(char c1, char c2)
        {
            return char.ToUpperInvariant(c1) <= char.ToUpperInvariant(c2);
        }

        [SchemeFunction("$$char-ci>=?")]
        public static bool CharCiGreaterEqual(char c1, char c2)
        {
            return char.ToUpperInvariant(c1) >= char.ToUpperInvariant(c2);
        }

        [SchemeFunction("char-alphabetic?")]
        public static bool CharAlphabetic(char ch)
        {
            return char.IsLetter(ch);
        }

        [SchemeFunction("char-numeric?")]
        public static bool CharNumeric(char ch)
        {
            return char.IsDigit(ch);
        }

        [SchemeFunction("char-whitespace?")]
        public static bool CharWhitespace(char ch)
        {
            return char.IsWhiteSpace(ch);
        }

        [SchemeFunction("char-upper-case?")]
        public static bool IsCharUpperCase(char ch)
        {
            return char.IsUpper(ch);
        }

        [SchemeFunction("char-lower-case?")]
        public static bool IsCharLowerCase(char ch)
        {
            return char.IsLower(ch);
        }

        [SchemeFunction("char-upcase")]
        public static char CharUpcase(char ch)
        {
            return char.ToUpperInvariant(ch);
        }

        [SchemeFunction("char-downcase")]
        public static char CharDowncase(char ch)
        {
            return char.ToLowerInvariant(ch);
        }

        [SchemeFunction("string=?")]
        public static bool StringEqual(SchemeString s1, SchemeString s2)
        {
            return s1 == s2;
        }

        [SchemeFunction("string!=?")]
        public static bool StringNotEqual(SchemeString s1, SchemeString s2)
        {
            return s1 != s2;
        }

        [SchemeFunction("string-ci=?")]
        public static bool StringCiEqual(SchemeString s1, SchemeString s2)
        {
            return string.Compare(s1.TheString, s2.TheString, true) == 0;
        }

        [SchemeFunction("string-ci!=?")]
        public static bool StringCiNotEqual(SchemeString s1, SchemeString s2)
        {
            return string.Compare(s1.TheString, s2.TheString, true) != 0;
        }

        [SchemeFunction("eq?")]
        public static bool FastEqual(object obj1, object obj2)
        {
            if (obj1.GetType() != obj2.GetType()) return false;
            else if (object.ReferenceEquals(obj1, obj2)) return true;
            else if (obj1 is SchemeString) return ((SchemeString)obj1).TheString == ((SchemeString)obj2).TheString;
            else if (obj1 is BigInteger) return ((BigInteger)obj1) == ((BigInteger)obj2);
            else if (obj1 is BigRational) return ((BigRational)obj1) == ((BigRational)obj2);
            else if (obj1 is bool) return ((bool)obj1) == ((bool)obj2);
            else if (obj1 is Symbol) return ((Symbol)obj1).Equals((Symbol)obj2);
            else if (obj1 is System.Net.IPAddress) return obj1.Equals(obj2);
            else if (obj1 is System.Net.IPEndPoint) return obj1.Equals(obj2);
            else if (obj1 is SpecialValue) return ((SpecialValue)obj1) == ((SpecialValue)obj2);
            else if (obj1 is double) return ((double)obj1) == ((double)obj2);
            else if (obj1 is Guid) return ((Guid)obj1) == ((Guid)obj2);
            else if (obj1 is DisposableID) return ((DisposableID)obj1) == ((DisposableID)obj2);
            else if (obj1 is AsyncID) return ((AsyncID)obj1) == ((AsyncID)obj2);
            else if (obj1 is ExprObjModel.ObjectSystem.OldObjectID) return ((ExprObjModel.ObjectSystem.OldObjectID)obj1) == ((ExprObjModel.ObjectSystem.OldObjectID)obj2);
            else if (obj1 is ExprObjModel.ObjectSystem.Signature) return object.ReferenceEquals(obj1, obj2);
            else if (obj1 is ByteRange)
            {
                ByteRange b1 = (ByteRange)obj1;
                ByteRange b2 = (ByteRange)obj2;
                return object.ReferenceEquals(b1.Array, b2.Array) && (b1.Length == b2.Length) && (b1.Offset == b2.Offset);
            }
            else if (obj1 is ByteRectangle)
            {
                ByteRectangle b1 = (ByteRectangle)obj1;
                ByteRectangle b2 = (ByteRectangle)obj2;
                return object.ReferenceEquals(b1.Array, b2.Array) && (b1.Height == b2.Height) && (b1.Offset == b2.Offset) && (b1.Width == b2.Width) && (b1.Stride == b2.Stride);
            }
            else return false;
        }

        [SchemeFunction("eqv?")]
        public static bool Equal2(object obj1, object obj2)
        {
            if (obj1.GetType() != obj2.GetType()) return false;
            if (FastEqual(obj1, obj2)) return true;

            if (obj1 is Deque<object>)
            {
                Deque<object> d1 = (Deque<object>)obj1;
                Deque<object> d2 = (Deque<object>)obj2;
                if (d1.Count != d2.Count) return false;
                int iEnd = d1.Count;
                for (int i = 0; i < iEnd; ++i)
                {
                    if (!FastEqual(d1[i], d2[i])) return false;
                }
                return true;
            }
            else if (obj1 is SchemeByteArray)
            {
                SchemeByteArray b1 = (SchemeByteArray)obj1;
                SchemeByteArray b2 = (SchemeByteArray)obj2;
                if (b1.Length != b2.Length) return false;
                int iEnd = b1.Length;
                for (int i = 0; i < iEnd; ++i)
                {
                    if (b1.Bytes[i] != b2.Bytes[i]) return false;
                }
                return true;
            }
            else if (obj1 is SchemeHashSet)
            {
                SchemeHashSet h1 = (SchemeHashSet)obj1;
                SchemeHashSet h2 = (SchemeHashSet)obj2;
                return SchemeHashSet.SymmetricDifference(h1, h2).Count == 0;
            }
            else if (obj1 is SchemeHashMap)
            {
                SchemeHashMap m1 = (SchemeHashMap)obj1;
                SchemeHashMap m2 = (SchemeHashMap)obj2;
                if (SchemeHashSet.SymmetricDifference(m1.GetKeys(), m2.GetKeys()).Count != 0) return false;
                foreach (KeyValuePair<object, object> kvp in m1)
                {
                    if (!(FastEqual(kvp.Value, m2[kvp.Key]))) return false;
                }
                return true;
            }
            else if (obj1 is ExprObjModel.ObjectSystem.Signature)
            {
                ExprObjModel.ObjectSystem.Signature s1 = (ExprObjModel.ObjectSystem.Signature)obj1;
                ExprObjModel.ObjectSystem.Signature s2 = (ExprObjModel.ObjectSystem.Signature)obj2;
                return s1 == s2;
            }
            else if (obj1 is ByteRange)
            {
                ByteRange b1 = (ByteRange)obj1;
                ByteRange b2 = (ByteRange)obj2;

                if (b1.Length != b2.Length) return false;
                if (!(b1.IsValid) || !(b2.IsValid)) return false;
                int iEnd = b1.LengthInt32;
                for (int i = 0; i < iEnd; ++i)
                {
                    if (b1.Array.Bytes[b1.Offset + i] != b2.Array.Bytes[b2.Offset + i]) return false;
                }
                return true;
            }
            else if (obj1 is ByteRectangle)
            {
                ByteRectangle r1 = (ByteRectangle)obj1;
                ByteRectangle r2 = (ByteRectangle)obj2;

                if (r1.Height != r2.Height) return false;
                if (r1.Width != r2.Width) return false;
                if (!(r1.IsValid) || !(r2.IsValid)) return false;
                int yEnd = r1.Height;
                for (int y = 0; y < yEnd; ++y)
                {
                    int yOffset1 = r1.Offset + r1.Stride * y;
                    int yOffset2 = r2.Offset + r2.Stride * y;

                    int xEnd = r1.Width;
                    for (int x = 0; x < xEnd; ++x)
                    {
                        if (r1.Array.Bytes[yOffset1 + x] != r2.Array.Bytes[yOffset2 + x]) return false;
                    }
                }
                return true;
            }
            else return false;
        }

        [SchemeFunction("atom?")]
        public static bool IsAtom(object obj)
        {
            return !(obj is Deque<object>) && !(obj is ConsCell) && !(obj is SchemeHashSet) && !(obj is SchemeHashMap);
        }

        [SchemeFunction("make-top-level")]
        public static TopLevel MakeTopLevel(IGlobalState gs)
        {
            return new TopLevel(gs, true);
        }

        [SchemeFunction("top-level?")]
        public static bool IsTopLevel(object obj)
        {
            return (obj is TopLevel);
        }

        [SchemeFunction("define-into")]
        public static void DefineInto(TopLevel t, Symbol var, object value)
        {
            t.Define(var, value);
        }

        [SchemeFunction("undefine-into")]
        public static void UndefineInto(TopLevel t, Symbol var)
        {
            t.Undefine(var);
        }

        [SchemeFunction("begin-module-in")]
        public static void BeginModuleIn(TopLevel t)
        {
            t.BeginModule();
        }

        [SchemeFunction("double->parts")]
        public static object DoubleToParts(double d)
        {
            long l = BitConverter.DoubleToInt64Bits(d);
            object o = new ConsCell(BigInteger.FromInt64(l & 0xFFFFFFFFFFFFFL), SpecialValue.EMPTY_LIST);
            o = new ConsCell(BigInteger.FromInt64((l >> 52) & 0x7FFL), o);
            o = new ConsCell(BigInteger.FromInt64((l >> 63) & 1L), o);
            return o;
        }

        [SchemeFunction("parts->double")]
        public static double PartsToDouble(long sign, long exp, long frac)
        {
            double d = BitConverter.Int64BitsToDouble((sign << 63) | (exp << 52) | frac);
            return d;
        }

        [SchemeFunction("float->parts")]
        public static object FloatToParts(float f)
        {
            int i = BitConverter.ToInt32(BitConverter.GetBytes(f), 0);
            object o = new ConsCell(BigInteger.FromInt32(i & 0x7FFFFF), SpecialValue.EMPTY_LIST);
            o = new ConsCell(BigInteger.FromInt32((i >> 23) & 0xFF), o);
            o = new ConsCell(BigInteger.FromInt32((i >> 31) & 1), o);
            return o;
        }

        [SchemeFunction("infinite?")]
        public static bool IsInfinite(object obj)
        {
            if (obj is double)
            {
                long l = BitConverter.DoubleToInt64Bits((double)obj);
                return ((l & 0x7FFFFFFFFFFFFFFFL) == 0x7FF0000000000000L);
            }
            else return false;
        }

        [SchemeFunction("nan?")]
        public static bool IsNan(object obj)
        {
            if (obj is double)
            {
                long l = BitConverter.DoubleToInt64Bits((double)obj);
                return (((l & 0x7FF0000000000000L) == 0x7FF0000000000000L) && ((l & 0xFFFFFFFFFFFFFL) != 0L));
            }
            else return false;
        }

        [SchemeFunction("parts->float")]
        public static float PartsToFloat(int sign, int exp, int frac)
        {
            float f = BitConverter.ToSingle(BitConverter.GetBytes((sign << 31) | (exp << 23) | frac), 0);
            return f;
        }

        [SchemeFunction("inexact->exact")]
        public static object InexactToExact(object obj)
        {
            if (obj is BigInteger) return obj;
            if (obj is BigRational) return obj;
            if (!(obj is double)) throw new SchemeRuntimeException("inexact->exact requires BigInteger, BigRational, or double");
            BigRational r = BigRational.GetRationalValue((double)obj);
            if (r.Denominator == BigInteger.One) return r.Numerator;
            else return r;
        }

        [SchemeFunction("float->string")]
        public static string FloatToString(float f) { return f.ToString("R"); }

        [SchemeFunction("string->float")]
        public static float StringToFloat(string s) { return float.Parse(s, System.Globalization.NumberStyles.Float); }

        [SchemeFunction("double->string")]
        public static string DoubleToString(double d) { return d.ToString("R"); }

        [SchemeFunction("string->double")]
        public static double StringToDouble(string s) { return double.Parse(s, System.Globalization.NumberStyles.Float); }

        [SchemeFunction("float->double")]
        public static double FloatToDouble(float f) { return (double)f; }

        [SchemeFunction("double->float")]
        public static float DoubleToFloat(double d) { return (float)d; }

        [SchemeFunction("decimal->parts")]
        public static object DecimalToParts(decimal d)
        {
            int[] i = decimal.GetBits(d);
            uint[] u = new uint[4];
            u[0] = (uint)i[0];
            u[1] = (uint)i[1];
            u[2] = (uint)i[2];
            u[3] = (uint)i[3];
            BigInteger b = new BigInteger(u, false);

            object o = new ConsCell(b & BigInteger.Parse("FFFFFFFFFFFFFFFFFFFFFFFF", 16), SpecialValue.EMPTY_LIST);
            o = new ConsCell((b >> 112) & BigInteger.FromInt32(0xFF), o);
            o = new ConsCell(b >> 127, o);
            return o;
        }

        [SchemeFunction("parts->decimal")]
        public static decimal PartsToDecimal(BigInteger sign, BigInteger expt, BigInteger frac)
        {
            int[] i = new int[4];
            i[0] = frac.GetInt32Value(OverflowBehavior.Wraparound);
            i[1] = (frac >> 32).GetInt32Value(OverflowBehavior.Wraparound);
            i[2] = (frac >> 64).GetInt32Value(OverflowBehavior.Wraparound);
            i[3] = (sign.IsZero ? 0 : unchecked((int)0x80000000)) | (expt.GetInt32Value(OverflowBehavior.ThrowException) << 16);
            return new Decimal(i);
        }

        [SchemeFunction("decimal->string")]
        public static string DecimalToString(decimal d)
        {
            return d.ToString("G");
        }

        [SchemeFunction("string->decimal")]
        public static decimal StringToDecimal(string s)
        {
            return decimal.Parse(s, System.Globalization.NumberStyles.Float);
        }

        [SchemeFunction("abs")]
        public static object Abs(object a)
        {
            if (a is BigInteger)
            {
                BigInteger ba = (BigInteger)a;
                return (ba.IsNegative) ? -ba : ba;
            }
            else if (a is BigRational)
            {
                BigRational br = (BigRational)a;
                return (br.IsNegative) ? -br : br;
            }
            else if (a is double)
            {
                return Math.Abs((double)a);
            }
            else throw new SchemeRuntimeException("abs requires BigInteger, BigRational, or double");
        }

        [SchemeFunction("expt")]
        public static object Expt(object @base, object expt)
        {
            if (expt is BigInteger)
            {
                BigInteger exptbi = (BigInteger)expt;
                if (@base is BigInteger)
                {
                    if (exptbi.IsNegative)
                    {
                        return BigRational.Pow(new BigRational((BigInteger)@base, BigInteger.One), exptbi.GetInt32Value(OverflowBehavior.ThrowException));
                    }
                    else
                    {
                        return BigInteger.Pow((BigInteger)@base, exptbi.GetUInt32Value(OverflowBehavior.ThrowException));
                    }
                }
                else if (@base is BigRational)
                {
                    return BigRational.Pow((BigRational)@base, exptbi.GetInt32Value(OverflowBehavior.ThrowException));
                }
                else if (@base is double)
                {
                    return Math.Pow((double)@base, ProxyGenerator.NumberToDouble(expt));
                }
            }
            else if (expt is BigRational || expt is double)
            {
                if (@base is BigInteger || @base is BigRational || @base is double)
                {
                    return Math.Pow(ProxyGenerator.NumberToDouble(@base), ProxyGenerator.NumberToDouble(expt));
                }
            }
            throw new SchemeRuntimeException("expt requires BigInteger, BigRational, or double arguments");
        }

        [SchemeFunction("acos")] public static double Acos(double ang) { return Math.Acos(ang); }
        [SchemeFunction("asin")] public static double Asin(double ang) { return Math.Asin(ang); }
        [SchemeFunction("atan")] public static double Atan(double ang) { return Math.Atan(ang); }
        [SchemeFunction("atan2")] public static double Atan2(double y, double x) { return Math.Atan2(y, x); }
        [SchemeFunction("cos")] public static double Cos(double ang) { return Math.Cos(ang); }
        [SchemeFunction("cosh")] public static double Cosh(double ang) { return Math.Cosh(ang); }
        [SchemeFunction("exp")] public static double Exp(double e) { return Math.Exp(e); }
        [SchemeFunction("log")] public static double Log(double e) { return Math.Log(e); }
        [SchemeFunction("log10")] public static double Log10(double e) { return Math.Log10(e); }
        [SchemeFunction("sin")] public static double Sin(double ang) { return Math.Sin(ang); }
        [SchemeFunction("sinh")] public static double Sinh(double ang) { return Math.Sinh(ang); }
        [SchemeFunction("sqrt")] public static double Sqrt(double a) { return Math.Sqrt(a); }
        [SchemeFunction("tan")] public static double Tan(double ang) { return Math.Tan(ang); }
        [SchemeFunction("tanh")] public static double Tanh(double ang) { return Math.Tanh(ang); }

        private static Random r = new Random((int)((System.Diagnostics.Stopwatch.GetTimestamp() >> 3) & 0x7FFFFFFF));

        [SchemeFunction("random-int")]
        public static int RandomInt(int upperBound)
        {
            lock (r)
            {
                return r.Next(upperBound);
            }
        }

        [SchemeFunction("random-float")]
        public static double RandomFloat()
        {
            lock(r)
            {
                return r.NextDouble();
            }
        }

        [SchemeFunction("random-bytes")]
        public static SchemeByteArray RandomBytes(int nBytes)
        {
            lock (r)
            {
                byte[] b = new byte[nBytes];
                r.NextBytes(b);
                return new SchemeByteArray(b, DigitOrder.LBLA);
            }
        }

        [SchemeFunction("random-string")]
        public static string RandomString(int nChars, string alphabet)
        {
            lock (r)
            {
                StringBuilder sb = new StringBuilder();
                while (nChars > 0)
                {
                    int i = r.Next(alphabet.Length);
                    sb.Append(alphabet[i]);
                    --nChars;
                }
                return sb.ToString();
            }
        }

        //[SchemeFunction("shuffle-vector")]
        //public static Deque<object> ShuffleVector(Deque<object> d)
        //{
        //    Deque<object> e = new Deque<object>(d);
        //    int iEnd = e.Count;
        //    while (iEnd > 0)
        //    {
        //        int index = r.Next(iEnd);
        //        --iEnd;
        //        if (index != iEnd)
        //        {
        //            object temp = e[index]; e[index] = e[iEnd]; e[iEnd] = temp;
        //        }
        //    }
        //    return e;
        //}

        [SchemeFunction("vector-iota")]
        public static Deque<object> VectorIota(int size)
        {
            Deque<object> e = new Deque<object>();
            e.Capacity = size;
            for (int i = 0; i < size; ++i)
            {
                e.PushBack(BigInteger.FromInt32(i));
            }
            return e;
        }

        [SchemeFunction("hashable?")]
        public static bool IsHashable(object obj)
        {
            return (obj is SchemeByteArray) || (obj is IHashable) || (obj is double) || (obj is bool) || (obj is Guid) || (obj is IPEndPoint) || (obj is IPAddress) || (obj is char);
        }

        private static byte[] boolTrueHashData = new byte[] { 0x3F, 0x1B, 0x11, 0x4A };
        private static byte[] boolFalseHashData = new byte[] { 0xC0, 0xE4, 0xEE, 0xB5 };

        [SchemeFunction("hash-bytes")]
        public static uint HashByteArrayPart(SchemeByteArray sba, int off, int len)
        {
            BigMath.HashGenerator h = new BigMath.HashGenerator();
            h.Add(sba.Bytes, off, len);
            return unchecked((uint)h.Hash);
        }

        [SchemeFunction("hash")]
        public static uint Hash(object obj)
        {
            BigMath.HashGenerator h = new BigMath.HashGenerator();
            if (obj is SchemeByteArray)
            {
                h.Add(((SchemeByteArray)obj).Bytes);
            }
            else if (obj is BigMath.IHashable)
            {
                ((IHashable)obj).AddToHash(h);
            }
            else if (obj is double)
            {
                h.Add(BitConverter.GetBytes((double)obj));
            }
            else if (obj is Guid)
            {
                h.Add(((Guid)obj).ToByteArray());
            }
            else if (obj is bool)
            {
                if ((bool)obj)
                {
                    h.Add(boolTrueHashData);
                }
                else
                {
                    h.Add(boolFalseHashData);
                }
            }
            else if (obj is char)
            {
                h.Add((char)obj);
            }
            else if (obj is IPEndPoint)
            {
                IPEndPoint ipep = (IPEndPoint)obj;
                SocketAddress sa = ipep.Serialize();
                int iEnd = sa.Size;
                for (int i = 0; i < iEnd; ++i)
                {
                    h.Add(sa[i]);
                }
            }
            else if (obj is IPAddress)
            {
                IPAddress ipa = (IPAddress)obj;
                byte[] b = ipa.GetAddressBytes();
                h.Add(b);
            }
            else throw new SchemeRuntimeException("Attempt to hash an object which was not of a hashable type");
            return unchecked((uint)h.Hash);
        }

        [SchemeFunction("guid?")]
        public static bool IsGuid(object obj)
        {
            return (obj is Guid);
        }

        [SchemeFunction("new-guid")]
        public static Guid NewGuid()
        {
            return Guid.NewGuid();
        }

        [SchemeFunction("guid=?")]
        public static bool GuidEquals(Guid a, Guid b)
        {
            return a == b;
        }

        [SchemeFunction("replace")]
        public static object Replace(object src, Symbol s, object sValue)
        {
            if (src is Symbol)
            {
                Symbol sSrc = (Symbol)src;
                if (sSrc == s) return sValue;
                else return src;
            }
            else if (src is ConsCell)
            {
                ConsCell cSrc = (ConsCell)src;
                return new ConsCell(Replace(cSrc.car, s, sValue), Replace(cSrc.cdr, s, sValue));
            }
            else if (src is Deque<object>)
            {
                Deque<object> d1 = (Deque<object>)src;
                Deque<object> e1 = new Deque<object>();
                for (int i = 0; i < d1.Count; ++i)
                {
                    e1.PushBack(Replace(d1[i], s, sValue));
                }
                return e1;
            }
            else return src;
        }
    }

    [SchemeSingleton("end-module-in")]
    public class EndModuleInTopLevel : IProcedure
    {
        public EndModuleInTopLevel() { }

        public int Arity { get { return 1; } }

        public bool More { get { return true; } }
    
        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            try
            {
                if (argList == null) throw new SchemeRuntimeException("end-module-in: insufficient arguments");
                if (!(argList.Head is TopLevel)) throw new SchemeRuntimeException("end-module-in: expected TopLevel");
                TopLevel t = (TopLevel)(argList.Head);

                argList = argList.Tail;

                List<Symbol> s = new List<Symbol>();
                while (argList != null)
                {
                    if (!(argList.Head is Symbol)) throw new SchemeRuntimeException("end-module-in: expected Symbol");
                    Symbol s1 = (Symbol)(argList.Head);
                    argList = argList.Tail;
                    s.Add(s1);
                }

                t.EndModule(s);

                return new RunnableReturn(k, SpecialValue.UNSPECIFIED);
            }
            catch (Exception exc)
            {
                return new RunnableThrow(k, exc);
            }
        }
    }
}
