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
using ControlledWindowLib;
using System.Drawing;

namespace ExprObjModel.ObjectSystem
{
    public class ControlledWindowObject : IMessageHandler<ExtendedMessage>
    {
        private ObjectSystem<ExtendedMessage> objectSystem;
        private OldObjectID self;
        private ControlledWindow2 window;
        private IGlobalState gs;
        private object syncRoot;
        private OldObjectID? dest;
        private object destData;
        private InitData initData;

        public ControlledWindowObject(IGlobalState gs, int xsize, int ysize)
        {
            this.gs = gs;
            this.syncRoot = new object();
            this.dest = null;
            this.destData = null;

            InitActionTable();

            this.initData = new ID_Size(xsize, ysize);
        }

        public ControlledWindowObject(IGlobalState gs, Bitmap b)
        {
            this.gs = gs;
            this.syncRoot = new object();
            this.dest = null;
            this.destData = null;

            InitActionTable();

            this.initData = new ID_Bitmap((Bitmap)(b.Clone()));
        }

        public void Welcome(ObjectSystem<ExtendedMessage> objectSystem, OldObjectID self)
        {
            this.objectSystem = objectSystem;
            this.self = self;

            window = new ControlledWindow2(new Action<CW_Event>(Post));

            if (initData is ID_Size)
            {
                ID_Size ids = (ID_Size)initData;
                using (Bitmap b = new Bitmap(ids.Width, ids.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb))
                {
                    using (Graphics g = Graphics.FromImage(b))
                    {
                        g.Clear(SystemColors.Window);
                    }
                    window.SetImage(b);
                }
            }
            else if (initData is ID_Bitmap)
            {
                ID_Bitmap idb = (ID_Bitmap)initData;
                window.SetImage(idb.Bitmap);
                idb.Bitmap.Dispose();
            }

            window.Sync(EventTypes.None, EventTypes.GotLostFocus | EventTypes.KeyDownUp | EventTypes.KeyPress);
        }

        private void Post(CW_Event e)
        {
            if (dest.HasValue)
            {
                objectSystem.Post(dest.Value, new EM_SchemeMessage(Convert(e, destData)));
            }
            if (e is CW_Closed)
            {
                objectSystem.Post(self, new EM_Close());
            }
        }

        public void Handle(ExtendedMessage message)
        {
            if (message is EM_SchemeMessage)
            {
                EM_SchemeMessage s = (EM_SchemeMessage)message;
                if (actionTable.ContainsKey(s.Message.Signature))
                {
                    ActionProc a = actionTable[s.Message.Signature];
                    a(this, s.Message);
                }
                else
                {
                    Console.WriteLine("Controlled Window: Unknown Signature " + SchemeDataWriter.ItemToString(s.Message.Signature));
                }
            }
            else if (message is EM_GetHandlerList)
            {
                EM_GetHandlerList ghl = (EM_GetHandlerList)message;
                objectSystem.Post(ghl.K, new EM_HandlerListResponse(actionTable.Keys.ToHashSet(), ghl.KData));
            }
            else if (message is EM_GetFieldList)
            {
                EM_GetFieldList gfl = (EM_GetFieldList)message;
                objectSystem.Post(gfl.K, new EM_FieldListResponse(new HashSet<Symbol>(), gfl.KData));
            }
            else if (message is EM_Close)
            {
                gs.RemoveOldObject(self);
                this.Dispose();
            }
            else
            {
                Console.WriteLine("Controlled Window: Unknown message type: " + message.GetType().FullName);
            }
        }

        public void Dispose()
        {
            window.Dispose();
        }

        private delegate void ActionProc(ControlledWindowObject win, Message<object> msg);

        private static Dictionary<Signature, ActionProc> actionTable;

        private static Signature Sig(string str)
        {
            return (Signature)(SchemeDataReader.ReadItem(str));
        }

        private static void InitActionTable()
        {
            if (actionTable == null)
            {
                lock (typeof(ControlledWindowObject))
                {
                    if (actionTable == null)
                    {
                        InitActionTable2();
                    }
                }
            }
        }

