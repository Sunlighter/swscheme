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
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
using ControlledWindowLib.Scheduling;
using System.Collections.Generic;

namespace ControlledWindowLib
{
    public static class ControlledWindow3
    {
        public static SignalID PostCreateWindow(this Scheduler s, int xsize, int ysize)
        {
            SignalID sid = s.GetNewSignalID();
            Thread t = new Thread(new ParameterizedThreadStart(ControlledWindowThreadProc));
            t.SetApartmentState(ApartmentState.STA);
            t.Start(new CreateParams(s, sid, new Size(xsize, ysize)));
            return sid;
        }

        public static void WindowDraw(this Scheduler s, ObjectID win, Action<Graphics> drawProcedure)
        {
            s.PostMessage(win, new CWI_Draw(drawProcedure));
        }

        public static void WindowGetPixelFormat(this Scheduler s, ObjectID win)
        {
            s.PostMessage(win, new CWI_GetPixelFormat());
        }

        public static void WindowHide(this Scheduler s, ObjectID win)
        {
            s.PostMessage(win, new CWI_Hide());
        }

        public static void WindowSetDest(this Scheduler s, ObjectID win, ObjectID? dest, object state)
        {
            s.PostMessage(win, new CWI_SetDest(dest, state));
        }

        public static void WindowSetImage(this Scheduler s, ObjectID win, Image image)
        {
            s.PostMessage(win, new CWI_SetImage(image));
        }

        public static void WindowSetTitle(this Scheduler s, ObjectID win, string title)
        {
            s.PostMessage(win, new CWI_SetTitle(title));
        }

        public static void WindowShow(this Scheduler s, ObjectID win)
        {
            s.PostMessage(win, new CWI_Show());
        }

        public static Guid WindowSync(this Scheduler s, ObjectID win, EventTypes andMask, EventTypes xorMask)
        {
            Guid g = Guid.NewGuid();
            s.PostMessage(win, new CWI_Sync(g, andMask, xorMask));
            return g;
        }

        private class CreateParams
        {
            private Scheduler s;
            private SignalID idTo;
            private Size? size;

            public CreateParams(Scheduler s, SignalID idTo, Size? size)
            {
                if (object.ReferenceEquals(s, null)) throw new ArgumentNullException("s");
                this.s = s;
                this.idTo = idTo;
                this.size = size;
            }

            public Scheduler Scheduler { get { return s; } }
            public SignalID SendIdTo { get { return idTo; } }
            public Size? Size { get { return size; } }
        }

        private static void ControlledWindowThreadProc(object obj)
        {
            if (obj is CreateParams)
            {
                CreateParams c = (CreateParams)obj;
                InternalForm i = new InternalForm(c);
                Application.Run(i);
            }
        }

        private class InternalForm : Form
        {
            private Scheduler s;
            private SignalID? idTo;
            private ObjectID? dest;
            private object destState;
            private IMessageReceiver mr;
            private Queue<object> cmds;
            private object syncRoot;

            private System.Windows.Forms.Timer t;
            private System.Drawing.Image i;
            private System.Drawing.Imaging.PixelFormat myPixelFormat;
            private EventTypes eventsWanted;
            private SignalID? gettingCommand;
            private bool reallyClosing;

            private static readonly Size DEFAULT_SIZE = new Size(512, 384);

            public InternalForm(CreateParams p)
            {
                this.s = p.Scheduler;
                this.idTo = p.SendIdTo;
                this.dest = null;
                this.destState = null;
                this.mr = null;
                this.cmds = new Queue<object>();
                this.syncRoot = new object();
                this.t = new System.Windows.Forms.Timer();
                this.i = null;
                this.myPixelFormat = System.Drawing.Imaging.PixelFormat.DontCare;
                this.eventsWanted = EventTypes.GotLostFocus | EventTypes.KeyDownUp | EventTypes.MouseEnterLeave | EventTypes.MouseDownUp;
                this.gettingCommand = null;
                this.reallyClosing = false;
                this.ClientSize = p.Size ?? DEFAULT_SIZE;
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
                this.Visible = false;
                
                t.Interval = 250;
                t.Tick += new EventHandler(t_Tick);
            }

