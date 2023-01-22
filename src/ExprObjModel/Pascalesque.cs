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

namespace Pascalesque.One
{
    public class EnvDescTypesOnly
    {
        private Dictionary<Symbol, Type> data;

        private EnvDescTypesOnly()
        {
            data = new Dictionary<Symbol,Type>();
        }

        private EnvDescTypesOnly(Symbol s, Type t)
        {
            data = new Dictionary<Symbol, Type>();
            data.Add(s, t);
        }

        private EnvDescTypesOnly(EnvDescTypesOnly src)
        {
            data = new Dictionary<Symbol, Type>();
            foreach (KeyValuePair<Symbol, Type> kvp in src.data)
            {
                data.Add(kvp.Key, kvp.Value);
            }
        }

        public static EnvDescTypesOnly Empty()
        {
            return new EnvDescTypesOnly();
        }

        public static EnvDescTypesOnly Singleton(Symbol s, Type t)
        {
            return new EnvDescTypesOnly(s, t);
        }

        [Obsolete]
        public static EnvDescTypesOnly FromSequence(IEnumerable<Tuple<Symbol, Type>> seq)
        {
            EnvDescTypesOnly e = new EnvDescTypesOnly();
            foreach (Tuple<Symbol, Type> t in seq)
            {
                e.data.Add(t.Item1, t.Item2);
            }
            return e;
        }

        public static EnvDescTypesOnly FromSequence(IEnumerable<ParamInfo> seq)
        {
            EnvDescTypesOnly e = new EnvDescTypesOnly();
            foreach (ParamInfo t in seq)
            {
                e.data.Add(t.Name, t.ParamType);
            }
            return e;
        }

        public static EnvDescTypesOnly Shadow(EnvDescTypesOnly e, Symbol s, Type t)
        {
            EnvDescTypesOnly r = new EnvDescTypesOnly(e);
            if (r.data.ContainsKey(s)) r.data.Remove(s);
            r.data.Add(s, t);
            return r;
        }