        private static void InitActionTable2()
        {
            Dictionary<Signature, ActionProc> dict = new Dictionary<Signature, ActionProc>();

            dict.Add(Sig("#sig(set-title . title)"), new ActionProc(H_SetTitle));
            dict.Add(Sig("#sig(set-image . image)"), new ActionProc(H_SetImage));
            dict.Add(Sig("#sig(draw . drawing)"), new ActionProc(H_Draw));
            dict.Add(Sig("#sig(sync . andmask xormask)"), new ActionProc(H_Sync));
            dict.Add(Sig("#sig(set-dest . k kdata)"), new ActionProc(H_SetDest));

            actionTable = dict;
        }

        private static void H_SetTitle(ControlledWindowObject win, Message<object> msg)
        {
            try
            {
                string title = ((SchemeString)(msg[new Symbol("title")])).TheString;
                win.window.SetTitle(title);
            }
            catch (Exception exc)
            {
                Console.WriteLine("Error Setting Title: " + exc.Message);
            }
        }

        private static void H_SetImage(ControlledWindowObject win, Message<object> msg)
        {
            try
            {
                DisposableID id = ((DisposableID)(msg[new Symbol("image")]));
                Image i = (Image)(win.gs.GetDisposableByID(id));
                win.window.SetImage(i);
            }
            catch (Exception exc)
            {
                Console.WriteLine("Error Setting Image: " + exc.Message);
            }
        }

        private static void H_Draw(ControlledWindowObject win, Message<object> msg)
        {
            try
            {
                object drawing = msg[new Symbol("drawing")];
                
                ExprObjModel.Drawing.DrawForm d = ExprObjModel.Drawing.Parser.Parse(drawing);

                if (d == null) throw new SchemeRuntimeException("Unable to parse drawing");

                win.window.Draw
                (
                    delegate(Graphics g)
                    {
                        using (ExprObjModel.Drawing.Context c = ExprObjModel.Drawing.Parser.NewInitialContext())
                        {
                            d.Draw(g, c);
                        }
                    }
                );
            }
            catch (Exception exc)
            {
                Console.WriteLine("Error Drawing: " + exc.Message);
            }
        }