            private void SetImage(Image i)
            {
                this.i.Dispose();
                this.i = i;
                if (this.ClientSize != i.Size)
                {
                    this.ClientSize = i.Size;
                }
                Invalidate();
            }

            private void SetTitle(string title)
            {
                this.Text = title;
            }

            private void Draw(Action<Graphics> action)
            {
                using (Graphics g = Graphics.FromImage(i))
                {
                    action(g);
                }
                Invalidate();
            }

            private void Sync(Guid id, EventTypes andMask, EventTypes xorMask)
            {
                if (dest.HasValue)
                {
                    EventTypes oldEventsWanted = eventsWanted;
                    eventsWanted = (eventsWanted & andMask) ^ xorMask;
                    s.PostMessage(dest.Value, new CW_Sync(destState, id, oldEventsWanted, eventsWanted));
                }
            }

            private void GetPixelFormat()
            {
                if (dest.HasValue)
                {
                    s.PostMessage(dest.Value, new CW_PixelFormat(destState, myPixelFormat));
                }
            }

            private void SetDest(ObjectID? newDest, object newDestState)
            {
                if (dest.HasValue)
                {
                    s.PostMessage(dest.Value, new CW_Goodbye(destState));
                }
                dest = newDest;
                destState = newDestState;
                if (dest.HasValue)
                {
                    s.PostMessage(dest.Value, new CW_Hello(destState));
                }
            }

            private void t_Tick(object sender, EventArgs e)
            {
                if (dest.HasValue && eventsWanted.HasFlag(EventTypes.Timer))
                {
                    s.PostMessage(dest.Value, new CW_Timer(destState));
                }
            }

            [DllImport("user32.dll", EntryPoint = "PostMessageW", SetLastError = true)]
            private extern static void PostMessage(IntPtr hWnd, uint wMsg, IntPtr wParam, IntPtr lParam);

            private void BeginGetCommand()
            {
                gettingCommand = mr.BeginGet();
                IntPtr handle = this.Handle;
                s.PostWait
                (
                    new SignalID[] { gettingCommand.Value },
                    delegate(SignalID dummy, object msg, bool isException)
                    {
                        lock (syncRoot)
                        {
                            cmds.Enqueue(msg);
                            PostMessage(handle, 0x401u, (IntPtr)0, (IntPtr)0);
                        }
                    }
                );
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == 0x401)
                {
                    object cmd = null;
                    lock (syncRoot)
                    {
                        cmd = cmds.Dequeue();
                    }
                    bool getAnother = true;
                    if (cmd is CWI_GetPixelFormat)
                    {
                        GetPixelFormat();
                    }
                    else if (cmd is CWI_Draw)
                    {
                        CWI_Draw d = (CWI_Draw)cmd;
                        Draw(d.DrawProcedure);
                    }
                    else if (cmd is CWI_SetImage)
                    {
                        CWI_SetImage i = (CWI_SetImage)cmd;
                        SetImage(i.Image);
                    }
                    else if (cmd is CWI_SetTitle)
                    {
                        CWI_SetTitle t = (CWI_SetTitle)cmd;
                        SetTitle(t.Title);
                    }
                    else if (cmd is CWI_Sync)
                    {
                        CWI_Sync s = (CWI_Sync)cmd;
                        Sync(s.ID, s.AndMask, s.XorMask);
                    }
                    else if (cmd is CWI_SetDest)
                    {
                        CWI_SetDest sd = (CWI_SetDest)cmd;
                        SetDest(sd.Dest, sd.State);
                    }
                    else if (cmd is CWI_Hide)
                    {
                        this.Visible = false;
                    }
                    else if (cmd is CWI_Show)
                    {
                        this.Visible = true;
                    }
                    else if (Scheduler.IsTheFinalMessage(cmd))
                    {
                        getAnother = false;
                        reallyClosing = true;
                        Close();
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Unknown message: " + ((cmd == null) ? "NULL" : cmd.GetType().ToString()));
                    }
                    if (getAnother)
                    {
                        BeginGetCommand();
                    }
                    else
                    {
                        gettingCommand = null;
                    }
                }
                else
                {
                    base.WndProc(ref m);
                }
            }