        [Obsolete]
        public static EnvDescTypesOnly Shadow(EnvDescTypesOnly i, IEnumerable<Tuple<Symbol, Type>> symbols)
        {
            EnvDescTypesOnly result = new EnvDescTypesOnly();
            foreach (KeyValuePair<Symbol, Type> kvp in i.data)
            {
                result.data.Add(kvp.Key, kvp.Value);
            }
            foreach (Tuple<Symbol, Type> t in symbols)
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

        public static EnvDescTypesOnly Shadow(EnvDescTypesOnly i, IEnumerable<ParamInfo> symbols)
        {
            EnvDescTypesOnly result = new EnvDescTypesOnly();
            foreach (KeyValuePair<Symbol, Type> kvp in i.data)
            {
                result.data.Add(kvp.Key, kvp.Value);
            }
            foreach (ParamInfo t in symbols)
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

        public Type this[Symbol s]
        {
            get
            {
                return data[s];
            }
        }

    }

    public interface ICompileContext
    {
        ModuleBuilder ModuleBuilder { get; }
        TypeBuilder TypeBuilder { get; }
        ILGenerator ILGenerator { get; }

        ICompileContext NewContext(TypeBuilder newTyb, ILGenerator newIlg);
        ICompileContext NewContext(ILGenerator newIlg);

        Type MakeBoxedType(Type t);
        bool IsBoxedType(Type t);
        Type BoxContentType(Type t);
    }

    public interface IVarDesc
    {
        Type VarType { get; }
        bool IsBoxed { get; }
        void Fetch(ICompileContext cc, bool tail);
        void Store(ICompileContext cc, Action exprToStore, bool tail);
        void FetchBox(ICompileContext cc, bool tail);
    }

    public class LocalVarDesc : IVarDesc
    {
        private Type varType;
        private bool isBoxed;
        private int index;

        public LocalVarDesc(Type varType, bool isBoxed, int localIndex)
        {
            this.varType = varType;
            this.isBoxed = isBoxed;
            this.index = localIndex;
        }

        public int LocalIndex { get { return index; } }

        #region IVarDesc Members

        public Type VarType { get { return varType; } }

        public bool IsBoxed { get { return isBoxed; } }

        public void Fetch(ICompileContext cc, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            ilg.LoadLocal(index);
            if (isBoxed)
            {
                if (tail) ilg.Tail();
                ilg.Call(cc.MakeBoxedType(varType).GetProperty("Value").GetGetMethod());
                if (tail) ilg.Return();
            }
            else
            {
                if (tail) ilg.Return();
            }
        }

        public void Store(ICompileContext cc, Action writeExprToStore, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            if (isBoxed)
            {
                ilg.LoadLocal(index);
                writeExprToStore();
                if (tail) ilg.Tail();
                ilg.Call(cc.MakeBoxedType(varType).GetProperty("Value").GetSetMethod());
                if (tail) ilg.Return();
            }
            else
            {
                writeExprToStore();
                ilg.StoreLocal(index);
                if (tail) ilg.Return();
            }
        }

        public void FetchBox(ICompileContext cc, bool tail)
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

    public class ArgVarDesc : IVarDesc
    {
        private Type varType;
        private bool isBoxed;
        private int index;

        public ArgVarDesc(Type varType, bool isBoxed, int index)
        {
            this.varType = varType;
            this.isBoxed = isBoxed;
            this.index = index;
        }

        #region IVarDesc Members

        public Type VarType { get { return varType; } }

        public bool IsBoxed { get { return isBoxed; } }

        public void Fetch(ICompileContext cc, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            ilg.LoadArg(index);
            if (isBoxed)
            {
                if (tail) ilg.Tail();
                ilg.Call(cc.MakeBoxedType(varType).GetProperty("Value").GetGetMethod());
                if (tail) ilg.Return();
            }
            else
            {
                if (tail) ilg.Return();
            }
        }

        public void Store(ICompileContext cc, Action writeExprToStore, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            if (isBoxed)
            {
                ilg.LoadArg(index);
                writeExprToStore();
                if (tail) ilg.Tail();
                ilg.Call(cc.MakeBoxedType(varType).GetProperty("Value").GetSetMethod());
                if (tail) ilg.Return();
            }
            else
            {
                writeExprToStore();
                ilg.StoreArg(index);
                if (tail) ilg.Return();
            }
        }

        public void FetchBox(ICompileContext cc, bool tail)
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

    public class FieldVarDesc : IVarDesc
    {
        private IVarDesc fieldOfWhat;
        private FieldInfo fieldInfo;
        private Type varType;
        private bool isBoxed;

        public FieldVarDesc(IVarDesc fieldOfWhat, FieldInfo fieldInfo, Type varType, bool isBoxed)
        {
            this.fieldOfWhat = fieldOfWhat;
            this.fieldInfo = fieldInfo;
            this.varType = varType;
            this.isBoxed = isBoxed;
        }

        #region IVarDesc Members

        public Type VarType
        {
            get
            {
                return varType;
            }
        }

        public bool IsBoxed { get { return isBoxed; } }

        public void Fetch(ICompileContext cc, bool tail)
        {
            if (isBoxed && fieldInfo.FieldType != cc.MakeBoxedType(varType)) throw new PascalesqueException("Field type isn't boxed type");
            ILGenerator ilg = cc.ILGenerator;

            fieldOfWhat.Fetch(cc, false);
            ilg.LoadField(fieldInfo);
            if (tail) ilg.Tail();
            ilg.Call(fieldInfo.FieldType.GetProperty("Value").GetGetMethod());
            if (tail) ilg.Return();
        }

        public void Store(ICompileContext cc, Action writeExprToStore, bool tail)
        {
            if (isBoxed && fieldInfo.FieldType != cc.MakeBoxedType(varType)) throw new PascalesqueException("Field type isn't boxed type");

            ILGenerator ilg = cc.ILGenerator;

            fieldOfWhat.Fetch(cc, false);
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

        public void FetchBox(ICompileContext cc, bool tail)
        {
            if (isBoxed && fieldInfo.FieldType != cc.MakeBoxedType(varType)) throw new PascalesqueException("Field type isn't boxed type");

            ILGenerator ilg = cc.ILGenerator;

            if (IsBoxed)
            {
                fieldOfWhat.Fetch(cc, false);
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

    public class EnvDesc
    {
        private Dictionary<Symbol, IVarDesc> data;

        private EnvDesc()
        {
            data = new Dictionary<Symbol, IVarDesc>();
        }

        private EnvDesc(Symbol s, IVarDesc v)
        {
            data = new Dictionary<Symbol, IVarDesc>();
            data.Add(s, v);
        }

        private EnvDesc(EnvDesc src)
        {
            data = new Dictionary<Symbol, IVarDesc>();
            foreach (KeyValuePair<Symbol, IVarDesc> kvp in src.data)
            {
                data.Add(kvp.Key, kvp.Value);
            }
        }

        public static EnvDesc Empty()
        {
            return new EnvDesc();
        }

        public static EnvDesc Singleton(Symbol s, IVarDesc v)
        {
            return new EnvDesc(s, v);
        }

        public static EnvDesc FromSequence(IEnumerable<Tuple<Symbol, IVarDesc>> seq)
        {
            EnvDesc e = new EnvDesc();
            foreach (Tuple<Symbol, IVarDesc> t in seq)
            {
                e.data.Add(t.Item1, t.Item2);
            }
            return e;
        }

        public static EnvDesc Shadow(EnvDesc e, Symbol s, IVarDesc v)
        {
            EnvDesc r = new EnvDesc(e);
            if (r.data.ContainsKey(s)) r.data.Remove(s);
            r.data.Add(s, v);
            return r;
        }

        public static EnvDesc Shadow(EnvDesc a, IEnumerable<Tuple<Symbol, IVarDesc>> symbols)
        {
            EnvDesc r = new EnvDesc(a);
            foreach (Tuple<Symbol, IVarDesc> t in symbols)
            {
                if (r.data.ContainsKey(t.Item1)) r.data.Remove(t.Item1);
                r.data.Add(t.Item1, t.Item2);
            }
            return r;
        }

        public EnvDescTypesOnly TypesOnly()
        {
            return EnvDescTypesOnly.FromSequence(data.Select(x => new ParamInfo(x.Key, x.Value.VarType)));
        }

        public bool ContainsKey(Symbol s) { return data.ContainsKey(s); }

        public HashSet<Symbol> Keys
        {
            get
            {
                return ExprObjModel.Utils.ToHashSet(data.Keys);
            }
        }

        public IVarDesc this[Symbol s]
        {
            get
            {
                return data[s];
            }
        }
    }

    public interface IExpression
    {
        EnvSpec GetEnvSpec();
        Type GetReturnType(EnvDescTypesOnly envDesc);
        void Compile(ICompileContext cc, EnvDesc envDesc, bool tail);
    }

    public class LiteralExpr : IExpression
    {
        private object val;

        public LiteralExpr(object val)
        {
            this.val = val;
        }

        #region IExpression Members

        public EnvSpec GetEnvSpec()
        {
            return EnvSpec.Empty();
        }

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            return val.GetType();
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
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
                ilg.LoadInt((int)(byte)val2);
            }
            else if (val2.GetType() == typeof(sbyte))
            {
                ilg.LoadInt((int)(sbyte)val2);
            }
            else if (val2.GetType() == typeof(short))
            {
                ilg.LoadInt((int)(short)val2);
            }
            else if (val2.GetType() == typeof(ushort))
            {
                ilg.LoadInt((int)(ushort)val2);
            }
            else if (val2.GetType() == typeof(int))
            {
                ilg.LoadInt((int)val2);
            }
            else if (val2.GetType() == typeof(uint))
            {
                ilg.LoadInt((int)(uint)val2);
            }
            else if (val2.GetType() == typeof(IntPtr))
            {
                ilg.LoadLong((long)(IntPtr)val2);
                ilg.Conv_I();
            }
            else if (val2.GetType() == typeof(UIntPtr))
            {
                ilg.LoadLong((long)(ulong)(UIntPtr)val2);
                ilg.Conv_U();
            }
            else if (val2.GetType() == typeof(long))
            {
                ilg.LoadLong((long)val2);
            }
            else if (val2.GetType() == typeof(ulong))
            {
                ilg.LoadLong((long)(ulong)val2);
            }
            else if (val2.GetType() == typeof(bool))
            {
                ilg.LoadInt(((bool)val2) ? 1 : 0);
            }
            else if (val2.GetType() == typeof(float))
            {
                ilg.LoadFloat((float)val2);
            }
            else if (val2.GetType() == typeof(double))
            {
                ilg.LoadDouble((double)val2);
            }
            else if (val2.GetType() == typeof(char))
            {
                ilg.LoadInt((int)(char)val2);
            }
            else if (val2.GetType() == typeof(string))
            {
                ilg.LoadString((string)val2);
            }
            else
            {
                throw new PascalesqueException("Literal of unsupported type");
            }

            if (tail) ilg.Return();
        }

        #endregion
    }

    public class VarRefExpr : IExpression
    {
        private Symbol name;

        public VarRefExpr(Symbol name)
        {
            this.name = name;
        }

        #region IExpression Members

        public EnvSpec GetEnvSpec()
        {
            return EnvSpec.Singleton(name, new VarSpec(false, false));
        }

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            return envDesc[name];
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {
            envDesc[name].Fetch(cc, tail);
        }

        #endregion
    }

    public class VarSetExpr : IExpression
    {
        private Symbol name;
        private IExpression val;

        public VarSetExpr(Symbol name, IExpression val)
        {
            this.name = name;
            this.val = val;
        }

        #region IExpression Members

        public EnvSpec GetEnvSpec()
        {
            EnvSpec es = val.GetEnvSpec();
            return EnvSpec.Add(es, name, new VarSpec(true, false));
        }

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            if (envDesc[name] != val.GetReturnType(envDesc)) throw new PascalesqueException("Type mismatch in VarSet");
            return typeof(void);
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {
            if (envDesc[name].VarType != val.GetReturnType(envDesc.TypesOnly())) throw new PascalesqueException("Type mismatch in VarSet");

            envDesc[name].Store(cc, delegate() { val.Compile(cc, envDesc, false); }, tail);
        }

        #endregion
    }

    public class BeginExpr : IExpression
    {
        private List<IExpression> body;

        public BeginExpr(IEnumerable<IExpression> body)
        {
            this.body = body.ToList();
        }

        #region IExpression Members

        public EnvSpec GetEnvSpec()
        {
            EnvSpec e = EnvSpec.Empty();

            foreach (IExpression expr in body)
            {
                e |= expr.GetEnvSpec();
            }

            return e;
        }

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            return body[body.Count - 1].GetReturnType(envDesc);
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            int iEnd = body.Count;
            EnvDescTypesOnly edto = envDesc.TypesOnly();

            for(int i = 0; i < iEnd; ++i)
            {
                bool isLast = (i + 1 == iEnd);

                body[i].Compile(cc, envDesc, tail && isLast);
                if (!isLast && body[i].GetReturnType(edto) != typeof(void))
                {
                    ilg.Pop();
                }
            }

            if (tail) ilg.Return();
        }

        #endregion

        public static IExpression FromList(IEnumerable<IExpression> exprs)
        {
            List<IExpression> l = exprs.ToList();
            if (l.Count == 0)
            {
                return new EmptyExpr();
            }
            else if (l.Count == 1)
            {
                return l[0];
            }
            else
            {
                return new BeginExpr(l);
            }
        }
    }

    public class EmptyExpr : IExpression
    {
        public EmptyExpr() { }

        #region IExpression Members

        public EnvSpec GetEnvSpec()
        {
            return EnvSpec.Empty();
        }

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            return typeof(void);
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {
            if (tail) cc.ILGenerator.Return();
        }

        #endregion
    }

    public class AndExpr : IExpression
    {
        private List<IExpression> body;

        public AndExpr(IEnumerable<IExpression> body)
        {
            this.body = body.ToList();
        }

        #region IExpression Members

        public EnvSpec GetEnvSpec()
        {
            EnvSpec e = EnvSpec.Empty();

            foreach (IExpression expr in body)
            {
                e |= expr.GetEnvSpec();
            }

            return e;
        }

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            foreach (IExpression e in body)
            {
                if (e.GetReturnType(envDesc) != typeof(bool)) throw new PascalesqueException("Elements in an \"and\" must be boolean");
            }
            return typeof(bool);
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            int iEnd = body.Count;
            EnvDescTypesOnly edto = envDesc.TypesOnly();

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

                    body[i].Compile(cc, envDesc, tail && isLast);
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

    public class OrExpr : IExpression
    {
        private List<IExpression> body;

        public OrExpr(IEnumerable<IExpression> body)
        {
            this.body = body.ToList();
        }

        #region IExpression Members

        public EnvSpec GetEnvSpec()
        {
            EnvSpec e = EnvSpec.Empty();

            foreach (IExpression expr in body)
            {
                e |= expr.GetEnvSpec();
            }

            return e;
        }

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            foreach (IExpression e in body)
            {
                if (e.GetReturnType(envDesc) != typeof(bool)) throw new PascalesqueException("Elements in an \"and\" must be boolean");
            }
            return typeof(bool);
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            int iEnd = body.Count;
            EnvDescTypesOnly edto = envDesc.TypesOnly();

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

                    body[i].Compile(cc, envDesc, tail && isLast);
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

    public class IfThenElseExpr : IExpression
    {
        private IExpression condition;
        private IExpression consequent;
        private IExpression alternate;

        public IfThenElseExpr(IExpression condition, IExpression consequent, IExpression alternate)
        {
            this.condition = condition;
            this.consequent = consequent;
            this.alternate = alternate;
        }

        #region IExpression Members

        public EnvSpec GetEnvSpec()
        {
            EnvSpec e = condition.GetEnvSpec() | consequent.GetEnvSpec() | alternate.GetEnvSpec();
            return e;
        }

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            if (condition.GetReturnType(envDesc) != typeof(bool)) throw new PascalesqueException("type of condition must be bool");
            if (consequent.GetReturnType(envDesc) != alternate.GetReturnType(envDesc)) throw new PascalesqueException("type of consequent and alternate must match");

            return consequent.GetReturnType(envDesc);
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            Label one = ilg.DefineLabel();
            Label two = ilg.DefineLabel();

            condition.Compile(cc, envDesc, false);
            ilg.Emit(OpCodes.Brfalse, one);
            consequent.Compile(cc, envDesc, tail);
            if (!tail) ilg.Emit(OpCodes.Br, two);
            ilg.MarkLabel(one);
            alternate.Compile(cc, envDesc, tail);
            ilg.MarkLabel(two);
        }

        #endregion
    }

    public class SwitchExpr : IExpression
    {
        private IExpression switchOnWhat;
        private IExpression defaultExpr;
        private List<Tuple<HashSet<uint>, IExpression>> targetExprs;

        public SwitchExpr(IExpression switchOnWhat, IExpression defaultExpr, IEnumerable<Tuple<IEnumerable<uint>, IExpression>> targetExprs)
        {
            this.switchOnWhat = switchOnWhat;
            this.defaultExpr = defaultExpr;
            this.targetExprs = targetExprs.Select(x => new Tuple<HashSet<uint>, IExpression>(x.Item1.ToHashSet(), x.Item2)).ToList();
        }

        public EnvSpec GetEnvSpec()
        {
            EnvSpec e = switchOnWhat.GetEnvSpec() | defaultExpr.GetEnvSpec();
            foreach (Tuple<HashSet<uint>, IExpression> kvp in targetExprs)
            {
                e |= kvp.Item2.GetEnvSpec();
            }
            return e;
        }

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            if (switchOnWhat.GetReturnType(envDesc) != typeof(uint)) throw new PascalesqueException("SwitchExpr: SwitchOnWhat must be of type uint");

            uint max = targetExprs.Select(x => x.Item1.Max()).Max();
            if (max > 255u) throw new PascalesqueException("Switch has more than 256 destinations");

            bool[] b1 = new bool[max + 1];

            Type t = defaultExpr.GetReturnType(envDesc);
            foreach (Tuple<HashSet<uint>, IExpression> kvp in targetExprs)
            {
                foreach (uint u in kvp.Item1)
                {
                    if (b1[(int)u]) throw new PascalesqueException("Switch error: A value can go to only one expression");
                    b1[(int)u] = true;
                }

                Type t2 = kvp.Item2.GetReturnType(envDesc);
                if (t != t2) throw new PascalesqueException("SwitchExpr: All alternatives must be of the same type");
            }

            return t;
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {
            List<Label> labelList = new List<Label>();
            foreach (Tuple<HashSet<uint>, IExpression> item in targetExprs)
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
                Tuple<HashSet<uint>, IExpression> item = targetExprs[i];
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

            switchOnWhat.Compile(cc, envDesc, false);

            Label? lEnd = tail ? (Label?)null : (Label?)(cc.ILGenerator.DefineLabel());

            cc.ILGenerator.Emit(OpCodes.Switch, larr);

            cc.ILGenerator.MarkLabel(lDefault);
            defaultExpr.Compile(cc, envDesc, tail);

            for (int i = 0; i < iEnd; ++i)
            {
                if (!tail) cc.ILGenerator.Emit(OpCodes.Br, lEnd.Value);
                cc.ILGenerator.MarkLabel(labelList[i]);
                targetExprs[i].Item2.Compile(cc, envDesc, tail);
            }

            if (!tail) cc.ILGenerator.MarkLabel(lEnd.Value);
        }
    }

    public class BeginWhileRepeatExpr : IExpression
    {
        private IExpression body1;
        private IExpression condition;
        private IExpression body2;

        public BeginWhileRepeatExpr(IExpression body1, IExpression condition, IExpression body2)
        {
            this.body1 = body1;
            this.condition = condition;
            this.body2 = body2;
        }

        #region IExpression Members

        public EnvSpec GetEnvSpec()
        {
            EnvSpec e = body1.GetEnvSpec() | condition.GetEnvSpec() | body2.GetEnvSpec();
            return e;
        }

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            if (condition.GetReturnType(envDesc) != typeof(bool)) throw new PascalesqueException("type of condition must be bool");

            return typeof(void);
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            Label one = ilg.DefineLabel();
            Label two = ilg.DefineLabel();

            ilg.MarkLabel(one);
            body1.Compile(cc, envDesc, false);
            if (body1.GetReturnType(envDesc.TypesOnly()) != typeof(void)) ilg.Pop();
            condition.Compile(cc, envDesc, false);
            if (condition.GetReturnType(envDesc.TypesOnly()) != typeof(bool)) throw new PascalesqueException("type of condition must be bool");
            ilg.Emit(OpCodes.Brfalse, two);
            body2.Compile(cc, envDesc, false);
            if (body2.GetReturnType(envDesc.TypesOnly()) != typeof(void)) ilg.Pop();
            ilg.Emit(OpCodes.Br, one);
            ilg.MarkLabel(two);
            if (tail) ilg.Return();
        }

        #endregion
    }

    public class LetClause
    {
        private Symbol name;
        private Type varType;
        private IExpression val;

        public LetClause(Symbol name, Type varType, IExpression val)
        {
            this.name = name;
            this.varType = varType;
            this.val = val;
        }

        public Symbol Name { get { return name; } }
        public Type VarType { get { return varType; } }
        public IExpression Value { get { return val; } }
    }

    public class LetExpr : IExpression
    {
        private List<LetClause> clauses;
        private IExpression body;

        public LetExpr(IEnumerable<LetClause> clauses, IExpression body)
        {
            this.clauses = clauses.ToList();
            this.body = body;

            if (clauses.Select(x => x.Name).HasDuplicates()) throw new PascalesqueException("Duplicate variables in letrec");
        }

        #region IExpression Members

        public EnvSpec GetEnvSpec()
        {
            EnvSpec e = body.GetEnvSpec() - clauses.Select(x => x.Name);
            foreach (LetClause lc in clauses)
            {
                e |= lc.Value.GetEnvSpec();
            }
            return e;
        }

        private EnvDescTypesOnly MakeInnerEnvDesc(EnvDescTypesOnly outerEnvDesc)
        {
            return EnvDescTypesOnly.Shadow(outerEnvDesc, clauses.Select(x => new ParamInfo(x.Name, x.VarType)));
        }

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            if (clauses.Select(x => x.Name).HasDuplicates()) throw new PascalesqueException("let has two variables with the same name");
            if (clauses.Any(x => x.VarType != x.Value.GetReturnType(envDesc))) throw new PascalesqueException("a variable's type does not match that of its initializer");

            EnvDescTypesOnly e2 = MakeInnerEnvDesc(envDesc);

            return body.GetReturnType(e2);
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            EnvSpec e = body.GetEnvSpec();

            List<Tuple<Symbol, IVarDesc>> theList = new List<Tuple<Symbol, IVarDesc>>();

            foreach (LetClause l in clauses)
            {
                bool boxed = false;
                if (e.ContainsKey(l.Name))
                {
                    boxed = e[l.Name].IsCaptured;
                }

                LocalBuilder lb = ilg.DeclareLocal(boxed ? cc.MakeBoxedType(l.VarType) : l.VarType);
                IVarDesc varDesc = new LocalVarDesc(l.VarType, boxed, lb.LocalIndex);

                theList.Add(new Tuple<Symbol, IVarDesc>(l.Name, varDesc));
                if (boxed)
                {
                    ilg.NewObj(cc.MakeBoxedType(l.VarType).GetConstructor(Type.EmptyTypes));
                    ilg.StoreLocal(lb);
                }
                varDesc.Store(cc, delegate() { l.Value.Compile(cc, envDesc, false); }, false);
            }

            EnvDesc innerEnvDesc = EnvDesc.Shadow(envDesc, theList);

            body.Compile(cc, innerEnvDesc, tail);
        }

        #endregion
    }

    public class LetStarExpr : IExpression
    {
        private List<LetClause> clauses;
        private IExpression body;

        public LetStarExpr(IEnumerable<LetClause> clauses, IExpression body)
        {
            this.clauses = clauses.ToList();
            this.body = body;
        }

        #region IExpression Members

        private EnvSpec GetEnvSpec(int j)
        {
            EnvSpec e = body.GetEnvSpec();
            int i = clauses.Count;
            while (i > j)
            {
                --i;
                LetClause lc = clauses[i];
                e -= lc.Name;
                e |= lc.Value.GetEnvSpec();
            }
            return e;
        }

        public EnvSpec GetEnvSpec()
        {
            return GetEnvSpec(0);
        }

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            EnvDescTypesOnly e2 = envDesc;

            foreach (LetClause lc in clauses)
            {
                if (lc.Value.GetReturnType(e2) != lc.VarType) throw new PascalesqueException("a variable's type does not match that of its initializer");
                e2 = EnvDescTypesOnly.Shadow(e2, lc.Name, lc.VarType);
            }

            return body.GetReturnType(e2);
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            EnvDesc e2 = envDesc;
            int iEnd = clauses.Count;
            for (int i = 0; i < iEnd; ++i)
            {
                LetClause l = clauses[i];
                EnvSpec e = GetEnvSpec(i + 1);
                bool boxed = false;
                if (e.ContainsKey(l.Name))
                {
                    boxed = e[l.Name].IsCaptured;
                }

                LocalBuilder lb = ilg.DeclareLocal(boxed ? cc.MakeBoxedType(l.VarType) : l.VarType);
                IVarDesc varDesc = new LocalVarDesc(l.VarType, boxed, lb.LocalIndex);

                if (boxed)
                {
                    ilg.NewObj(cc.MakeBoxedType(l.VarType).GetConstructor(Type.EmptyTypes));
                    ilg.StoreLocal(lb);
                }
                varDesc.Store(cc, delegate() { l.Value.Compile(cc, e2, false); }, false);

                e2 = EnvDesc.Shadow(e2, l.Name, varDesc);
            }

            body.Compile(cc, e2, tail);
        }

        #endregion
    }

    public class LetRecExpr : IExpression
    {
        private List<LetClause> clauses;
        private IExpression body;

        public LetRecExpr(IEnumerable<LetClause> clauses, IExpression body)
        {
            this.clauses = clauses.ToList();
            this.body = body;

            if (clauses.Select(x => x.Name).HasDuplicates()) throw new PascalesqueException("Duplicate variables in letrec");
        }

        #region IExpression Members

        public EnvSpec GetEnvSpec()
        {
            EnvSpec e = body.GetEnvSpec();
            foreach (LetClause lc in clauses)
            {
                e |= lc.Value.GetEnvSpec();
            }
            return e - clauses.Select(x => x.Name);
        }

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            EnvDescTypesOnly innerEnvDesc = EnvDescTypesOnly.Shadow(envDesc, clauses.Select(x => new ParamInfo(x.Name, x.VarType)));
            return body.GetReturnType(innerEnvDesc);
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            List<Tuple<Symbol, IVarDesc>> theList = new List<Tuple<Symbol, IVarDesc>>();

            foreach (LetClause l in clauses)
            {
                LocalBuilder lb = ilg.DeclareLocal(cc.MakeBoxedType(l.VarType));
                IVarDesc varDesc = new LocalVarDesc(l.VarType, true, lb.LocalIndex);

                theList.Add(new Tuple<Symbol, IVarDesc>(l.Name, varDesc));
                ilg.NewObj(cc.MakeBoxedType(l.VarType).GetConstructor(Type.EmptyTypes));
                ilg.StoreLocal(lb);
            }

            EnvDesc innerEnvDesc = EnvDesc.Shadow(envDesc, theList);

            int iEnd = theList.Count;
            for (int i = 0; i < iEnd; ++i)
            {
                IVarDesc varDesc = theList[i].Item2;
                varDesc.Store(cc, delegate() { clauses[i].Value.Compile(cc, innerEnvDesc, false); }, false);
            }

            body.Compile(cc, innerEnvDesc, tail);
        }

        #endregion
    }

    public class LetLoopExpr : IExpression
    {
        private Symbol loopName;
        private Type loopReturnType;
        private List<LetClause> clauses;
        private IExpression body;

        public LetLoopExpr(Symbol loopName, Type loopReturnType, IEnumerable<LetClause> clauses, IExpression body)
        {
            this.loopName = loopName;
            this.loopReturnType = loopReturnType;
            this.clauses = clauses.ToList();
            this.body = body;

            if (clauses.Select(x => x.Name).HasDuplicates()) throw new PascalesqueException("Duplicate variables in letrec");
        }

        #region IExpression Members

        public EnvSpec GetEnvSpec()
        {
            EnvSpec e = body.GetEnvSpec();
            foreach (LetClause lc in clauses)
            {
                e |= lc.Value.GetEnvSpec();
            }
            return e - clauses.Select(x => x.Name);
        }

        private Type GetFuncType()
        {
            if (loopReturnType == typeof(void))
            {
                return System.Linq.Expressions.Expression.GetActionType(clauses.Select(x => x.VarType).ToArray());
            }
            else
            {
                return System.Linq.Expressions.Expression.GetFuncType(clauses.Select(x => x.VarType).AndAlso(loopReturnType).ToArray());
            }
        }

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            Type funcType = GetFuncType();

            EnvDescTypesOnly innerEnvDesc = EnvDescTypesOnly.Shadow(envDesc, clauses.Select(x => new ParamInfo(x.Name, x.VarType)).AndAlso(new ParamInfo(loopName, funcType)));
            Type t = body.GetReturnType(innerEnvDesc);
            if (t != loopReturnType) throw new PascalesqueException("let loop: loop does not return expected type");

            return t;
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {

            Type funcType = GetFuncType();

            IExpression e2 = new LetRecExpr
            (
                new LetClause[]
                {
                    new LetClause
                    (
                        loopName, funcType,
                        new LambdaExpr
                        (
                            clauses.Select(x => new ParamInfo(x.Name, x.VarType)),
                            body
                        )
                    )
                },
                new InvokeExpr
                (
                    new VarRefExpr(loopName),
                    clauses.Select(x => x.Value)
                )
            );
            e2.Compile(cc, envDesc, tail);
        }

        #endregion
    }

    public class ParamInfo
    {
        private Symbol name;
        private Type paramType;

        public ParamInfo(Symbol name, Type paramType)
        {
            this.name = name;
            this.paramType = paramType;
        }

        public Symbol Name { get { return name; } }

        public Type ParamType { get { return paramType; } }
    }

    public class LambdaExpr : IExpression
    {
        private List<ParamInfo> parameters;
        private IExpression body;

        public LambdaExpr(IEnumerable<ParamInfo> parameters, IExpression body)
        {
            this.parameters = parameters.ToList();
            this.body = body;
        }

        #region IExpression Members

        public EnvSpec GetEnvSpec()
        {
            EnvSpec e = body.GetEnvSpec() - parameters.Select(x => x.Name);
            return EnvSpec.CaptureAll(e);
        }

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            EnvDescTypesOnly innerEnvDesc = EnvDescTypesOnly.Shadow(envDesc, parameters.Select(x => new ParamInfo(x.Name, x.ParamType)));
            Type returnType = body.GetReturnType(innerEnvDesc);
            if (returnType == typeof(void))
            {
                return System.Linq.Expressions.Expression.GetActionType(parameters.Select(x => x.ParamType).ToArray());
            }
            else
            {
                List<Type> t = parameters.Select(x => x.ParamType).ToList();
                t.Add(returnType);

                return System.Linq.Expressions.Expression.GetFuncType(t.ToArray());
            }
        }

        public IEnumerable<ParamInfo> ParamInfos { get { return parameters.AsEnumerable(); } }

        public IExpression Body { get { return body; } }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {
            EnvDescTypesOnly innerEnvDesc = EnvDescTypesOnly.Shadow(envDesc.TypesOnly(), parameters.Select(x => new ParamInfo(x.Name, x.ParamType)));
            Type returnType = body.GetReturnType(innerEnvDesc);

            EnvSpec e = body.GetEnvSpec() - parameters.Select(x => x.Name);
            Symbol[] capturedVars = e.Keys.ToArray();

            Symbol typeName = new Symbol();
            TypeBuilder lambdaObj = cc.ModuleBuilder.DefineType(typeName.ToString(), TypeAttributes.Public);
            
            Type[] constructorParams = capturedVars.Select(s => cc.MakeBoxedType(envDesc[s].VarType)).ToArray();
            FieldBuilder[] lambdaFields = capturedVars.Select(s => lambdaObj.DefineField(s.ToString(), cc.MakeBoxedType(envDesc[s].VarType), FieldAttributes.Private)).ToArray();

            ConstructorBuilder cb = lambdaObj.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, constructorParams);

            ILGenerator cilg = cb.GetILGenerator();

            int iEnd = capturedVars.Length;
            List<Tuple<Symbol, IVarDesc>> innerVars = new List<Tuple<Symbol, IVarDesc>>();
            for (int i = 0; i < iEnd; ++i)
            {
                cilg.LoadArg(0);
                cilg.LoadArg(i+1);
                cilg.StoreField(lambdaFields[i]);
                
                innerVars.Add(new Tuple<Symbol, IVarDesc>(capturedVars[i], new FieldVarDesc(new ArgVarDesc(lambdaObj, false, 0), lambdaFields[i], innerEnvDesc[capturedVars[i]], true)));
            }
            cilg.Return();

            int jEnd = parameters.Count;
            for (int j = 0; j < jEnd; ++j)
            {
                innerVars.Add(new Tuple<Symbol, IVarDesc>(parameters[j].Name, new ArgVarDesc(parameters[j].ParamType, false, j + 1)));
            }

            Type[] invokeParams = parameters.Select(x => x.ParamType).ToArray();
            MethodBuilder mb = lambdaObj.DefineMethod("Invoke", MethodAttributes.Public, returnType, invokeParams);

            ILGenerator milg = mb.GetILGenerator();

            EnvDesc innerEnvDesc2 = EnvDesc.FromSequence(innerVars);

            body.Compile(cc.NewContext(lambdaObj, milg), innerEnvDesc2, true);

            Type lambdaObjType = lambdaObj.CreateType();

            ILGenerator ilg = cc.ILGenerator;
            for (int i = 0; i < iEnd; ++i)
            {
                envDesc[capturedVars[i]].FetchBox(cc, false);
            }
            ilg.NewObj(lambdaObjType.GetConstructor(constructorParams));

            ilg.LoadFunction(lambdaObjType.GetMethod("Invoke"));

            Type dType = GetReturnType(envDesc.TypesOnly());

            ConstructorInfo[] dci = dType.GetConstructors();

            ilg.NewObj(dType.GetConstructor(new Type[] { typeof(object), typeof(IntPtr) }));
            if (tail) ilg.Return();
        }

