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
using BigMath;
using System.Reflection.Emit;
using ExprObjModel;
using System.Reflection;

namespace Pascalesque.Two
{
    public interface ICompileStep
    {
        int Phase { get; }
        HashSet2<ItemKey> Inputs { get; }
        HashSet2<ItemKey> Outputs { get; }
        void Compile(ModuleBuilder mb, Dictionary<ItemKey, SaBox<object>> vars);
    }

    public class ParamInfo2
    {
        private Symbol name;
        private TypeReference paramType;

        public ParamInfo2(Symbol name, TypeReference paramType)
        {
            this.name = name;
            this.paramType = paramType;
        }

        public Symbol Name { get { return name; } }

        public TypeReference ParamType { get { return paramType; } }
    }

    public class EnvDescTypesOnly2
    {
        private Dictionary<Symbol, TypeReference> data;

        private EnvDescTypesOnly2()
        {
            data = new Dictionary<Symbol, TypeReference>();
        }

        private EnvDescTypesOnly2(Symbol s, TypeReference t)
        {
            data = new Dictionary<Symbol, TypeReference>();
            data.Add(s, t);
        }

        private EnvDescTypesOnly2(EnvDescTypesOnly2 src)
        {
            data = new Dictionary<Symbol, TypeReference>();
            foreach (KeyValuePair<Symbol, TypeReference> kvp in src.data)
            {
                data.Add(kvp.Key, kvp.Value);
            }
        }

        public static EnvDescTypesOnly2 Empty()
        {
            return new EnvDescTypesOnly2();
        }

        public static EnvDescTypesOnly2 Singleton(Symbol s, TypeReference t)
        {
            return new EnvDescTypesOnly2(s, t);
        }

        [Obsolete]
        public static EnvDescTypesOnly2 FromSequence(IEnumerable<Tuple<Symbol, TypeReference>> seq)
        {
            EnvDescTypesOnly2 e = new EnvDescTypesOnly2();
            foreach (Tuple<Symbol, TypeReference> t in seq)
            {
                e.data.Add(t.Item1, t.Item2);
            }
            return e;
        }

        public static EnvDescTypesOnly2 FromSequence(IEnumerable<ParamInfo2> seq)
        {
            EnvDescTypesOnly2 e = new EnvDescTypesOnly2();
            foreach (ParamInfo2 t in seq)
            {
                e.data.Add(t.Name, t.ParamType);
            }
            return e;
        }

        public static EnvDescTypesOnly2 Shadow(EnvDescTypesOnly2 e, Symbol s, TypeReference t)
        {
            EnvDescTypesOnly2 r = new EnvDescTypesOnly2(e);
            if (r.data.ContainsKey(s)) r.data.Remove(s);
            r.data.Add(s, t);
            return r;
        }

        [Obsolete]
        public static EnvDescTypesOnly2 Shadow(EnvDescTypesOnly2 i, IEnumerable<Tuple<Symbol, TypeReference>> symbols)
        {
            EnvDescTypesOnly2 result = new EnvDescTypesOnly2();
            foreach (KeyValuePair<Symbol, TypeReference> kvp in i.data)
            {
                result.data.Add(kvp.Key, kvp.Value);
            }
            foreach (Tuple<Symbol, TypeReference> t in symbols)
            {
                if (result.data.ContainsKey(t.Item1))
                {
                    result.data[t.Item1] = t.Item2;
                }
                else
                {
                    result.data.Add(t.Item1, t.Item2);
                }
            }
            return result;
        }

        public static EnvDescTypesOnly2 Shadow(EnvDescTypesOnly2 i, IEnumerable<ParamInfo2> symbols)
        {
            EnvDescTypesOnly2 result = new EnvDescTypesOnly2();
            foreach (KeyValuePair<Symbol, TypeReference> kvp in i.data)
            {
                result.data.Add(kvp.Key, kvp.Value);
            }
            foreach (ParamInfo2 t in symbols)
            {
                if (result.data.ContainsKey(t.Name))
                {
                    result.data[t.Name] = t.ParamType;
                }
                else
                {
                    result.data.Add(t.Name, t.ParamType);
                }
            }
            return result;
        }


        public bool ContainsKey(Symbol s) { return data.ContainsKey(s); }

        public HashSet<Symbol> Keys
        {
            get
            {
                return ExprObjModel.Utils.ToHashSet(data.Keys);
            }
        }

        public TypeReference this[Symbol s]
        {
            get
            {
                return data[s];
            }
        }

    }

    public interface IVarDesc2
    {
        TypeReference VarType { get; }
        bool IsBoxed { get; }
        void Fetch(CompileContext2 cc, Dictionary<ItemKey, ExprObjModel.SaBox<object>> references, bool tail);
        void Store(CompileContext2 cc, Dictionary<ItemKey, ExprObjModel.SaBox<object>> references, Action exprToStore, bool tail);
        void FetchBox(CompileContext2 cc, Dictionary<ItemKey, ExprObjModel.SaBox<object>> references, bool tail);
    }

    public class LocalVarDesc2 : IVarDesc2
    {
        private TypeReference varType;
        private bool isBoxed;
        private int index;

        public LocalVarDesc2(TypeReference varType, bool isBoxed, int localIndex)
        {
            this.varType = varType;
            this.isBoxed = isBoxed;
            this.index = localIndex;
        }

        public int LocalIndex { get { return index; } }

        #region IVarDesc Members

        public TypeReference VarType { get { return varType; } }

        public bool IsBoxed { get { return isBoxed; } }

        public void Fetch(CompileContext2 cc, Dictionary<ItemKey, ExprObjModel.SaBox<object>> references, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            ilg.LoadLocal(index);
            if (isBoxed)
            {
                if (tail) ilg.Tail();
                ilg.Call(TypeReference.MakeBoxedType(varType).Resolve(references).GetProperty("Value").GetGetMethod());
                if (tail) ilg.Return();
            }
            else
            {
                if (tail) ilg.Return();
            }
        }

        public void Store(CompileContext2 cc, Dictionary<ItemKey, ExprObjModel.SaBox<object>> references, Action writeExprToStore, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            if (isBoxed)
            {
                ilg.LoadLocal(index);
                writeExprToStore();
                if (tail) ilg.Tail();
                ilg.Call(TypeReference.MakeBoxedType(varType).Resolve(references).GetProperty("Value").GetSetMethod());
                if (tail) ilg.Return();
            }
            else
            {
                writeExprToStore();
                ilg.StoreLocal(index);
                if (tail) ilg.Return();
            }
        }

        public void FetchBox(CompileContext2 cc, Dictionary<ItemKey, ExprObjModel.SaBox<object>> references, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;
            if (isBoxed)
            {
                ilg.LoadLocal(index);
                if (tail) ilg.Return();
            }
            else
            {
                throw new PascalesqueException("Tried to fetch the box of an unboxed local");
            }
        }

        #endregion
    }

    public class ArgVarDesc2 : IVarDesc2
    {
        private TypeReference varType;
        private bool isBoxed;
        private int index;

        public ArgVarDesc2(TypeReference varType, bool isBoxed, int index)
        {
            this.varType = varType;
            this.isBoxed = isBoxed;
            this.index = index;
        }

        #region IVarDesc2 Members

        public TypeReference VarType { get { return varType; } }

        public bool IsBoxed { get { return isBoxed; } }

        public void Fetch(CompileContext2 cc, Dictionary<ItemKey, ExprObjModel.SaBox<object>> references, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            ilg.LoadArg(index);
            if (isBoxed)
            {
                if (tail) ilg.Tail();
                ilg.Call(TypeReference.MakeBoxedType(varType).Resolve(references).GetProperty("Value").GetGetMethod());
                if (tail) ilg.Return();
            }
            else
            {
                if (tail) ilg.Return();
            }
        }

        public void Store(CompileContext2 cc, Dictionary<ItemKey, ExprObjModel.SaBox<object>> references, Action writeExprToStore, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            if (isBoxed)
            {
                ilg.LoadArg(index);
                writeExprToStore();
                if (tail) ilg.Tail();
                ilg.Call(TypeReference.MakeBoxedType(varType).Resolve(references).GetProperty("Value").GetSetMethod());
                if (tail) ilg.Return();
            }
            else
            {
                writeExprToStore();
                ilg.StoreArg(index);
                if (tail) ilg.Return();
            }
        }

        public void FetchBox(CompileContext2 cc, Dictionary<ItemKey, ExprObjModel.SaBox<object>> references, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;
            if (isBoxed)
            {
                ilg.LoadLocal(index);
                if (tail) ilg.Return();
            }
            else
            {
                throw new PascalesqueException("Tried to fetch the box of an unboxed argument");
            }
        }

        #endregion
    }

    public class FieldVarDesc2 : IVarDesc2
    {
        private IVarDesc2 fieldOfWhat;
        private FieldInfo fieldInfo;
        private TypeReference varType;
        private bool isBoxed;

        public FieldVarDesc2(IVarDesc2 fieldOfWhat, FieldInfo fieldInfo, TypeReference varType, bool isBoxed)
        {
            this.fieldOfWhat = fieldOfWhat;
            this.fieldInfo = fieldInfo;
            this.varType = varType;
            this.isBoxed = isBoxed;
        }

        #region IVarDesc Members

        public TypeReference VarType
        {
            get
            {
                return varType;
            }
        }

        public bool IsBoxed { get { return isBoxed; } }

        public void Fetch(CompileContext2 cc, Dictionary<ItemKey, ExprObjModel.SaBox<object>> references, bool tail)
        {
            if (isBoxed && fieldInfo.FieldType != TypeReference.MakeBoxedType(varType).Resolve(references)) throw new PascalesqueException("Field type isn't boxed type");
            ILGenerator ilg = cc.ILGenerator;

            fieldOfWhat.Fetch(cc, references, false);
            ilg.LoadField(fieldInfo);
            if (tail) ilg.Tail();
            ilg.Call(fieldInfo.FieldType.GetProperty("Value").GetGetMethod());
            if (tail) ilg.Return();
        }

        public void Store(CompileContext2 cc, Dictionary<ItemKey, ExprObjModel.SaBox<object>> references, Action writeExprToStore, bool tail)
        {
            if (isBoxed && fieldInfo.FieldType != TypeReference.MakeBoxedType(varType).Resolve(references)) throw new PascalesqueException("Field type isn't boxed type");

            ILGenerator ilg = cc.ILGenerator;

            fieldOfWhat.Fetch(cc, references, false);
            if (isBoxed)
            {
                ilg.LoadField(fieldInfo);
                writeExprToStore();
                if (tail) ilg.Tail();
                ilg.Call(fieldInfo.FieldType.GetProperty("Value").GetSetMethod());
            }
            else
            {
                writeExprToStore();
                ilg.StoreField(fieldInfo);
            }
            if (tail) ilg.Return();
        }

        public void FetchBox(CompileContext2 cc, Dictionary<ItemKey, ExprObjModel.SaBox<object>> references, bool tail)
        {
            if (isBoxed && fieldInfo.FieldType != TypeReference.MakeBoxedType(varType).Resolve(references)) throw new PascalesqueException("Field type isn't boxed type");

            ILGenerator ilg = cc.ILGenerator;

            if (IsBoxed)
            {
                fieldOfWhat.Fetch(cc, references, false);
                if (tail) ilg.Tail();
                ilg.LoadField(fieldInfo);
                if (tail) ilg.Return();
            }
            else
            {
                throw new PascalesqueException("Tried to fetch the box of an unboxed field");
            }
        }

        #endregion
    }

    public class EnvDesc2
    {
        private Dictionary<Symbol, IVarDesc2> data;

        private EnvDesc2()
        {
            data = new Dictionary<Symbol, IVarDesc2>();
        }

        private EnvDesc2(Symbol s, IVarDesc2 v)
        {
            data = new Dictionary<Symbol, IVarDesc2>();
            data.Add(s, v);
        }

        private EnvDesc2(EnvDesc2 src)
        {
            data = new Dictionary<Symbol, IVarDesc2>();
            foreach (KeyValuePair<Symbol, IVarDesc2> kvp in src.data)
            {
                data.Add(kvp.Key, kvp.Value);
            }
        }

        public static EnvDesc2 Empty()
        {
            return new EnvDesc2();
        }

        public static EnvDesc2 Singleton(Symbol s, IVarDesc2 v)
        {
            return new EnvDesc2(s, v);
        }

        public static EnvDesc2 FromSequence(IEnumerable<Tuple<Symbol, IVarDesc2>> seq)
        {
            EnvDesc2 e = new EnvDesc2();
            foreach (Tuple<Symbol, IVarDesc2> t in seq)
            {
                e.data.Add(t.Item1, t.Item2);
            }
            return e;
        }

        public static EnvDesc2 Shadow(EnvDesc2 e, Symbol s, IVarDesc2 v)
        {
            EnvDesc2 r = new EnvDesc2(e);
            if (r.data.ContainsKey(s)) r.data.Remove(s);
            r.data.Add(s, v);
            return r;
        }

        public static EnvDesc2 Shadow(EnvDesc2 a, IEnumerable<Tuple<Symbol, IVarDesc2>> symbols)
        {
            EnvDesc2 r = new EnvDesc2(a);
            foreach (Tuple<Symbol, IVarDesc2> t in symbols)
            {
                if (r.data.ContainsKey(t.Item1)) r.data.Remove(t.Item1);
                r.data.Add(t.Item1, t.Item2);
            }
            return r;
        }

        public EnvDescTypesOnly2 TypesOnly()
        {
            return EnvDescTypesOnly2.FromSequence(data.Select(x => new ParamInfo2(x.Key, x.Value.VarType)));
        }

        public bool ContainsKey(Symbol s) { return data.ContainsKey(s); }

        public HashSet<Symbol> Keys
        {
            get
            {
                return ExprObjModel.Utils.ToHashSet(data.Keys);
            }
        }

        public IVarDesc2 this[Symbol s]
        {
            get
            {
                return data[s];
            }
        }
    }

    public class CompileContext2
    {
        private ModuleBuilder mb;
        private TypeBuilder tyb;
        private ILGenerator ilg;
        private bool isConstructor;

        public CompileContext2(ModuleBuilder mb, TypeBuilder tyb, ILGenerator ilg, bool isConstructor)
        {
            this.mb = mb;
            this.tyb = tyb;
            this.ilg = ilg;
            this.isConstructor = isConstructor;
        }

        //public ModuleBuilder ModuleBuilder { get { return mb; } }

        //public TypeBuilder TypeBuilder { get { return tyb; } }

        public ILGenerator ILGenerator { get { return ilg; } }

        public bool IsConstructor { get { return isConstructor; } }
    }

