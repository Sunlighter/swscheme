using System;
using System.Collections.Generic;

namespace ExprObjModel.Procedures
{
    public class UserType
    {
        private Symbol id;
        private object payload;

        public UserType(Symbol id, object payload)
        {
            this.id = id;
            this.payload = payload;
        }

        public Symbol ID { get { return id; } }
        public object Payload { get { return payload; } set { payload = value; } }
    }

    public class CreateUserTypeProc : IProcedure
    {
        private Symbol id;

        public CreateUserTypeProc(Symbol id)
        {
            this.id = id;
        }

        public int Arity { get { return 1; } }

        public bool More { get { return false; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            if (argList == null)
            {
                return k.Throw(gs, new SchemeRuntimeException("Too few arguments to create-user-type " + id));
            }
            object initialPayload = argList.Head;
            argList = argList.Tail;
            if (argList != null)
            {
                return k.Throw(gs, new SchemeRuntimeException("Too many arguments to create-user-type " + id));
            }
            return k.Return(gs, new UserType(id, initialPayload));
        }
    }

    public class IsUserTypeProc : IProcedure
    {
        private Symbol id;

        public IsUserTypeProc(Symbol id)
        {
            this.id = id;
        }

        public int Arity { get { return 1; } }

        public bool More { get { return false; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            if (argList == null)
            {
                return k.Throw(gs, new SchemeRuntimeException("Too few arguments to is-user-type " + id));
            }
            object testObj = argList.Head;
            argList = argList.Tail;
            if (argList != null)
            {
                return k.Throw(gs, new SchemeRuntimeException("Too many arguments to is-user-type " + id));
            }

            bool result = (testObj is UserType && ((UserType)testObj).ID == id);
            return k.Return(gs, result);
        }
    }

    public class GetUserTypePayloadProc : IProcedure
    {
        private Symbol id;

        public GetUserTypePayloadProc(Symbol id)
        {
            this.id = id;
        }

        public int Arity { get { return 1; } }

        public bool More { get { return false; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            if (argList == null)
            {
                return k.Throw(gs, new SchemeRuntimeException("Too few arguments to get-user-data " + id));
            }
            object arg = argList.Head;
            argList = argList.Tail;
            if (argList != null)
            {
                return k.Throw(gs, new SchemeRuntimeException("Too many arguments to get-user-data " + id));
            }

            bool validArg = (arg is UserType && ((UserType)arg).ID == id);
            if (validArg)
            {
                return k.Return(gs, ((UserType)arg).Payload);
            }
            else
            {
                return k.Throw(gs, new SchemeRuntimeException("Type mismatch in argument to get-user-data " + id));
            }
        }
    }

    public class SetUserTypePayloadProc : IProcedure
    {
        private Symbol id;

        public SetUserTypePayloadProc(Symbol id)
        {
            this.id = id;
        }

        public int Arity { get { return 2; } }

        public bool More { get { return false; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            if (argList == null)
            {
                return k.Throw(gs, new SchemeRuntimeException("set-user-data " + id + ": Too few arguments"));
            }
            object arg = argList.Head;
            argList = argList.Tail;
            if (argList == null)
            {
                return k.Throw(gs, new SchemeRuntimeException("set-user-data " + id + ": Too few arguments"));
            }
            object value = argList.Head;
            argList = argList.Tail;
            if (argList != null)
            {
                return k.Throw(gs, new SchemeRuntimeException("set-user-data " + id + ": Too many arguments"));
            }

            bool validArg = (arg is UserType && ((UserType)arg).ID == id);
            if (validArg)
            {
                ((UserType)arg).Payload = value;
                return k.Return(gs, SpecialValue.UNSPECIFIED);
            }
            else
            {
                return k.Throw(gs, new SchemeRuntimeException("set-user-data " + id + ": Type mismatch"));
            }
        }
    }

    [SchemeSingleton("with-user-type")]
    public class WithUserType : IProcedure
    {
        public WithUserType()
        {
        }

        public int Arity { get { return 1; } }

        public bool More { get { return false; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            if (argList == null)
            {
                return k.Throw(gs, new SchemeRuntimeException("with-user-type: Too few arguments"));
            }
            object proc = argList.Head;
            argList = argList.Tail;
            if (argList != null)
            {
                return k.Throw(gs, new SchemeRuntimeException("with-user-type: Too many arguments"));
            }

            if (!(proc is IProcedure))
            {
                return k.Throw(gs, new SchemeRuntimeException("with-user-type: Argument must be a procedure"));
            }

            IProcedure proc2 = (IProcedure)proc;
            if (proc2.AcceptsParameterCount(4))
            {
                Symbol id = new Symbol();
                FList<object> args = new FList<object>(new SetUserTypePayloadProc(id));
                args = new FList<object>(new GetUserTypePayloadProc(id), args);
                args = new FList<object>(new IsUserTypeProc(id), args);
                args = new FList<object>(new CreateUserTypeProc(id), args);
                return proc2.Call(gs, args, k);
            }
            else
            {
                return k.Throw(gs, new SchemeRuntimeException("with-user-type: Argument does not accept enough arguments"));
            }
        }
    }
}