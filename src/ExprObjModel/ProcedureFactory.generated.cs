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

namespace ExprObjModel
{
    public static partial class Utils
    {
        
        private class ActionProcedure_0 : IProcedure
        {
            private string name;
            private Action proc;

            public ActionProcedure_0(string name, Action proc)
            {
                this.name = name;
                this.proc = proc;
            }

            public int Arity { get { return 0; } }

            public bool More { get { return false; } }

            public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
            {
                
                if (argList != null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": too many arguments"));
                }

                
                try
                {
                    
                    proc();
                    return new RunnableReturn(k, SpecialValue.UNSPECIFIED);

                    
                }
                catch(Exception exc)
                {
                    return new RunnableReturn(k, exc);
                }
            }
        }

        public static IProcedure CreateProcedure(string name, Action proc)
        {
            return new ActionProcedure_0(name, proc);
        }

        
        private class ActionProcedure_GS_0 : IProcedure
        {
            private string name;
            private Action<IGlobalState> proc;

            public ActionProcedure_GS_0(string name, Action<IGlobalState> proc)
            {
                this.name = name;
                this.proc = proc;
            }

            public int Arity { get { return 0; } }

            public bool More { get { return false; } }

            public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
            {
                
                if (argList != null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": too many arguments"));
                }

                
                try
                {
                    
                    proc(gs);
                    return new RunnableReturn(k, SpecialValue.UNSPECIFIED);

                    
                }
                catch(Exception exc)
                {
                    return new RunnableReturn(k, exc);
                }
            }
        }

        public static IProcedure CreateProcedure(string name, Action<IGlobalState> proc)
        {
            return new ActionProcedure_GS_0(name, proc);
        }

        
        private class FuncProcedure_0 : IProcedure
        {
            private string name;
            private Func<object> proc;

            public FuncProcedure_0(string name, Func<object> proc)
            {
                this.name = name;
                this.proc = proc;
            }

            public int Arity { get { return 0; } }

            public bool More { get { return false; } }

            public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
            {
                
                if (argList != null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": too many arguments"));
                }

                
                object result;

                
                try
                {
                    
                    result = proc();
                    return new RunnableReturn(k, result);

                    
                }
                catch(Exception exc)
                {
                    return new RunnableReturn(k, exc);
                }
            }
        }

        public static IProcedure CreateProcedure(string name, Func<object> proc)
        {
            return new FuncProcedure_0(name, proc);
        }

        
        private class FuncProcedure_GS_0 : IProcedure
        {
            private string name;
            private Func<IGlobalState, object> proc;

            public FuncProcedure_GS_0(string name, Func<IGlobalState, object> proc)
            {
                this.name = name;
                this.proc = proc;
            }

            public int Arity { get { return 0; } }

            public bool More { get { return false; } }

            public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
            {
                
                if (argList != null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": too many arguments"));
                }

                
                object result;

                
                try
                {
                    
                    result = proc(gs);
                    return new RunnableReturn(k, result);

                    
                }
                catch(Exception exc)
                {
                    return new RunnableReturn(k, exc);
                }
            }
        }

        public static IProcedure CreateProcedure(string name, Func<IGlobalState, object> proc)
        {
            return new FuncProcedure_GS_0(name, proc);
        }

        
        private class ActionProcedure_1 : IProcedure
        {
            private string name;
            private Action<object> proc;

            public ActionProcedure_1(string name, Action<object> proc)
            {
                this.name = name;
                this.proc = proc;
            }

            public int Arity { get { return 1; } }

            public bool More { get { return false; } }

            public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
            {
                                
                object arg1;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg1 = argList.Head;
                    argList = argList.Tail;
                }

                
                if (argList != null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": too many arguments"));
                }

                
                try
                {
                    
                    proc(arg1);
                    return new RunnableReturn(k, SpecialValue.UNSPECIFIED);

                    
                }
                catch(Exception exc)
                {
                    return new RunnableReturn(k, exc);
                }
            }
        }

        public static IProcedure CreateProcedure(string name, Action<object> proc)
        {
            return new ActionProcedure_1(name, proc);
        }

        
        private class ActionProcedure_GS_1 : IProcedure
        {
            private string name;
            private Action<IGlobalState, object> proc;

            public ActionProcedure_GS_1(string name, Action<IGlobalState, object> proc)
            {
                this.name = name;
                this.proc = proc;
            }

            public int Arity { get { return 1; } }

            public bool More { get { return false; } }

            public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
            {
                                
                object arg1;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg1 = argList.Head;
                    argList = argList.Tail;
                }

                
                if (argList != null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": too many arguments"));
                }

                
                try
                {
                    
                    proc(gs, arg1);
                    return new RunnableReturn(k, SpecialValue.UNSPECIFIED);

                    
                }
                catch(Exception exc)
                {
                    return new RunnableReturn(k, exc);
                }
            }
        }

        public static IProcedure CreateProcedure(string name, Action<IGlobalState, object> proc)
        {
            return new ActionProcedure_GS_1(name, proc);
        }

        
        private class FuncProcedure_1 : IProcedure
        {
            private string name;
            private Func<object, object> proc;

            public FuncProcedure_1(string name, Func<object, object> proc)
            {
                this.name = name;
                this.proc = proc;
            }

            public int Arity { get { return 1; } }

            public bool More { get { return false; } }

            public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
            {
                                
                object arg1;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg1 = argList.Head;
                    argList = argList.Tail;
                }

                
                if (argList != null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": too many arguments"));
                }

                
                object result;

                
                try
                {
                    
                    result = proc(arg1);
                    return new RunnableReturn(k, result);

                    
                }
                catch(Exception exc)
                {
                    return new RunnableReturn(k, exc);
                }
            }
        }

        public static IProcedure CreateProcedure(string name, Func<object, object> proc)
        {
            return new FuncProcedure_1(name, proc);
        }

        
        private class FuncProcedure_GS_1 : IProcedure
        {
            private string name;
            private Func<IGlobalState, object, object> proc;

            public FuncProcedure_GS_1(string name, Func<IGlobalState, object, object> proc)
            {
                this.name = name;
                this.proc = proc;
            }

            public int Arity { get { return 1; } }

            public bool More { get { return false; } }

            public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
            {
                                
                object arg1;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg1 = argList.Head;
                    argList = argList.Tail;
                }

                
                if (argList != null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": too many arguments"));
                }

                
                object result;

                
                try
                {
                    
                    result = proc(gs, arg1);
                    return new RunnableReturn(k, result);

                    
                }
                catch(Exception exc)
                {
                    return new RunnableReturn(k, exc);
                }
            }
        }

        public static IProcedure CreateProcedure(string name, Func<IGlobalState, object, object> proc)
        {
            return new FuncProcedure_GS_1(name, proc);
        }

        
        private class ActionProcedure_2 : IProcedure
        {
            private string name;
            private Action<object, object> proc;

            public ActionProcedure_2(string name, Action<object, object> proc)
            {
                this.name = name;
                this.proc = proc;
            }

            public int Arity { get { return 2; } }

            public bool More { get { return false; } }

            public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
            {
                                
                object arg1;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg1 = argList.Head;
                    argList = argList.Tail;
                }

                                
                object arg2;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg2 = argList.Head;
                    argList = argList.Tail;
                }

                
                if (argList != null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": too many arguments"));
                }

                
                try
                {
                    
                    proc(arg1, arg2);
                    return new RunnableReturn(k, SpecialValue.UNSPECIFIED);

                    
                }
                catch(Exception exc)
                {
                    return new RunnableReturn(k, exc);
                }
            }
        }

        public static IProcedure CreateProcedure(string name, Action<object, object> proc)
        {
            return new ActionProcedure_2(name, proc);
        }

        
        private class ActionProcedure_GS_2 : IProcedure
        {
            private string name;
            private Action<IGlobalState, object, object> proc;

            public ActionProcedure_GS_2(string name, Action<IGlobalState, object, object> proc)
            {
                this.name = name;
                this.proc = proc;
            }

            public int Arity { get { return 2; } }

            public bool More { get { return false; } }

            public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
            {
                                
                object arg1;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg1 = argList.Head;
                    argList = argList.Tail;
                }

                                
                object arg2;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg2 = argList.Head;
                    argList = argList.Tail;
                }

                
                if (argList != null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": too many arguments"));
                }

                
                try
                {
                    
                    proc(gs, arg1, arg2);
                    return new RunnableReturn(k, SpecialValue.UNSPECIFIED);

                    
                }
                catch(Exception exc)
                {
                    return new RunnableReturn(k, exc);
                }
            }
        }

        public static IProcedure CreateProcedure(string name, Action<IGlobalState, object, object> proc)
        {
            return new ActionProcedure_GS_2(name, proc);
        }

        
        private class FuncProcedure_2 : IProcedure
        {
            private string name;
            private Func<object, object, object> proc;

            public FuncProcedure_2(string name, Func<object, object, object> proc)
            {
                this.name = name;
                this.proc = proc;
            }

            public int Arity { get { return 2; } }

            public bool More { get { return false; } }

            public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
            {
                                
                object arg1;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg1 = argList.Head;
                    argList = argList.Tail;
                }

                                
                object arg2;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg2 = argList.Head;
                    argList = argList.Tail;
                }

                
                if (argList != null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": too many arguments"));
                }

                
                object result;

                
                try
                {
                    
                    result = proc(arg1, arg2);
                    return new RunnableReturn(k, result);

                    
                }
                catch(Exception exc)
                {
                    return new RunnableReturn(k, exc);
                }
            }
        }

        public static IProcedure CreateProcedure(string name, Func<object, object, object> proc)
        {
            return new FuncProcedure_2(name, proc);
        }

        
        private class FuncProcedure_GS_2 : IProcedure
        {
            private string name;
            private Func<IGlobalState, object, object, object> proc;

            public FuncProcedure_GS_2(string name, Func<IGlobalState, object, object, object> proc)
            {
                this.name = name;
                this.proc = proc;
            }

            public int Arity { get { return 2; } }

            public bool More { get { return false; } }

            public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
            {
                                
                object arg1;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg1 = argList.Head;
                    argList = argList.Tail;
                }

                                
                object arg2;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg2 = argList.Head;
                    argList = argList.Tail;
                }

                
                if (argList != null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": too many arguments"));
                }

                
                object result;

                
                try
                {
                    
                    result = proc(gs, arg1, arg2);
                    return new RunnableReturn(k, result);

                    
                }
                catch(Exception exc)
                {
                    return new RunnableReturn(k, exc);
                }
            }
        }

        public static IProcedure CreateProcedure(string name, Func<IGlobalState, object, object, object> proc)
        {
            return new FuncProcedure_GS_2(name, proc);
        }

        
        private class ActionProcedure_3 : IProcedure
        {
            private string name;
            private Action<object, object, object> proc;

            public ActionProcedure_3(string name, Action<object, object, object> proc)
            {
                this.name = name;
                this.proc = proc;
            }

            public int Arity { get { return 3; } }

            public bool More { get { return false; } }

            public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
            {
                                
                object arg1;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg1 = argList.Head;
                    argList = argList.Tail;
                }

                                
                object arg2;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg2 = argList.Head;
                    argList = argList.Tail;
                }

                                
                object arg3;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg3 = argList.Head;
                    argList = argList.Tail;
                }

                
                if (argList != null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": too many arguments"));
                }

                
                try
                {
                    
                    proc(arg1, arg2, arg3);
                    return new RunnableReturn(k, SpecialValue.UNSPECIFIED);

                    
                }
                catch(Exception exc)
                {
                    return new RunnableReturn(k, exc);
                }
            }
        }

        public static IProcedure CreateProcedure(string name, Action<object, object, object> proc)
        {
            return new ActionProcedure_3(name, proc);
        }

        
        private class ActionProcedure_GS_3 : IProcedure
        {
            private string name;
            private Action<IGlobalState, object, object, object> proc;

            public ActionProcedure_GS_3(string name, Action<IGlobalState, object, object, object> proc)
            {
                this.name = name;
                this.proc = proc;
            }

            public int Arity { get { return 3; } }

            public bool More { get { return false; } }

            public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
            {
                                
                object arg1;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg1 = argList.Head;
                    argList = argList.Tail;
                }

                                
                object arg2;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg2 = argList.Head;
                    argList = argList.Tail;
                }

                                
                object arg3;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg3 = argList.Head;
                    argList = argList.Tail;
                }

                
                if (argList != null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": too many arguments"));
                }

                
                try
                {
                    
                    proc(gs, arg1, arg2, arg3);
                    return new RunnableReturn(k, SpecialValue.UNSPECIFIED);

                    
                }
                catch(Exception exc)
                {
                    return new RunnableReturn(k, exc);
                }
            }
        }

        public static IProcedure CreateProcedure(string name, Action<IGlobalState, object, object, object> proc)
        {
            return new ActionProcedure_GS_3(name, proc);
        }

        
        private class FuncProcedure_3 : IProcedure
        {
            private string name;
            private Func<object, object, object, object> proc;

            public FuncProcedure_3(string name, Func<object, object, object, object> proc)
            {
                this.name = name;
                this.proc = proc;
            }

            public int Arity { get { return 3; } }

            public bool More { get { return false; } }

            public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
            {
                                
                object arg1;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg1 = argList.Head;
                    argList = argList.Tail;
                }

                                
                object arg2;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg2 = argList.Head;
                    argList = argList.Tail;
                }

                                
                object arg3;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg3 = argList.Head;
                    argList = argList.Tail;
                }

                
                if (argList != null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": too many arguments"));
                }

                
                object result;

                
                try
                {
                    
                    result = proc(arg1, arg2, arg3);
                    return new RunnableReturn(k, result);

                    
                }
                catch(Exception exc)
                {
                    return new RunnableReturn(k, exc);
                }
            }
        }

        public static IProcedure CreateProcedure(string name, Func<object, object, object, object> proc)
        {
            return new FuncProcedure_3(name, proc);
        }

        
        private class FuncProcedure_GS_3 : IProcedure
        {
            private string name;
            private Func<IGlobalState, object, object, object, object> proc;

            public FuncProcedure_GS_3(string name, Func<IGlobalState, object, object, object, object> proc)
            {
                this.name = name;
                this.proc = proc;
            }

            public int Arity { get { return 3; } }

            public bool More { get { return false; } }

            public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
            {
                                
                object arg1;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg1 = argList.Head;
                    argList = argList.Tail;
                }

                                
                object arg2;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg2 = argList.Head;
                    argList = argList.Tail;
                }

                                
                object arg3;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg3 = argList.Head;
                    argList = argList.Tail;
                }

                
                if (argList != null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": too many arguments"));
                }

                
                object result;

                
                try
                {
                    
                    result = proc(gs, arg1, arg2, arg3);
                    return new RunnableReturn(k, result);

                    
                }
                catch(Exception exc)
                {
                    return new RunnableReturn(k, exc);
                }
            }
        }

        public static IProcedure CreateProcedure(string name, Func<IGlobalState, object, object, object, object> proc)
        {
            return new FuncProcedure_GS_3(name, proc);
        }

        
        private class ActionProcedure_4 : IProcedure
        {
            private string name;
            private Action<object, object, object, object> proc;

            public ActionProcedure_4(string name, Action<object, object, object, object> proc)
            {
                this.name = name;
                this.proc = proc;
            }

            public int Arity { get { return 4; } }

            public bool More { get { return false; } }

            public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
            {
                                
                object arg1;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg1 = argList.Head;
                    argList = argList.Tail;
                }

                                
                object arg2;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg2 = argList.Head;
                    argList = argList.Tail;
                }

                                
                object arg3;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg3 = argList.Head;
                    argList = argList.Tail;
                }

                                
                object arg4;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg4 = argList.Head;
                    argList = argList.Tail;
                }

                
                if (argList != null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": too many arguments"));
                }

                
                try
                {
                    
                    proc(arg1, arg2, arg3, arg4);
                    return new RunnableReturn(k, SpecialValue.UNSPECIFIED);

                    
                }
                catch(Exception exc)
                {
                    return new RunnableReturn(k, exc);
                }
            }
        }

        public static IProcedure CreateProcedure(string name, Action<object, object, object, object> proc)
        {
            return new ActionProcedure_4(name, proc);
        }

        
        private class ActionProcedure_GS_4 : IProcedure
        {
            private string name;
            private Action<IGlobalState, object, object, object, object> proc;

            public ActionProcedure_GS_4(string name, Action<IGlobalState, object, object, object, object> proc)
            {
                this.name = name;
                this.proc = proc;
            }

            public int Arity { get { return 4; } }

            public bool More { get { return false; } }

            public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
            {
                                
                object arg1;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg1 = argList.Head;
                    argList = argList.Tail;
                }

                                
                object arg2;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg2 = argList.Head;
                    argList = argList.Tail;
                }

                                
                object arg3;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg3 = argList.Head;
                    argList = argList.Tail;
                }

                                
                object arg4;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg4 = argList.Head;
                    argList = argList.Tail;
                }

                
                if (argList != null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": too many arguments"));
                }

                
                try
                {
                    
                    proc(gs, arg1, arg2, arg3, arg4);
                    return new RunnableReturn(k, SpecialValue.UNSPECIFIED);

                    
                }
                catch(Exception exc)
                {
                    return new RunnableReturn(k, exc);
                }
            }
        }

        public static IProcedure CreateProcedure(string name, Action<IGlobalState, object, object, object, object> proc)
        {
            return new ActionProcedure_GS_4(name, proc);
        }

        
        private class FuncProcedure_4 : IProcedure
        {
            private string name;
            private Func<object, object, object, object, object> proc;

            public FuncProcedure_4(string name, Func<object, object, object, object, object> proc)
            {
                this.name = name;
                this.proc = proc;
            }

            public int Arity { get { return 4; } }

            public bool More { get { return false; } }

            public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
            {
                                
                object arg1;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg1 = argList.Head;
                    argList = argList.Tail;
                }

                                
                object arg2;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg2 = argList.Head;
                    argList = argList.Tail;
                }

                                
                object arg3;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg3 = argList.Head;
                    argList = argList.Tail;
                }

                                
                object arg4;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg4 = argList.Head;
                    argList = argList.Tail;
                }

                
                if (argList != null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": too many arguments"));
                }

                
                object result;

                
                try
                {
                    
                    result = proc(arg1, arg2, arg3, arg4);
                    return new RunnableReturn(k, result);

                    
                }
                catch(Exception exc)
                {
                    return new RunnableReturn(k, exc);
                }
            }
        }

        public static IProcedure CreateProcedure(string name, Func<object, object, object, object, object> proc)
        {
            return new FuncProcedure_4(name, proc);
        }

        
        private class FuncProcedure_GS_4 : IProcedure
        {
            private string name;
            private Func<IGlobalState, object, object, object, object, object> proc;

            public FuncProcedure_GS_4(string name, Func<IGlobalState, object, object, object, object, object> proc)
            {
                this.name = name;
                this.proc = proc;
            }

            public int Arity { get { return 4; } }

            public bool More { get { return false; } }

            public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
            {
                                
                object arg1;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg1 = argList.Head;
                    argList = argList.Tail;
                }

                                
                object arg2;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg2 = argList.Head;
                    argList = argList.Tail;
                }

                                
                object arg3;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg3 = argList.Head;
                    argList = argList.Tail;
                }

                                
                object arg4;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg4 = argList.Head;
                    argList = argList.Tail;
                }

                
                if (argList != null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": too many arguments"));
                }

                
                object result;

                
                try
                {
                    
                    result = proc(gs, arg1, arg2, arg3, arg4);
                    return new RunnableReturn(k, result);

                    
                }
                catch(Exception exc)
                {
                    return new RunnableReturn(k, exc);
                }
            }
        }

        public static IProcedure CreateProcedure(string name, Func<IGlobalState, object, object, object, object, object> proc)
        {
            return new FuncProcedure_GS_4(name, proc);
        }

        
        private class ActionProcedure_5 : IProcedure
        {
            private string name;
            private Action<object, object, object, object, object> proc;

            public ActionProcedure_5(string name, Action<object, object, object, object, object> proc)
            {
                this.name = name;
                this.proc = proc;
            }

            public int Arity { get { return 5; } }

            public bool More { get { return false; } }

            public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
            {
                                
                object arg1;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg1 = argList.Head;
                    argList = argList.Tail;
                }

                                
                object arg2;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg2 = argList.Head;
                    argList = argList.Tail;
                }

                                
                object arg3;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg3 = argList.Head;
                    argList = argList.Tail;
                }

                                
                object arg4;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg4 = argList.Head;
                    argList = argList.Tail;
                }

                                
                object arg5;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg5 = argList.Head;
                    argList = argList.Tail;
                }

                
                if (argList != null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": too many arguments"));
                }

                
                try
                {
                    
                    proc(arg1, arg2, arg3, arg4, arg5);
                    return new RunnableReturn(k, SpecialValue.UNSPECIFIED);

                    
                }
                catch(Exception exc)
                {
                    return new RunnableReturn(k, exc);
                }
            }
        }

        public static IProcedure CreateProcedure(string name, Action<object, object, object, object, object> proc)
        {
            return new ActionProcedure_5(name, proc);
        }

        
        private class ActionProcedure_GS_5 : IProcedure
        {
            private string name;
            private Action<IGlobalState, object, object, object, object, object> proc;

            public ActionProcedure_GS_5(string name, Action<IGlobalState, object, object, object, object, object> proc)
            {
                this.name = name;
                this.proc = proc;
            }

            public int Arity { get { return 5; } }

            public bool More { get { return false; } }

            public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
            {
                                
                object arg1;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg1 = argList.Head;
                    argList = argList.Tail;
                }

                                
                object arg2;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg2 = argList.Head;
                    argList = argList.Tail;
                }

                                
                object arg3;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg3 = argList.Head;
                    argList = argList.Tail;
                }

                                
                object arg4;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg4 = argList.Head;
                    argList = argList.Tail;
                }

                                
                object arg5;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg5 = argList.Head;
                    argList = argList.Tail;
                }

                
                if (argList != null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": too many arguments"));
                }

                
                try
                {
                    
                    proc(gs, arg1, arg2, arg3, arg4, arg5);
                    return new RunnableReturn(k, SpecialValue.UNSPECIFIED);

                    
                }
                catch(Exception exc)
                {
                    return new RunnableReturn(k, exc);
                }
            }
        }

        public static IProcedure CreateProcedure(string name, Action<IGlobalState, object, object, object, object, object> proc)
        {
            return new ActionProcedure_GS_5(name, proc);
        }

        
        private class FuncProcedure_5 : IProcedure
        {
            private string name;
            private Func<object, object, object, object, object, object> proc;

            public FuncProcedure_5(string name, Func<object, object, object, object, object, object> proc)
            {
                this.name = name;
                this.proc = proc;
            }

            public int Arity { get { return 5; } }

            public bool More { get { return false; } }

            public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
            {
                                
                object arg1;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg1 = argList.Head;
                    argList = argList.Tail;
                }

                                
                object arg2;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg2 = argList.Head;
                    argList = argList.Tail;
                }

                                
                object arg3;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg3 = argList.Head;
                    argList = argList.Tail;
                }

                                
                object arg4;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg4 = argList.Head;
                    argList = argList.Tail;
                }

                                
                object arg5;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg5 = argList.Head;
                    argList = argList.Tail;
                }

                
                if (argList != null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": too many arguments"));
                }

                
                object result;

                
                try
                {
                    
                    result = proc(arg1, arg2, arg3, arg4, arg5);
                    return new RunnableReturn(k, result);

                    
                }
                catch(Exception exc)
                {
                    return new RunnableReturn(k, exc);
                }
            }
        }

        public static IProcedure CreateProcedure(string name, Func<object, object, object, object, object, object> proc)
        {
            return new FuncProcedure_5(name, proc);
        }

        
        private class FuncProcedure_GS_5 : IProcedure
        {
            private string name;
            private Func<IGlobalState, object, object, object, object, object, object> proc;

            public FuncProcedure_GS_5(string name, Func<IGlobalState, object, object, object, object, object, object> proc)
            {
                this.name = name;
                this.proc = proc;
            }

            public int Arity { get { return 5; } }

            public bool More { get { return false; } }

            public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
            {
                                
                object arg1;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg1 = argList.Head;
                    argList = argList.Tail;
                }

                                
                object arg2;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg2 = argList.Head;
                    argList = argList.Tail;
                }

                                
                object arg3;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg3 = argList.Head;
                    argList = argList.Tail;
                }

                                
                object arg4;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg4 = argList.Head;
                    argList = argList.Tail;
                }

                                
                object arg5;

                if (argList == null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": insufficient arguments"));
                }
                else
                {
                    arg5 = argList.Head;
                    argList = argList.Tail;
                }

                
                if (argList != null)
                {
                    return new RunnableThrow(k, new SchemeRuntimeException(name + ": too many arguments"));
                }

                
                object result;

                
                try
                {
                    
                    result = proc(gs, arg1, arg2, arg3, arg4, arg5);
                    return new RunnableReturn(k, result);

                    
                }
                catch(Exception exc)
                {
                    return new RunnableReturn(k, exc);
                }
            }
        }

        public static IProcedure CreateProcedure(string name, Func<IGlobalState, object, object, object, object, object, object> proc)
        {
            return new FuncProcedure_GS_5(name, proc);
        }

        
    }
}
