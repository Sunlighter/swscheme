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
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using BigMath;
using System.Net.Sockets;
using System.Net;
using ControlledWindowLib.Scheduling;

namespace ExprObjModel.Procedures
{
    public static partial class ProxyDiscovery
    {
        [SchemeFunction("ipaddr")]
        public static object IPAddr(object obj)
        {
            if (obj is Symbol)
            {
                Symbol s = (Symbol)obj;
                if (s.IsSymbol("any"))
                {
                    return IPAddress.Any;
                }
                else if (s.IsSymbol("broadcast"))
                {
                    return IPAddress.Broadcast;
                }
                else if (s.IsSymbol("none"))
                {
                    return IPAddress.None;
                }
                else if (s.IsSymbol("loopback"))
                {
                    return IPAddress.Loopback;
                }
                else if (s.IsSymbol("v6any"))
                {
                    return IPAddress.IPv6Any;
                }
                else if (s.IsSymbol("v6none"))
                {
                    return IPAddress.IPv6None;
                }
                else if (s.IsSymbol("v6loopback"))
                {
                    return IPAddress.IPv6Loopback;
                }
                else throw new SchemeRuntimeException("Unknown symbolic IP address");
            }
            else if (obj is SchemeString)
            {
                SchemeString s = (SchemeString)obj;
                return System.Net.IPAddress.Parse(s.TheString);
            }
            else if (obj is IPEndPoint)
            {
                return ((IPEndPoint)obj).Address;
            }
            else if (obj is IPAddress)
            {
                return obj;
            }
            else throw new SchemeRuntimeException("Cannot convert an object of type " + obj.GetType().FullName + " to an IP Address");
        }

#if false
        [Obsolete]
        [SchemeFunction("begin-dns-lookup-old")]
        public static AsyncID BeginDnsLookupOld(IGlobalState gs, string name)
        {
            IAsyncResult iar = Dns.BeginGetHostEntry(name, null, null);
            CompletionProc completion = delegate(IGlobalState gs2, IAsyncResult iar2)
            {
                IPHostEntry he = Dns.EndGetHostEntry(iar2);
                object results = SpecialValue.EMPTY_LIST;
                int iEnd = he.AddressList.Length;
                for (int i = 0; i < iEnd; ++i)
                {
                    results = new ConsCell(he.AddressList[iEnd - i - 1], results);
                }
                return results;
            };

            return gs.RegisterAsync(iar, completion, "DNS Lookup");
        }
#endif

        [SchemeFunction("begin-dns-lookup")]
        public static SignalID BeginDnsLookup(IGlobalState gs, string name)
        {
            IAsyncResult iar = Dns.BeginGetHostEntry(name, null, null);
            SignalID sid = gs.Scheduler.GetNewSignalID();
            Action onCompletion = delegate()
            {
                try
                {
                    IPHostEntry he = Dns.EndGetHostEntry(iar);
                    object results = SpecialValue.EMPTY_LIST;
                    int iEnd = he.AddressList.Length;
                    for (int i = 0; i < iEnd; ++i)
                    {
                        results = new ConsCell(he.AddressList[iEnd - i - 1], results);
                    }
                    gs.Scheduler.PostSignal(sid, results, false);
                }
                catch(Exception exc)
                {
                    gs.Scheduler.PostSignal(sid, exc, true);
                }
            };
            gs.Scheduler.PostActionOnCompletion(iar.AsyncWaitHandle, onCompletion);
            gs.RegisterSignal(sid, "DNS Lookup", false);
            return sid;
        }

        [SchemeFunction("ipaddr->string")]
        public static object IPAddrToString(IPAddress addr)
        {
            return new SchemeString(addr.ToString());
        }

        [SchemeFunction("ipaddr?")]
        public static bool IsIPAddr(object obj)
        {
            return obj is IPAddress;
        }

        [SchemeFunction("endpoint->string")]
        public static object IPEndpointToString(IPEndPoint endPoint)
        {
            return new SchemeString(endPoint.ToString());
        }

