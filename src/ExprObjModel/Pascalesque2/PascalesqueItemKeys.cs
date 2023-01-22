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
    enum ItemHashDelimiters : byte
    {
        TypeKey = (byte)0x3A,
        CompletedTypeKey = (byte)0x91,
        TypeKeyReference = (byte)0x3F,
        ExistingTypeReference = (byte)0x34,
        ExistingGenericTypeReference = (byte)0x37,
        MethodKey = (byte)0x36,
        ConstructorKey = (byte)0x31,
        FieldKey = (byte)0x3B,
        ArrayTypeReference = (byte)0x3D,
        MethodKeyReference = (byte)0x56,
        ExistingMethodReference = (byte)0x51,
        ExistingConstructorReference = (byte)0x52,
        ConstructorKeyReference = (byte)0x53,
        FieldKeyReference = (byte)0x54,
        ExistingFieldReference = (byte)0x55,
        PropertyKey = (byte)0x39,
    }

#pragma warning disable 660, 661 // defines == and != but not object.Equals or object.GetHashCode
    public abstract class ItemKey : IEquatable<ItemKey>, IHashable
    {
        public abstract bool Equals(ItemKey other);

        public abstract void AddToHash(IHashGenerator hg);

        public static bool operator ==(ItemKey a, ItemKey b)
        {
            if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null)) return true;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null)) return false;
            return a.Equals(b);
        }

        public static bool operator !=(ItemKey a, ItemKey b)
        {
            if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null)) return false;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null)) return true;
            return !(a.Equals(b));
        }
    }
#pragma warning restore 660, 661

    public class TypeKey : ItemKey
    {
        private Symbol name;

        public TypeKey(Symbol name)
        {
            this.name = name;
        }

        public Symbol Name { get { return name; } }

        public override bool Equals(object obj)
        {
            if (!(obj is TypeKey)) return false;
            TypeKey tObj = (TypeKey)obj;
            return this.name == tObj.name;
        }

        public override void AddToHash(IHashGenerator hg)
        {
            hg.Add((byte)ItemHashDelimiters.TypeKey);
            name.AddToHash(hg);
        }

        public override bool Equals(ItemKey other)
        {
            if (!(other is TypeKey)) return false;
            TypeKey tObj = (TypeKey)other;
            return this.name == tObj.name;
        }

        public override int GetHashCode()
        {
            HashGenerator hg = new HashGenerator();
            this.AddToHash(hg);
            return hg.Hash;
        }

        public override string ToString()
        {
            return "[ TypeKey " + SchemeDataWriter.ItemToString(name) + " ]";
        }

        public static bool operator ==(TypeKey a, TypeKey b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(TypeKey a, TypeKey b)
        {
            return !(a.Equals(b));
        }
    }

    public class CompletedTypeKey : ItemKey
    {
        private Symbol name;

        public CompletedTypeKey(Symbol name)
        {
            this.name = name;
        }

        public Symbol Name { get { return name; } }

        public override bool Equals(object obj)
        {
            if (!(obj is CompletedTypeKey)) return false;
            CompletedTypeKey tObj = (CompletedTypeKey)obj;
            return this.name == tObj.name;
        }

        public override void AddToHash(IHashGenerator hg)
        {
            hg.Add((byte)ItemHashDelimiters.CompletedTypeKey);
            name.AddToHash(hg);
        }

        public override bool Equals(ItemKey other)
        {
            if (!(other is CompletedTypeKey)) return false;
            CompletedTypeKey tObj = (CompletedTypeKey)other;
            return this.name == tObj.name;
        }

        public override int GetHashCode()
        {
            HashGenerator hg = new HashGenerator();
            this.AddToHash(hg);
            return hg.Hash;
        }

        public override string ToString()
        {
            return "[ CompletedTypeKey " + SchemeDataWriter.ItemToString(name) + " ]";
        }

        public static bool operator ==(CompletedTypeKey a, CompletedTypeKey b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(CompletedTypeKey a, CompletedTypeKey b)
        {
            return !(a.Equals(b));
        }
    }

#pragma warning disable 660, 661
    public abstract class TypeReference : IEquatable<TypeReference>, IHashable
    {
        public abstract bool Equals(TypeReference other);

        public abstract void AddToHash(IHashGenerator hg);

        public abstract HashSet2<ItemKey> GetReferences();

        public abstract Type Resolve(Dictionary<ItemKey, SaBox<object>> references);

        public static bool operator ==(TypeReference a, TypeReference b)
        {
            if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null)) return true;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null)) return false;
            return a.Equals(b);
        }

        public static bool operator !=(TypeReference a, TypeReference b)
        {
            if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null)) return false;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null)) return true;
            return !(a.Equals(b));
        }

        public static TypeReference GetActionType(TypeReference[] arguments)
        {
            Type t;
            switch (arguments.Length)
            {
                case 0:
                    return new ExistingTypeReference(typeof(Action));
                case 1:
                    t = typeof(Action<>);
                    break;
                case 2:
                    t = typeof(Action<,>);
                    break;
                case 3:
                    t = typeof(Action<,,>);
                    break;
                case 4:
                    t = typeof(Action<,,,>);
                    break;
                case 5:
                    t = typeof(Action<,,,,>);
                    break;
                case 6:
                    t = typeof(Action<,,,,,>);
                    break;
                case 7:
                    t = typeof(Action<,,,,,,>);
                    break;
                case 8:
                    t = typeof(Action<,,,,,,,>);
                    break;
                case 9:
                    t = typeof(Action<,,,,,,,,>);
                    break;
                case 10:
                    t = typeof(Action<,,,,,,,,,>);
                    break;
                case 11:
                    t = typeof(Action<,,,,,,,,,,>);
                    break;
                case 12:
                    t = typeof(Action<,,,,,,,,,,,>);
                    break;
                case 13:
                    t = typeof(Action<,,,,,,,,,,,,>);
                    break;
                case 14:
                    t = typeof(Action<,,,,,,,,,,,,,>);
                    break;
                case 15:
                    t = typeof(Action<,,,,,,,,,,,,,,>);
                    break;
                case 16:
                    t = typeof(Action<,,,,,,,,,,,,,,,>);
                    break;
                default:
                    throw new PascalesqueException("Too many Action<...> type arguments");
            }
            return new ExistingGenericTypeReference(t, arguments);
        }

        public static TypeReference GetFuncType(TypeReference[] arguments)
        {
            Type t;
            switch (arguments.Length)
            {
                case 0:
                    throw new PascalesqueException("Func<...> requires at least one type argument");
                case 1:
                    t = typeof(Func<>);
                    break;
                case 2:
                    t = typeof(Func<,>);
                    break;
                case 3:
                    t = typeof(Func<,,>);
                    break;
                case 4:
                    t = typeof(Func<,,,>);
                    break;
                case 5:
                    t = typeof(Func<,,,,>);
                    break;
                case 6:
                    t = typeof(Func<,,,,,>);
                    break;
                case 7:
                    t = typeof(Func<,,,,,,>);
                    break;
                case 8:
                    t = typeof(Func<,,,,,,,>);
                    break;
                case 9:
                    t = typeof(Func<,,,,,,,,>);
                    break;
                case 10:
                    t = typeof(Func<,,,,,,,,,>);
                    break;
                case 11:
                    t = typeof(Func<,,,,,,,,,,>);
                    break;
                case 12:
                    t = typeof(Func<,,,,,,,,,,,>);
                    break;
                case 13:
                    t = typeof(Func<,,,,,,,,,,,,>);
                    break;
                case 14:
                    t = typeof(Func<,,,,,,,,,,,,,>);
                    break;
                case 15:
                    t = typeof(Func<,,,,,,,,,,,,,,>);
                    break;
                case 16:
                    t = typeof(Func<,,,,,,,,,,,,,,,>);
                    break;
                case 17:
                    t = typeof(Func<,,,,,,,,,,,,,,,,>);
                    break;
                default:
                    throw new PascalesqueException("Too many Func<...> type arguments");
            }

            return new ExistingGenericTypeReference(t, arguments);
        }

        public abstract bool IsDelegate { get; }

        public abstract TypeReference[] GetDelegateParameterTypes();

        public abstract TypeReference GetDelegateReturnType();

        public static TypeReference MakeBoxedType(TypeReference t)
        {
            if (IsBoxedType(t)) throw new ArgumentException("Nested boxes are not allowed");
            return new ExistingGenericTypeReference(typeof(Box<>), new TypeReference[] { t });
        }

        public static bool IsBoxedType(TypeReference t)
        {
            if (!(t is ExistingGenericTypeReference)) return false;
            ExistingGenericTypeReference et = (ExistingGenericTypeReference)t;
            if (!(et.OpenGenericType == typeof(Box<>))) return false;
            if (et.TypeArguments.Count != 1) return false;
            return true;
        }

        public static TypeReference GetBoxContentType(TypeReference t)
        {
            if (!(t is ExistingGenericTypeReference)) throw new ArgumentException("Box<T> expected");
            ExistingGenericTypeReference et = (ExistingGenericTypeReference)t;
            if (!(et.OpenGenericType == typeof(Box<>))) throw new ArgumentException("Box<T> expected");
            if (et.TypeArguments.Count != 1) throw new ArgumentException("Ill-formed Box<T>");
            return et.TypeArguments[0];
        }

        public abstract bool IsArray { get; }

        public abstract TypeReference GetElementType();

        public abstract TypeReference MakeArrayType();

        public abstract bool IsValueType(SymbolTable s);

        public static bool IsAssignable(SymbolTable s, TypeReference dest, TypeReference src)
        {
            throw new NotImplementedException();
        }
    }
