using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ExprObjModel;

namespace Pascalesque.Two
{
    public class ILLocalDef
    {
        private Symbol name;
        private TypeReference type;

        public ILLocalDef(Symbol name, TypeReference type)
        {
            this.name = name;
            this.type = type;
        }

        public Symbol Name { get { return name; } }
        public TypeReference Type { get { return type; } }
    }

    public class ILContext
    {
        private Dictionary<Symbol, Label> labels;
        private Dictionary<Symbol, LocalBuilder> locals;
        private Dictionary<Symbol, int> parameters;
        private Dictionary<ItemKey, SaBox<object>> references;
        private SymbolTable symbolTable;

        public ILContext
        (
            Dictionary<Symbol, Label> labels,
            Dictionary<Symbol, LocalBuilder> locals,
            Dictionary<Symbol, int> parameters,
            Dictionary<ItemKey, SaBox<object>> references,
            SymbolTable symbolTable
        )
        {
            this.labels = labels;
            this.locals = locals;
            this.parameters = parameters;
            this.references = references;
            this.symbolTable = symbolTable;
        }

        public Dictionary<Symbol, Label> Labels { get { return labels; } }
        public Dictionary<Symbol, LocalBuilder> Locals { get { return locals; } }
        public Dictionary<Symbol, int> Parameters { get { return parameters; } }
        public Dictionary<ItemKey, SaBox<object>> References { get { return references; } }
        public SymbolTable SymbolTable { get { return symbolTable; } }
    }

    public abstract class ILEmit
    {
        public virtual HashSet2<Symbol> LabelsDefined { get { return HashSet2<Symbol>.Empty; } }

        public virtual HashSet2<Symbol> LabelsUsed { get { return HashSet2<Symbol>.Empty; } }

        public virtual HashSet2<ItemKey> References { get { return HashSet2<ItemKey>.Empty; } }

        public abstract void Emit(ILGenerator ilg, ILContext context);
    }

    public enum ILNoArg
    {
        Add, AddOvf, AddOvfUn, Sub, SubOvf, SubOvfUn, Mul, MulOvf, MulOvfUn, Div, DivUn, Rem, RemUn,
        And, Or, Xor, Invert, Negate, Shl, Shr, ShrUn, Dup, Pop, Not,
        LoadNullPtr,
        Throw, Tail, Return, Ceq, Clt, CltUn, Cgt, CgtUn,
        Conv_I, Conv_I1, Conv_I2, Conv_I4, Conv_I8,
        Conv_Ovf_I, Conv_Ovf_I1, Conv_Ovf_I2, Conv_Ovf_I4, Conv_Ovf_I8,
        Conv_Ovf_I_Un, Conv_Ovf_I1_Un, Conv_Ovf_I2_Un, Conv_Ovf_I4_Un, Conv_Ovf_I8_Un,
        Conv_Ovf_U, Conv_Ovf_U1, Conv_Ovf_U2, Conv_Ovf_U4, Conv_Ovf_U8,
        Conv_Ovf_U_Un, Conv_Ovf_U1_Un, Conv_Ovf_U2_Un, Conv_Ovf_U4_Un, Conv_Ovf_U8_Un,
        Conv_R_Un, Conv_R4, Conv_R8,
        Conv_U, Conv_U1, Conv_U2, Conv_U4, Conv_U8,
        LoadObjRef
    }

    public class ILEmitNoArg : ILEmit
    {
        private ILNoArg insn;

        public ILEmitNoArg(ILNoArg insn)
        {
            this.insn = insn;
        }

