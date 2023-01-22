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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;

namespace ControlledWindowLib
{
    internal abstract partial class LogWindowInternal : Form
    {
        public LogWindowInternal()
        {
            InitializeComponent();
        }

        protected abstract void buttonCopy_Click(object sender, EventArgs e);

        protected abstract void buttonClose_Click(object sender, EventArgs e);
    }

    public class LogWindow : IDisposable
    {
        private int capacity;
        private LogWindowInternal internalForm;
        private Thread internalThread;
        private AsyncQueue<LWI_Command> cqueue;
        private ManualResetEvent ready;
        private object syncRoot;
        private bool isOpen;
        private Action onClose;

        public LogWindow(int capacity, Action onClose)
        {
            this.capacity = capacity;
            this.onClose = onClose;
            this.cqueue = new AsyncQueue<LWI_Command>();
            this.ready = new ManualResetEvent(false);
            this.syncRoot = new object();
            this.isOpen = false;

            this.internalThread = new Thread(new ThreadStart(ClientThread));
            this.internalThread.Name = "Log Window";
            this.internalThread.SetApartmentState(ApartmentState.STA);
            this.internalThread.Start();
        }

        private void ClientThread()
        {
            System.Diagnostics.Debug.WriteLine("ControlledWindowLib.LogWindow.ClientThread (began)");
            internalForm = new InternalForm(this, capacity);
            Application.Run(internalForm);
            System.Diagnostics.Debug.WriteLine("ControlledWindowLib.LogWindow.ClientThread (Application.Run returned; draining command queue)");
            while (!(cqueue.IsEmpty)) cqueue.Get();
            cqueue.Dispose();
            System.Diagnostics.Debug.WriteLine("ControlledWindowLib.LogWindow.ClientThread (ended)");
        }

        public void Clear()
        {
            cqueue.Put(new LWI_Clear());
        }

        public void Message(string msg, bool important)
        {
            cqueue.Put(new LWI_Message(msg, important));
        }

        public void Dispose()
        {
            System.Diagnostics.Debug.WriteLine("ControlledWindowLib.LogWindow.Dispose");
            if (!(cqueue.IsClosed))
            {
                cqueue.Put(new LWI_Dispose());
                cqueue.Close();
                internalThread.Join();
            }
        }

        private class InternalForm : LogWindowInternal
        {
            private LogWindow parent;
            private int capacity;
            private IAsyncResult gettingCommand;

            public InternalForm(LogWindow parent, int capacity)
            {
                this.parent = parent;
                this.capacity = capacity;
            }

            protected override void buttonCopy_Click(object sender, EventArgs e)
            {
                StringBuilder sb = new StringBuilder();
                bool needNewline = false;
                foreach(string str in listBox1.SelectedItems)
                {
                    if (needNewline) sb.Append("\r\n");
                    sb.Append(str);
                }
                Clipboard.Clear();
                Clipboard.SetData(DataFormats.UnicodeText, sb.ToString());
            }

            protected override void buttonClose_Click(object sender, EventArgs e)
            {
                Close();
            }

            private void Message(string message, bool important)
            {
                listBox1.Items.Add(message);
                if (listBox1.Items.Count > capacity)
                {
                    listBox1.Items.RemoveAt(0);
                }
            }

            private void ClearMessages()
            {
                listBox1.Items.Clear();
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
                        LWI_Command cmd = parent.cqueue.EndGet(gettingCommand);
                        bool getAnother = true;
                        if (cmd is LWI_Message)
                        {
                            LWI_Message msg = (LWI_Message)cmd;
                            Message(msg.Message, msg.Important);
                        }
                        else if (cmd is LWI_Clear)
                        {
                            ClearMessages();
                        }
                        else if (cmd is LWI_Dispose)
                        {
                            System.Diagnostics.Debug.WriteLine("ControlledWindowLib.LogWindow.InternalForm.WndProc (cmd is LWI_Dispose)");
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
                            parent.cqueue.Dispose();
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

                lock (parent.syncRoot)
                {
                    parent.isOpen = true;
                }
                parent.ready.Set();
                base.OnLoad(e);
            }

            protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
            {
                System.Diagnostics.Debug.WriteLine("ControlledWindowLib.LogWindow.InternalForm.OnClosing");
                if (parent.isOpen)
                {
                    parent.cqueue.Put(new LWI_Dispose());
                    e.Cancel = true;
                }
                base.OnClosing(e);
            }

            protected override void OnFormClosed(FormClosedEventArgs e)
            {
                System.Diagnostics.Debug.WriteLine("ControlledWindowLib.LogWindow.InternalForm.OnFormClosed");
                parent.onClose();
                base.OnFormClosed(e);
            }
        }
    }

    internal abstract class LWI_Command
    {
    }

    internal class LWI_Message : LWI_Command
    {
        private string msg;
        private bool important;

        public LWI_Message(string msg, bool important)
        {
            this.msg = msg;
            this.important = important;
        }

        public string Message { get { return msg; } }
        public bool Important { get { return important; } }
    }

    internal class LWI_Clear : LWI_Command
    {
        public LWI_Clear()
        {
        }
    }

    internal class LWI_Dispose : LWI_Command
    {
        public LWI_Dispose()
        {
        }
    }
}
