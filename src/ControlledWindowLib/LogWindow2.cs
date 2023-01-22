using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ControlledWindowLib.Scheduling;
using System.Runtime.InteropServices;
using System.Threading;
using ExprObjModel;

namespace ControlledWindowLib
{
    public partial class LogWindow2 : Form
    {
        private Scheduler s;
        private SignalID? idTo;
        private ObjectID? dest;
        private object destState;
        private IMessageReceiver mr;
        private Queue<object> cmds;
        private object syncRoot;

        private SignalID? gettingCommand;
        private bool reallyClosing;
        private Deque<LogItem> messages;
        private Deque<LogItem> pausedMessages;
        private bool isPaused;
        private int nextNumber;

        private static readonly int MAX_MESSAGES = 20;

        public LogWindow2(Scheduler s, SignalID idTo)
        {
            InitializeComponent();

            this.s = s;
            this.idTo = idTo;
            this.dest = null;
            this.destState = null;
            this.mr = null;
            this.cmds = new Queue<object>();
            this.syncRoot = new object();

            this.gettingCommand = null;
            this.reallyClosing = false;
            this.messages = new Deque<LogItem>();
            this.pausedMessages = new Deque<LogItem>();
            this.isPaused = false;
            this.nextNumber = 0;
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

        private void Clear()
        {
            messages.Clear();
            Invalidate();
        }

        private void AddMessage(LWI_Message msg)
        {
            int number = nextNumber; ++nextNumber;
            if (!isPaused)
            {
                messages.PushBack(new LogItem(msg.Important, number, msg.Message));
                if (messages.Count > MAX_MESSAGES)
                {
                    messages.PopFront();
                }
                listView1.VirtualListSize = messages.Count;
                if (cbScrollToEnd.Checked)
                {
                    listView1.EnsureVisible(messages.Count - 1);
                }
                listView1.Refresh();
            }
            else
            {
                pausedMessages.PushBack(new LogItem(msg.Important, number, msg.Message));
                if (pausedMessages.Count > MAX_MESSAGES)
                {
                    pausedMessages.PopFront();
                }
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
                if (cmd is LWI_Clear)
                {
                    Clear();
                }
                else if (cmd is LWI_Message)
                {
                    AddMessage((LWI_Message)cmd);
                }
                else if (cmd is CWI_SetDest)
                {
                    CWI_SetDest msg = (CWI_SetDest)cmd;
                    SetDest(msg.Dest, msg.State);
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

        private void LogWindow2_Load(object sender, EventArgs e)
        {
            mr = s.GetBlockingObject();
            System.Diagnostics.Debug.Assert(idTo.HasValue);
            s.PostSignal(idTo.Value, mr.ID, false);
            idTo = null;

            chMessage.Width = listView1.ClientSize.Width - chNumber.Width;

            BeginGetCommand();
        }

        private void listView1_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            LogItem li = messages[e.ItemIndex];
            ListViewItem lvi = new ListViewItem(new string[] { li.Number.ToString(), li.Message });
            if (li.IsRed)
            {
                lvi.ForeColor = Color.Red;
                lvi.BackColor = Color.Yellow;
            }
            e.Item = lvi;
        }

        private void buttonPause_Click(object sender, EventArgs e)
        {
            if (!isPaused)
            {
                isPaused = true;
                buttonPause.Text = "&Resume";
            }
            else
            {
                isPaused = false;
                int msize = messages.Count + pausedMessages.Count;
                if (msize > MAX_MESSAGES)
                {
                    int excess = msize - MAX_MESSAGES;
                    messages.PopFront(excess);
                }
                messages.PushBack(pausedMessages);
                pausedMessages.Clear();
                listView1.VirtualListSize = messages.Count;
                listView1.Refresh();
                buttonPause.Text = "&Pause";
            }
        }

        private void buttonCopy_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            bool needNewline = false;
            foreach (LogItem li in messages)
            {
                if (needNewline) sb.Append("\r\n");
                sb.Append(li.IsRed ? "*** " : "    ");
                sb.Append(li.Number);
                sb.Append(": ");
                sb.Append(li.Message);
                needNewline = true;
            }
            Clipboard.Clear();
            Clipboard.SetData(DataFormats.UnicodeText, sb.ToString());
        }

        private void LogWindow2_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!reallyClosing)
            {
                if (dest.HasValue)
                {
                    s.PostMessage(dest.Value, new CW_CloseRequested(destState));
                }
                e.Cancel = true;
            }
        }

        private void LogWindow2_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (dest.HasValue)
            {
                s.PostMessage(dest.Value, new CW_Closed(destState));
            }
        }

        private void listView1_ClientSizeChanged(object sender, EventArgs e)
        {
            chMessage.Width = listView1.ClientSize.Width - chNumber.Width;
        }
    }

    internal class LogItem
    {
        private bool isRed;
        private int number;
        private string message;

        public LogItem(bool isRed, int number, string message)
        {
            this.isRed = isRed;
            this.number = number;
            this.message = message;
        }

        public bool IsRed { get { return isRed; } }
        public int Number { get { return number; } }
        public string Message { get { return message; } }
    }

    public static class LogWindowUtil
    {
        public static SignalID PostCreateLogWindow(this Scheduler s)
        {
            SignalID sid = s.GetNewSignalID();
            Thread t = new Thread(new ParameterizedThreadStart(LogWindowThreadProc));
            t.SetApartmentState(ApartmentState.STA);
            t.Start(new CreateParams(s, sid));
            return sid;
        }

        private class CreateParams
        {
            private Scheduler s;
            private SignalID idTo;

            public CreateParams(Scheduler s, SignalID idTo)
            {
                this.s = s;
                this.idTo = idTo;
            }

            public Scheduler Scheduler { get { return s; } }
            public SignalID SendIdTo { get { return idTo; } }
        }

        private static void LogWindowThreadProc(object obj)
        {
            if (obj is CreateParams)
            {
                CreateParams c = (CreateParams)obj;
                LogWindow2 form = new LogWindow2(c.Scheduler, c.SendIdTo);
                Application.Run(form);
            }
        }

        public static void LogWindowPost(this Scheduler s, ObjectID logWindow, string message, bool isImportant)
        {
            s.PostMessage(logWindow, new LWI_Message(message, isImportant));
        }
    }
}