        public override void Emit(ILGenerator ilg, ILContext context)
        {
            switch (insn)
            {
                case ILNoArg.Add:
                    ilg.Add();
                    break;
                case ILNoArg.AddOvf:
                    ilg.AddOvf();
                    break;
                case ILNoArg.AddOvfUn:
                    ilg.AddOvfUn();
                    break;
                case ILNoArg.Sub:
                    ilg.Sub();
                    break;
                case ILNoArg.SubOvf:
                    ilg.SubOvf();
                    break;
                case ILNoArg.SubOvfUn:
                    ilg.SubOvfUn();
                    break;
                case ILNoArg.Mul:
                    ilg.Mul();
                    break;
                case ILNoArg.MulOvf:
                    ilg.MulOvf();
                    break;
                case ILNoArg.MulOvfUn:
                    ilg.MulOvfUn();
                    break;
                case ILNoArg.Div:
                    ilg.Div();
                    break;
                case ILNoArg.DivUn:
                    ilg.DivUn();
                    break;
                case ILNoArg.Rem:
                    ilg.Rem();
                    break;
                case ILNoArg.RemUn:
                    ilg.RemUn();
                    break;
                case ILNoArg.And:
                    ilg.And();
                    break;
                case ILNoArg.Or:
                    ilg.Or();
                    break;
                case ILNoArg.Xor:
                    ilg.Xor();
                    break;
                case ILNoArg.Invert:
                    ilg.Invert();
                    break;
                case ILNoArg.Negate:
                    ilg.Negate();
                    break;
                case ILNoArg.Shl:
                    ilg.Shl();
                    break;
                case ILNoArg.Shr:
                    ilg.Shr();
                    break;
                case ILNoArg.ShrUn:
                    ilg.ShrUn();
                    break;
                case ILNoArg.Dup:
                    ilg.Dup();
                    break;
                case ILNoArg.Pop:
                    ilg.Pop();
                    break;
                case ILNoArg.Not:
                    ilg.Not();
                    break;
                case ILNoArg.LoadNullPtr:
                    ilg.LoadNullPtr();
                    break;
                case ILNoArg.Throw:
                    ilg.Throw();
                    break;
                case ILNoArg.Tail:
                    ilg.Tail();
                    break;
                case ILNoArg.Return:
                    ilg.Return();
                    break;
                case ILNoArg.Ceq:
                    ilg.Ceq();
                    break;
                case ILNoArg.Clt:
                    ilg.Clt();
                    break;
                case ILNoArg.CltUn:
                    ilg.CltUn();
                    break;
                case ILNoArg.Cgt:
                    ilg.Cgt();
                    break;
                case ILNoArg.CgtUn:
                    ilg.CgtUn();
                    break;
                case ILNoArg.Conv_I:
                    ilg.Conv_I();
                    break;
                case ILNoArg.Conv_I1:
                    ilg.Conv_I1();
                    break;
                case ILNoArg.Conv_I2:
                    ilg.Conv_I2();
                    break;
                case ILNoArg.Conv_I4:
                    ilg.Conv_I4();
                    break;
                case ILNoArg.Conv_I8:
                    ilg.Conv_I8();
                    break;
                case ILNoArg.Conv_Ovf_I:
                    ilg.Conv_Ovf_I();
                    break;
                case ILNoArg.Conv_Ovf_I1:
                    ilg.Conv_Ovf_I1();
                    break;
                case ILNoArg.Conv_Ovf_I2:
                    ilg.Conv_Ovf_I2();
                    break;
                case ILNoArg.Conv_Ovf_I4:
                    ilg.Conv_Ovf_I4();
                    break;
                case ILNoArg.Conv_Ovf_I8:
                    ilg.Conv_Ovf_I8();
                    break;
                case ILNoArg.Conv_Ovf_I_Un:
                    ilg.Conv_Ovf_I_Un();
                    break;
                case ILNoArg.Conv_Ovf_I1_Un:
                    ilg.Conv_Ovf_I1_Un();
                    break;
                case ILNoArg.Conv_Ovf_I2_Un:
                    ilg.Conv_Ovf_I2_Un();
                    break;
                case ILNoArg.Conv_Ovf_I4_Un:
                    ilg.Conv_Ovf_I4_Un();
                    break;
                case ILNoArg.Conv_Ovf_I8_Un:
                    ilg.Conv_Ovf_I8_Un();
                    break;
                case ILNoArg.Conv_Ovf_U:
                    ilg.Conv_Ovf_U();
                    break;
                case ILNoArg.Conv_Ovf_U1:
                    ilg.Conv_Ovf_U1();
                    break;
                case ILNoArg.Conv_Ovf_U2:
                    ilg.Conv_Ovf_U2();
                    break;
                case ILNoArg.Conv_Ovf_U4:
                    ilg.Conv_Ovf_U4();
                    break;
                case ILNoArg.Conv_Ovf_U8:
                    ilg.Conv_Ovf_U8();
                    break;
                case ILNoArg.Conv_Ovf_U_Un:
                    ilg.Conv_Ovf_U_Un();
                    break;
                case ILNoArg.Conv_Ovf_U1_Un:
                    ilg.Conv_Ovf_U1_Un();
                    break;
                case ILNoArg.Conv_Ovf_U2_Un:
                    ilg.Conv_Ovf_U2_Un();
                    break;
                case ILNoArg.Conv_Ovf_U4_Un:
                    ilg.Conv_Ovf_U4_Un();
                    break;
                case ILNoArg.Conv_Ovf_U8_Un:
                    ilg.Conv_Ovf_U8_Un();
                    break;
                case ILNoArg.Conv_R_Un:
                    ilg.Conv_R_Un();
                    break;
                case ILNoArg.Conv_R4:
                    ilg.Conv_R4();
                    break;
                case ILNoArg.Conv_R8:
                    ilg.Conv_R8();
                    break;
                case ILNoArg.Conv_U:
                    ilg.Conv_U();
                    break;
                case ILNoArg.Conv_U1:
                    ilg.Conv_U1();
                    break;
                case ILNoArg.Conv_U2:
                    ilg.Conv_U2();
                    break;
                case ILNoArg.Conv_U4:
                    ilg.Conv_U4();
                    break;
                case ILNoArg.Conv_U8:
                    ilg.Conv_U8();
                    break;
                case ILNoArg.LoadObjRef:
                    ilg.LoadObjRef();
                    break;
                default:
                    throw new InvalidOperationException("Unknown no-argument opcode");
            }
        }
    }

