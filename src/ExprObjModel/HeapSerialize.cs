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
using System.IO;
using System.Runtime.Serialization;

namespace ExprObjModel2
{
    public interface IHeapWriter
    {
        void WriteByte(byte b);
        void WriteInt16(short s);
        void WriteInt32(int i);
        void WriteInt64(long l);
        void WriteSByte(sbyte sb);
        void WriteUInt16(ushort us);
        void WriteUInt32(uint ui);
        void WriteUInt64(ulong ul);

        void WriteSingle(float f);
        void WriteDouble(double d);

        void WriteAddress(IHeapSerializable obj);
    }

    public interface IHeapSerializable
    {
        void HeapSerialize(IHeapWriter hw);
    }

    public static partial class Utils
    {
        private class HeapWriter : IHeapWriter
        {
            public MemoryStream ms;
            public Queue<IHeapSerializable> q;
            public ObjectIDGenerator idgen;
            public Dictionary<long, long> written;
            public Dictionary<long, ExprObjModel.FList<long>> unwritten;

            public void WriteByte(byte b)
            {
                ms.WriteByte(b);
            }

            public void WriteInt16(short s)
            {
                byte[] bs = BitConverter.GetBytes(s);
                ms.Write(bs, 0, bs.Length);
            }

            public void WriteInt32(int i)
            {
                byte[] bs = BitConverter.GetBytes(i);
                ms.Write(bs, 0, bs.Length);
            }

            public void WriteInt64(long l)
            {
                byte[] bs = BitConverter.GetBytes(l);
                ms.Write(bs, 0, bs.Length);
            }

            public void WriteSByte(sbyte sb)
            {
                ms.WriteByte(unchecked((byte)sb));
            }

            public void WriteUInt16(ushort us)
            {
                byte[] bs = BitConverter.GetBytes(us);
                ms.Write(bs, 0, bs.Length);
            }

            public void WriteUInt32(uint ui)
            {
                byte[] bs = BitConverter.GetBytes(ui);
                ms.Write(bs, 0, bs.Length);
            }

            public void WriteUInt64(ulong ul)
            {
                byte[] bs = BitConverter.GetBytes(ul);
                ms.Write(bs, 0, bs.Length);
            }

            public void WriteSingle(float f)
            {
                byte[] bs = BitConverter.GetBytes(f);
                ms.Write(bs, 0, bs.Length);
            }

            public void WriteDouble(double d)
            {
                byte[] bs = BitConverter.GetBytes(d);
                ms.Write(bs, 0, bs.Length);
            }

            public void WriteAddress(IHeapSerializable obj)
            {
                bool firstTime;
                long id = idgen.GetId(obj, out firstTime);

                if (written.ContainsKey(id))
                {
                    WriteInt64(written[id]);
                }
                else if (unwritten.ContainsKey(id))
                {
                    long fixup = ms.Position;
                    WriteInt64(0L);
                    unwritten[id] = new ExprObjModel.FList<long>(fixup, unwritten[id]);
                }
                else
                {
                    long fixup = ms.Position;
                    WriteInt64(0L);
                    unwritten.Add(id, new ExprObjModel.FList<long>(fixup));
                    q.Enqueue(obj);
                }
            }
        }

        public static byte[] SerializeAsHeap(IHeapSerializable root)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                HeapWriter h = new HeapWriter();
                h.ms = ms;
                h.q = new Queue<IHeapSerializable>();
                h.written = new Dictionary<long, long>();
                h.unwritten = new Dictionary<long, ExprObjModel.FList<long>>();
                h.idgen = new ObjectIDGenerator();

                h.q.Enqueue(root);

                while (h.q.Count > 0)
                {
                    IHeapSerializable obj = h.q.Dequeue();

                    bool firstTime;
                    long id = h.idgen.GetId(obj, out firstTime);

                    long address = h.ms.Position;

                    if (h.written.ContainsKey(id))
                    {
                        throw new InvalidOperationException("Serializing an object twice shouldn't happen");
                    }
                    else if (h.unwritten.ContainsKey(id))
                    {
                        ExprObjModel.FList<long> fixups = h.unwritten[id];
                        while (fixups != null)
                        {
                            h.ms.Position = fixups.Head;
                            h.WriteInt64(address);
                            fixups = fixups.Tail;
                        }
                        h.unwritten.Remove(id);
                        h.ms.Position = address;
                    }

                    h.written.Add(id, address);

                    obj.HeapSerialize(h);
                }

                return ms.ToArray();
            }
        }
    }
}