        #endregion
    }

    public class InvokeExpr : IExpression
    {
        private IExpression func;
        private List<IExpression> args;

        public InvokeExpr(IExpression func, IEnumerable<IExpression> args)
        {
            this.func = func;
            this.args = args.ToList();
        }

        #region IExpression Members

        public EnvSpec GetEnvSpec()
        {
            EnvSpec e = func.GetEnvSpec();
            foreach (IExpression arg in args)
            {
                e |= arg.GetEnvSpec();
            }
            return e;
        }

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            Type funcType = func.GetReturnType(envDesc);
            if (!(funcType.IsSubclassOf(typeof(Delegate)))) throw new PascalesqueException("Invocation of a non-delegate");
            MethodInfo mi = funcType.GetMethod("Invoke");

            ParameterInfo[] p = mi.GetParameters();
            if (p.Length != args.Count) throw new PascalesqueException("Argument count doesn't match parameter count");

            int iEnd = p.Length;
            for (int i = 0; i < iEnd; ++i)
            {
                if (p[i].ParameterType != args[i].GetReturnType(envDesc)) throw new PascalesqueException("Argument " + i + " type doesn't match parameter type");
            }

            return mi.ReturnType;
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {
            Type funcType = func.GetReturnType(envDesc.TypesOnly());

            func.Compile(cc, envDesc, false);
            foreach (IExpression arg in args)
            {
                arg.Compile(cc, envDesc, false);
            }
            ILGenerator ilg = cc.ILGenerator;
            if (tail) ilg.Tail();
            ilg.CallVirt(funcType.GetMethod("Invoke"));
            if (tail) ilg.Return();
        }