    public class ILEmitLabel : ILEmit
    {
        private Symbol name;

        public ILEmitLabel(Symbol name)
        {
            this.name = name;
        }

        public override HashSet2<Symbol> LabelsDefined
        {
            get
            {
                return HashSet2<Symbol>.Singleton(name);
            }
        }

        public override void Emit(ILGenerator ilg, ILContext context)
        {
            ilg.MarkLabel(context.Labels[name]);
        }
    }

    public enum ILBranch
    {
        Beq, Beq_S,
        Bge, Bge_S, Bge_Un, Bge_Un_S,
        Bgt, Bgt_S, Bgt_Un, Bgt_Un_S,
        Ble, Ble_S, Ble_Un, Ble_Un_S,
        Blt, Blt_S, Blt_Un, Blt_Un_S,
        Bne_Un, Bne_Un_S,
        Br, Br_S,
        Brfalse, Brfalse_S,
        Brtrue, Brtrue_S,
        Leave, Leave_S,
    }

    public class ILEmitBranch : ILEmit
    {
        private ILBranch branch;
        private Symbol target;

        public ILEmitBranch(ILBranch branch, Symbol target)
        {
            this.branch = branch;
            this.target = target;
        }

        public override HashSet2<Symbol> LabelsUsed
        {
            get
            {
                return HashSet2<Symbol>.Singleton(target);
            }
        }

        public override void Emit(ILGenerator ilg, ILContext context)
        {
            ilg.Emit(GetOpCode(branch), context.Labels[target]);
        }

        private static OpCode GetOpCode(ILBranch branch)
        {
            switch(branch)
            {
                case ILBranch.Beq:
                    return OpCodes.Beq;
                case ILBranch.Beq_S:
                    return OpCodes.Beq_S;
                case ILBranch.Bge:
                    return OpCodes.Bge;
                case ILBranch.Bge_S:
                    return OpCodes.Bge_S;
                case ILBranch.Bge_Un:
                    return OpCodes.Bge_Un;
                case ILBranch.Bge_Un_S:
                    return OpCodes.Bge_Un_S;
                case ILBranch.Bgt:
                    return OpCodes.Bgt;
                case ILBranch.Bgt_S:
                    return OpCodes.Bgt_S;
                case ILBranch.Bgt_Un:
                    return OpCodes.Bgt_Un;
                case ILBranch.Bgt_Un_S:
                    return OpCodes.Bgt_Un_S;
                case ILBranch.Ble:
                    return OpCodes.Ble;
                case ILBranch.Ble_S:
                    return OpCodes.Ble_S;
                case ILBranch.Ble_Un:
                    return OpCodes.Ble_Un;
                case ILBranch.Ble_Un_S:
                    return OpCodes.Ble_Un_S;
                case ILBranch.Blt:
                    return OpCodes.Blt;
                case ILBranch.Blt_S:
                    return OpCodes.Blt_S;
                case ILBranch.Blt_Un:
                    return OpCodes.Blt_Un;
                case ILBranch.Blt_Un_S:
                    return OpCodes.Blt_Un_S;
                case ILBranch.Bne_Un:
                    return OpCodes.Bne_Un;
                case ILBranch.Bne_Un_S:
                    return OpCodes.Bne_Un_S;
                case ILBranch.Br:
                    return OpCodes.Br;
                case ILBranch.Br_S:
                    return OpCodes.Br_S;
                case ILBranch.Brfalse:
                    return OpCodes.Brfalse;
                case ILBranch.Brfalse_S:
                    return OpCodes.Brfalse_S;
                case ILBranch.Brtrue:
                    return OpCodes.Brtrue;
                case ILBranch.Brtrue_S:
                    return OpCodes.Brtrue_S;
                case ILBranch.Leave:
                    return OpCodes.Leave;
                case ILBranch.Leave_S:
                    return OpCodes.Leave_S;
                default:
                    throw new ArgumentException("Unknown branch opcode");
            }
        }
    }