        [SchemeFunction("ipendpoint?")]
        public static bool IsIPEndPoint(object obj)
        {
            return obj is IPEndPoint;
        }

        [SchemeFunction("ipendpoint->ipaddr")]
        public static IPAddress IPEndPointToIPAddr(IPEndPoint endPoint)
        {
            return endPoint.Address;
        }

        [SchemeFunction("ipendpoint->port")]
        public static int IPEndPointToPort(IPEndPoint endPoint)
        {
            return endPoint.Port;
        }

        [SchemeFunction("make-ipendpoint")]
        public static IPEndPoint MakeEndPoint(IPAddress addr, int port)
        {
            return new IPEndPoint(addr, port);
        }
   
        [SchemeFunction("open-udp")]
        public static object OpenUDP(IGlobalState gs, System.Net.IPAddress addr, int port)
        {
            System.Net.Sockets.Socket s = new System.Net.Sockets.Socket
            (
                addr.AddressFamily,
                System.Net.Sockets.SocketType.Dgram,
                System.Net.Sockets.ProtocolType.Udp
            );
            System.Net.IPEndPoint localEp = new System.Net.IPEndPoint(addr, port);
            s.Bind(localEp);
            return gs.RegisterDisposable(s, "UDP socket, port " + port);
        }

#if false
        [Obsolete]
        [SchemeFunction("begin-udp-receive-old!")]
        public static AsyncID AsyncUdpReceiveOld(IGlobalState gs, Socket s)
        {
            if (!(s.ProtocolType == ProtocolType.Udp)) throw new SchemeRuntimeException("Socket is not UDP");
            byte[] buffer = new byte[10240];
            EndPoint ep = null;
            IAsyncResult iar = s.BeginReceiveFrom(buffer, 0, 10240, SocketFlags.None, ref ep, null, null);
            CompletionProc complete = delegate(IGlobalState gs2, IAsyncResult iar2)
            {
                int i = s.EndReceiveFrom(iar, ref ep);
                byte[] buf2 = new byte[i];
                Buffer.BlockCopy(buffer, 0, buf2, 0, i);
                SchemeHashMap m = new SchemeHashMap();
                if (ep is IPEndPoint)
                {
                    IPEndPoint ep1 = (IPEndPoint)ep;
                    m[new Symbol("address")] = ep1.Address;
                    m[new Symbol("port")] = BigInteger.FromInt32(ep1.Port);
                }
                else
                {
                    m[new Symbol("endpoint")] = ep;
                }
                m[new Symbol("data")] = new SchemeByteArray(buf2, DigitOrder.LBLA);
                return m;
            };

            return gs.RegisterAsync(iar, complete, "udp-receive");
        }
#endif

        [SchemeFunction("begin-udp-receive!")]
        public static SignalID AsyncUdpReceive(IGlobalState gs, Socket s)
        {
            if (!(s.ProtocolType == ProtocolType.Udp)) throw new SchemeRuntimeException("Socket is not UDP");
            byte[] buffer = new byte[10240];
            EndPoint ep = null;
            SignalID sid = gs.Scheduler.GetNewSignalID();
            AsyncCallback a = delegate(IAsyncResult iar)
            {
                try
                {
                    int i = s.EndReceiveFrom(iar, ref ep);
                    byte[] buf2 = new byte[i];
                    Buffer.BlockCopy(buffer, 0, buf2, 0, i);
                    SchemeHashMap m = new SchemeHashMap();
                    if (ep is IPEndPoint)
                    {
                        IPEndPoint ep1 = (IPEndPoint)ep;
                        m[new Symbol("address")] = ep1.Address;
                        m[new Symbol("port")] = BigInteger.FromInt32(ep1.Port);
                    }
                    else
                    {
                        m[new Symbol("endpoint")] = ep;
                    }
                    m[new Symbol("data")] = new SchemeByteArray(buf2, DigitOrder.LBLA);
                    gs.Scheduler.PostSignal(sid, m, false);
                }
                catch (Exception exc)
                {
                    gs.Scheduler.PostSignal(sid, exc, true);
                }
            };

            s.BeginReceiveFrom(buffer, 0, 10240, SocketFlags.None, ref ep, a, null);
            gs.RegisterSignal(sid, "udp-receive", false);
            return sid;
        }