        private static void H_Sync(ControlledWindowObject win, Message<object> msg)
        {
            try
            {
                OldObjectID? k = null;
                object kdata = null;
                ObjectSystem<ExtendedMessage> objectSystem = null;
                lock(win.syncRoot)
                {
                    k = win.dest;
                    kdata = win.destData;
                    objectSystem = win.objectSystem;
                }
                if (k.HasValue)
                {
                    try
                    {
                        EventTypes andMask = ((EventTypes)(msg[new Symbol("andmask")]));
                        EventTypes xorMask = ((EventTypes)(msg[new Symbol("xormask")]));

                        Guid g = win.window.Sync(andMask, xorMask);

                        objectSystem.Post
                        (
                            k.Value,
                            new EM_SchemeMessage
                            (
                                new Message<object>
                                (
                                    new Symbol("sync-posted"),
                                    new Tuple<Symbol, object>[]
                                    {
                                        new Tuple<Symbol, object>(new Symbol("id"), g),
                                        new Tuple<Symbol, object>(new Symbol("kdata"), kdata)
                                    }
                                )
                            )
                        );
                    }
                    catch (Exception exc)
                    {
                        objectSystem.Post
                        (
                            k.Value,
                            new EM_SchemeMessage
                            (
                                new Message<object>
                                (
                                    new Symbol("exception"),
                                    new Tuple<Symbol, object>[]
                                    {
                                        new Tuple<Symbol, object>(new Symbol("exception"), exc),
                                        new Tuple<Symbol, object>(new Symbol("kdata"), kdata)
                                    }
                                )
                            )
                        );
                    }
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("Error In Sync: " + exc.Message);
            }
        }

        private static void H_SetDest(ControlledWindowObject win, Message<object> msg)
        {
            try
            {
                lock (win.syncRoot)
                {
                    if (win.dest.HasValue)
                    {
                        win.objectSystem.Post
                        (
                            win.dest.Value,
                            new EM_SchemeMessage
                            (
                                new Message<object>
                                (
                                    new Symbol("goodbye"),
                                    new Tuple<Symbol, object>[]
                                    {
                                        new Tuple<Symbol, object>(new Symbol("kdata"), win.destData)
                                    }
                                )
                            )
                        );
                    }
                }

                OldObjectID k = ((OldObjectID)(msg[new Symbol("k")]));
                object kdata = msg[new Symbol("kdata")];
                
                lock(win.syncRoot)
                {
                    win.dest = k;
                    win.destData = kdata;

                    win.objectSystem.Post
                    (
                        win.dest.Value,
                        new EM_SchemeMessage
                        (
                            new Message<object>
                            (
                                new Symbol("hello"),
                                new Tuple<Symbol, object>[]
                                {
                                    new Tuple<Symbol, object>(new Symbol("kdata"), win.destData)
                                }
                            )
                        )
                    );
                }
            }
            catch(Exception exc)
            {
                Console.WriteLine("Error in SetDest: " + exc.Message);
            }
        }

        public static Message<object> Convert(CW_Event e, object kdata)
        {
            if (e is CW_Closed)
            {
                return new Message<object>
                (
                    new Symbol("closed"),
                    new Tuple<Symbol, object>[]
                    {
                        new Tuple<Symbol, object>(new Symbol("kdata"), kdata)
                    }
                );
            }
            else if (e is CW_GotFocus)
            {
                return new Message<object>
                (
                    new Symbol("got-focus"),
                    new Tuple<Symbol, object>[]
                    {
                        new Tuple<Symbol, object>(new Symbol("kdata"), kdata)
                    }
                );
            }
            else if (e is CW_KeyDown)
            {
                CW_KeyDown ek = (CW_KeyDown)e;
                return new Message<object>
                (
                    new Symbol("key-down"),
                    new Tuple<Symbol, object>[]
                    {
                        new Tuple<Symbol, object>(new Symbol("kdata"), kdata),
                        new Tuple<Symbol, object>(new Symbol("keydata"), ek.KeyData),
                    }
                );
            }
            else if (e is CW_KeyPress)
            {
                CW_KeyPress ek = (CW_KeyPress)e;
                return new Message<object>
                (
                    new Symbol("key-press"),
                    new Tuple<Symbol, object>[]
                    {
                        new Tuple<Symbol, object>(new Symbol("kdata"), kdata),
                        new Tuple<Symbol, object>(new Symbol("char"), ek.Char),
                    }
                );
            }
            else if (e is CW_KeyUp)
            {
                CW_KeyUp ek = (CW_KeyUp)e;
                return new Message<object>
                (
                    new Symbol("key-up"),
                    new Tuple<Symbol, object>[]
                    {
                        new Tuple<Symbol, object>(new Symbol("kdata"), kdata),
                        new Tuple<Symbol, object>(new Symbol("keydata"), ek.KeyData),
                    }
                );
            }
            else if (e is CW_LostFocus)
            {
                return new Message<object>
                (
                    new Symbol("lost-focus"),
                    new Tuple<Symbol, object>[]
                    {
                        new Tuple<Symbol, object>(new Symbol("kdata"), kdata)
                    }
                );
            }
            else if (e is CW_MouseDown)
            {
                CW_MouseDown em = (CW_MouseDown)e;
                return new Message<object>
                (
                    new Symbol("mouse-down"),
                    new Tuple<Symbol, object>[]
                    {
                        new Tuple<Symbol, object>(new Symbol("kdata"), kdata),
                        new Tuple<Symbol, object>(new Symbol("x"), BigMath.BigInteger.FromInt32(em.X)),
                        new Tuple<Symbol, object>(new Symbol("y"), BigMath.BigInteger.FromInt32(em.Y)),
                        new Tuple<Symbol, object>(new Symbol("buttons"), em.MouseButtons)
                    }
                );
            }
            else if (e is CW_MouseEnter)
            {
                return new Message<object>
                (
                    new Symbol("mouse-enter"),
                    new Tuple<Symbol, object>[]
                    {
                        new Tuple<Symbol, object>(new Symbol("kdata"), kdata)
                    }
                );
            }
            else if (e is CW_MouseLeave)
            {
                return new Message<object>
                (
                    new Symbol("mouse-leave"),
                    new Tuple<Symbol, object>[]
                    {
                        new Tuple<Symbol, object>(new Symbol("kdata"), kdata)
                    }
                );
            }
            else if (e is CW_MouseMove)
            {
                CW_MouseMove em = (CW_MouseMove)e;
                return new Message<object>
                (
                    new Symbol("mouse-move"),
                    new Tuple<Symbol, object>[]
                    {
                        new Tuple<Symbol, object>(new Symbol("kdata"), kdata),
                        new Tuple<Symbol, object>(new Symbol("x"), BigMath.BigInteger.FromInt32(em.X)),
                        new Tuple<Symbol, object>(new Symbol("y"), BigMath.BigInteger.FromInt32(em.Y))
                    }
                );
            }
            else if (e is CW_MouseUp)
            {
                CW_MouseUp em = (CW_MouseUp)e;
                return new Message<object>
                (
                    new Symbol("mouse-up"),
                    new Tuple<Symbol, object>[]
                    {
                        new Tuple<Symbol, object>(new Symbol("kdata"), kdata),
                        new Tuple<Symbol, object>(new Symbol("x"), BigMath.BigInteger.FromInt32(em.X)),
                        new Tuple<Symbol, object>(new Symbol("y"), BigMath.BigInteger.FromInt32(em.Y)),
                        new Tuple<Symbol, object>(new Symbol("buttons"), em.MouseButtons)
                    }
                );
            }
            else if (e is CW_PixelFormat)
            {
                CW_PixelFormat ep = (CW_PixelFormat)e;
                return new Message<object>
                (
                    new Symbol("pixel-format"),
                    new Tuple<Symbol, object>[]
                    {
                        new Tuple<Symbol, object>(new Symbol("kdata"), kdata),
                        new Tuple<Symbol, object>(new Symbol("pixel-format"), ep.PixelFormat),
                    }
                );
            }
            else if (e is CW_Sync)
            {
                CW_Sync es = (CW_Sync)e;
                return new Message<object>
                (
                    new Symbol("sync"),
                    new Tuple<Symbol, object>[]
                    {
                        new Tuple<Symbol, object>(new Symbol("kdata"), kdata),
                        new Tuple<Symbol, object>(new Symbol("id"), es.ID),
                        new Tuple<Symbol, object>(new Symbol("old-event-types"), es.OldEventTypes),
                        new Tuple<Symbol, object>(new Symbol("event-types"), es.EventTypes)
                    }
                );
            }
            else if (e is CW_Timer)
            {
                return new Message<object>
                (
                    new Symbol("timer"),
                    new Tuple<Symbol, object>[]
                    {
                        new Tuple<Symbol, object>(new Symbol("kdata"), kdata)
                    }
                );
            }
            else
            {
                return new Message<object>
                (
                    new Symbol("unknown"),
                    new Tuple<Symbol, object>[]
                    {
                        new Tuple<Symbol, object>(new Symbol("kdata"), kdata)
                    }
                );
            }
        }

        private abstract class InitData
        {
        }

        private class ID_Size : InitData
        {
            private int width;
            private int height;

            public ID_Size(int width, int height)
            {
                this.width = width;
                this.height = height;
            }

            public int Width { get { return width; } }
            public int Height { get { return height; } }
        }

        private class ID_Bitmap : InitData
        {
            private Bitmap b;

            public ID_Bitmap(Bitmap b)
            {
                this.b = b;
            }

            public Bitmap Bitmap { get { return b; } }
        }
    }
}
