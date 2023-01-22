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
using ExprObjModel.ObjectSystem;

namespace ExprObjModel.Procedures
{
    public static partial class ProxyDiscovery
    {
        [SchemeFunction("object-id")]
        public static OldObjectID MakeObjectID(uint i)
        {
            return new OldObjectID(i);
        }

        [SchemeFunction("object-id?")]
        public static bool IsObjectID(object obj)
        {
            return obj is OldObjectID;
        }

        [SchemeFunction("message?")]
        public static bool IsMessage(object obj)
        {
            return obj is Message<object>;
        }

        [SchemeFunction("message-get-type")]
        public static Symbol MessageGetType(Message<object> msg)
        {
            return msg.Type;
        }

        [SchemeFunction("message-get-signature")]
        public static Signature MessageGetSignature(Message<object> msg)
        {
            return msg.Signature;
        }

        [SchemeFunction("message-get-arguments")]
        public static SchemeHashMap MessageGetArguments(Message<object> msg)
        {
            SchemeHashMap m = new SchemeHashMap();
            foreach (Tuple<Symbol, object> kvp in msg.Arguments)
            {
                m[kvp.Item1] = kvp.Item2;
            }
            return m;
        }

        [SchemeFunction("make-message")]
        public static Message<object> MakeMessage(Symbol type, SchemeHashMap arguments)
        {
            if (arguments.Any(x => !(x.Key is Symbol))) throw new SchemeRuntimeException("make-message : argument names must be symbols");
            return new Message<object>(type, arguments.Select(x => new Tuple<Symbol, object>((Symbol)(x.Key), x.Value)));
        }

        [SchemeFunction("message-has-argument?")]
        public static bool MessageHasArgument(Message<object> msg, Symbol argKey)
        {
            return msg.HasArgument(argKey);
        }

        [SchemeFunction("message-ref")]
        public static object MessageRef(Message<object> msg, Symbol argKey)
        {
            return msg[argKey];
        }

        [SchemeFunction("make-object")]
        public static OldObjectID MakeObject(IGlobalState gs)
        {
            OldObjectID id = gs.AddOldObject("Object", new SchemeObject(gs));
            return id;
        }

        [SchemeFunction("make-log-window")]
        public static OldObjectID MakeLogWindow(IGlobalState gs)
        {
            OldObjectID id = gs.AddOldObject("LogWindow", new LogWindowObject(gs));
            return id;
        }

        [SchemeFunction("make-window-obj-from-bitmap")]
        public static OldObjectID MakeControlledWindowObj(IGlobalState gs, System.Drawing.Bitmap b)
        {
            OldObjectID id = gs.AddOldObject("Controlled Window", new ControlledWindowObject(gs, b));
            return id;
        }

        [SchemeFunction("make-window-obj")]
        public static OldObjectID MakeControlledWindowObj(IGlobalState gs, int width, int height)
        {
            OldObjectID id = gs.AddOldObject("Controlled Window", new ControlledWindowObject(gs, width, height));
            return id;
        }

        [SchemeFunction("set-handler!")]
        public static bool ObjectSetHandler(IGlobalState gs, OldObjectID id, IMsgProcedure handler)
        {
            return gs.OldPostMessage(id, new EM_SetHandler(handler));
        }

        [SchemeFunction("unset-handler!")]
        public static bool ObjectUnsetHandler(IGlobalState gs, OldObjectID id, Signature sig)
        {
            return gs.OldPostMessage(id, new EM_UnsetHandler(sig));
        }

        [SchemeFunction("set-catch-all!")]
        public static bool ObjectSetCatchAll(IGlobalState gs, OldObjectID id, IProcedure proc)
        {
            if (proc.AcceptsParameterCount(1))
            {
                return gs.OldPostMessage(id, new EM_SetCatchAll(proc));
            }
            else
            {
                throw new SchemeRuntimeException("Catchall must accept one parameter");
            }
        }

