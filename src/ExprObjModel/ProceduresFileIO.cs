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
using BigMath;
using System.Text;
using System.IO;
using System.Linq;
using System.ComponentModel;
using ControlledWindowLib;
using ControlledWindowLib.Scheduling;

namespace ExprObjModel.Procedures
{
    public static partial class ProxyDiscovery
    {
        [SchemeFunction("file-exists?")]
        public static bool FileExists(object o)
        {
            if (o is SchemeString)
            {
                return File.Exists(((SchemeString)o).TheString);
            }
            else if (o is FileInfo)
            {
                return ((FileInfo)o).Exists;
            }
            else return false;
        }

        [SchemeFunction("directory-exists?")]
        public static bool DirectoryExists(object o)
        {
            if (o is SchemeString)
            {
                return Directory.Exists(((SchemeString)o).TheString);
            }
            else if (o is DirectoryInfo)
            {
                return ((DirectoryInfo)o).Exists;
            }
            else return false;
        }

        [SchemeFunction("get-desktop")]
        public static string GetDesktop()
        {
            return System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory);
        }

        [SchemeFunction("list-directory")]
        public static object ListDirectory(object o)
        {
            object theList = SpecialValue.EMPTY_LIST;
            DirectoryInfo temp = null;
            if (o is DirectoryInfo)
            {
                temp = (DirectoryInfo)o;
            }
            else if (o is SchemeString)
            {
                temp =  new DirectoryInfo(((SchemeString)o).TheString);
                if (!temp.Exists) throw new SchemeRuntimeException("Directory does not exist");
            }
            else if (o is FileInfo)
            {
                return theList;
            }
            else throw new SchemeRuntimeException("list-directory requires a string or a FileSystemInfo");

            if (temp.Exists)
            {
                foreach(FileSystemInfo fi in temp.GetFileSystemInfos())
                {
                    theList = new ConsCell(fi, theList);
                }
            }
            ConsCell.Reverse(ref theList);
            return theList;
        }

        [SchemeFunction("is-fsi?")]
        public static bool IsFileSystemInfo(object obj)
        {
            return obj is FileSystemInfo;
        }

        [SchemeFunction("fsi-is-directory?")]
        public static bool IsDirectory(object obj)
        {
            if (obj is DirectoryInfo)
            {
                return true;
            }
            else return false;
        }

        [SchemeFunction("fsi-is-file?")]
        public static bool IsFile(object obj)
        {
            if (obj is FileInfo)
            {
                return true;
            }
            else return false;
        }

        [SchemeFunction("fsi-get-name")]
        public static string GetFileName(FileSystemInfo fi)
        {
            return fi.Name;
        }

        [SchemeFunction("fsi-get-length")]
        public static long GetFileLength(FileSystemInfo fi)
        {
            return (fi is FileInfo) ? ((FileInfo)fi).Length : 0L;
        }

        private static string GetPathname(string name, object o)
        {
            string pathName = null;
            if (o is SchemeString)
            {
                pathName = ((SchemeString)o).TheString;
            }
            else if (o is FileSystemInfo)
            {
                if (o is FileInfo)
                {
                    pathName = ((FileInfo)o).FullName;
                }
                else
                {
                    throw new SchemeRuntimeException("Cannot " + name + " of a directory!");
                }
            }
            else
            {
                throw new SchemeRuntimeException("Unknown argument type for " + name);
            }
            return pathName;
        }

        [SchemeFunction("slurp-bytes")]
        public static SchemeByteArray SlurpBytes(object o)
        {
            string pathName = GetPathname("slurp-bytes", o);
            byte[] b = File.ReadAllBytes(pathName);
            return new SchemeByteArray(b, DigitOrder.LBLA);
        }

        [SchemeFunction("unslurp-bytes")]
        public static void UnslurpBytes(object o, SchemeByteArray bytes)
        {
            string pathName = GetPathname("slurp-lines", o);
            File.WriteAllBytes(pathName, bytes.Bytes);
        }

