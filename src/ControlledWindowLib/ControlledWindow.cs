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
    public class ControlledWindow : IDisposable
    {
        private ControlledWindow2 cw;
        private AsyncQueue<CW_Event> aqueue;

        public ControlledWindow()
        {
            aqueue = new AsyncQueue<CW_Event>();
            cw = new ControlledWindow2(new Action<CW_Event>(Post));
        }

        private void Post(CW_Event e)
        {
            if (!(aqueue.IsClosed))
            {
                aqueue.Put(e);
            }
        }

        public IAsyncResult BeginGetEvent(AsyncCallback callback, object state)
        {
            return aqueue.BeginGet(callback, state);
        }

        public CW_Event EndGetEvent(IAsyncResult iar)
        {
            return aqueue.EndGet(iar);
        }

        public CW_Event GetEvent()
        {
            return aqueue.Get();
        }

        public void GetPixelFormat()
        {
            cw.GetPixelFormat();
        }

        public void SetImage(Image image)
        {
            cw.SetImage(image);
        }

        public void Draw(Action<Graphics> drawProcedure)
        {
            cw.Draw(drawProcedure);
        }

        public void SetTitle(string title)
        {
            cw.SetTitle(title);
        }

        public Guid Sync(EventTypes andMask, EventTypes xorMask)
        {
            return cw.Sync(andMask, xorMask);
        }

        public void Dispose()
        {
            cw.Dispose();
            System.Threading.Thread.Sleep(1);
            aqueue.Close();
            while (!(aqueue.IsEmpty)) aqueue.Get();
            aqueue.Dispose();
        }
    }
}
