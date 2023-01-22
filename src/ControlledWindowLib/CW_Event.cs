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
    [Flags]
    public enum EventTypes
    {
        None = 0,
        KeyDownUp = 1,
        KeyPress = 2,
        MouseDownUp = 4,
        MouseEnterLeave = 8,
        MouseMove = 16,
        GotLostFocus = 32,
        Timer = 64,
        All = 127
    }

    public abstract class CW_Event
    {
        private object state;

        public CW_Event(object state)
        {
            this.state = state;
        }

        public object State { get { return state; } }
    }

    public class CW_Sync : CW_Event
    {
        private Guid id;
        private EventTypes oldEventTypes;
        private EventTypes eventTypes;

        public CW_Sync(object state, Guid id, EventTypes oldEventTypes, EventTypes eventTypes) : base(state)
        {
            this.id = id;
            this.oldEventTypes = oldEventTypes;
            this.eventTypes = eventTypes;
        }

        public Guid ID { get { return id; } }
        public EventTypes OldEventTypes { get { return oldEventTypes; } }
        public EventTypes EventTypes { get { return eventTypes; } }
    }

    public class CW_PixelFormat : CW_Event
    {
        private System.Drawing.Imaging.PixelFormat pixelFormat;

        public CW_PixelFormat(object state, System.Drawing.Imaging.PixelFormat pixelFormat) : base(state)
        {
            this.pixelFormat = pixelFormat;
        }

        public System.Drawing.Imaging.PixelFormat PixelFormat { get { return pixelFormat; } }
    }

    public class CW_Timer : CW_Event
    {
        public CW_Timer(object state) : base(state)
        {
        }
    }

    public class CW_GotFocus : CW_Event
    {
        public CW_GotFocus(object state) : base(state)
        {
        }
    }

    public class CW_LostFocus : CW_Event
    {
        public CW_LostFocus(object state) : base(state)
        {
        }
    }

    public class CW_MouseEnter : CW_Event
    {
        public CW_MouseEnter(object state) : base(state)
        {
        }
    }

    public class CW_MouseLeave : CW_Event
    {
        public CW_MouseLeave(object state) : base(state)
        {
        }
    }

    public class CW_Closed : CW_Event
    {
        public CW_Closed(object state) : base(state)
        {
        }
    }

    public class CW_KeyDown : CW_Event
    {
        private Keys keyData;

        public CW_KeyDown(object state, Keys keyData) : base(state)
        {
            this.keyData = keyData;
        }

        public Keys KeyData { get { return keyData; } }
    }

    public class CW_KeyUp : CW_Event
    {
        private Keys keyData;

        public CW_KeyUp(object state, Keys keyData) : base(state)
        {
            this.keyData = keyData;
        }

        public Keys KeyData { get { return keyData; } }
    }

    public class CW_KeyPress : CW_Event
    {
        private char ch;

        public CW_KeyPress(object state, char ch) : base(state)
        {
            this.ch = ch;
        }

        public char Char { get { return ch; } }
    }

    public class CW_MouseDown : CW_Event
    {
        private int x;
        private int y;
        private MouseButtons mouseButtons;

        public CW_MouseDown(object state, int x, int y, MouseButtons mouseButtons) : base(state)
        {
            this.x = x;
            this.y = y;
            this.mouseButtons = mouseButtons;
        }

        public int X { get { return x; } }
        public int Y { get { return y; } }
        public MouseButtons MouseButtons { get { return mouseButtons; } }
    }

    public class CW_MouseUp : CW_Event
    {
        private int x;
        private int y;
        private MouseButtons mouseButtons;

        public CW_MouseUp(object state, int x, int y, MouseButtons mouseButtons) : base(state)
        {
            this.x = x;
            this.y = y;
            this.mouseButtons = mouseButtons;
        }

        public int X { get { return x; } }
        public int Y { get { return y; } }
        public MouseButtons MouseButtons { get { return mouseButtons; } }
    }

    public class CW_MouseMove : CW_Event
    {
        private int x;
        private int y;

        public CW_MouseMove(object state, int x, int y) : base(state)
        {
            this.x = x;
            this.y = y;
        }

        public int X { get { return x; } }
        public int Y { get { return y; } }
    }

    public class CW_CloseRequested : CW_Event
    {
        public CW_CloseRequested(object state) : base(state)
        {
        }
    }

    public class CW_Goodbye : CW_Event
    {
        public CW_Goodbye(object state) : base(state)
        {
        }
    }

    public class CW_Hello : CW_Event
    {
        public CW_Hello(object state) : base(state)
        {
        }
    }

    internal abstract class CWI_Command
    {

    }

    internal class CWI_GetPixelFormat : CWI_Command
    {
        public CWI_GetPixelFormat()
        {
        }
    }

    internal class CWI_Draw : CWI_Command
    {
        private Action<Graphics> drawProcedure;

        public CWI_Draw(Action<Graphics> drawProcedure)
        {
            this.drawProcedure = drawProcedure;
        }

        public Action<Graphics> DrawProcedure { get { return drawProcedure; } }
    }

    internal class CWI_SetImage : CWI_Command
    {
        private Image image;

        public CWI_SetImage(Image image)
        {
            this.image = image;
        }

        public Image Image { get { return image; } }
    }

    internal class CWI_SetTitle : CWI_Command
    {
        private string title;

        public CWI_SetTitle(string title)
        {
            this.title = title;
        }

        public string Title { get { return title; } }
    }

    internal class CWI_Sync : CWI_Command
    {
        private Guid id;
        private EventTypes andMask;
        private EventTypes xorMask;

        public CWI_Sync(Guid id, EventTypes andMask, EventTypes xorMask)
        {
            this.id = id;
            this.andMask = andMask;
            this.xorMask = xorMask;
        }

        public Guid ID { get { return id; } }
        public EventTypes AndMask { get { return andMask; } }
        public EventTypes XorMask { get { return xorMask; } }
    }

    internal class CWI_Dispose : CWI_Command
    {
        public CWI_Dispose()
        {
        }
    }

    internal class CWI_SetDest : CWI_Command
    {
        private ControlledWindowLib.Scheduling.ObjectID? dest;
        private object state;

        public CWI_SetDest(ControlledWindowLib.Scheduling.ObjectID? dest, object state)
        {
            this.dest = dest;
            this.state = state;
        }

        public ControlledWindowLib.Scheduling.ObjectID? Dest { get { return dest; } }

        public object State { get { return state; } }
    }

    internal class CWI_Show : CWI_Command
    {
        public CWI_Show()
        {
        }
    }

    internal class CWI_Hide : CWI_Command
    {
        public CWI_Hide()
        {
        }
    }
}
