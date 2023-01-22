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
using System.Reflection.Emit;
using System.Runtime.Serialization;
using ExprObjModel.CodeGeneration;
using BigMath;

namespace ExprObjModel
{
    public static partial class ProxyGenerator
    {
        
        public static byte NumberToByte(object obj, OverflowBehavior ob)
        {
            if (obj is BigInteger)
            {
                return ((BigInteger)obj).GetByteValue(ob);
            }
            else if (obj is BigRational)
            {
                return ((BigRational)obj).Round().GetByteValue(ob);
            }
            else if (obj is double)
            {
                return (byte)(double)obj;
            }
            else throw new SchemeRuntimeException("BigInteger or BigRational or double expected");
        }

        private static void MarshalToByte(CodeGenerator cg, OverflowBehavior ob, LocalBuilder localByteDest)
        {
            cg.LoadInt((int)ob);
            cg.Call(typeof(ProxyGenerator).GetMethod("NumberToByte", new Type[] { typeof(object), typeof(OverflowBehavior) }));
            cg.StoreLocal(localByteDest);
        }

        private static void UnmarshalFromByte(CodeGenerator cg)
        {
            cg.Call(typeof(BigInteger).GetMethod("FromByte"));
        }

        
        public static short NumberToInt16(object obj, OverflowBehavior ob)
        {
            if (obj is BigInteger)
            {
                return ((BigInteger)obj).GetInt16Value(ob);
            }
            else if (obj is BigRational)
            {
                return ((BigRational)obj).Round().GetInt16Value(ob);
            }
            else if (obj is double)
            {
                return (short)(double)obj;
            }
            else throw new SchemeRuntimeException("BigInteger or BigRational or double expected");
        }

        private static void MarshalToInt16(CodeGenerator cg, OverflowBehavior ob, LocalBuilder localInt16Dest)
        {
            cg.LoadInt((int)ob);
            cg.Call(typeof(ProxyGenerator).GetMethod("NumberToInt16", new Type[] { typeof(object), typeof(OverflowBehavior) }));
            cg.StoreLocal(localInt16Dest);
        }

        private static void UnmarshalFromInt16(CodeGenerator cg)
        {
            cg.Call(typeof(BigInteger).GetMethod("FromInt16"));
        }

        
        public static int NumberToInt32(object obj, OverflowBehavior ob)
        {
            if (obj is BigInteger)
            {
                return ((BigInteger)obj).GetInt32Value(ob);
            }
            else if (obj is BigRational)
            {
                return ((BigRational)obj).Round().GetInt32Value(ob);
            }
            else if (obj is double)
            {
                return (int)(double)obj;
            }
            else throw new SchemeRuntimeException("BigInteger or BigRational or double expected");
        }

        private static void MarshalToInt32(CodeGenerator cg, OverflowBehavior ob, LocalBuilder localInt32Dest)
        {
            cg.LoadInt((int)ob);
            cg.Call(typeof(ProxyGenerator).GetMethod("NumberToInt32", new Type[] { typeof(object), typeof(OverflowBehavior) }));
            cg.StoreLocal(localInt32Dest);
        }

        private static void UnmarshalFromInt32(CodeGenerator cg)
        {
            cg.Call(typeof(BigInteger).GetMethod("FromInt32"));
        }

        
        public static long NumberToInt64(object obj, OverflowBehavior ob)
        {
            if (obj is BigInteger)
            {
                return ((BigInteger)obj).GetInt64Value(ob);
            }
            else if (obj is BigRational)
            {
                return ((BigRational)obj).Round().GetInt64Value(ob);
            }
            else if (obj is double)
            {
                return (long)(double)obj;
            }
            else throw new SchemeRuntimeException("BigInteger or BigRational or double expected");
        }