#pragma warning restore 660, 661

    public class TypeKeyReference : TypeReference
    {
        private TypeKey typeKey;

        public TypeKeyReference(TypeKey typeKey)
        {
            this.typeKey = typeKey;
        }

        public TypeKey TypeKey { get { return typeKey; } }

        public override bool Equals(object obj)
        {
            if (!(obj is TypeKeyReference)) return false;
            TypeKeyReference tObj = (TypeKeyReference)obj;
            return this.typeKey == tObj.typeKey;
        }

        public override void AddToHash(IHashGenerator hg)
        {
            hg.Add((byte)ItemHashDelimiters.TypeKeyReference);
            typeKey.AddToHash(hg);
        }

        public override bool Equals(TypeReference other)
        {
            if (!(other is TypeKeyReference)) return false;
            TypeKeyReference tObj = (TypeKeyReference)other;
            return this.typeKey == tObj.typeKey;
        }

        public override int GetHashCode()
        {
            HashGenerator hg = new HashGenerator();
            this.AddToHash(hg);
            return hg.Hash;
        }

        public override string ToString()
        {
            return "[ TypeKeyReference " + SchemeDataWriter.ItemToString(typeKey.Name) + " ]";
        }

        public override HashSet2<ItemKey> GetReferences()
        {
            return HashSet2<ItemKey>.Singleton(typeKey);
        }

        public override Type Resolve(Dictionary<ItemKey, SaBox<object>> references)
        {
            if (references.ContainsKey(typeKey))
            {
                if (references[typeKey].HasValue)
                {
                    return (TypeBuilder)(references[typeKey].Value);
                }
                else
                {
                    throw new PascalesqueException("Attempt to resolve undefined type " + this.ToString());
                }
            }
            else
            {
                throw new PascalesqueException("Attempt to resolve unknown type " + this.ToString());
            }
        }

        public override bool IsDelegate
        {
            get { return false; }
        }

        public override TypeReference[] GetDelegateParameterTypes()
        {
            throw new InvalidOperationException("Not a delegate");
        }

        public override TypeReference GetDelegateReturnType()
        {
            throw new InvalidOperationException("Not a delegate");
        }

        public override bool IsArray
        {
            get { return false; }
        }

        public override TypeReference GetElementType()
        {
            throw new InvalidOperationException("Not an array");
        }

        public override TypeReference MakeArrayType()
        {
            return new ArrayTypeReference(this);
        }

        public override bool IsValueType(SymbolTable s)
        {
            return s[typeKey].IsValueType;
        }
    }

    public class ExistingTypeReference : TypeReference
    {
        private Type existingType;

        public ExistingTypeReference(Type existingType)
        {
            this.existingType = existingType;
        }

        public Type ExistingType { get { return existingType; } }

        public override bool Equals(object obj)
        {
            if (!(obj is ExistingTypeReference)) return false;
            ExistingTypeReference tObj = (ExistingTypeReference)obj;
            return this.existingType == tObj.existingType;
        }

        public override void AddToHash(IHashGenerator hg)
        {
            hg.Add((byte)ItemHashDelimiters.ExistingTypeReference);
            hg.Add(existingType);
        }

        public override bool Equals(TypeReference other)
        {
            if (!(other is ExistingTypeReference)) return false;
            ExistingTypeReference tObj = (ExistingTypeReference)other;
            return this.existingType == tObj.existingType;
        }

        public override int GetHashCode()
        {
            HashGenerator hg = new HashGenerator();
            this.AddToHash(hg);
            return hg.Hash;
        }

        public override string ToString()
        {
            return "[ ExistingTypeReference " + SchemeDataWriter.ItemToString(existingType) + " ]";
        }

        public override HashSet2<ItemKey> GetReferences()
        {
            return HashSet2<ItemKey>.Empty;
        }

        public override Type Resolve(Dictionary<ItemKey, SaBox<object>> references)
        {
            return existingType;
        }

        #region Constants

        private static Lazy<ExistingTypeReference> theVoid = new Lazy<ExistingTypeReference>(() => new ExistingTypeReference(typeof(void)), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
        public static ExistingTypeReference Void { get { return theVoid.Value; } }

        private static Lazy<ExistingTypeReference> theBool = new Lazy<ExistingTypeReference>(() => new ExistingTypeReference(typeof(bool)), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
        public static ExistingTypeReference Boolean { get { return theBool.Value; } }

        private static Lazy<ExistingTypeReference> theChar = new Lazy<ExistingTypeReference>(() => new ExistingTypeReference(typeof(char)), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
        public static ExistingTypeReference Char { get { return theChar.Value; } }

        private static Lazy<ExistingTypeReference> theString = new Lazy<ExistingTypeReference>(() => new ExistingTypeReference(typeof(string)), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
        public static ExistingTypeReference String { get { return theString.Value; } }

        private static Lazy<ExistingTypeReference> theByte = new Lazy<ExistingTypeReference>(() => new ExistingTypeReference(typeof(byte)), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
        public static ExistingTypeReference Byte { get { return theByte.Value; } }

        private static Lazy<ExistingTypeReference> theInt16 = new Lazy<ExistingTypeReference>(() => new ExistingTypeReference(typeof(short)), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
        public static ExistingTypeReference Int16 { get { return theInt16.Value; } }

        private static Lazy<ExistingTypeReference> theInt32 = new Lazy<ExistingTypeReference>(() => new ExistingTypeReference(typeof(int)), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
        public static ExistingTypeReference Int32 { get { return theInt32.Value; } }

        private static Lazy<ExistingTypeReference> theInt64 = new Lazy<ExistingTypeReference>(() => new ExistingTypeReference(typeof(long)), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
        public static ExistingTypeReference Int64 { get { return theInt64.Value; } }

        private static Lazy<ExistingTypeReference> theIntPtr = new Lazy<ExistingTypeReference>(() => new ExistingTypeReference(typeof(IntPtr)), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
        public static ExistingTypeReference IntPtr { get { return theIntPtr.Value; } }

        private static Lazy<ExistingTypeReference> theSByte = new Lazy<ExistingTypeReference>(() => new ExistingTypeReference(typeof(sbyte)), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
        public static ExistingTypeReference SByte { get { return theSByte.Value; } }

        private static Lazy<ExistingTypeReference> theUInt16 = new Lazy<ExistingTypeReference>(() => new ExistingTypeReference(typeof(ushort)), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
        public static ExistingTypeReference UInt16 { get { return theUInt16.Value; } }

        private static Lazy<ExistingTypeReference> theUInt32 = new Lazy<ExistingTypeReference>(() => new ExistingTypeReference(typeof(uint)), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
        public static ExistingTypeReference UInt32 { get { return theUInt32.Value; } }

        private static Lazy<ExistingTypeReference> theUInt64 = new Lazy<ExistingTypeReference>(() => new ExistingTypeReference(typeof(ulong)), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
        public static ExistingTypeReference UInt64 { get { return theUInt64.Value; } }

        private static Lazy<ExistingTypeReference> theUIntPtr = new Lazy<ExistingTypeReference>(() => new ExistingTypeReference(typeof(UIntPtr)), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
        public static ExistingTypeReference UIntPtr { get { return theUIntPtr.Value; } }

        private static Lazy<ExistingTypeReference> theSingle = new Lazy<ExistingTypeReference>(() => new ExistingTypeReference(typeof(float)), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
        public static ExistingTypeReference Single { get { return theSingle.Value; } }

        private static Lazy<ExistingTypeReference> theDouble = new Lazy<ExistingTypeReference>(() => new ExistingTypeReference(typeof(double)), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
        public static ExistingTypeReference Double { get { return theDouble.Value; } }

        private static Lazy<ExistingTypeReference> theObject = new Lazy<ExistingTypeReference>(() => new ExistingTypeReference(typeof(object)), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
        public static ExistingTypeReference Object { get { return theObject.Value; } }

        #endregion

        public override bool IsDelegate
        {
            get { return existingType.IsSubclassOf(typeof(Delegate)); }
        }

        public override TypeReference[] GetDelegateParameterTypes()
        {
            if (!(existingType.IsSubclassOf(typeof(Delegate)))) throw new InvalidOperationException("Not a delegate");
            MethodInfo mi = existingType.GetMethod("Invoke");
            return mi.GetParameters().Select(x => new ExistingTypeReference(x.ParameterType)).ToArray();
        }

        public override TypeReference GetDelegateReturnType()
        {
            if (!(existingType.IsSubclassOf(typeof(Delegate)))) throw new InvalidOperationException("Not a delegate");
            MethodInfo mi = existingType.GetMethod("Invoke");
            return new ExistingTypeReference(mi.ReturnType);
        }

        public override bool IsArray
        {
            get { return existingType.IsArray; }
        }

        public override TypeReference GetElementType()
        {
            if (existingType.IsArray)
            {
                return new ExistingTypeReference(existingType.GetElementType());
            }
            else
            {
                throw new InvalidOperationException("Call for element type of non-array");
            }
        }

        public override TypeReference MakeArrayType()
        {
            return new ExistingTypeReference(existingType.MakeArrayType());
        }

        public override bool IsValueType(SymbolTable s)
        {
            return existingType.IsValueType;
        }
    }

    public class ExistingGenericTypeReference : TypeReference
    {
        private Type openGenericType;
        private TypeReference[] typeArguments;

        public ExistingGenericTypeReference(Type openGenericType, IEnumerable<TypeReference> typeArguments)
        {
            this.openGenericType = openGenericType;
            this.typeArguments = typeArguments.ToArray();
        }

        public Type OpenGenericType { get { return openGenericType; } }

        public IReadOnlyArray<TypeReference> TypeArguments { get { return ExprObjModel.Utils.AsReadOnlyArray<TypeReference>(typeArguments); } }

        public override bool Equals(object obj)
        {
            if (!(obj is ExistingGenericTypeReference)) return false;
            ExistingGenericTypeReference eObj = (ExistingGenericTypeReference)obj;
            if (openGenericType != eObj.openGenericType) return false;
            if (typeArguments.Length != eObj.typeArguments.Length) return false;
            int iEnd = typeArguments.Length;
            for (int i = 0; i < iEnd; ++i)
            {
                if (typeArguments[i] != eObj.typeArguments[i]) return false;
            }
            return true;
        }

        public override void AddToHash(IHashGenerator hg)
        {
            hg.Add((byte)ItemHashDelimiters.ExistingGenericTypeReference);
            hg.Add(openGenericType);
            foreach (TypeReference typeArgument in typeArguments)
            {
                typeArgument.AddToHash(hg);
            }
        }

        public override bool Equals(TypeReference other)
        {
            if (!(other is ExistingGenericTypeReference)) return false;
            ExistingGenericTypeReference eObj = (ExistingGenericTypeReference)other;
            if (openGenericType != eObj.openGenericType) return false;
            if (typeArguments.Length != eObj.typeArguments.Length) return false;
            int iEnd = typeArguments.Length;
            for (int i = 0; i < iEnd; ++i)
            {
                if (typeArguments[i] != eObj.typeArguments[i]) return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            HashGenerator hg = new HashGenerator();
            this.AddToHash(hg);
            return hg.Hash;
        }

        public override string ToString()
        {
            return "[ ExistingGenericTypeReference, openGenericType = " + SchemeDataWriter.ItemToString(openGenericType) +
                ", typeArguments = #(" + typeArguments.Select(x => x.ToString()).Concatenate(" ") + ") ]";
        }

        public override HashSet2<ItemKey> GetReferences()
        {
            return typeArguments.Select(x => x.GetReferences()).HashSet2Union();
        }

        public override Type Resolve(Dictionary<ItemKey, SaBox<object>> references)
        {
            Type[] ts = typeArguments.Select(x => x.Resolve(references)).ToArray();
            return openGenericType.MakeGenericType(ts);
        }

        public override bool IsDelegate
        {
            get { return openGenericType.IsSubclassOf(typeof(Delegate)); }
        }

        public override TypeReference[] GetDelegateParameterTypes()
        {
            if (!(openGenericType.IsSubclassOf(typeof(Delegate)))) throw new InvalidOperationException("Not a delegate");
            MethodInfo mi = openGenericType.GetMethod("Invoke");
            Type[] genericParameters = openGenericType.GetGenericArguments();
            Func<Type, TypeReference> subst = delegate(Type t)
            {
                int iEnd = genericParameters.Length;
                for (int i = 0; i < iEnd; ++i)
                {
                    if (t == genericParameters[i]) return typeArguments[i];
                }
                return new ExistingTypeReference(t);
            };

            return mi.GetParameters().Select(x => subst(x.ParameterType)).ToArray();
        }

        public override TypeReference GetDelegateReturnType()
        {
            if (!(openGenericType.IsSubclassOf(typeof(Delegate)))) throw new InvalidOperationException("Not a delegate");
            MethodInfo mi = openGenericType.GetMethod("Invoke");
            Type[] genericParameters = openGenericType.GetGenericArguments();
            Func<Type, TypeReference> subst = delegate(Type t)
            {
                int iEnd = genericParameters.Length;
                for (int i = 0; i < iEnd; ++i)
                {
                    if (t == genericParameters[i]) return typeArguments[i];
                }
                return new ExistingTypeReference(t);
            };
            return subst(mi.ReturnType);
        }

        public override bool IsArray
        {
            get { return false; }
        }

        public override TypeReference GetElementType()
        {
            throw new InvalidOperationException("Not an array");
        }

        public override TypeReference MakeArrayType()
        {
            return new ArrayTypeReference(this);
        }

        public override bool IsValueType(SymbolTable s)
        {
            return openGenericType.IsValueType;
        }
    }

    public class ArrayTypeReference : TypeReference
    {
        private TypeReference elementType;

        public ArrayTypeReference(TypeReference elementType)
        {
            this.elementType = elementType;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ArrayTypeReference)) return false;
            ArrayTypeReference aObj = (ArrayTypeReference)obj;
            if (elementType != aObj.elementType) return false;
            return true;
        }

        public override void AddToHash(IHashGenerator hg)
        {
            hg.Add((byte)ItemHashDelimiters.ArrayTypeReference);
            elementType.AddToHash(hg);
        }

        public override bool Equals(TypeReference other)
        {
            if (!(other is ArrayTypeReference)) return false;
            ArrayTypeReference aObj = (ArrayTypeReference)other;
            if (elementType != aObj.elementType) return false;
            return true;
        }

        public override int GetHashCode()
        {
            HashGenerator hg = new HashGenerator();
            this.AddToHash(hg);
            return hg.Hash;
        }

        public override string ToString()
        {
            return "[ ArrayTypeReference, elementType = " + elementType.ToString() + " ]";
        }

        public override HashSet2<ItemKey> GetReferences()
        {
            return elementType.GetReferences();
        }

        public override Type Resolve(Dictionary<ItemKey, SaBox<object>> references)
        {
            Type eType = elementType.Resolve(references);
            return eType.MakeArrayType();
        }

        public override bool IsDelegate
        {
            get { return false; }
        }

        public override TypeReference[] GetDelegateParameterTypes()
        {
            throw new InvalidOperationException("Not a delegate");
        }

        public override TypeReference GetDelegateReturnType()
        {
            throw new InvalidOperationException("Not a delegate");
        }

        public override bool IsArray
        {
            get { return true; }
        }

        public override TypeReference GetElementType()
        {
            return elementType;
        }

        public override TypeReference MakeArrayType()
        {
            return new ArrayTypeReference(this);
        }

        public override bool IsValueType(SymbolTable s)
        {
            return false;
        }
    }

    public class MethodKey : ItemKey
    {
        TypeKey owner;
        Symbol name;
        bool isInstance;
        TypeReference[] parameters;

        public MethodKey(TypeKey owner, Symbol name, bool isInstance, IEnumerable<TypeReference> parameters)
        {
            this.owner = owner;
            this.name = name;
            this.isInstance = isInstance;
            this.parameters = parameters.ToArray();
        }

        public TypeKey Owner { get { return owner; } }

        public Symbol Name { get { return name; } }

        public bool IsInstance { get { return isInstance; } }

        public ExprObjModel.IReadOnlyArray<TypeReference> Parameters { get { return ExprObjModel.Utils.AsReadOnlyArray(parameters); } }

        public override bool Equals(object obj)
        {
            if (!(obj is MethodKey)) return false;
            MethodKey mObj = (MethodKey)obj;
            if (owner != mObj.owner) return false;
            if (name != mObj.name) return false;
            if (isInstance != mObj.isInstance) return false;
            if (parameters.Length != mObj.parameters.Length) return false;
            int iEnd = parameters.Length;
            for (int i = 0; i < iEnd; ++i)
            {
                if (parameters[i] != mObj.parameters[i]) return false;
            }
            return true;
        }

        public override void AddToHash(IHashGenerator hg)
        {
            hg.Add((byte)ItemHashDelimiters.MethodKey);
            owner.AddToHash(hg);
            name.AddToHash(hg);
            hg.Add(BitConverter.GetBytes(isInstance));
            foreach (TypeReference t in parameters)
            {
                t.AddToHash(hg);
            }
        }

        public override bool Equals(ItemKey other)
        {
            if (!(other is MethodKey)) return false;
            MethodKey mObj = (MethodKey)other;
            if (owner != mObj.owner) return false;
            if (name != mObj.name) return false;
            if (isInstance != mObj.isInstance) return false;
            if (parameters.Length != mObj.parameters.Length) return false;
            int iEnd = parameters.Length;
            for (int i = 0; i < iEnd; ++i)
            {
                if (parameters[i] != mObj.parameters[i]) return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            HashGenerator hg = new HashGenerator();
            this.AddToHash(hg);
            return hg.Hash;
        }

        public override string ToString()
        {
            return "[ MethodKey owner = " + owner.ToString() +
                ", name = " + SchemeDataWriter.ItemToString(name) +
                ", isInstance = " + SchemeDataWriter.ItemToString(isInstance) +
                ", parameters = #(" + parameters.Select(x => x.ToString()).Concatenate(" ") + ")" +
                " ]";
        }
    }

#pragma warning disable 660, 661
    public abstract class MethodReference : IEquatable<MethodReference>, IHashable
    {
        public abstract bool Equals(MethodReference other);

        public abstract void AddToHash(IHashGenerator hg);

        public abstract HashSet2<ItemKey> GetReferences();

        public abstract MethodInfo Resolve(Dictionary<ItemKey, SaBox<object>> references);

        public static bool operator ==(MethodReference a, MethodReference b)
        {
            if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null)) return true;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null)) return false;
            return a.Equals(b);
        }

        public static bool operator !=(MethodReference a, MethodReference b)
        {
            if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null)) return false;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null)) return true;
            return !(a.Equals(b));
        }

        public abstract int ParameterCount { get; }

        public abstract TypeReference GetParameterType(int index);

        public abstract TypeReference GetReturnType(SymbolTable s);
    }
#pragma warning restore 660, 661

    public class MethodKeyReference : MethodReference
    {
        private MethodKey methodKey;

        public MethodKeyReference(MethodKey methodKey)
        {
            this.methodKey = methodKey;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is MethodKeyReference)) return false;
            MethodKeyReference mObj = (MethodKeyReference)obj;
            return methodKey == mObj.methodKey;
        }

        public override void AddToHash(IHashGenerator hg)
        {
            hg.Add((byte)ItemHashDelimiters.MethodKeyReference);
            methodKey.AddToHash(hg);
        }

        public override bool Equals(MethodReference other)
        {
            if (!(other is MethodKeyReference)) return false;
            MethodKeyReference mObj = (MethodKeyReference)other;
            return methodKey == mObj.methodKey;
        }

        public override int GetHashCode()
        {
            HashGenerator hg = new HashGenerator();
            this.AddToHash(hg);
            return hg.Hash;
        }

        public override HashSet2<ItemKey> GetReferences()
        {
            return HashSet2<ItemKey>.Singleton(methodKey);
        }

        public override MethodInfo Resolve(Dictionary<ItemKey, SaBox<object>> references)
        {
            return (MethodInfo)(references[methodKey].Value);
        }

        public override string ToString()
        {
            return "[MethodKeyInfo]";
        }

        public override int ParameterCount
        {
            get { return methodKey.IsInstance ? methodKey.Parameters.Count + 1 : methodKey.Parameters.Count; }
        }

        public override TypeReference GetParameterType(int index)
        {
            if (methodKey.IsInstance)
            {
                if (index == 0) return new TypeKeyReference(methodKey.Owner);
                else return methodKey.Parameters[index - 1];
            }
            else
            {
                return methodKey.Parameters[index];
            }
        }

        public override TypeReference GetReturnType(SymbolTable s)
        {
            return s[methodKey].ReturnType;
        }
    }

    public class ExistingMethodReference : MethodReference
    {
        private MethodInfo methodInfo;

        public ExistingMethodReference(MethodInfo methodInfo)
        {
            this.methodInfo = methodInfo;
        }

        public MethodInfo ExistingMethod { get { return ExistingMethod; } }

        public override bool Equals(object obj)
        {
            if (!(obj is ExistingMethodReference)) return false;
            ExistingMethodReference eObj = (ExistingMethodReference)obj;
            if (eObj.methodInfo != methodInfo) return false;
            return true;
        }

        public override void AddToHash(IHashGenerator hg)
        {
            hg.Add((byte)ItemHashDelimiters.ExistingMethodReference);
            hg.Add(methodInfo);
        }

        public override bool Equals(MethodReference other)
        {
            if (!(other is ExistingMethodReference)) return false;
            ExistingMethodReference eObj = (ExistingMethodReference)other;
            if (eObj.methodInfo != methodInfo) return false;
            return true;
        }

        public override int GetHashCode()
        {
            HashGenerator hg = new HashGenerator();
            this.AddToHash(hg);
            return hg.Hash;
        }

        public override HashSet2<ItemKey> GetReferences()
        {
            return HashSet2<ItemKey>.Empty;
        }

        public override MethodInfo Resolve(Dictionary<ItemKey, SaBox<object>> references)
        {
            return methodInfo;
        }

        public override string ToString()
        {
            return "[ExistingMethodInfo]";
        }

        public override int ParameterCount
        {
            get { return methodInfo.IsStatic ? methodInfo.GetParameters().Length : (methodInfo.GetParameters().Length + 1); }
        }

        public override TypeReference GetParameterType(int index)
        {
            if (methodInfo.IsStatic)
            {
                return new ExistingTypeReference(methodInfo.GetParameters()[index].ParameterType);
            }
            else
            {
                if (index == 0) return new ExistingTypeReference(methodInfo.ReturnType);
                else return new ExistingTypeReference(methodInfo.GetParameters()[index - 1].ParameterType);
            }
        }

        public override TypeReference GetReturnType(SymbolTable s)
        {
            return new ExistingTypeReference(methodInfo.ReturnType);
        }
    }

    public class ConstructorKey : ItemKey
    {
        private TypeKey owner;
        private TypeReference[] parameters;

        public ConstructorKey(TypeKey owner, IEnumerable<TypeReference> parameters)
        {
            this.owner = owner;
            this.parameters = parameters.ToArray();
        }

        public TypeKey Owner { get { return owner; } }

        public ExprObjModel.IReadOnlyArray<TypeReference> Parameters { get { return ExprObjModel.Utils.AsReadOnlyArray(parameters); } }

        public override bool Equals(object obj)
        {
            if (!(obj is ConstructorKey)) return false;
            ConstructorKey cObj = (ConstructorKey)obj;
            if (owner != cObj.owner) return false;
            if (parameters.Length != cObj.parameters.Length) return false;
            int iEnd = parameters.Length;
            for (int i = 0; i < iEnd; ++i)
            {
                if (parameters[i] != cObj.parameters[i]) return false;
            }
            return true;
        }

        public override void AddToHash(IHashGenerator hg)
        {
            hg.Add((byte)ItemHashDelimiters.ConstructorKey);
            owner.AddToHash(hg);
            foreach (TypeReference t in parameters)
            {
                t.AddToHash(hg);
            }
        }

        public override bool Equals(ItemKey other)
        {
            if (!(other is ConstructorKey)) return false;
            ConstructorKey cObj = (ConstructorKey)other;
            if (owner != cObj.owner) return false;
            if (parameters.Length != cObj.parameters.Length) return false;
            int iEnd = parameters.Length;
            for (int i = 0; i < iEnd; ++i)
            {
                if (parameters[i] != cObj.parameters[i]) return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            HashGenerator hg = new HashGenerator();
            this.AddToHash(hg);
            return hg.Hash;
        }

        public override string ToString()
        {
            return "[ ConstructorKey owner = " + owner.ToString() +
                ", parameters = #(" + parameters.Select(x => x.ToString()).Concatenate(" ") + ") ]";
        }
    }

#pragma warning disable 660, 661
    public abstract class ConstructorReference : IEquatable<ConstructorReference>, IHashable
    {
        public abstract bool Equals(ConstructorReference other);

        public abstract void AddToHash(IHashGenerator hg);

        public abstract HashSet2<ItemKey> GetReferences();

        public abstract ConstructorInfo Resolve(Dictionary<ItemKey, SaBox<object>> references);

        public static bool operator ==(ConstructorReference a, ConstructorReference b)
        {
            if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null)) return true;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null)) return false;
            return a.Equals(b);
        }

        public static bool operator !=(ConstructorReference a, ConstructorReference b)
        {
            if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null)) return false;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null)) return true;
            return !(a.Equals(b));
        }

        public abstract int ParameterCount { get; }

        public abstract TypeReference GetParameterType(int index);

        public abstract TypeReference ConstructorOfWhat { get; }
    }
