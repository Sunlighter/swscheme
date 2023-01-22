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
    public partial class ObjectSystem<M>
    {

        
        private abstract class InternalCommand
        {
        }

        private class IC_AddObject : InternalCommand
        {
            private IMessageHandler<M> obj;
            private AsyncContinuation<OldObjectID> k;

            public IC_AddObject
            (
                IMessageHandler<M> obj,
                AsyncContinuation<OldObjectID> k
            )
            {
                this.obj = obj;
                this.k = k;
            }

            public IMessageHandler<M> Object { get { return obj; } }
            public AsyncContinuation<OldObjectID> K { get { return k; } }
        }

        private class IC_RemoveObject : InternalCommand
        {
            private OldObjectID id;

            public IC_RemoveObject
            (
                OldObjectID id
            )
            {
                this.id = id;
            }

            public OldObjectID ID { get { return id; } }
        }

        private class IC_PostMessage : InternalCommand
        {
            private OldObjectID id;
            private M msg;
            private AsyncContinuation<bool> k;

            public IC_PostMessage
            (
                OldObjectID id,
                M msg,
                AsyncContinuation<bool> k
            )
            {
                this.id = id;
                this.msg = msg;
                this.k = k;
            }

            public OldObjectID ID { get { return id; } }
            public M Message { get { return msg; } }
            public AsyncContinuation<bool> K { get { return k; } }
        }

        private class IC_PostMessageLater : InternalCommand
        {
            private uint delay_ms;
            private OldObjectID id;
            private M msg;
            private Action<M> cancelled;

            public IC_PostMessageLater
            (
                uint delay_ms,
                OldObjectID id,
                M msg,
                Action<M> cancelled
            )
            {
                this.delay_ms = delay_ms;
                this.id = id;
                this.msg = msg;
                this.cancelled = cancelled;
            }

            public uint Delay_ms { get { return delay_ms; } }
            public OldObjectID ID { get { return id; } }
            public M Message { get { return msg; } }
            public Action<M> Cancelled { get { return cancelled; } }
        }

        private class IC_PostMessageOnCompletion : InternalCommand
        {
            private OldObjectID id;
            private IAsyncResult iar;
            private Func<IAsyncResult, M> completion;
            private Action<M> cancelled;

            public IC_PostMessageOnCompletion
            (
                OldObjectID id,
                IAsyncResult iar,
                Func<IAsyncResult, M> completion,
                Action<M> cancelled
            )
            {
                this.id = id;
                this.iar = iar;
                this.completion = completion;
                this.cancelled = cancelled;
            }

            public OldObjectID ID { get { return id; } }
            public IAsyncResult AsyncResult { get { return iar; } }
            public Func<IAsyncResult, M> Completion { get { return completion; } }
            public Action<M> Cancelled { get { return cancelled; } }
        }

        private class IC_MessageComplete : InternalCommand
        {
            private OldObjectID id;
            private int thread;

            public IC_MessageComplete
            (
                OldObjectID id,
                int thread
            )
            {
                this.id = id;
                this.thread = thread;
            }

            public OldObjectID ID { get { return id; } }
            public int Thread { get { return thread; } }
        }

        private class IC_Shutdown : InternalCommand
        {
            public IC_Shutdown
            (
            )
            {
            }
        }

        private abstract class InternalQueueItem
        {
        }

        private class IQI_Post : InternalQueueItem
        {
            private OldObjectID id;
            private M msg;

            public IQI_Post
            (
                OldObjectID id,
                M msg
            )
            {
                this.id = id;
                this.msg = msg;
            }

            public OldObjectID ID { get { return id; } }
            public M Message { get { return msg; } }
        }

        private class IQI_Revisit : InternalQueueItem
        {
            private OldObjectID id;

            public IQI_Revisit
            (
                OldObjectID id
            )
            {
                this.id = id;
            }

            public OldObjectID ID { get { return id; } }
        }

    }
}