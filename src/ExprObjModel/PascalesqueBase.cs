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
using System.Linq;

using Symbol = ExprObjModel.Symbol;

namespace Pascalesque
{
    [Serializable]
    public class PascalesqueException : Exception
    {
        public PascalesqueException() { }
        public PascalesqueException(string message) : base(message) { }
        public PascalesqueException(string message, Exception inner) : base(message, inner) { }
        protected PascalesqueException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    public static class ExtMethods
    {
        #region ILGenerator Extension Methods

        public static void Add(this ILGenerator ilg) { ilg.Emit(OpCodes.Add); }
        public static void AddOvf(this ILGenerator ilg) { ilg.Emit(OpCodes.Add_Ovf); }
        public static void AddOvfUn(this ILGenerator ilg) { ilg.Emit(OpCodes.Add_Ovf_Un); }

        public static void Sub(this ILGenerator ilg) { ilg.Emit(OpCodes.Sub); }
        public static void SubOvf(this ILGenerator ilg) { ilg.Emit(OpCodes.Sub_Ovf); }
        public static void SubOvfUn(this ILGenerator ilg) { ilg.Emit(OpCodes.Sub_Ovf_Un); }

        public static void Mul(this ILGenerator ilg) { ilg.Emit(OpCodes.Mul); }
        public static void MulOvf(this ILGenerator ilg) { ilg.Emit(OpCodes.Mul_Ovf); }
        public static void MulOvfUn(this ILGenerator ilg) { ilg.Emit(OpCodes.Mul_Ovf_Un); }

        public static void Div(this ILGenerator ilg) { ilg.Emit(OpCodes.Div); }
        public static void DivUn(this ILGenerator ilg) { ilg.Emit(OpCodes.Div_Un); }

        public static void Rem(this ILGenerator ilg) { ilg.Emit(OpCodes.Rem); }
        public static void RemUn(this ILGenerator ilg) { ilg.Emit(OpCodes.Rem_Un); }

        public static void And(this ILGenerator ilg) { ilg.Emit(OpCodes.And); }
        public static void Or(this ILGenerator ilg) { ilg.Emit(OpCodes.Or); }
        public static void Xor(this ILGenerator ilg) { ilg.Emit(OpCodes.Xor); }
        public static void Invert(this ILGenerator ilg) { ilg.Emit(OpCodes.Not); }
        public static void Negate(this ILGenerator ilg) { ilg.Emit(OpCodes.Neg); }

        public static void Shl(this ILGenerator ilg) { ilg.Emit(OpCodes.Shl); }
        public static void Shr(this ILGenerator ilg) { ilg.Emit(OpCodes.Shr); }
        public static void ShrUn(this ILGenerator ilg) { ilg.Emit(OpCodes.Shr_Un); }

        public static void Dup(this ILGenerator ilg)
        {
            ilg.Emit(OpCodes.Dup);
        }

        public static void Pop(this ILGenerator ilg)
        {
            ilg.Emit(OpCodes.Pop);
        }

        public static void Not(this ILGenerator ilg)
        {
            Label l1 = ilg.DefineLabel();
            Label l2 = ilg.DefineLabel();
            ilg.Emit(OpCodes.Brfalse_S, l1);
            ilg.LoadInt(0);
            ilg.Emit(OpCodes.Br_S, l2);
            ilg.MarkLabel(l1);
            ilg.LoadInt(1);
            ilg.MarkLabel(l2);
        }

        public static void LoadLocal(this ILGenerator ilg, int index)
        {
            if (index == 0)
            {
                ilg.Emit(OpCodes.Ldloc_0);
            }
            else if (index == 1)
            {
                ilg.Emit(OpCodes.Ldloc_1);
            }
            else if (index == 2)
            {
                ilg.Emit(OpCodes.Ldloc_2);
            }
            else if (index == 3)
            {
                ilg.Emit(OpCodes.Ldloc_3);
            }
            else if (index < 256)
            {
                ilg.Emit(OpCodes.Ldloc_S, (byte)index);
            }
            else
            {
                ilg.Emit(OpCodes.Ldloc, index);
            }
        }

        public static void LoadLocal(this ILGenerator ilg, LocalBuilder lb)
        {
            ilg.LoadLocal(lb.LocalIndex);
        }

        public static void LoadLocalAddress(this ILGenerator ilg, int index)
        {
            if (index < 256)
            {
                ilg.Emit(OpCodes.Ldloca_S, (byte)index);
            }
            else
            {
                ilg.Emit(OpCodes.Ldloca, index);
            }
        }

        public static void LoadLocalAddress(this ILGenerator ilg, LocalBuilder lb)
        {
            ilg.LoadLocalAddress(lb.LocalIndex);
        }

        public static void StoreLocal(this ILGenerator ilg, int index)
        {
            if (index == 0)
            {
                ilg.Emit(OpCodes.Stloc_0);
            }
            else if (index == 1)
            {
                ilg.Emit(OpCodes.Stloc_1);
            }
            else if (index == 2)
            {
                ilg.Emit(OpCodes.Stloc_2);
            }
            else if (index == 3)
            {
                ilg.Emit(OpCodes.Stloc_3);
            }
            else if (index < 256)
            {
                ilg.Emit(OpCodes.Stloc_S, (byte)index);
            }
            else
            {
                ilg.Emit(OpCodes.Stloc, index);
            }
        }

        public static void StoreLocal(this ILGenerator ilg, LocalBuilder lb)
        {
            ilg.StoreLocal(lb.LocalIndex);
        }

        public static void LoadArg(this ILGenerator ilg, int index)
        {
            if (index == 0)
            {
                ilg.Emit(OpCodes.Ldarg_0);
            }
            else if (index == 1)
            {
                ilg.Emit(OpCodes.Ldarg_1);
            }
            else if (index == 2)
            {
                ilg.Emit(OpCodes.Ldarg_2);
            }
            else if (index == 3)
            {
                ilg.Emit(OpCodes.Ldarg_3);
            }
            else if (index < 256)
            {
                ilg.Emit(OpCodes.Ldarg_S, (byte)index);
            }
            else
            {
                ilg.Emit(OpCodes.Ldarg, index);
            }
        }

        public static void LoadArgAddress(this ILGenerator ilg, int index)
        {
            if (index < 256)
            {
                ilg.Emit(OpCodes.Ldarga_S, (byte)index);
            }
            else
            {
                ilg.Emit(OpCodes.Ldarga, index);
            }
        }

        public static void StoreArg(this ILGenerator ilg, int index)
        {
            if (index < 256)
            {
                ilg.Emit(OpCodes.Starg_S, (byte)index);
            }
            else
            {
                ilg.Emit(OpCodes.Starg, index);
            }
        }

        public static void LoadInt(this ILGenerator ilg, int literal)
        {
            if (literal == 0)
            {
                ilg.Emit(OpCodes.Ldc_I4_0);
            }
            else if (literal == 1)
            {
                ilg.Emit(OpCodes.Ldc_I4_1);
            }
            else if (literal == 2)
            {
                ilg.Emit(OpCodes.Ldc_I4_2);
            }
            else if (literal == 3)
            {
                ilg.Emit(OpCodes.Ldc_I4_3);
            }
            else if (literal == 4)
            {
                ilg.Emit(OpCodes.Ldc_I4_4);
            }
            else if (literal == 5)
            {
                ilg.Emit(OpCodes.Ldc_I4_5);
            }
            else if (literal == 6)
            {
                ilg.Emit(OpCodes.Ldc_I4_6);
            }
            else if (literal == 7)
            {
                ilg.Emit(OpCodes.Ldc_I4_7);
            }
            else if (literal == 8)
            {
                ilg.Emit(OpCodes.Ldc_I4_8);
            }
            else if (literal == -1)
            {
                ilg.Emit(OpCodes.Ldc_I4_M1);
            }
            else if (literal >= -128 && literal <= 127)
            {
                ilg.Emit(OpCodes.Ldc_I4_S, unchecked((byte)literal));
            }
            else
            {
                ilg.Emit(OpCodes.Ldc_I4, literal);
            }
        }

        public static void LoadLong(this ILGenerator ilg, long literal)
        {
            ilg.Emit(OpCodes.Ldc_I8, literal);
        }

        public static void LoadFloat(this ILGenerator ilg, float literal)
        {
            ilg.Emit(OpCodes.Ldc_R4, literal);
        }

        public static void LoadDouble(this ILGenerator ilg, double literal)
        {
            ilg.Emit(OpCodes.Ldc_R8, literal);
        }

        public static void LoadString(this ILGenerator ilg, string literal)
        {
            ilg.Emit(OpCodes.Ldstr, literal);
        }

        public static void LoadNullPtr(this ILGenerator ilg)
        {
            ilg.Emit(OpCodes.Ldnull);
        }

        public static void LoadField(this ILGenerator ilg, FieldInfo fi) // ( objref -- value )
        {
            ilg.Emit(OpCodes.Ldfld, fi);
        }

        public static void LoadFieldAddress(this ILGenerator ilg, FieldInfo fi) // ( objref -- ptr )
        {
            ilg.Emit(OpCodes.Ldflda, fi);
        }

        public static void StoreField(this ILGenerator ilg, FieldInfo fi) // ( objref value -- )
        {
            ilg.Emit(OpCodes.Stfld, fi);
        }

        public static void LoadStaticField(this ILGenerator ilg, FieldInfo fi) // ( -- value )
        {
            ilg.Emit(OpCodes.Ldsfld, fi);
        }

        public static void LoadStaticFieldAddress(this ILGenerator ilg, FieldInfo fi) // ( -- ptr )
        {
            ilg.Emit(OpCodes.Ldsflda, fi);
        }

        public static void StoreStaticField(this ILGenerator ilg, FieldInfo fi) // ( value -- )
        {
            ilg.Emit(OpCodes.Stsfld, fi);
        }

        public static void LoadFunction(this ILGenerator ilg, MethodInfo mi)
        {
            ilg.Emit(OpCodes.Ldftn, mi);
        }

        public static void LoadToken(this ILGenerator ilg, Type t) // ( -- token )
        {
            ilg.Emit(OpCodes.Ldtoken, t);
        }

        public static void LoadToken(this ILGenerator ilg, TypeToken t) // ( -- token )
        {
            ilg.Emit(OpCodes.Ldtoken, t.Token);
        }

        public static void LoadToken(this ILGenerator ilg, MethodInfo mi) // ( -- token )
        {
            ilg.Emit(OpCodes.Ldtoken, mi);
        }

        public static void LoadToken(this ILGenerator ilg, ConstructorInfo ci) // ( -- token )
        {
            ilg.Emit(OpCodes.Ldtoken, ci);
        }

        public static void LoadToken(this ILGenerator ilg, FieldInfo fi) // ( -- token )
        {
            ilg.Emit(OpCodes.Ldtoken, fi);
        }

        public static void LoadToken(this ILGenerator ilg, MethodToken t)
        {
            ilg.Emit(OpCodes.Ldtoken, t.Token);
        }

        public static void LoadToken(this ILGenerator ilg, FieldToken t)
        {
            ilg.Emit(OpCodes.Ldtoken, t.Token);
        }

        public static void NewObj(this ILGenerator ilg, ConstructorInfo ci) { ilg.Emit(OpCodes.Newobj, ci); }

        public static void Throw(this ILGenerator ilg) { ilg.Emit(OpCodes.Throw); }

        public static void Tail(this ILGenerator ilg) { ilg.Emit(OpCodes.Tailcall); }

        public static void Call(this ILGenerator ilg, MethodInfo mi) { ilg.Emit(OpCodes.Call, mi); }

        public static void Call(this ILGenerator ilg, ConstructorInfo ci) { ilg.Emit(OpCodes.Call, ci); }

        public static void CallVirt(this ILGenerator ilg, MethodInfo mi) { ilg.Emit(OpCodes.Callvirt, mi); }

        public static void Return(this ILGenerator ilg) { ilg.Emit(OpCodes.Ret); }

        public static void IsInst(this ILGenerator ilg, Type t) { ilg.Emit(OpCodes.Isinst, t); }

        public static void CastClass(this ILGenerator ilg, Type t) { ilg.Emit(OpCodes.Castclass, t); }

        public static void Ceq(this ILGenerator ilg) { ilg.Emit(OpCodes.Ceq); }
        public static void Clt(this ILGenerator ilg) { ilg.Emit(OpCodes.Clt); }
        public static void CltUn(this ILGenerator ilg) { ilg.Emit(OpCodes.Clt_Un); }
        public static void Cgt(this ILGenerator ilg) { ilg.Emit(OpCodes.Cgt); }
        public static void CgtUn(this ILGenerator ilg) { ilg.Emit(OpCodes.Cgt_Un); }

        public static void Conv_I(this ILGenerator ilg) { ilg.Emit(OpCodes.Conv_I); }
        public static void Conv_I1(this ILGenerator ilg) { ilg.Emit(OpCodes.Conv_I1); }
        public static void Conv_I2(this ILGenerator ilg) { ilg.Emit(OpCodes.Conv_I2); }
        public static void Conv_I4(this ILGenerator ilg) { ilg.Emit(OpCodes.Conv_I4); }
        public static void Conv_I8(this ILGenerator ilg) { ilg.Emit(OpCodes.Conv_I8); }

        public static void Conv_Ovf_I(this ILGenerator ilg) { ilg.Emit(OpCodes.Conv_Ovf_I); }
        public static void Conv_Ovf_I1(this ILGenerator ilg) { ilg.Emit(OpCodes.Conv_Ovf_I1); }
        public static void Conv_Ovf_I2(this ILGenerator ilg) { ilg.Emit(OpCodes.Conv_Ovf_I2); }
        public static void Conv_Ovf_I4(this ILGenerator ilg) { ilg.Emit(OpCodes.Conv_Ovf_I4); }
        public static void Conv_Ovf_I8(this ILGenerator ilg) { ilg.Emit(OpCodes.Conv_Ovf_I8); }

        public static void Conv_Ovf_I_Un(this ILGenerator ilg) { ilg.Emit(OpCodes.Conv_Ovf_I_Un); }
        public static void Conv_Ovf_I1_Un(this ILGenerator ilg) { ilg.Emit(OpCodes.Conv_Ovf_I1_Un); }
        public static void Conv_Ovf_I2_Un(this ILGenerator ilg) { ilg.Emit(OpCodes.Conv_Ovf_I2_Un); }
        public static void Conv_Ovf_I4_Un(this ILGenerator ilg) { ilg.Emit(OpCodes.Conv_Ovf_I4_Un); }
        public static void Conv_Ovf_I8_Un(this ILGenerator ilg) { ilg.Emit(OpCodes.Conv_Ovf_I8_Un); }

        public static void Conv_Ovf_U(this ILGenerator ilg) { ilg.Emit(OpCodes.Conv_Ovf_U); }
        public static void Conv_Ovf_U1(this ILGenerator ilg) { ilg.Emit(OpCodes.Conv_Ovf_U1); }
        public static void Conv_Ovf_U2(this ILGenerator ilg) { ilg.Emit(OpCodes.Conv_Ovf_U2); }
        public static void Conv_Ovf_U4(this ILGenerator ilg) { ilg.Emit(OpCodes.Conv_Ovf_U4); }
        public static void Conv_Ovf_U8(this ILGenerator ilg) { ilg.Emit(OpCodes.Conv_Ovf_U8); }

        public static void Conv_Ovf_U_Un(this ILGenerator ilg) { ilg.Emit(OpCodes.Conv_Ovf_U_Un); }
        public static void Conv_Ovf_U1_Un(this ILGenerator ilg) { ilg.Emit(OpCodes.Conv_Ovf_U1_Un); }
        public static void Conv_Ovf_U2_Un(this ILGenerator ilg) { ilg.Emit(OpCodes.Conv_Ovf_U2_Un); }
        public static void Conv_Ovf_U4_Un(this ILGenerator ilg) { ilg.Emit(OpCodes.Conv_Ovf_U4_Un); }
        public static void Conv_Ovf_U8_Un(this ILGenerator ilg) { ilg.Emit(OpCodes.Conv_Ovf_U8_Un); }

        public static void Conv_R_Un(this ILGenerator ilg) { ilg.Emit(OpCodes.Conv_R_Un); }
        public static void Conv_R4(this ILGenerator ilg) { ilg.Emit(OpCodes.Conv_R4); }
        public static void Conv_R8(this ILGenerator ilg) { ilg.Emit(OpCodes.Conv_R8); }

        public static void Conv_U(this ILGenerator ilg) { ilg.Emit(OpCodes.Conv_U); }
        public static void Conv_U1(this ILGenerator ilg) { ilg.Emit(OpCodes.Conv_U1); }
        public static void Conv_U2(this ILGenerator ilg) { ilg.Emit(OpCodes.Conv_U2); }
        public static void Conv_U4(this ILGenerator ilg) { ilg.Emit(OpCodes.Conv_U4); }
        public static void Conv_U8(this ILGenerator ilg) { ilg.Emit(OpCodes.Conv_U8); }

        public static void SizeOf(this ILGenerator ilg, Type t) { ilg.Emit(OpCodes.Sizeof, t); }

        public static void LoadObjRef(this ILGenerator ilg) { ilg.Emit(OpCodes.Ldind_Ref); }

        public static void Unaligned(this ILGenerator ilg, Alignment a)
        {
            byte b;
            switch (a)
            {
                case Alignment.One: b = 1; break;
                case Alignment.Two: b = 2; break;
                case Alignment.Four: b = 4; break;
                default: throw new ArgumentException("Unknown alignment");
            }
            ilg.Emit(OpCodes.Unaligned, b);
        }

        public static void LoadObjIndirect(this ILGenerator ilg, Type t)
        {
            if (t == typeof(byte))
            {
                ilg.Emit(OpCodes.Ldind_U1);
            }
            else if (t == typeof(sbyte))
            {
                ilg.Emit(OpCodes.Ldind_I1);
            }
            else if (t == typeof(ushort))
            {
                ilg.Emit(OpCodes.Ldind_U2);
            }
            else if (t == typeof(short))
            {
                ilg.Emit(OpCodes.Ldind_I2);
            }
            else if (t == typeof(uint))
            {
                ilg.Emit(OpCodes.Ldind_U4);
            }
            else if (t == typeof(int))
            {
                ilg.Emit(OpCodes.Ldind_I4);
            }
            else if (t == typeof(long) || t == typeof(ulong))
            {
                ilg.Emit(OpCodes.Ldind_I8);
            }
            else if (t == typeof(IntPtr) || t == typeof(UIntPtr))
            {
                ilg.Emit(OpCodes.Ldind_I);
            }
            else if (t == typeof(float))
            {
                ilg.Emit(OpCodes.Ldind_R4);
            }
            else if (t == typeof(double))
            {
                ilg.Emit(OpCodes.Ldind_R8);
            }
            else if (t.IsValueType)
            {
                ilg.Emit(OpCodes.Ldobj, t);
            }
            else
            {
                ilg.Emit(OpCodes.Ldind_Ref);
            }
        }

        public static void StoreObjIndirect(this ILGenerator ilg, Type t)
        {
            if (t == typeof(sbyte) || t == typeof(byte))
            {
                ilg.Emit(OpCodes.Stind_I1);
            }
            else if (t == typeof(short) || t == typeof(ushort))
            {
                ilg.Emit(OpCodes.Stind_I2);
            }
            else if (t == typeof(int) || t == typeof(uint))
            {
                ilg.Emit(OpCodes.Stind_I4);
            }
            else if (t == typeof(long) || t == typeof(ulong))
            {
                ilg.Emit(OpCodes.Stind_I8);
            }
            else if (t == typeof(System.IntPtr) || t == typeof(System.UIntPtr))
            {
                ilg.Emit(OpCodes.Stind_I);
            }
            else if (t == typeof(float))
            {
                ilg.Emit(OpCodes.Stind_R4);
            }
            else if (t == typeof(double))
            {
                ilg.Emit(OpCodes.Stind_R8);
            }
            else if (t.IsValueType)
            {
                ilg.Emit(OpCodes.Stobj, t);
            }
            else
            {
                ilg.Emit(OpCodes.Stind_Ref);
            }
        }

        public static void LoadElement(this ILGenerator ilg, Type t)
        {
            if (t == typeof(sbyte))
            {
                ilg.Emit(OpCodes.Ldelem_I1);
            }
            else if (t == typeof(byte))
            {
                ilg.Emit(OpCodes.Ldelem_U1);
            }
            else if (t == typeof(short))
            {
                ilg.Emit(OpCodes.Ldelem_I2);
            }
            else if (t == typeof(ushort))
            {
                ilg.Emit(OpCodes.Ldelem_U2);
            }
            else if (t == typeof(int))
            {
                ilg.Emit(OpCodes.Ldelem_I4);
            }
            else if (t == typeof(uint))
            {
                ilg.Emit(OpCodes.Ldelem_U4);
            }
            else if (t == typeof(long) || t == typeof(ulong))
            {
                ilg.Emit(OpCodes.Ldelem_I8);
            }
            else if (t == typeof(IntPtr) || t == typeof(UIntPtr))
            {
                ilg.Emit(OpCodes.Ldelem_I);
            }
            else if (t == typeof(float))
            {
                ilg.Emit(OpCodes.Ldelem_R4);
            }
            else if (t == typeof(double))
            {
                ilg.Emit(OpCodes.Ldelem_R8);
            }
            else if (!(t.IsValueType))
            {
                ilg.Emit(OpCodes.Ldelem_Ref);
            }
            else
            {
                ilg.Emit(OpCodes.Ldelem, t);
            }
        }

        public static void LoadElementAddress(this ILGenerator ilg, Type t)
        {
            ilg.Emit(OpCodes.Ldelema, t);
        }

        public static void StoreElement(this ILGenerator ilg, Type t)
        {
            if (t == typeof(sbyte) || t == typeof(byte))
            {
                ilg.Emit(OpCodes.Stelem_I1);
            }
            else if (t == typeof(short) || t == typeof(ushort))
            {
                ilg.Emit(OpCodes.Stelem_I2);
            }
            else if (t == typeof(int) || t == typeof(uint))
            {
                ilg.Emit(OpCodes.Stelem_I4);
            }
            else if (t == typeof(long) || t == typeof(ulong))
            {
                ilg.Emit(OpCodes.Stelem_I8);
            }
            else if (t == typeof(IntPtr) || t == typeof(UIntPtr))
            {
                ilg.Emit(OpCodes.Stelem_I);
            }
            else if (t == typeof(float))
            {
                ilg.Emit(OpCodes.Stelem_R4);
            }
            else if (t == typeof(double))
            {
                ilg.Emit(OpCodes.Stelem_R8);
            }
            else if (!(t.IsValueType))
            {
                ilg.Emit(OpCodes.Stelem_Ref);
            }
            else
            {
                ilg.Emit(OpCodes.Stelem, t);
            }
        }

        public static void Box(this ILGenerator ilg, Type t) { ilg.Emit(OpCodes.Box, t); }

        public static void Unbox(this ILGenerator ilg, Type t) { ilg.Emit(OpCodes.Unbox, t); }
        public static void UnboxAny(this ILGenerator ilg, Type t) { ilg.Emit(OpCodes.Unbox_Any, t); }

        public static void Leave(this ILGenerator ilg, Label l) { ilg.Emit(OpCodes.Leave, l); }

        #endregion

        public static bool HasDuplicates<T>(this IEnumerable<T> items)
        {
            HashSet<T> h = new HashSet<T>();
            foreach (T item in items)
            {
                if (h.Contains(item)) return true;
                h.Add(item);
            }
            return false;
        }

        public static IEnumerable<T> AndAlso<T>(this IEnumerable<T> items, T another)
        {
            foreach (T item in items)
            {
                yield return item;
            }
            yield return another;
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> items)
        {
            HashSet<T> h = new HashSet<T>();
            h.UnionWith(items);
            return h;
        }

        public static T Last<T>(this List<T> list)
        {
            if (list.Count == 0) throw new IndexOutOfRangeException();
            return list[list.Count - 1];
        }

        public static EnvSpec EnvSpecUnion(this IEnumerable<EnvSpec> envSpecs)
        {
            EnvSpec e = EnvSpec.Empty();
            foreach (EnvSpec i in envSpecs)
            {
                e |= i;
            }
            return e;
        }
    }