        [SchemeFunction("unset-catch-all!")]
        public static bool ObjectUnsetCatchAll(IGlobalState gs, OldObjectID id)
        {
            return gs.OldPostMessage(id, new EM_UnsetCatchAll());
        }

        [SchemeFunction("post!")]
        public static bool ObjectPost(IGlobalState gs, OldObjectID id, Message<object> msg)
        {
            return gs.OldPostMessage(id, new EM_SchemeMessage(msg));
        }

        [SchemeFunction("self")]
        public static object ObjectSelf(IGlobalState gs)
        {
            if (gs.CurrentObject == null)
            {
                return false;
            }
            else
            {
                return gs.CurrentObject.Self;
            }
        }

        [SchemeFunction("list-objects")]
        public static object ListObjects(IGlobalState gs)
        {
            object r = SpecialValue.EMPTY_LIST;
            foreach (Tuple<OldObjectID, string> t in gs.ListOldObjects())
            {
                SchemeHashMap s = new SchemeHashMap();
                s[new Symbol("id")] = t.Item1;
                s[new Symbol("desc")] = new SchemeString(t.Item2);
                r = new ConsCell(s, r);
            }
            ConsCell.Reverse(ref r);
            return r;
        }

        [SchemeFunction("list-handlers")]
        public static SchemeHashSet ObjectListHandlers(IGlobalState gs, OldObjectID id)
        {
            using (ControlledWindowLib.AsyncQueue<ExtendedMessage> q = new ControlledWindowLib.AsyncQueue<ExtendedMessage>())
            {
                TemporaryObject te = new TemporaryObject(gs, q);
                OldObjectID myid = gs.AddOldObject("list-handlers", te);
                if (gs.OldPostMessage(id, new EM_GetHandlerList(myid, false)))
                {
                    ExtendedMessage e = q.Get();
                    q.Dispose();
                    if (e is EM_HandlerListResponse)
                    {
                        EM_HandlerListResponse hlr = (EM_HandlerListResponse)e;
                        SchemeHashSet s = SchemeHashSet.FromEnumerable(hlr.Value);
                        return s;
                    }
                    else
                    {
                        throw new SchemeRuntimeException("Received the wrong type of response: " + e.GetType());
                    }
                }
                else
                {
                    throw new SchemeRuntimeException("Unable to queue the request (does the object still exist?)");
                }
            }
        }

        [SchemeFunction("begin-list-handlers")]
        public static AsyncID ObjectBeginListHandlers(IGlobalState gs, OldObjectID id)
        {
            using (ControlledWindowLib.AsyncQueue<ExtendedMessage> q = new ControlledWindowLib.AsyncQueue<ExtendedMessage>())
            {
                TemporaryObject te = new TemporaryObject(gs, q);
                OldObjectID myid = gs.AddOldObject("list-handlers", te);
                if (gs.OldPostMessage(id, new EM_GetHandlerList(myid, false)))
                {
                    AdvancedCompletionProc acp = delegate(AsyncID a, IGlobalState gs2, IAsyncResult iar2)
                    {
                        ExtendedMessage e = q.EndGet(iar2);
                        q.Dispose();
                        if (e is EM_HandlerListResponse)
                        {
                            EM_HandlerListResponse hlr = (EM_HandlerListResponse)e;
                            SchemeHashSet s = SchemeHashSet.FromEnumerable(hlr.Value);
                            return new WaitResult()
                            {
                                id = a,
                                result = s,
                                isException = false
                            };
                        }
                        else
                        {
                            return new WaitResult()
                            {
                                id = a,
                                result = new SchemeRuntimeException("Received the wrong type of response: " + e.GetType()),
                                isException = true
                            };
                        }
                    };
                    AsyncID a2 = gs.RegisterAsync(q.BeginGet(null, null), acp, "begin-list-handlers");
                    return a2;
                }
                else
                {
                    throw new SchemeRuntimeException("Unable to queue the request (does the object still exist?)");
                }
            }
        }