        [SchemeFunction("slurp-lines")]
        public static Deque<object> SlurpLines(object o)
        {
            string pathName = GetPathname("slurp-lines", o);
            string[] strArray = File.ReadAllLines(pathName, Encoding.UTF8);
            Deque<object> d = new Deque<object>();
            foreach (string str in strArray)
            {
                d.PushBack(new SchemeString(str));
            }
            return d;
        }

        [SchemeFunction("unslurp-lines")]
        public static void UnslurpLines(object o, Deque<object> lines)
        {
            string pathName = GetPathname("unslurp-lines", o);
            if (lines.Any(x => !(x is SchemeString))) throw new SchemeRuntimeException("unslurp-lines: Non-string in vector parameter");
            File.WriteAllLines(pathName, lines.Select(x => ((SchemeString)x).TheString), Encoding.UTF8);
        }

        private class FileStringSource : IStringSource
        {
            private string[] lines;
            int index;

            public FileStringSource(string pathName)
            {
                lines = File.ReadAllLines(pathName, Encoding.UTF8);
                index = -1;
            }

            #region IStringSource Members

            public bool Next(int parenDepth)
            {
                ++index;
                return (index < lines.Length);
            }

            public string Current
            {
                get { return lines[index]; }
            }

            #endregion
        }

        [SchemeFunction("slurp-data")]
        public static Deque<object> SlurpData(object o)
        {
            string pathName = GetPathname("slurp-data", o);
            FileStringSource fss = new FileStringSource(pathName);
            SchemeDataReader sdr = new SchemeDataReader(new LexemeSource(fss));
            Deque<object> d = new Deque<object>();
            while (true)
            {
                object obj = sdr.ReadItem();
                if (obj == null) break;
                d.PushBack(obj);
            }
            return d;
        }

        [SchemeFunction("byte-copy-with-files!")]
        public static int ByteCopyWithFiles(object src, long srcOff, int len, object dest, long destOff)
        {
            if (src is SchemeByteArray && dest is SchemeByteArray)
            {
                SchemeByteArray aSrc = (SchemeByteArray)src;
                SchemeByteArray aDest = (SchemeByteArray)dest;

                if (!aSrc.IsValidRange(srcOff, len)) throw new SchemeRuntimeException("Source offset or length out of range");
                if (!aDest.IsValidRange(destOff, len)) throw new SchemeRuntimeException("Dest offset or length out of range");

                Buffer.BlockCopy(aSrc.Bytes, checked((int)srcOff), aDest.Bytes, checked((int)destOff), len);

                return len;
            }
            else if (src is SchemeByteArray && ((dest is FileInfo) || (dest is SchemeString)))
            {
                SchemeByteArray aSrc = (SchemeByteArray)src;
                string destFile = GetPathname("byte-copy-with-files!", dest);
                using (FileStream fs = new FileStream(destFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 32768))
                {
                    fs.Seek(destOff, SeekOrigin.Begin);
                    fs.Write(aSrc.Bytes, checked((int)srcOff), len);
                    fs.Flush();
                }
                return len;
            }
            else if (((src is FileInfo) || (src is SchemeString)) && dest is SchemeByteArray)
            {
                string srcFile = GetPathname("byte-copy-with-files!", src);
                SchemeByteArray aDest = (SchemeByteArray)dest;
                int bytesRead;
                using (FileStream fs = new FileStream(srcFile, FileMode.Open, FileAccess.Read, FileShare.Read, 32768))
                {
                    fs.Seek(srcOff, SeekOrigin.Begin);
                    bytesRead = fs.Read(aDest.Bytes, checked((int)destOff), len);
                }
                return bytesRead;
            }
            else if (((src is FileInfo) || (src is SchemeString)) && ((dest is FileInfo) || (dest is SchemeString)))
            {
                string srcFile = GetPathname("byte-copy-with-files!", src);
                string destFile = GetPathname("byte-copy-with-files!", dest);
                int bytesCopied = 0;
                using (FileStream fs1 = new FileStream(srcFile, FileMode.Open, FileAccess.Read, FileShare.Read, 32768))
                {
                    fs1.Seek(srcOff, SeekOrigin.Begin);
                    using (FileStream fs2 = new FileStream(destFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 32768))
                    {
                        fs2.Seek(destOff, SeekOrigin.Begin);
                        int BUFSIZE = 4 * 1024 * 1024;
                        byte[] buf = new byte[BUFSIZE];
                        int bytesToCopy = len;

                        while (bytesToCopy > 0)
                        {
                            int thisTime = Math.Min(bytesToCopy, BUFSIZE);
                            int bytesRead = fs1.Read(buf, 0, thisTime);
                            fs2.Write(buf, 0, bytesRead);
                            bytesCopied += bytesRead;
                            if (bytesRead < thisTime) break;
                            bytesToCopy -= thisTime;
                        }
                        fs2.Flush();
                    }
                }
                return bytesCopied;
            }
            else throw new SchemeRuntimeException("byte-copy-with-files! has wrong argument types");
        }

