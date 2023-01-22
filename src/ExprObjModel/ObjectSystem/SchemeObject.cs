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
    public class SchemeObject : IMessageHandler<ExtendedMessage>
    {
        private ObjectSystem<ExtendedMessage> objectSystem;
        private OldObjectID self;
        private IGlobalState gs;

        private Dictionary<Signature, IMsgProcedure> handlers;
        private IProcedure catchAll;
        private Dictionary<Symbol, Box> fields;

        public SchemeObject(IGlobalState gs)
        {
            this.handlers = new Dictionary<Signature, IMsgProcedure>();
            this.fields = new Dictionary<Symbol, Box>();
            this.gs = gs.WithCurrentObject(this);
        }

        public void Welcome(ObjectSystem<ExtendedMessage> objectSystem, OldObjectID self)
        {
            this.objectSystem = objectSystem;
            this.self = self;
        }

        public bool HasLocal(Symbol s) { return fields.ContainsKey(s); }

        public Box GetLocal(Symbol s) { return fields[s]; }

        public void Handle(ExtendedMessage message)
        {
            if (message is EM_SchemeMessage)
            {
                EM_SchemeMessage sm = (EM_SchemeMessage)message;
                Signature s = sm.Message.Signature;
                if (handlers.ContainsKey(s))
                {
                    IMsgProcedure proc = handlers[s];
                    try
                    {
                        DoerResult dr = Doer.Apply(gs, proc, sm.Message);
                        if (dr.IsException)
                        {
                            Console.WriteLine("Exception while handling message: " + SchemeDataWriter.ItemToString(dr.Result));
                        }
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine("Improperly handled exception while handling message:");
                        Console.WriteLine(exc);
                    }
                }
                else if (catchAll != null)
                {
                    try
                    {
                        DoerResult dr = Doer.Apply(gs, catchAll, new FList<object>(sm.Message));
                        if (dr.IsException)
                        {
                            Console.WriteLine("Exception while running catch-all message handler: " + SchemeDataWriter.ItemToString(dr.Result));
                        }
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine("Improperly handled exception while running catch-all message handler:");
                        Console.WriteLine(exc);
                    }
                }
                else
                {
                    // do nothing
                }
            }
            else if (message is EM_SetHandler)
            {
                EM_SetHandler sh = (EM_SetHandler)message;
                if (handlers.ContainsKey(sh.MProc.Signature))
                {
                    handlers[sh.MProc.Signature] = sh.MProc;
                }
                else
                {
                    handlers.Add(sh.MProc.Signature, sh.MProc);
                }
            }
            else if (message is EM_UnsetHandler)
            {
                EM_UnsetHandler uh = (EM_UnsetHandler)message;
                if (handlers.ContainsKey(uh.Signature))
                {
                    handlers.Remove(uh.Signature);
                }
            }
            else if (message is EM_SetCatchAll)
            {
                EM_SetCatchAll sca = (EM_SetCatchAll)message;
                catchAll = sca.Proc;
            }
            else if (message is EM_UnsetCatchAll)
            {
                catchAll = null;
            }
            else if (message is EM_AddField)
            {
                EM_AddField af = (EM_AddField)message;
                if (fields.ContainsKey(af.FieldName))
                {
                    Console.WriteLine("Adding field " + af.FieldName + " which already exists");
                }
                else
                {
                    fields.Add(af.FieldName, new Box(af.InitialValue));
                }
            }
            else if (message is EM_RemoveField)
            {
                EM_RemoveField rf = (EM_RemoveField)message;
                if (fields.ContainsKey(rf.FieldName))
                {
                    fields.Remove(rf.FieldName);
                }
                else
                {
                    Console.WriteLine("Removing nonexistent field " + rf.FieldName);
                }
            }
            else if (message is EM_GetHandlerList)
            {
                EM_GetHandlerList ghl = (EM_GetHandlerList)message;
                bool queued = objectSystem.Post(ghl.K, new EM_HandlerListResponse(handlers.Keys.ToHashSet(), ghl.KData));
                if (!queued) Console.WriteLine("Handler List Response not successfully queued");
            }
            else if (message is EM_GetFieldList)
            {
                EM_GetFieldList gfl = (EM_GetFieldList)message;
                bool queued = objectSystem.Post(gfl.K, new EM_FieldListResponse(fields.Keys.ToHashSet(), gfl.KData));
                if (!queued) Console.WriteLine("Field List Response not successfully queued");
            }
            else
            {
                Console.WriteLine("Unknown extended message: " + message.GetType());
            }
        }

        public OldObjectID Self { get { return self; } }

        public void Dispose()
        {
            gs.Dispose();
        }

    }
}
