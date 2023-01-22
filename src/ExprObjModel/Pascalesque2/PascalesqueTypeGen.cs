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
    public abstract class ElementOfClass
    {
        public abstract void DefineSymbols(SymbolTable s, TypeKey owner);
        public abstract void AddCompileSteps(SymbolTable s, TypeKey owner, Action<ICompileStep> add);
    }

    public class ConstructorToBuild : ElementOfClass
    {
        private MethodAttributes attributes;
        private LambdaExpr2 body;

        public ConstructorToBuild(MethodAttributes attributes, LambdaExpr2 body)
        {
            this.attributes = attributes;
            this.body = body;
        }

        public override void DefineSymbols(SymbolTable s, TypeKey owner)
        {
            ConstructorKey ck = new ConstructorKey(owner, body.ParamInfos.Skip(1).Select(x => x.ParamType));
            ConstructorAux a = new ConstructorAux(attributes);
            s[ck] = a;
        }

        private class MakeConstructor : ICompileStep
        {
            private ConstructorToBuild parent;
            private TypeKey owner;
            private ConstructorKey constructorKey;

            public MakeConstructor(ConstructorToBuild parent, TypeKey owner, ConstructorKey constructorKey)
            {
                this.parent = parent;
                this.owner = owner;
                this.constructorKey = constructorKey;
            }

            public int Phase { get { return 1; } }

            public HashSet2<ItemKey> Inputs
            {
                get
                {
                    return HashSet2<ItemKey>.Singleton(owner) |
                        parent.body.ParamInfos.Select(x => x.ParamType.GetReferences()).HashSet2Union();
                }
            }

            public HashSet2<ItemKey> Outputs
            {
                get
                {
                    return HashSet2<ItemKey>.Singleton(constructorKey);
                }
            }

            public void Compile(ModuleBuilder mb, Dictionary<ItemKey, SaBox<object>> vars)
            {
                TypeBuilder oType = (TypeBuilder)(vars[owner].Value);

                ConstructorBuilder cb = oType.DefineConstructor(parent.attributes, CallingConventions.Standard, parent.body.ParamInfos.Skip(1).Select(x => (Type)(x.ParamType.Resolve(vars))).ToArray());

                vars[constructorKey].Value = cb;
            }
        }

        private class MakeConstructorBody : ICompileStep
        {
            private ConstructorToBuild parent;
            private SymbolTable symbolTable;
            private TypeKey owner;
            private ConstructorKey constructorKey;
            private EnvDesc2 envDesc;

            public MakeConstructorBody(ConstructorToBuild parent, SymbolTable symbolTable, TypeKey owner, ConstructorKey constructorKey, EnvDesc2 envDesc)
            {
                this.parent = parent;
                this.symbolTable = symbolTable;
                this.owner = owner;
                this.constructorKey = constructorKey;
                this.envDesc = envDesc;
            }

            public int Phase { get { return 1; } }

            public HashSet2<ItemKey> Inputs
            {
                get
                {
                    return HashSet2<ItemKey>.Singleton(owner) |
                        parent.body.ParamInfos.Select(x => x.ParamType.GetReferences()).HashSet2Union() |
                        HashSet2<ItemKey>.Singleton(constructorKey) |
                        parent.body.Body.GetReferences(symbolTable, owner, envDesc.TypesOnly());
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
                TypeBuilder tb = (TypeBuilder)(vars[owner].Value);
                ConstructorBuilder cb = (ConstructorBuilder)(vars[constructorKey].Value);
                ILGenerator ilg = cb.GetILGenerator();
                CompileContext2 cc = new CompileContext2(mb, tb, ilg, true);
                parent.body.Body.Compile(symbolTable, owner, cc, envDesc, vars, true);
            }
        }

        public override void AddCompileSteps(SymbolTable s, TypeKey owner, Action<ICompileStep> add)
        {
            EnvSpec es = body.Body.GetEnvSpec();

            List<ParamInfo2> paramInfos = body.ParamInfos.ToList();
            List<Tuple<Symbol, IVarDesc2>> vars = new List<Tuple<Symbol,IVarDesc2>>();

            if (paramInfos.Count == 0) throw new PascalesqueException("Constructor must at least take a \"this\" parameter");

            if (paramInfos[0].ParamType != new TypeKeyReference(owner)) throw new PascalesqueException("A constructor's \"this\" parameter is not of the correct type");

            int iEnd = paramInfos.Count;
            for (int i = 0; i < iEnd; ++i)
            {
                ParamInfo2 x = paramInfos[i];
                vars.Add(new Tuple<Symbol, IVarDesc2>(x.Name, new ArgVarDesc2(x.ParamType, es[x.Name].IsCaptured, i)));
            }

            EnvDesc2 e = EnvDesc2.FromSequence(vars);

            ConstructorKey ck = new ConstructorKey(owner, paramInfos.Skip(1).Select(x => x.ParamType));

            body.Body.AddCompileSteps(s, owner, e.TypesOnly(), add);
            add(new MakeConstructor(this, owner, ck));
            add(new MakeConstructorBody(this, s, owner, ck, e));
        }
    }

    public class MethodToBuild : ElementOfClass
    {
        private Symbol name;
        private MethodAttributes attributes;
        private TypeReference returnType;
        private LambdaExpr2 body;

        public MethodToBuild(Symbol name, MethodAttributes attributes, TypeReference returnType, LambdaExpr2 body)
        {
            this.name = name;
            this.attributes = attributes;
            this.returnType = returnType;
            this.body = body;
        }

        public override void DefineSymbols(SymbolTable s, TypeKey owner)
        {
            bool isInstance = !attributes.HasFlag(MethodAttributes.Static);
            MethodKey mk = new MethodKey(owner, name, isInstance, body.ParamInfos.Select(x => x.ParamType).Skip(isInstance ? 1 : 0));
            MethodAux ma = new MethodAux(attributes, returnType);
            s[mk] = ma;
        }

        private class MakeMethod : ICompileStep
        {
            private MethodToBuild parent;
            private SymbolTable symbolTable;
            private TypeKey owner;
            private MethodKey methodKey;

            public MakeMethod(MethodToBuild parent, SymbolTable symbolTable, TypeKey owner, MethodKey methodKey)
            {
                this.parent = parent;
                this.symbolTable = symbolTable;
                this.owner = owner;
                this.methodKey = methodKey;
            }

            public int Phase { get { return 1; } }

            public HashSet2<ItemKey> Inputs
            {
                get
                {
                    return HashSet2<ItemKey>.Singleton(owner) |
                        parent.body.ParamInfos.Select(x => x.ParamType.GetReferences()).HashSet2Union() |
                        symbolTable[methodKey].ReturnType.GetReferences();
                }
            }

            public HashSet2<ItemKey> Outputs
            {
                get
                {
                    return HashSet2<ItemKey>.Singleton(methodKey);
                }
            }

            public void Compile(ModuleBuilder mb, Dictionary<ItemKey, SaBox<object>> vars)
            {
                TypeBuilder oType = (TypeBuilder)(vars[owner].Value);

                MethodBuilder meb = oType.DefineMethod(parent.name.Name, parent.attributes, symbolTable[methodKey].ReturnType.Resolve(vars), methodKey.Parameters.Select(x => x.Resolve(vars)).ToArray());

                vars[methodKey].Value = meb;
            }
        }

        private class MakeMethodBody : ICompileStep
        {
            private MethodToBuild parent;
            private SymbolTable symbolTable;
            private TypeKey owner;
            private MethodKey methodKey;
            private EnvDesc2 envDesc;

            public MakeMethodBody(MethodToBuild parent, SymbolTable symbolTable, TypeKey owner, MethodKey methodKey, EnvDesc2 envDesc)
            {
                this.parent = parent;
                this.symbolTable = symbolTable;
                this.owner = owner;
                this.methodKey = methodKey;
                this.envDesc = envDesc;
            }

            public int Phase { get { return 1; } }

            public HashSet2<ItemKey> Inputs
            {
                get
                {
                    return HashSet2<ItemKey>.Singleton(owner) |
                        parent.body.ParamInfos.Select(x => x.ParamType.GetReferences()).HashSet2Union() | 
                        HashSet2<ItemKey>.Singleton(methodKey) |
                        parent.body.Body.GetReferences(symbolTable, owner, envDesc.TypesOnly());
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
                TypeBuilder tb = (TypeBuilder)(vars[owner].Value);
                MethodBuilder meb = (MethodBuilder)(vars[methodKey].Value);
                ILGenerator ilg = meb.GetILGenerator();
                CompileContext2 cc = new CompileContext2(mb, tb, ilg, false);
                parent.body.Body.Compile(symbolTable, owner, cc, envDesc, vars, true);
            }
        }

        public override void AddCompileSteps(SymbolTable s, TypeKey owner, Action<ICompileStep> add)
        {
            EnvSpec es = body.Body.GetEnvSpec();

            List<ParamInfo2> paramInfos = body.ParamInfos.ToList();
            List<Tuple<Symbol, IVarDesc2>> vars = new List<Tuple<Symbol, IVarDesc2>>();

            if (!attributes.HasFlag(MethodAttributes.Static))
            {
                if (paramInfos.Count == 0) throw new PascalesqueException("An instance method must at least take a \"this\" parameter");

                if (paramInfos[0].ParamType != new TypeKeyReference(owner)) throw new PascalesqueException("An instance method's \"this\" parameter is not of the correct type");
            }

            int iEnd = paramInfos.Count;
            for (int i = 0; i < iEnd; ++i)
            {
                ParamInfo2 x = paramInfos[i];
                vars.Add(new Tuple<Symbol, IVarDesc2>(x.Name, new ArgVarDesc2(x.ParamType, es[x.Name].IsCaptured, i)));
            }

            EnvDesc2 e = EnvDesc2.FromSequence(vars);

            TypeReference returnType2 = body.Body.GetReturnType(s, e.TypesOnly());

            if (returnType != returnType2) throw new PascalesqueException("Return type of method does not match");

            bool isInstance = !attributes.HasFlag(MethodAttributes.Static);
            MethodKey mk = new MethodKey(owner, name, isInstance, paramInfos.Select(x => x.ParamType).Skip(isInstance ? 1 : 0));

            body.Body.AddCompileSteps(s, owner, e.TypesOnly(), add);
            add(new MakeMethod(this, s, owner, mk));
            add(new MakeMethodBody(this, s, owner, mk, e));
        }
    }

    public class MethodOverrideToBuild : ElementOfClass
    {
        private MethodReference methodToOverride;
        private Symbol name;
        private MethodAttributes attributes;
        private TypeReference returnType;
        private LambdaExpr2 body;

        public MethodOverrideToBuild(MethodReference methodToOverride, MethodAttributes attributes, TypeReference returnType, LambdaExpr2 body)
        {
            this.methodToOverride = methodToOverride;
            this.name = new Symbol();
            this.attributes = attributes;
            this.returnType = returnType;
            this.body = body;
        }

        public override void DefineSymbols(SymbolTable s, TypeKey owner)
        {
            bool isInstance = !attributes.HasFlag(MethodAttributes.Static);
            MethodKey mk = new MethodKey(owner, name, isInstance, body.ParamInfos.Select(x => x.ParamType).Skip(isInstance ? 1 : 0));
            MethodAux ma = new MethodAux(attributes, returnType);
            s[mk] = ma;
        }

        private class MakeMethodOverride : ICompileStep
        {
            private MethodOverrideToBuild parent;
            private SymbolTable symbolTable;
            private TypeKey owner;
            private MethodKey methodKey;

            public MakeMethodOverride(MethodOverrideToBuild parent, SymbolTable symbolTable, TypeKey owner, MethodKey methodKey)
            {
                this.parent = parent;
                this.symbolTable = symbolTable;
                this.owner = owner;
                this.methodKey = methodKey;
            }

            public int Phase { get { return 1; } }

            public HashSet2<ItemKey> Inputs
            {
                get
                {
                    return HashSet2<ItemKey>.Singleton(owner) |
                        parent.body.ParamInfos.Select(x => x.ParamType.GetReferences()).HashSet2Union() |
                        symbolTable[methodKey].ReturnType.GetReferences();
                }
            }

            public HashSet2<ItemKey> Outputs
            {
                get
                {
                    return HashSet2<ItemKey>.Singleton(methodKey);
                }
            }

            public void Compile(ModuleBuilder mb, Dictionary<ItemKey, SaBox<object>> vars)
            {
                TypeBuilder oType = (TypeBuilder)(vars[owner].Value);

                MethodBuilder meb = oType.DefineMethod(parent.name.Name, parent.attributes, symbolTable[methodKey].ReturnType.Resolve(vars), methodKey.Parameters.Select(x => x.Resolve(vars)).ToArray());

                vars[methodKey].Value = meb;
            }
        }

        private class MakeMethodOverrideBody : ICompileStep
        {
            private MethodOverrideToBuild parent;
            private SymbolTable symbolTable;
            private TypeKey owner;
            private MethodKey methodKey;
            private EnvDesc2 envDesc;

            public MakeMethodOverrideBody(MethodOverrideToBuild parent, SymbolTable symbolTable, TypeKey owner, MethodKey methodKey, EnvDesc2 envDesc)
            {
                this.parent = parent;
                this.symbolTable = symbolTable;
                this.owner = owner;
                this.methodKey = methodKey;
                this.envDesc = envDesc;
            }

            public int Phase { get { return 1; } }

            public HashSet2<ItemKey> Inputs
            {
                get
                {
                    return HashSet2<ItemKey>.Singleton(owner) |
                        parent.body.ParamInfos.Select(x => x.ParamType.GetReferences()).HashSet2Union() | 
                        HashSet2<ItemKey>.Singleton(methodKey) |
                        parent.body.Body.GetReferences(symbolTable, owner, envDesc.TypesOnly());
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
                TypeBuilder tb = (TypeBuilder)(vars[owner].Value);
                MethodBuilder meb = (MethodBuilder)(vars[methodKey].Value);
                ILGenerator ilg = meb.GetILGenerator();
                CompileContext2 cc = new CompileContext2(mb, tb, ilg, false);
                parent.body.Body.Compile(symbolTable, owner, cc, envDesc, vars, true);
            }
        }

        private class DefineMethodOverride : ICompileStep
        {
            private MethodOverrideToBuild parent;
            private SymbolTable symbolTable;
            private TypeKey owner;
            private MethodKey methodKey;

            public DefineMethodOverride(MethodOverrideToBuild parent, SymbolTable symbolTable, TypeKey owner, MethodKey methodKey)
            {
                this.parent = parent;
                this.symbolTable = symbolTable;
                this.owner = owner;
                this.methodKey = methodKey;
            }

            public int Phase { get { return 1; } }

            public HashSet2<ItemKey> Inputs
            {
                get
                {
                    return HashSet2<ItemKey>.Singleton(methodKey) | parent.methodToOverride.GetReferences();
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
                TypeBuilder tb = (TypeBuilder)(vars[owner].Value);
                MethodBuilder meb = (MethodBuilder)(vars[methodKey].Value);
                MethodInfo mi = parent.methodToOverride.Resolve(vars);

                tb.DefineMethodOverride(meb, mi);
            }
        }

        public override void AddCompileSteps(SymbolTable s, TypeKey owner, Action<ICompileStep> add)
        {
            EnvSpec es = body.Body.GetEnvSpec();

            List<ParamInfo2> paramInfos = body.ParamInfos.ToList();
            List<Tuple<Symbol, IVarDesc2>> vars = new List<Tuple<Symbol, IVarDesc2>>();

            if (!attributes.HasFlag(MethodAttributes.Static))
            {
                if (paramInfos.Count == 0) throw new PascalesqueException("An instance method must at least take a \"this\" parameter");

                if (paramInfos[0].ParamType != new TypeKeyReference(owner)) throw new PascalesqueException("An instance method's \"this\" parameter is not of the correct type");
            }

            int iEnd = paramInfos.Count;
            for (int i = 0; i < iEnd; ++i)
            {
                ParamInfo2 x = paramInfos[i];
                vars.Add(new Tuple<Symbol, IVarDesc2>(x.Name, new ArgVarDesc2(x.ParamType, es[x.Name].IsCaptured, i)));
            }

            EnvDesc2 e = EnvDesc2.FromSequence(vars);

            TypeReference returnType2 = body.Body.GetReturnType(s, e.TypesOnly());

            if (returnType != returnType2) throw new PascalesqueException("Return type of method does not match");

            bool isInstance = !attributes.HasFlag(MethodAttributes.Static);
            MethodKey mk = new MethodKey(owner, name, isInstance, paramInfos.Select(x => x.ParamType).Skip(isInstance ? 1 : 0));

            body.Body.AddCompileSteps(s, owner, e.TypesOnly(), add);
            add(new MakeMethodOverride(this, s, owner, mk));
            add(new MakeMethodOverrideBody(this, s, owner, mk, e));
            add(new DefineMethodOverride(this, s, owner, mk));
        }
    }

    public class AbstractMethodToBuild : ElementOfClass
    {
        private Symbol name;
        private MethodAttributes attributes;
        private TypeReference returnType;
        private List<TypeReference> parameterTypes;

        public AbstractMethodToBuild(Symbol name, MethodAttributes attributes, TypeReference returnType, IEnumerable<TypeReference> parameterTypes)
        {
            if (!(attributes.HasFlag(MethodAttributes.Abstract))) throw new ArgumentException("Abstract attribute must be set");

            this.name = name;
            this.attributes = attributes;
            this.returnType = returnType;
            this.parameterTypes = parameterTypes.ToList();
        }

        public override void DefineSymbols(SymbolTable s, TypeKey owner)
        {
            MethodKey m = new MethodKey(owner, name, true, parameterTypes);
            s[m] = new MethodAux(attributes, returnType);
        }

        private class MakeAbstractMethod : ICompileStep
        {
            private AbstractMethodToBuild parent;
            private SymbolTable symbolTable;
            private TypeKey owner;
            private MethodKey methodKey;

            public MakeAbstractMethod(AbstractMethodToBuild parent, SymbolTable symbolTable, TypeKey owner, MethodKey methodKey)
            {
                this.parent = parent;
                this.symbolTable = symbolTable;
                this.owner = owner;
                this.methodKey = methodKey;
            }

            public int Phase { get { return 1; } }

            public HashSet2<ItemKey> Inputs
            {
                get
                {
                    return HashSet2<ItemKey>.Singleton(owner) |
                        parent.parameterTypes.Select(x => x.GetReferences()).HashSet2Union() |
                        symbolTable[methodKey].ReturnType.GetReferences();
                }
            }

            public HashSet2<ItemKey> Outputs
            {
                get
                {
                    return HashSet2<ItemKey>.Singleton(methodKey);
                }
            }

            public void Compile(ModuleBuilder mb, Dictionary<ItemKey, SaBox<object>> vars)
            {
                TypeBuilder oType = (TypeBuilder)(vars[owner].Value);

                MethodBuilder meb = oType.DefineMethod(parent.name.Name, parent.attributes, symbolTable[methodKey].ReturnType.Resolve(vars), methodKey.Parameters.Select(x => x.Resolve(vars)).ToArray());

                vars[methodKey].Value = meb;
            }
        }

        public override void AddCompileSteps(SymbolTable s, TypeKey owner, Action<ICompileStep> add)
        {
            MethodKey mk = new MethodKey(owner, name, true, parameterTypes);
            add(new MakeAbstractMethod(this, s, owner, mk));
        }
    }

    public class FieldToBuild : ElementOfClass
    {
        private FieldAttributes attributes;
        private TypeReference fieldType;
        private Symbol name;

        public FieldToBuild(FieldAttributes attributes, TypeReference fieldType, Symbol name)
        {
            this.attributes = attributes;
            this.fieldType = fieldType;
            this.name = name;
        }

        public override void DefineSymbols(SymbolTable s, TypeKey owner)
        {
            FieldKey fk = new FieldKey(owner, name, fieldType);
            FieldAux fa = new FieldAux();
            s[fk] = fa;
        }

        private class MakeField : ICompileStep
        {
            private TypeKey owner;
            private FieldToBuild parent;
            private FieldKey fieldKey;

            public MakeField(TypeKey owner, FieldToBuild parent)
            {
                this.owner = owner;
                this.parent = parent;
                this.fieldKey = new FieldKey(owner, parent.name, parent.fieldType);
            }

            #region ICompileStep Members

            public int Phase
            {
                get { return 1;  }
            }

            public HashSet2<ItemKey> Inputs
            {
                get { return owner | parent.fieldType.GetReferences(); }
            }

            public HashSet2<ItemKey> Outputs
            {
                get { return HashSet2<ItemKey>.Singleton(fieldKey); }
            }

            public void Compile(ModuleBuilder mb, Dictionary<ItemKey, SaBox<object>> vars)
            {
                TypeBuilder t = (TypeBuilder)(vars[owner].Value);
                FieldBuilder fb = t.DefineField(parent.name.Name, parent.fieldType.Resolve(vars), parent.attributes);
                vars[fieldKey].Value = fb;
            }

            #endregion
        }

        public override void AddCompileSteps(SymbolTable s, TypeKey owner, Action<ICompileStep> add)
        {
            add(new MakeField(owner, this));
        }
    }

    public class PropertyToBuild : ElementOfClass
    {
        private PropertyAttributes attributes;
        private TypeReference propertyType;
        private Symbol name;
        private List<TypeReference> propertyArgs;
        private MethodKey getter;
        private MethodKey setter;

        public PropertyToBuild(PropertyAttributes attributes, TypeReference propertyType, Symbol name, IEnumerable<TypeReference> propertyArgs, MethodKey getter, MethodKey setter)
        {
            this.attributes = attributes;
            this.propertyType = propertyType;
            this.name = name;
            this.propertyArgs = propertyArgs.ToList();
            this.getter = getter;
            this.setter = setter;
        }

        public override void DefineSymbols(SymbolTable s, TypeKey owner)
        {
            PropertyKey p = new PropertyKey(owner, name, propertyType, propertyArgs);
            s[p] = new PropertyAux();
        }

        private class MakeProperty : ICompileStep
        {
            private TypeKey owner;
            private PropertyToBuild parent;
            private PropertyKey propertyKey;

            public MakeProperty(TypeKey owner, PropertyToBuild parent, PropertyKey propertyKey)
            {
                this.owner = owner;
                this.parent = parent;
                this.propertyKey = propertyKey;
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
                    return parent.propertyType.GetReferences() |
                        owner |
                        parent.propertyArgs.Select(x => x.GetReferences()).HashSet2Union() |
                        ((parent.getter != null) ? HashSet2<ItemKey>.Singleton(parent.getter) : HashSet2<ItemKey>.Empty) |
                        ((parent.setter != null) ? HashSet2<ItemKey>.Singleton(parent.setter) : HashSet2<ItemKey>.Empty);
                }
            }

            public HashSet2<ItemKey> Outputs
            {
                get
                {
                    return HashSet2<ItemKey>.Singleton(propertyKey);
                }
            }

            public void Compile(ModuleBuilder mb, Dictionary<ItemKey, SaBox<object>> vars)
            {
                TypeBuilder t = (TypeBuilder)(vars[owner].Value);
                PropertyBuilder p = t.DefineProperty
                (
                    parent.name.Name,
                    parent.attributes,
                    parent.propertyType.Resolve(vars),
                    parent.propertyArgs.Select(x => x.Resolve(vars)).ToArray()
                );
                if (parent.getter != null)
                {
                    MethodBuilder m = (MethodBuilder)(vars[parent.getter].Value);
                    p.SetGetMethod(m);
                }
                if (parent.setter != null)
                {
                    MethodBuilder m = (MethodBuilder)(vars[parent.setter].Value);
                    p.SetSetMethod(m);
                }
            }

            #endregion
        }

        public override void AddCompileSteps(SymbolTable s, TypeKey owner, Action<ICompileStep> add)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class ElementOfModule
    {
        public abstract void DefineSymbols(SymbolTable symbolTable);
        public abstract void AddCompileSteps(SymbolTable symbolTable, Action<ICompileStep> add);
    }

    public class ClassToBuild : ElementOfModule
    {
        private Symbol name;
        private TypeAttributes attributes;
        private TypeReference ancestor;
        private List<TypeReference> interfaces;
        private List<ElementOfClass> elements;

        public ClassToBuild(Symbol name, TypeAttributes attributes, TypeReference ancestor, IEnumerable<TypeReference> interfaces, IEnumerable<ElementOfClass> elements)
        {
            this.name = name;
            this.attributes = attributes;
            this.ancestor = ancestor;
            this.interfaces = interfaces.ToList();
            this.elements = elements.ToList();
        }

        private class MakeClass : ICompileStep
        {
            private ClassToBuild parent;
            private TypeKey classKey;

            public MakeClass(ClassToBuild parent)
            {
                this.parent = parent;
                this.classKey = new TypeKey(parent.name);
            }

            #region ICompileStep Members

            public int Phase
            {
                get { return 1; }
            }

            public HashSet2<ItemKey> Inputs
            {
                get { return parent.ancestor.GetReferences() | parent.interfaces.Select(x => x.GetReferences()).HashSet2Union(); }
            }

            public HashSet2<ItemKey> Outputs
            {
                get { return HashSet2<ItemKey>.Singleton(classKey); }
            }

            public void Compile(ModuleBuilder mb, Dictionary<ItemKey, SaBox<object>> vars)
            {
                if (parent.ancestor == null)
                {
                    Type[] interfaces = parent.interfaces.Select(x => x.Resolve(vars)).ToArray();
                    TypeBuilder tb = mb.DefineType(parent.name.Name, parent.attributes, null, interfaces);
                }
                else
                {
                    Type ancestor = parent.ancestor.Resolve(vars);
                    Type[] interfaces = parent.interfaces.Select(x => x.Resolve(vars)).ToArray();
                    TypeBuilder tb = mb.DefineType(parent.name.Name, parent.attributes, ancestor, interfaces);
                    vars[classKey].Value = tb;
                }
            }

            #endregion
        }

        private class BakeClass : ICompileStep
        {
            private ClassToBuild parent;
            private TypeKey classKey;
            private CompletedTypeKey completedClassKey;

            public BakeClass(ClassToBuild parent)
            {
                this.parent = parent;
                this.classKey = new TypeKey(parent.name);
                this.completedClassKey = new CompletedTypeKey(parent.name);
            }

            #region ICompileStep Members

            public int Phase
            {
                get { return 2; }
            }

            public HashSet2<ItemKey> Inputs
            {
                get { return HashSet2<ItemKey>.Singleton(classKey); }
            }

            public HashSet2<ItemKey> Outputs
            {
                get { return HashSet2<ItemKey>.Singleton(completedClassKey); }
            }

            public void Compile(ModuleBuilder mb, Dictionary<ItemKey, SaBox<object>> vars)
            {
                TypeBuilder tb = (TypeBuilder)(vars[classKey].Value);
                Type t = tb.CreateType();
                vars[completedClassKey].Value = t;
            }

            #endregion
        }

        public override void DefineSymbols(SymbolTable s)
        {
            TypeKey typeKey = new TypeKey(name);
            TypeAux a = new TypeAux(false, false);
            s[typeKey] = a;
            foreach (ElementOfClass element in elements)
            {
                element.DefineSymbols(s, typeKey);
            }
        }

        public override void AddCompileSteps(SymbolTable s, Action<ICompileStep> add)
        {
            add(new MakeClass(this));
            add(new BakeClass(this));

            foreach (ElementOfClass element in elements)
            {
                element.AddCompileSteps(s, new TypeKey(name), add);
            }
        }
    }

    public class ModuleToBuild
    {
        private List<ElementOfModule> elements;

        public ModuleToBuild(IEnumerable<ElementOfModule> elements)
        {
            this.elements = elements.ToList();
        }

        public void DefineSymbols(SymbolTable symbolTable)
        {
            foreach (ElementOfModule element in elements)
            {
                element.DefineSymbols(symbolTable);
            }
        }

        public void AddCompileSteps(SymbolTable symbolTable, Action<ICompileStep> add)
        {
            foreach (ElementOfModule element in elements)
            {
                element.AddCompileSteps(symbolTable, add);
            }
        }
    }

    public static partial class Utils
    {
        public static void Add(this IHashGenerator hg, MemberInfo mi)
        {
            hg.Add(BitConverter.GetBytes(mi.Module.MetadataToken));
            hg.Add(BitConverter.GetBytes(mi.MetadataToken));
        }

        public static void Add(this IHashGenerator hg, int i)
        {
            byte[] b = BitConverter.GetBytes(i);
            hg.Add(b, 0, 4);
        }

        public static HashSet2<T> HashSet2Union<T>(this IEnumerable<HashSet2<T>> sets)
        {
            HashSet2<T> r = HashSet2<T>.Empty;
            foreach (HashSet2<T> s in sets)
            {
                r |= s;
            }
            return r;
        }

        public static ModuleToBuild GetTestModule()
        {
            return new ModuleToBuild
            (
                new ElementOfModule[]
                {
                    new ClassToBuild
                    (
                        new Symbol("MessagePrinter"),
                        TypeAttributes.Public,
                        ExistingTypeReference.Object,
                        new TypeReference[] { },
                        new ElementOfClass[]
                        {
                            new FieldToBuild
                            (
                                FieldAttributes.Private | FieldAttributes.InitOnly,
                                ExistingTypeReference.String,
                                new Symbol("message")
                            ),
                            new ConstructorToBuild
                            (
                                MethodAttributes.Public,
                                new LambdaExpr2
                                (
                                    new ParamInfo2[]
                                    {
                                        new ParamInfo2
                                        (
                                            new Symbol("this"),
                                            new TypeKeyReference(new TypeKey(new Symbol("MessagePrinter")))
                                        ),
                                        new ParamInfo2
                                        (
                                            new Symbol("message"),
                                            ExistingTypeReference.String
                                        )
                                    },
                                    new BeginExpr2
                                    (
                                        new IExpression2[]
                                        {
                                            new ConstructorCallExpr2
                                            (
                                                new ExistingConstructorReference
                                                (
                                                    typeof(object).GetConstructor(Type.EmptyTypes)
                                                ),
                                                new VarRefExpr2
                                                (
                                                    new Symbol("this")
                                                ),
                                                new IExpression2[] { }
                                            ),
                                            new FieldSetExpr2
                                            (
                                                new VarRefExpr2(new Symbol("this")),
                                                new FieldKeyReference(new FieldKey(new TypeKey(new Symbol("MessagePrinter")), new Symbol("message"), ExistingTypeReference.String)),
                                                new VarRefExpr2(new Symbol("message"))
                                            )
                                        }
                                    )
                                )
                            ),
                            new MethodToBuild
                            (
                                new Symbol("Print"),
                                MethodAttributes.Public,
                                ExistingTypeReference.Void,
                                new LambdaExpr2
                                (
                                    new ParamInfo2[]
                                    {
                                        new ParamInfo2
                                        (
                                            new Symbol("this"),
                                            new TypeKeyReference(new TypeKey(new Symbol("MessagePrinter")))
                                        )
                                    },
                                    new MethodCallExpr2
                                    (
                                        new ExistingMethodReference(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) })),
                                        false,
                                        new IExpression2[]
                                        {
                                            new FieldRefExpr2
                                            (
                                                new VarRefExpr2(new Symbol("this")),
                                                new FieldKeyReference(new FieldKey(new TypeKey(new Symbol("MessagePrinter")), new Symbol("message"), ExistingTypeReference.String))
                                            )
                                        }
                                    )
                                )
                            )
                        }
                    ),
                    new ClassToBuild
                    (
                        new Symbol("Program"),
                        TypeAttributes.Public,
                        ExistingTypeReference.Object,
                        new TypeReference[] { },
                        new ElementOfClass[]
                        {
                            new MethodToBuild
                            (
                                new Symbol("Run"),
                                MethodAttributes.Static | MethodAttributes.Public,
                                ExistingTypeReference.Void,
                                new LambdaExpr2
                                (
                                    new ParamInfo2[] { },
                                    new LetExpr2
                                    (
                                        new LetClause2[]
                                        {
                                            new LetClause2
                                            (
                                                new Symbol("a"),
                                                new TypeKeyReference(new TypeKey(new Symbol("MessagePrinter"))),
                                                new NewObjExpr2
                                                (
                                                    new ConstructorKeyReference
                                                    (
                                                        new ConstructorKey
                                                        (
                                                            new TypeKey(new Symbol("MessagePrinter")),
                                                            new TypeReference[]
                                                            {
                                                                ExistingTypeReference.String
                                                            }
                                                        )
                                                    ),
                                                    new IExpression2[]
                                                    {
                                                        new LiteralExpr2("Hello, world!")
                                                    }
                                                )
                                            ),
                                            new LetClause2
                                            (
                                                new Symbol("b"),
                                                new TypeKeyReference(new TypeKey(new Symbol("MessagePrinter"))),
                                                new NewObjExpr2
                                                (
                                                    new ConstructorKeyReference
                                                    (
                                                        new ConstructorKey
                                                        (
                                                            new TypeKey(new Symbol("MessagePrinter")),
                                                            new TypeReference[]
                                                            {
                                                                ExistingTypeReference.String
                                                            }
                                                        )
                                                    ),
                                                    new IExpression2[]
                                                    {
                                                        new LiteralExpr2("This is a test!")
                                                    }
                                                )
                                            )
                                        },
                                        new BeginExpr2
                                        (
                                            new IExpression2[]
                                            {
                                                new MethodCallExpr2
                                                (
                                                    new MethodKeyReference
                                                    (
                                                        new MethodKey
                                                        (
                                                            new TypeKey(new Symbol("MessagePrinter")),
                                                            new Symbol("Print"),
                                                            true,
                                                            new TypeReference[]
                                                            {
                                                            }
                                                        )
                                                    ),
                                                    true,
                                                    new IExpression2[]
                                                    {
                                                        new VarRefExpr2(new Symbol("a"))
                                                    }
                                                ),
                                                new MethodCallExpr2
                                                (
                                                    new MethodKeyReference
                                                    (
                                                        new MethodKey
                                                        (
                                                            new TypeKey(new Symbol("MessagePrinter")),
                                                            new Symbol("Print"),
                                                            true,
                                                            new TypeReference[]
                                                            {
                                                            }
                                                        )
                                                    ),
                                                    true,
                                                    new IExpression2[]
                                                    {
                                                        new VarRefExpr2(new Symbol("b"))
                                                    }
                                                )
                                            }
                                        )
                                    )
                                )
                            )
                        }
                    )
                }
            );                          
        }

        public static IEnumerable<T> TopologicalSort<T>(IEnumerable<T> items, Func<T, HashSet2<T>> getParents)
        {
            List<T> results = new List<T>();
            List<T> items1 = items.ToList();
            List<T> items2 = new List<T>();
            HashSet2<T> inResults = HashSet2<T>.Empty;

            while (items1.Count > 0)
            {
                foreach (T item in items1)
                {
                    HashSet2<T> h1 = getParents(item);
                    h1 = h1 - inResults;
                    if (h1.IsEmpty)
                    {
                        results.Add(item);
                        inResults |= item;
                    }
                    else
                    {
                        items2.Add(item);
                    }
                }

                if (items2.Count == items1.Count) throw new Exception("Topological Sort: cycles detected");

                items1 = items2;
                items2 = new List<T>();
            }

            return results.AsEnumerable();
        }

        public static Dictionary<ItemKey, Type> Compile(ModuleBuilder mb, ModuleToBuild mtb)
        {
            SymbolTable s = new SymbolTable();
            mtb.DefineSymbols(s);

            List<ICompileStep> steps = new List<ICompileStep>();

            mtb.AddCompileSteps(s, x => steps.Add(x));

            HashSet2<ItemKey> allItemKeys = steps.Select(x => x.Inputs).HashSet2Union() | steps.Select(x => x.Outputs).HashSet2Union();

            Dictionary<ItemKey, SaBox<object>> references = new Dictionary<ItemKey, SaBox<object>>();
            foreach (ItemKey ik in allItemKeys.Items) references.Add(ik, new SaBox<object>());

            Dictionary<ItemKey, HashSet2<int>> inputDict = new Dictionary<ItemKey, HashSet2<int>>();
            Dictionary<ItemKey, int> outputDict = new Dictionary<ItemKey, int>();

            int iEnd = steps.Count;
            for (int i = 0; i < iEnd; ++i)
            {
                if (steps[i].Phase == 1)
                {
                    foreach (ItemKey ik in steps[i].Inputs.Items)
                    {
                        if (inputDict.ContainsKey(ik))
                        {
                            inputDict[ik] |= i;
                        }
                        else
                        {
                            inputDict.Add(ik, HashSet2<int>.Singleton(i));
                        }
                    }
                    foreach (ItemKey ik in steps[i].Outputs.Items)
                    {
                        if (outputDict.ContainsKey(ik))
                        {
                            throw new PascalesqueException("Output conflict: " + ik + " is generated by two different steps");
                        }
                        else
                        {
                            outputDict.Add(ik, i);
                        }
                    }
                }
            }

            Func<int, HashSet2<int>> getParents = delegate(int i)
            {
                HashSet2<ItemKey> inputs = steps[i].Inputs;
                HashSet2<ItemKey> cantMake = HashSet2<ItemKey>.FromSequence(inputs.Items.Where(x => !outputDict.ContainsKey(x)));
                if (!(cantMake.IsEmpty))
                {
                    throw new PascalesqueException("Don't know how to make " + cantMake.Items.Select(x => x.ToString()).Concatenate(", "));
                }
                HashSet2<int> suppliers = HashSet2<int>.FromSequence(inputs.Items.Select(x => outputDict[x]));
                return suppliers;
            };

#if false
            foreach (int jj in Enumerable.Range(0, iEnd).Where(x => steps[x].Phase == 1))
            {
                Console.WriteLine("Step " + jj + " making " + steps[jj].Outputs.Items.Select(x => x.ToString()).Concatenate(", "));
                foreach (ItemKey ik in steps[jj].Inputs.Items)
                {
                    Console.WriteLine("  Requires " + ik + " made by step " + steps.Numbered().Where(x => x.Item2.Outputs.Contains(ik)).Select(x => x.Item1.ToString()).Concatenate(", "));
                }
            }
#endif

            List<int> phase1 = TopologicalSort<int>(Enumerable.Range(0, iEnd).Where(x => steps[x].Phase == 1), getParents).ToList();

            foreach (int i in phase1)
            {
                steps[i].Compile(mb, references);
            }

            HashSet2<ItemKey> phase1Results = steps.Where(x => x.Phase == 1).Select(x => x.Outputs).HashSet2Union();

            Func<int, HashSet2<int>> getParents2 = delegate(int i)
            {
                HashSet2<ItemKey> inputs = steps[i].Inputs - phase1Results;
                HashSet2<ItemKey> cantMake = HashSet2<ItemKey>.FromSequence(inputs.Items.Where(x => !outputDict.ContainsKey(x)));

                if (!(cantMake.IsEmpty))
                {
                    throw new PascalesqueException("Don't know how to make " + cantMake.Items.Select(x => x.ToString()).Concatenate(", "));
                }
                HashSet2<int> suppliers = HashSet2<int>.FromSequence(inputs.Items.Select(x => outputDict[x]));
                return suppliers;
            };

            List<int> phase2 = TopologicalSort<int>(Enumerable.Range(0, iEnd).Where(x => steps[x].Phase == 2), getParents2).ToList();

            foreach (int i in phase2)
            {
                steps[i].Compile(mb, references);
            }

            HashSet2<ItemKey> phase2Results = steps.Where(x => x.Phase == 2).Select(x => x.Outputs).HashSet2Union();

            return phase2Results.Items.ToDictionary(x => x, x => (Type)(references[x].Value));
        }

        [SchemeFunction("pascalesque-2-test")]
        public static object Pascalesque2Test()
        {
            AssemblyName assemblyName = new AssemblyName("Pascalesque2TestOutput.dll");
            AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
            ModuleBuilder mb = ab.DefineDynamicModule("Pascalesque2TestOutput.dll");

            Dictionary<ItemKey, Type> r = Compile(mb, GetTestModule());

            ab.Save("Pascalesque2TestOutput.dll");

            ItemKey programKey = new CompletedTypeKey(new Symbol("Program"));
            if (r.ContainsKey(programKey))
            {
                Type program = r[programKey];
                MethodInfo m = program.GetMethod("Run", Type.EmptyTypes);
                m.Invoke(null, null);
            }

            SchemeHashMap h = new SchemeHashMap();
            foreach (KeyValuePair<ItemKey, Type> kvp in r)
            {
                if (kvp.Key is CompletedTypeKey)
                {
                    h[((CompletedTypeKey)(kvp.Key)).Name] = kvp.Value;
                }
            }

            return h;
        }

        [SchemeFunction("generate-dll-2")]
        public static object GenerateDll2(string dllName, object desc)
        {
            AssemblyName assemblyName = new AssemblyName(dllName);
            AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
            ModuleBuilder mb = ab.DefineDynamicModule(dllName);

            ModuleToBuild mtb = Pascalesque.Two.Syntax.SyntaxAnalyzer.AnalyzeModule(desc);

            Dictionary<ItemKey, Type> r = Compile(mb, mtb);

            ab.Save(dllName);

            SchemeHashMap h = new SchemeHashMap();
            foreach (KeyValuePair<ItemKey, Type> kvp in r)
            {
                if (kvp.Key is CompletedTypeKey)
                {
                    h[((CompletedTypeKey)(kvp.Key)).Name] = kvp.Value;
                }
            }

            return h;
        }
    }
}