        [SchemeFunction("list-locals")]
        public static SchemeHashSet ObjectListLocals(IGlobalState gs, OldObjectID id)
        {
            using (ControlledWindowLib.AsyncQueue<ExtendedMessage> q = new ControlledWindowLib.AsyncQueue<ExtendedMessage>())
            {
                TemporaryObject te = new TemporaryObject(gs, q);
                OldObjectID myid = gs.AddOldObject("list-fields", te);
                if (gs.OldPostMessage(id, new EM_GetFieldList(myid, false)))
                {
                    ExtendedMessage e = q.Get();
                    q.Dispose();
                    if (e is EM_FieldListResponse)
                    {
                        EM_FieldListResponse flr = (EM_FieldListResponse)e;
                        SchemeHashSet s = SchemeHashSet.FromEnumerable(flr.Value);
                        return s;
                    }
                    else
                    {
                        throw new SchemeRuntimeException("Received the wrong type of response: " + e.GetType());
                    }
                }
                else
                {
                    throw new SchemeRuntimeException("Unable to queue the request (does the object still exist?)");
                }
            }
        }

        [SchemeFunction("begin-list-locals")]
        public static AsyncID ObjectBeginListLocals(IGlobalState gs, OldObjectID id)
        {
            using (ControlledWindowLib.AsyncQueue<ExtendedMessage> q = new ControlledWindowLib.AsyncQueue<ExtendedMessage>())
            {
                TemporaryObject te = new TemporaryObject(gs, q);
                OldObjectID myid = gs.AddOldObject("list-fields", te);
                if (gs.OldPostMessage(id, new EM_GetFieldList(myid, false)))
                {
                    AdvancedCompletionProc acp = delegate(AsyncID a, IGlobalState gs2, IAsyncResult iar2)
                    {
                        ExtendedMessage e = q.EndGet(iar2);
                        q.Dispose();
                        if (e is EM_FieldListResponse)
                        {
                            EM_FieldListResponse flr = (EM_FieldListResponse)e;
                            SchemeHashSet s = SchemeHashSet.FromEnumerable(flr.Value);
                            return new WaitResult()
                            {
                                id = a,
                                result = s,
                                isException = false
                            };
                        }
                        else
                        {
                            return new WaitResult()
                            {
                                id = a,
                                result = new SchemeRuntimeException("Received the wrong type of response: " + e.GetType()),
                                isException = true
                            };
                        }
                    };
                    AsyncID a2 = gs.RegisterAsync(q.BeginGet(null, null), acp, "begin-list-fields");
                    return a2;
                }
                else
                {
                    throw new SchemeRuntimeException("Unable to queue the request (does the object still exist?)");
                }
            }
        }

        [SchemeFunction("add-local!")]
        public static void ObjectAddField(IGlobalState gs, OldObjectID id, Symbol fieldName, object initialValue)
        {
            gs.OldPostMessage(id, new EM_AddField(fieldName, initialValue));
        }

        [SchemeFunction("remove-local!")]
        public static void ObjectRemoveField(IGlobalState gs, OldObjectID id, Symbol fieldName)
        {
            gs.OldPostMessage(id, new EM_RemoveField(fieldName));
        }

        [SchemeFunction("remove-object!")]
        public static void ObjectRemove(IGlobalState gs, OldObjectID id)
        {
            gs.RemoveOldObject(id);
        }
    }

    [SchemeSingleton("message-has-signature?")]
    public class MsgHasSignatureProcedure : IProcedure
    {
        public MsgHasSignatureProcedure() { }