        private static void MarshalToInt64(CodeGenerator cg, OverflowBehavior ob, LocalBuilder localInt64Dest)
        {
            cg.LoadInt((int)ob);
            cg.Call(typeof(ProxyGenerator).GetMethod("NumberToInt64", new Type[] { typeof(object), typeof(OverflowBehavior) }));
            cg.StoreLocal(localInt64Dest);
        }

        private static void UnmarshalFromInt64(CodeGenerator cg)
        {
            cg.Call(typeof(BigInteger).GetMethod("FromInt64"));
        }

        
        public static IntPtr NumberToIntPtr(object obj, OverflowBehavior ob)
        {
            if (obj is BigInteger)
            {
                return ((BigInteger)obj).GetIntPtrValue(ob);
            }
            else if (obj is BigRational)
            {
                return ((BigRational)obj).Round().GetIntPtrValue(ob);
            }
            else if (obj is double)
            {
                return (IntPtr)(double)obj;
            }
            else throw new SchemeRuntimeException("BigInteger or BigRational or double expected");
        }

        private static void MarshalToIntPtr(CodeGenerator cg, OverflowBehavior ob, LocalBuilder localIntPtrDest)
        {
            cg.LoadInt((int)ob);
            cg.Call(typeof(ProxyGenerator).GetMethod("NumberToIntPtr", new Type[] { typeof(object), typeof(OverflowBehavior) }));
            cg.StoreLocal(localIntPtrDest);
        }

        private static void UnmarshalFromIntPtr(CodeGenerator cg)
        {
            cg.Call(typeof(BigInteger).GetMethod("FromIntPtr"));
        }

        
        public static sbyte NumberToSByte(object obj, OverflowBehavior ob)
        {
            if (obj is BigInteger)
            {
                return ((BigInteger)obj).GetSByteValue(ob);
            }
            else if (obj is BigRational)
            {
                return ((BigRational)obj).Round().GetSByteValue(ob);
            }
            else if (obj is double)
            {
                return (sbyte)(double)obj;
            }
            else throw new SchemeRuntimeException("BigInteger or BigRational or double expected");
        }

        private static void MarshalToSByte(CodeGenerator cg, OverflowBehavior ob, LocalBuilder localSByteDest)
        {
            cg.LoadInt((int)ob);
            cg.Call(typeof(ProxyGenerator).GetMethod("NumberToSByte", new Type[] { typeof(object), typeof(OverflowBehavior) }));
            cg.StoreLocal(localSByteDest);
        }

        private static void UnmarshalFromSByte(CodeGenerator cg)
        {
            cg.Call(typeof(BigInteger).GetMethod("FromSByte"));
        }

        
        public static ushort NumberToUInt16(object obj, OverflowBehavior ob)
        {
            if (obj is BigInteger)
            {
                return ((BigInteger)obj).GetUInt16Value(ob);
            }
            else if (obj is BigRational)
            {
                return ((BigRational)obj).Round().GetUInt16Value(ob);
            }
            else if (obj is double)
            {
                return (ushort)(double)obj;
            }
            else throw new SchemeRuntimeException("BigInteger or BigRational or double expected");
        }

        private static void MarshalToUInt16(CodeGenerator cg, OverflowBehavior ob, LocalBuilder localUInt16Dest)
        {
            cg.LoadInt((int)ob);
            cg.Call(typeof(ProxyGenerator).GetMethod("NumberToUInt16", new Type[] { typeof(object), typeof(OverflowBehavior) }));
            cg.StoreLocal(localUInt16Dest);
        }

        private static void UnmarshalFromUInt16(CodeGenerator cg)
        {
            cg.Call(typeof(BigInteger).GetMethod("FromUInt16"));
        }

        
        public static uint NumberToUInt32(object obj, OverflowBehavior ob)
        {
            if (obj is BigInteger)
            {
                return ((BigInteger)obj).GetUInt32Value(ob);
            }
            else if (obj is BigRational)
            {
                return ((BigRational)obj).Round().GetUInt32Value(ob);
            }
            else if (obj is double)
            {
                return (uint)(double)obj;
            }
            else throw new SchemeRuntimeException("BigInteger or BigRational or double expected");
        }