    public enum Alignment
    {
        One,
        Two,
        Four
    }

    public static partial class Utils
    {
        public static T[] ConcatArrays<T>(T[] a1, T[] a2)
        {
            T[] a3 = new T[a1.Length + a2.Length];
            Array.Copy(a1, 0, a3, 0, a1.Length);
            Array.Copy(a2, 0, a3, a1.Length, a2.Length);
            return a3;
        }
    }

    public class Box<T>
    {
        private T item;
        private bool inited = false;

        public Box()
        {
            item = default(T);
            inited = false;
        }

        public Box(T item)
        {
            this.item = item;
            inited = true;
        }

        public T Value
        {
            get
            {
                if (!inited) throw new PascalesqueException("Runtime: attempt to read from an uninitialized box");
                return item;
            }
            set
            {
                item = value;
                inited = true;
            }
        }
    }

    public class VarSpec
    {
        private bool isWritten;
        private bool isCaptured;

        public VarSpec(bool isWritten, bool isCaptured)
        {
            this.isWritten = isWritten;
            this.isCaptured = isCaptured;
        }

        public bool IsWritten { get { return isWritten; } }

        public bool IsCaptured { get { return isCaptured; } }

        public static VarSpec operator |(VarSpec a, VarSpec b)
        {
            return new VarSpec(a.IsWritten || b.IsWritten, a.IsCaptured || b.IsCaptured);
        }
    }
    