        [SchemeFunction("begin-udp-send!")]
        public static SignalID AsyncUdpSend(IGlobalState gs, Socket s, IPAddress addr, int port, ByteRange message)
        {
            if (message.IsValid)
            {
                byte[] b2 = ProxyDiscovery.ByteRangeToBytes1(message);
                IPEndPoint ep = new IPEndPoint(addr, port);
                SignalID sid = gs.Scheduler.GetNewSignalID();
                AsyncCallback a = delegate(IAsyncResult iar2)
                {
                    try
                    {
                        int bytesSent = s.EndSendTo(iar2);
                        gs.Scheduler.PostSignal(sid, bytesSent, false);
                    }
                    catch(Exception exc)
                    {
                        gs.Scheduler.PostSignal(sid, exc, true);
                    }
                };

                IAsyncResult iar = s.BeginSendTo(b2, 0, b2.Length, SocketFlags.None, ep, a, null);

                gs.RegisterSignal(sid, "udp-send", false);
                return sid;
            }
            else throw new SchemeRuntimeException("Invalid range");
        }

#if false
        [SchemeFunction("udp-send!")]
        public static void UdpSend(Socket s, IPAddress addr, int port, ByteRange message)
        {
            if (message.IsValid)
            {
                byte[] b2 = ProxyDiscovery.ByteRangeToBytes1(message);
                IPEndPoint ep = new IPEndPoint(addr, port);
                s.SendTo(b2, ep);
            }
            else throw new SchemeRuntimeException("Invalid range");
        }

        [SchemeFunction("make-timer-queue")]
        public static DisposableID MakeTimerQueue(IGlobalState gs)
        {
            TimerQueue<object> t = new TimerQueue<object>();
            DisposableID d = gs.RegisterDisposable(t, "Timer Queue");
            return d;
        }

        [SchemeFunction("timer-queue-put!")]
        public static void TimerQueueEnqueue(TimerQueue<object> t, uint delay, object obj)
        {
            t.Put(delay, obj);
        }

        [SchemeFunction("begin-timer-queue-get!")]
        public static AsyncID BeginTimerQueueGet(IGlobalState gs, TimerQueue<object> t)
        {
            IAsyncResult iar = t.BeginGet(null, null);
            CompletionProc completion = delegate(IGlobalState gs2, IAsyncResult iar2)
            {
                Option<object> op = t.EndGet(iar2);
                if (op is Some<object>)
                {
                    return new ConsCell(true, ((Some<object>)op).value);
                }
                else
                {
                    return new ConsCell(false, false);
                }
            };

            AsyncID a = gs.RegisterAsync(iar, completion, "Timer Queue Get");
            return a;
        }

        [SchemeFunction("make-async-queue-old")]
        public static DisposableID MakeAsyncQueueOld(IGlobalState gs)
        {
            ControlledWindowLib.AsyncQueue<object> t = new ControlledWindowLib.AsyncQueue<object>();
            DisposableID d = gs.RegisterDisposable(t, "Async Queue");
            return d;
        }

        [SchemeFunction("async-queue-put-old!")]
        public static void AsyncQueuePutOld(ControlledWindowLib.AsyncQueue<object> t, object obj)
        {
            t.Put(obj);
        }

        [SchemeFunction("async-queue-close-old!")]
        public static void AsyncQueueCloseOld(ControlledWindowLib.AsyncQueue<object> t)
        {
            t.Close();
        }

        [SchemeFunction("async-queue-closed-old?")]
        public static bool AsyncQueueIsClosedOld(ControlledWindowLib.AsyncQueue<object> t)
        {
            return t.IsClosed;
        }

        [SchemeFunction("async-queue-empty-old?")]
        public static bool AsyncQueueIsEmptyOld(ControlledWindowLib.AsyncQueue<object> t)
        {
            return t.IsEmpty;
        }