        private static void MarshalToUInt32(CodeGenerator cg, OverflowBehavior ob, LocalBuilder localUInt32Dest)
        {
            cg.LoadInt((int)ob);
            cg.Call(typeof(ProxyGenerator).GetMethod("NumberToUInt32", new Type[] { typeof(object), typeof(OverflowBehavior) }));
            cg.StoreLocal(localUInt32Dest);
        }

        private static void UnmarshalFromUInt32(CodeGenerator cg)
        {
            cg.Call(typeof(BigInteger).GetMethod("FromUInt32"));
        }

        
        public static ulong NumberToUInt64(object obj, OverflowBehavior ob)
        {
            if (obj is BigInteger)
            {
                return ((BigInteger)obj).GetUInt64Value(ob);
            }
            else if (obj is BigRational)
            {
                return ((BigRational)obj).Round().GetUInt64Value(ob);
            }
            else if (obj is double)
            {
                return (ulong)(double)obj;
            }
            else throw new SchemeRuntimeException("BigInteger or BigRational or double expected");
        }

        private static void MarshalToUInt64(CodeGenerator cg, OverflowBehavior ob, LocalBuilder localUInt64Dest)
        {
            cg.LoadInt((int)ob);
            cg.Call(typeof(ProxyGenerator).GetMethod("NumberToUInt64", new Type[] { typeof(object), typeof(OverflowBehavior) }));
            cg.StoreLocal(localUInt64Dest);
        }

        private static void UnmarshalFromUInt64(CodeGenerator cg)
        {
            cg.Call(typeof(BigInteger).GetMethod("FromUInt64"));
        }

        
        public static UIntPtr NumberToUIntPtr(object obj, OverflowBehavior ob)
        {
            if (obj is BigInteger)
            {
                return ((BigInteger)obj).GetUIntPtrValue(ob);
            }
            else if (obj is BigRational)
            {
                return ((BigRational)obj).Round().GetUIntPtrValue(ob);
            }
            else if (obj is double)
            {
                return (UIntPtr)(double)obj;
            }
            else throw new SchemeRuntimeException("BigInteger or BigRational or double expected");
        }

        private static void MarshalToUIntPtr(CodeGenerator cg, OverflowBehavior ob, LocalBuilder localUIntPtrDest)
        {
            cg.LoadInt((int)ob);
            cg.Call(typeof(ProxyGenerator).GetMethod("NumberToUIntPtr", new Type[] { typeof(object), typeof(OverflowBehavior) }));
            cg.StoreLocal(localUIntPtrDest);
        }