    public interface IExpression2
    {
        EnvSpec GetEnvSpec();
        TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc);
        void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add);
        HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc);
        void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, ExprObjModel.SaBox<object>> references, bool tail);
    }

    public class LiteralExpr2 : IExpression2
    {
        private object val;

        public LiteralExpr2(object val)
        {
            this.val = val;
        }

        #region IExpression2 Members

        public EnvSpec GetEnvSpec()
        {
            return EnvSpec.Empty();
        }

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            return new ExistingTypeReference(val.GetType());
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            // empty
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            return HashSet2<ItemKey>.Empty;
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, ExprObjModel.SaBox<object>> references, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            object val2 = val;

            if (val.GetType().IsEnum)
            {
                Type ut = Enum.GetUnderlyingType(val.GetType());
                if (ut == typeof(byte))
                {
                    val2 = Convert.ToByte(val);
                }
                else if (ut == typeof(sbyte))
                {
                    val2 = Convert.ToSByte(val);
                }
                else if (ut == typeof(ushort))
                {
                    val2 = Convert.ToUInt16(val);
                }
                else if (ut == typeof(short))
                {
                    val2 = Convert.ToInt16(val);
                }
                else if (ut == typeof(uint))
                {
                    val2 = Convert.ToUInt32(val);
                }
                else if (ut == typeof(int))
                {
                    val2 = Convert.ToInt32(val);
                }
                else if (ut == typeof(ulong))
                {
                    val2 = Convert.ToUInt64(val);
                }
                else if (ut == typeof(long))
                {
                    val2 = Convert.ToInt64(val);
                }
                else if (ut == typeof(char))
                {
                    val2 = Convert.ToChar(val);
                }
                else if (ut == typeof(bool))
                {
                    val2 = Convert.ToBoolean(val);
                }
                else
                {
                    throw new PascalesqueException("Enum with unsupported underlying type");
                }
            }

            if (val2.GetType() == typeof(byte))
            {
                ilg.LoadInt((int)(byte)val);
            }
            else if (val2.GetType() == typeof(sbyte))
            {
                ilg.LoadInt((int)(sbyte)val);
            }
            else if (val2.GetType() == typeof(short))
            {
                ilg.LoadInt((int)(short)val);
            }
            else if (val2.GetType() == typeof(ushort))
            {
                ilg.LoadInt((int)(ushort)val);
            }
            else if (val2.GetType() == typeof(int))
            {
                ilg.LoadInt((int)val);
            }
            else if (val2.GetType() == typeof(uint))
            {
                ilg.LoadInt((int)(uint)val);
            }
            else if (val2.GetType() == typeof(IntPtr))
            {
                ilg.LoadLong((long)(IntPtr)val);
                ilg.Conv_I();
            }
            else if (val2.GetType() == typeof(UIntPtr))
            {
                ilg.LoadLong((long)(ulong)(UIntPtr)val);
                ilg.Conv_U();
            }
            else if (val2.GetType() == typeof(long))
            {
                ilg.LoadLong((long)val);
            }
            else if (val2.GetType() == typeof(ulong))
            {
                ilg.LoadLong((long)(ulong)val);
            }
            else if (val2.GetType() == typeof(bool))
            {
                ilg.LoadInt(((bool)val) ? 1 : 0);
            }
            else if (val2.GetType() == typeof(float))
            {
                ilg.LoadFloat((float)val);
            }
            else if (val2.GetType() == typeof(double))
            {
                ilg.LoadDouble((double)val);
            }
            else if (val2.GetType() == typeof(char))
            {
                ilg.LoadInt((int)(char)val);
            }
            else if (val2.GetType() == typeof(string))
            {
                ilg.LoadString((string)val);
            }
            else
            {
                throw new PascalesqueException("Literal of unsupported type");
            }

            if (tail) ilg.Return();
        }

        #endregion
    }

    public class VarRefExpr2 : IExpression2
    {
        private Symbol name;

        public VarRefExpr2(Symbol name)
        {
            this.name = name;
        }

        #region IExpression2 Members

        public EnvSpec GetEnvSpec()
        {
            return EnvSpec.Singleton(name, new VarSpec(false, false));
        }

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            return envDesc[name];
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            // empty
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            return GetReturnType(s, envDesc).GetReferences();
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, ExprObjModel.SaBox<object>> references, bool tail)
        {
            envDesc[name].Fetch(cc, references, tail);
        }

        #endregion
    }

    public class VarSetExpr2 : IExpression2
    {
        private Symbol name;
        private IExpression2 val;

        public VarSetExpr2(Symbol name, IExpression2 val)
        {
            this.name = name;
            this.val = val;
        }

        #region IExpression2 Members

        public EnvSpec GetEnvSpec()
        {
            EnvSpec es = val.GetEnvSpec();
            return EnvSpec.Add(es, name, new VarSpec(true, false));
        }

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            if (envDesc[name] != val.GetReturnType(s, envDesc)) throw new PascalesqueException("Type mismatch in VarSet");
            return ExistingTypeReference.Void;
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            val.AddCompileSteps(s, owner, envDesc, add);
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            return GetReturnType(s, envDesc).GetReferences();
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, ExprObjModel.SaBox<object>> references, bool tail)
        {
            if (envDesc[name].VarType != val.GetReturnType(s, envDesc.TypesOnly())) throw new PascalesqueException("Type mismatch in VarSet");

            envDesc[name].Store(cc, references, delegate() { val.Compile(s, owner, cc, envDesc, references, false); }, tail);
        }

        #endregion
    }

    public class BeginExpr2 : IExpression2
    {
        private List<IExpression2> body;

        public BeginExpr2(IEnumerable<IExpression2> body)
        {
            this.body = body.ToList();
        }

        #region IExpression2 Members

        public EnvSpec GetEnvSpec()
        {
            EnvSpec e = EnvSpec.Empty();

            foreach (IExpression2 expr in body)
            {
                e |= expr.GetEnvSpec();
            }

            return e;
        }

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            return body[body.Count - 1].GetReturnType(s, envDesc);
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            foreach (IExpression2 expr in body)
            {
                expr.AddCompileSteps(s, owner, envDesc, add);
            }
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            return body.Select(expr => expr.GetReferences(s, owner, envDesc)).HashSet2Union();
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, ExprObjModel.SaBox<object>> references, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            int iEnd = body.Count;
            EnvDescTypesOnly2 edto = envDesc.TypesOnly();

            for (int i = 0; i < iEnd; ++i)
            {
                bool isLast = (i + 1 == iEnd);

                body[i].Compile(s, owner, cc, envDesc, references, tail && isLast);
                if (!isLast && body[i].GetReturnType(s, edto) != ExistingTypeReference.Void)
                {
                    ilg.Pop();
                }
            }

            if (tail) ilg.Return();
        }

        #endregion

        public static IExpression2 FromList(IEnumerable<IExpression2> exprs)
        {
            List<IExpression2> l = exprs.ToList();
            if (l.Count == 0)
            {
                return new EmptyExpr2();
            }
            else if (l.Count == 1)
            {
                return l[0];
            }
            else
            {
                return new BeginExpr2(l);
            }
        }
    }

    public class EmptyExpr2 : IExpression2
    {
        public EmptyExpr2() { }

        #region IExpression2 Members

        public EnvSpec GetEnvSpec()
        {
            return EnvSpec.Empty();
        }

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            return ExistingTypeReference.Void;
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            // empty
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            return HashSet2<ItemKey>.Empty;
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, ExprObjModel.SaBox<object>> references, bool tail)
        {
            if (tail) cc.ILGenerator.Return();
        }

        #endregion
    }

    public class AndExpr2 : IExpression2
    {
        private List<IExpression2> body;

        public AndExpr2(IEnumerable<IExpression2> body)
        {
            this.body = body.ToList();
        }

        #region IExpression2 Members

        public EnvSpec GetEnvSpec()
        {
            EnvSpec e = EnvSpec.Empty();

            foreach (IExpression2 expr in body)
            {
                e |= expr.GetEnvSpec();
            }

            return e;
        }

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            foreach (IExpression2 e in body)
            {
                if (e.GetReturnType(s, envDesc) != ExistingTypeReference.Boolean) throw new PascalesqueException("Elements in an \"and\" must be boolean");
            }
            return ExistingTypeReference.Boolean;
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            foreach (IExpression2 expr in body)
            {
                expr.AddCompileSteps(s, owner, envDesc, add);
            }
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            return body.Select(expr => expr.GetReferences(s, owner, envDesc)).HashSet2Union();
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, ExprObjModel.SaBox<object>> references, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            int iEnd = body.Count;
            EnvDescTypesOnly2 edto = envDesc.TypesOnly();

            if (iEnd == 0)
            {
                ilg.LoadInt(1);
            }
            else
            {
                Label lEnd = ilg.DefineLabel();

                for (int i = 0; i < iEnd; ++i)
                {
                    bool isLast = (i + 1 == iEnd);

                    body[i].Compile(s, owner, cc, envDesc, references, tail && isLast);
                    if (!isLast)
                    {
                        ilg.Dup();
                        ilg.Emit(OpCodes.Brfalse, lEnd);
                    }
                }
                ilg.MarkLabel(lEnd);
            }
            if (tail) ilg.Return();
        }

        #endregion
    }

    public class OrExpr2 : IExpression2
    {
        private List<IExpression2> body;

        public OrExpr2(IEnumerable<IExpression2> body)
        {
            this.body = body.ToList();
        }

        #region IExpression2 Members

        public EnvSpec GetEnvSpec()
        {
            EnvSpec e = EnvSpec.Empty();

            foreach (IExpression2 expr in body)
            {
                e |= expr.GetEnvSpec();
            }

            return e;
        }

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            foreach (IExpression2 e in body)
            {
                if (e.GetReturnType(s, envDesc) != ExistingTypeReference.Boolean) throw new PascalesqueException("Elements in an \"and\" must be boolean");
            }
            return ExistingTypeReference.Boolean;
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            foreach (IExpression2 expr in body)
            {
                expr.AddCompileSteps(s, owner, envDesc, add);
            }
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            return body.Select(expr => expr.GetReferences(s, owner, envDesc)).HashSet2Union();
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, ExprObjModel.SaBox<object>> references, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            int iEnd = body.Count;
            EnvDescTypesOnly2 edto = envDesc.TypesOnly();

            if (iEnd == 0)
            {
                ilg.LoadInt(1);
            }
            else
            {
                Label lEnd = ilg.DefineLabel();

                for (int i = 0; i < iEnd; ++i)
                {
                    bool isLast = (i + 1 == iEnd);

                    body[i].Compile(s, owner, cc, envDesc, references, tail && isLast);
                    if (!isLast)
                    {
                        ilg.Dup();
                        ilg.Emit(OpCodes.Brtrue, lEnd);
                    }
                }
                ilg.MarkLabel(lEnd);
            }
            if (tail) ilg.Return();
        }

        #endregion
    }

    public class IfThenElseExpr2 : IExpression2
    {
        private IExpression2 condition;
        private IExpression2 consequent;
        private IExpression2 alternate;

        public IfThenElseExpr2(IExpression2 condition, IExpression2 consequent, IExpression2 alternate)
        {
            this.condition = condition;
            this.consequent = consequent;
            this.alternate = alternate;
        }

        #region IExpression2 Members

        public EnvSpec GetEnvSpec()
        {
            EnvSpec e = condition.GetEnvSpec() | consequent.GetEnvSpec() | alternate.GetEnvSpec();
            return e;
        }

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            if (condition.GetReturnType(s, envDesc) != ExistingTypeReference.Boolean) throw new PascalesqueException("type of condition must be bool");
            if (consequent.GetReturnType(s, envDesc) != alternate.GetReturnType(s, envDesc)) throw new PascalesqueException("type of consequent and alternate must match");

            return consequent.GetReturnType(s, envDesc);
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            condition.AddCompileSteps(s, owner, envDesc, add);
            consequent.AddCompileSteps(s, owner, envDesc, add);
            alternate.AddCompileSteps(s, owner, envDesc, add);
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            return condition.GetReferences(s, owner, envDesc) | consequent.GetReferences(s, owner, envDesc) | alternate.GetReferences(s, owner, envDesc);
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, ExprObjModel.SaBox<object>> references, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            Label one = ilg.DefineLabel();
            Label two = ilg.DefineLabel();

            condition.Compile(s, owner, cc, envDesc, references, false);
            ilg.Emit(OpCodes.Brfalse, one);
            consequent.Compile(s, owner, cc, envDesc, references, tail);
            if (!tail) ilg.Emit(OpCodes.Br, two);
            ilg.MarkLabel(one);
            alternate.Compile(s, owner, cc, envDesc, references, tail);
            ilg.MarkLabel(two);
        }

        #endregion
    }

    public class SwitchExpr2 : IExpression2
    {
        private IExpression2 switchOnWhat;
        private IExpression2 defaultExpr;
        private List<Tuple<HashSet<uint>, IExpression2>> targetExprs;

        public SwitchExpr2(IExpression2 switchOnWhat, IExpression2 defaultExpr, IEnumerable<Tuple<IEnumerable<uint>, IExpression2>> targetExprs)
        {
            this.switchOnWhat = switchOnWhat;
            this.defaultExpr = defaultExpr;
            this.targetExprs = targetExprs.Select(x => new Tuple<HashSet<uint>, IExpression2>(x.Item1.ToHashSet(), x.Item2)).ToList();
        }

        public EnvSpec GetEnvSpec()
        {
            EnvSpec e = switchOnWhat.GetEnvSpec() | defaultExpr.GetEnvSpec();
            foreach (Tuple<HashSet<uint>, IExpression2> kvp in targetExprs)
            {
                e |= kvp.Item2.GetEnvSpec();
            }
            return e;
        }

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            if (switchOnWhat.GetReturnType(s, envDesc) != ExistingTypeReference.UInt32) throw new PascalesqueException("SwitchExpr: SwitchOnWhat must be of type uint");

            uint max = targetExprs.Select(x => x.Item1.Max()).Max();
            if (max > 255u) throw new PascalesqueException("Switch has more than 256 destinations");

            bool[] b1 = new bool[max + 1];

            TypeReference t = defaultExpr.GetReturnType(s, envDesc);
            foreach (Tuple<HashSet<uint>, IExpression2> kvp in targetExprs)
            {
                foreach (uint u in kvp.Item1)
                {
                    if (b1[(int)u]) throw new PascalesqueException("Switch error: A value can go to only one expression");
                    b1[(int)u] = true;
                }

                TypeReference t2 = kvp.Item2.GetReturnType(s, envDesc);
                if (t != t2) throw new PascalesqueException("SwitchExpr: All alternatives must be of the same type");
            }

            return t;
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            switchOnWhat.AddCompileSteps(s, owner, envDesc, add);
            defaultExpr.AddCompileSteps(s, owner, envDesc, add);
            foreach (Tuple<HashSet<uint>, IExpression2> expr in targetExprs)
            {
                expr.Item2.AddCompileSteps(s, owner, envDesc, add);
            }
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            return switchOnWhat.GetReferences(s, owner, envDesc) | defaultExpr.GetReferences(s, owner, envDesc) | targetExprs.Select(x => x.Item2.GetReferences(s, owner, envDesc)).HashSet2Union();
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, ExprObjModel.SaBox<object>> references, bool tail)
        {
            List<Label> labelList = new List<Label>();
            foreach (Tuple<HashSet<uint>, IExpression2> item in targetExprs)
            {
                Label l0 = cc.ILGenerator.DefineLabel();
                labelList.Add(l0);
            }

            Label lDefault = cc.ILGenerator.DefineLabel();

            uint max = targetExprs.Select(x => x.Item1.Max()).Max();
            if (max > 255u) throw new PascalesqueException("Switch has more than 256 destinations");

            bool[] assigned = new bool[max + 1];
            Label[] larr = new Label[max + 1];
            int iEnd = targetExprs.Count;
            for (int i = 0; i < iEnd; ++i)
            {
                Tuple<HashSet<uint>, IExpression2> item = targetExprs[i];
                foreach (uint u in item.Item1)
                {
                    if (assigned[u]) throw new PascalesqueException("A value can go to only one expression");
                    larr[u] = labelList[i];
                    assigned[u] = true;
                }
            }
            for (uint j = 0; j < max; ++j)
            {
                if (!assigned[j]) larr[j] = lDefault;
            }

            switchOnWhat.Compile(s, owner, cc, envDesc, references, false);

            Label? lEnd = tail ? (Label?)null : (Label?)(cc.ILGenerator.DefineLabel());

            cc.ILGenerator.Emit(OpCodes.Switch, larr);

            cc.ILGenerator.MarkLabel(lDefault);
            defaultExpr.Compile(s, owner, cc, envDesc, references, tail);

            for (int i = 0; i < iEnd; ++i)
            {
                if (!tail) cc.ILGenerator.Emit(OpCodes.Br, lEnd.Value);
                cc.ILGenerator.MarkLabel(labelList[i]);
                targetExprs[i].Item2.Compile(s, owner, cc, envDesc, references, tail);
            }

            if (!tail) cc.ILGenerator.MarkLabel(lEnd.Value);
        }
    }

    public class BeginWhileRepeatExpr2 : IExpression2
    {
        private IExpression2 body1;
        private IExpression2 condition;
        private IExpression2 body2;

        public BeginWhileRepeatExpr2(IExpression2 body1, IExpression2 condition, IExpression2 body2)
        {
            this.body1 = body1;
            this.condition = condition;
            this.body2 = body2;
        }

        #region IExpression2 Members

        public EnvSpec GetEnvSpec()
        {
            EnvSpec e = body1.GetEnvSpec() | condition.GetEnvSpec() | body2.GetEnvSpec();
            return e;
        }

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            if (condition.GetReturnType(s, envDesc) != ExistingTypeReference.Boolean) throw new PascalesqueException("type of condition must be bool");

            return ExistingTypeReference.Void;
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            body1.AddCompileSteps(s, owner, envDesc, add);
            condition.AddCompileSteps(s, owner, envDesc, add);
            body2.AddCompileSteps(s, owner, envDesc, add);
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            return body1.GetReferences(s, owner, envDesc) | condition.GetReferences(s, owner, envDesc) | body2.GetReferences(s, owner, envDesc);
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, ExprObjModel.SaBox<object>> references, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            Label one = ilg.DefineLabel();
            Label two = ilg.DefineLabel();

            ilg.MarkLabel(one);
            body1.Compile(s, owner, cc, envDesc, references, false);
            if (body1.GetReturnType(s, envDesc.TypesOnly()) != ExistingTypeReference.Void) ilg.Pop();
            condition.Compile(s, owner, cc, envDesc, references, false);
            if (condition.GetReturnType(s, envDesc.TypesOnly()) != ExistingTypeReference.Boolean) throw new PascalesqueException("type of condition must be bool");
            ilg.Emit(OpCodes.Brfalse, two);
            body2.Compile(s, owner, cc, envDesc, references, false);
            if (body2.GetReturnType(s, envDesc.TypesOnly()) != ExistingTypeReference.Void) ilg.Pop();
            ilg.Emit(OpCodes.Br, one);
            ilg.MarkLabel(two);
            if (tail) ilg.Return();
        }

        #endregion
    }

    public class LetClause2
    {
        private Symbol name;
        private TypeReference varType;
        private IExpression2 val;

        public LetClause2(Symbol name, TypeReference varType, IExpression2 val)
        {
            this.name = name;
            this.varType = varType;
            this.val = val;
        }

        public Symbol Name { get { return name; } }
        public TypeReference VarType { get { return varType; } }
        public IExpression2 Value { get { return val; } }
    }

    public class LetExpr2 : IExpression2
    {
        private List<LetClause2> clauses;
        private IExpression2 body;

        public LetExpr2(IEnumerable<LetClause2> clauses, IExpression2 body)
        {
            this.clauses = clauses.ToList();
            this.body = body;

            if (clauses.Select(x => x.Name).HasDuplicates()) throw new PascalesqueException("Duplicate variables in letrec");
        }

        #region IExpression2 Members

        public EnvSpec GetEnvSpec()
        {
            EnvSpec e = body.GetEnvSpec() - clauses.Select(x => x.Name);
            foreach (LetClause2 lc in clauses)
            {
                e |= lc.Value.GetEnvSpec();
            }
            return e;
        }

        private EnvDescTypesOnly2 MakeInnerEnvDesc(EnvDescTypesOnly2 outerEnvDesc)
        {
            return EnvDescTypesOnly2.Shadow(outerEnvDesc, clauses.Select(x => new ParamInfo2(x.Name, x.VarType)));
        }

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            if (clauses.Select(x => x.Name).HasDuplicates()) throw new PascalesqueException("let has two variables with the same name");
            if (clauses.Any(x => x.VarType != x.Value.GetReturnType(s, envDesc))) throw new PascalesqueException("a variable's type does not match that of its initializer");

            EnvDescTypesOnly2 e2 = MakeInnerEnvDesc(envDesc);

            return body.GetReturnType(s, e2);
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            foreach (LetClause2 l in clauses)
            {
                l.Value.AddCompileSteps(s, owner, envDesc, add);
            }

            EnvDescTypesOnly2 e2 = MakeInnerEnvDesc(envDesc);
            body.AddCompileSteps(s, owner, e2, add);
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            EnvDescTypesOnly2 e2 = MakeInnerEnvDesc(envDesc);
            return clauses.Select(x => x.Value.GetReferences(s, owner, envDesc)).HashSet2Union() | body.GetReferences(s, owner, e2);
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, ExprObjModel.SaBox<object>> references, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            EnvSpec e = body.GetEnvSpec();

            List<Tuple<Symbol, IVarDesc2>> theList = new List<Tuple<Symbol, IVarDesc2>>();

            foreach (LetClause2 l in clauses)
            {
                bool boxed = false;
                if (e.ContainsKey(l.Name))
                {
                    boxed = e[l.Name].IsCaptured;
                }

                LocalBuilder lb = ilg.DeclareLocal((boxed ? TypeReference.MakeBoxedType(l.VarType) : l.VarType).Resolve(references));
                IVarDesc2 varDesc = new LocalVarDesc2(l.VarType, boxed, lb.LocalIndex);

                theList.Add(new Tuple<Symbol, IVarDesc2>(l.Name, varDesc));
                if (boxed)
                {
                    ilg.NewObj(TypeReference.MakeBoxedType(l.VarType).Resolve(references).GetConstructor(Type.EmptyTypes));
                    ilg.StoreLocal(lb);
                }
                varDesc.Store(cc, references, delegate() { l.Value.Compile(s, owner, cc, envDesc, references, false); }, false);
            }

            EnvDesc2 innerEnvDesc = EnvDesc2.Shadow(envDesc, theList);

            body.Compile(s, owner, cc, innerEnvDesc, references, tail);
        }

        #endregion
    }

    public class LetStarExpr2 : IExpression2
    {
        private List<LetClause2> clauses;
        private IExpression2 body;

        public LetStarExpr2(IEnumerable<LetClause2> clauses, IExpression2 body)
        {
            this.clauses = clauses.ToList();
            this.body = body;
        }

        #region IExpression2 Members

        private EnvSpec GetEnvSpec(int j)
        {
            EnvSpec e = body.GetEnvSpec();
            int i = clauses.Count;
            while (i > j)
            {
                --i;
                LetClause2 lc = clauses[i];
                e -= lc.Name;
                e |= lc.Value.GetEnvSpec();
            }
            return e;
        }

        public EnvSpec GetEnvSpec()
        {
            return GetEnvSpec(0);
        }

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            EnvDescTypesOnly2 e2 = envDesc;

            foreach (LetClause2 lc in clauses)
            {
                if (lc.Value.GetReturnType(s, e2) != lc.VarType) throw new PascalesqueException("a variable's type does not match that of its initializer");
                e2 = EnvDescTypesOnly2.Shadow(e2, lc.Name, lc.VarType);
            }

            return body.GetReturnType(s, e2);
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            EnvDescTypesOnly2 e2 = envDesc;
            int iEnd = clauses.Count;
            for (int i = 0; i < iEnd; ++i)
            {
                clauses[i].Value.AddCompileSteps(s, owner, e2, add);
                e2 = EnvDescTypesOnly2.Shadow(e2, clauses[i].Name, clauses[i].VarType);
            }
            body.AddCompileSteps(s, owner, e2, add);
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            HashSet2<ItemKey> h = HashSet2<ItemKey>.Empty;
            EnvDescTypesOnly2 e2 = envDesc;
            int iEnd = clauses.Count;
            for (int i = 0; i < iEnd; ++i)
            {
                h |= clauses[i].Value.GetReferences(s, owner, e2);
                e2 = EnvDescTypesOnly2.Shadow(e2, clauses[i].Name, clauses[i].VarType);
            }
            h |= body.GetReferences(s, owner, e2);
            return h;
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, ExprObjModel.SaBox<object>> references, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            EnvDesc2 e2 = envDesc;
            int iEnd = clauses.Count;
            for (int i = 0; i < iEnd; ++i)
            {
                LetClause2 l = clauses[i];
                EnvSpec e = GetEnvSpec(i + 1);
                bool boxed = false;
                if (e.ContainsKey(l.Name))
                {
                    boxed = e[l.Name].IsCaptured;
                }

                LocalBuilder lb = ilg.DeclareLocal((boxed ? TypeReference.MakeBoxedType(l.VarType) : l.VarType).Resolve(references));
                IVarDesc2 varDesc = new LocalVarDesc2(l.VarType, boxed, lb.LocalIndex);

                if (boxed)
                {
                    ilg.NewObj(TypeReference.MakeBoxedType(l.VarType).Resolve(references).GetConstructor(Type.EmptyTypes));
                    ilg.StoreLocal(lb);
                }
                varDesc.Store(cc, references, delegate() { l.Value.Compile(s, owner, cc, e2, references, false); }, false);

                e2 = EnvDesc2.Shadow(e2, l.Name, varDesc);
            }

            body.Compile(s, owner, cc, e2, references, tail);
        }

        #endregion
    }

    public class LetRecExpr2 : IExpression2
    {
        private List<LetClause2> clauses;
        private IExpression2 body;

        public LetRecExpr2(IEnumerable<LetClause2> clauses, IExpression2 body)
        {
            this.clauses = clauses.ToList();
            this.body = body;

            if (clauses.Select(x => x.Name).HasDuplicates()) throw new PascalesqueException("Duplicate variables in letrec");
        }

        #region IExpression2 Members

        public EnvSpec GetEnvSpec()
        {
            EnvSpec e = body.GetEnvSpec();
            foreach (LetClause2 lc in clauses)
            {
                e |= lc.Value.GetEnvSpec();
            }
            return e - clauses.Select(x => x.Name);
        }

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            EnvDescTypesOnly2 innerEnvDesc = EnvDescTypesOnly2.Shadow(envDesc, clauses.Select(x => new ParamInfo2(x.Name, x.VarType)));
            return body.GetReturnType(s, innerEnvDesc);
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            List<ParamInfo2> theList = new List<ParamInfo2>();
            foreach (LetClause2 l in clauses)
            {
                theList.Add(new ParamInfo2(l.Name, l.VarType));
            }
            EnvDescTypesOnly2 innerEnvDesc = EnvDescTypesOnly2.Shadow(envDesc, theList);
            foreach (LetClause2 l in clauses)
            {
                l.Value.AddCompileSteps(s, owner, innerEnvDesc, add);

            }
            body.AddCompileSteps(s, owner, innerEnvDesc, add);
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            HashSet2<ItemKey> h = HashSet2<ItemKey>.Empty;
            List<ParamInfo2> theList = new List<ParamInfo2>();
            foreach (LetClause2 l in clauses)
            {
                theList.Add(new ParamInfo2(l.Name, l.VarType));
            }
            EnvDescTypesOnly2 innerEnvDesc = EnvDescTypesOnly2.Shadow(envDesc, theList);
            foreach (LetClause2 l in clauses)
            {
                h |= l.Value.GetReferences(s, owner, innerEnvDesc);
            }
            h |= body.GetReferences(s, owner, innerEnvDesc);
            return h;
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, ExprObjModel.SaBox<object>> references, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            List<Tuple<Symbol, IVarDesc2>> theList = new List<Tuple<Symbol, IVarDesc2>>();

            foreach (LetClause2 l in clauses)
            {
                LocalBuilder lb = ilg.DeclareLocal(TypeReference.MakeBoxedType(l.VarType).Resolve(references));
                IVarDesc2 varDesc = new LocalVarDesc2(l.VarType, true, lb.LocalIndex);

                theList.Add(new Tuple<Symbol, IVarDesc2>(l.Name, varDesc));
                ilg.NewObj(TypeReference.MakeBoxedType(l.VarType).Resolve(references).GetConstructor(Type.EmptyTypes));
                ilg.StoreLocal(lb);
            }

            EnvDesc2 innerEnvDesc = EnvDesc2.Shadow(envDesc, theList);

            int iEnd = theList.Count;
            for (int i = 0; i < iEnd; ++i)
            {
                IVarDesc2 varDesc = theList[i].Item2;
                varDesc.Store(cc, references, delegate() { clauses[i].Value.Compile(s, owner, cc, innerEnvDesc, references, false); }, false);
            }

            body.Compile(s, owner, cc, innerEnvDesc, references, tail);
        }

        #endregion
    }

    public class LetLoopExpr2 : IExpression2
    {
        private Symbol loopName;
        private TypeReference loopReturnType;
        private List<LetClause2> clauses;
        private IExpression2 body;

        private Lazy<IExpression2> equivalency;

        public LetLoopExpr2(Symbol loopName, TypeReference loopReturnType, IEnumerable<LetClause2> clauses, IExpression2 body)
        {
            this.loopName = loopName;
            this.loopReturnType = loopReturnType;
            this.clauses = clauses.ToList();
            this.body = body;

            if (clauses.Select(x => x.Name).HasDuplicates()) throw new PascalesqueException("Duplicate variables in letrec");

            equivalency = new Lazy<IExpression2>(new Func<IExpression2>(MakeEquivalency), false);
        }

        private IExpression2 MakeEquivalency()
        {
            TypeReference funcType = GetFuncType();

            return new LetRecExpr2
            (
                new LetClause2[]
                {
                    new LetClause2
                    (
                        loopName, funcType,
                        new LambdaExpr2
                        (
                            clauses.Select(x => new ParamInfo2(x.Name, x.VarType)),
                            body
                        )
                    )
                },
                new InvokeExpr2
                (
                    new VarRefExpr2(loopName),
                    clauses.Select(x => x.Value)
                )
            );
        }

        #region IExpression2 Members

        public EnvSpec GetEnvSpec()
        {
            EnvSpec e = body.GetEnvSpec();
            foreach (LetClause2 lc in clauses)
            {
                e |= lc.Value.GetEnvSpec();
            }
            return e - clauses.Select(x => x.Name);
        }

        private TypeReference GetFuncType()
        {
            if (loopReturnType == ExistingTypeReference.Void)
            {
                return TypeReference.GetActionType(clauses.Select(x => x.VarType).ToArray());
            }
            else
            {
                return TypeReference.GetFuncType(clauses.Select(x => x.VarType).AndAlso(loopReturnType).ToArray());
            }
        }

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            TypeReference funcType = GetFuncType();

            EnvDescTypesOnly2 innerEnvDesc = EnvDescTypesOnly2.Shadow(envDesc, clauses.Select(x => new ParamInfo2(x.Name, x.VarType)).AndAlso(new ParamInfo2(loopName, funcType)));
            TypeReference t = body.GetReturnType(s, innerEnvDesc);
            if (t != loopReturnType) throw new PascalesqueException("let loop: loop does not return expected type");

            return t;
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            equivalency.Value.AddCompileSteps(s, owner, envDesc, add);
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            return equivalency.Value.GetReferences(s, owner, envDesc);
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, ExprObjModel.SaBox<object>> references, bool tail)
        {
            equivalency.Value.Compile(s, owner, cc, envDesc, references, tail);
        }

        #endregion
    }

    public class LambdaExpr2 : IExpression2
    {
        private List<ParamInfo2> parameters;
        private IExpression2 body;
        private Symbol lambdaObjTypeName;

        public LambdaExpr2(IEnumerable<ParamInfo2> parameters, IExpression2 body)
        {
            this.parameters = parameters.ToList();
            this.body = body;
            this.lambdaObjTypeName = new Symbol();
        }

        #region IExpression2 Members

        public EnvSpec GetEnvSpec()
        {
            EnvSpec e = body.GetEnvSpec() - parameters.Select(x => x.Name);
            return EnvSpec.CaptureAll(e);
        }

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            EnvDescTypesOnly2 innerEnvDesc = EnvDescTypesOnly2.Shadow(envDesc, parameters.Select(x => new ParamInfo2(x.Name, x.ParamType)));
            TypeReference returnType = body.GetReturnType(s, innerEnvDesc);
            if (returnType == ExistingTypeReference.Void)
            {
                return TypeReference.GetActionType(parameters.Select(x => x.ParamType).ToArray());
            }
            else
            {
                List<TypeReference> t = parameters.Select(x => x.ParamType).ToList();
                t.Add(returnType);

                return TypeReference.GetFuncType(t.ToArray());
            }
        }

        public IEnumerable<ParamInfo2> ParamInfos { get { return parameters.AsEnumerable(); } }

        public IExpression2 Body { get { return body; } }

        private class CompileStepInfo
        {
            private LambdaExpr2 parent;
            private SymbolTable symbolTable;
            private EnvDescTypesOnly2 envDesc;

            private Lazy<TypeKey> classKey;
            private Lazy<ConstructorKey> constructorKey;
            private Lazy<MethodKey> methodKey;
            private Lazy<FieldKey[]> fieldKeys;
            private Lazy<TypeReference> methodReturnType;
            private Lazy<EnvDescTypesOnly2> innerEnvDesc;
            private Lazy<CompletedTypeKey> completedClassKey;

            public CompileStepInfo(LambdaExpr2 parent, SymbolTable symbolTable, EnvDescTypesOnly2 envDesc)
            {
                this.parent = parent;
                this.symbolTable = symbolTable;
                this.envDesc = envDesc;

                this.classKey = new Lazy<TypeKey>(new Func<TypeKey>(MakeClassKey), false);
                this.constructorKey = new Lazy<ConstructorKey>(new Func<ConstructorKey>(MakeConstructorKey), false);
                this.methodKey = new Lazy<MethodKey>(new Func<MethodKey>(MakeMethodKey), false);
                this.fieldKeys = new Lazy<FieldKey[]>(new Func<FieldKey[]>(MakeFieldKeys), false);
                this.methodReturnType = new Lazy<TypeReference>(new Func<TypeReference>(MakeReturnType), false);
                this.innerEnvDesc = new Lazy<EnvDescTypesOnly2>(new Func<EnvDescTypesOnly2>(MakeInnerEnvDesc), false);
                this.completedClassKey = new Lazy<CompletedTypeKey>(new Func<CompletedTypeKey>(MakeCompletedClassKey), false);
            }

            public SymbolTable SymbolTable { get { return symbolTable; } }

            private TypeKey MakeClassKey()
            {
                return new TypeKey(parent.lambdaObjTypeName);
            }

            public TypeKey ClassKey { get { return classKey.Value; } }

            private CompletedTypeKey MakeCompletedClassKey()
            {
                return new CompletedTypeKey(parent.lambdaObjTypeName);
            }

            public CompletedTypeKey CompletedClassKey { get { return completedClassKey.Value; } }

            private ConstructorKey MakeConstructorKey()
            {
                EnvSpec e = parent.body.GetEnvSpec() - parent.parameters.Select(x => x.Name);
                Symbol[] capturedVars = e.Keys.ToArray();

                TypeReference[] constructorParams = capturedVars.Select(s => TypeReference.MakeBoxedType(envDesc[s])).ToArray();

                return new ConstructorKey(classKey.Value, constructorParams);
            }

            public ConstructorKey ConstructorKey { get { return constructorKey.Value; } }

            private TypeReference MakeReturnType()
            {
                return parent.GetReturnType(symbolTable, envDesc);
            }

            public TypeReference MethodReturnType { get { return methodReturnType.Value; } }

            private MethodKey MakeMethodKey()
            {
                return new MethodKey(classKey.Value, new Symbol("Invoke"), true, parent.parameters.Select(x => x.ParamType).ToArray());
            }

            public MethodKey MethodKey { get { return methodKey.Value; } }

            public Symbol ClassName { get { return parent.lambdaObjTypeName; } }

            private FieldKey[] MakeFieldKeys()
            {
                ConstructorKey c = constructorKey.Value;
                int iEnd = c.Parameters.Count;
                FieldKey[] farr = new FieldKey[iEnd];
                for (int i = 0; i < iEnd; ++i)
                {
                    farr[i] = new FieldKey(classKey.Value, new Symbol(), c.Parameters[i]);
                }
                return farr;
            }

            public IReadOnlyArray<FieldKey> FieldKeys { get { return fieldKeys.Value.AsReadOnlyArray(); } }

            private EnvDescTypesOnly2 MakeInnerEnvDesc()
            {
                return EnvDescTypesOnly2.Shadow(envDesc, parent.parameters.Select(x => new ParamInfo2(x.Name, x.ParamType)));
            }

            public EnvDescTypesOnly2 InnerEnvDesc { get { return innerEnvDesc.Value; } }

            public List<ParamInfo2> Parameters { get { return parent.parameters; } }

            public IExpression2 Body { get { return parent.body; } }
        }

        private class MakeClass : ICompileStep
        {
            private CompileStepInfo info;

            public MakeClass(CompileStepInfo info)
            {
                this.info = info;
            }

            #region ICompileStep Members

            public int Phase
            {
                get { return 1; }
            }

            public HashSet2<ItemKey> Inputs
            {
                get { return HashSet2<ItemKey>.Empty; }
            }

            public HashSet2<ItemKey> Outputs
            {
                get { return HashSet2<ItemKey>.Singleton(info.ClassKey); }
            }

            public void Compile(ModuleBuilder mb, Dictionary<ItemKey, SaBox<object>> vars)
            {
                TypeKey t = info.ClassKey;
                TypeBuilder tyb = mb.DefineType(info.ClassName.Name, TypeAttributes.Public);
                vars[t].Value = tyb;
            }

            #endregion
        }

        private class MakeConstructor : ICompileStep
        {
            private CompileStepInfo info;

            public MakeConstructor(CompileStepInfo info)
            {
                this.info = info;
            }

            #region ICompileStep Members

            public int Phase
            {
                get { return 1; }
            }

            public HashSet2<ItemKey> Inputs
            {
                get
                {
                    return info.ConstructorKey.Parameters.Select(x => x.GetReferences()).HashSet2Union() | info.ClassKey;
                }
            }

            public HashSet2<ItemKey> Outputs
            {
                get
                {
                    return HashSet2<ItemKey>.Singleton(info.ConstructorKey);
                }
            }

            public void Compile(ModuleBuilder mb, Dictionary<ItemKey, SaBox<object>> vars)
            {
                TypeBuilder t = (TypeBuilder)(vars[info.ClassKey].Value);
                ConstructorBuilder cb = t.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, info.ConstructorKey.Parameters.Select(x => x.Resolve(vars)).ToArray());
                vars[info.ConstructorKey].Value = cb;
            }

            #endregion
        }

        private class MakeField : ICompileStep
        {
            private CompileStepInfo info;
            private int fieldIndex;

            public MakeField(CompileStepInfo info, int fieldIndex)
            {
                this.info = info;
                this.fieldIndex = fieldIndex;
            }

            #region ICompileStep Members

            public int Phase { get { return 1; } }

            public HashSet2<ItemKey> Inputs
            {
                get
                {
                    return HashSet2<ItemKey>.Singleton(info.ClassKey);
                }
            }

            public HashSet2<ItemKey> Outputs
            {
                get
                {
                    return HashSet2<ItemKey>.Singleton(info.FieldKeys[fieldIndex]);
                }
            }

            public void Compile(ModuleBuilder mb, Dictionary<ItemKey, SaBox<object>> vars)
            {
                TypeBuilder tyb = (TypeBuilder)(vars[info.ClassKey].Value);
                FieldKey fk = info.FieldKeys[fieldIndex];
                FieldBuilder fb = tyb.DefineField(fk.Name.Name, fk.FieldType.Resolve(vars), FieldAttributes.Private);
                vars[fk].Value = fb;
            }

            #endregion
        }

        private class MakeInvokeMethod : ICompileStep
        {
            private CompileStepInfo info;

            public MakeInvokeMethod(CompileStepInfo info)
            {
                this.info = info;
            }

            #region ICompileStep Members

            public int Phase { get { return 1; } }

            public HashSet2<ItemKey> Inputs
            {
                get
                {
                    return HashSet2<ItemKey>.Singleton(info.ClassKey) | info.MethodReturnType.GetReferences();
                }
            }

            public HashSet2<ItemKey> Outputs
            {
                get
                {
                    return HashSet2<ItemKey>.Singleton(info.MethodKey);
                }
            }

            public void Compile(ModuleBuilder mb, Dictionary<ItemKey, SaBox<object>> vars)
            {
                TypeBuilder tyb = (TypeBuilder)(vars[info.ClassKey].Value);
                MethodKey mk = info.MethodKey;
                TypeReference returnType = info.MethodReturnType;
                MethodBuilder meb = tyb.DefineMethod(mk.Name.Name, MethodAttributes.Public, returnType.Resolve(vars), mk.Parameters.Select(x => x.Resolve(vars)).ToArray());
                vars[mk].Value = meb;
            }

            #endregion
        }

        private class MakeConstructorBody : ICompileStep
        {
            private CompileStepInfo info;

            public MakeConstructorBody(CompileStepInfo info)
            {
                this.info = info;
            }

            #region ICompileStep Members

            public int Phase { get { return 1; } }

            public HashSet2<ItemKey> Inputs
            {
                get
                {
                    return info.ConstructorKey | HashSet2<ItemKey>.FromSequence(info.FieldKeys);
                }
            }

            public HashSet2<ItemKey> Outputs
            {
                get
                {
                    return HashSet2<ItemKey>.Empty;
                }
            }

            public void Compile(ModuleBuilder mb, Dictionary<ItemKey, SaBox<object>> vars)
            {
                ConstructorBuilder cb = (ConstructorBuilder)(vars[info.ConstructorKey].Value);
                FieldBuilder[] lambdaFields = info.FieldKeys.Select(x => vars[x].Value).Cast<FieldBuilder>().ToArray();

                ILGenerator cilg = cb.GetILGenerator();
                int iEnd = lambdaFields.Length;

                cilg.LoadArg(0);
                cilg.Call(typeof(object).GetConstructor(Type.EmptyTypes));

                for (int i = 0; i < iEnd; ++i)
                {
                    cilg.LoadArg(0);
                    cilg.LoadArg(i + 1);
                    cilg.StoreField(lambdaFields[i]);
                }

                cilg.Return();
            }

            #endregion
        }

        private class MakeInvokeMethodBody : ICompileStep
        {
            private CompileStepInfo info;

            public MakeInvokeMethodBody(CompileStepInfo info)
            {
                this.info = info;
            }

            #region ICompileStep Members

            public int Phase { get { return 1; } }

            public HashSet2<ItemKey> Inputs
            {
                get
                {
                    return HashSet2<ItemKey>.Singleton(info.ClassKey) |
                        HashSet2<ItemKey>.Singleton(info.MethodKey) |
                        HashSet2<ItemKey>.FromSequence(info.FieldKeys) |
                        info.Body.GetReferences(info.SymbolTable, info.ClassKey, info.InnerEnvDesc);
                }
            }

            public HashSet2<ItemKey> Outputs
            {
                get
                {
                    return HashSet2<ItemKey>.Empty;
                }
            }

            public void Compile(ModuleBuilder mb, Dictionary<ItemKey, SaBox<object>> vars)
            {
                TypeBuilder lambdaObj = (TypeBuilder)(vars[info.ClassKey].Value);
                MethodBuilder meb = (MethodBuilder)(vars[info.MethodKey].Value);
                FieldBuilder[] lambdaFields = info.FieldKeys.Select(x => vars[x].Value).Cast<FieldBuilder>().ToArray();
                Symbol[] capturedVars = info.FieldKeys.Select(x => x.Name).ToArray();
                TypeReference lambdaObjRef = new TypeKeyReference(info.ClassKey);
                EnvDescTypesOnly2 innerEnvDesc = info.InnerEnvDesc;

                int iEnd = lambdaFields.Length;
                List<Tuple<Symbol, IVarDesc2>> innerVars = new List<Tuple<Symbol, IVarDesc2>>();

                for (int i = 0; i < iEnd; ++i)
                {
                    innerVars.Add(new Tuple<Symbol, IVarDesc2>(capturedVars[i], new FieldVarDesc2(new ArgVarDesc2(lambdaObjRef, false, 0), lambdaFields[i], innerEnvDesc[capturedVars[i]], true)));
                }
                int jEnd = info.Parameters.Count;
                for (int j = 0; j < jEnd; ++j)
                {
                    innerVars.Add(new Tuple<Symbol, IVarDesc2>(info.Parameters[j].Name, new ArgVarDesc2(info.Parameters[j].ParamType, false, j + 1)));
                }

                ILGenerator milg = meb.GetILGenerator();

                EnvDesc2 innerEnvDesc2 = EnvDesc2.FromSequence(innerVars);

                CompileContext2 cc2 = new CompileContext2(mb, lambdaObj, milg, false);

                info.Body.Compile(info.SymbolTable, info.ClassKey, cc2, innerEnvDesc2, vars, true);
            }

            #endregion
        }

        private class BakeClass : ICompileStep
        {
            private CompileStepInfo info;

            public BakeClass(CompileStepInfo info)
            {
                this.info = info;
            }

            #region ICompileStep Members

            public int Phase
            {
                get { return 2; }
            }

            public HashSet2<ItemKey> Inputs
            {
                get
                {
                    return HashSet2<ItemKey>.Singleton(info.ClassKey) | info.ConstructorKey;
                }
            }

            public HashSet2<ItemKey> Outputs
            {
                get
                {
                    return HashSet2<ItemKey>.Singleton(info.CompletedClassKey);
                }
            }

            public void Compile(ModuleBuilder mb, Dictionary<ItemKey, SaBox<object>> vars)
            {
                TypeBuilder tyb = (TypeBuilder)(vars[info.ClassKey].Value);

                Type t = tyb.CreateType();

                vars[info.CompletedClassKey].Value = t;
            }

            #endregion
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            CompileStepInfo info = new CompileStepInfo(this, s, envDesc);
            add(new MakeClass(info));
            add(new MakeConstructor(info));
            add(new MakeInvokeMethod(info));
            int iEnd = info.FieldKeys.Count;
            for (int i = 0; i < iEnd; ++i)
            {
                add(new MakeField(info, i));
            }
            add(new MakeConstructorBody(info));
            add(new MakeInvokeMethodBody(info));
            EnvDescTypesOnly2 innerEnvDesc = EnvDescTypesOnly2.Shadow(envDesc, parameters.Select(x => new ParamInfo2(x.Name, x.ParamType)));
            body.AddCompileSteps(s, new TypeKey(lambdaObjTypeName), innerEnvDesc, add);
            add(new BakeClass(info));
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            HashSet2<ItemKey> h = HashSet2<ItemKey>.Empty;
            EnvDescTypesOnly2 innerEnvDesc = EnvDescTypesOnly2.Shadow(envDesc, parameters.Select(x => new ParamInfo2(x.Name, x.ParamType)));
            h |= body.GetReferences(s, new TypeKey(lambdaObjTypeName), innerEnvDesc);
            h |= GetReturnType(s, envDesc).GetReferences();

            CompileStepInfo csi = new CompileStepInfo(this, s, envDesc);
            h |= csi.ClassKey;
            h |= csi.ConstructorKey;
            h |= csi.MethodKey;

            return h;
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, ExprObjModel.SaBox<object>> references, bool tail)
        {
            CompileStepInfo info = new CompileStepInfo(this, s, envDesc.TypesOnly());

            TypeBuilder lambdaObjType = (TypeBuilder)(references[info.ClassKey].Value);
            ConstructorBuilder constructor = (ConstructorBuilder)(references[info.ConstructorKey].Value);
            MethodBuilder invokeMethod = (MethodBuilder)(references[info.MethodKey].Value);

            Symbol[] capturedVars = info.FieldKeys.Select(x => x.Name).ToArray();
            int iEnd = capturedVars.Length;

            ILGenerator ilg = cc.ILGenerator;
            for (int i = 0; i < iEnd; ++i)
            {
                envDesc[capturedVars[i]].FetchBox(cc, references, false);
            }

            ilg.NewObj(constructor);

            ilg.LoadFunction(invokeMethod);

            TypeReference dTypeRef = GetReturnType(s, envDesc.TypesOnly());
            Type dType = dTypeRef.Resolve(references);

            ConstructorInfo[] dci = dType.GetConstructors();

            ilg.NewObj(dType.GetConstructor(new Type[] { typeof(object), typeof(IntPtr) }));
            if (tail) ilg.Return();
        }

        #endregion
    }

    public class InvokeExpr2 : IExpression2
    {
        private IExpression2 func;
        private List<IExpression2> args;

        public InvokeExpr2(IExpression2 func, IEnumerable<IExpression2> args)
        {
            this.func = func;
            this.args = args.ToList();
        }

        #region IExpression2 Members

        public EnvSpec GetEnvSpec()
        {
            EnvSpec e = func.GetEnvSpec();
            foreach (IExpression2 arg in args)
            {
                e |= arg.GetEnvSpec();
            }
            return e;
        }

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            TypeReference funcType = func.GetReturnType(s, envDesc);

            if (!(funcType.IsDelegate)) throw new PascalesqueException("Invocation of a non-delegate");

            TypeReference[] p = funcType.GetDelegateParameterTypes();
            if (p.Length != args.Count) throw new PascalesqueException("Argument count doesn't match parameter count");

            int iEnd = p.Length;
            for (int i = 0; i < iEnd; ++i)
            {
                if (p[i] != args[i].GetReturnType(s, envDesc)) throw new PascalesqueException("Argument " + i + " type doesn't match parameter type");
            }

            return funcType.GetDelegateReturnType();
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            func.AddCompileSteps(s, owner, envDesc, add);
            foreach (IExpression2 arg in args)
            {
                arg.AddCompileSteps(s, owner, envDesc, add);
            }
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            return func.GetReferences(s, owner, envDesc) | args.Select(x => x.GetReferences(s, owner, envDesc)).HashSet2Union();
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, ExprObjModel.SaBox<object>> references, bool tail)
        {
            TypeReference funcType = func.GetReturnType(s, envDesc.TypesOnly());

            func.Compile(s, owner, cc, envDesc, references, false);
            foreach (IExpression2 arg in args)
            {
                arg.Compile(s, owner, cc, envDesc, references, false);
            }
            ILGenerator ilg = cc.ILGenerator;
            if (tail) ilg.Tail();
            ilg.CallVirt(funcType.Resolve(references).GetMethod("Invoke"));
            if (tail) ilg.Return();
        }

        #endregion
    }

    public class BinaryOpExpr2 : IExpression2
    {
        private BinaryOp op;
        private IExpression2 addend1;
        private IExpression2 addend2;

        public BinaryOpExpr2(BinaryOp op, IExpression2 addend1, IExpression2 addend2)
        {
            this.op = op;
            this.addend1 = addend1;
            this.addend2 = addend2;
        }

        #region IExpression2 Members

        public EnvSpec GetEnvSpec()
        {
            return addend1.GetEnvSpec() | addend2.GetEnvSpec();
        }

        private static BinaryOp[] opArray = new BinaryOp[]
        {
            BinaryOp.Atan2, BinaryOp.IEEERemainder, BinaryOp.LogBase,
            BinaryOp.Max, BinaryOp.Min, BinaryOp.Pow
        };

        private static string[] opName = new string[]
        {
            "Atan2", "IEEERemainder", "Log",
            "Max", "Min", "Pow"
        };


        private static Type[] opTypes = new Type[]
        {
            typeof(byte), typeof(short), typeof(int), typeof(long), typeof(IntPtr),
            typeof(sbyte), typeof(ushort), typeof(uint), typeof(ulong), typeof(UIntPtr)
        };

        private static BinaryOp[] opF = new BinaryOp[]
        {
            BinaryOp.Add, BinaryOp.Sub, BinaryOp.Mul, BinaryOp.Div
        };

        private static Type[] opTypesF = new Type[]
        {
            typeof(byte), typeof(short), typeof(int), typeof(long), typeof(IntPtr),
            typeof(sbyte), typeof(ushort), typeof(uint), typeof(ulong), typeof(UIntPtr),
            typeof(float), typeof(double)
        };

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            TypeReference t1 = addend1.GetReturnType(s, envDesc);
            TypeReference t2 = addend2.GetReturnType(s, envDesc);

            if (!(t1 is ExistingTypeReference)) throw new PascalesqueException("Binary op on unsupported type");
            if (!(t2 is ExistingTypeReference)) throw new PascalesqueException("Binary op on unsupported type");

            ExistingTypeReference et1 = (ExistingTypeReference)t1;
            ExistingTypeReference et2 = (ExistingTypeReference)t2;

            if (op == BinaryOp.Shl || op == BinaryOp.Shr)
            {
                if (t2 != ExistingTypeReference.Int32 && t2 != ExistingTypeReference.IntPtr) throw new PascalesqueException("Shift amount must be int or IntPtr");
            }
            else if (opArray.Contains(op))
            {

                int index = Array.IndexOf<BinaryOp>(opArray, op);
                System.Diagnostics.Debug.Assert(index >= 0 && index < opArray.Length);
                MethodInfo mi = typeof(Math).GetMethod(opName[index], BindingFlags.Public | BindingFlags.Static, null, new Type[] { et1.ExistingType, et2.ExistingType }, null);
                if (mi == null) throw new PascalesqueException("Unknown binary op / types");
                return new ExistingTypeReference(mi.ReturnType);
            }
            else
            {
                if (t1 != t2) throw new PascalesqueException("Attempt to do binary op on two different types");

                Type[] opTypes1 = (opF.Any(x => x == op)) ? opTypesF : opTypes;

                if (!opTypes1.Any(x => x == et1.ExistingType)) throw new PascalesqueException("Attempt to do a binary op on an unsupported type");
            }

            return t1;
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            addend1.AddCompileSteps(s, owner, envDesc, add);
            addend2.AddCompileSteps(s, owner, envDesc, add);
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            return addend1.GetReferences(s, owner, envDesc) | addend2.GetReferences(s, owner, envDesc);
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, SaBox<object>> references, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            EnvDescTypesOnly2 edto = envDesc.TypesOnly();
            TypeReference t1 = addend1.GetReturnType(s, edto);
            TypeReference t2 = addend2.GetReturnType(s, edto);

            if (!(t1 is ExistingTypeReference)) throw new PascalesqueException("Binary op on unsupported type");
            if (!(t2 is ExistingTypeReference)) throw new PascalesqueException("Binary op on unsupported type");

            ExistingTypeReference et1 = (ExistingTypeReference)t1;
            ExistingTypeReference et2 = (ExistingTypeReference)t2;

            bool isUnsigned = (et1.ExistingType == typeof(byte) || et1.ExistingType == typeof(ushort) || et1.ExistingType == typeof(uint) || et1.ExistingType == typeof(ulong));

            addend1.Compile(s, owner, cc, envDesc, references, false);
            addend2.Compile(s, owner, cc, envDesc, references, false);
            switch (op)
            {
                case BinaryOp.Add:
                    ilg.Add();
                    break;
                case BinaryOp.Sub:
                    ilg.Sub();
                    break;
                case BinaryOp.Mul:
                    ilg.Mul();
                    break;
                case BinaryOp.Div:
                    if (isUnsigned)
                    {
                        ilg.DivUn();
                    }
                    else
                    {
                        ilg.Div();
                    }
                    break;
                case BinaryOp.Rem:
                    if (isUnsigned)
                    {
                        ilg.RemUn();
                    }
                    else
                    {
                        ilg.Rem();
                    }
                    break;
                case BinaryOp.And:
                    ilg.And();
                    break;
                case BinaryOp.Or:
                    ilg.Or();
                    break;
                case BinaryOp.Xor:
                    ilg.Xor();
                    break;
                case BinaryOp.Shl:
                    ilg.Shl();
                    break;
                case BinaryOp.Shr:
                    if (isUnsigned)
                    {
                        ilg.ShrUn();
                    }
                    else
                    {
                        ilg.Shr();
                    }
                    break;
                default:
                    if (opArray.Contains(op))
                    {
                        int index = Array.IndexOf<BinaryOp>(opArray, op);
                        System.Diagnostics.Debug.Assert(index >= 0 && index < opArray.Length);
                        MethodInfo mi = typeof(Math).GetMethod(opName[index], BindingFlags.Public | BindingFlags.Static, null, new Type[] { et1.ExistingType, et2.ExistingType }, null);
                        if (mi == null) throw new PascalesqueException("Unknown binary op / types");
                        if (tail) ilg.Tail();
                        ilg.Call(mi);
                    }
                    else
                    {
                        throw new PascalesqueException("Unknown binary op / type combination");
                    }
                    break;
            }
            if (tail) ilg.Return();
        }

        #endregion
    }

    public class UnaryOpExpr2 : IExpression2
    {
        UnaryOp op;
        IExpression2 expr;

        public UnaryOpExpr2(UnaryOp op, IExpression2 expr)
        {
            this.op = op;
            this.expr = expr;
        }

        #region IExpression2 Members

        public EnvSpec GetEnvSpec()
        {
            return expr.GetEnvSpec();
        }

        private static UnaryOp[] opArray = new UnaryOp[]
        {
            UnaryOp.Abs, UnaryOp.Acos, UnaryOp.Asin, UnaryOp.Atan, UnaryOp.Ceil,
            UnaryOp.Cos, UnaryOp.Cosh, UnaryOp.Exp, UnaryOp.Floor, UnaryOp.Log,
            UnaryOp.Log10, UnaryOp.Round, UnaryOp.Sign, UnaryOp.Sin, UnaryOp.Sinh,
            UnaryOp.Sqrt, UnaryOp.Tan, UnaryOp.Tanh, UnaryOp.Trunc
        };

        private static string[] opName = new string[]
        {
            "Abs", "Acos", "Asin", "Atan", "Ceiling",
            "Cos", "Cosh", "Exp", "Floor", "Log",
            "Log10", "Round", "Sign", "Sin", "Sinh",
            "Sqrt", "Tan", "Tanh", "Truncate"
        };

        private static MethodInfo GetMathMethod(UnaryOp u, Type t)
        {
            int index = Array.IndexOf<UnaryOp>(opArray, u);
            if (index < 0 || index >= opArray.Length) return null;
            return typeof(Math).GetMethod(opName[index], BindingFlags.Public | BindingFlags.Static, null, new Type[] { t }, null);
        }

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            TypeReference t1 = expr.GetReturnType(s, envDesc);
            if (!(t1 is ExistingTypeReference)) throw new PascalesqueException("Unsupported type for unary operation");
            ExistingTypeReference et1 = (ExistingTypeReference)t1;
            Type t = et1.ExistingType;

            if (op == UnaryOp.Invert)
            {
                Type[] supportedTypes = new Type[]
                {
                    typeof(byte), typeof(ushort), typeof(uint), typeof(ulong), typeof(UIntPtr),
                    typeof(sbyte), typeof(short), typeof(int), typeof(long), typeof(IntPtr)
                };
                if (!supportedTypes.Any(x => t == x)) throw new PascalesqueException("Unsupported type for invert");
            }
            else if (op == UnaryOp.Negate)
            {
                Type[] supportedTypes = new Type[]
                {
                    typeof(byte), typeof(ushort), typeof(uint), typeof(ulong), typeof(UIntPtr),
                    typeof(sbyte), typeof(short), typeof(int), typeof(long), typeof(IntPtr)
                };
                if (!supportedTypes.Any(x => t == x)) throw new PascalesqueException("Unsupported type for negate");
            }
            else if (op == UnaryOp.Not)
            {
                Type[] supportedTypes = new Type[]
                {
                    typeof(byte), typeof(ushort), typeof(uint), typeof(ulong), typeof(UIntPtr),
                    typeof(sbyte), typeof(short), typeof(int), typeof(long), typeof(IntPtr),
                    typeof(bool)
                };
                if (!supportedTypes.Any(x => t == x)) throw new PascalesqueException("Unsupported type for not");
                return ExistingTypeReference.Boolean;
            }
            else
            {
                MethodInfo m = GetMathMethod(op, t);
                if (m == null) throw new PascalesqueException("Unknown UnaryOp / type");
                return new ExistingTypeReference(m.ReturnType);
            }
            return t1;
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            expr.AddCompileSteps(s, owner, envDesc, add);
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            return expr.GetReferences(s, owner, envDesc);
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, SaBox<object>> references, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            Type exprType = expr.GetReturnType(s, envDesc.TypesOnly()).Resolve(references);

            expr.Compile(s, owner, cc, envDesc, references, false);
            switch (op)
            {
                case UnaryOp.Invert:
                    ilg.Invert();
                    break;
                case UnaryOp.Negate:
                    ilg.Negate();
                    break;
                case UnaryOp.Not:
                    ilg.Not();
                    break;
                default:
                    MethodInfo m = GetMathMethod(op, exprType);
                    if (m == null) throw new PascalesqueException("Unknown UnaryOp / type (missed the first time)");
                    if (tail) ilg.Tail();
                    ilg.Call(m);
                    break;
            }
            if (tail) ilg.Return();
        }

        #endregion
    }

    public class ConvertExpr2 : IExpression2
    {
        private ConvertTo convertTo;
        private IExpression2 expression;

        public ConvertExpr2(ConvertTo convertTo, IExpression2 expression)
        {
            this.convertTo = convertTo;
            this.expression = expression;
        }

        #region IExpression2 Members

        public EnvSpec GetEnvSpec()
        {
            return expression.GetEnvSpec();
        }

        public static TypeReference GetReturnType(ConvertTo convertTo)
        {
            switch (convertTo)
            {
                case ConvertTo.Byte: return ExistingTypeReference.Byte;
                case ConvertTo.Short: return ExistingTypeReference.Int16;
                case ConvertTo.Int: return ExistingTypeReference.Int32;
                case ConvertTo.Long: return ExistingTypeReference.Int64;
                case ConvertTo.IntPtr: return ExistingTypeReference.IntPtr;
                case ConvertTo.SByte: return ExistingTypeReference.SByte;
                case ConvertTo.UShort: return ExistingTypeReference.UInt16;
                case ConvertTo.UInt: return ExistingTypeReference.UInt32;
                case ConvertTo.ULong: return ExistingTypeReference.UInt64;
                case ConvertTo.UIntPtr: return ExistingTypeReference.UIntPtr;
                case ConvertTo.Float: return ExistingTypeReference.Single;
                case ConvertTo.Double: return ExistingTypeReference.Double;
                default: throw new PascalesqueException("Unknown ConvertTo type");
            }
        }

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            return GetReturnType(convertTo);
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            expression.AddCompileSteps(s, owner, envDesc, add);
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            return expression.GetReferences(s, owner, envDesc);
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, SaBox<object>> references, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            TypeReference exprType = expression.GetReturnType(s, envDesc.TypesOnly());

            bool isUnsigned = (exprType == ExistingTypeReference.Byte || exprType == ExistingTypeReference.UInt16 || exprType == ExistingTypeReference.UInt32 || exprType == ExistingTypeReference.UInt64 || exprType == ExistingTypeReference.UIntPtr);

            expression.Compile(s, owner, cc, envDesc, references, false);
            switch (convertTo)
            {
                case ConvertTo.Byte: ilg.Conv_U1(); break;
                case ConvertTo.Short: ilg.Conv_I2(); break;
                case ConvertTo.Int: ilg.Conv_I4(); break;
                case ConvertTo.Long: ilg.Conv_I8(); break;
                case ConvertTo.IntPtr: ilg.Conv_I(); break;
                case ConvertTo.SByte: ilg.Conv_I1(); break;
                case ConvertTo.UShort: ilg.Conv_U2(); break;
                case ConvertTo.UInt: ilg.Conv_U4(); break;
                case ConvertTo.ULong: ilg.Conv_U8(); break;
                case ConvertTo.UIntPtr: ilg.Conv_U(); break;
                case ConvertTo.Float: if (isUnsigned) { ilg.Conv_R_Un(); } ilg.Conv_R4(); break;
                case ConvertTo.Double: if (isUnsigned) { ilg.Conv_R_Un(); } ilg.Conv_R8(); break;
            }
            if (tail) ilg.Return();
        }

        #endregion
    }

    public class RegardAsExpr2 : IExpression2
    {
        private ConvertTo regardAsWhat;
        private IExpression2 expression;

        public RegardAsExpr2(ConvertTo regardAsWhat, IExpression2 expression)
        {
            this.regardAsWhat = regardAsWhat;
            this.expression = expression;
        }

        public static ActualStackType GetActualStackType(ConvertTo convertTo)
        {
            switch (convertTo)
            {
                case ConvertTo.Byte: return ActualStackType.Int32;
                case ConvertTo.SByte: return ActualStackType.Int32;
                case ConvertTo.Short: return ActualStackType.Int32;
                case ConvertTo.UShort: return ActualStackType.Int32;
                case ConvertTo.Int: return ActualStackType.Int32;
                case ConvertTo.UInt: return ActualStackType.Int32;

                case ConvertTo.Long: return ActualStackType.Int64;
                case ConvertTo.ULong: return ActualStackType.Int64;

                case ConvertTo.IntPtr: return ActualStackType.IntPtr;
                case ConvertTo.UIntPtr: return ActualStackType.IntPtr;

                case ConvertTo.Float: return ActualStackType.Float;
                case ConvertTo.Double: return ActualStackType.Float;

                default: throw new ArgumentException("Unknown ConvertTo type");
            }
        }

        public static ActualStackType GetActualStackType(TypeReference t)
        {
            if (t == ExistingTypeReference.Byte || t == ExistingTypeReference.SByte || t == ExistingTypeReference.Int16 || t == ExistingTypeReference.UInt16 || t == ExistingTypeReference.Int32 || t == ExistingTypeReference.UInt32 || t == ExistingTypeReference.Boolean || t == ExistingTypeReference.Char)
            {
                return ActualStackType.Int32;
            }
            else if (t == ExistingTypeReference.Int64 || t == ExistingTypeReference.UInt64)
            {
                return ActualStackType.Int64;
            }
            else if (t == ExistingTypeReference.IntPtr || t == ExistingTypeReference.UIntPtr)
            {
                return ActualStackType.IntPtr;
            }
            else if (t == ExistingTypeReference.Single || t == ExistingTypeReference.Double)
            {
                return ActualStackType.Float;
            }
            else throw new ArgumentException("Unknown actual stack type");
        }

        #region IExpression Members

        public EnvSpec GetEnvSpec()
        {
            return expression.GetEnvSpec();
        }

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            TypeReference eType = expression.GetReturnType(s, envDesc);

            if (GetActualStackType(eType) != GetActualStackType(regardAsWhat))
                throw new PascalesqueException("RegardAs doesn't work if types are physically different");

            return ConvertExpr2.GetReturnType(regardAsWhat);
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            expression.AddCompileSteps(s, owner, envDesc, add);
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            return expression.GetReferences(s, owner, envDesc);
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, SaBox<object>> references, bool tail)
        {
            expression.Compile(s, owner, cc, envDesc, references, tail);
        }

        #endregion
    }

    public class ComparisonExpr2 : IExpression2
    {
        private Comparison comp;
        private IExpression2 expr1;
        private IExpression2 expr2;

        public ComparisonExpr2(Comparison comp, IExpression2 expr1, IExpression2 expr2)
        {
            this.comp = comp;
            this.expr1 = expr1;
            this.expr2 = expr2;
        }

        #region IExpression2 Members

        public EnvSpec GetEnvSpec()
        {
            return expr1.GetEnvSpec() | expr2.GetEnvSpec();
        }

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            TypeReference t1 = expr1.GetReturnType(s, envDesc);
            TypeReference t2 = expr2.GetReturnType(s, envDesc);
            if (t1 != t2) throw new PascalesqueException("Comparison requires operands of same type");

            return ExistingTypeReference.Boolean;
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            expr1.AddCompileSteps(s, owner, envDesc, add);
            expr2.AddCompileSteps(s, owner, envDesc, add);
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            return expr1.GetReferences(s, owner, envDesc) | expr2.GetReferences(s, owner, envDesc);
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, SaBox<object>> references, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            TypeReference t = expr1.GetReturnType(s, envDesc.TypesOnly());
            bool isUnsigned = (t == ExistingTypeReference.Byte || t == ExistingTypeReference.UInt16 || t == ExistingTypeReference.UInt32 || t == ExistingTypeReference.UInt64);

            expr1.Compile(s, owner, cc, envDesc, references, false);

            if (t == ExistingTypeReference.Byte) ilg.Conv_U1();
            else if (t == ExistingTypeReference.Int16) ilg.Conv_I2();
            else if (t == ExistingTypeReference.SByte) ilg.Conv_I1();
            else if (t == ExistingTypeReference.UInt16) ilg.Conv_U2();

            expr2.Compile(s, owner, cc, envDesc, references, false);

            if (t == ExistingTypeReference.Byte) ilg.Conv_U1();
            else if (t == ExistingTypeReference.Int16) ilg.Conv_I2();
            else if (t == ExistingTypeReference.SByte) ilg.Conv_I1();
            else if (t == ExistingTypeReference.UInt16) ilg.Conv_U2();

            switch (comp)
            {
                case Comparison.LessThan:
                    if (isUnsigned)
                    {
                        ilg.CltUn();
                    }
                    else
                    {
                        ilg.Clt();
                    }
                    break;
                case Comparison.GreaterThan:
                    if (isUnsigned)
                    {
                        ilg.CgtUn();
                    }
                    else
                    {
                        ilg.Cgt();
                    }
                    break;
                case Comparison.LessEqual:
                    if (isUnsigned)
                    {
                        ilg.CgtUn();
                    }
                    else
                    {
                        ilg.Cgt();
                    }
                    ilg.Not();
                    break;
                case Comparison.GreaterEqual:
                    if (isUnsigned)
                    {
                        ilg.CltUn();
                    }
                    else
                    {
                        ilg.Clt();
                    }
                    ilg.Not();
                    break;
                case Comparison.Equal:
                    ilg.Ceq();
                    break;
                case Comparison.NotEqual:
                    ilg.Ceq();
                    ilg.Not();
                    break;
            }

            if (tail) ilg.Return();
        }

        #endregion
    }

    public class ArrayLenExpr2 : IExpression2
    {
        private IExpression2 array;

        public ArrayLenExpr2(IExpression2 array)
        {
            this.array = array;
        }

        #region IExpression2 Members

        public EnvSpec GetEnvSpec()
        {
            return array.GetEnvSpec();
        }

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            TypeReference tx = array.GetReturnType(s, envDesc);
            if (!(tx.IsArray)) throw new PascalesqueException("ArrayLen requires an array");

            return ExistingTypeReference.Int32;
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            array.AddCompileSteps(s, owner, envDesc, add);
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            return array.GetReferences(s, owner, envDesc);
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, SaBox<object>> references, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            TypeReference tx = array.GetReturnType(s, envDesc.TypesOnly());

            array.Compile(s, owner, cc, envDesc, references, false);

            if (tail) ilg.Tail();
            ilg.CallVirt(tx.Resolve(references).GetMethod("get_Length", Type.EmptyTypes));
            if (tail) ilg.Return();
        }

        #endregion
    }

    public class ArrayRefExpr2 : IExpression2
    {
        private IExpression2 array;
        private IExpression2 index;

        public ArrayRefExpr2(IExpression2 array, IExpression2 index)
        {
            this.array = array;
            this.index = index;
        }

        #region IExpression2 Members

        public EnvSpec GetEnvSpec()
        {
            return array.GetEnvSpec() | index.GetEnvSpec();
        }

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            TypeReference t = array.GetReturnType(s, envDesc);
            if (!(t.IsArray)) throw new PascalesqueException("ArrayRef type mismatch; array required");

            TypeReference i = index.GetReturnType(s, envDesc);
            if (i != ExistingTypeReference.Int32 && i != ExistingTypeReference.IntPtr) throw new PascalesqueException("ArrayRef type mismatch; index must be int or IntPtr");

            return t.GetElementType();
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            array.AddCompileSteps(s, owner, envDesc, add);
            index.AddCompileSteps(s, owner, envDesc, add);
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            return array.GetReferences(s, owner, envDesc) | array.GetReturnType(s, envDesc).GetElementType().GetReferences() | index.GetReferences(s, owner, envDesc);
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, SaBox<object>> references, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            TypeReference t = array.GetReturnType(s, envDesc.TypesOnly());
            if (!(t.IsArray)) throw new PascalesqueException("ArrayRef type mismatch; array required");

            TypeReference elType = t.GetElementType();

            array.Compile(s, owner, cc, envDesc, references, false);
            index.Compile(s, owner, cc, envDesc, references, false);
            ilg.LoadElement(elType.Resolve(references));
            if (tail) ilg.Return();
        }

        #endregion
    }

    public class ArraySetExpr2 : IExpression2
    {
        private IExpression2 array;
        private IExpression2 index;
        private IExpression2 value;

        public ArraySetExpr2(IExpression2 array, IExpression2 index, IExpression2 value)
        {
            this.array = array;
            this.index = index;
            this.value = value;
        }

        #region IExpression2 Members

        public EnvSpec GetEnvSpec()
        {
            return array.GetEnvSpec() | index.GetEnvSpec() | value.GetEnvSpec();
        }

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            TypeReference t = array.GetReturnType(s, envDesc);
            if (!(t.IsArray)) throw new PascalesqueException("ArraySet type mismatch; array required");

            TypeReference i = index.GetReturnType(s, envDesc);
            if (i != ExistingTypeReference.Int32 && i != ExistingTypeReference.IntPtr) throw new PascalesqueException("ArraySet type mismatch; index must be int or IntPtr");

            TypeReference x = value.GetReturnType(s, envDesc);
            if (x != t.GetElementType()) throw new PascalesqueException("ArraySet type mismatch; value must match item type of array");

            return ExistingTypeReference.Void;
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            array.AddCompileSteps(s, owner, envDesc, add);
            index.AddCompileSteps(s, owner, envDesc, add);
            value.AddCompileSteps(s, owner, envDesc, add);
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            return array.GetReferences(s, owner, envDesc) |
                array.GetReturnType(s, envDesc).GetElementType().GetReferences() |
                index.GetReferences(s, owner, envDesc) |
                value.GetReferences(s, owner, envDesc);
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, SaBox<object>> references, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            TypeReference t = array.GetReturnType(s, envDesc.TypesOnly());
            if (!(t.IsArray)) throw new PascalesqueException("ArrayRef type mismatch; array required");

            TypeReference elType = t.GetElementType();

            array.Compile(s, owner, cc, envDesc, references, false);
            index.Compile(s, owner, cc, envDesc, references, false);
            value.Compile(s, owner, cc, envDesc, references, false);
            ilg.StoreElement(elType.Resolve(references));
            if (tail) ilg.Return();
        }

        #endregion
    }

    public class NewArrayExpr2 : IExpression2
    {
        private TypeReference itemType;
        private IExpression2 size;

        public NewArrayExpr2(TypeReference itemType, IExpression2 size)
        {
            this.itemType = itemType;
            this.size = size;
        }

        #region IExpression2 Members

        public EnvSpec GetEnvSpec()
        {
            return size.GetEnvSpec();
        }

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            TypeReference sizeType = size.GetReturnType(s, envDesc);
            if (sizeType != ExistingTypeReference.Int32 && sizeType != ExistingTypeReference.IntPtr) throw new PascalesqueException("Incorrect type for size of new array");
            return itemType.MakeArrayType();
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            size.AddCompileSteps(s, owner, envDesc, add);
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            return size.GetReferences(s, owner, envDesc) | itemType.GetReferences();
        }


        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, SaBox<object>> references, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            size.Compile(s, owner, cc, envDesc, references, false);
            ilg.Emit(OpCodes.Newarr, itemType.Resolve(references));
            if (tail) ilg.Return();
        }

        #endregion
    }

    public class MethodCallExpr2 : IExpression2
    {
        private MethodReference methodToCall;
        private bool allowVirtual;
        private List<IExpression2> arguments;

        public MethodCallExpr2(MethodReference methodToCall, bool allowVirtual, IEnumerable<IExpression2> arguments)
        {
            this.methodToCall = methodToCall;
            this.allowVirtual = allowVirtual;
            this.arguments = arguments.ToList();
        }

        #region IExpression2 Members

        public EnvSpec GetEnvSpec()
        {
            EnvSpec e = EnvSpec.Empty();
            foreach (IExpression2 arg in arguments)
            {
                e |= arg.GetEnvSpec();
            }
            return e;
        }

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            int iEnd = methodToCall.ParameterCount;
            if (arguments.Count != iEnd) throw new PascalesqueException("Argument count doesn't match parameter count");

            for (int i = 0; i < iEnd; ++i)
            {
                TypeReference tParam = methodToCall.GetParameterType(i);
                //if (tParam.IsByRef) throw new PascalesqueException("ByRef parameters not supported");
                TypeReference tArg = arguments[i].GetReturnType(s, envDesc);
                if (tParam != tArg) throw new PascalesqueException("Argument type doesn't match parameter type");
            }

            return methodToCall.GetReturnType(s);
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            foreach (IExpression2 arg in arguments)
            {
                arg.AddCompileSteps(s, owner, envDesc, add);
            }
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            return arguments.Select(x => x.GetReferences(s, owner, envDesc)).HashSet2Union() | methodToCall.GetReferences();
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, SaBox<object>> references, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            foreach (IExpression2 arg in arguments)
            {
                arg.Compile(s, owner, cc, envDesc, references, false);
            }

            MethodInfo mi = methodToCall.Resolve(references);
            if (mi.IsVirtual && allowVirtual)
            {
                if (tail) ilg.Tail();
                ilg.CallVirt(mi);
                if (tail) ilg.Return();
            }
            else
            {
                if (tail) ilg.Tail();
                ilg.Call(mi);
                if (tail) ilg.Return();
            }
        }

        #endregion
    }

    public class ConstructorCallExpr2 : IExpression2
    {
        private ConstructorReference constructorToCall;
        private IExpression2 thisObj;
        private List<IExpression2> arguments;

        public ConstructorCallExpr2(ConstructorReference constructorToCall, IExpression2 thisObj, IEnumerable<IExpression2> arguments)
        {
            this.constructorToCall = constructorToCall;
            this.thisObj = thisObj;
            this.arguments = arguments.ToList();
        }

        #region IExpression2 Members

        public EnvSpec GetEnvSpec()
        {
            EnvSpec e = thisObj.GetEnvSpec();
            foreach (IExpression2 arg in arguments)
            {
                e |= arg.GetEnvSpec();
            }
            return e;
        }

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            int iEnd = constructorToCall.ParameterCount;
            if (arguments.Count != iEnd) throw new PascalesqueException("Argument count doesn't match parameter count");

            // thisObj type not checked

            for (int i = 0; i < iEnd; ++i)
            {
                TypeReference tParam = constructorToCall.GetParameterType(i);
                //if (tParam.IsByRef) throw new PascalesqueException("ByRef parameters not supported");
                TypeReference tArg = arguments[i].GetReturnType(s, envDesc);
                if (tParam != tArg) throw new PascalesqueException("Argument type doesn't match parameter type");
            }

            return ExistingTypeReference.Void;
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            thisObj.AddCompileSteps(s, owner, envDesc, add);
            foreach (IExpression2 arg in arguments)
            {
                arg.AddCompileSteps(s, owner, envDesc, add);
            }
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            return thisObj.GetReferences(s, owner, envDesc) | arguments.Select(x => x.GetReferences(s, owner, envDesc)).HashSet2Union() | constructorToCall.GetReferences();
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, SaBox<object>> references, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            thisObj.Compile(s, owner, cc, envDesc, references, false);

            foreach (IExpression2 arg in arguments)
            {
                arg.Compile(s, owner, cc, envDesc, references, false);
            }

            ConstructorInfo ci = constructorToCall.Resolve(references);

            if (tail) ilg.Tail();
            ilg.Call(ci);
            if (tail) ilg.Return();
        }

        #endregion
    }

    public class NewObjExpr2 : IExpression2
    {
        private ConstructorReference constructorToCall;
        private List<IExpression2> arguments;

        public NewObjExpr2(ConstructorReference constructorToCall, IEnumerable<IExpression2> arguments)
        {
            this.constructorToCall = constructorToCall;
            this.arguments = arguments.ToList();
        }

        #region IExpression2 Members

        public EnvSpec GetEnvSpec()
        {
            EnvSpec e = EnvSpec.Empty();
            foreach (IExpression2 arg in arguments)
            {
                e |= arg.GetEnvSpec();
            }
            return e;
        }

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            int iEnd = arguments.Count;
            for (int i = 0; i < iEnd; ++i)
            {
                TypeReference t1 = arguments[i].GetReturnType(s, envDesc);
                TypeReference t2 = constructorToCall.GetParameterType(i);
                if (t1 != t2) throw new PascalesqueException("Type mismatch in NewObj constructor call");
            }
            return constructorToCall.ConstructorOfWhat;
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            foreach (IExpression2 arg in arguments)
            {
                arg.AddCompileSteps(s, owner, envDesc, add);
            }
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            return arguments.Select(x => x.GetReferences(s, owner, envDesc)).HashSet2Union();
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, SaBox<object>> references, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            foreach (IExpression2 arg in arguments)
            {
                arg.Compile(s, owner, cc, envDesc, references, false);
            }

            ConstructorInfo ci = constructorToCall.Resolve(references);

            ilg.NewObj(ci);
            if (tail) ilg.Return();
        }

        #endregion
    }

    public class FieldRefExpr2 : IExpression2
    {
        private IExpression2 fieldOfWhat;
        private FieldReference fieldReference;

        public FieldRefExpr2(IExpression2 fieldOfWhat, FieldReference fieldReference)
        {
            this.fieldOfWhat = fieldOfWhat;
            this.fieldReference = fieldReference;
        }

        #region IExpression2 Members

        public EnvSpec GetEnvSpec()
        {
            return fieldOfWhat.GetEnvSpec();
        }

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            TypeReference t = fieldOfWhat.GetReturnType(s, envDesc);
            if (t != fieldReference.Owner) throw new PascalesqueException("Type Mismatch for Field Reference");

            return fieldReference.FieldType;
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            fieldOfWhat.AddCompileSteps(s, owner, envDesc, add);
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            return fieldOfWhat.GetReferences(s, owner, envDesc) | fieldReference.GetReferences();
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, SaBox<object>> references, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            fieldOfWhat.Compile(s, owner, cc, envDesc, references, false);
            ilg.LoadField(fieldReference.Resolve(references));
            if (tail) ilg.Return();
        }

        #endregion
    }

    public class FieldSetExpr2 : IExpression2
    {
        private IExpression2 fieldOfWhat;
        private FieldReference fieldReference;
        private IExpression2 val;

        public FieldSetExpr2(IExpression2 fieldOfWhat, FieldReference fieldReference, IExpression2 val)
        {
            this.fieldOfWhat = fieldOfWhat;
            this.fieldReference = fieldReference;
            this.val = val;
        }

        #region IExpression2 Members

        public EnvSpec GetEnvSpec()
        {
            return fieldOfWhat.GetEnvSpec() | val.GetEnvSpec();
        }

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            TypeReference t1 = fieldOfWhat.GetReturnType(s, envDesc);
            TypeReference t2 = val.GetReturnType(s, envDesc);
            if (t1 != fieldReference.Owner || t2 != fieldReference.FieldType) throw new PascalesqueException("Type mismatch for Field Set");

            return ExistingTypeReference.Void;
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            fieldOfWhat.AddCompileSteps(s, owner, envDesc, add);
            val.AddCompileSteps(s, owner, envDesc, add);
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            return fieldOfWhat.GetReferences(s, owner, envDesc) | val.GetReferences(s, owner, envDesc) | fieldReference.GetReferences();
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, SaBox<object>> references, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            fieldOfWhat.Compile(s, owner, cc, envDesc, references, false);
            val.Compile(s, owner, cc, envDesc, references, false);
            ilg.StoreField(fieldReference.Resolve(references));
            if (tail) ilg.Return();
        }

        #endregion
    }

    public class PokeExpr2 : IExpression2
    {
        private IExpression2 ptr;
        private IExpression2 value;

        public PokeExpr2(IExpression2 ptr, IExpression2 value)
        {
            this.ptr = ptr;
            this.value = value;
        }

        #region IExpression2 Members

        public EnvSpec GetEnvSpec()
        {
            return ptr.GetEnvSpec() | value.GetEnvSpec();
        }

        private static Type[] okTypes = new Type[]
        {
            typeof(byte), typeof(short), typeof(int), typeof(long), typeof(IntPtr),
            typeof(sbyte), typeof(ushort), typeof(uint), typeof(ulong), typeof(UIntPtr),
            typeof(float), typeof(double), typeof(char), typeof(bool)
        };

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            TypeReference t1 = ptr.GetReturnType(s, envDesc);
            TypeReference t2 = value.GetReturnType(s, envDesc);

            if (!(t2 is ExistingTypeReference) || !(okTypes.Contains(((ExistingTypeReference)t2).ExistingType))) throw new PascalesqueException("Poke: argument type cannot be poked");

            if (t1 != ExistingTypeReference.IntPtr && t1 != ExistingTypeReference.UIntPtr) throw new PascalesqueException("Poke: pointer type is not IntPtr");

            return ExistingTypeReference.Void;
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            ptr.AddCompileSteps(s, owner, envDesc, add);
            value.AddCompileSteps(s, owner, envDesc, add);
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            return ptr.GetReferences(s, owner, envDesc) | value.GetReferences(s, owner, envDesc);
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, SaBox<object>> references, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            ptr.Compile(s, owner, cc, envDesc, references, false);
            value.Compile(s, owner, cc, envDesc, references, false);
            Type t2 = value.GetReturnType(s, envDesc.TypesOnly()).Resolve(references);
            ilg.Unaligned(Alignment.One);
            ilg.StoreObjIndirect(t2);
            if (tail) ilg.Return();
        }

        #endregion
    }

    public class PeekExpr2 : IExpression2
    {
        private IExpression2 ptr;
        private TypeReference type;

        public PeekExpr2(IExpression2 ptr, TypeReference type)
        {
            this.ptr = ptr;
            this.type = type;
        }

        #region IExpression2 Members

        public EnvSpec GetEnvSpec()
        {
            return ptr.GetEnvSpec();
        }

        private static Type[] okTypes = new Type[]
        {
            typeof(byte), typeof(short), typeof(int), typeof(long), typeof(IntPtr),
            typeof(sbyte), typeof(ushort), typeof(uint), typeof(ulong), typeof(UIntPtr),
            typeof(float), typeof(double), typeof(char), typeof(bool)
        };

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            if (!(type is ExistingTypeReference) || !(okTypes.Contains(((ExistingTypeReference)type).ExistingType))) throw new PascalesqueException("Peek: type can't be peeked");

            return type;
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            ptr.AddCompileSteps(s, owner, envDesc, add);
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            return ptr.GetReferences(s, owner, envDesc);
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, SaBox<object>> references, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;
            ptr.Compile(s, owner, cc, envDesc, references, false);
            ilg.Unaligned(Alignment.One);
            ilg.LoadObjIndirect(type.Resolve(references));
            if (tail) ilg.Return();
        }

        #endregion
    }

    public class MemCpyExpr2 : IExpression2
    {
        private IExpression2 destAddr;
        private IExpression2 srcAddr;
        private IExpression2 count;

        public MemCpyExpr2(IExpression2 destAddr, IExpression2 srcAddr, IExpression2 count)
        {
            this.destAddr = destAddr;
            this.srcAddr = srcAddr;
            this.count = count;
        }

        #region IExpression2 Members

        public EnvSpec GetEnvSpec()
        {
            return destAddr.GetEnvSpec() | srcAddr.GetEnvSpec() | count.GetEnvSpec();
        }

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            TypeReference destAddrType = destAddr.GetReturnType(s, envDesc);
            TypeReference srcAddrType = srcAddr.GetReturnType(s, envDesc);
            TypeReference countType = count.GetReturnType(s, envDesc);

            if (destAddrType != ExistingTypeReference.IntPtr && destAddrType != ExistingTypeReference.UIntPtr) throw new PascalesqueException("memcpy dest address must be IntPtr or UIntPtr");
            if (srcAddrType != ExistingTypeReference.IntPtr && srcAddrType != ExistingTypeReference.UIntPtr) throw new PascalesqueException("memcpy source address must be IntPtr or UIntPtr");
            if (countType != ExistingTypeReference.UInt32) throw new PascalesqueException("memcpy count must be uint");

            return ExistingTypeReference.Void;
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            destAddr.AddCompileSteps(s, owner, envDesc, add);
            srcAddr.AddCompileSteps(s, owner, envDesc, add);
            count.AddCompileSteps(s, owner, envDesc, add);
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            return destAddr.GetReferences(s, owner, envDesc) | srcAddr.GetReferences(s, owner, envDesc) | count.GetReferences(s, owner, envDesc);
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, SaBox<object>> references, bool tail)
        {
            destAddr.Compile(s, owner, cc, envDesc, references, false);
            srcAddr.Compile(s, owner, cc, envDesc, references, false);
            count.Compile(s, owner, cc, envDesc, references, false);
            cc.ILGenerator.Unaligned(Alignment.One);
            cc.ILGenerator.Emit(OpCodes.Cpblk);
            if (tail) cc.ILGenerator.Return();
        }

        #endregion
    }

    public class MemSetExpr2 : IExpression2
    {
        private IExpression2 destAddr;
        private IExpression2 fillValue;
        private IExpression2 count;

        public MemSetExpr2(IExpression2 destAddr, IExpression2 fillValue, IExpression2 count)
        {
            this.destAddr = destAddr;
            this.fillValue = fillValue;
            this.count = count;
        }

        #region IExpression2 Members

        public EnvSpec GetEnvSpec()
        {
            return destAddr.GetEnvSpec() | fillValue.GetEnvSpec() | count.GetEnvSpec();
        }

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            TypeReference destAddrType = destAddr.GetReturnType(s, envDesc);
            TypeReference fillValueType = fillValue.GetReturnType(s, envDesc);
            TypeReference countType = count.GetReturnType(s, envDesc);

            if (destAddrType != ExistingTypeReference.IntPtr && destAddrType != ExistingTypeReference.UIntPtr) throw new PascalesqueException("memset dest address must be IntPtr or UIntPtr");
            if (fillValueType != ExistingTypeReference.Byte && fillValueType != ExistingTypeReference.SByte) throw new PascalesqueException("memset fillValue must be byte or sbyte");
            if (countType != ExistingTypeReference.UInt32) throw new PascalesqueException("memset count must be uint");

            return ExistingTypeReference.Void;
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            destAddr.AddCompileSteps(s, owner, envDesc, add);
            fillValue.AddCompileSteps(s, owner, envDesc, add);
            count.AddCompileSteps(s, owner, envDesc, add);
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            return destAddr.GetReferences(s, owner, envDesc) | fillValue.GetReferences(s, owner, envDesc) | count.GetReferences(s, owner, envDesc);
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, SaBox<object>> references, bool tail)
        {
            destAddr.Compile(s, owner, cc, envDesc, references, false);
            fillValue.Compile(s, owner, cc, envDesc, references, false);
            count.Compile(s, owner, cc, envDesc, references, false);
            cc.ILGenerator.Unaligned(Alignment.One);
            cc.ILGenerator.Emit(OpCodes.Initblk);
            if (tail) cc.ILGenerator.Return();
        }

        #endregion
    }

    public class PinClause2
    {
        private Symbol name;
        private IExpression2 val;

        public PinClause2(Symbol name, IExpression2 val)
        {
            this.name = name;
            this.val = val;
        }

        public Symbol Name { get { return name; } }
        public IExpression2 Value { get { return val; } }
    }

    public class PinExpr2 : IExpression2
    {
        private List<PinClause2> clauses;
        private IExpression2 body;
        private Symbol innerMethodName;

        public PinExpr2(IEnumerable<PinClause2> clauses, IExpression2 body)
        {
            this.clauses = clauses.ToList();
            this.body = body;
            this.innerMethodName = new Symbol();
        }

        #region IExpression2 Members

        public EnvSpec GetEnvSpec()
        {
            EnvSpec e = body.GetEnvSpec() - clauses.Select(x => x.Name);
            e = EnvSpec.CaptureAll(e);
            foreach (PinClause2 lc in clauses)
            {
                e |= lc.Value.GetEnvSpec();
            }
            return e;
        }

        private static TypeReference[] okTypes = new TypeReference[]
        {
            ExistingTypeReference.Byte,
            ExistingTypeReference.Int16,
            ExistingTypeReference.Int32,
            ExistingTypeReference.Int64,
            ExistingTypeReference.IntPtr,
            ExistingTypeReference.SByte,
            ExistingTypeReference.UInt16,
            ExistingTypeReference.UInt32,
            ExistingTypeReference.UInt64,
            ExistingTypeReference.UIntPtr,
            ExistingTypeReference.Single,
            ExistingTypeReference.Double
        };

        private bool IsOkArrayType(TypeReference t)
        {
            return t.IsArray && okTypes.Contains(t.GetElementType());
        }

        private EnvDescTypesOnly2 MakeInnerEnvDesc(EnvDescTypesOnly2 outerEnvDesc)
        {
            return EnvDescTypesOnly2.Shadow(outerEnvDesc, clauses.Select(x => new ParamInfo2(x.Name, ExistingTypeReference.IntPtr)));
        }

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            if (clauses.Select(x => x.Name).HasDuplicates()) throw new PascalesqueException("pin has two variables with the same name");
            if (clauses.Any(x => !IsOkArrayType(x.Value.GetReturnType(s, envDesc)))) throw new PascalesqueException("attempt to pin impossible type");

            EnvDescTypesOnly2 e2 = MakeInnerEnvDesc(envDesc);

            return body.GetReturnType(s, e2);
        }

        private class CompileStepInfo
        {
            private PinExpr2 parent;
            private SymbolTable symbolTable;
            private TypeKey owner;
            private EnvDescTypesOnly2 envDesc;

            private Lazy<EnvDescTypesOnly2> innerEnvDesc;
            private Lazy<TypeReference> returnType;
            private Lazy<EnvSpec> capturedEnvSpec;
            private Lazy<Symbol[]> capturedVars;
            private Lazy<TypeReference[]> arrayToPinTypes;
            private Lazy<TypeReference[]> captures;
            private Lazy<TypeReference[]> paramTypes;
            private Lazy<MethodKey> innerMethod;

            public CompileStepInfo(PinExpr2 parent, SymbolTable symbolTable, TypeKey owner, EnvDescTypesOnly2 envDesc)
            {
                this.parent = parent;
                this.symbolTable = symbolTable;
                this.owner = owner;
                this.envDesc = envDesc;

                this.innerEnvDesc = new Lazy<EnvDescTypesOnly2>(() => parent.MakeInnerEnvDesc(envDesc), false);
                this.returnType = new Lazy<TypeReference>(() => parent.body.GetReturnType(symbolTable, InnerEnvDesc), false);
                this.capturedEnvSpec = new Lazy<EnvSpec>(new Func<EnvSpec>(GetCapturedEnvSpec), false);
                this.capturedVars = new Lazy<Symbol[]>(new Func<Symbol[]>(GetCapturedVars), false);
                this.arrayToPinTypes = new Lazy<TypeReference[]>(new Func<TypeReference[]>(GetArrayToPinTypes), false);
                this.captures = new Lazy<TypeReference[]>(new Func<TypeReference[]>(GetCaptures), false);
                this.paramTypes = new Lazy<TypeReference[]>(new Func<TypeReference[]>(GetParamTypes), false);
                this.innerMethod = new Lazy<MethodKey>(new Func<MethodKey>(GetInnerMethod), false);
            }

            private EnvSpec GetCapturedEnvSpec()
            {
                EnvSpec e = parent.body.GetEnvSpec() - parent.clauses.Select(x => x.Name);
                e = EnvSpec.CaptureAll(e);
                return e;
            }

            private Symbol[] GetCapturedVars()
            {
                return CapturedEnvSpec.Keys.ToArray();
            }

            private TypeReference[] GetArrayToPinTypes()
            {
                return parent.clauses.Select(x => x.Value.GetReturnType(symbolTable, envDesc)).ToArray();
            }

            private TypeReference[] GetCaptures()
            {
                return CapturedVars.Select(s => TypeReference.MakeBoxedType(envDesc[s])).ToArray();
            }

            private TypeReference[] GetParamTypes()
            {
                return Pascalesque.Utils.ConcatArrays<TypeReference>(Captures, ArrayToPinTypes);
            }

            private MethodKey GetInnerMethod()
            {
                return new MethodKey(owner, parent.innerMethodName, false, ParamTypes);
            }

            public PinExpr2 Parent { get { return parent; } }

            public SymbolTable SymbolTable { get { return symbolTable; } }

            public TypeKey Owner { get { return owner; } }

            public EnvDescTypesOnly2 EnvDesc { get { return envDesc; } }

            public EnvDescTypesOnly2 InnerEnvDesc { get { return innerEnvDesc.Value; } }

            public TypeReference ReturnType { get { return returnType.Value; } }

            public EnvSpec CapturedEnvSpec { get { return capturedEnvSpec.Value; } }

            public Symbol[] CapturedVars { get { return capturedVars.Value; } }

            public TypeReference[] ArrayToPinTypes { get { return arrayToPinTypes.Value; } }

            public TypeReference[] Captures { get { return captures.Value; } }

            public TypeReference[] ParamTypes { get { return paramTypes.Value; } }

            public MethodKey InnerMethod { get { return innerMethod.Value; } }
        }

        private class MakeMethod : ICompileStep
        {
            private CompileStepInfo info;

            public MakeMethod(CompileStepInfo info)
            {
                this.info = info;
            }

            #region ICompileStep Members

            public int Phase
            {
                get { return 1; }
            }

            public HashSet2<ItemKey> Inputs
            {
                get
                {
                    return HashSet2<ItemKey>.Singleton(info.Owner) |
                        info.ReturnType.GetReferences() |
                        info.InnerMethod.Parameters.Select(x => x.GetReferences()).HashSet2Union();
                }
            }

            public HashSet2<ItemKey> Outputs
            {
                get
                {
                    return HashSet2<ItemKey>.Singleton(info.InnerMethod);
                }
            }

            public void Compile(ModuleBuilder mb, Dictionary<ItemKey, SaBox<object>> vars)
            {
                TypeBuilder t = (TypeBuilder)(vars[info.Owner].Value);
                MethodBuilder m = t.DefineMethod
                (
                    info.InnerMethod.Name.Name,
                    MethodAttributes.Private | MethodAttributes.Static,
                    info.SymbolTable[info.InnerMethod].ReturnType.Resolve(vars),
                    info.InnerMethod.Parameters.Select(x => x.Resolve(vars)).ToArray()
                );
                vars[info.InnerMethod].Value = m;
            }

            #endregion
        }

        private class MakeMethodBody : ICompileStep
        {
            private CompileStepInfo info;

            public MakeMethodBody(CompileStepInfo info)
            {
                this.info = info;
            }

            #region ICompileStep Members

            public int Phase
            {
                get { return 1; }
            }

            public HashSet2<ItemKey> Inputs
            {
                get
                {
                    return HashSet2<ItemKey>.Singleton(info.InnerMethod) |
                        info.Owner |
                        info.Parent.body.GetReferences(info.SymbolTable, info.Owner, info.InnerEnvDesc);
                    ;
                }
            }

            public HashSet2<ItemKey> Outputs
            {
                get { return HashSet2<ItemKey>.Empty; }
            }

            public void Compile(ModuleBuilder mb, Dictionary<ItemKey, SaBox<object>> vars)
            {
                TypeBuilder t = (TypeBuilder)(vars[info.Owner].Value);
                MethodBuilder m = (MethodBuilder)(vars[info.InnerMethod].Value);
                ILGenerator milg = m.GetILGenerator();
                CompileContext2 mcc = new CompileContext2(mb, t, milg, false);

                int iEnd = info.CapturedVars.Length;
                List<Tuple<Symbol, IVarDesc2>> innerVars = new List<Tuple<Symbol, IVarDesc2>>();
                for (int i = 0; i < iEnd; ++i)
                {
                    innerVars.Add(new Tuple<Symbol, IVarDesc2>(info.CapturedVars[i], new ArgVarDesc2(info.InnerEnvDesc[info.CapturedVars[i]], true, i)));
                }

                iEnd = info.Parent.clauses.Count;
                for (int i = 0; i < iEnd; ++i)
                {
                    LocalBuilder lb_pin = milg.DeclareLocal(info.ArrayToPinTypes[i].GetElementType().Resolve(vars).MakeByRefType(), true);
                    bool boxed = false;
                    if (info.CapturedEnvSpec.ContainsKey(info.Parent.clauses[i].Name))
                    {
                        boxed = info.CapturedEnvSpec[info.Parent.clauses[i].Name].IsCaptured;
                    }
                    LocalBuilder lb_ptr = milg.DeclareLocal((boxed ? TypeReference.MakeBoxedType(ExistingTypeReference.IntPtr) : ExistingTypeReference.IntPtr).Resolve(vars));

                    milg.LoadArg(info.Captures.Length + i);
                    milg.LoadInt(0);
                    milg.LoadElementAddress(info.ArrayToPinTypes[i].GetElementType().Resolve(vars));
                    milg.StoreLocal(lb_pin);
                    if (boxed)
                    {
                        milg.NewObj(TypeReference.MakeBoxedType(ExistingTypeReference.IntPtr).Resolve(vars).GetConstructor(Type.EmptyTypes));
                        milg.StoreLocal(lb_ptr);
                    }
                    LocalVarDesc2 lvd = new LocalVarDesc2(ExistingTypeReference.IntPtr, boxed, lb_ptr.LocalIndex);
                    lvd.Store(mcc, vars, delegate() { milg.LoadLocal(lb_pin); }, false);
                    innerVars.Add(new Tuple<Symbol, IVarDesc2>(info.Parent.clauses[i].Name, lvd));
                }

                EnvDesc2 innerEnvDesc = EnvDesc2.FromSequence(innerVars);
                // "body" is not in the tail position, because we don't want the variables to become unpinned until it finishes.
                info.Parent.body.Compile(info.SymbolTable, info.Owner, mcc, innerEnvDesc, vars, false);
                milg.Return();
            }

            #endregion
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            foreach (PinClause2 clause in clauses)
            {
                clause.Value.AddCompileSteps(s, owner, envDesc, add);
            }
            EnvDescTypesOnly2 e2 = MakeInnerEnvDesc(envDesc);
            body.AddCompileSteps(s, owner, e2, add);

            CompileStepInfo info = new CompileStepInfo(this, s, owner, envDesc);
            add(new MakeMethod(info));
            add(new MakeMethodBody(info));

            // TODO
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            HashSet2<ItemKey> references = HashSet2<ItemKey>.Empty;
            foreach (PinClause2 clause in clauses)
            {
                references |= clause.Value.GetReferences(s, owner, envDesc);
            }
            EnvDescTypesOnly2 e2 = MakeInnerEnvDesc(envDesc);
            references |= body.GetReferences(s, owner, e2);

            CompileStepInfo info = new CompileStepInfo(this, s, owner, envDesc);
            references |= info.InnerMethod;

            return references;
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, SaBox<object>> references, bool tail)
        {
            CompileStepInfo info = new CompileStepInfo(this, s, owner, envDesc.TypesOnly());

            ILGenerator ilg = cc.ILGenerator;
            MethodBuilder mb = (MethodBuilder)(references[info.InnerMethod].Value);

            int iEnd = info.CapturedVars.Length;
            for (int i = 0; i < iEnd; ++i)
            {
                envDesc[info.CapturedVars[i]].FetchBox(cc, references, false);
            }

            iEnd = clauses.Count;
            for (int i = 0; i < iEnd; ++i)
            {
                clauses[i].Value.Compile(s, owner, cc, envDesc, references, false);
            }

            if (tail) ilg.Tail();
            ilg.Call(mb);
            if (tail) ilg.Return();
        }

        #endregion
    }

    public class ThrowExpr2 : IExpression2
    {
        private TypeReference typeNotReturned;
        private IExpression2 body;

        public ThrowExpr2(TypeReference typeNotReturned, IExpression2 body)
        {
            this.typeNotReturned = typeNotReturned;
            this.body = body;
        }

        #region IExpression2 Members

        public EnvSpec GetEnvSpec()
        {
            return body.GetEnvSpec();
        }

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            //System.Diagnostics.Debug.Assert(typeof(Exception).IsAssignableFrom(body.GetReturnType(envDesc)));
            return typeNotReturned;
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            body.AddCompileSteps(s, owner, envDesc, add);
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            return body.GetReferences(s, owner, envDesc);
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, SaBox<object>> references, bool tail)
        {
            body.Compile(s, owner, cc, envDesc, references, false);

            cc.ILGenerator.Throw();
        }

        #endregion
    }

    public class CatchClause2
    {
        private TypeReference exceptionType;
        private Symbol exceptionName;
        private IExpression2 body;

        public CatchClause2(TypeReference exceptionType, Symbol exceptionName, IExpression2 body)
        {
            this.exceptionType = exceptionType;
            this.exceptionName = exceptionName;
            this.body = body;
        }

        public TypeReference ExceptionType { get { return exceptionType; } }
        public Symbol ExceptionName { get { return exceptionName; } }
        public IExpression2 Body { get { return body; } }
    }

    public class TryCatchFinallyExpr2 : IExpression2
    {
        private IExpression2 body;
        private List<CatchClause2> catchClauses;
        private IExpression2 finallyClause;
        private Symbol methodName;

        public TryCatchFinallyExpr2(IExpression2 body, IEnumerable<CatchClause2> catchClauses, IExpression2 finallyClause)
        {
            this.body = body;
            this.catchClauses = catchClauses.ToList();
            this.finallyClause = finallyClause;
            this.methodName = new Symbol();
        }

        #region IExpression2 Members

        public EnvSpec GetEnvSpec()
        {
            EnvSpec e = body.GetEnvSpec() | finallyClause.GetEnvSpec() | catchClauses.Select(x => (x.Body.GetEnvSpec() - x.ExceptionName)).EnvSpecUnion();
            return EnvSpec.CaptureAll(e);
        }

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            TypeReference t = body.GetReturnType(s, envDesc);
            bool areValid = catchClauses.All(x => t == x.Body.GetReturnType(s, EnvDescTypesOnly2.Shadow(envDesc, x.ExceptionName, x.ExceptionType)));
            if (!areValid) throw new PascalesqueException("catch clause does not have same return type as body");
            TypeReference f = finallyClause.GetReturnType(s, envDesc);
            if (!(f == ExistingTypeReference.Void)) throw new PascalesqueException("finally clause must have type of void");
            return t;
        }

        private class CompileStepInfo
        {
            private TryCatchFinallyExpr2 parent;
            private SymbolTable symbolTable;
            private TypeKey owner;
            private EnvDescTypesOnly2 envDesc;

            private Lazy<TypeReference> returnType;
            private Lazy<Symbol[]> capturedVars;
            private Lazy<TypeReference[]> captures;
            private Lazy<MethodKey> methodKey;

            public CompileStepInfo(TryCatchFinallyExpr2 parent, SymbolTable symbolTable, TypeKey owner, EnvDescTypesOnly2 envDesc)
            {
                this.parent = parent;
                this.symbolTable = symbolTable;
                this.owner = owner;
                this.envDesc = envDesc;

                this.returnType = new Lazy<TypeReference>(new Func<TypeReference>(GetReturnType), false);
                this.capturedVars = new Lazy<Symbol[]>(new Func<Symbol[]>(GetCapturedVars), false);
                this.captures = new Lazy<TypeReference[]>(new Func<TypeReference[]>(GetCaptures), false);
                this.methodKey = new Lazy<MethodKey>(new Func<MethodKey>(GetMethodKey), false);
            }

            private TypeReference GetReturnType()
            {
                return parent.body.GetReturnType(symbolTable, envDesc);
            }

            private Symbol[] GetCapturedVars()
            {
                return parent.GetEnvSpec().Keys.ToArray();
            }

            private TypeReference[] GetCaptures()
            {
                return CapturedVars.Select(s => TypeReference.MakeBoxedType(envDesc[s])).ToArray();
            }

            private MethodKey GetMethodKey()
            {
                return new MethodKey(owner, parent.methodName, false, Captures);
            }

            public TryCatchFinallyExpr2 Parent { get { return parent; } }

            public SymbolTable SymbolTable { get { return symbolTable; } }

            public TypeKey Owner { get { return owner; } }

            public EnvDescTypesOnly2 EnvDesc { get { return envDesc; } }

            public TypeReference ReturnType { get { return returnType.Value; } }

            public Symbol[] CapturedVars { get { return capturedVars.Value; } }

            public Symbol MethodName { get { return parent.methodName; } }

            public TypeReference[] Captures { get { return captures.Value; } }

            public MethodKey MethodKey { get { return methodKey.Value; } }
        }

        private class MakeMethod : ICompileStep
        {
            private CompileStepInfo info;

            public MakeMethod(CompileStepInfo info)
            {
                this.info = info;
            }

            #region ICompileStep Members

            public int Phase
            {
                get { return 1; }
            }

            public HashSet2<ItemKey> Inputs
            {
                get
                {
                    return info.Owner |
                        info.ReturnType.GetReferences() |
                        info.Captures.Select(x => x.GetReferences()).HashSet2Union();
                }
            }

            public HashSet2<ItemKey> Outputs
            {
                get { return HashSet2<ItemKey>.Singleton(info.MethodKey); }
            }

            public void Compile(ModuleBuilder mb, Dictionary<ItemKey, SaBox<object>> vars)
            {
                TypeBuilder tb = (TypeBuilder)(vars[info.Owner].Value);

                MethodBuilder meb = tb.DefineMethod
                (
                    info.MethodName.ToString(),
                    MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.Final,
                    info.ReturnType.Resolve(vars),
                    info.Captures.Select(x => x.Resolve(vars)).ToArray()
                );

                vars[info.MethodKey].Value = meb;
            }

            #endregion
        }

        private class MakeMethodBody : ICompileStep
        {
            private CompileStepInfo info;

            public MakeMethodBody(CompileStepInfo info)
            {
                this.info = info;
            }

            #region ICompileStep Members

            public int Phase
            {
                get { return 1; }
            }

            public HashSet2<ItemKey> Inputs
            {
                get
                {
                    HashSet2<ItemKey> requirements =
                        info.MethodKey |
                        info.ReturnType.GetReferences() |
                        info.Captures.Select(x => x.GetReferences()).HashSet2Union() |
                        info.Parent.catchClauses.Select(x => x.ExceptionType.GetReferences()).HashSet2Union();

                    requirements |= info.Parent.body.GetReferences(info.SymbolTable, info.Owner, info.EnvDesc);
                    foreach (CatchClause2 x in info.Parent.catchClauses)
                    {
                        EnvDescTypesOnly2 innerEnv2 = EnvDescTypesOnly2.Shadow(info.EnvDesc, x.ExceptionName, x.ExceptionType);
                        requirements |= x.Body.GetReferences(info.SymbolTable, info.Owner, innerEnv2);
                    }
                    requirements |= info.Parent.finallyClause.GetReferences(info.SymbolTable, info.Owner, info.EnvDesc);

                    return requirements;
                }
            }

            public HashSet2<ItemKey> Outputs
            {
                get
                {
                    return HashSet2<ItemKey>.Empty;
                }
            }

            public void Compile(ModuleBuilder mb, Dictionary<ItemKey, SaBox<object>> vars)
            {
                TypeBuilder tb = (TypeBuilder)(vars[info.Owner].Value);

                MethodBuilder meb = (MethodBuilder)(vars[info.MethodKey].Value);

                ILGenerator milg = meb.GetILGenerator();

                CompileContext2 mcc = new CompileContext2(mb, tb, milg, false);

                int iEnd = info.CapturedVars.Length;
                List<Tuple<Symbol, IVarDesc2>> innerVars = new List<Tuple<Symbol, IVarDesc2>>();
                for (int i = 0; i < iEnd; ++i)
                {
                    innerVars.Add(new Tuple<Symbol, IVarDesc2>(info.CapturedVars[i], new ArgVarDesc2(info.EnvDesc[info.CapturedVars[i]], true, i)));
                }

                EnvDesc2 innerEnvDesc = EnvDesc2.FromSequence(innerVars);

                LocalBuilder returnValueLocal = null;
                if (info.ReturnType != ExistingTypeReference.Void)
                {
                    returnValueLocal = milg.DeclareLocal(info.ReturnType.Resolve(vars));
                }
                Label l = milg.BeginExceptionBlock();
                info.Parent.body.Compile(info.SymbolTable, info.Owner, mcc, innerEnvDesc, vars, false);
                if (info.ReturnType != ExistingTypeReference.Void)
                {
                    milg.StoreLocal(returnValueLocal);
                }
                //milg.Leave(l); redundant
                foreach (CatchClause2 catchClause in info.Parent.catchClauses)
                {
                    EnvSpec e2 = catchClause.Body.GetEnvSpec();
                    if (e2.ContainsKey(catchClause.ExceptionName))
                    {
                        VarSpec evc = e2[catchClause.ExceptionName];

                        IVarDesc2 exceptionVarDesc;
                        EnvDesc2 catchEnvDesc;

                        milg.BeginCatchBlock(catchClause.ExceptionType.Resolve(vars));

                        if (evc.IsCaptured)
                        {
                            LocalBuilder lb0 = milg.DeclareLocal(catchClause.ExceptionType.Resolve(vars));
                            milg.StoreLocal(lb0);

                            LocalBuilder lb = milg.DeclareLocal(TypeReference.MakeBoxedType(catchClause.ExceptionType).Resolve(vars));
                            milg.NewObj(TypeReference.MakeBoxedType(catchClause.ExceptionType).Resolve(vars).GetConstructor(Type.EmptyTypes));
                            milg.StoreLocal(lb);
                            exceptionVarDesc = new LocalVarDesc2(catchClause.ExceptionType, true, lb.LocalIndex);
                            catchEnvDesc = EnvDesc2.Shadow(innerEnvDesc, catchClause.ExceptionName, exceptionVarDesc);
                            exceptionVarDesc.Store(mcc, vars, delegate() { milg.LoadLocal(lb0); }, false);
                        }
                        else
                        {
                            LocalBuilder lb = milg.DeclareLocal(catchClause.ExceptionType.Resolve(vars));
                            exceptionVarDesc = new LocalVarDesc2(catchClause.ExceptionType, false, lb.LocalIndex);
                            catchEnvDesc = EnvDesc2.Shadow(innerEnvDesc, catchClause.ExceptionName, exceptionVarDesc);
                            milg.StoreLocal(lb);
                        }

                        catchClause.Body.Compile(info.SymbolTable, info.Owner, mcc, catchEnvDesc, vars, false);
                        if (info.ReturnType != ExistingTypeReference.Void)
                        {
                            milg.StoreLocal(returnValueLocal);
                        }
                        //milg.Leave(l); redundant
                    }
                    else
                    {
                        milg.BeginCatchBlock(catchClause.ExceptionType.Resolve(vars));
                        catchClause.Body.Compile(info.SymbolTable, info.Owner, mcc, innerEnvDesc, vars, false);
                        if (info.ReturnType != ExistingTypeReference.Void)
                        {
                            milg.StoreLocal(returnValueLocal);
                        }
                        //milg.Leave(l); redundant
                    }
                }

                if (!(info.Parent.finallyClause is EmptyExpr2))
                {
                    milg.BeginFinallyBlock();
                    info.Parent.finallyClause.Compile(info.SymbolTable, info.Owner, mcc, innerEnvDesc, vars, false);
                }

                milg.EndExceptionBlock();

                if (info.ReturnType != ExistingTypeReference.Void)
                {
                    milg.LoadLocal(returnValueLocal);
                }
                milg.Return();
            }

            #endregion
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            CompileStepInfo info = new CompileStepInfo(this, s, owner, envDesc);
            add(new MakeMethod(info));
            add(new MakeMethodBody(info));

            body.AddCompileSteps(s, owner, envDesc, add);
            foreach (CatchClause2 cc in catchClauses)
            {
                EnvDescTypesOnly2 inner = EnvDescTypesOnly2.Shadow(envDesc, cc.ExceptionName, cc.ExceptionType);
                cc.Body.AddCompileSteps(s, owner, inner, add);
            }
            finallyClause.AddCompileSteps(s, owner, envDesc, add);
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            CompileStepInfo info = new CompileStepInfo(this, s, owner, envDesc);

            HashSet2<ItemKey> references = HashSet2<ItemKey>.Singleton(info.MethodKey);

            references |= body.GetReferences(s, owner, envDesc);
            foreach (CatchClause2 cc in catchClauses)
            {
                EnvDescTypesOnly2 inner = EnvDescTypesOnly2.Shadow(envDesc, cc.ExceptionName, cc.ExceptionType);
                references |= body.GetReferences(s, owner, inner);
            }
            references |= finallyClause.GetReferences(s, owner, envDesc);

            references |= info.Captures.Select(x => x.GetReferences()).HashSet2Union();

            return references;
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, SaBox<object>> references, bool tail)
        {
            CompileStepInfo info = new CompileStepInfo(this, s, owner, envDesc.TypesOnly());
            MethodBuilder mb = (MethodBuilder)(references[info.MethodKey].Value);
            ILGenerator ilg = cc.ILGenerator;

            int iEnd = info.CapturedVars.Length;
            for (int i = 0; i < iEnd; ++i)
            {
                envDesc[info.CapturedVars[i]].FetchBox(cc, references, false);
            }

            if (tail) ilg.Tail();
            ilg.Call(mb);
            if (tail) ilg.Return();
        }

        #endregion
    }

    public class CastClassExpr2 : IExpression2
    {
        private IExpression2 body;
        private TypeReference toType;

        public CastClassExpr2(TypeReference toType, IExpression2 body)
        {
            this.body = body;
            this.toType = toType;
        }

        #region IExpression2 Members

        public EnvSpec GetEnvSpec()
        {
            return body.GetEnvSpec();
        }

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            TypeReference fromType = body.GetReturnType(s, envDesc);
            if (!(TypeReference.IsAssignable(s, toType, fromType))) throw new PascalesqueException("CastClass won't work with provided types");
            return toType;
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            body.AddCompileSteps(s, owner, envDesc, add);
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            return body.GetReferences(s, owner, envDesc) | toType.GetReferences();
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, ExprObjModel.SaBox<object>> references, bool tail)
        {
            body.Compile(s, owner, cc, envDesc, references, false);
            cc.ILGenerator.CastClass(toType.Resolve(references));
            if (tail) cc.ILGenerator.Return();
        }

        #endregion
    }

    public class RegardAsClassExpr2 : IExpression2
    {
        private IExpression2 body;
        private TypeReference toType;

        public RegardAsClassExpr2(TypeReference toType, IExpression2 body)
        {
            this.body = body;
            this.toType = toType;
        }

        #region IExpression2 Members

        public EnvSpec GetEnvSpec()
        {
            return body.GetEnvSpec();
        }

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            TypeReference fromType = body.GetReturnType(s, envDesc);
            if (!(TypeReference.IsAssignable(s, toType, fromType))) throw new PascalesqueException("RegardAsClass won't work with provided types");
            return toType;
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            body.AddCompileSteps(s, owner, envDesc, add);
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            return body.GetReferences(s, owner, envDesc);
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, ExprObjModel.SaBox<object>> references, bool tail)
        {
            body.Compile(s, owner, cc, envDesc, references, tail);
        }

        #endregion
    }

    public class IsInstanceExpr2 : IExpression2
    {
        private IExpression2 body;
        private TypeReference toType;

        public IsInstanceExpr2(IExpression2 body, TypeReference toType)
        {
            this.body = body;
            this.toType = toType;
        }

        #region IExpression Members

        public EnvSpec GetEnvSpec()
        {
            return body.GetEnvSpec();
        }

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            TypeReference fromType = body.GetReturnType(s, envDesc);

            // questionable logic
            if (TypeReference.IsAssignable(s, toType, fromType)) throw new PascalesqueException("IsInstance would always return true with provided types");
            if (!(TypeReference.IsAssignable(s, toType, fromType))) throw new PascalesqueException("IsInstance would always return false with provided types");

            return ExistingTypeReference.Boolean;
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            body.AddCompileSteps(s, owner, envDesc, add);
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            return body.GetReferences(s, owner, envDesc) | toType.GetReferences();
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, ExprObjModel.SaBox<object>> references, bool tail)
        {
            body.Compile(s, owner, cc, envDesc, references, false);
            cc.ILGenerator.IsInst(toType.Resolve(references));
            if (tail) cc.ILGenerator.Return();
        }

        #endregion
    }

    public class IsNullExpr2 : IExpression2
    {
        private IExpression2 body;

        public IsNullExpr2(IExpression2 body)
        {
            this.body = body;
        }

        public EnvSpec GetEnvSpec()
        {
            return body.GetEnvSpec();
        }

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            TypeReference fromType = body.GetReturnType(s, envDesc);
            if (fromType.IsValueType(s)) throw new PascalesqueException("IsNull used on an expression of value type");
            return ExistingTypeReference.Boolean;
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            body.AddCompileSteps(s, owner, envDesc, add);
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            return body.GetReferences(s, owner, envDesc);
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, ExprObjModel.SaBox<object>> references, bool tail)
        {
            body.Compile(s, owner, cc, envDesc, references, false);
            Label l0 = cc.ILGenerator.DefineLabel();
            Label l1 = cc.ILGenerator.DefineLabel();
            cc.ILGenerator.Emit(OpCodes.Brfalse_S, l0);
            cc.ILGenerator.LoadInt(0);
            cc.ILGenerator.Emit(OpCodes.Br_S, l1);
            cc.ILGenerator.MarkLabel(l0);
            cc.ILGenerator.LoadInt(1);
            cc.ILGenerator.MarkLabel(l1);
            if (tail) cc.ILGenerator.Return();
        }
    }

    public class BoxExpr2 : IExpression2
    {
        private IExpression2 valueToBox;

        public BoxExpr2(IExpression2 valueToBox)
        {
            this.valueToBox = valueToBox;
        }

        #region IExpression Members

        public EnvSpec GetEnvSpec()
        {
            return valueToBox.GetEnvSpec();
        }

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            TypeReference t = valueToBox.GetReturnType(s, envDesc);
            if (!(t.IsValueType(s))) throw new PascalesqueException("Attempt to box a non-value type " + t);

            return ExistingTypeReference.Object;
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            valueToBox.AddCompileSteps(s, owner, envDesc, add);
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            TypeReference t = valueToBox.GetReturnType(s, envDesc);
            return valueToBox.GetReferences(s, owner, envDesc) | t.GetReferences();
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, ExprObjModel.SaBox<object>> references, bool tail)
        {
            TypeReference t = valueToBox.GetReturnType(s, envDesc.TypesOnly());

            valueToBox.Compile(s, owner, cc, envDesc, references, false);
            cc.ILGenerator.Box(t.Resolve(references));
            if (tail) cc.ILGenerator.Return();
        }

        #endregion
    }

    public class UnboxExpr2 : IExpression2
    {
        private IExpression2 valueToUnbox;
        private TypeReference t;

        public UnboxExpr2(IExpression2 valueToUnbox, TypeReference t)
        {
            this.valueToUnbox = valueToUnbox;
            this.t = t;
        }

        #region IExpression Members

        public EnvSpec GetEnvSpec()
        {
            return valueToUnbox.GetEnvSpec();
        }

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            TypeReference u = valueToUnbox.GetReturnType(s, envDesc);
            if (u != ExistingTypeReference.Object) throw new PascalesqueException("Attempt to unbox a non-object of type " + t);

            return t;
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            valueToUnbox.AddCompileSteps(s, owner, envDesc, add);
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            return valueToUnbox.GetReferences(s, owner, envDesc) | t.GetReferences();
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, ExprObjModel.SaBox<object>> references, bool tail)
        {
            valueToUnbox.Compile(s, owner, cc, envDesc, references, false);
            Type tt = t.Resolve(references);
            cc.ILGenerator.Unbox(tt);
            cc.ILGenerator.LoadObjIndirect(tt);
            if (tail) cc.ILGenerator.Return();
        }

        #endregion
    }

    public class TupleItemExpr2 : IExpression2
    {
        private IExpression2 tupleValue;
        private int index;

        public TupleItemExpr2(IExpression2 tupleValue, int index)
        {
            this.tupleValue = tupleValue;
            this.index = index;
        }

        public EnvSpec GetEnvSpec()
        {
            return tupleValue.GetEnvSpec();
        }

#if false
        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            TypeReference t = tupleValue.GetReturnType(s, envDesc);
            if (!Utils.IsTupleType(t)) throw new PascalesqueException("TupleFirstExpr requires a tuple");
            return Utils.GetTupleProperty(t, index).PropertyType;
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {
            tupleValue.Compile(cc, envDesc, false);
            MethodInfo m = Utils.GetTupleProperty(tupleValue.GetReturnType(envDesc.TypesOnly()), index).GetGetMethod();
            if (tail) cc.ILGenerator.Tail();
            cc.ILGenerator.Call(m);
            if (tail) cc.ILGenerator.Return();
        }
#endif

        public TypeReference GetReturnType(SymbolTable s, EnvDescTypesOnly2 envDesc)
        {
            throw new NotImplementedException();
        }

        public void AddCompileSteps(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc, Action<ICompileStep> add)
        {
            throw new NotImplementedException();
        }

        public HashSet2<ItemKey> GetReferences(SymbolTable s, TypeKey owner, EnvDescTypesOnly2 envDesc)
        {
            throw new NotImplementedException();
        }

        public void Compile(SymbolTable s, TypeKey owner, CompileContext2 cc, EnvDesc2 envDesc, Dictionary<ItemKey, SaBox<object>> references, bool tail)
        {
            throw new NotImplementedException();
        }
    }
}