        [SchemeFunction("begin-async-queue-get-old!")]
        public static AsyncID BeginAsyncQueueGetOld(IGlobalState gs, ControlledWindowLib.AsyncQueue<object> t)
        {
            IAsyncResult iar = t.BeginGet(null, null);
            CompletionProc completion = delegate(IGlobalState gs2, IAsyncResult iar2)
            {
                object obj= t.EndGet(iar2);
                return obj;
            };

            AsyncID a = gs.RegisterAsync(iar, completion, "Async Queue Get");
            return a;
        }

        [SchemeFunction("async-queue-closed-exception?")]
        public static bool IsAsyncQueueClosedException(object obj)
        {
            return obj is ControlledWindowLib.AsyncQueueClosedException;
        }
#endif

        [SchemeFunction("object-disposed-exception?")]
        public static bool IsObjectDisposed(object obj)
        {
            return obj is ObjectDisposedException;
        }

        [SchemeFunction("make-tcp-server")]
        public static DisposableID MakeTcpServer(IGlobalState gs, IPAddress localAddr, [OverflowMode(OverflowBehavior.ThrowException)] ushort port, int backlog)
        {
            Socket s = new Socket(localAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                s.Bind(new IPEndPoint(localAddr, (int)port));
                s.Listen(backlog);
                return gs.RegisterDisposable(s, "TCP Server Socket, port " + port);
            }
            catch (Exception)
            {
                s.Dispose();
                throw;
            }
        }

#if false
        [SchemeFunction("begin-tcp-accept-old!")]
        public static AsyncID BeginTcpAcceptOld(IGlobalState gs, Socket s)
        {
            IAsyncResult iar = s.BeginAccept(null, null);
            CompletionProc completion = delegate(IGlobalState gs2, IAsyncResult iar2)
            {
                Socket s2 = s.EndAccept(iar2);
                DisposableID d = gs2.RegisterDisposable(s2, "TCP Client Socket");
                return d;
            };
            AsyncID a = gs.RegisterAsync(iar, completion, "tcp-accept");
            return a;
        }
#endif

        [SchemeFunction("begin-tcp-accept")]
        public static SignalID BeginTcpAccept(IGlobalState gs, Socket s)
        {
            SignalID sid = gs.Scheduler.GetNewSignalID();
            AsyncCallback a = delegate(IAsyncResult iar2)
            {
                try
                {
                    Socket s2 = s.EndAccept(iar2);
                    try
                    {
                        DisposableID d = gs.RegisterDisposable(s2, "TCP Client Socket");
                        gs.Scheduler.PostSignal(sid, d, false);
                    }
                    catch
                    {
                        s2.Dispose();
                        throw;
                    }
                }
                catch (Exception exc)
                {
                    gs.Scheduler.PostSignal(sid, exc, true);
                }
            };
            IAsyncResult iar = s.BeginAccept(a, null);
            gs.RegisterSignal(sid, "tcp-accept", false);
            return sid;
        }

#if false
        [SchemeFunction("begin-tcp-connect-old!")]
        public static AsyncID MakeTcpClientOld(IGlobalState gs, IPAddress remoteAddr, [OverflowMode(OverflowBehavior.ThrowException)] ushort port)
        {
            Socket s = new Socket(remoteAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                IAsyncResult iar = s.BeginConnect(new IPEndPoint(remoteAddr, (int)port), null, null);
                CompletionProc completion = delegate(IGlobalState gs2, IAsyncResult iar2)
                {
                    s.EndConnect(iar2);
                    DisposableID d = gs2.RegisterDisposable(s, "TCP Client Socket");
                    return d;
                };
                AsyncID a = gs.RegisterAsync(iar, completion, "tcp-connect");
                return a;
            }
            catch
            {
                s.Dispose();
                throw;
            }
        }
#endif

