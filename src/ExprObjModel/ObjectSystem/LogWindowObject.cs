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
    public class LogWindowObject : IMessageHandler<ExtendedMessage>
    {
        private ObjectSystem<ExtendedMessage> objectSystem;
        private OldObjectID self;
        private LogWindow window;
        private IGlobalState gs;

        public LogWindowObject(IGlobalState gs)
        {
            this.gs = gs;
        }

        public void OnClose()
        {
            this.objectSystem.Post(self, new EM_Close());
        }

        public void Welcome(ObjectSystem<ExtendedMessage> objectSystem, OldObjectID self)
        {
            this.objectSystem = objectSystem;
            this.self = self;
            this.window = new LogWindow(500, new Action(OnClose));
        }

        public void Handle(ExtendedMessage message)
        {
            if (message is EM_SchemeMessage)
            {
                EM_SchemeMessage m = (EM_SchemeMessage)message;
                window.Message(SchemeDataWriter.ItemToString(m.Message), false);
            }
            else if (message is EM_GetFieldList)
            {
                EM_GetFieldList gfl = (EM_GetFieldList)message;
                objectSystem.Post(gfl.K, new EM_FieldListResponse(new HashSet<Symbol>(), gfl.KData));
            }
            else if (message is EM_GetHandlerList)
            {
                EM_GetHandlerList ghl = (EM_GetHandlerList)message;
                objectSystem.Post(ghl.K, new EM_HandlerListResponse(new HashSet<Signature>(), ghl.KData));
            }
            else if (message is EM_Close)
            {
                gs.RemoveOldObject(self);
                this.Dispose();
            }
            else
            {
                window.Message("Unknown message " + message.GetType().FullName, true);
            }
        }

        public void Dispose()
        {
            window.Dispose();
        }
    }
}