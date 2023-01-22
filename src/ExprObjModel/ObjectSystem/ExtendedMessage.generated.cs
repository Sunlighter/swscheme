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

namespace ExprObjModel.ObjectSystem
{
    
        public abstract class ExtendedMessage
        {
        }

        public class EM_SchemeMessage : ExtendedMessage
        {
            private Message<object> msg;

            public EM_SchemeMessage
            (
                Message<object> msg
            )
            {
                this.msg = msg;
            }

            public Message<object> Message { get { return msg; } }
        }

        public class EM_SetHandler : ExtendedMessage
        {
            private IMsgProcedure mproc;

            public EM_SetHandler
            (
                IMsgProcedure mproc
            )
            {
                this.mproc = mproc;
            }

            public IMsgProcedure MProc { get { return mproc; } }
        }

        public class EM_UnsetHandler : ExtendedMessage
        {
            private Signature sig;

            public EM_UnsetHandler
            (
                Signature sig
            )
            {
                this.sig = sig;
            }

            public Signature Signature { get { return sig; } }
        }

        public class EM_SetCatchAll : ExtendedMessage
        {
            private IProcedure proc;

            public EM_SetCatchAll
            (
                IProcedure proc
            )
            {
                this.proc = proc;
            }

            public IProcedure Proc { get { return proc; } }
        }

        public class EM_UnsetCatchAll : ExtendedMessage
        {
            public EM_UnsetCatchAll
            (
            )
            {
            }
        }

        public class EM_AddField : ExtendedMessage
        {
            private Symbol fieldName;
            private object initialValue;

            public EM_AddField
            (
                Symbol fieldName,
                object initialValue
            )
            {
                this.fieldName = fieldName;
                this.initialValue = initialValue;
            }

            public Symbol FieldName { get { return fieldName; } }
            public object InitialValue { get { return initialValue; } }
        }

        public class EM_RemoveField : ExtendedMessage
        {
            private Symbol fieldName;

            public EM_RemoveField
            (
                Symbol fieldName
            )
            {
                this.fieldName = fieldName;
            }

            public Symbol FieldName { get { return fieldName; } }
        }

        public class EM_GetHandlerList : ExtendedMessage
        {
            private OldObjectID k;
            private object kdata;

            public EM_GetHandlerList
            (
                OldObjectID k,
                object kdata
            )
            {
                this.k = k;
                this.kdata = kdata;
            }

            public OldObjectID K { get { return k; } }
            public object KData { get { return kdata; } }
        }

        public class EM_HandlerListResponse : ExtendedMessage
        {
            private HashSet<Signature> v;
            private object kdata;

            public EM_HandlerListResponse
            (
                HashSet<Signature> v,
                object kdata
            )
            {
                this.v = v;
                this.kdata = kdata;
            }

            public HashSet<Signature> Value { get { return v; } }
            public object KData { get { return kdata; } }
        }

        public class EM_GetFieldList : ExtendedMessage
        {
            private OldObjectID k;
            private object kdata;

            public EM_GetFieldList
            (
                OldObjectID k,
                object kdata
            )
            {
                this.k = k;
                this.kdata = kdata;
            }

            public OldObjectID K { get { return k; } }
            public object KData { get { return kdata; } }
        }

        public class EM_FieldListResponse : ExtendedMessage
        {
            private HashSet<Symbol> v;
            private object kdata;

            public EM_FieldListResponse
            (
                HashSet<Symbol> v,
                object kdata
            )
            {
                this.v = v;
                this.kdata = kdata;
            }

            public HashSet<Symbol> Value { get { return v; } }
            public object KData { get { return kdata; } }
        }

        public class EM_Close : ExtendedMessage
        {
            public EM_Close
            (
            )
            {
            }
        }

        public class EM_PoolQueue : ExtendedMessage
        {
            private int priority;
            private Action action;

            public EM_PoolQueue
            (
                int priority,
                Action action
            )
            {
                this.priority = priority;
                this.action = action;
            }

            public int Priority { get { return priority; } }
            public Action Action { get { return action; } }
        }

        public class EM_PoolComplete : ExtendedMessage
        {
            private int thread;

            public EM_PoolComplete
            (
                int thread
            )
            {
                this.thread = thread;
            }

            public int Thread { get { return thread; } }
        }

}