        [SchemeFunction("begin-tcp-connect")]
        public static SignalID MakeTcpClient(IGlobalState gs, IPAddress remoteAddr, [OverflowMode(OverflowBehavior.ThrowException)] ushort port)
        {
            Socket s = new Socket(remoteAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            SignalID sid = gs.Scheduler.GetNewSignalID();
            try
            {
                AsyncCallback a = delegate(IAsyncResult iar2)
                {
                    try
                    {
                        s.EndConnect(iar2);
                        DisposableID d = gs.RegisterDisposable(s, "TCP Client Socket");
                        gs.Scheduler.PostSignal(sid, d, false);
                    }
                    catch (Exception exc)
                    {
                        s.Dispose();
                        gs.Scheduler.PostSignal(sid, exc, true);
                    }
                };

                IAsyncResult iar = s.BeginConnect(new IPEndPoint(remoteAddr, (int)port), a, null);

                gs.RegisterSignal(sid, "tcp-connect", false);
                return sid;
            }
            catch
            {
                s.Dispose();
                throw;
            }
        }

#if false
        [Obsolete]
        [SchemeFunction("begin-tcp-send-old!")]
        public static AsyncID BeginTcpSendOld(IGlobalState gs, Socket s, ByteRange buf)
        {
            if (!(buf.IsValid)) throw new SchemeRuntimeException("begin-tcp-send!: Invalid range");
            IAsyncResult iar = s.BeginSend(buf.Array.Bytes, buf.Offset, buf.LengthInt32, SocketFlags.None, null, null);
            CompletionProc completion = delegate(IGlobalState gs2, IAsyncResult iar2)
            {
                int bytesSent = s.EndSend(iar2);
                return BigInteger.FromInt32(bytesSent);
            };
            AsyncID a = gs.RegisterAsync(iar, completion, "tcp-send");
            return a;
        }
#endif

        [SchemeFunction("begin-tcp-send!")]
        public static SignalID BeginTcpSend(IGlobalState gs, Socket s, ByteRange buf)
        {
            if (!(buf.IsValid)) throw new SchemeRuntimeException("begin-tcp-send!: Invalid range");
            SignalID sid = gs.Scheduler.GetNewSignalID();
            AsyncCallback a = delegate(IAsyncResult iar)
            {
                try
                {
                    int bytesSent = s.EndSend(iar);
                    gs.Scheduler.PostSignal(sid, BigInteger.FromInt32(bytesSent), false);
                }
                catch (Exception exc)
                {
                    gs.Scheduler.PostSignal(sid, exc, true);
                }
            };
            s.BeginSend(buf.Array.Bytes, buf.Offset, buf.LengthInt32, SocketFlags.None, a, null);
            gs.RegisterSignal(sid, "tcp-send", false);

            return sid;
        }

#if false
        [Obsolete]
        [SchemeFunction("begin-tcp-receive-old!")]
        public static AsyncID BeginTcpReceiveOld(IGlobalState gs, Socket s, ByteRange buf)
        {
            if (!(buf.IsValid)) throw new SchemeRuntimeException("begin-tcp-receive!: Invalid range");
            IAsyncResult iar = s.BeginReceive(buf.Array.Bytes, buf.Offset, buf.LengthInt32, SocketFlags.None, null, null);
            CompletionProc completion = delegate(IGlobalState gs2, IAsyncResult iar2)
            {
                int bytesReceived = s.EndReceive(iar2);
                return BigInteger.FromInt32(bytesReceived);
            };
            AsyncID a = gs.RegisterAsync(iar, completion, "tcp-receive");
            return a;
        }
#endif

        [SchemeFunction("begin-tcp-receive")]
        public static SignalID BeginTcpReceive(IGlobalState gs, Socket s, ByteRange buf)
        {
            if (!(buf.IsValid)) throw new SchemeRuntimeException("begin-tcp-receive!: Invalid range");
            SignalID sid = gs.Scheduler.GetNewSignalID();
            AsyncCallback a = delegate(IAsyncResult iar)
            {
                try
                {
                    int bytesReceived = s.EndReceive(iar);
                    gs.Scheduler.PostSignal(sid, BigInteger.FromInt32(bytesReceived), false);
                }
                catch (Exception exc)
                {
                    gs.Scheduler.PostSignal(sid, exc, true);
                }
            };
            s.BeginReceive(buf.Array.Bytes, buf.Offset, buf.LengthInt32, SocketFlags.None, a, null);
            gs.RegisterSignal(sid, "tcp-receive", false);
            return sid;
        }
    }