        [SchemeFunction("eof?")]
        public static bool IsEof(object obj)
        {
            return (obj is SpecialValue && ((SpecialValue)obj) == SpecialValue.EOF);
        }

        [SchemeFunction("eof")]
        public static object MakeEof()
        {
            return SpecialValue.EOF;
        }

        [SchemeFunction("stream?")]
        public static bool IsStream(IGlobalState gs, object obj)
        {
            if (!(obj is DisposableID)) return false;
            DisposableID d = (DisposableID)obj;
            IDisposable dObj = gs.GetDisposableByID(d);
            return (dObj is Stream);
        }

        [SchemeFunction("stream-can-seek?")]
        public static bool IsStreamSeekable(Stream s)
        {
            return s.CanSeek;
        }

        [SchemeFunction("stream-position")]
        public static long StreamPosition(Stream s)
        {
            return s.Position;
        }

        [SchemeFunction("stream-set-position!")]
        public static void StreamSetPosition(Stream s, long position)
        {
            s.Position = position;
        }

        [SchemeFunction("stream-length")]
        public static long StreamLength(Stream s)
        {
            return s.Length;
        }

        [SchemeFunction("stream-set-length!")]
        public static void StreamSetLength(Stream s, long length)
        {
            s.SetLength(length);
        }

        [SchemeFunction("begin-stream-read!")]
        public static SignalID BeginStreamRead(IGlobalState gs, Stream s, ByteRange dest)
        {
            SignalID sid = gs.Scheduler.GetNewSignalID();
            IAsyncResult iar = s.BeginRead(dest.Array.Bytes, dest.Offset, dest.LengthInt32, null, null);
            gs.Scheduler.PostActionOnCompletion
            (
                iar.AsyncWaitHandle,
                delegate()
                {
                    try
                    {
                        int bytesRead = s.EndRead(iar);
                        gs.Scheduler.PostSignal(sid, BigInteger.FromInt32(bytesRead), false);
                    }
                    catch (Exception exc)
                    {
                        gs.Scheduler.PostSignal(sid, exc, true);
                    }
                }
            );
            gs.RegisterSignal(sid, "stream-read", false);
            return sid;
        }

        [SchemeFunction("begin-stream-write!")]
        public static SignalID BeginStreamWrite(IGlobalState gs, Stream s, ByteRange src)
        {
            SignalID sid = gs.Scheduler.GetNewSignalID();
            IAsyncResult iar = s.BeginWrite(src.Array.Bytes, src.Offset, src.LengthInt32, null, null);
            gs.Scheduler.PostActionOnCompletion
            (
                iar.AsyncWaitHandle,
                delegate()
                {
                    try
                    {
                        s.EndWrite(iar);
                        gs.Scheduler.PostSignal(sid, SpecialValue.UNSPECIFIED, false);
                    }
                    catch (Exception exc)
                    {
                        gs.Scheduler.PostSignal(sid, exc, true);
                    }
                }
            );
            gs.RegisterSignal(sid, "stream-write", false);
            return sid;
        }