        private static void UnmarshalFromUIntPtr(CodeGenerator cg)
        {
            cg.Call(typeof(BigInteger).GetMethod("FromUIntPtr"));
        }

        
        private static void MarshalToTypeAndStore(CodeGenerator cg, OverflowBehavior beh, LocalBuilder localDest, int globalStateIndex)
        {
                    
            if (localDest.LocalType == typeof(byte))
            {
                MarshalToByte(cg, beh, localDest);
            }
                    
            else if (localDest.LocalType == typeof(short))
            {
                MarshalToInt16(cg, beh, localDest);
            }
                    
            else if (localDest.LocalType == typeof(int))
            {
                MarshalToInt32(cg, beh, localDest);
            }
                    
            else if (localDest.LocalType == typeof(long))
            {
                MarshalToInt64(cg, beh, localDest);
            }
                    
            else if (localDest.LocalType == typeof(IntPtr))
            {
                MarshalToIntPtr(cg, beh, localDest);
            }
                    
            else if (localDest.LocalType == typeof(sbyte))
            {
                MarshalToSByte(cg, beh, localDest);
            }
                    
            else if (localDest.LocalType == typeof(ushort))
            {
                MarshalToUInt16(cg, beh, localDest);
            }
                    
            else if (localDest.LocalType == typeof(uint))
            {
                MarshalToUInt32(cg, beh, localDest);
            }
                    
            else if (localDest.LocalType == typeof(ulong))
            {
                MarshalToUInt64(cg, beh, localDest);
            }
                    
            else if (localDest.LocalType == typeof(UIntPtr))
            {
                MarshalToUIntPtr(cg, beh, localDest);
            }
        
            else if (localDest.LocalType == typeof(string))
            {
                MarshalToString(cg, localDest);
            }
            else if (localDest.LocalType == typeof(bool))
            {
                MarshalToBool(cg, localDest);
            }
            else if (localDest.LocalType == typeof(char))
            {
                MarshalToChar(cg, localDest);
            }
            else if (localDest.LocalType == typeof(float))
            {
                MarshalToFloat(cg, localDest);
            }
            else if (localDest.LocalType == typeof(double))
            {
                MarshalToDouble(cg, localDest);
            }
            else if (localDest.LocalType == typeof(SchemeString))
            {
                MarshalToSchemeString(cg, localDest);
            }
            else if (localDest.LocalType == typeof(BigRational))
            {
                MarshalToBigRational(cg, localDest);
            }
            else if (typeof(IDisposable).IsAssignableFrom(localDest.LocalType))
            {
                MarshalToIDisposable(cg, localDest, globalStateIndex);
            }
            else if (localDest.LocalType == typeof(object))
            {
                MarshalToObject(cg, localDest);
            }
            else if (localDest.LocalType.IsValueType)
            {
                MarshalToValueType(cg, localDest);
            }
            else
            {
                MarshalToSpecificClass(cg, localDest);
            }
        }

        private static void UnmarshalFromType(CodeGenerator cg, Type t, string disposableName, int globalStateIndex)
        {
                    
            if (t == typeof(byte))
            {
                UnmarshalFromByte(cg);
            }
                    
            else if (t == typeof(short))
            {
                UnmarshalFromInt16(cg);
            }
                    
            else if (t == typeof(int))
            {
                UnmarshalFromInt32(cg);
            }
                    
            else if (t == typeof(long))
            {
                UnmarshalFromInt64(cg);
            }
                    
            else if (t == typeof(IntPtr))
            {
                UnmarshalFromIntPtr(cg);
            }
                    
            else if (t == typeof(sbyte))
            {
                UnmarshalFromSByte(cg);
            }
                    
            else if (t == typeof(ushort))
            {
                UnmarshalFromUInt16(cg);
            }
                    
            else if (t == typeof(uint))
            {
                UnmarshalFromUInt32(cg);
            }
                    
            else if (t == typeof(ulong))
            {
                UnmarshalFromUInt64(cg);
            }
                    
            else if (t == typeof(UIntPtr))
            {
                UnmarshalFromUIntPtr(cg);
            }
        
            else if (t == typeof(void))
            {
                UnmarshalFromVoid(cg);
            }
            else if (t == typeof(string))
            {
                UnmarshalFromString(cg);
            }
            else if (t == typeof(bool))
            {
                UnmarshalFromBool(cg);
            }
            else if (t == typeof(char))
            {
                UnmarshalFromChar(cg);
            }
            else if (t == typeof(SchemeString))
            {
                UnmarshalFromSchemeString(cg);
            }
            else if (t == typeof(BigRational))
            {
                UnmarshalFromBigRational(cg);
            }
            else if (t == typeof(float))
            {
                UnmarshalFromFloat(cg);
            }
            else if (typeof(IDisposable).IsAssignableFrom(t))
            {
                UnmarshalFromIDisposable(cg, disposableName, globalStateIndex);
            }
            else if (t.IsValueType)
            {
                cg.Box(t);
            }
            else
            {
                ;
                // do nothing: object will pass directly to Scheme
            }
        }
    }
}