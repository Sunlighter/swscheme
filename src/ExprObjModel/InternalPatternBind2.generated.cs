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
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExprObjModel
{
    public static partial class Utils
    {
    
        private static Option<object> ParseByte(object input)
        {
            if (input is BigInteger)
            {
                BigInteger bInput = (BigInteger)input;
                if (bInput.FitsInByte)
                {
                    return new Some<object>() { value = bInput.GetByteValue(OverflowBehavior.Wraparound) };
                }
            }
            return new None<object>();
        }
    
        private static Option<object> ParseInt16(object input)
        {
            if (input is BigInteger)
            {
                BigInteger bInput = (BigInteger)input;
                if (bInput.FitsInInt16)
                {
                    return new Some<object>() { value = bInput.GetInt16Value(OverflowBehavior.Wraparound) };
                }
            }
            return new None<object>();
        }
    
        private static Option<object> ParseInt32(object input)
        {
            if (input is BigInteger)
            {
                BigInteger bInput = (BigInteger)input;
                if (bInput.FitsInInt32)
                {
                    return new Some<object>() { value = bInput.GetInt32Value(OverflowBehavior.Wraparound) };
                }
            }
            return new None<object>();
        }
    
        private static Option<object> ParseInt64(object input)
        {
            if (input is BigInteger)
            {
                BigInteger bInput = (BigInteger)input;
                if (bInput.FitsInInt64)
                {
                    return new Some<object>() { value = bInput.GetInt64Value(OverflowBehavior.Wraparound) };
                }
            }
            return new None<object>();
        }
    
        private static Option<object> ParseSByte(object input)
        {
            if (input is BigInteger)
            {
                BigInteger bInput = (BigInteger)input;
                if (bInput.FitsInSByte)
                {
                    return new Some<object>() { value = bInput.GetSByteValue(OverflowBehavior.Wraparound) };
                }
            }
            return new None<object>();
        }
    
        private static Option<object> ParseUInt16(object input)
        {
            if (input is BigInteger)
            {
                BigInteger bInput = (BigInteger)input;
                if (bInput.FitsInUInt16)
                {
                    return new Some<object>() { value = bInput.GetUInt16Value(OverflowBehavior.Wraparound) };
                }
            }
            return new None<object>();
        }
    
        private static Option<object> ParseUInt32(object input)
        {
            if (input is BigInteger)
            {
                BigInteger bInput = (BigInteger)input;
                if (bInput.FitsInUInt32)
                {
                    return new Some<object>() { value = bInput.GetUInt32Value(OverflowBehavior.Wraparound) };
                }
            }
            return new None<object>();
        }
    
        private static Option<object> ParseUInt64(object input)
        {
            if (input is BigInteger)
            {
                BigInteger bInput = (BigInteger)input;
                if (bInput.FitsInUInt64)
                {
                    return new Some<object>() { value = bInput.GetUInt64Value(OverflowBehavior.Wraparound) };
                }
            }
            return new None<object>();
        }
    
    }
}