    public class ILEmitSwitch : ILEmit
    {
        private Symbol[] targets;

        public ILEmitSwitch(IEnumerable<Symbol> targets)
        {
            this.targets = targets.ToArray();
        }

        public override HashSet2<Symbol> LabelsUsed
        {
            get
            {
                return HashSet2<Symbol>.FromSequence(targets);
            }
        }

        public override void Emit(ILGenerator ilg, ILContext context)
        {
            ilg.Emit(OpCodes.Switch, targets.Select(x => context.Labels[x]).ToArray());
        }
    }

    public class ILEmitLoadArg : ILEmit
    {
        private Symbol name;

        public ILEmitLoadArg(Symbol name)
        {
            this.name = name;
        }

        public override void Emit(ILGenerator ilg, ILContext context)
        {
            ilg.LoadArg(context.Parameters[name]);
        }
    }

    public class ILEmitLoadArgAddress : ILEmit
    {
        private Symbol name;

        public ILEmitLoadArgAddress(Symbol name)
        {
            this.name = name;
        }

        public override void Emit(ILGenerator ilg, ILContext context)
        {
            ilg.LoadArgAddress(context.Parameters[name]);
        }
    }

    public class ILEmitStoreArg : ILEmit
    {
        private Symbol name;

        public ILEmitStoreArg(Symbol name)
        {
            this.name = name;
        }

        public override void Emit(ILGenerator ilg, ILContext context)
        {
            ilg.StoreArg(context.Parameters[name]);
        }
    }

    public class ILEmitLoadLocal : ILEmit
    {
        private Symbol name;

        public ILEmitLoadLocal(Symbol name)
        {
            this.name = name;
        }

        public override void Emit(ILGenerator ilg, ILContext context)
        {
            ilg.LoadLocal(context.Locals[name]);
        }
    }

    public class ILEmitLoadLocalAddress : ILEmit
    {
        private Symbol name;

        public ILEmitLoadLocalAddress(Symbol name)
        {
            this.name = name;
        }

        public override void Emit(ILGenerator ilg, ILContext context)
        {
            ilg.LoadLocalAddress(context.Locals[name]);
        }
    }

    public class ILEmitStoreLocal : ILEmit
    {
        private Symbol name;

        public ILEmitStoreLocal(Symbol name)
        {
            this.name = name;
        }

        public override void Emit(ILGenerator ilg, ILContext context)
        {
            ilg.StoreLocal(context.Locals[name]);
        }
    }

    public class ILEmitLoadInt : ILEmit
    {
        private int literal;

        public ILEmitLoadInt(int literal)
        {
            this.literal = literal;
        }

        public override void Emit(ILGenerator ilg, ILContext context)
        {
            ilg.LoadInt(literal);
        }
    }

    public class ILEmitLoadLong : ILEmit
    {
        private long literal;

        public ILEmitLoadLong(long literal)
        {
            this.literal = literal;
        }

        public override void Emit(ILGenerator ilg, ILContext context)
        {
            ilg.LoadLong(literal);
        }
    }

    public class ILEmitLoadFloat : ILEmit
    {
        private float literal;

        public ILEmitLoadFloat(float literal)
        {
            this.literal = literal;
        }