    public class EnvSpec
    {
        private Dictionary<Symbol, VarSpec> data;

        private EnvSpec()
        {
            data = new Dictionary<Symbol, VarSpec>();
        }

        private EnvSpec(Symbol s, VarSpec v)
        {
            data = new Dictionary<Symbol, VarSpec>();
            data.Add(s, v);
        }

        public static EnvSpec Empty()
        {
            return new EnvSpec();
        }

        public bool IsEmpty { get { return data.Count == 0; } }

        public static EnvSpec Singleton(Symbol s, VarSpec v)
        {
            return new EnvSpec(s, v);
        }

        public static EnvSpec FromSequence(IEnumerable<Tuple<Symbol, VarSpec>> seq)
        {
            EnvSpec r = new EnvSpec();
            foreach (Tuple<Symbol, VarSpec> item in seq)
            {
                if (r.data.ContainsKey(item.Item1))
                {
                    r.data[item.Item1] |= item.Item2;
                }
                else
                {
                    r.data.Add(item.Item1, item.Item2);
                }
            }
            return r;
        }

        public static EnvSpec CaptureAll(EnvSpec e)
        {
            EnvSpec r = new EnvSpec();
            foreach (KeyValuePair<Symbol, VarSpec> item in e.data)
            {
                r.data.Add(item.Key, new VarSpec(item.Value.IsWritten, true));
            }
            return r;
        }