#pragma warning restore 660, 661

    public class ConstructorKeyReference : ConstructorReference
    {
        private ConstructorKey constructorKey;

        public ConstructorKeyReference(ConstructorKey constructorKey)
        {
            this.constructorKey = constructorKey;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ConstructorKeyReference)) return false;
            ConstructorKeyReference cObj = (ConstructorKeyReference)obj;
            return constructorKey == cObj.constructorKey;
        }

        public override void AddToHash(IHashGenerator hg)
        {
            hg.Add((byte)ItemHashDelimiters.ConstructorKeyReference);
            constructorKey.AddToHash(hg);
        }

        public override bool Equals(ConstructorReference other)
        {
            if (!(other is ConstructorKeyReference)) return false;
            ConstructorKeyReference cObj = (ConstructorKeyReference)other;
            return constructorKey == cObj.constructorKey;
        }

        public override int GetHashCode()
        {
            IHashGenerator hg = new HashGenerator();
            AddToHash(hg);
            return hg.Hash;
        }

        public override HashSet2<ItemKey> GetReferences()
        {
            return HashSet2<ItemKey>.Singleton(constructorKey);
        }

        public override ConstructorInfo Resolve(Dictionary<ItemKey, SaBox<object>> references)
        {
            return (ConstructorInfo)(references[constructorKey].Value);
        }

        public override string ToString()
        {
            return "[ConstructorKeyReference, key = " + constructorKey.ToString() + "]";
        }

        public override int ParameterCount
        {
            get { return constructorKey.Parameters.Count; }
        }

        public override TypeReference GetParameterType(int index)
        {
            return constructorKey.Parameters[index];
        }

        public override TypeReference ConstructorOfWhat
        {
            get { return new TypeKeyReference(constructorKey.Owner); }
        }
    }

    public class ExistingConstructorReference : ConstructorReference
    {
        private ConstructorInfo constructorInfo;

        public ExistingConstructorReference(ConstructorInfo constructorInfo)
        {
            this.constructorInfo = constructorInfo;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ExistingConstructorReference)) return false;
            ExistingConstructorReference eObj = (ExistingConstructorReference)obj;
            return constructorInfo == eObj.constructorInfo;
        }

        public override void AddToHash(IHashGenerator hg)
        {
            hg.Add((byte)ItemHashDelimiters.ExistingConstructorReference);
            hg.Add(constructorInfo);
        }

        public override bool Equals(ConstructorReference other)
        {
            if (!(other is ExistingConstructorReference)) return false;
            ExistingConstructorReference eObj = (ExistingConstructorReference)other;
            return constructorInfo == eObj.constructorInfo;
        }

        public override int GetHashCode()
        {
            IHashGenerator hg = new HashGenerator();
            AddToHash(hg);
            return hg.Hash;
        }

        public override HashSet2<ItemKey> GetReferences()
        {
            return HashSet2<ItemKey>.Empty;
        }

        public override ConstructorInfo Resolve(Dictionary<ItemKey, SaBox<object>> references)
        {
            return constructorInfo;
        }

        public override string ToString()
        {
            return "[ExistingConstructorInfo]";
        }

        public override int ParameterCount
        {
            get { return constructorInfo.GetParameters().Length; }
        }

        public override TypeReference GetParameterType(int index)
        {
            return new ExistingTypeReference(constructorInfo.GetParameters()[index].ParameterType);
        }

        public override TypeReference ConstructorOfWhat
        {
            get { return new ExistingTypeReference(constructorInfo.DeclaringType); }
        }
    }

    public class FieldKey : ItemKey
    {
        private TypeKey owner;
        private Symbol name;
        private TypeReference fieldType;

        public FieldKey(TypeKey owner, Symbol name, TypeReference fieldType)
        {
            this.owner = owner;
            this.name = name;
            this.fieldType = fieldType;
        }

        public TypeKey Owner { get { return owner; } }

        public Symbol Name { get { return name; } }

        public TypeReference FieldType { get { return fieldType; } }

        public override bool Equals(object obj)
        {
            if (!(obj is FieldKey)) return false;
            FieldKey fObj = (FieldKey)obj;
            if (owner != fObj.owner) return false;
            if (name != fObj.name) return false;
            if (fieldType != fObj.fieldType) return false;
            return true;
        }

        public override void AddToHash(IHashGenerator hg)
        {
            hg.Add((byte)ItemHashDelimiters.FieldKey);
            owner.AddToHash(hg);
            name.AddToHash(hg);
            fieldType.AddToHash(hg);
        }

        public override bool Equals(ItemKey other)
        {
            if (!(other is FieldKey)) return false;
            FieldKey fObj = (FieldKey)other;
            if (owner != fObj.owner) return false;
            if (name != fObj.name) return false;
            if (fieldType != fObj.fieldType) return false;
            return true;
        }

        public override int GetHashCode()
        {
            HashGenerator hg = new HashGenerator();
            this.AddToHash(hg);
            return hg.Hash;
        }

        public override string ToString()
        {
            return "[ FieldKey owner = " + owner.ToString() +
                ", name = " + SchemeDataWriter.ItemToString(name) +
                ", fieldType = " + fieldType.ToString() + " ]";
        }
    }