        public override void Emit(ILGenerator ilg, ILContext context)
        {
            ilg.LoadFloat(literal);
        }
    }

    public class ILEmitLoadDouble : ILEmit
    {
        private double literal;

        public ILEmitLoadDouble(double literal)
        {
            this.literal = literal;
        }

        public override void Emit(ILGenerator ilg, ILContext context)
        {
            ilg.LoadDouble(literal);
        }
    }

    public class ILEmitLoadString : ILEmit
    {
        private string literal;

        public ILEmitLoadString(string literal)
        {
            this.literal = literal;
        }

        public override void Emit(ILGenerator ilg, ILContext context)
        {
            ilg.LoadString(literal);
        }
    }

    public class ILEmitLoadMethodToken : ILEmit
    {
        private MethodReference methodReference;

        public ILEmitLoadMethodToken(MethodReference methodReference)
        {
            this.methodReference = methodReference;
        }

        public override HashSet2<ItemKey> References
        {
            get
            {
                return methodReference.GetReferences();
            }
        }

        public override void Emit(ILGenerator ilg, ILContext context)
        {
            ilg.LoadToken(methodReference.Resolve(context.References));
        }
    }

    public class ILEmitLoadFieldToken : ILEmit
    {
        private FieldReference fieldReference;

        public ILEmitLoadFieldToken(FieldReference fieldReference)
        {
            this.fieldReference = fieldReference;
        }

        public override HashSet2<ItemKey> References
        {
            get
            {
                return fieldReference.GetReferences();
            }
        }

        public override void Emit(ILGenerator ilg, ILContext context)
        {
            ilg.LoadToken(fieldReference.Resolve(context.References));
        }
    }

    public class ILEmitLoadConstructorToken : ILEmit
    {
        private ConstructorReference constructorReference;

        public ILEmitLoadConstructorToken(ConstructorReference constructorReference)
        {
            this.constructorReference = constructorReference;
        }

        public override HashSet2<ItemKey> References
        {
            get
            {
                return constructorReference.GetReferences();
            }
        }

        public override void Emit(ILGenerator ilg, ILContext context)
        {
            ilg.LoadToken(constructorReference.Resolve(context.References));
        }
    }

    public class ILEmitLoadTypeToken : ILEmit
    {
        private TypeReference typeReference;

        public ILEmitLoadTypeToken(TypeReference typeReference)
        {
            this.typeReference = typeReference;
        }

        public override HashSet2<ItemKey> References
        {
            get
            {
                return typeReference.GetReferences();
            }
        }

        public override void Emit(ILGenerator ilg, ILContext context)
        {
            ilg.LoadToken(typeReference.Resolve(context.References));
        }
    }

    public class ILEmitNewObj : ILEmit
    {
        private ConstructorReference constructorReference;

        public ILEmitNewObj(ConstructorReference constructorReference)
        {
            this.constructorReference = constructorReference;
        }

        public override HashSet2<ItemKey> References
        {
            get
            {
                return constructorReference.GetReferences();
            }
        }

        public override void Emit(ILGenerator ilg, ILContext context)
        {
            ilg.NewObj(constructorReference.Resolve(context.References));
        }
    }

    public class ILEmitCall : ILEmit
    {
        private MethodReference methodReference;

        public ILEmitCall(MethodReference methodReference)
        {
            this.methodReference = methodReference;
        }

        public override HashSet2<ItemKey> References
        {
            get
            {
                return methodReference.GetReferences();
            }
        }

        public override void Emit(ILGenerator ilg, ILContext context)
        {
            ilg.Call(methodReference.Resolve(context.References));
        }
    }

    public class ILEmitConstructorCall : ILEmit
    {
        private ConstructorReference constructorReference;

        public ILEmitConstructorCall(ConstructorReference constructorReference)
        {
            this.constructorReference = constructorReference;
        }

        public override HashSet2<ItemKey> References
        {
            get
            {
                return constructorReference.GetReferences();
            }
        }

        public override void Emit(ILGenerator ilg, ILContext context)
        {
            ilg.Call(constructorReference.Resolve(context.References));
        }
    }

    public class ILEmitCallVirt : ILEmit
    {
        private MethodReference methodReference;

        public ILEmitCallVirt(MethodReference methodReference)
        {
            this.methodReference = methodReference;
        }

