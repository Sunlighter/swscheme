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

namespace ExprObjModel.ObjectSystem
{
    public class TemporaryObject : IMessageHandler<ExtendedMessage>
    {
        private ObjectSystem<ExtendedMessage> objectSystem;
        private OldObjectID self;
        private ControlledWindowLib.AsyncQueue<ExtendedMessage> queue;
        private IGlobalState gs;

        public TemporaryObject(IGlobalState gs, ControlledWindowLib.AsyncQueue<ExtendedMessage> queue)
        {
            this.gs = gs;
            this.queue = queue;
        }

        public void Welcome(ObjectSystem<ExtendedMessage> objectSystem, OldObjectID self)
        {
            this.objectSystem = objectSystem;
            this.self = self;
        }

        public void Handle(ExtendedMessage message)
        {
            if (message is EM_SchemeMessage || message is EM_HandlerListResponse || message is EM_FieldListResponse)
            {
                queue.Put(message);
                gs.RemoveOldObject(self);
            }
            else if (message is EM_GetHandlerList)
            {
                EM_GetHandlerList ghl = (EM_GetHandlerList)message;
                gs.OldPostMessage(ghl.K, new EM_HandlerListResponse(new HashSet<Signature>(), ghl.KData));
            }
            else if (message is EM_GetFieldList)
            {
                EM_GetFieldList gfl = (EM_GetFieldList)message;
                gs.OldPostMessage(gfl.K, new EM_FieldListResponse(new HashSet<Symbol>(), gfl.KData));
            }
            else
            {
                Console.WriteLine("Unexpected message of type " + message.GetType());
            }
        }

        public void Dispose()
        {
            // do nothing
        }
    }
}
