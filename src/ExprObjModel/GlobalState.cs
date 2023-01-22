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
using System.Threading;
using System.Runtime.Serialization;
using ControlledWindowLib.Scheduling;
using ExprObjModel.ObjectSystem;

namespace ExprObjModel
{
    public struct DisposableID : IEquatable<DisposableID>
    {
        public ushort id;

        public DisposableID(ushort id)
        {
            this.id = id;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is DisposableID)) return false;
            DisposableID d = (DisposableID)obj;
            return this.id == d.id;
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public override string ToString()
        {
            return "(disposable id: " + id.ToString() + ")";
        }

        public static bool operator == (DisposableID a, DisposableID b)
        {
            return a.id == b.id;
        }

        public static bool operator != (DisposableID a, DisposableID b)
        {
            return a.id != b.id;
        }

        public static explicit operator ushort (DisposableID d)
        {
            return d.id;
        }

        public static explicit operator DisposableID(ushort d)
        {
            return new DisposableID(d);
        }

        public static DisposableID operator++(DisposableID d)
        {
            return new DisposableID(unchecked((ushort)(d.id + 1u)));
        }
    
        public bool Equals(DisposableID other)
        {
         	return this.id == other.id;
        }

        public static DisposableID Zero { get { return new DisposableID((ushort)0u); } }
    }

    public struct AsyncID : IEquatable<AsyncID>
    {
        public ushort id;

        public AsyncID(ushort id)
        {
            this.id = id;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is AsyncID)) return false;
            AsyncID d = (AsyncID)obj;
            return this.id == d.id;
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public override string ToString()
        {
            return "(async id: " + id.ToString() + ")";
        }

        public static bool operator == (AsyncID a, AsyncID b)
        {
            return a.id == b.id;
        }

        public static bool operator != (AsyncID a, AsyncID b)
        {
            return a.id != b.id;
        }

        public static explicit operator ushort(AsyncID d)
        {
            return d.id;
        }

        public static explicit operator AsyncID(ushort d)
        {
            return new AsyncID(d);
        }

        public static AsyncID operator ++(AsyncID d)
        {
            return new AsyncID(unchecked((ushort)(d.id + 1u)));
        }

        public bool Equals(AsyncID other)
        {
            return this.id == other.id;
        }

        public static AsyncID Zero { get { return new AsyncID((ushort)0u); } }
    }

    public class WaitResult
    {
        public AsyncID id;
        public object result;
        public bool isException;
    }

    public class AsyncInfo
    {
        public AsyncID id;
        public string desc;
        public bool waitInProgress;
    }

    public enum AsyncType
    {
        Real,
        Fake,
        Undefined
    }

    [Obsolete]
    public delegate object CompletionProc(IGlobalState gs, IAsyncResult iar);

    [Obsolete]
    public delegate WaitResult AdvancedCompletionProc(AsyncID a, IGlobalState gs, IAsyncResult iar);

    public interface IGlobalState : IDisposable
    {
        DisposableID RegisterDisposable(IDisposable item, string desc);
        IDisposable GetDisposableByID(DisposableID id);
        IEnumerable<Tuple<DisposableID, string>> ListDisposables();
        void DisposeByID(DisposableID id);

        [Obsolete]
        AsyncID RegisterAsync(IAsyncResult iar, CompletionProc completion, string desc);

        [Obsolete]
        AsyncID RegisterAsync(IAsyncResult iar, AdvancedCompletionProc completion, string desc);

        WaitResult WaitOne(AsyncID asyncId);
        bool IsCompleted(AsyncID asyncId);
        WaitResult WaitAny(AsyncID[] asyncIds);
        IEnumerable<AsyncInfo> ListAsyncs();

        OldObjectID AddOldObject(string desc, IMessageHandler<ExtendedMessage> obj);
        bool OldPostMessage(OldObjectID dest, ExtendedMessage msg);
        void OldPostMessageLater(uint delay_ms, OldObjectID dest, ExtendedMessage msg, Action<ExtendedMessage> cancelled);
        void RemoveOldObject(OldObjectID obj);
        IEnumerable<Tuple<OldObjectID, string>> ListOldObjects();

        SchemeObject CurrentObject { get; }
        IGlobalState WithCurrentObject(SchemeObject obj);

        IConsole Console { get; }

        Scheduler Scheduler { get; }

        void RegisterSignal(SignalID sig, string text, bool isUserDefined);
        bool IsUserDefinedSignal(SignalID sig);
        void UnregisterSignal(SignalID sig);
        IEnumerable<Tuple<SignalID, string>> ListSignals();

        void RegisterObject(ObjectID obj, string text);
        void UnregisterObject(ObjectID obj);
        IEnumerable<Tuple<ObjectID, string>> ListObjects();
    }

    public class GlobalState : IGlobalState
    {
        private class AsyncInfoInternal
        {
            public IAsyncResult iar;
            public AdvancedCompletionProc completion;
            public string desc;
            public bool waitInProgress;
        }

        private Dictionary<DisposableID, Tuple<IDisposable, string>> disposables;
        private Dictionary<AsyncID, AsyncInfoInternal> asyncs;

        private DisposableID nextDisposableID;
        private AsyncID nextAsyncID;

        private ReaderWriterLockSlim theLock;

        private ObjectSystem<ExtendedMessage> objectSystem;
        private Dictionary<OldObjectID, string> objectDescriptions;

        private IConsole console;

        private Scheduler scheduler;
        private Dictionary<SignalID, string> signals;
        private HashSet<SignalID> userDefinedSignals;
        private Dictionary<ObjectID, string> objects;

        public GlobalState(Scheduler scheduler, IConsole console)
        {
            disposables = new Dictionary<DisposableID, Tuple<IDisposable, string>>();
            asyncs = new Dictionary<AsyncID, AsyncInfoInternal>();
            theLock = new ReaderWriterLockSlim();
            objectSystem = new ObjectSystem<ExtendedMessage>(System.Environment.ProcessorCount);
            objectDescriptions = new Dictionary<OldObjectID, string>();
            this.console = console;
            this.scheduler = scheduler;
            this.signals = new Dictionary<SignalID, string>();
            this.userDefinedSignals = new HashSet<SignalID>();
            this.objects = new Dictionary<ObjectID, string>();
        }

        private DisposableID GetUnusedDisposableID()
        {
            DisposableID a = nextDisposableID;
            ++nextDisposableID;
            return a;
        }

        public DisposableID RegisterDisposable(IDisposable item, string desc)
        {
            theLock.EnterWriteLock();
            try
            {
                ObjectIDGenerator idgen = new ObjectIDGenerator();
                Dictionary<long, DisposableID> d = new Dictionary<long, DisposableID>();
                foreach (KeyValuePair<DisposableID, Tuple<IDisposable, string>> kvp in disposables)
                {
                    long id = idgen.GetId(kvp.Value.Item1);
                    d.Add(id, kvp.Key);
                }
                long id2 = idgen.GetId(item);
                if (d.ContainsKey(id2))
                {
                    return d[id2]; // an existing object was re-registered
                }
                else
                {
                    DisposableID key = GetUnusedDisposableID();
                    disposables.Add(key, new Tuple<IDisposable, string>(item, desc));
                    return key;
                }
            }
            finally
            {
                theLock.ExitWriteLock();
            }
        }

        public IDisposable GetDisposableByID(DisposableID id)
        {
            theLock.EnterReadLock();
            try
            {
                if (!(disposables.ContainsKey(id))) throw new SchemeRuntimeException("Disposable object not found");
                return disposables[id].Item1;
            }
            finally
            {
                theLock.ExitReadLock();
            }
        }

        private IEnumerable<Tuple<DisposableID, string>> ListDisposablesInner()
        {
            foreach (KeyValuePair<DisposableID, Tuple<IDisposable, string>> kvp in disposables)
            {
                yield return new Tuple<DisposableID, string>(kvp.Key, kvp.Value.Item2);
            }
        }

        public IEnumerable<Tuple<DisposableID, string>> ListDisposables()
        {
            List<Tuple<DisposableID, string>> results = new List<Tuple<DisposableID, string>>();
            theLock.EnterReadLock();
            try
            {
                results.AddRange(ListDisposablesInner());
            }
            finally
            {
                theLock.ExitReadLock();
            }
            return results.AsReadOnly();
        }

        public void DisposeByID(DisposableID id)
        {
            theLock.EnterWriteLock();
            try
            {
                if (!(disposables.ContainsKey(id))) return;
                IDisposable item = disposables[id].Item1;
                disposables.Remove(id);
                item.Dispose();
            }
            finally
            {
                theLock.ExitWriteLock();
            }
        }

        private AsyncID GetFirstUnusedAsyncID()
        {
            AsyncID a = nextAsyncID;
            ++nextAsyncID;
            return a;
        }

        private static AdvancedCompletionProc MakeAdvanced(CompletionProc completion)
        {
            AdvancedCompletionProc a = delegate(AsyncID asyncId, IGlobalState gs, IAsyncResult iar)
            {
                try
                {
                    return new WaitResult() { id = asyncId, result = completion(gs, iar), isException = false };
                }
                catch (Exception exc)
                {
                    return new WaitResult() { id = asyncId, result = exc, isException = true };
                }
            };
            return a;
        }

        public AsyncID RegisterAsync(IAsyncResult iar, CompletionProc completion, string desc)
        {
            return RegisterAsync(iar, MakeAdvanced(completion), desc);
        }

        public AsyncID RegisterAsync(IAsyncResult iar, AdvancedCompletionProc completion, string desc)
        {
            theLock.EnterWriteLock();
            try
            {
                AsyncID key = GetFirstUnusedAsyncID();
                asyncs.Add(key, new AsyncInfoInternal() { iar = iar, completion = completion, desc = desc, waitInProgress = false });
                return key;
            }
            finally
            {
                theLock.ExitWriteLock();
            }
        }

        public WaitResult WaitOne(AsyncID asyncId)
        {
            theLock.EnterWriteLock();
            AsyncInfoInternal tuple;
            try
            {
                if (!(asyncs.ContainsKey(asyncId))) throw new SchemeRuntimeException("Async operation not found");
                tuple = asyncs[asyncId];
                if (tuple.waitInProgress) throw new SchemeRuntimeException("Only one thread at a time may wait on an Async operation");
                tuple.waitInProgress = true;
            }
            finally
            {
                theLock.ExitWriteLock();
            }
            tuple.iar.AsyncWaitHandle.WaitOne();
            theLock.EnterWriteLock();
            try
            {
                asyncs.Remove(asyncId);
            }
            finally
            {
                theLock.ExitWriteLock();
            }
            return tuple.completion(asyncId, this, tuple.iar);
        }

        public bool IsCompleted(AsyncID asyncId)
        {
            theLock.EnterReadLock();
            try
            {
                if (!(asyncs.ContainsKey(asyncId))) throw new SchemeRuntimeException("Async operation not found");
                AsyncInfoInternal tuple = asyncs[asyncId];
                return tuple.iar.IsCompleted;
            }
            finally
            {
                theLock.ExitReadLock();
            }
        }

        public WaitResult WaitAny(AsyncID[] asyncIds)
        {
            theLock.EnterWriteLock();
            WaitHandle[] waitHandles;
            try
            {
                if (asyncIds.Any(x => !asyncs.ContainsKey(x))) throw new SchemeRuntimeException("Async operation not found");
                
                var tuples = asyncIds.Select(x => asyncs[x]);

                foreach (AsyncInfoInternal tuple in tuples)
                {
                    if (tuple.waitInProgress) throw new SchemeRuntimeException("Only one thread at a time may wait on an Async operation");
                }
                foreach (AsyncInfoInternal tuple in tuples)
                {
                    tuple.waitInProgress = true;
                }
                waitHandles = asyncIds.Select(x => asyncs[x].iar.AsyncWaitHandle).ToArray();
            }
            finally
            {
                theLock.ExitWriteLock();
            }
            int index = WaitHandle.WaitAny(waitHandles);
            AsyncInfoInternal tuple3;
            AsyncID a = asyncIds[index];

            theLock.EnterWriteLock();
            try
            {
                tuple3 = asyncs[a];
                asyncs.Remove(a);
                foreach (AsyncInfoInternal tuple2 in asyncIds.Where(x => x != a).Select(x => asyncs[x]))
                {
                    tuple2.waitInProgress = false;
                }
            }
            finally
            {
                theLock.ExitWriteLock();
            }

            return tuple3.completion(a, this, tuple3.iar);
        }

        private IEnumerable<AsyncInfo> ListAsyncsInner()
        {
            foreach (KeyValuePair<AsyncID, AsyncInfoInternal> kvp in asyncs)
            {
                yield return new AsyncInfo() { id = kvp.Key, desc = kvp.Value.desc, waitInProgress = kvp.Value.waitInProgress };
            }
        }

        public IEnumerable<AsyncInfo> ListAsyncs()
        {
            List<AsyncInfo> snapshot = new List<AsyncInfo>();
            theLock.EnterReadLock();
            try
            {
                snapshot.AddRange(ListAsyncsInner());
            }
            finally
            {
                theLock.ExitReadLock();
            }
            return snapshot.AsReadOnly();
        }

        public OldObjectID AddOldObject(string desc, IMessageHandler<ExtendedMessage> obj)
        {
            OldObjectID id = objectSystem.AddObject(obj);
            theLock.EnterWriteLock();
            try
            {
                objectDescriptions.Add(id, desc);
            }
            finally
            {
                theLock.ExitWriteLock();
            }
            return id;
        }

        public bool OldPostMessage(OldObjectID dest, ExtendedMessage msg)
        {
            return objectSystem.Post(dest, msg);
        }

        public void OldPostMessageLater(uint delay_ms, OldObjectID dest, ExtendedMessage msg, Action<ExtendedMessage> cancelled)
        {
            objectSystem.PostLater(delay_ms, dest, msg, cancelled);
        }

        public void RemoveOldObject(OldObjectID obj)
        {
            theLock.EnterWriteLock();
            try
            {
                if (objectDescriptions.ContainsKey(obj))
                {
                    objectDescriptions.Remove(obj);
                }
            }
            finally
            {
                theLock.ExitWriteLock();
            }
            objectSystem.RemoveObject(obj);
        }

        public IEnumerable<Tuple<OldObjectID, string>> ListOldObjects()
        {
            theLock.EnterReadLock();
            List<Tuple<OldObjectID, string>> theList = null;
            try
            {
                theList = objectDescriptions.Select(x => new Tuple<OldObjectID, String>(x.Key, x.Value)).ToList();
            }
            finally
            {
                theLock.ExitReadLock();
            }
            return theList.AsReadOnly();
        }

        public SchemeObject CurrentObject { get { return null; } }

        private class GlobalStateProxy : IGlobalState
        {
            private IGlobalState parent;
            private SchemeObject currentObject;

            public GlobalStateProxy(IGlobalState parent, SchemeObject currentObject)
            {
                this.parent = parent;
                this.currentObject = currentObject;
            }

            public DisposableID RegisterDisposable(IDisposable item, string desc)
            {
                return parent.RegisterDisposable(item, desc);
            }

            public IDisposable GetDisposableByID(DisposableID id)
            {
                return parent.GetDisposableByID(id);
            }

            public IEnumerable<Tuple<DisposableID, string>> ListDisposables()
            {
                return parent.ListDisposables();
            }

            public void DisposeByID(DisposableID id)
            {
                parent.DisposeByID(id);
            }

            public AsyncID RegisterAsync(IAsyncResult iar, CompletionProc completion, string desc)
            {
                return parent.RegisterAsync(iar, completion, desc);
            }

            public AsyncID RegisterAsync(IAsyncResult iar, AdvancedCompletionProc completion, string desc)
            {
                return parent.RegisterAsync(iar, completion, desc);
            }

            public WaitResult WaitOne(AsyncID asyncId)
            {
                return parent.WaitOne(asyncId);
            }

            public bool IsCompleted(AsyncID asyncId)
            {
                return parent.IsCompleted(asyncId);
            }

            public WaitResult WaitAny(AsyncID[] asyncIds)
            {
                return parent.WaitAny(asyncIds);
            }

            public IEnumerable<AsyncInfo> ListAsyncs()
            {
                return parent.ListAsyncs();
            }

            public OldObjectID AddOldObject(string desc, IMessageHandler<ExtendedMessage> obj)
            {
                return parent.AddOldObject(desc, obj);
            }

            public bool OldPostMessage(OldObjectID dest, ExtendedMessage msg)
            {
                return parent.OldPostMessage(dest, msg);
            }

            public void OldPostMessageLater(uint delay_ms, OldObjectID dest, ExtendedMessage msg, Action<ExtendedMessage> cancelled)
            {
                parent.OldPostMessageLater(delay_ms, dest, msg, cancelled);
            }

            public void RemoveOldObject(OldObjectID obj)
            {
                parent.RemoveOldObject(obj);
            }

            public IEnumerable<Tuple<OldObjectID, string>> ListOldObjects()
            {
                return parent.ListOldObjects();
            }

            public SchemeObject CurrentObject
            {
                get { return currentObject; }
            }

            public IGlobalState WithCurrentObject(SchemeObject obj)
            {
                return new GlobalStateProxy(parent, obj);
            }

            public IConsole Console { get { return parent.Console; } }

            public void Dispose()
            {
                // do nothing
            }

            public Scheduler Scheduler
            {
                get { return parent.Scheduler; }
            }

            public void RegisterSignal(SignalID sig, string text, bool isUserDefined)
            {
                parent.RegisterSignal(sig, text, isUserDefined);
            }

            public bool IsUserDefinedSignal(SignalID sig)
            {
                return parent.IsUserDefinedSignal(sig);
            }

            public void UnregisterSignal(SignalID sig)
            {
                parent.UnregisterSignal(sig);
            }

            public IEnumerable<Tuple<SignalID, string>> ListSignals()
            {
                return parent.ListSignals();
            }

            public void RegisterObject(ObjectID obj, string text)
            {
                parent.RegisterObject(obj, text);
            }

            public void UnregisterObject(ObjectID obj)
            {
                parent.UnregisterObject(obj);
            }

            public IEnumerable<Tuple<ObjectID, string>> ListObjects()
            {
                return parent.ListObjects();
            }
        }

        public IGlobalState WithCurrentObject(SchemeObject obj)
        {
            return new GlobalStateProxy(this, obj);
        }

        public IConsole Console { get { return console; } }

        public void Dispose()
        {
            objectSystem.Dispose();
            foreach (KeyValuePair<DisposableID, Tuple<IDisposable, string>> kvp in disposables)
            {
                kvp.Value.Item1.Dispose();
            }
            disposables.Clear();
        }

        #region IGlobalState Members


        public Scheduler Scheduler
        {
            get { return scheduler; }
        }

        public void RegisterSignal(SignalID sig, string text, bool isUserDefined)
        {
            theLock.EnterWriteLock();
            try
            {
                if (!signals.ContainsKey(sig))
                {
                    signals.Add(sig, text);
                    if (isUserDefined)
                    {
                        userDefinedSignals.Add(sig);
                    }
                }

            }
            finally
            {
                theLock.ExitWriteLock();
            }
        }

        public bool IsUserDefinedSignal(SignalID sig)
        {
            theLock.EnterReadLock();
            try
            {
                return (signals.ContainsKey(sig) && userDefinedSignals.Contains(sig));
            }
            finally
            {
                theLock.ExitReadLock();
            }
        }

        public void UnregisterSignal(SignalID sig)
        {
            theLock.EnterWriteLock();
            try
            {
                if (signals.ContainsKey(sig))
                {
                    signals.Remove(sig);
                }
            }
            finally
            {
                theLock.ExitWriteLock();
            }
        }

        public IEnumerable<Tuple<SignalID, string>> ListSignals()
        {
            List<Tuple<SignalID, string>> results = new List<Tuple<SignalID, string>>();
            theLock.EnterReadLock();
            try
            {
                results = signals.Select(x => new Tuple<SignalID, string>(x.Key, x.Value)).ToList();
            }
            finally
            {
                theLock.ExitReadLock();
            }
            return results;
        }

        public void RegisterObject(ObjectID obj, string text)
        {
            theLock.EnterWriteLock();
            try
            {
                if (!objects.ContainsKey(obj))
                {
                    objects.Add(obj, text);
                }
            }
            finally
            {
                theLock.ExitWriteLock();
            }
        }

        public void UnregisterObject(ObjectID obj)
        {
            theLock.EnterWriteLock();
            try
            {
                if (objects.ContainsKey(obj))
                {
                    objects.Remove(obj);
                }
            }
            finally
            {
                theLock.ExitWriteLock();
            }
        }

        public IEnumerable<Tuple<ObjectID, string>> ListObjects()
        {
            List<Tuple<ObjectID, string>> results = new List<Tuple<ObjectID, string>>();
            theLock.EnterReadLock();
            try
            {
                results = objects.Select(x => new Tuple<ObjectID, string>(x.Key, x.Value)).ToList();
            }
            finally
            {
                theLock.ExitReadLock();
            }
            return results;
        }

        #endregion
    }

    namespace Procedures
    {
        static partial class ProxyDiscovery
        {
            [SchemeFunction("list-disposables")]
            public static object ListDisposables(IGlobalState gs)
            {
                object r = SpecialValue.EMPTY_LIST;
                foreach (Tuple<DisposableID, string> t in gs.ListDisposables())
                {
                    SchemeHashMap s = new SchemeHashMap();
                    s[new Symbol("id")] = t.Item1;
                    s[new Symbol("desc")] = new SchemeString(t.Item2);
                    r = new ConsCell(s, r);
                }
                ConsCell.Reverse(ref r);
                return r;
            }

            [SchemeFunction("dispose!")]
            public static void DisposeByID(IGlobalState gs, DisposableID id)
            {
                gs.DisposeByID(id);
            }

            [SchemeFunction("list-asyncs")]
            public static object ListAsyncs(IGlobalState gs)
            {
                object r = SpecialValue.EMPTY_LIST;
                foreach (AsyncInfo t in gs.ListAsyncs())
                {
                    SchemeHashMap s = new SchemeHashMap();
                    s[new Symbol("id")] = t.id;
                    s[new Symbol("desc")] = new SchemeString(t.desc);
                    s[new Symbol("wait-in-progress")] = t.waitInProgress;
                    r = new ConsCell(s, r);
                }
                ConsCell.Reverse(ref r);
                return r;
            }

            [SchemeFunction("is-completed?")]
            public static bool IsCompleted(IGlobalState gs, AsyncID id)
            {
                return gs.IsCompleted(id);
            }

            [SchemeFunction("list-signals")]
            public static object ListSignals(IGlobalState gs)
            {
                object r = SpecialValue.EMPTY_LIST;
                foreach (Tuple<SignalID, string> t in gs.ListSignals())
                {
                    SchemeHashMap s = new SchemeHashMap();
                    s[new Symbol("id")] = t.Item1;
                    s[new Symbol("desc")] = new SchemeString(t.Item2);
                    r = new ConsCell(s, r);
                }
                ConsCell.Reverse(ref r);
                return r;
            }

#if false
            // deprecated
            [SchemeFunction("wait-one")]
            public static object WaitOne(IGlobalState gs, AsyncID id)
            {
                WaitResult w = gs.WaitOne(id);

                object result = EncodeWaitResult(w);

                return result;
            }
#endif

            [SchemeFunction("disposable-id")]
            public static DisposableID MakeDisposableID(ushort id)
            {
                return (DisposableID)id;
            }

            [SchemeFunction("async-id")]
            public static AsyncID MakeAsyncID(ushort id)
            {
                return (AsyncID)id;
            }

            [SchemeFunction("disposable-id?")]
            public static bool IsDisposableID(object obj)
            {
                return obj is DisposableID;
            }

            [SchemeFunction("async-id?")]
            public static bool IsAsyncID(object obj)
            {
                return obj is AsyncID;
            }

            [SchemeFunction("signal-id")]
            public static SignalID MakeSignalID(ulong id)
            {
                return (SignalID)id;
            }

            [SchemeFunction("signal-id?")]
            public static bool IsSignalID(object obj)
            {
                return obj is SignalID;
            }

            [SchemeFunction("make-signal")]
            public static SignalID MakeSignal(IGlobalState gs)
            {
                SignalID sid = gs.Scheduler.GetNewSignalID();
                gs.RegisterSignal(sid, "User-Defined Event", true);
                return sid;
            }

            [SchemeFunction("post-signal")]
            public static void SendSignal(IGlobalState gs, SignalID dest, object obj)
            {
                if (!(gs.IsUserDefinedSignal(dest))) throw new SchemeRuntimeException("Can't post to a signal that isn't user-defined");
                gs.Scheduler.PostSignal(dest, obj, false);
                gs.UnregisterSignal(dest);
            }

            [SchemeFunction("throw-signal")]
            public static void ThrowSignal(IGlobalState gs, SignalID dest, object obj)
            {
                if (!(gs.IsUserDefinedSignal(dest))) throw new SchemeRuntimeException("Can't throw an exception to a signal that isn't user defined");
                gs.Scheduler.PostSignal(dest, obj, true);
                gs.UnregisterSignal(dest);
            }

            [SchemeFunction("make-test")]
            public static DisposableID MakeTest(IGlobalState gs)
            {
                MyTest t = new MyTest();
                DisposableID d = gs.RegisterDisposable(t, "test");
                return d;
            }

            public static object EncodeWaitResult(WaitResult w)
            {
                SchemeHashMap result = new SchemeHashMap();
                result[new Symbol("id")] = w.id;
                result[new Symbol("result")] = w.result;
                result[new Symbol("exception?")] = w.isException;

                return result;
            }

            public static object EncodeSignalResult(SignalID sid, object result, bool isException)
            {
                SchemeHashMap result1 = new SchemeHashMap();
                result1[new Symbol("id")] = sid;
                result1[new Symbol("result")] = result;
                result1[new Symbol("exception?")] = isException;

                return result1;
            }
        }

        [SchemeSingleton("wait-any-old")]
        public class WaitAnyOld : IProcedure
        {
            public WaitAnyOld() { }

            public int Arity { get { return 1; } }

            public bool More { get { return true; } }

            public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
            {
                List<AsyncID> l = new List<AsyncID>();
                FList<object> f = argList;
                while (f != null)
                {
                    if (!(f.Head is AsyncID)) throw new SchemeRuntimeException("Async ID expected");
                    l.Add((AsyncID)(f.Head));
                    f = f.Tail;
                }
                
                WaitResult w = gs.WaitAny(l.ToArray());

                object result = ProxyDiscovery.EncodeWaitResult(w);
                
                return new RunnableReturn(k, result);
            }
        }

        [SchemeSingleton("wait-any")]
        public class WaitAny : IProcedure
        {
            public WaitAny() { }

            public int Arity { get { return 1; } }

            public bool More { get { return true; } }

            public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
            {
                HashSet<SignalID> sids = new HashSet<SignalID>();
                FList<object> f = argList;
                while (f != null)
                {
                    if (!(f.Head is SignalID)) throw new SchemeRuntimeException("Signal ID expected");
                    sids.Add((SignalID)(f.Head));
                    f = f.Tail;
                }

                gs.Scheduler.PostWait
                (
                    sids,
                    delegate(SignalID sid, object result, bool isException)
                    {
                        gs.UnregisterSignal(sid);
                        Doer.PostReturn(gs, k, ProxyDiscovery.EncodeSignalResult(sid, result, isException));
                    }
                );

                return null;
            }
        }
    }

    public static partial class Utils
    {
        public static long GetId(this ObjectIDGenerator idgen, object obj)
        {
            bool firstTime;
            long id = idgen.GetId(obj, out firstTime);
            return id;
        }
    }

    [SchemeIsAFunction("is-test?")]
    public class MyTest : IDisposable
    {
        private string message;

        public MyTest() { }

        public string Message
        {
            [SchemeFunction("test-get-message")]
            get
            {
                return message;
            }
            [SchemeFunction("test-set-message!")]
            set
            {
                message = value;
            }
        }

        public void Dispose()
        {
        }
    }
}