        public override HashSet2<ItemKey> References
        {
            get
            {
                return methodReference.GetReferences();
            }
        }

        public override void Emit(ILGenerator ilg, ILContext context)
        {
            ilg.CallVirt(methodReference.Resolve(context.References));
        }
    }

    public class ILEmitIsInstance : ILEmit
    {
        private TypeReference typeReference;

        public ILEmitIsInstance(TypeReference typeReference)
        {
            this.typeReference = typeReference;
        }

        public override HashSet2<ItemKey> References
        {
            get
            {
                return typeReference.GetReferences();
            }
        }

        public override void Emit(ILGenerator ilg, ILContext context)
        {
            ilg.IsInst(typeReference.Resolve(context.References));
        }
    }

    public class ILEmitCastClass : ILEmit
    {
        private TypeReference typeReference;

        public ILEmitCastClass(TypeReference typeReference)
        {
            this.typeReference = typeReference;
        }

        public override HashSet2<ItemKey> References
        {
            get
            {
                return typeReference.GetReferences();
            }
        }

        public override void Emit(ILGenerator ilg, ILContext context)
        {
            ilg.CastClass(typeReference.Resolve(context.References));
        }
    }

    public class ILEmitSizeOf : ILEmit
    {
        private TypeReference typeReference;

        public ILEmitSizeOf(TypeReference typeReference)
        {
            this.typeReference = typeReference;
        }

        public override HashSet2<ItemKey> References
        {
            get
            {
                return typeReference.GetReferences();
            }
        }

        public override void Emit(ILGenerator ilg, ILContext context)
        {
            ilg.SizeOf(typeReference.Resolve(context.References));
        }
    }

    public class ILEmitUnaligned : ILEmit
    {
        private Alignment a;

        public ILEmitUnaligned(Alignment a)
        {
            this.a = a;
        }

        public override void Emit(ILGenerator ilg, ILContext context)
        {
            ilg.Unaligned(a);
        }
    }

    public class ILEmitLoadObjIndirect : ILEmit
    {
        private TypeReference typeReference;

        public ILEmitLoadObjIndirect(TypeReference typeReference)
        {
            this.typeReference = typeReference;
        }

        public override HashSet2<ItemKey> References
        {
            get
            {
                return typeReference.GetReferences();
            }
        }

        public override void Emit(ILGenerator ilg, ILContext context)
        {
            if (typeReference == ExistingTypeReference.Byte)
            {
                ilg.Emit(OpCodes.Ldind_U1);
            }
            else if (typeReference == ExistingTypeReference.SByte)
            {
                ilg.Emit(OpCodes.Ldind_I1);
            }
            else if (typeReference == ExistingTypeReference.UInt16)
            {
                ilg.Emit(OpCodes.Ldind_U2);
            }
            else if (typeReference == ExistingTypeReference.Int16)
            {
                ilg.Emit(OpCodes.Ldind_I2);
            }
            else if (typeReference == ExistingTypeReference.UInt32)
            {
                ilg.Emit(OpCodes.Ldind_U4);
            }
            else if (typeReference == ExistingTypeReference.Int32)
            {
                ilg.Emit(OpCodes.Ldind_I4);
            }
            else if (typeReference == ExistingTypeReference.Int64 || typeReference == ExistingTypeReference.UInt64)
            {
                ilg.Emit(OpCodes.Ldind_I8);
            }
            else if (typeReference == ExistingTypeReference.IntPtr || typeReference == ExistingTypeReference.UIntPtr)
            {
                ilg.Emit(OpCodes.Ldind_I);
            }
            else if (typeReference == ExistingTypeReference.Single)
            {
                ilg.Emit(OpCodes.Ldind_R4);
            }
            else if (typeReference == ExistingTypeReference.Double)
            {
                ilg.Emit(OpCodes.Ldind_R8);
            }
            else if (typeReference.IsValueType(context.SymbolTable))
            {
                ilg.Emit(OpCodes.Ldobj, typeReference.Resolve(context.References));
            }
            else
            {
                ilg.Emit(OpCodes.Ldind_Ref);
            }
        }
    }

    public class ILEmitStoreObjIndirect : ILEmit
    {
        private TypeReference typeReference;

        public ILEmitStoreObjIndirect(TypeReference typeReference)
        {
            this.typeReference = typeReference;
        }

