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

namespace ExprObjModel.CodeGeneration
{
    public class CodeGenerator
    {
        private ILGenerator ilg;
        private string context;

        private Stack<Label> labelStack;

        public CodeGenerator(ILGenerator ilg, string context)
        {
            this.ilg = ilg;
            this.context = context;
            labelStack = new Stack<Label>();
        }

        public ILGenerator ILGenerator { get { return ilg; } }

        public string Context { get { return context; } }

        #region Arithmetic / Logic

        public void Add() { ilg.Emit(OpCodes.Add); }
        public void AddOvf() { ilg.Emit(OpCodes.Add_Ovf); }
        public void AddOvfUn() { ilg.Emit(OpCodes.Add_Ovf_Un); }

        public void Sub() { ilg.Emit(OpCodes.Sub); }
        public void SubOvf() { ilg.Emit(OpCodes.Sub_Ovf); }
        public void SubOvfUn() { ilg.Emit(OpCodes.Sub_Ovf_Un); }

        public void Mul() { ilg.Emit(OpCodes.Mul); }
        public void MulOvf() { ilg.Emit(OpCodes.Mul_Ovf); }
        public void MulOvfUn() { ilg.Emit(OpCodes.Mul_Ovf_Un); }

        public void Div() { ilg.Emit(OpCodes.Div); }
        public void DivUn() { ilg.Emit(OpCodes.Div_Un); }

        public void Rem() { ilg.Emit(OpCodes.Rem); }
        public void RemUn() { ilg.Emit(OpCodes.Rem_Un); }

        public void And() { ilg.Emit(OpCodes.And); }
        public void Or() { ilg.Emit(OpCodes.Or); }
        public void Xor() { ilg.Emit(OpCodes.Xor); }
        public void Invert() { ilg.Emit(OpCodes.Not); }
        public void Negate() { ilg.Emit(OpCodes.Neg); }

        public void Shl() /* ( value shiftamount -- value ) */ { ilg.Emit(OpCodes.Shl); }
        public void Shr() /* ( value shiftamount -- value ) */ { ilg.Emit(OpCodes.Shr); }
        public void ShrUn() /* ( value shiftamount -- value ) */ { ilg.Emit(OpCodes.Shr_Un); }

        #endregion

        public void Dup() { ilg.Emit(OpCodes.Dup); }
        public void Drop() { ilg.Emit(OpCodes.Pop); }

        public void LoadLocal(LocalBuilder lb)
        {
            if (lb.LocalIndex == 0)
            {
                ilg.Emit(OpCodes.Ldloc_0);
            }
            else if (lb.LocalIndex == 1)
            {
                ilg.Emit(OpCodes.Ldloc_1);
            }
            else if (lb.LocalIndex == 2)
            {
                ilg.Emit(OpCodes.Ldloc_2);
            }
            else if (lb.LocalIndex == 3)
            {
                ilg.Emit(OpCodes.Ldloc_3);
            }
            else if (lb.LocalIndex < 256)
            {
                ilg.Emit(OpCodes.Ldloc_S, (byte)lb.LocalIndex);
            }
            else
            {
                ilg.Emit(OpCodes.Ldloc, lb);
            }
        }

        public void StoreLocal(LocalBuilder lb)
        {
            if (lb.LocalIndex == 0)
            {
                ilg.Emit(OpCodes.Stloc_0);
            }
            else if (lb.LocalIndex == 1)
            {
                ilg.Emit(OpCodes.Stloc_1);
            }
            else if (lb.LocalIndex == 2)
            {
                ilg.Emit(OpCodes.Stloc_2);
            }
            else if (lb.LocalIndex == 3)
            {
                ilg.Emit(OpCodes.Stloc_3);
            }
            else if (lb.LocalIndex < 256)
            {
                ilg.Emit(OpCodes.Stloc_S, (byte)lb.LocalIndex);
            }
            else
            {
                ilg.Emit(OpCodes.Stloc, lb);
            }
        }

        public void LoadArg(int index)
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

        public void StoreArg(int index)
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

        public void LoadInt(int literal)
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

        public void LoadLong(long literal)
        {
            ilg.Emit(OpCodes.Ldc_I8, literal);
        }

        public void LoadFloat(float literal)
        {
            ilg.Emit(OpCodes.Ldc_R4, literal);
        }

        public void LoadDouble(double literal)
        {
            ilg.Emit(OpCodes.Ldc_R8, literal);
        }

        public void LoadString(string literal)
        {
            ilg.Emit(OpCodes.Ldstr, literal);
        }

        public void LoadNullPtr()
        {
            ilg.Emit(OpCodes.Ldnull);
        }

        public void LoadField(FieldInfo fi) // ( objref -- value )
        {
            ilg.Emit(OpCodes.Ldfld, fi);
        }

        public void LoadFieldAddress(FieldInfo fi) // ( objref -- ptr )
        {
            ilg.Emit(OpCodes.Ldflda, fi);
        }

        public void StoreField(FieldInfo fi) // ( objref value -- )
        {
            ilg.Emit(OpCodes.Stfld, fi);
        }

        public void LoadStaticField(FieldInfo fi) // ( -- value )
        {
            ilg.Emit(OpCodes.Ldsfld, fi);
        }

        public void LoadStaticFieldAddress(FieldInfo fi) // ( -- ptr )
        {
            ilg.Emit(OpCodes.Ldsflda, fi);
        }

        public void StoreStaticField(FieldInfo fi) // ( value -- )
        {
            ilg.Emit(OpCodes.Stsfld, fi);
        }

        public void LoadToken(Type t) // ( -- token )
        {
            ilg.Emit(OpCodes.Ldtoken, t);
        }