            protected override void OnLoad(EventArgs e)
            {
                mr = s.GetBlockingObject();
                System.Diagnostics.Debug.Assert(idTo.HasValue);
                s.PostSignal(idTo.Value, mr.ID, false);
                idTo = null;

                BeginGetCommand();

                using (Graphics g = CreateGraphics())
                {
                    i = new Bitmap(ClientSize.Width, ClientSize.Height, g);
                    using (Graphics h = Graphics.FromImage(i))
                    {
                        h.Clear(Color.Black);
                    }
                    myPixelFormat = ((Bitmap)i).PixelFormat;
                }
                t.Start();
                base.OnLoad(e);
            }

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                // IGNORE, do not call base class
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                e.Graphics.DrawImageUnscaled(i, new Point(0, 0));
            }

            protected override void OnKeyDown(KeyEventArgs e)
            {
                if (dest.HasValue && eventsWanted.HasFlag(EventTypes.KeyDownUp))
                {
                    s.PostMessage(dest.Value, new CW_KeyDown(destState, e.KeyData));
                }
                base.OnKeyDown(e);
            }

            protected override void OnKeyUp(KeyEventArgs e)
            {
                if (dest.HasValue && eventsWanted.HasFlag(EventTypes.KeyDownUp))
                {
                    s.PostMessage(dest.Value, new CW_KeyUp(destState, e.KeyData));
                }
                base.OnKeyUp(e);
            }

            protected override void OnKeyPress(KeyPressEventArgs e)
            {
                if (dest.HasValue && eventsWanted.HasFlag(EventTypes.KeyPress))
                {
                    s.PostMessage(dest.Value, new CW_KeyPress(destState, e.KeyChar));
                }
                base.OnKeyPress(e);
            }

            protected override void OnGotFocus(EventArgs e)
            {
                if (dest.HasValue && eventsWanted.HasFlag(EventTypes.GotLostFocus))
                {
                    s.PostMessage(dest.Value, new CW_GotFocus(destState));
                }
                base.OnGotFocus(e);
            }

            protected override void OnLostFocus(EventArgs e)
            {
                if (dest.HasValue && eventsWanted.HasFlag(EventTypes.GotLostFocus))
                {
                    s.PostMessage(dest.Value, new CW_LostFocus(destState));
                }
                base.OnLostFocus(e);
            }

            protected override void OnMouseEnter(EventArgs e)
            {
                if (dest.HasValue && eventsWanted.HasFlag(EventTypes.MouseEnterLeave))
                {
                    s.PostMessage(dest.Value, new CW_MouseEnter(destState));
                }
                base.OnMouseEnter(e);
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                if (dest.HasValue && eventsWanted.HasFlag(EventTypes.MouseEnterLeave))
                {
                    s.PostMessage(dest.Value, new CW_MouseLeave(destState));
                }
                base.OnMouseLeave(e);
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                if (dest.HasValue && eventsWanted.HasFlag(EventTypes.MouseMove))
                {
                    s.PostMessage(dest.Value, new CW_MouseMove(destState, e.X, e.Y));
                }
                base.OnMouseMove(e);
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                if (dest.HasValue && eventsWanted.HasFlag(EventTypes.MouseDownUp))
                {
                    s.PostMessage(dest.Value, new CW_MouseDown(destState, e.X, e.Y, e.Button));
                }
                base.OnMouseDown(e);
            }

            protected override void OnMouseUp(MouseEventArgs e)
            {
                if (dest.HasValue && eventsWanted.HasFlag(EventTypes.MouseDownUp))
                {
                    s.PostMessage(dest.Value, new CW_MouseUp(destState, e.X, e.Y, e.Button));
                }
                base.OnMouseUp(e);
            }

            protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
            {
                if (!reallyClosing)
                {
                    if (dest.HasValue)
                    {
                        s.PostMessage(dest.Value, new CW_CloseRequested(destState));
                    }
                    e.Cancel = true;
                }
                
                base.OnClosing(e);
            }

            protected override void OnFormClosed(FormClosedEventArgs e)
            {
                if (dest.HasValue)
                {
                    s.PostMessage(dest.Value, new CW_Closed(destState));
                }
                base.OnFormClosed(e);
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
            }
        }
    }
}