        public bool ContainsKey(Symbol s) { return data.ContainsKey(s); }

        public HashSet<Symbol> Keys
        {
            get
            {
                return ExprObjModel.Utils.ToHashSet(data.Keys);
            }
        }

        public IEnumerable<VarSpec> Values
        {
            get
            {
                return data.Values;
            }
        }

        public VarSpec this[Symbol s]
        {
            get
            {
                return data[s];
            }
        }

        public static EnvSpec operator |(EnvSpec a, EnvSpec b)
        {
            EnvSpec r = new EnvSpec();
            foreach (KeyValuePair<Symbol, VarSpec> d in a.data)
            {
                r.data.Add(d.Key, d.Value);
            }
            foreach (KeyValuePair<Symbol, VarSpec> d in b.data)
            {
                if (r.data.ContainsKey(d.Key))
                {
                    VarSpec dOld = r.data[d.Key];
                    r.data[d.Key] = dOld | d.Value;
                }
                else
                {
                    r.data.Add(d.Key, d.Value);
                }
            }
            return r;
        }

        public static EnvSpec operator -(EnvSpec a, IEnumerable<Symbol> b)
        {
            HashSet<Symbol> hs = new HashSet<Symbol>();
            hs.UnionWith(b);

            EnvSpec r = new EnvSpec();
            foreach (KeyValuePair<Symbol, VarSpec> kvp in a.data)
            {
                if (!(hs.Contains(kvp.Key)))
                {
                    r.data.Add(kvp.Key, kvp.Value);
                }
            }

            return r;
        }