    [SchemeSingleton("make-async-queue-pair")]
    public class MakeAsyncQueuePairProc : IProcedure
    {
        public MakeAsyncQueuePairProc()
        {
        }

        public int Arity { get { return 1; } }
        public bool More { get { return false; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            if (argList == null)
            {
                return new RunnableThrow(k, new SchemeRuntimeException("make-async-queue-pair: Insufficient arguments"));
            }

            object arg1 = argList.Head;
            argList = argList.Tail;

            if (argList != null)
            {
                return new RunnableThrow(k, new SchemeRuntimeException("make-async-queue-pair: Too many arguments"));
            }

            if (!(arg1 is IProcedure))
            {
                return new RunnableThrow(k, new SchemeRuntimeException("make-async-queue-pair: Argument must be a procedure"));
            }

            IProcedure pArg1 = (IProcedure)arg1;

            if (!pArg1.AcceptsParameterCount(2))
            {
                return new RunnableThrow(k, new SchemeRuntimeException("make-async-queue-pair: Procedure argument must accept two parameters"));
            }

            Tuple<ControlledWindowLib.IAsyncSender<object>, ControlledWindowLib.IAsyncReceiver<object>> t = ControlledWindowLib.AsyncPipeFactory<object>.MakeAsyncPipe(gs.Scheduler);

            DisposableID sender = gs.RegisterDisposable(t.Item1, "AsyncSender");
            DisposableID receiver = gs.RegisterDisposable(t.Item2, "AsyncReceiver");

            return new RunnableCall(pArg1, new FList<object>(sender, new FList<object>(receiver)), k);
        }
    }

    [SchemeSingleton("async-queue-send!")]
    public class AsyncQueueSendProc : IProcedure
    {
        public AsyncQueueSendProc()
        {
        }

        public int Arity { get { return 2; } }
        public bool More { get { return false; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            if (argList == null)
            {
                return new RunnableThrow(k, new SchemeRuntimeException("async-queue-send!: Insufficient arguments"));
            }

            object arg1 = argList.Head;
            argList = argList.Tail;

            if (argList == null)
            {
                return new RunnableThrow(k, new SchemeRuntimeException("async-queue-send!: Insufficient arguments"));
            }

            object arg2 = argList.Head;
            argList = argList.Tail;

            if (argList != null)
            {
                return new RunnableThrow(k, new SchemeRuntimeException("async-queue-send!: Too many arguments"));
            }

            ControlledWindowLib.IAsyncSender<object> s = null;

            if (arg1 is DisposableID)
            {
                DisposableID ds = (DisposableID)arg1;
                IDisposable a2 = gs.GetDisposableByID(ds);
                if (a2 is ControlledWindowLib.IAsyncSender<object>)
                {
                    s = (ControlledWindowLib.IAsyncSender<object>)a2;
                }
            }

            if (s == null)
            {
                return new RunnableThrow(k, new SchemeRuntimeException("async-queue-send!: First argument must be an AsyncSender"));
            }

            if (arg2 is SpecialValue && ((SpecialValue)arg2) == SpecialValue.EOF)
            {
                return new RunnableThrow(k, new SchemeRuntimeException("async-queue-send!: Cannot send the EOF object (try disposing the AsyncSender instead)"));
            }

            s.Send
            (
                arg2,
                delegate(ControlledWindowLib.SendResult sr)
                {
                    if (sr == ControlledWindowLib.SendResult.Succeeded || sr == ControlledWindowLib.SendResult.Closed)
                    {
                        Doer.PostReturn(gs, k, SpecialValue.UNSPECIFIED);
                    }
                    else
                    {
                        Doer.PostThrow(gs, k, new SchemeRuntimeException("AsyncSender: " + sr));
                    }
                }
            );

            return null;
        }

    }
}