        public void LoadToken(TypeToken t) // ( -- token )
        {
            ilg.Emit(OpCodes.Ldtoken, t.Token);
        }

        public void LoadToken(MethodInfo mi) // ( -- token )
        {
            ilg.Emit(OpCodes.Ldtoken, mi);
        }

        public void LoadToken(FieldInfo fi) // ( -- token )
        {
            ilg.Emit(OpCodes.Ldtoken, fi);
        }

        public void LoadToken(MethodToken t)
        {
            ilg.Emit(OpCodes.Ldtoken, t.Token);
        }

        public void LoadToken(FieldToken t)
        {
            ilg.Emit(OpCodes.Ldtoken, t.Token);
        }

        public void NewObj(ConstructorInfo ci) { ilg.Emit(OpCodes.Newobj, ci); }

        public void Throw() { ilg.Emit(OpCodes.Throw); }

        public void Tail() { ilg.Emit(OpCodes.Tailcall); }

        public void Call(MethodInfo mi) { ilg.Emit(OpCodes.Call, mi); }

        public void CallVirt(MethodInfo mi) { ilg.Emit(OpCodes.Callvirt, mi); }

        public void Return() { ilg.Emit(OpCodes.Ret); }

        public void IsInst(Type t) { ilg.Emit(OpCodes.Isinst, t); }

        public void CastClass(Type t) { ilg.Emit(OpCodes.Castclass, t); }

        private void SwapLabels()
        {
            Label l = labelStack.Pop();
            Label m = labelStack.Pop();
            labelStack.Push(l);
            labelStack.Push(m);
        }

        public void Ahead(bool useLongForm)
        {
            Label l = ilg.DefineLabel();
            if (useLongForm)
            {
                ilg.Emit(OpCodes.Br, l);
            }
            else
            {
                ilg.Emit(OpCodes.Br_S, l);
            }
            labelStack.Push(l);
        }

        public void Then()
        {
            Label l = labelStack.Pop();
            ilg.MarkLabel(l);
        }

        public void IfNot(OpCode branch)
        {
            Label l = ilg.DefineLabel();
            ilg.Emit(branch, l);
            labelStack.Push(l);
        }

        public void Else(bool useLongForm)
        {
            Ahead(useLongForm);
            SwapLabels();
            Then();
        }

        public void Begin()
        {
            Label l = ilg.DefineLabel();
            ilg.MarkLabel(l);
            labelStack.Push(l);
        }

        public void Again(bool useLongForm)
        {
            Label l = labelStack.Pop();
            if (useLongForm)
            {
                ilg.Emit(OpCodes.Br, l);
            }
            else
            {
                ilg.Emit(OpCodes.Br_S, l);
            }
        }

        public void UntilNot(OpCode branch)
        {
            Label l = labelStack.Pop();
            ilg.Emit(branch, l);
        }

        public void WhileNot(OpCode branch)
        {
            IfNot(branch);
        }

        public void Repeat(bool useLongForm)
        {
            SwapLabels();
            Again(useLongForm);
            Then();
        }

        public LocalBuilder DeclareLocal(Type t)
        {
            return ilg.DeclareLocal(t);
        }

        public void Ceq() { ilg.Emit(OpCodes.Ceq); }
        public void Clt() { ilg.Emit(OpCodes.Clt); }
        public void CltUn() { ilg.Emit(OpCodes.Clt_Un); }
        public void Cgt() { ilg.Emit(OpCodes.Cgt); }
        public void CgtUn() { ilg.Emit(OpCodes.Cgt_Un); }

        public void SizeOf(Type t) { ilg.Emit(OpCodes.Sizeof, t); }
        
        public void LoadObjRef() { ilg.Emit(OpCodes.Ldind_Ref); }

        public void LoadObjIndirect(Type t)
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
            else if (t == typeof(long))
            {
                ilg.Emit(OpCodes.Ldind_I8);
            }
            else if (t == typeof(System.IntPtr))
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
            else
            {
                ilg.Emit(OpCodes.Ldobj, t);
            }
            // Opcodes.Ldind_Ref not represented here
        }

        public void StoreObjRef() { ilg.Emit(OpCodes.Stind_Ref); }

        public void StoreObjIndirect(Type t)
        {
            if (t == typeof(sbyte))
            {
                ilg.Emit(OpCodes.Stind_I1);
            }
            else if (t == typeof(short))
            {
                ilg.Emit(OpCodes.Stind_I2);
            }
            else if (t == typeof(int))
            {
                ilg.Emit(OpCodes.Stind_I4);
            }
            else if (t == typeof(long))
            {
                ilg.Emit(OpCodes.Stind_I8);
            }
            else if (t == typeof(System.IntPtr))
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
            else
            {
                ilg.Emit(OpCodes.Stobj, t);
            }
        }

        public void Box(Type t) { ilg.Emit(OpCodes.Box, t); }

        public void Unbox(Type t) { ilg.Emit(OpCodes.Unbox, t); }
        public void UnboxAny(Type t) { ilg.Emit(OpCodes.Unbox_Any, t); }

        // The general structure of a try...finally block is:

        //    try
        //    {
        //       ...
        //       leave -> x
        //    }
        //    finally
        //    {
        //    }
        //    x:

        public void Try() { Label l = ilg.BeginExceptionBlock(); labelStack.Push(l); }
        public void Catch(Type exceptionType) { ilg.BeginCatchBlock(exceptionType); }
        public void Finally() { ilg.BeginFinallyBlock(); }
        public void EndTryCatchFinally() { ilg.EndExceptionBlock(); labelStack.Pop(); }
        public void Leave() { ilg.Emit(OpCodes.Leave, labelStack.Peek()); }
    }
}