        public static EnvSpec operator -(EnvSpec a, Symbol b)
        {
            EnvSpec r = new EnvSpec();
            foreach (KeyValuePair<Symbol, VarSpec> kvp in a.data)
            {
                if (kvp.Key != b)
                {
                    r.data.Add(kvp.Key, kvp.Value);
                }
            }

            return r;
        }

        public static EnvSpec Add(EnvSpec a, Symbol s, VarSpec v)
        {
            EnvSpec r = new EnvSpec();
            foreach (KeyValuePair<Symbol, VarSpec> d in a.data)
            {
                r.data.Add(d.Key, d.Value);
            }
            if (r.data.ContainsKey(s))
            {
                VarSpec vOld = r.data[s];
                r.data[s] = v | vOld;
            }
            else
            {
                r.data.Add(s, v);
            }
            return r;
        }

        public static EnvSpec Add(EnvSpec a, IEnumerable<Tuple<Symbol, VarSpec>> vars)
        {
            EnvSpec r = new EnvSpec();
            foreach (KeyValuePair<Symbol, VarSpec> d in a.data)
            {
                r.data.Add(d.Key, d.Value);
            }
            foreach (Tuple<Symbol, VarSpec> d in vars)
            {
                if (r.data.ContainsKey(d.Item1))
                {
                    VarSpec vOld = r.data[d.Item1];
                    r.data[d.Item1] = d.Item2 | vOld;
                }
                else
                {
                    r.data.Add(d.Item1, d.Item2);
                }
            }
            return r;
        }

