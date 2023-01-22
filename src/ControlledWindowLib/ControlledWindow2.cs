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

namespace ControlledWindowLib
{
    public class ControlledWindow2 : IDisposable
    {
        private InternalForm internalForm;
        private Thread internalThread;
        private AsyncQueue<CWI_Command> cqueue;
        private Action<CW_Event> post;
        private ManualResetEvent ready;
        private object syncRoot;
        private bool isOpen;

        public ControlledWindow2(Action<CW_Event> post)
        {
            this.post = post;
            this.cqueue = new AsyncQueue<CWI_Command>();
            this.ready = new ManualResetEvent(false);
            this.syncRoot = new object();
            this.isOpen = false;

            this.internalThread = new Thread(new ThreadStart(ClientThread));
            this.internalThread.Name = "Controlled Window";
            this.internalThread.SetApartmentState(ApartmentState.STA);
            this.internalThread.Start();

            this.ready.WaitOne();
        }

        private void ClientThread()
        {
            internalForm = new InternalForm(this);
            Application.Run(internalForm);
            while (!(cqueue.IsEmpty)) cqueue.Get();
            cqueue.Dispose();
        }

        public void GetPixelFormat()
        {
            cqueue.Put(new CWI_GetPixelFormat());
        }

        public void SetImage(Image image)
        {
            cqueue.Put(new CWI_SetImage((Image)(image.Clone())));
        }

        public void Draw(Action<Graphics> drawProcedure)
        {
            cqueue.Put(new CWI_Draw(drawProcedure));
        }

        public void SetTitle(string title)
        {
            cqueue.Put(new CWI_SetTitle(title));
        }

        public Guid Sync(EventTypes andMask, EventTypes xorMask)
        {
            Guid g = Guid.NewGuid();
            cqueue.Put(new CWI_Sync(g, andMask, xorMask));
            return g;
        }

        public void Dispose()
        {
            if (!(cqueue.IsClosed))
            {
                cqueue.Put(new CWI_Dispose());
                cqueue.Close();
                internalThread.Join();
            }
        }

        private class InternalForm : Form
        {
            private ControlledWindow2 parent;
            private System.Windows.Forms.Timer t;
            private System.Drawing.Image i;
            private EventTypes eventsWanted;
            private System.Drawing.Imaging.PixelFormat myPixelFormat;
            private IAsyncResult gettingCommand;

            private static Size INITIAL_SIZE = new Size(512, 384);

            public InternalForm(ControlledWindow2 parent)
            {
                this.parent = parent;
                this.eventsWanted = EventTypes.GotLostFocus | EventTypes.KeyDownUp | EventTypes.MouseEnterLeave | EventTypes.MouseDownUp;
                this.ClientSize = INITIAL_SIZE;
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
                this.t = new System.Windows.Forms.Timer();
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
                EventTypes oldEventsWanted = eventsWanted;
                eventsWanted = (eventsWanted & andMask) ^ xorMask;
                parent.post(new CW_Sync(null, id, oldEventsWanted, eventsWanted));
            }

            private void GetPixelFormat()
            {
                parent.post(new CW_PixelFormat(null, myPixelFormat));
            }

            private void t_Tick(object sender, EventArgs e)
            {
                if (eventsWanted.HasFlag(EventTypes.Timer))
                {
                    parent.post(new CW_Timer(null));
                }
            }

            [DllImport("user32.dll", EntryPoint = "PostMessageW", SetLastError = true)]
            private extern static void PostMessage(IntPtr hWnd, uint wMsg, IntPtr wParam, IntPtr lParam);

            private static void GettingCommandAsyncCallback(IAsyncResult r)
            {
                IntPtr handle = (IntPtr)(r.AsyncState);
                PostMessage(handle, 0x401u, (IntPtr)0, (IntPtr)0);
            }

            private void BeginGetCommand()
            {
                gettingCommand = parent.cqueue.BeginGet(new AsyncCallback(GettingCommandAsyncCallback), this.Handle);
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == 0x401)
                {
                    try
                    {
                        CWI_Command cmd = parent.cqueue.EndGet(gettingCommand);
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
                        else if (cmd is CWI_Dispose)
                        {
                            parent.isOpen = false;
                            getAnother = false;
                            Close();
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
                    catch (AsyncQueueClosedException)
                    {
                        parent.isOpen = false;
                        Close();
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
                BeginGetCommand();

                using (Graphics g = CreateGraphics())
                {
                    i = new Bitmap(INITIAL_SIZE.Width, INITIAL_SIZE.Height, g);
                    using (Graphics h = Graphics.FromImage(i))
                    {
                        h.Clear(Color.Black);
                    }
                    myPixelFormat = ((Bitmap)i).PixelFormat;
                }
                t.Start();
                lock (parent.syncRoot)
                {
                    parent.isOpen = true;
                }
                parent.ready.Set();
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
                if (eventsWanted.HasFlag(EventTypes.KeyDownUp))
                {
                    parent.post(new CW_KeyDown(null, e.KeyData));
                }
                base.OnKeyDown(e);
            }

            protected override void OnKeyUp(KeyEventArgs e)
            {
                if (eventsWanted.HasFlag(EventTypes.KeyDownUp))
                {
                    parent.post(new CW_KeyUp(null, e.KeyData));
                }
                base.OnKeyUp(e);
            }

            protected override void OnKeyPress(KeyPressEventArgs e)
            {
                if (eventsWanted.HasFlag(EventTypes.KeyPress))
                {
                    parent.post(new CW_KeyPress(null, e.KeyChar));
                }
                base.OnKeyPress(e);
            }

            protected override void OnGotFocus(EventArgs e)
            {
                if (eventsWanted.HasFlag(EventTypes.GotLostFocus))
                {
                    parent.post(new CW_GotFocus(null));
                }
                base.OnGotFocus(e);
            }

            protected override void OnLostFocus(EventArgs e)
            {
                if (eventsWanted.HasFlag(EventTypes.GotLostFocus))
                {
                    parent.post(new CW_LostFocus(null));
                }
                base.OnLostFocus(e);
            }

            protected override void OnMouseEnter(EventArgs e)
            {
                if (eventsWanted.HasFlag(EventTypes.MouseEnterLeave))
                {
                    parent.post(new CW_MouseEnter(null));
                }
                base.OnMouseEnter(e);
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                if (eventsWanted.HasFlag(EventTypes.MouseEnterLeave))
                {
                    parent.post(new CW_MouseLeave(null));
                }
                base.OnMouseLeave(e);
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                if (eventsWanted.HasFlag(EventTypes.MouseMove))
                {
                    parent.post(new CW_MouseMove(null, e.X, e.Y));
                }
                base.OnMouseMove(e);
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                if (eventsWanted.HasFlag(EventTypes.MouseDownUp))
                {
                    parent.post(new CW_MouseDown(null, e.X, e.Y, e.Button));
                }
                base.OnMouseDown(e);
            }

            protected override void OnMouseUp(MouseEventArgs e)
            {
                if (eventsWanted.HasFlag(EventTypes.MouseDownUp))
                {
                    parent.post(new CW_MouseUp(null, e.X, e.Y, e.Button));
                }
                base.OnMouseUp(e);
            }

            protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
            {
                if (parent.isOpen)
                {
                    parent.cqueue.Put(new CWI_Dispose());
                    e.Cancel = true;
                }
                base.OnClosing(e);
            }

            protected override void OnFormClosed(FormClosedEventArgs e)
            {
                parent.post(new CW_Closed(null));
                base.OnFormClosed(e);
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
            }
        }
    }
}