        public override HashSet2<ItemKey> References
        {
            get
            {
                return typeReference.GetReferences();
            }
        }

        public override void Emit(ILGenerator ilg, ILContext context)
        {
            if (typeReference == ExistingTypeReference.SByte || typeReference == ExistingTypeReference.Byte)
            {
                ilg.Emit(OpCodes.Stind_I1);
            }
            else if (typeReference == ExistingTypeReference.Int16 || typeReference == ExistingTypeReference.UInt16)
            {
                ilg.Emit(OpCodes.Stind_I2);
            }
            else if (typeReference == ExistingTypeReference.Int32 || typeReference == ExistingTypeReference.UInt32)
            {
                ilg.Emit(OpCodes.Stind_I4);
            }
            else if (typeReference == ExistingTypeReference.Int64 || typeReference == ExistingTypeReference.UInt64)
            {
                ilg.Emit(OpCodes.Stind_I8);
            }
            else if (typeReference == ExistingTypeReference.IntPtr || typeReference == ExistingTypeReference.UIntPtr)
            {
                ilg.Emit(OpCodes.Stind_I);
            }
            else if (typeReference == ExistingTypeReference.Single)
            {
                ilg.Emit(OpCodes.Stind_R4);
            }
            else if (typeReference == ExistingTypeReference.Double)
            {
                ilg.Emit(OpCodes.Stind_R8);
            }
            else if (typeReference.IsValueType(context.SymbolTable))
            {
                ilg.Emit(OpCodes.Stobj, typeReference.Resolve(context.References));
            }
            else
            {
                ilg.Emit(OpCodes.Stind_Ref);
            }
        }
    }

    public class ILEmitLoadElement : ILEmit
    {
        private TypeReference typeReference;

        public ILEmitLoadElement(TypeReference typeReference)
        {
            this.typeReference = typeReference;
        }

        public override HashSet2<ItemKey> References
        {
            get
            {
                return typeReference.GetReferences();
            }
        }

        public override void Emit(ILGenerator ilg, ILContext context)
        {
            if (typeReference == ExistingTypeReference.SByte)
            {
                ilg.Emit(OpCodes.Ldelem_I1);
            }
            else if (typeReference == ExistingTypeReference.Byte)
            {
                ilg.Emit(OpCodes.Ldelem_U1);
            }
            else if (typeReference == ExistingTypeReference.Int16)
            {
                ilg.Emit(OpCodes.Ldelem_I2);
            }
            else if (typeReference == ExistingTypeReference.UInt16)
            {
                ilg.Emit(OpCodes.Ldelem_U2);
            }
            else if (typeReference == ExistingTypeReference.Int32)
            {
                ilg.Emit(OpCodes.Ldelem_I4);
            }
            else if (typeReference == ExistingTypeReference.UInt32)
            {
                ilg.Emit(OpCodes.Ldelem_U4);
            }
            else if (typeReference == ExistingTypeReference.Int64 || typeReference == ExistingTypeReference.UInt64)
            {
                ilg.Emit(OpCodes.Ldelem_I8);
            }
            else if (typeReference == ExistingTypeReference.IntPtr || typeReference == ExistingTypeReference.UIntPtr)
            {
                ilg.Emit(OpCodes.Ldelem_I);
            }
            else if (typeReference == ExistingTypeReference.Single)
            {
                ilg.Emit(OpCodes.Ldelem_R4);
            }
            else if (typeReference == ExistingTypeReference.Double)
            {
                ilg.Emit(OpCodes.Ldelem_R8);
            }
            else if (!(typeReference.IsValueType(context.SymbolTable)))
            {
                ilg.Emit(OpCodes.Ldelem_Ref);
            }
            else
            {
                ilg.Emit(OpCodes.Ldelem, typeReference.Resolve(context.References));
            }
        }
    }

    public class LocalInfo2
    {
        private Symbol name;
        private TypeReference paramType;
        private bool isPinned;

        public LocalInfo2(Symbol name, TypeReference paramType, bool isPinned)
        {
            this.name = name;
            this.paramType = paramType;
            this.isPinned = isPinned;
        }

        public Symbol Name { get { return name; } }

        public TypeReference ParamType { get { return paramType; } }

        public bool IsPinned { get { return isPinned; } }
    }

    public class ILConstructorToBuild : ElementOfClass
    {
        public override void DefineSymbols(SymbolTable s, TypeKey owner)
        {
            throw new NotImplementedException();
        }