        #endregion
    }

    public class BinaryOpExpr : IExpression
    {
        private BinaryOp op;
        private IExpression addend1;
        private IExpression addend2;

        public BinaryOpExpr(BinaryOp op, IExpression addend1, IExpression addend2)
        {
            this.op = op;
            this.addend1 = addend1;
            this.addend2 = addend2;
        }

        #region IExpression Members

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

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            Type t1 = addend1.GetReturnType(envDesc);
            Type t2 = addend2.GetReturnType(envDesc);

            if (op == BinaryOp.Shl || op == BinaryOp.Shr)
            {
                if (t2 != typeof(int) && t2 != typeof(IntPtr)) throw new PascalesqueException("Shift amount must be int or IntPtr");
            }
            else if (opArray.Contains(op))
            {
                int index = Array.IndexOf<BinaryOp>(opArray, op);
                System.Diagnostics.Debug.Assert(index >= 0 && index < opArray.Length);
                MethodInfo mi = typeof(Math).GetMethod(opName[index], BindingFlags.Public | BindingFlags.Static, null, new Type[] { t1, t2 }, null);
                if (mi == null) throw new PascalesqueException("Unknown binary op / types");
                return mi.ReturnType;
            }
            else
            {
                if (t1 != t2) throw new PascalesqueException("Attempt to do binary op on two different types");
            }

            return t1;
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            EnvDescTypesOnly edto = envDesc.TypesOnly();
            Type t1 = addend1.GetReturnType(edto);
            Type t2 = addend2.GetReturnType(edto);

            bool isUnsigned = (t1 == typeof(byte) || t1 == typeof(ushort) || t1 == typeof(uint) || t1 == typeof(ulong));

            addend1.Compile(cc, envDesc, false);
            addend2.Compile(cc, envDesc, false);
            switch(op)
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
                        MethodInfo mi = typeof(Math).GetMethod(opName[index], BindingFlags.Public | BindingFlags.Static, null, new Type[] { t1, t2 }, null);
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

    public class UnaryOpExpr : IExpression
    {
        UnaryOp op;
        IExpression expr;

        public UnaryOpExpr(UnaryOp op, IExpression expr)
        {
            this.op = op;
            this.expr = expr;
        }

        #region IExpression Members

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

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            Type t = expr.GetReturnType(envDesc);
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
                return typeof(bool);
            }
            else
            {
                MethodInfo m = GetMathMethod(op, t);
                if (m == null) throw new PascalesqueException("Unknown UnaryOp / type");
                return m.ReturnType;
            }
            return t;
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            Type exprType = expr.GetReturnType(envDesc.TypesOnly());

            expr.Compile(cc, envDesc, false);
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

    public class ConvertExpr : IExpression
    {
        private ConvertTo convertTo;
        private IExpression expression;

        public ConvertExpr(ConvertTo convertTo, IExpression expression)
        {
            this.convertTo = convertTo;
            this.expression = expression;
        }

        #region IExpression Members

        public EnvSpec GetEnvSpec()
        {
            return expression.GetEnvSpec();
        }

        public static Type GetReturnType(ConvertTo convertTo)
        {
            switch (convertTo)
            {
                case ConvertTo.Byte: return typeof(byte);
                case ConvertTo.Short: return typeof(short);
                case ConvertTo.Int: return typeof(int);
                case ConvertTo.Long: return typeof(long);
                case ConvertTo.IntPtr: return typeof(IntPtr);
                case ConvertTo.SByte: return typeof(sbyte);
                case ConvertTo.UShort: return typeof(ushort);
                case ConvertTo.UInt: return typeof(uint);
                case ConvertTo.ULong: return typeof(ulong);
                case ConvertTo.UIntPtr: return typeof(UIntPtr);
                case ConvertTo.Float: return typeof(float);
                case ConvertTo.Double: return typeof(double);
                default: throw new PascalesqueException("Unknown ConvertTo type");
            }
        }

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            return GetReturnType(convertTo);
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            Type exprType = expression.GetReturnType(envDesc.TypesOnly());

            bool isUnsigned = (exprType == typeof(byte) || exprType == typeof(ushort) || exprType == typeof(uint) || exprType == typeof(ulong));

            expression.Compile(cc, envDesc, false);
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

    public class RegardAsExpr : IExpression
    {
        private ConvertTo regardAsWhat;
        private IExpression expression;

        public RegardAsExpr(ConvertTo regardAsWhat, IExpression expression)
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

        public static ActualStackType GetActualStackType(Type t)
        {
            if (t == typeof(byte) || t == typeof(sbyte) || t == typeof(short) || t == typeof(ushort) || t == typeof(int) || t == typeof(uint) || t == typeof(bool))
            {
                return ActualStackType.Int32;
            }
            else if (t == typeof(long) || t == typeof(ulong))
            {
                return ActualStackType.Int64;
            }
            else if (t == typeof(IntPtr) || t == typeof(UIntPtr))
            {
                return ActualStackType.IntPtr;
            }
            else if (t == typeof(float) || t == typeof(double))
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

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            Type eType = expression.GetReturnType(envDesc);

            if (GetActualStackType(eType) != GetActualStackType(regardAsWhat))
                throw new PascalesqueException("RegardAs doesn't work if types are physically different");

            return ConvertExpr.GetReturnType(regardAsWhat);
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {
            expression.Compile(cc, envDesc, tail);
        }

        #endregion
    }

    public class ComparisonExpr : IExpression
    {
        private Comparison comp;
        private IExpression expr1;
        private IExpression expr2;

        public ComparisonExpr(Comparison comp, IExpression expr1, IExpression expr2)
        {
            this.comp = comp;
            this.expr1 = expr1;
            this.expr2 = expr2;
        }

        #region IExpression Members

        public EnvSpec GetEnvSpec()
        {
            return expr1.GetEnvSpec() | expr2.GetEnvSpec();
        }

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            Type t1 = expr1.GetReturnType(envDesc);
            Type t2 = expr2.GetReturnType(envDesc);
            if (t1 != t2) throw new PascalesqueException("Comparison requires operands of same type");

            return typeof(bool);
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            Type t = expr1.GetReturnType(envDesc.TypesOnly());
            bool isUnsigned = (t == typeof(byte) || t == typeof(ushort) || t == typeof(uint) || t == typeof(ulong));

            expr1.Compile(cc, envDesc, false);
            
            if (t == typeof(byte)) ilg.Conv_U1();
            else if (t == typeof(short)) ilg.Conv_I2();
            else if (t == typeof(sbyte)) ilg.Conv_I1();
            else if (t == typeof(ushort)) ilg.Conv_U2();

            expr2.Compile(cc, envDesc, false);

            if (t == typeof(byte)) ilg.Conv_U1();
            else if (t == typeof(short)) ilg.Conv_I2();
            else if (t == typeof(sbyte)) ilg.Conv_I1();
            else if (t == typeof(ushort)) ilg.Conv_U2();

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

    public class ArrayLenExpr : IExpression
    {
        private IExpression array;

        public ArrayLenExpr(IExpression array)
        {
            this.array = array;
        }

        #region IExpression Members