        public IEnumerable<KeyValuePair<Symbol, VarSpec>> AsEnumerable()
        {
            return data.AsEnumerable();
        }
    }

    public enum BinaryOp
    {
        Add,
        Sub,
        Mul,
        Div,
        Rem,
        And,
        Or,
        Xor,
        Shl,
        Shr,
        Atan2,
        IEEERemainder,
        LogBase,
        Max,
        Min,
        Pow,
    }

    public enum UnaryOp
    {
        Invert,
        Negate,
        Not,
        Abs,
        Acos,
        Asin,
        Atan,
        Ceil,
        Cos,
        Cosh,
        Exp,
        Floor,
        Log,
        Log10,
        Round,
        Sign,
        Sin,
        Sinh,
        Sqrt,
        Tan,
        Tanh,
        Trunc
    }

    public enum ConvertTo
    {
        Byte,
        Short,
        Int,
        Long,
        IntPtr,
        SByte,
        UShort,
        UInt,
        ULong,
        UIntPtr,
        Float,
        Double
    }

    public enum ActualStackType
    {
        Int32,
        IntPtr,
        Int64,
        Float,
    }

    public enum Comparison
    {
        LessThan,
        GreaterThan,
        LessEqual,
        GreaterEqual,
        Equal,
        NotEqual
    }