        public override void AddCompileSteps(SymbolTable s, TypeKey owner, Action<ICompileStep> add)
        {
            throw new NotImplementedException();
        }
    }

    public class ILMethodToBuild : ElementOfClass
    {
        private Symbol name;
        private MethodAttributes attributes;
        private TypeReference returnType;
        private ParamInfo2[] parameters;
        private LocalInfo2[] locals;
        private ILEmit[] instructions;

        public ILMethodToBuild
        (
            Symbol name,
            MethodAttributes attributes,
            TypeReference returnType,
            IEnumerable<ParamInfo2> parameters,
            IEnumerable<LocalInfo2> locals,
            IEnumerable<ILEmit> instructions
        )
        {
            this.name = name;
            this.attributes = attributes;
            this.returnType = returnType;
            this.parameters = parameters.ToArray();
            this.locals = locals.ToArray();
            this.instructions = instructions.ToArray();   
        }

        private MethodKey GetMethodKey(TypeKey owner)
        {
            return new MethodKey(owner, name, !(attributes.HasFlag(MethodAttributes.Static)), parameters.Select(x => x.ParamType));
        }

        private class MakeILMethod : ICompileStep
        {
            private ILMethodToBuild parent;
            private SymbolTable symbolTable;
            private TypeKey owner;
            private MethodKey methodKey;

            public MakeILMethod(ILMethodToBuild parent, SymbolTable symbolTable, TypeKey owner, MethodKey methodKey)
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
                        parent.parameters.Select(x => x.ParamType.GetReferences()).HashSet2Union() |
                        parent.locals.Select(x => x.ParamType.GetReferences()).HashSet2Union() |
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

        private class MakeILMethodBody : ICompileStep
        {
            private ILMethodToBuild parent;
            private SymbolTable symbolTable;
            private TypeKey owner;
            private MethodKey methodKey;

            public MakeILMethodBody(ILMethodToBuild parent, SymbolTable symbolTable, TypeKey owner, MethodKey methodKey)
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
                        parent.parameters.Select(x => x.ParamType.GetReferences()).HashSet2Union() |
                        parent.locals.Select(x => x.ParamType.GetReferences()).HashSet2Union() |
                        HashSet2<ItemKey>.Singleton(methodKey);
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

                HashSet2<Symbol> labelsDefined = parent.instructions.Select(x => x.LabelsDefined).HashSet2Union();
                HashSet2<Symbol> labelsUsed = parent.instructions.Select(x => x.LabelsUsed).HashSet2Union();

                if (!((labelsUsed - labelsDefined).IsEmpty)) throw new PascalesqueException("Labels { " + ((labelsUsed - labelsDefined).Items.Select(x => x.Name).Concatenate(" ")) + " } used without being defined");

                Dictionary<Symbol, Label> labels = new Dictionary<Symbol,Label>();

                foreach(Symbol s in labelsUsed.Items)
                {
                    labels.Add(s, ilg.DefineLabel());
                }

                Dictionary<Symbol, LocalBuilder> locals = new Dictionary<Symbol, LocalBuilder>();

                foreach (LocalInfo2 localInfo in parent.locals)
                {
                    locals.Add(localInfo.Name, ilg.DeclareLocal(localInfo.ParamType.Resolve(vars), localInfo.IsPinned));
                }

                Dictionary<Symbol, int> parameters = new Dictionary<Symbol, int>();

                for (int i = 0; i < parent.parameters.Length; ++i)
                {
                    parameters.Add(parent.parameters[i].Name, i);
                }

                ILContext c = new ILContext(labels, locals, parameters, vars, symbolTable);

                foreach (ILEmit instruction in parent.instructions)
                {
                    instruction.Emit(ilg, c);
                }
            }
        }

        public override void DefineSymbols(SymbolTable s, TypeKey owner)
        {
            MethodKey mk = GetMethodKey(owner);
            s[mk] = new MethodAux(attributes, returnType);
        }

        public override void AddCompileSteps(SymbolTable s, TypeKey owner, Action<ICompileStep> add)
        {
            MethodKey mk = GetMethodKey(owner);
            add(new MakeILMethod(this, s, owner, mk));
            add(new MakeILMethodBody(this, s, owner, mk));
        }
    }
}