#pragma warning disable 660, 661
    public abstract class FieldReference : IEquatable<FieldReference>, IHashable
    {
        public abstract bool Equals(FieldReference other);

        public abstract void AddToHash(IHashGenerator hg);

        public abstract HashSet2<ItemKey> GetReferences();

        public abstract FieldInfo Resolve(Dictionary<ItemKey, SaBox<object>> references);

        public static bool operator ==(FieldReference a, FieldReference b)
        {
            if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null)) return true;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null)) return false;
            return a.Equals(b);
        }

        public static bool operator !=(FieldReference a, FieldReference b)
        {
            if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null)) return false;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null)) return true;
            return !(a.Equals(b));
        }

        public abstract TypeReference Owner { get; }

        public abstract TypeReference FieldType { get; }
    }
#pragma warning restore 660, 661

    public class FieldKeyReference : FieldReference
    {
        private FieldKey fieldKey;

        public FieldKeyReference(FieldKey fieldKey)
        {
            this.fieldKey = fieldKey;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is FieldKeyReference)) return false;
            FieldKeyReference fObj = (FieldKeyReference)obj;
            return fieldKey == fObj.fieldKey;
        }

        public override void AddToHash(IHashGenerator hg)
        {
            hg.Add((byte)ItemHashDelimiters.FieldKeyReference);
            fieldKey.AddToHash(hg);
        }

        public override bool Equals(FieldReference other)
        {
            if (!(other is FieldKeyReference)) return false;
            FieldKeyReference fObj = (FieldKeyReference)other;
            return fieldKey == fObj.fieldKey;
        }

        public override int GetHashCode()
        {
            HashGenerator hg = new HashGenerator();
            this.AddToHash(hg);
            return hg.Hash;
        }

        public override HashSet2<ItemKey> GetReferences()
        {
            return HashSet2<ItemKey>.Singleton(fieldKey);
        }

        public override FieldInfo Resolve(Dictionary<ItemKey, SaBox<object>> references)
        {
            return (FieldInfo)(references[fieldKey].Value);
        }

        public override string ToString()
        {
            return "[FieldKeyReference, key = " + fieldKey.ToString() + "]";
        }

        public override TypeReference Owner
        {
            get { return new TypeKeyReference(fieldKey.Owner); }
        }

        public override TypeReference FieldType
        {
            get { return fieldKey.FieldType; }
        }
    }

    public class ExistingFieldReference : FieldReference
    {
        private FieldInfo fieldInfo;

        public ExistingFieldReference(FieldInfo fieldInfo)
        {
            this.fieldInfo = fieldInfo;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ExistingFieldReference)) return false;
            ExistingFieldReference eObj = (ExistingFieldReference)obj;
            return fieldInfo == eObj.fieldInfo;
        }

        public override void AddToHash(IHashGenerator hg)
        {
            hg.Add((byte)ItemHashDelimiters.ExistingFieldReference);
            hg.Add(fieldInfo);
        }

        public override bool Equals(FieldReference other)
        {
            if (!(other is ExistingFieldReference)) return false;
            ExistingFieldReference eObj = (ExistingFieldReference)other;
            return fieldInfo == eObj.fieldInfo;
        }

        public override int GetHashCode()
        {
            HashGenerator hg = new HashGenerator();
            this.AddToHash(hg);
            return hg.Hash;
        }

        public override HashSet2<ItemKey> GetReferences()
        {
            return HashSet2<ItemKey>.Empty;
        }

        public override FieldInfo Resolve(Dictionary<ItemKey, SaBox<object>> references)
        {
            return fieldInfo;
        }

        public override string ToString()
        {
            return "[ExistingFieldReference]";
        }

        public override TypeReference Owner
        {
            get { return new ExistingTypeReference(fieldInfo.DeclaringType); }
        }

        public override TypeReference FieldType
        {
            get { return new ExistingTypeReference(fieldInfo.FieldType); }
        }
    }

    public class PropertyKey : ItemKey
    {
        private TypeKey owner;
        private Symbol name;
        private TypeReference propertyType;
        private List<TypeReference> propertyArgs;

        public PropertyKey(TypeKey owner, Symbol name, TypeReference propertyType, IEnumerable<TypeReference> propertyArgs)
        {
            this.owner = owner;
            this.name = name;
            this.propertyType = propertyType;
            this.propertyArgs = propertyArgs.ToList();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PropertyKey)) return false;
            PropertyKey pObj = (PropertyKey)obj;
            if (owner != pObj.owner) return false;
            if (name != pObj.name) return false;
            if (propertyType != pObj.propertyType) return false;
            if (propertyArgs.Count != pObj.propertyArgs.Count) return false;
            int iEnd = propertyArgs.Count;
            for (int i = 0; i < iEnd; ++i)
            {
                if (propertyArgs[i] != pObj.propertyArgs[i]) return false;
            }
            return true;
        }

        public override void AddToHash(IHashGenerator hg)
        {
            hg.Add((byte)ItemHashDelimiters.PropertyKey);
            owner.AddToHash(hg);
            name.AddToHash(hg);
            propertyType.AddToHash(hg);
            int iEnd = propertyArgs.Count;
            hg.Add(iEnd);
            for (int i = 0; i < iEnd; ++i)
            {
                propertyArgs[i].AddToHash(hg);
            }
        }

        public override bool Equals(ItemKey other)
        {
            if (!(other is PropertyKey)) return false;
            PropertyKey pObj = (PropertyKey)other;
            if (owner != pObj.owner) return false;
            if (name != pObj.name) return false;
            if (propertyType != pObj.propertyType) return false;
            if (propertyArgs.Count != pObj.propertyArgs.Count) return false;
            int iEnd = propertyArgs.Count;
            for (int i = 0; i < iEnd; ++i)
            {
                if (propertyArgs[i] != pObj.propertyArgs[i]) return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            HashGenerator hg = new HashGenerator();
            this.AddToHash(hg);
            return hg.Hash;
        }

        public override string ToString()
        {
            return "[ PropertyKey owner = " + owner.ToString() +
                ", name = " + name.ToString() +
                ", propertyType = " + propertyType.ToString() +
                ", propertyArgs = (" + propertyArgs.Select(x => x.ToString()).Concatenate(", ") + ") ]";
        }
    }

    public abstract class ItemAux
    {
    }

    public class TypeAux : ItemAux
    {
        private bool isValueType;
        private bool isInterface;

        public TypeAux(bool isValueType, bool isInterface)
        {
            this.isValueType = isValueType;
            this.isInterface = isInterface;
        }

        public bool IsValueType { get { return isValueType; } }

        public bool IsInterface { get { return isInterface; } }
    }

    public class MethodAux : ItemAux
    {
        private MethodAttributes attributes;
        private TypeReference returnType;

        public MethodAux(MethodAttributes attributes, TypeReference returnType)
        {
            this.attributes = attributes;
            this.returnType = returnType;
        }

        public MethodAttributes Attributes { get { return attributes; } }

        public TypeReference ReturnType { get { return returnType; } }
    }

    public class ConstructorAux : ItemAux
    {
        private MethodAttributes attributes;

        public ConstructorAux(MethodAttributes attributes)
        {
            this.attributes = attributes;
        }

        public MethodAttributes Attributes { get { return attributes; } }
    }

    public class FieldAux : ItemAux
    {
    }

    public class PropertyAux : ItemAux
    {
    }

    public class SymbolTable
    {
        private Dictionary<ItemKey, ItemAux> dict;

        public SymbolTable()
        {
            dict = new Dictionary<ItemKey, ItemAux>();
        }

        public TypeAux this[TypeKey t]
        {
            get
            {
                return (TypeAux)(dict[t]);
            }
            set
            {
                if (dict.ContainsKey(t)) throw new InvalidOperationException("Single assignment violation");
                dict.Add(t, value);
            }
        }

        public MethodAux this[MethodKey m]
        {
            get
            {
                return (MethodAux)(dict[m]);
            }
            set
            {
                if (dict.ContainsKey(m)) throw new InvalidOperationException("Single assignment violation");
                dict.Add(m, value);
            }
        }

        public ConstructorAux this[ConstructorKey c]
        {
            get
            {
                return (ConstructorAux)(dict[c]);
            }
            set
            {
                if (dict.ContainsKey(c)) throw new InvalidOperationException("Single assignment violation");
                dict.Add(c, value);
            }
        }

        public FieldAux this[FieldKey f]
        {
            get
            {
                return (FieldAux)(dict[f]);
            }
            set
            {
                if (dict.ContainsKey(f)) throw new InvalidOperationException("Single assignment violation");
                dict.Add(f, value);
            }
        }

        public PropertyAux this[PropertyKey p]
        {
            get
            {
                return (PropertyAux)(dict[p]);
            }
            set
            {
                if (dict.ContainsKey(p)) throw new InvalidOperationException("Single assignment violation");
                dict.Add(p, value);
            }
        }
    }

    public class HashSet2<T>
    {
        private HashSet<T> set;

        private HashSet2(HashSet<T> set)
        {
            this.set = set;
        }

        public static HashSet2<T> Empty { get { return new HashSet2<T>(new HashSet<T>()); } }

        public IEnumerable<T> Items { get { return set.AsEnumerable(); } }

        public static HashSet2<T> Singleton(T k)
        {
            HashSet<T> h = new HashSet<T>();
            h.Add(k);
            return new HashSet2<T>(h);
        }

        public static HashSet2<T> FromSequence(IEnumerable<T> klist)
        {
            HashSet<T> h = new HashSet<T>();
            h.UnionWith(klist);
            return new HashSet2<T>(h);
        }

        public static HashSet2<T> operator |(HashSet2<T> a, HashSet2<T> b)
        {
            HashSet<T> h = new HashSet<T>();
            h.UnionWith(a.set);
            h.UnionWith(b.set);
            return new HashSet2<T>(h);
        }

        public static HashSet2<T> operator |(HashSet2<T> a, T b)
        {
            HashSet<T> h = new HashSet<T>();
            h.UnionWith(a.set);
            h.Add(b);
            return new HashSet2<T>(h);
        }

        public static HashSet2<T> operator |(T a, HashSet2<T> b)
        {
            HashSet<T> h = new HashSet<T>();
            h.Add(a);
            h.UnionWith(b.set);
            return new HashSet2<T>(h);
        }

        public static HashSet2<T> operator &(HashSet2<T> a, HashSet2<T> b)
        {
            HashSet<T> h = new HashSet<T>();
            h.UnionWith(a.set);
            h.IntersectWith(b.set);
            return new HashSet2<T>(h);
        }

        public static HashSet2<T> operator -(HashSet2<T> a, HashSet2<T> b)
        {
            HashSet<T> h = new HashSet<T>();
            h.UnionWith(a.set);
            h.ExceptWith(b.set);
            return new HashSet2<T>(h);
        }

        public bool Contains(T i)
        {
            return set.Contains(i);
        }

        public bool IsEmpty { get { return set.Count == 0; } }
    }
}