    [Flags]
    public enum MethodCriteria
    {
        None = 0,
        IsPublic = 1,
        IsNotPublic = 2,
        IsStatic = 4,
        IsNotStatic = 8,
        IsSpecialName = 16,
        IsNotSpecialName = 32,
        IsVirtual = 64,
        IsNotVirtual = 128
    }

    [Flags]
    public enum PropertyCriteria
    {
        None = 0,
        IsSpecialName = 1,
        IsNotSpecialName = 2,
        IsGettable = 4,
        IsNotGettable = 8,
        IsSettable = 16,
        IsNotSettable = 32,
        IsPublic = 64,
        IsNotPublic = 128,
        IsStatic = 256,
        IsNotStatic = 512
    }

    public static partial class Utils
    {
        public static bool ArrayMatch<T>(T[] array1, T[] array2, Func<T, T, bool> isEqual)
        {
            int iEnd = array1.Length;
            if (iEnd != array2.Length) return false;
            for (int i = 0; i < iEnd; ++i)
            {
                if (!(isEqual(array1[i], array2[i]))) return false;
            }
            return true;
        }

        public static bool TypesMatch(Type[] array1, Type[] array2)
        {
            return ArrayMatch<Type>(array1, array2, delegate(Type a, Type b) { return a == b; });
        }

        public static bool FitsCriteria(this MethodInfo mi, MethodCriteria criteria)
        {
            if (criteria.HasFlag(MethodCriteria.IsPublic) && !mi.IsPublic) return false;
            if (criteria.HasFlag(MethodCriteria.IsNotPublic) && mi.IsPublic) return false;
            if (criteria.HasFlag(MethodCriteria.IsStatic) && !mi.IsStatic) return false;
            if (criteria.HasFlag(MethodCriteria.IsNotStatic) && mi.IsStatic) return false;
            if (criteria.HasFlag(MethodCriteria.IsSpecialName) && !mi.IsSpecialName) return false;
            if (criteria.HasFlag(MethodCriteria.IsNotSpecialName) && mi.IsSpecialName) return false;
            if (criteria.HasFlag(MethodCriteria.IsVirtual) && !mi.IsVirtual) return false;
            if (criteria.HasFlag(MethodCriteria.IsNotVirtual) && mi.IsVirtual) return false;

            return true;
        }

        public static MethodInfo GetMethod(this Type t, string name, MethodCriteria criteria, Type[] theParams)
        {
            List<MethodInfo> mi =
                t.GetMethods()
                .Where(x => x.Name == name)
                .Where(x => TypesMatch(x.GetParameters().Select(y => y.ParameterType).ToArray(), theParams))
                .Where(x => x.FitsCriteria(criteria))
                .ToList();

            if (mi.Count == 0) throw new PascalesqueException("Method not found");

            if (mi.Count == 2) throw new AmbiguousMatchException();

            return mi[0];
        }

