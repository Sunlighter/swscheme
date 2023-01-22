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

namespace ExprObjModel
{
    [Serializable]
    public class SchemeString : BigMath.IHashable
    {
        private enum Fmt
        {
            String,
            CharArray
        };

        private Fmt fmt;
        private string str;
        private char[] charArray;

        public SchemeString(string s)
        {
            fmt = Fmt.String;
            str = s;
            charArray = null;
        }

        public SchemeString(char[] ch)
        {
            fmt = Fmt.CharArray;
            str = null;
            charArray = ch;
        }

        private void ChangeFmt(Fmt newFmt)
        {
            if (fmt == newFmt) return;
            if (newFmt == Fmt.String)
            {
                fmt = newFmt;
                str = new string(charArray);
                charArray = null;
            }
            else
            {
                System.Diagnostics.Debug.Assert(newFmt == Fmt.CharArray);
                fmt = newFmt;
                charArray = str.ToCharArray();
                str = null;
            }
        }

        public override string ToString()
        {
            ChangeFmt(Fmt.String);
            return str;
        }

        public string TheString { get { ChangeFmt(Fmt.String); return str; } }

        public char[] TheCharArray { get { ChangeFmt(Fmt.CharArray); return charArray; } }

        public bool IsCharArray { get { return fmt == Fmt.CharArray; } }

        public int Length { get { if (fmt == Fmt.CharArray) return charArray.Length; else return str.Length; } }

        public char this[int i]
        {
            get
            {
                if (fmt == Fmt.CharArray) return charArray[i]; else return str[i];
            }
            set
            {
                ChangeFmt(Fmt.CharArray);
                charArray[i] = value;
            }
        }

        public void AddToHash(BigMath.IHashGenerator hg)
        {
            if (fmt == Fmt.CharArray) hg.Add(charArray); else hg.Add(str);    
        }

        public static bool operator < (SchemeString a, SchemeString b)
        {
            return string.Compare(a.TheString, b.TheString, false) < 0;
        }

        public static bool operator > (SchemeString a, SchemeString b)
        {
            return string.Compare(a.TheString, b.TheString, false) > 0;
        }

        public static bool operator <= (SchemeString a, SchemeString b)
        {
            return string.Compare(a.TheString, b.TheString, false) <= 0;
        }

        public static bool operator >= (SchemeString a, SchemeString b)
        {
            return string.Compare(a.TheString, b.TheString, false) >= 0;
        }

        public static bool operator == (SchemeString a, SchemeString b)
        {
            if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null)) return true;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null)) return false;
            return string.Compare(a.TheString, b.TheString, false) == 0;
        }

        public static bool operator != (SchemeString a, SchemeString b)
        {
            if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null)) return false;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null)) return true;
            return string.Compare(a.TheString, b.TheString, false) != 0;
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(obj, null)) return false;
            if (!(obj is SchemeString)) return false;
            return this == (SchemeString)obj;
        }

        public override int GetHashCode()
        {
            BigMath.HashGenerator h = new BigMath.HashGenerator();
            AddToHash(h);
            return unchecked((int)(h.Hash));
        }
    }
}