        public const int BUFSIZE = 1 << 20;
        [SchemeFunction("make-file-stream-reader")]
        public static DisposableID MakeFileStreamReader(IGlobalState gs, string fileName)
        {
            Stream s = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, BUFSIZE, FileOptions.Asynchronous);
            return gs.RegisterDisposable(s, "file-stream-reader \"" + fileName + "\"");
        }

        [SchemeFunction("make-file-stream-writer")]
        public static DisposableID MakeFileStreamWriter(IGlobalState gs, string fileName)
        {
            Stream s = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None, BUFSIZE, FileOptions.Asynchronous);
            return gs.RegisterDisposable(s, "file-stream-writer \"" + fileName + "\"");
        }

        [SchemeFunction("make-file-stream-reader-writer")]
        public static DisposableID MakeFileStreamReaderWriter(IGlobalState gs, string fileName)
        {
            Stream s = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, BUFSIZE, FileOptions.Asynchronous);
            return gs.RegisterDisposable(s, "file-stream-reader-writer \"" + fileName + "\"");
        }

        public class SchemeTextReader : IDisposable
        {
            public SchemeTextReader(Stream stream, StreamReader streamReader)
            {
                this.stream = stream;
                this.streamReader = streamReader;
            }

            private Stream stream;
            private StreamReader streamReader;

            public StreamReader StreamReader { get { return streamReader; } }

            public void Dispose()
            {
                streamReader.Dispose();
                stream.Dispose();
            }
        }

        [SchemeFunction("make-file-text-reader")]
        public static DisposableID MakeFileTextReader(IGlobalState gs, string fileName)
        {
            Stream s = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, BUFSIZE, FileOptions.None);
            StreamReader sr = new StreamReader(s, true);
            SchemeTextReader atr = new SchemeTextReader(s, sr);
            return gs.RegisterDisposable(atr, "file-text-reader");
        }

        [SchemeFunction("text-reader-read-char")]
        public static object TextReaderReadChar(SchemeTextReader atr)
        {
            int i = atr.StreamReader.Read();
            if (i < 0) return SpecialValue.EOF;
            else return (char)i;
        }

        [SchemeFunction("text-reader-read-chars")]
        public static object TextReaderReadChars(SchemeTextReader atr, SchemeString buf, int off, int len)
        {
            int bytesRead = atr.StreamReader.Read(buf.TheCharArray, off, len);
            return BigInteger.FromInt32(bytesRead);
        }

        [SchemeFunction("text-reader-read-line")]
        public static object TextReaderReadLine(SchemeTextReader atr)
        {
            string str = atr.StreamReader.ReadLine();
            if (str == null) return SpecialValue.EOF;
            return new SchemeString(str);
        }

        public class SchemeTextWriter : IDisposable
        {
            public SchemeTextWriter(Stream stream, StreamWriter streamWriter)
            {
                this.stream = stream;
                this.streamWriter = streamWriter;
            }

            private Stream stream;
            private StreamWriter streamWriter;

            public StreamWriter StreamWriter { get { return streamWriter; } }

            public void Dispose()
            {
                streamWriter.Flush();
                streamWriter.Dispose();
                stream.Dispose();
            }
        }

        [SchemeFunction("make-file-text-writer")]
        public static DisposableID MakeFileTextWriter(IGlobalState gs, string fileName)
        {
            Stream s = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None, BUFSIZE, FileOptions.Asynchronous);
            StreamWriter sw = new StreamWriter(s, Encoding.UTF8);
            SchemeTextWriter atw = new SchemeTextWriter(s, sw);
            return gs.RegisterDisposable(atw, "file-text-writer");
        }

        [SchemeFunction("text-writer-write-char")]
        public static void TextWriterWriteChar(SchemeTextWriter w, char ch)
        {
            w.StreamWriter.Write(ch);
        }

        [SchemeFunction("text-writer-write-chars")]
        public static void TextWriterWriteChars(SchemeTextWriter w, SchemeString str, int off, int len)
        {
            w.StreamWriter.Write(str.TheCharArray, off, len);
        }

        [SchemeFunction("text-writer-write-string")]
        public static void TextWriterWriteChar(SchemeTextWriter w, string str)
        {
            w.StreamWriter.Write(str);
        }

        [SchemeFunction("text-writer-write-line")]
        public static void TextWriterWriteLine(SchemeTextWriter w, string str)
        {
            w.StreamWriter.WriteLine(str);
        }

        [SchemeFunction("text-writer-write-newline")]
        public static void TextWriterWriteNewline(SchemeTextWriter w)
        {
            w.StreamWriter.WriteLine();
        }
    }