        public static bool IsAny(this PropertyInfo pi, MethodCriteria criteria)
        {
            MethodInfo mi = pi.GetGetMethod();
            if (mi != null)
            {
                if (mi.FitsCriteria(criteria)) return true;
            }
            mi = pi.GetSetMethod();
            if (mi != null)
            {
                if (mi.FitsCriteria(criteria)) return true;
            }
            return false;
        }

        public static bool IsAll(this PropertyInfo pi, MethodCriteria criteria)
        {
            MethodInfo mi = pi.GetGetMethod();
            if (mi != null)
            {
                if (!mi.FitsCriteria(criteria)) return false;
            }
            mi = pi.GetSetMethod();
            if (mi != null)
            {
                if (!mi.FitsCriteria(criteria)) return false;
            }
            return true;
        }

        public static bool FitsCriteria(this PropertyInfo pi, PropertyCriteria criteria)
        {
            if (criteria.HasFlag(PropertyCriteria.IsSpecialName) && !pi.IsSpecialName) return false;
            if (criteria.HasFlag(PropertyCriteria.IsNotSpecialName) && pi.IsSpecialName) return false;
            if (criteria.HasFlag(PropertyCriteria.IsGettable) && pi.GetGetMethod() == null) return false;
            if (criteria.HasFlag(PropertyCriteria.IsNotGettable) && pi.GetGetMethod() != null) return false;
            if (criteria.HasFlag(PropertyCriteria.IsSettable) && pi.GetSetMethod() == null) return false;
            if (criteria.HasFlag(PropertyCriteria.IsNotSettable) && pi.GetSetMethod() == null) return false;
            if (criteria.HasFlag(PropertyCriteria.IsPublic) && !pi.IsAny(MethodCriteria.IsPublic)) return false;
            if (criteria.HasFlag(PropertyCriteria.IsNotPublic) && pi.IsAny(MethodCriteria.IsPublic)) return false;
            if (criteria.HasFlag(PropertyCriteria.IsStatic) && !pi.IsAll(MethodCriteria.IsStatic)) return false;
            if (criteria.HasFlag(PropertyCriteria.IsNotStatic) && pi.IsAll(MethodCriteria.IsStatic)) return false;

            return true;
        }

        public static PropertyInfo GetProperty(this Type t, string name, PropertyCriteria criteria, Type[] theParams)
        {
            List<PropertyInfo> pi =
                t.GetProperties()
                .Where(x => x.Name == name)
                .Where(x => TypesMatch(x.GetIndexParameters().Select(y => y.ParameterType).ToArray(), theParams))
                .Where(x => x.FitsCriteria(criteria))
                .ToList();

            if (pi.Count == 0) throw new PascalesqueException("Property not found");

            if (pi.Count == 2) throw new AmbiguousMatchException();

            return pi[0];
        }

        public static MethodCriteria Combine(this IEnumerable<MethodCriteria> seq)
        {
            MethodCriteria result = MethodCriteria.None;
            foreach (MethodCriteria a in seq)
            {
                result |= a;
            }
            return result;
        }

        public static PropertyCriteria Combine(this IEnumerable<PropertyCriteria> seq)
        {
            PropertyCriteria result = PropertyCriteria.None;
            foreach (PropertyCriteria a in seq)
            {
                result |= a;
            }
            return result;
        }

        private static readonly Type[] tupleTypes = new Type[]
        {
            typeof(Tuple<>),
            typeof(Tuple<,>),
            typeof(Tuple<,,>),
            typeof(Tuple<,,,>),
            typeof(Tuple<,,,,>),
            typeof(Tuple<,,,,,>),
            typeof(Tuple<,,,,,,>)
        };

        public static bool IsTupleType(Type t)
        {
            if (t.IsGenericTypeDefinition) return false; // we're looking for concrete tuple types
            if (!(t.IsGenericType)) return false;
            Type openGeneric = t.GetGenericTypeDefinition();
            return tupleTypes.Any(x => x == openGeneric);
        }

        public static int TupleElements(Type t)
        {
            if (t.IsGenericTypeDefinition)
            {
                int index = Array.FindIndex(tupleTypes, x => x == t);
                if (index == -1) throw new PascalesqueException("Unknown tuple type");
                else return index + 1;
            }
            else if (t.IsGenericType)
            {
                return TupleElements(t.GetGenericTypeDefinition());
            }
            else throw new PascalesqueException("unknown tuple type");
        }

        public static PropertyInfo GetTupleProperty(Type t, int index)
        {
            if (!IsTupleType(t)) throw new ArgumentException("unknown tuple type");
            int x = TupleElements(t);
            if (index < 0 || index >= x) throw new IndexOutOfRangeException("index");
            PropertyInfo p = t.GetProperty("Item" + (index + 1));
            System.Diagnostics.Debug.Assert(p != null);
            return p;
        }
    }
}