        public int Arity { get { return 1; } }
        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            if (argList == null) return new RunnableThrow(k, new SchemeRuntimeException("message-has-signature?: Insufficient arguments"));
            if (!(argList.Head is Message<object>)) return new RunnableReturn(k, false);
            Signature s1 = ((Message<object>)(argList.Head)).Signature;
            FList<object> testSignatures = argList.Tail;
            if (testSignatures == null) return new RunnableReturn(k, true);
            while (testSignatures != null)
            {
                object o2 = testSignatures.Head;
                testSignatures = testSignatures.Tail;
                if (!(o2 is Signature)) return new RunnableThrow(k, new SchemeRuntimeException("message-has-signature?: Signature expected"));
                Signature s2 = (Signature)o2;
                if (s1 == s2) return new RunnableReturn(k, true);
            }
            return new RunnableReturn(k, false);
        }
    }

    [SchemeSingleton("minvoke")]
    public class MsgInvokeProcedure : IProcedure
    {
        public MsgInvokeProcedure() { }

        #region IProcedure Members

        public int Arity { get { return 2; } }
        public bool More { get { return false; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            try
            {
                IMsgProcedure proc = null;
                Message<object> m = null;
                FList<object> a1 = argList;
                if (a1 == null) throw new SchemeRuntimeException("minvoke: insufficient args");
                if (!(a1.Head is IMsgProcedure)) throw new SchemeRuntimeException("minvoke: first arg is not an IMsgProcedure");
                proc = (IMsgProcedure)a1.Head;
                a1 = a1.Tail;
                if (a1 == null) throw new SchemeRuntimeException("minvoke: insufficient args");
                if (!(a1.Head is Message<object>)) throw new SchemeRuntimeException("minvoke: second arg is not a Message");
                m = (Message<object>)a1.Head;
                a1 = a1.Tail;
                if (a1 != null) throw new SchemeRuntimeException("minvoke: excessive args");

                return new RunnableMsgCall(proc, m, k);
            }
            catch(Exception exc)
            {
                return new RunnableThrow(k, exc);
            }
        }

        #endregion
    }

    [SchemeSingleton("with-get-from-temporary")]
    public class WithGetFromTemporary : IProcedure
    {
        public WithGetFromTemporary() { }

        public int Arity { get { return 1; } }
        public bool More { get { return false; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            try
            {
                // create a temporary object
                // start a "get" on it
                // pass the ObjectID and the AsyncID to the proc received as a paraemter

                // the proc does a post! using the ObjectID
                // the proc can do a WaitAny on the AsyncID to get the response

                IProcedure proc;

                FList<object> a1 = argList;
                if (a1 == null) return new RunnableThrow(k, new SchemeRuntimeException("with-get-from-temporary: insufficient args"));
                
                if (!(a1.Head is IProcedure)) return new RunnableThrow(k, new SchemeRuntimeException("with-get-from-temporary: first arg must be an ObjectID"));
                proc = (IProcedure)a1.Head;
                a1 = a1.Tail;

                if (a1 != null) return new RunnableThrow(k, new SchemeRuntimeException("with-get-from-temporary: excessive args"));

                if (proc.Arity == 2 || (proc.Arity < 2 && proc.More))
                {
                    ControlledWindowLib.AsyncQueue<ExtendedMessage> q = new ControlledWindowLib.AsyncQueue<ExtendedMessage>();
                    TemporaryObject te = new TemporaryObject(gs, q);
                    OldObjectID tempobj = gs.AddOldObject("temporary", te);
                    AdvancedCompletionProc acp = delegate(AsyncID a, IGlobalState gs2, IAsyncResult iar2)
                    {
                        try
                        {
                            ExtendedMessage em = q.EndGet(iar2);
                            if (em is EM_SchemeMessage)
                            {
                                EM_SchemeMessage sm = (EM_SchemeMessage)em;
                                return new WaitResult()
                                {
                                    id = a,
                                    result = sm.Message,
                                    isException = false
                                };
                            }
                            else
                            {
                                return new WaitResult()
                                {
                                    id = a,
                                    result = new SchemeRuntimeException("Received the wrong type of response: " + em.GetType()),
                                    isException = true
                                };
                            }
                        }
                        finally
                        {
                            q.Dispose();
                        }
                    };
                    IAsyncResult iar = q.BeginGet(null, null);
                    AsyncID getop = gs.RegisterAsync(iar, acp, "get-from-temporary");

                    FList<object> results = new FList<object>(tempobj, new FList<object>(getop, null));

                    return new RunnableCall(proc, results, k);

                }
                else
                {
                    return new RunnableThrow(k, new SchemeRuntimeException("with-get-from-temporary: insufficient args"));
                }
            }
            catch (Exception exc)
            {
                return new RunnableThrow(k, exc);
            }
        }
    }

    [SchemeSingleton("post-later!")]
    public class PostLaterProcedure : IProcedure
    {
        public PostLaterProcedure() { }

        public int Arity { get { return 3; } }
        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            try
            {
                if (argList == null) throw new SchemeRuntimeException("post-later!: Insufficient arguments");
                if (!(argList.Head is BigMath.BigInteger)) throw new SchemeRuntimeException("post-later!: first argument must be an integer");
                BigMath.BigInteger biDelay = (BigMath.BigInteger)(argList.Head);
                if (!(biDelay.FitsInUInt32)) throw new SchemeRuntimeException("post-later!: first argument out of range");
                uint delay_ms = biDelay.GetUInt32Value(BigMath.OverflowBehavior.Wraparound);
                argList = argList.Tail;
                if (argList == null) throw new SchemeRuntimeException("post-later!: Insufficient arguments");
                if (!(argList.Head is OldObjectID)) throw new SchemeRuntimeException("post-later!: second argument must be an ObjectID");
                OldObjectID oiDest = (OldObjectID)(argList.Head);
                argList = argList.Tail;
                if (argList == null) throw new SchemeRuntimeException("post-later!: Insufficient arguments");
                if (!(argList.Head is Message<object>)) throw new SchemeRuntimeException("post-later!: third argument must be a message");
                Message<object> msg = (Message<object>)(argList.Head);
                argList = argList.Tail;
                if (argList == null)
                {
                    PostLater(gs, delay_ms, oiDest, msg, null);
                    return new RunnableReturn(k, SpecialValue.UNSPECIFIED);
                }
                else
                {
                    if (!(argList.Head is IProcedure)) throw new SchemeRuntimeException("post-later!: fourth argument must be a procedure");
                    IProcedure proc = (IProcedure)(argList.Head);
                    if (!(proc.AcceptsParameterCount(1))) throw new SchemeRuntimeException("post-later!: fourth argument must accept one parameter");
                    argList = argList.Tail;
                    if (argList != null) throw new SchemeRuntimeException("post-later: Too many arguments");
                    PostLater(gs, delay_ms, oiDest, msg, proc);
                    return new RunnableReturn(k, SpecialValue.UNSPECIFIED);
                }
            }
            catch (Exception exc)
            {
                return new RunnableThrow(k, exc);
            }
        }

        private static void PostLater(IGlobalState gs, uint delay_ms, OldObjectID id, Message<object> message, IProcedure cancelled)
        {
            Action<ExtendedMessage> aCancelled = null;
            if (cancelled != null)
            {
                aCancelled = delegate(ExtendedMessage em)
                {
                    //System.Diagnostics.Debug.WriteLine("Entered cancellation");
                    if (em is EM_SchemeMessage)
                    {
                        EM_SchemeMessage emsm = (EM_SchemeMessage)em;
                        Doer.Apply(gs, cancelled, new FList<object>(emsm.Message));
                    }
                };
            }
            else
            {
                aCancelled = delegate(ExtendedMessage em)
                {
                    // do nothing
                };
            }

            gs.OldPostMessageLater(delay_ms, id, new EM_SchemeMessage(message), aCancelled);
        }
    }
}