#if false
    [SchemeIsAFunction("bstream?")]
    public class SchemeBinaryStream : IDisposable
    {
        private Stream stream;
        private bool isHbf;
        private bool isClosed;

        public SchemeBinaryStream(Stream stream)
        {
            this.stream = stream;
            this.isHbf = false;
            this.isClosed = false;
        }

        public bool CanRead
        {
            [SchemeFunction("bstream-can-read?")] get { return stream.CanRead; }
        }

        public bool CanWrite
        {
            [SchemeFunction("bstream-can-write?")] get { return stream.CanWrite; }
        }

        public bool CanSeek
        {
            [SchemeFunction("bstream-can-seek?")] get { return stream.CanSeek; }
        }

        public long Position
        {
            [SchemeFunction("bstream-position-ref")] get { return stream.Position; }
            [SchemeFunction("bstream-position-set!")] set { stream.Position = value; }
        }

        public bool HasLength
        {
            [SchemeFunction("bstream-has-length?")] get { return !stream.CanTimeout; }
        }

        public long Length
        {
            [SchemeFunction("bstream-length-ref")] get { return stream.Length; }
            [SchemeFunction("bstream-length-set!")] set { stream.SetLength(value); }
        }

        public bool HBF
        {
            [SchemeFunction("bstream-is-hbf?")] get { return isHbf; }
            [SchemeFunction("bstream-set-hbf!")] set { isHbf = value; }
        }

        private DigitOrder DigitOrder { get { return isHbf ? DigitOrder.HBLA : DigitOrder.LBLA; } }

        [SchemeFunction("bstream-read-byte!")]
        public object ReadByte()
        {
            int i = stream.ReadByte();
            if (i == -1) return SpecialValue.EOF;
            else return BigInteger.FromByte((byte)i);
        }

        [SchemeFunction("bstream-read-bytes!")]
        public object ReadBytes(int count)
        {
            byte[] b = new byte[count];
            int actuallyRead = stream.Read(b, 0, count);
            if (actuallyRead == 0)
            {
                return SpecialValue.EOF;
            }
            else if (actuallyRead == count)
            {
                return new SchemeByteArray(b, isHbf ? DigitOrder.HBLA : DigitOrder.LBLA);
            }
            else
            {
                byte[] b2 = new byte[actuallyRead];
                Array.Copy(b, b2, actuallyRead);
                return new SchemeByteArray(b2, isHbf ? DigitOrder.HBLA : DigitOrder.LBLA);
            }
        }

        [SchemeFunction("bstream-read-bytes-into-buffer!")]
        public int ReadBytesIntoBuffer(SchemeByteArray buffer, int off, int len)
        {
            return stream.Read(buffer.Bytes, off, len);
        }

        [SchemeFunction("bstream-read-int!")]
        public object ReadInt(int bytes)
        {
            byte[] b = new byte[bytes];
            int actuallyRead = stream.Read(b, 0, bytes);
            if (actuallyRead < bytes) return SpecialValue.EOF;
            BigInteger r = BigInteger.FromByteArray(b, 0, actuallyRead, true, this.DigitOrder);
            return r;
        }

        [SchemeFunction("bstream-read-uint!")]
        public object ReadUInt(int bytes)
        {
            byte[] b = new byte[bytes];
            int actuallyRead = stream.Read(b, 0, bytes);
            if (actuallyRead < bytes) return SpecialValue.EOF;
            BigInteger r = BigInteger.FromByteArray(b, 0, actuallyRead, false, this.DigitOrder);
            return r;
        }

        [SchemeFunction("bstream-read-string-field!")]
        public object ReadString(int length)
        {
            byte[] b = new byte[length];
            int actuallyRead = stream.Read(b, 0, length);
            if (actuallyRead < length) return SpecialValue.EOF;
            return new SchemeString(ProxyDiscovery.ByteRefString(b, 0, length));
        }

        [SchemeFunction("bstream-read-float!")]
        public object ReadFloat()
        {
            byte[] b = new byte[4];
            int actuallyRead = stream.Read(b, 0, 4);
            if (actuallyRead < 4) return SpecialValue.EOF;
            return (double)ProxyDiscovery.ByteRefFloat(b, 0, isHbf);
        }

        [SchemeFunction("bstream-read-double!")]
        public object ReadDouble()
        {
            byte[] b = new byte[8];
            int actuallyRead = stream.Read(b, 0, 8);
            if (actuallyRead < 8) return SpecialValue.EOF;
            return ProxyDiscovery.ByteRefDouble(b, 0, isHbf);
        }

        [SchemeFunction("bstream-write-byte!")]
        public void WriteByte(byte b)
        {
            stream.WriteByte(b);
        }

        [SchemeFunction("bstream-write-bytes!")]
        public void WriteBytes(byte[] b)
        {
            stream.Write(b, 0, b.Length);
        }

        [SchemeFunction("bstream-write-bytes-from-buffer!")]
        public void WriteBytesFromBuffer(byte[] buffer, int off, int len)
        {
            stream.Write(buffer, off, len);
        }

        [SchemeFunction("bstream-write-int!")]
        public void WriteInt(BigInteger b, int bytes)
        {
            byte[] theByteArray = new byte[bytes];
            b.WriteBytesToArray(theByteArray, 0, bytes, false, OverflowBehavior.Wraparound, isHbf ? DigitOrder.HBLA : DigitOrder.LBLA);
            stream.Write(theByteArray, 0, bytes);
        }

        [SchemeFunction("bstream-write-string-field!")]
        public void WriteString(string str, int length)
        {
            byte[] theByteArray = new byte[length];
            ProxyDiscovery.ByteSetString(theByteArray, 0, length, str);
            stream.Write(theByteArray, 0, length);
        }

        [SchemeFunction("bstream-write-float!")]
        public void WriteFloat(float f)
        {
            byte[] theByteArray = new byte[4];
            ProxyDiscovery.ByteSetFloat(theByteArray, 0, isHbf, f);
            stream.Write(theByteArray, 0, 4);
        }

        [SchemeFunction("bstream-write-double!")]
        public void WriteDouble(double d)
        {
            byte[] theByteArray = new byte[8];
            ProxyDiscovery.ByteSetDouble(theByteArray, 0, isHbf, d);
            stream.Write(theByteArray, 0, 8);
        }

        [SchemeFunction("bstream-close!")]
        public void Close()
        {
            stream.Close();
            isClosed = true;
        }

        public bool IsClosed { [SchemeFunction("bstream-is-closed?")] get { return isClosed; } }

        #region IDisposable Members

        public void Dispose()
        {
            stream.Dispose();
        }

        #endregion
    }
#endif
}