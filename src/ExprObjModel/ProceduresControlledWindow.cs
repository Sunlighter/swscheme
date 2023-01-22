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
using ControlledWindowLib;
using System.Drawing;
using ControlledWindowLib.Scheduling;

namespace ExprObjModel.Procedures
{
    public static partial class ProxyDiscovery
    {
        [SchemeFunction("make-window")]
        public static DisposableID CreateWindow(IGlobalState gs, int xsize, int ysize)
        {
            ControlledWindow cw = new ControlledWindow();
            using (Bitmap b = new Bitmap(xsize, ysize, System.Drawing.Imaging.PixelFormat.Format32bppRgb))
            {
                cw.SetImage(b);
            }
            
            cw.Sync(EventTypes.None, EventTypes.GotLostFocus | EventTypes.KeyDownUp | EventTypes.KeyPress);

            DisposableID d = gs.RegisterDisposable(cw, "Window");
            return d;
        }

        [SchemeFunction("make-window-from-bitmap")]
        public static DisposableID CreateWindow(IGlobalState gs, Bitmap b)
        {
            ControlledWindow cw = new ControlledWindow();
            cw.SetImage(b);

            cw.Sync(EventTypes.None, EventTypes.GotLostFocus | EventTypes.KeyDownUp | EventTypes.KeyPress);

            DisposableID d = gs.RegisterDisposable(cw, "Window");
            return d;
        }

        [SchemeFunction("set-window-image!")]
        public static void SetWindowImage(ControlledWindow cw, Bitmap b)
        {
            cw.SetImage(b);
        }

        [SchemeFunction("window-draw!")]
        public static void WindowDraw(ControlledWindow cw, object drawing)
        {
            ExprObjModel.Drawing.DrawForm d = ExprObjModel.Drawing.Parser.Parse(drawing);

            if (d == null) throw new SchemeRuntimeException("Unable to parse drawing");

            cw.Draw
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

        [SchemeFunction("get-window-pixel-format")]
        public static void GetWindowPixelFormat(ControlledWindow cw)
        {
            cw.GetPixelFormat();
        }

        [SchemeFunction("set-window-title!")]
        public static void SetWindowTitle(ControlledWindow cw, string str)
        {
            cw.SetTitle(str);
        }

        [SchemeFunction("window-sync!")]
        public static Guid WindowSync(ControlledWindow cw, EventTypes andMask, EventTypes xorMask)
        {
            return cw.Sync(andMask, xorMask);
        }

        private static object MarshalGetWindowEvent(CW_Event e)
        {
            if (e is CW_Closed)
            {
                SchemeHashMap m = new SchemeHashMap();
                m[new Symbol("type")] = new Symbol("closed");
                return m;
            }
            else if (e is CW_GotFocus)
            {
                SchemeHashMap m = new SchemeHashMap();
                m[new Symbol("type")] = new Symbol("got-focus");
                return m;
            }
            else if (e is CW_LostFocus)
            {
                SchemeHashMap m = new SchemeHashMap();
                m[new Symbol("type")] = new Symbol("lost-focus");
                return m;
            }
            else if (e is CW_MouseEnter)
            {
                SchemeHashMap m = new SchemeHashMap();
                m[new Symbol("type")] = new Symbol("mouse-enter");
                return m;
            }
            else if (e is CW_MouseLeave)
            {
                SchemeHashMap m = new SchemeHashMap();
                m[new Symbol("type")] = new Symbol("mouse-leave");
                return m;
            }
            else if (e is CW_Timer)
            {
                SchemeHashMap m = new SchemeHashMap();
                m[new Symbol("type")] = new Symbol("timer-tick");
                return m;
            }
            else if (e is CW_KeyDown)
            {
                CW_KeyDown kd = (CW_KeyDown)e;
                SchemeHashMap m = new SchemeHashMap();
                m[new Symbol("type")] = new Symbol("key-down");
                m[new Symbol("key-data")] = kd.KeyData;
                return m;
            }
            else if (e is CW_KeyUp)
            {
                CW_KeyUp ku = (CW_KeyUp)e;
                SchemeHashMap m = new SchemeHashMap();
                m[new Symbol("type")] = new Symbol("key-up");
                m[new Symbol("key-data")] = ku.KeyData;
                return m;
            }
            else if (e is CW_KeyPress)
            {
                CW_KeyPress kp = (CW_KeyPress)e;
                SchemeHashMap m = new SchemeHashMap();
                m[new Symbol("type")] = new Symbol("key-press");
                m[new Symbol("char")] = kp.Char;
                return m;
            }
            else if (e is CW_MouseDown)
            {
                CW_MouseDown md = (CW_MouseDown)e;
                SchemeHashMap m = new SchemeHashMap();
                m[new Symbol("type")] = new Symbol("mouse-down");
                m[new Symbol("x")] = BigMath.BigInteger.FromInt32(md.X);
                m[new Symbol("y")] = BigMath.BigInteger.FromInt32(md.Y);
                m[new Symbol("buttons")] = md.MouseButtons;
                return m;
            }
            else if (e is CW_MouseUp)
            {
                CW_MouseUp mu = (CW_MouseUp)e;
                SchemeHashMap m = new SchemeHashMap();
                m[new Symbol("type")] = new Symbol("mouse-up");
                m[new Symbol("x")] = BigMath.BigInteger.FromInt32(mu.X);
                m[new Symbol("y")] = BigMath.BigInteger.FromInt32(mu.Y);
                m[new Symbol("buttons")] = mu.MouseButtons;
                return m;
            }
            else if (e is CW_MouseMove)
            {
                CW_MouseMove mm = (CW_MouseMove)e;
                SchemeHashMap m = new SchemeHashMap();
                m[new Symbol("type")] = new Symbol("mouse-move");
                m[new Symbol("x")] = BigMath.BigInteger.FromInt32(mm.X);
                m[new Symbol("y")] = BigMath.BigInteger.FromInt32(mm.Y);
                return m;
            }
            else if (e is CW_PixelFormat)
            {
                CW_PixelFormat pf = (CW_PixelFormat)e;
                SchemeHashMap m = new SchemeHashMap();
                m[new Symbol("type")] = new Symbol("pixel-format");
                m[new Symbol("pixel-format")] = pf.PixelFormat;
                return m;
            }
            else if (e is CW_Sync)
            {
                CW_Sync s = (CW_Sync)e;
                SchemeHashMap m = new SchemeHashMap();
                m[new Symbol("type")] = new Symbol("sync");
                m[new Symbol("id")] = s.ID;
                m[new Symbol("event-types")] = s.EventTypes;
                m[new Symbol("old-event-types")] = s.OldEventTypes;
                return m;
            }
            else return e;
        }

        [SchemeFunction("begin-get-window-event")]
        public static SignalID BeginGetWindowEvent(IGlobalState gs, ControlledWindow cw)
        {
            SignalID sid = gs.Scheduler.GetNewSignalID();
            IAsyncResult iar = cw.BeginGetEvent(null, null);
            gs.Scheduler.PostActionOnCompletion
            (
                iar.AsyncWaitHandle,
                delegate()
                {
                    CW_Event e = cw.EndGetEvent(iar);
                    gs.Scheduler.PostSignal(sid, MarshalGetWindowEvent(e), false);
                }
            );
            gs.RegisterSignal(sid, "get-window-event", false);
            return sid;
        }
    }
}