        public EnvSpec GetEnvSpec()
        {
            return array.GetEnvSpec();
        }

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            Type tx = array.GetReturnType(envDesc);
            if (!(tx.IsArray)) throw new PascalesqueException("ArrayLen requires an array");

            return typeof(int);
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            Type tx = array.GetReturnType(envDesc.TypesOnly());

            array.Compile(cc, envDesc, false);

            if (tail) ilg.Tail();
            ilg.CallVirt(tx.GetMethod("get_Length", Type.EmptyTypes));
            if (tail) ilg.Return();
        }

        #endregion
    }

    public class ArrayRefExpr : IExpression
    {
        private IExpression array;
        private IExpression index;

        public ArrayRefExpr(IExpression array, IExpression index)
        {
            this.array = array;
            this.index = index;
        }

        #region IExpression Members

        public EnvSpec GetEnvSpec()
        {
            return array.GetEnvSpec() | index.GetEnvSpec();
        }

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            Type t = array.GetReturnType(envDesc);
            if (!(t.IsArray)) throw new PascalesqueException("ArrayRef type mismatch; array required");

            Type i = index.GetReturnType(envDesc);
            if (i != typeof(int) && i != typeof(IntPtr)) throw new PascalesqueException("ArrayRef type mismatch; index must be int or IntPtr");

            return t.GetElementType();
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            Type t = array.GetReturnType(envDesc.TypesOnly());
            if (!(t.IsArray)) throw new PascalesqueException("ArrayRef type mismatch; array required");

            Type elType = t.GetElementType();

            array.Compile(cc, envDesc, false);
            index.Compile(cc, envDesc, false);
            ilg.LoadElement(elType);
            if (tail) ilg.Return();
        }

        #endregion
    }

    public class ArraySetExpr : IExpression
    {
        private IExpression array;
        private IExpression index;
        private IExpression value;

        public ArraySetExpr(IExpression array, IExpression index, IExpression value)
        {
            this.array = array;
            this.index = index;
            this.value = value;
        }

        #region IExpression Members

        public EnvSpec GetEnvSpec()
        {
            return array.GetEnvSpec() | index.GetEnvSpec() | value.GetEnvSpec();
        }

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            Type t = array.GetReturnType(envDesc);
            if (!(t.IsArray)) throw new PascalesqueException("ArraySet type mismatch; array required");

            Type i = index.GetReturnType(envDesc);
            if (i != typeof(int) && i != typeof(IntPtr)) throw new PascalesqueException("ArraySet type mismatch; index must be int or IntPtr");

            Type x = value.GetReturnType(envDesc);
            if (x != t.GetElementType()) throw new PascalesqueException("ArraySet type mismatch; value must match item type of array");

            return typeof(void);
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            Type t = array.GetReturnType(envDesc.TypesOnly());
            if (!(t.IsArray)) throw new PascalesqueException("ArrayRef type mismatch; array required");

            Type elType = t.GetElementType();

            array.Compile(cc, envDesc, false);
            index.Compile(cc, envDesc, false);
            value.Compile(cc, envDesc, false);
            ilg.StoreElement(elType);
            if (tail) ilg.Return();
        }

        #endregion
    }

    public class NewArrayExpr : IExpression
    {
        private Type itemType;
        private IExpression size;

        public NewArrayExpr(Type itemType, IExpression size)
        {
            this.itemType = itemType;
            this.size = size;
        }

        #region IExpression Members

        public EnvSpec GetEnvSpec()
        {
            return size.GetEnvSpec();
        }

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            return itemType.MakeArrayType();
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            size.Compile(cc, envDesc, false);
            ilg.Emit(OpCodes.Newarr, itemType);
            if (tail) ilg.Return();
        }

        #endregion
    }

    public class MethodCallExpr : IExpression
    {
        private MethodBase methodToCall;
        private List<IExpression> arguments;

        public MethodCallExpr(MethodBase methodToCall, IEnumerable<IExpression> arguments)
        {
            this.methodToCall = methodToCall;
            this.arguments = arguments.ToList();
        }

        #region IExpression Members

        public EnvSpec GetEnvSpec()
        {
            EnvSpec e = EnvSpec.Empty();
            foreach (IExpression arg in arguments)
            {
                e |= arg.GetEnvSpec();
            }
            return e;
        }

        private static int GetParameterCount(MethodBase b)
        {
            if (b is MethodInfo)
            {
                MethodInfo mi = (MethodInfo)b;
                if (mi.IsStatic)
                {
                    return mi.GetParameters().Length;
                }
                else
                {
                    return mi.GetParameters().Length + 1;
                }
            }
            else if (b is ConstructorInfo)
            {
                ConstructorInfo ci = (ConstructorInfo)b;
                return ci.GetParameters().Length;
            }
            else throw new PascalesqueException("MethodBase is neither a MethodInfo nor a ConstructorInfo");
        }

        private static Type GetParameterType(MethodBase b, int index)
        {
            if (b is MethodInfo)
            {
                MethodInfo mi = (MethodInfo)b;
                if (mi.IsStatic)
                {
                    return mi.GetParameters()[index].ParameterType;
                }
                else
                {
                    if (index == 0)
                    {
                        return mi.DeclaringType;
                    }
                    else
                    {
                        return mi.GetParameters()[index - 1].ParameterType;
                    }
                }
            }
            else if (b is ConstructorInfo)
            {
                ConstructorInfo ci = (ConstructorInfo)b;
                return ci.GetParameters()[index].ParameterType;
            }
            else throw new PascalesqueException("MethodBase is neither a MethodInfo nor a ConstructorInfo");
        }

        private static Type GetReturnType(MethodBase b)
        {
            if (b is MethodInfo)
            {
                MethodInfo mi = (MethodInfo)b;
                return mi.ReturnType;
            }
            else if (b is ConstructorInfo)
            {
                ConstructorInfo ci = (ConstructorInfo)b;
                return ci.DeclaringType;
            }
            else throw new PascalesqueException("MethodBase is neither a MethodInfo nor a ConstructorInfo");
        }

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            int iEnd = GetParameterCount(methodToCall);
            if (arguments.Count != iEnd) throw new PascalesqueException("Argument count doesn't match parameter count");

            for (int i = 0; i < iEnd; ++i)
            {
                Type tParam = GetParameterType(methodToCall, i);
                if (tParam.IsByRef) throw new PascalesqueException("ByRef parameters not supported");
                Type tArg = arguments[i].GetReturnType(envDesc);
                if (tParam != tArg) throw new PascalesqueException("Argument type doesn't match parameter type");
            }

            return GetReturnType(methodToCall);
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            foreach (IExpression arg in arguments)
            {
                arg.Compile(cc, envDesc, false);
            }
            
            if (methodToCall is ConstructorInfo)
            {
                ilg.NewObj((ConstructorInfo)methodToCall);
                if (tail) ilg.Return();
            }
            else if (methodToCall is MethodInfo)
            {
                MethodInfo mi = (MethodInfo)methodToCall;
                if (mi.IsVirtual)
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
        }

        #endregion
    }

    public class FieldRefExpr : IExpression
    {
        private IExpression fieldOfWhat;
        private FieldInfo fieldInfo;

        public FieldRefExpr(IExpression fieldOfWhat, FieldInfo fieldInfo)
        {
            this.fieldOfWhat = fieldOfWhat;
            this.fieldInfo = fieldInfo;
        }

        #region IExpression Members

        public EnvSpec GetEnvSpec()
        {
            return fieldOfWhat.GetEnvSpec();
        }

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            Type t = fieldOfWhat.GetReturnType(envDesc);
            if (t != fieldInfo.DeclaringType) throw new PascalesqueException("Type Mismatch for Field Reference");

            return fieldInfo.FieldType;
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            fieldOfWhat.Compile(cc, envDesc, false);
            ilg.LoadField(fieldInfo);
            if (tail) ilg.Return();
        }

        #endregion
    }

    public class FieldSetExpr : IExpression
    {
        private IExpression fieldOfWhat;
        private FieldInfo fieldInfo;
        private IExpression val;

        public FieldSetExpr(IExpression fieldOfWhat, FieldInfo fieldInfo, IExpression val)
        {
            this.fieldOfWhat = fieldOfWhat;
            this.fieldInfo = fieldInfo;
            this.val = val;
        }

        #region IExpression Members

        public EnvSpec GetEnvSpec()
        {
            return fieldOfWhat.GetEnvSpec() | val.GetEnvSpec();
        }

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            Type t1 = fieldOfWhat.GetReturnType(envDesc);
            Type t2 = val.GetReturnType(envDesc);
            if (t1 != fieldInfo.DeclaringType || t2 != fieldInfo.FieldType) throw new PascalesqueException("Type mismatch for Field Set");

            return typeof(void);
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            fieldOfWhat.Compile(cc, envDesc, false);
            val.Compile(cc, envDesc, false);
            ilg.StoreField(fieldInfo);
            if (tail) ilg.Return();
        }

        #endregion
    }

    public class PokeExpr : IExpression
    {
        private IExpression ptr;
        private IExpression value;

        public PokeExpr(IExpression ptr, IExpression value)
        {
            this.ptr = ptr;
            this.value = value;
        }

        #region IExpression Members

        public EnvSpec GetEnvSpec()
        {
            return ptr.GetEnvSpec() | value.GetEnvSpec();
        }

        private static Type[] okTypes = new Type[]
        {
            typeof(byte), typeof(short), typeof(int), typeof(long), typeof(IntPtr),
            typeof(sbyte), typeof(ushort), typeof(uint), typeof(ulong), typeof(UIntPtr),
            typeof(float), typeof(double)
        };

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            Type t1 = ptr.GetReturnType(envDesc);
            Type t2 = value.GetReturnType(envDesc);

            if (!okTypes.Contains(t2)) throw new PascalesqueException("Poke: argument type cannot be poked");
            if (t1 != typeof(IntPtr) && t1 != typeof(UIntPtr)) throw new PascalesqueException("Poke: pointer type is not IntPtr");
            
            return typeof(void);
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;

            ptr.Compile(cc, envDesc, false);
            value.Compile(cc, envDesc, false);
            Type t2 = value.GetReturnType(envDesc.TypesOnly());
            ilg.Unaligned(Alignment.One);
            ilg.StoreObjIndirect(t2);
            if (tail) ilg.Return();
        }

        #endregion
    }

    public class PeekExpr : IExpression
    {
        private IExpression ptr;
        private Type type;

        public PeekExpr(IExpression ptr, Type type)
        {
            this.ptr = ptr;
            this.type = type;
        }

        #region IExpression Members

        public EnvSpec GetEnvSpec()
        {
            return ptr.GetEnvSpec();
        }

        private static Type[] okTypes = new Type[]
        {
            typeof(byte), typeof(short), typeof(int), typeof(long), typeof(IntPtr),
            typeof(sbyte), typeof(ushort), typeof(uint), typeof(ulong), typeof(UIntPtr),
            typeof(float), typeof(double)
        };

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            if (!(okTypes.Contains(type))) throw new PascalesqueException("Peek: type can't be peeked");

            return type;
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {
            ILGenerator ilg = cc.ILGenerator;
            ptr.Compile(cc, envDesc, false);
            ilg.Unaligned(Alignment.One);
            ilg.LoadObjIndirect(type);
            if (tail) ilg.Return();
        }

        #endregion
    }

    public class MemCpyExpr : IExpression
    {
        private IExpression destAddr;
        private IExpression srcAddr;
        private IExpression count;

        public MemCpyExpr(IExpression destAddr, IExpression srcAddr, IExpression count)
        {
            this.destAddr = destAddr;
            this.srcAddr = srcAddr;
            this.count = count;
        }

        #region IExpression Members

        public EnvSpec GetEnvSpec()
        {
            return destAddr.GetEnvSpec() | srcAddr.GetEnvSpec() | count.GetEnvSpec();
        }

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            Type destAddrType = destAddr.GetReturnType(envDesc);
            Type srcAddrType = srcAddr.GetReturnType(envDesc);
            Type countType = count.GetReturnType(envDesc);

            if (destAddrType != typeof(IntPtr) && destAddrType != typeof(UIntPtr)) throw new PascalesqueException("memcpy dest address must be IntPtr or UIntPtr");
            if (srcAddrType != typeof(IntPtr) && srcAddrType != typeof(UIntPtr)) throw new PascalesqueException("memcpy source address must be IntPtr or UIntPtr");
            if (countType != typeof(uint)) throw new PascalesqueException("memcpy count must be uint");

            return typeof(void);
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {
            destAddr.Compile(cc, envDesc, false);
            srcAddr.Compile(cc, envDesc, false);
            count.Compile(cc, envDesc, false);
            cc.ILGenerator.Unaligned(Alignment.One);
            cc.ILGenerator.Emit(OpCodes.Cpblk);
            if (tail) cc.ILGenerator.Return();
        }

        #endregion
    }

    public class MemSetExpr : IExpression
    {
        private IExpression destAddr;
        private IExpression fillValue;
        private IExpression count;

        public MemSetExpr(IExpression destAddr, IExpression fillValue, IExpression count)
        {
            this.destAddr = destAddr;
            this.fillValue = fillValue;
            this.count = count;
        }

        #region IExpression Members

        public EnvSpec GetEnvSpec()
        {
            return destAddr.GetEnvSpec() | fillValue.GetEnvSpec() | count.GetEnvSpec();
        }

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            Type destAddrType = destAddr.GetReturnType(envDesc);
            Type fillValueType = fillValue.GetReturnType(envDesc);
            Type countType = count.GetReturnType(envDesc);

            if (destAddrType != typeof(IntPtr) && destAddrType != typeof(UIntPtr)) throw new PascalesqueException("memset dest address must be IntPtr or UIntPtr");
            if (fillValueType != typeof(byte) && fillValueType != typeof(sbyte)) throw new PascalesqueException("memset fillValue must be byte or sbyte");
            if (countType != typeof(uint)) throw new PascalesqueException("memset count must be uint");

            return typeof(void);
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {
            destAddr.Compile(cc, envDesc, false);
            fillValue.Compile(cc, envDesc, false);
            count.Compile(cc, envDesc, false);
            cc.ILGenerator.Unaligned(Alignment.One);
            cc.ILGenerator.Emit(OpCodes.Initblk);
            if (tail) cc.ILGenerator.Return();
        }

        #endregion
    }

    public class PinClause
    {
        private Symbol name;
        private IExpression val;

        public PinClause(Symbol name, IExpression val)
        {
            this.name = name;
            this.val = val;
        }

        public Symbol Name { get { return name; } }
        public IExpression Value { get { return val; } }
    }

    public class PinExpr : IExpression
    {
        private List<PinClause> clauses;
        private IExpression body;

        public PinExpr(IEnumerable<PinClause> clauses, IExpression body)
        {
            this.clauses = clauses.ToList();
            this.body = body;
        }

        #region IExpression Members

        public EnvSpec GetEnvSpec()
        {
            EnvSpec e = body.GetEnvSpec() - clauses.Select(x => x.Name);
            e = EnvSpec.CaptureAll(e);
            foreach (PinClause lc in clauses)
            {
                e |= lc.Value.GetEnvSpec();
            }
            return e;
        }

        private static Type[] okTypes = new Type[]
        {
            typeof(byte), typeof(short), typeof(int), typeof(long), typeof(IntPtr),
            typeof(sbyte), typeof(ushort), typeof(uint), typeof(ulong), typeof(UIntPtr),
            typeof(float), typeof(double)
        };

        private bool IsOkArrayType(Type t)
        {
            return t.IsArray && okTypes.Contains(t.GetElementType());
        }

        private EnvDescTypesOnly MakeInnerEnvDesc(EnvDescTypesOnly outerEnvDesc)
        {
            return EnvDescTypesOnly.Shadow(outerEnvDesc, clauses.Select(x => new ParamInfo(x.Name, typeof(IntPtr))));
        }

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            if (clauses.Select(x => x.Name).HasDuplicates()) throw new PascalesqueException("pin has two variables with the same name");
            if (clauses.Any(x => !IsOkArrayType(x.Value.GetReturnType(envDesc)))) throw new PascalesqueException("attempt to pin impossible type");

            EnvDescTypesOnly e2 = MakeInnerEnvDesc(envDesc);

            return body.GetReturnType(e2);
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {
            EnvDescTypesOnly innerEnvDesc1 = MakeInnerEnvDesc(envDesc.TypesOnly());
            Type returnType = body.GetReturnType(innerEnvDesc1);

            EnvSpec e = body.GetEnvSpec() - clauses.Select(x => x.Name);
            e = EnvSpec.CaptureAll(e);
            Symbol[] capturedVars = e.Keys.ToArray();

            Symbol methodName = new Symbol();

            Type[] arrayToPinTypes = clauses.Select(x => x.Value.GetReturnType(envDesc.TypesOnly())).ToArray();

            Type[] captures = capturedVars.Select(s => cc.MakeBoxedType(envDesc[s].VarType)).ToArray();

            Type[] paramTypes = Pascalesque.Utils.ConcatArrays<Type>(captures, arrayToPinTypes);

            MethodBuilder mb = cc.TypeBuilder.DefineMethod(methodName.ToString(), MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.Final, returnType, paramTypes);

            ILGenerator milg = mb.GetILGenerator();

            ICompileContext mcc = cc.NewContext(milg);

            int iEnd = capturedVars.Length;
            List<Tuple<Symbol, IVarDesc>> innerVars = new List<Tuple<Symbol, IVarDesc>>();
            for (int i = 0; i < iEnd; ++i)
            {
                innerVars.Add(new Tuple<Symbol, IVarDesc>(capturedVars[i], new ArgVarDesc(innerEnvDesc1[capturedVars[i]], true, i)));
            }

            iEnd = clauses.Count;
            for (int i = 0; i < iEnd; ++i)
            {
                LocalBuilder lb_pin = milg.DeclareLocal(arrayToPinTypes[i].GetElementType().MakeByRefType(), true);
                bool boxed = false;
                if (e.ContainsKey(clauses[i].Name))
                {
                    boxed = e[clauses[i].Name].IsCaptured;
                }
                LocalBuilder lb_ptr = milg.DeclareLocal(boxed ? mcc.MakeBoxedType(typeof(IntPtr)) : typeof(IntPtr));

                milg.LoadArg(captures.Length + i);
                milg.LoadInt(0);
                milg.LoadElementAddress(arrayToPinTypes[i].GetElementType());
                milg.StoreLocal(lb_pin);
                if (boxed)
                {
                    milg.NewObj(mcc.MakeBoxedType(typeof(IntPtr)).GetConstructor(Type.EmptyTypes));
                    milg.StoreLocal(lb_ptr);
                }
                LocalVarDesc lvd = new LocalVarDesc(typeof(IntPtr), boxed, lb_ptr.LocalIndex);
                lvd.Store(mcc, delegate() { milg.LoadLocal(lb_pin); }, false);
                innerVars.Add(new Tuple<Symbol, IVarDesc>(clauses[i].Name, lvd));
            }

            EnvDesc innerEnvDesc = EnvDesc.FromSequence(innerVars);
            // "body" is not in the tail position, because we don't want the variables to become unpinned until it finishes.
            body.Compile(mcc, innerEnvDesc, false);
            milg.Return();

            // now to call this function...

            ILGenerator ilg = cc.ILGenerator;

            iEnd = capturedVars.Length;
            for (int i = 0; i < iEnd; ++i)
            {
                envDesc[capturedVars[i]].FetchBox(cc, false);
            }

            iEnd = clauses.Count;
            for (int i = 0; i < iEnd; ++i)
            {
                clauses[i].Value.Compile(cc, envDesc, false);
            }

            if (tail) ilg.Tail();
            ilg.Call(mb);
            if (tail) ilg.Return();
        }

        #endregion
    }

    public class ThrowExpr : IExpression
    {
        private Type typeNotReturned;
        private IExpression body;

        public ThrowExpr(Type typeNotReturned, IExpression body)
        {
            this.typeNotReturned = typeNotReturned;
            this.body = body;
        }

        #region IExpression Members

        public EnvSpec GetEnvSpec()
        {
            return body.GetEnvSpec();
        }

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            System.Diagnostics.Debug.Assert(typeof(Exception).IsAssignableFrom(body.GetReturnType(envDesc)));
            return typeNotReturned;
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {
            //Type t = body.GetReturnType(envDesc.TypesOnly());

            body.Compile(cc, envDesc, false);

            cc.ILGenerator.Throw();
        }

        #endregion
    }

    public class CatchClause
    {
        private Type exceptionType;
        private Symbol exceptionName;
        private IExpression body;

        public CatchClause(Type exceptionType, Symbol exceptionName, IExpression body)
        {
            this.exceptionType = exceptionType;
            this.exceptionName = exceptionName;
            this.body = body;
        }

        public Type ExceptionType { get { return exceptionType; } }
        public Symbol ExceptionName { get { return exceptionName; } }
        public IExpression Body { get { return body; } }
    }

    public class TryCatchFinallyExpr : IExpression
    {
        private IExpression body;
        private List<CatchClause> catchClauses;
        private IExpression finallyClause;

        public TryCatchFinallyExpr(IExpression body, IEnumerable<CatchClause> catchClauses, IExpression finallyClause)
        {
            this.body = body;
            this.catchClauses = catchClauses.ToList();
            this.finallyClause = finallyClause;
        }

        #region IExpression Members

        public EnvSpec GetEnvSpec()
        {
            EnvSpec e = body.GetEnvSpec() | finallyClause.GetEnvSpec() | catchClauses.Select(x => (x.Body.GetEnvSpec() - x.ExceptionName)).EnvSpecUnion();
            return EnvSpec.CaptureAll(e);
        }

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            Type t = body.GetReturnType(envDesc);
            bool areValid = catchClauses.All(x => t.IsAssignableFrom(x.Body.GetReturnType(EnvDescTypesOnly.Shadow(envDesc, x.ExceptionName, x.ExceptionType))));
            if (!areValid) throw new PascalesqueException("catch clause does not have same return type as body");
            Type f = finallyClause.GetReturnType(envDesc);
            if (!(f == typeof(void))) throw new PascalesqueException("finally clause must have type of void");
            return t;
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {
            Type returnType = body.GetReturnType(envDesc.TypesOnly());

            Symbol[] capturedVars = GetEnvSpec().Keys.ToArray();

            Symbol methodName = new Symbol();

            Type[] captures = capturedVars.Select(s => cc.MakeBoxedType(envDesc[s].VarType)).ToArray();

            MethodBuilder mb = cc.TypeBuilder.DefineMethod(methodName.ToString(), MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.Final, returnType, captures);

            ILGenerator milg = mb.GetILGenerator();

            ICompileContext mcc = cc.NewContext(milg);

            EnvDescTypesOnly envDescTypesOnly = envDesc.TypesOnly();

            int iEnd = capturedVars.Length;
            List<Tuple<Symbol, IVarDesc>> innerVars = new List<Tuple<Symbol, IVarDesc>>();
            for (int i = 0; i < iEnd; ++i)
            {
                innerVars.Add(new Tuple<Symbol, IVarDesc>(capturedVars[i], new ArgVarDesc(envDescTypesOnly[capturedVars[i]], true, i)));
            }

            EnvDesc innerEnvDesc = EnvDesc.FromSequence(innerVars);

            LocalBuilder returnValueLocal = null;
            if (returnType != typeof(void))
            {
                returnValueLocal = milg.DeclareLocal(returnType);
            }
            Label l = milg.BeginExceptionBlock();
            body.Compile(mcc, innerEnvDesc, false);
            if (returnType != typeof(void))
            {
                milg.StoreLocal(returnValueLocal);
            }
            //milg.Leave(l); redundant
            foreach (CatchClause catchClause in catchClauses)
            {
                EnvSpec e2 = catchClause.Body.GetEnvSpec();
                if (e2.ContainsKey(catchClause.ExceptionName))
                {
                    VarSpec evc = e2[catchClause.ExceptionName];
                    
                    IVarDesc exceptionVarDesc;
                    EnvDesc catchEnvDesc;

                    milg.BeginCatchBlock(catchClause.ExceptionType);

                    if (evc.IsCaptured)
                    {
                        LocalBuilder lb0 = milg.DeclareLocal(catchClause.ExceptionType);
                        milg.StoreLocal(lb0);

                        LocalBuilder lb = milg.DeclareLocal(mcc.MakeBoxedType(catchClause.ExceptionType));
                        milg.NewObj(mcc.MakeBoxedType(catchClause.ExceptionType).GetConstructor(Type.EmptyTypes));
                        milg.StoreLocal(lb);
                        exceptionVarDesc = new LocalVarDesc(catchClause.ExceptionType, true, lb.LocalIndex);
                        catchEnvDesc = EnvDesc.Shadow(innerEnvDesc, catchClause.ExceptionName, exceptionVarDesc);
                        exceptionVarDesc.Store(mcc, delegate() { milg.LoadLocal(lb0); }, false);
                    }
                    else
                    {
                        LocalBuilder lb = milg.DeclareLocal(catchClause.ExceptionType);
                        exceptionVarDesc = new LocalVarDesc(catchClause.ExceptionType, false, lb.LocalIndex);
                        catchEnvDesc = EnvDesc.Shadow(innerEnvDesc, catchClause.ExceptionName, exceptionVarDesc);
                        milg.StoreLocal(lb);
                    }
                    
                    catchClause.Body.Compile(mcc, catchEnvDesc, false);
                    if (returnType != typeof(void))
                    {
                        milg.StoreLocal(returnValueLocal);
                    }
                    //milg.Leave(l); redundant
                }
                else
                {
                    milg.BeginCatchBlock(catchClause.ExceptionType);
                    catchClause.Body.Compile(mcc, innerEnvDesc, false);
                    if (returnType != typeof(void))
                    {
                        milg.StoreLocal(returnValueLocal);
                    }
                    //milg.Leave(l); redundant
                }
            }

            if (!(finallyClause is EmptyExpr))
            {
                milg.BeginFinallyBlock();
                finallyClause.Compile(mcc, innerEnvDesc, false);
            }

            milg.EndExceptionBlock();

            if (returnType != typeof(void))
            {
                milg.LoadLocal(returnValueLocal);
            }
            milg.Return();

            // now to call this function...

            ILGenerator ilg = cc.ILGenerator;

            iEnd = capturedVars.Length;
            for (int i = 0; i < iEnd; ++i)
            {
                envDesc[capturedVars[i]].FetchBox(cc, false);
            }

            if (tail) ilg.Tail();
            ilg.Call(mb);
            if (tail) ilg.Return();
        }

        #endregion
    }

    public class CastClassExpr : IExpression
    {
        private IExpression body;
        private Type toType;

        public CastClassExpr(Type toType, IExpression body)
        {
            this.body = body;
            this.toType = toType;
        }

        #region IExpression Members

        public EnvSpec GetEnvSpec()
        {
            return body.GetEnvSpec();
        }

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            Type fromType = body.GetReturnType(envDesc);
            if (!(fromType.IsAssignableFrom(toType))) throw new PascalesqueException("CastClass won't work with provided types");
            return toType;
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {
            body.Compile(cc, envDesc, false);
            cc.ILGenerator.CastClass(toType);
            if (tail) cc.ILGenerator.Return();
        }

        #endregion
    }

    public class RegardAsClassExpr : IExpression
    {
        private IExpression body;
        private Type toType;

        public RegardAsClassExpr(Type toType, IExpression body)
        {
            this.body = body;
            this.toType = toType;
        }

        #region IExpression Members

        public EnvSpec GetEnvSpec()
        {
            return body.GetEnvSpec();
        }

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            Type fromType = body.GetReturnType(envDesc);
            if (!(toType.IsAssignableFrom(fromType))) throw new PascalesqueException("RegardAsClass won't work with provided types");
            return toType;
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {
            body.Compile(cc, envDesc, tail);
        }

        #endregion
    }

    public class IsInstanceExpr : IExpression
    {
        private IExpression body;
        private Type toType;

        public IsInstanceExpr(IExpression body, Type toType)
        {
            this.body = body;
            this.toType = toType;
        }

        #region IExpression Members

        public EnvSpec GetEnvSpec()
        {
            return body.GetEnvSpec();
        }

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            Type fromType = body.GetReturnType(envDesc);
            if (toType.IsAssignableFrom(fromType)) throw new PascalesqueException("IsInstance would always return true with provided types");
            if (!(fromType.IsAssignableFrom(toType))) throw new PascalesqueException("IsInstance would always return false with provided types");
            return typeof(bool);
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {
            body.Compile(cc, envDesc, false);
            cc.ILGenerator.IsInst(toType);
            if (tail) cc.ILGenerator.Return();
        }

        #endregion
    }

    public class IsNullExpr : IExpression
    {
        private IExpression body;

        public IsNullExpr(IExpression body)
        {
            this.body = body;
        }

        public EnvSpec GetEnvSpec()
        {
            return body.GetEnvSpec();
        }

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            Type fromType = body.GetReturnType(envDesc);
            if (fromType.IsValueType) throw new PascalesqueException("IsNull used on an expression of value type");
            return typeof(bool);
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {
            body.Compile(cc, envDesc, false);
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

    public class BoxExpr : IExpression
    {
        private IExpression valueToBox;

        public BoxExpr(IExpression valueToBox)
        {
            this.valueToBox = valueToBox;
        }

        #region IExpression Members

        public EnvSpec GetEnvSpec()
        {
            return valueToBox.GetEnvSpec();
        }

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            Type t = valueToBox.GetReturnType(envDesc);
            if (!(t.IsValueType)) throw new PascalesqueException("Attempt to box a non-value type " + t.FullName);

            return typeof(object);
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {
            Type t = valueToBox.GetReturnType(envDesc.TypesOnly());

            valueToBox.Compile(cc, envDesc, false);
            cc.ILGenerator.Box(t);
            if (tail) cc.ILGenerator.Return();
        }

        #endregion
    }

    public class UnboxExpr : IExpression
    {
        private IExpression valueToUnbox;
        private Type t;

        public UnboxExpr(IExpression valueToUnbox, Type t)
        {
            this.valueToUnbox = valueToUnbox;
            this.t = t;
        }

        #region IExpression Members

        public EnvSpec GetEnvSpec()
        {
            return valueToUnbox.GetEnvSpec();
        }

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            Type u = valueToUnbox.GetReturnType(envDesc);
            if (u != typeof(object)) throw new PascalesqueException("Attempt to unbox a non-object to " + t.FullName);

            return t;
        }

        public void Compile(ICompileContext cc, EnvDesc envDesc, bool tail)
        {
            valueToUnbox.Compile(cc, envDesc, false);
            cc.ILGenerator.Unbox(t);
            cc.ILGenerator.LoadObjIndirect(t);
            if (tail) cc.ILGenerator.Return();
        }

        #endregion
    }

    public class TupleItemExpr : IExpression
    {
        private IExpression tupleValue;
        private int index;

        public TupleItemExpr(IExpression tupleValue, int index)
        {
            this.tupleValue = tupleValue;
            this.index = index;
        }

        #region IExpression Members

        public EnvSpec GetEnvSpec()
        {
            return tupleValue.GetEnvSpec();
        }

        public Type GetReturnType(EnvDescTypesOnly envDesc)
        {
            Type t = tupleValue.GetReturnType(envDesc);
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

        #endregion
    }

    public class MethodToBuild
    {
        private Type delegateType;
        private Symbol name;
        private ParamInfo[] parameters;
        private Type returnType;
        private IExpression body;

        public MethodToBuild(Type delegateType, Symbol name, ParamInfo[] parameters, Type returnType, IExpression body)
        {
            this.delegateType = delegateType;
            this.name = name;
            this.parameters = parameters;
            this.returnType = returnType;
            this.body = body;
        }

        public Type DelegateType { get { return delegateType; } }
        public Symbol Name { get { return name; } }
        public ParamInfo[] Parameters { get { return parameters; } }
        public Type ReturnType { get { return returnType; } }
        public IExpression Body { get { return body; } }
    }

    public static class Compiler
    {
        private static IEnumerable<Tuple<Symbol, IVarDesc>> MakeVarDesc(ICompileContext cc, ParamInfo[] parameters, EnvSpec envSpec)
        {
            ILGenerator ilg = cc.ILGenerator;

            for (int i = 0; i < parameters.Length; ++i)
            {
                Symbol pName = parameters[i].Name;
                Type pType = parameters[i].ParamType;

                bool boxed = false;
                if (envSpec.ContainsKey(pName))
                {
                    boxed = envSpec[pName].IsCaptured;
                }

                if (boxed)
                {
                    Type pBoxedType = cc.MakeBoxedType(pType);
                    LocalBuilder lb = ilg.DeclareLocal(pBoxedType);
                    ilg.LoadArg(i);
                    ilg.NewObj(pBoxedType.GetConstructor(new Type[] { pType }));
                    ilg.StoreLocal(lb);

                    yield return new Tuple<Symbol, IVarDesc>(pName, new LocalVarDesc(pType, true, lb.LocalIndex));
                }
                else
                {
                    yield return new Tuple<Symbol, IVarDesc>(pName, new ArgVarDesc(pType, false, i));
                }
            }
        }

#if false
        private class DynamicMethodContext : ICompileContext
        {
            private ILGenerator ilg;

            public DynamicMethodContext(ILGenerator ilg)
            {
                this.ilg = ilg;
            }

            #region ICompileContext Members

            public ModuleBuilder ModuleBuilder { get { throw new PascalesqueException("ModuleBuilder not available in Dynamic Method Context"); } }

            public ICompileContext NewContext(ILGenerator newIlg) { throw new PascalesqueException("NewContext not available in Dynamic Method Context"); }

            public ILGenerator ILGenerator { get { return ilg; } }

            public Type MakeBoxedType(Type t)
            {
                return typeof(Box<>).MakeGenericType(new Type[] { t });
            }

            public bool IsBoxedType(Type t)
            {
                return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Box<>);
            }

            public Type BoxContentType(Type t)
            {
                if (!IsBoxedType(t)) throw new ArgumentException("Can only get box content types for boxes");

                return t.GetGenericArguments()[0];
            }

            #endregion
        }

        public static Delegate CompileDynamicMethod(Type delegateType, ParamInfo[] parameters, Type returnType, IExpression body)
        {
            EnvSpec envSpec = body.GetEnvSpec();

            EnvDescTypesOnly edto = EnvDescTypesOnly.FromSequence(parameters);

            Type bodyType = body.GetReturnType(edto);

            if (bodyType != returnType) throw new InvalidOperationException("body type doesn't match return type");

            DynamicMethod dm = new DynamicMethod(new Symbol().ToString(), returnType, parameters.Select(x => x.ParamType).ToArray(), typeof(PascalesqueCompiler));
            ILGenerator ilg = dm.GetILGenerator();
            ICompileContext cc = new DynamicMethodContext(ilg);

            EnvDesc envDesc = EnvDesc.FromSequence(MakeVarDesc(cc, parameters, envSpec));

            body.Compile(cc, envDesc, true);

            return dm.CreateDelegate(delegateType);
        }
#endif
        private class RunAndCollectContext : ICompileContext
        {
            private ModuleBuilder mb;
            private TypeBuilder tyb;
            private ILGenerator ilg;

            public RunAndCollectContext(ModuleBuilder mb, TypeBuilder tyb, ILGenerator ilg)
            {
                this.mb = mb;
                this.tyb = tyb;
                this.ilg = ilg;
            }

            #region ICompileContext Members

            public ModuleBuilder ModuleBuilder { get { return mb; } }

            public TypeBuilder TypeBuilder { get { return tyb; } }

            public ILGenerator ILGenerator { get { return ilg; } }

            public ICompileContext NewContext(TypeBuilder newTyb, ILGenerator newIlg)
            {
                return new RunAndCollectContext(mb, newTyb, newIlg);
            }

            public ICompileContext NewContext(ILGenerator newIlg)
            {
                return new RunAndCollectContext(mb, tyb, newIlg);
            }

            public Type MakeBoxedType(Type t)
            {
                return typeof(Box<>).MakeGenericType(new Type[] { t });
            }

            public bool IsBoxedType(Type t)
            {
                return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Box<>);
            }

            public Type BoxContentType(Type t)
            {
                if (!IsBoxedType(t)) throw new ArgumentException("Can only get box content types for boxes");

                return t.GetGenericArguments()[0];
            }

            #endregion
        }

        public static Delegate CompileRunAndCollect(Type delegateType, ParamInfo[] parameters, Type returnType, IExpression body)
        {
            EnvSpec envSpec = body.GetEnvSpec();

            EnvDescTypesOnly edto = EnvDescTypesOnly.FromSequence(parameters);

            Type bodyType = body.GetReturnType(edto);

            if (bodyType != returnType) throw new InvalidOperationException("body type doesn't match return type");

            Symbol assemblyNameSymbol = new Symbol();
            AssemblyName assemblyName = new AssemblyName(assemblyNameSymbol.ToString() + ".dll");
            AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            ModuleBuilder mb = ab.DefineDynamicModule(assemblyNameSymbol.ToString() + ".dll");
            Symbol typeName = new Symbol();
            TypeBuilder tyb = mb.DefineType(typeName.ToString(), TypeAttributes.Class | TypeAttributes.Public);
            Symbol methodName = new Symbol();
            MethodBuilder meb = tyb.DefineMethod(methodName.ToString(), MethodAttributes.Public | MethodAttributes.Static, returnType, parameters.Select(x => x.ParamType).ToArray());

            ILGenerator ilg = meb.GetILGenerator();
            ICompileContext cc = new RunAndCollectContext(mb, tyb, ilg);

            EnvDesc envDesc = EnvDesc.FromSequence(MakeVarDesc(cc, parameters, envSpec));

            body.Compile(cc, envDesc, true);

            Type tDone = tyb.CreateType();

            //ab.Save(assemblyNameSymbol.ToString() + ".dll");
            //Console.WriteLine("Saved: " + assemblyNameSymbol.ToString() + ".dll");

            return Delegate.CreateDelegate(delegateType, tDone.GetMethod(methodName.ToString()));
        }

        public static ExprObjModel.IProcedure CompileAsProcedure(LambdaExpr body)
        {
            EnvSpec envSpec = body.GetEnvSpec();

            if (!(envSpec.IsEmpty)) throw new PascalesqueException("CompileAsProcedure requires that there be no free variables");

            EnvDescTypesOnly edto = EnvDescTypesOnly.Empty();

            Type delegateType = body.GetReturnType(edto);

            Type returnType = delegateType.GetMethod("Invoke").ReturnType;

            Symbol assemblyNameSymbol = new Symbol();
            AssemblyName assemblyName = new AssemblyName(assemblyNameSymbol.ToString() + ".dll");
            AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            ModuleBuilder mb = ab.DefineDynamicModule(assemblyNameSymbol.ToString() + ".dll");
            Symbol typeName = new Symbol();
            TypeBuilder tyb = mb.DefineType(typeName.ToString(), TypeAttributes.Class | TypeAttributes.Public);
            Symbol methodName = new Symbol();
            MethodBuilder meb = tyb.DefineMethod(methodName.ToString(), MethodAttributes.Public | MethodAttributes.Static, returnType, body.ParamInfos.Select(x => x.ParamType).ToArray());

            ILGenerator ilg = meb.GetILGenerator();
            ICompileContext cc = new RunAndCollectContext(mb, tyb, ilg);

            EnvDesc envDesc = EnvDesc.FromSequence(MakeVarDesc(cc, body.ParamInfos.ToArray(), body.Body.GetEnvSpec()));

            body.Body.Compile(cc, envDesc, true);

            Type tDone = tyb.CreateType();

            //ab.Save(assemblyNameSymbol.ToString() + ".dll");
            //Console.WriteLine("Saved: " + assemblyNameSymbol.ToString() + ".dll");

            Delegate theDelegate = Delegate.CreateDelegate(delegateType, tDone.GetMethod(methodName.ToString()));

            return ExprObjModel.ProxyGenerator.GenerateProxyFromDelegate(mb, new Symbol().ToString(), new Symbol().ToString(), theDelegate);
        }

        public static List<Delegate> CompileRunAndCollect(IEnumerable<MethodToBuild> methods)
        {
            List<MethodToBuild> methodList = methods.ToList();

            if (methodList.Select(x => x.Name).HasDuplicates()) throw new PascalesqueException("Duplicate method name");

            List<EnvSpec> envSpecs = new List<EnvSpec>();

            foreach(MethodToBuild method in methodList)
            {
                EnvSpec envSpec = method.Body.GetEnvSpec();

                EnvDescTypesOnly edto = EnvDescTypesOnly.FromSequence(method.Parameters);

                Type bodyType = method.Body.GetReturnType(edto);

                if (bodyType != method.ReturnType) throw new InvalidOperationException("body type doesn't match return type");

                envSpecs.Add(envSpec);
            }

            Symbol assemblyNameSymbol = new Symbol();
            AssemblyName assemblyName = new AssemblyName(assemblyNameSymbol.ToString() + ".dll");
            AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            ModuleBuilder mb = ab.DefineDynamicModule(assemblyNameSymbol.ToString() + ".dll");
            Symbol typeName = new Symbol();
            TypeBuilder tyb = mb.DefineType(typeName.ToString(), TypeAttributes.Class | TypeAttributes.Public);

            List<Delegate> results = new List<Delegate>();

            int iEnd = methodList.Count;
            for(int i = 0; i < iEnd; ++i)
            {
                MethodToBuild method = methodList[i];
                EnvSpec envSpec = envSpecs[i];

                Symbol methodName = method.Name;
                MethodBuilder meb = tyb.DefineMethod(methodName.ToString(), MethodAttributes.Public | MethodAttributes.Static, method.ReturnType, method.Parameters.Select(x => x.ParamType).ToArray());

                ILGenerator ilg = meb.GetILGenerator();
                ICompileContext cc = new RunAndCollectContext(mb, tyb, ilg);

                EnvDesc envDesc = EnvDesc.FromSequence(MakeVarDesc(cc, method.Parameters, envSpec));

                method.Body.Compile(cc, envDesc, true);
            }

            Type tDone = tyb.CreateType();

            //ab.Save(assemblyNameSymbol.ToString() + ".dll");
            //Console.WriteLine("Saved: " + assemblyNameSymbol.ToString() + ".dll");

            for (int i = 0; i < iEnd; ++i)
            {
                MethodToBuild method = methodList[i];
                results.Add(Delegate.CreateDelegate(method.DelegateType, tDone.GetMethod(method.Name.ToString())));
            }
            return results;
        }

        public static IExpression ForLoopInt(Symbol var, int start, int pastEnd, IExpression body)
        {
            return new LetExpr
            (
                new LetClause[]
                {
                    new LetClause(var, typeof(int), new LiteralExpr(start))
                },
                new BeginWhileRepeatExpr
                (
                    new EmptyExpr(),
                    new ComparisonExpr(Comparison.LessThan, new VarRefExpr(var), new LiteralExpr(pastEnd)),
                    new BeginExpr
                    (
                        new IExpression[]
                        {
                            body,
                            new VarSetExpr(var, new BinaryOpExpr(BinaryOp.Add, new VarRefExpr(var), new LiteralExpr(1)))
                        }
                    )
                )
            );
        }

        public static IExpression ForLoopInt(Symbol var, int start, IExpression pastEnd, IExpression body)
        {
            Symbol varEnd = new Symbol();

            return new LetExpr
            (
                new LetClause[]
                {
                    new LetClause(var, typeof(int), new LiteralExpr(start)),
                    new LetClause(varEnd, typeof(int), pastEnd)
                },
                new BeginWhileRepeatExpr
                (
                    new EmptyExpr(),
                    new ComparisonExpr(Comparison.LessThan, new VarRefExpr(var), new VarRefExpr(varEnd)),
                    new BeginExpr
                    (
                        new IExpression[]
                        {
                            body,
                            new VarSetExpr(var, new BinaryOpExpr(BinaryOp.Add, new VarRefExpr(var), new LiteralExpr(1)))
                        }
                    )
                )
            );
        }

        public static void DefineMethod(this TypeBuilder t, ModuleBuilder mb, string name, MethodAttributes methodAttributes, Type returnType, ParamInfo[] parameters, IExpression body)
        {
            EnvSpec envSpec = body.GetEnvSpec();

            MethodBuilder meb = t.DefineMethod(name, methodAttributes, returnType, parameters.Select(x => x.ParamType).ToArray());

            if (!methodAttributes.HasFlag(MethodAttributes.Static))
            {
                if (parameters.Length == 0) throw new PascalesqueException("DefineMethod requires a \"this\" parameter for instance methods");

                if (parameters[0].ParamType != t) throw new PascalesqueException("DefineMethod requires a \"this\" parameter for instance methods");
            }

            ILGenerator ilg = meb.GetILGenerator();
            ICompileContext cc = new RunAndCollectContext(mb, t, ilg);

            EnvDesc envDesc = EnvDesc.FromSequence(MakeVarDesc(cc, parameters, envSpec));

            body.Compile(cc, envDesc, true);
        }
    }
}
