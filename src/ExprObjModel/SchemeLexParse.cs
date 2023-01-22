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
using System.Text.RegularExpressions;
using ExprObjModel.Lexing;
using BigMath;
using ControlledWindowLib;

namespace ExprObjModel
{
    public enum LexemeType
    {
        Whitespace, LeftParen, PoundLeftParen, PoundSLeftParen, PoundMLeftParen, RightParen, Dot,
        PoundSigLeftParen, PoundMsgLeftParen, PoundVector3LeftParen, PoundVertex3LeftParen,
        PoundVector2LeftParen, PoundVertex2LeftParen, PoundQuatLeftParen,
        Quote, QuasiQuote, Unquote, UnquoteSplicing,
        BeginString, CharEscape, HexEscape, OctEscape, UnicodeEscape, StrChars, EndString,
        BeginSymbol, EndSymbol,
        IPV4EndPoint, IPV4Address, IPV6EndPoint, IPV6Address, Guid,
        Symbol, Integer, HexInteger, OctalInteger, Double, Char, HexChar, BooleanTrue, BooleanFalse,
        Numerator, FractionBar, Denominator,
        BeginComment, CommentChars, EndComment,
        EndOfInput, LexicalError,
    }

    public struct ScanResult
    {
        public LexemeType type;
        public string str;
    }

    public class SchemeScanner
    {
        static SchemeScanner()
        {
            modes = new IMatcher<LexemeType>[5];

            CompoundMatcherFactory<LexemeType> cmf = new CompoundMatcherFactory<LexemeType>();
            cmf.AddRegex(@"\G\s+", LexemeType.Whitespace);
            cmf.AddString("(", LexemeType.LeftParen);
            cmf.AddString("#(", LexemeType.PoundLeftParen);
            cmf.AddString("#s(", LexemeType.PoundSLeftParen);
            cmf.AddString("#m(", LexemeType.PoundMLeftParen);
            cmf.AddString("#sig(", LexemeType.PoundSigLeftParen);
            cmf.AddString("#msg(", LexemeType.PoundMsgLeftParen);
            cmf.AddString("#vec3(", LexemeType.PoundVector3LeftParen);
            cmf.AddString("#vtx3(", LexemeType.PoundVertex3LeftParen);
            cmf.AddString("#vec2(", LexemeType.PoundVector2LeftParen);
            cmf.AddString("#vtx2(", LexemeType.PoundVertex2LeftParen);
            cmf.AddString("#quat(", LexemeType.PoundQuatLeftParen);
            cmf.AddString(")", LexemeType.RightParen);
            cmf.AddString(".", LexemeType.Dot);
            cmf.AddString("'", LexemeType.Quote);
            cmf.AddString("`", LexemeType.QuasiQuote);
            cmf.AddString(",", LexemeType.Unquote);
            cmf.AddString(",@", LexemeType.UnquoteSplicing);
            cmf.AddString("\"", LexemeType.BeginString);
            cmf.AddString("|", LexemeType.BeginSymbol);
            cmf.AddString(";", LexemeType.BeginComment);
            cmf.AddString("#t", LexemeType.BooleanTrue);
            cmf.AddString("#f", LexemeType.BooleanFalse);
            cmf.AddRegex(@"\G#g\{([0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12})\}", LexemeType.Guid);
            cmf.AddRegex(@"\G(\+|-)?[0-9]+(?=/)", LexemeType.Numerator);
            cmf.AddRegex(@"\G(\+|-)?(([0-9]+[Ee](\+|-)?[0-9]+)|([0-9]+\.[0-9]*)([Ee](\+|-)?[0-9]+)?|(([0-9]*\.[0-9]+)([Ee](\+|-)?[0-9]+)?))", LexemeType.Double);
            cmf.AddRegex(@"\G(\+|-)?[0-9]+", LexemeType.Integer);
            cmf.AddRegex(@"\G(\+|-|~)?#x[0-9A-Fa-f]+", LexemeType.HexInteger);
            cmf.AddRegex(@"\G#\\x[0-9A-Fa-f]{4}", LexemeType.HexChar);
            cmf.AddRegex(@"\G#\\[A-Za-z]+", LexemeType.Char);
            cmf.AddRegex(@"\G#\\[!-~]", LexemeType.Char);
            cmf.AddRegex(@"\G[A-Za-z!$%&*+./:<=>?@^_~][A-Za-z0-9!$%&*+\-./:<=>?@^_~]*", LexemeType.Symbol);
            cmf.AddRegex(@"\G-(?:[A-Za-z!$%&*+./:<=>?@^_~][A-Za-z0-9!$%&*+\-./:<=>?@^_~]*)?", LexemeType.Symbol);
            cmf.AddRegex(@"\G#ipv4\[[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+\]:[0-9]+", LexemeType.IPV4EndPoint);
            cmf.AddRegex(@"\G#ipv4\[[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+\]", LexemeType.IPV4Address);
            cmf.AddRegex(@"\G#ipv6\[([0-9A-Fa-f:]*)\]:[0-9]+", LexemeType.IPV6EndPoint);
            cmf.AddRegex(@"\G#ipv6\[([0-9A-Fa-f:]*)\]", LexemeType.IPV6Address);
            modes[0] = cmf.GetMatcher();

            // ----- in string -----
            cmf.Clear();
            cmf.AddRegex(@"\\[abtnvfre\\\""]", LexemeType.CharEscape);
            cmf.AddRegex(@"\\x[0-9A-Fa-f]{2}", LexemeType.HexEscape);
            cmf.AddRegex(@"\\[0-7]{3}", LexemeType.OctEscape);
            cmf.AddRegex(@"\\u[0-9A-Fa-f]{4}", LexemeType.UnicodeEscape);
            cmf.AddRegex(@"[ !#-\[\]-~]+", LexemeType.StrChars);
            cmf.AddString("\"", LexemeType.EndString);
            modes[1] = cmf.GetMatcher();

            // ----- in symbol -----
            cmf.Clear();
            cmf.AddRegex(@"\\[abtnvfre\\\|]", LexemeType.CharEscape);
            cmf.AddRegex(@"\\x[0-9A-Fa-f]{2}", LexemeType.HexEscape);
            cmf.AddRegex(@"\\[0-7]{3}", LexemeType.OctEscape);
            cmf.AddRegex(@"\\u[0-9A-Fa-f]{4}", LexemeType.UnicodeEscape);
            cmf.AddRegex(@"[ !-\[\]-{}-~]+", LexemeType.StrChars);
            cmf.AddString("|", LexemeType.EndSymbol);
            modes[2] = cmf.GetMatcher();

            // ----- in comment -----
            cmf.Clear();
            cmf.AddRegex(@"[ -~]+", LexemeType.CommentChars);
            cmf.AddRegex(@"\n", LexemeType.EndComment);
            modes[3] = cmf.GetMatcher();

            // ----- in fraction -----
            cmf.Clear();
            cmf.AddRegex(@"/", LexemeType.FractionBar);
            cmf.AddRegex(@"\G-?(0|[1-9][0-9]*)", LexemeType.Denominator);
            modes[4] = cmf.GetMatcher();
        }

        public SchemeScanner()
        {
            mode = 0;
        }

        private static int NextMode(int mode, LexemeType l)
        {
            switch(l)
            {
                case LexemeType.BeginString: return 1;
                case LexemeType.BeginSymbol: return 2;
                case LexemeType.BeginComment: return 3;
                case LexemeType.EndString: return 0;
                case LexemeType.EndSymbol: return 0;
                case LexemeType.EndComment: return 0;
                case LexemeType.Numerator: return 4;
                case LexemeType.Denominator: return 0;
                default: return mode;
            }
        }

        private static IMatcher<LexemeType>[] modes;
        private int mode;

        public void ResetMode() { mode = 0; }

        public void Scan(string str, int pos, out ScanResult sr, out int newPos)
        {
            if (pos == str.Length)
            {
                sr.type = LexemeType.EndOfInput;
                sr.str = "";
                newPos = pos;
            }
            else
            {
                IMatcher<LexemeType> i = modes[mode];
                bool result = i.Matches(str, pos);
                if (result)
                {
                    sr.type = (LexemeType)i.AcceptCode;
                    sr.str = i.Match;
                    newPos = pos + i.MatchLength;
                    mode = NextMode(mode, sr.type);
                }
                else
                {
                    sr.type = LexemeType.LexicalError;
                    sr.str = str.Substring(pos, 1);
                    newPos = pos + 1;
                }
            }
        }
    }

    public interface IStringSource
    {
        bool Next(int parenDepth);
        string Current { get; }
    }

    public class SingleString : IStringSource
    {
        public SingleString(string s)
        {
            this.s = s;
            this.pos = 0;
        }

        private string s;
        private int pos;

        public bool Next(int parenDepth)
        {
            ++pos;
            return (pos == 1);
        }

        public string Current { get { return s; } }
    }

    public class StringArraySource : IStringSource
    {
        public StringArraySource(string[] sa)
        {
            this.sa = sa;
            pos = -1;
        }

        private string[] sa;
        private int pos;

        public bool Next(int parenDepth)
        {
            ++pos;
            return (pos >= 0 && pos < sa.Length);
        }

        public string Current { get { return sa[pos]; } }
    }

    public class LexemeSource
    {
        private int parenDepth;

        public LexemeSource(IStringSource ss)
        {
            sc = new SchemeScanner();
            this.ss = ss;
            parenDepth = 0;
            NextString();
        }

        private void NextString()
        {
            bool b = ss.Next(parenDepth);
            if (b)
            {
                s = ss.Current + "\n";
                endOfStrings = false;
                pos = 0;
            }
            else
            {
                endOfStrings = true;
            }
        }

        private SchemeScanner sc;
        private IStringSource ss;
        private bool endOfStrings;
        private string s;
        private int pos;
        private ScanResult sr;

        public static bool IsLParen(LexemeType lt)
        {
            LexemeType[] lts = new LexemeType[]
            {
                LexemeType.LeftParen,
                LexemeType.PoundLeftParen,
                LexemeType.PoundMLeftParen,
                LexemeType.PoundSLeftParen,
                LexemeType.PoundMsgLeftParen,
                LexemeType.PoundSigLeftParen,
                LexemeType.PoundVector3LeftParen,
                LexemeType.PoundVertex3LeftParen,
                LexemeType.PoundVector2LeftParen,
                LexemeType.PoundQuatLeftParen,
            };

            return lts.Any(x => x == lt);
        }

        public bool Next()
        {
            if (endOfStrings)
            {
                sr.type = LexemeType.EndOfInput;
                sr.str = "";
                return false;
            }

            int newPos;
            sc.Scan(s, pos, out sr, out newPos);

            if (IsLParen(sr.type)) ++parenDepth;
            if (sr.type == LexemeType.RightParen)
            {
                --parenDepth;
                if (parenDepth < 0) parenDepth = 0;
            }

            if (newPos == pos)
            {
                NextString();
                return Next();
            }
            else
            {
                pos = newPos;
                return true;
            }
        }

        public ScanResult Current { get { return sr; } }
        public LexemeType CurrentType { get { return sr.type; } }
        public string CurrentString { get { return sr.str; } }
    }

    public class ParsingException : ApplicationException
    {
        public ParsingException(): base() { }
        public ParsingException(string message): base(message) { }
        public ParsingException(string message, Exception cause): base(message, cause) { }
    }

    public class SchemeDataReader
    {
        public SchemeDataReader(LexemeSource ls)
        {
            this.ls = ls;
        }

        private LexemeSource ls;

        private static bool IsImportant(LexemeType t)
        {
            switch(t)
            {
                case LexemeType.Whitespace: return false;
                case LexemeType.BeginComment: return false;
                case LexemeType.CommentChars: return false;
                case LexemeType.EndComment: return false;
                default: return true;
            }
        }

        private static bool IsStartOfSchemeItem(LexemeType t)
        {
            switch(t)
            {
                case LexemeType.LeftParen: return true;
                case LexemeType.PoundLeftParen: return true;
                case LexemeType.PoundSLeftParen: return true;
                case LexemeType.PoundMLeftParen: return true;
                case LexemeType.PoundSigLeftParen: return true;
                case LexemeType.PoundMsgLeftParen: return true;
                case LexemeType.PoundVector3LeftParen: return true;
                case LexemeType.PoundVertex3LeftParen: return true;
                case LexemeType.PoundVector2LeftParen: return true;
                case LexemeType.PoundQuatLeftParen: return true;
                case LexemeType.Quote: return true;
                case LexemeType.QuasiQuote: return true;
                case LexemeType.Unquote: return true;
                case LexemeType.UnquoteSplicing: return true;
                case LexemeType.BeginString: return true;
                case LexemeType.BeginSymbol: return true;
                case LexemeType.Symbol: return true;
                case LexemeType.Integer: return true;
                case LexemeType.Char: return true;
                case LexemeType.HexChar: return true;
                case LexemeType.BooleanTrue: return true;
                case LexemeType.BooleanFalse: return true;
                case LexemeType.Numerator: return true;
                case LexemeType.IPV4Address: return true;
                case LexemeType.IPV4EndPoint: return true;
                case LexemeType.IPV6Address: return true;
                case LexemeType.IPV6EndPoint: return true;
                case LexemeType.Guid: return true;
                default: return false;
            }
        }

        /*
        It is assumed that, at the start of each ReadSomething function, ls points
        to the FIRST lexeme of the Something.

        Therefore, for a list or vector, it points to the ( or #(.

        At the end of each ReadSomething function, ls points to the FIRST lexeme
        AFTER the Something. This could be End Of Input.
        */

        private void ReadUnimportant()
        {
            while (!IsImportant(ls.CurrentType))
            {
                ls.Next();
            }
        }

        private char ReadCharEscape()
        {
            System.Diagnostics.Debug.Assert(ls.CurrentType == LexemeType.CharEscape);
            string s = ls.CurrentString;
            char r;
            switch(s[1])
            {
                case '\\': r = '\\'; break;
                case '"': r = '"'; break;
                case '|': r = '|'; break;
                case 'a': r = '\a'; break;
                case 'b': r = '\b'; break;
                case 't': r = '\t'; break;
                case 'n': r = '\n'; break;
                case 'v': r = '\v'; break;
                case 'f': r = '\f'; break;
                case 'r': r = '\r'; break;
                case 'e': r = '\x1b'; break;
                default: System.Diagnostics.Debug.Assert(false); r = s[1]; break;
            }
            ls.Next();
            return r;
        }

        private static int NumericValue(char c)
        {
            if (c >= '0' && c <= '9') return (int)(c - '0');
            if (c >= 'A' && c <= 'Z') return (int)(c - 'A' + 10);
            if (c >= 'a' && c <= 'z') return (int)(c - 'a' + 10);
            System.Diagnostics.Debug.Assert(false);
            return 0;
        }

        private char ReadHexEscape()
        {
            System.Diagnostics.Debug.Assert(ls.CurrentType == LexemeType.HexEscape);
            string s = ls.CurrentString;
            int v = NumericValue(s[2]) * 16 + NumericValue(s[3]);
            ls.Next();
            return (char)v;
        }

        private char ReadUnicodeEscape()
        {
            System.Diagnostics.Debug.Assert(ls.CurrentType == LexemeType.UnicodeEscape);
            string s = ls.CurrentString;
            int v = NumericValue(s[2]) * 4096 + NumericValue(s[3]) * 256 + NumericValue(s[4]) * 16 + NumericValue(s[5]);
            ls.Next();
            return (char)v;
        }

        private char ReadOctEscape()
        {
            System.Diagnostics.Debug.Assert(ls.CurrentType == LexemeType.OctEscape);
            string s = ls.CurrentString;
            int v = NumericValue(s[1]) * 64 + NumericValue(s[2]) * 8 + NumericValue(s[3]);
            v &= 0xFF;
            ls.Next();
            return (char)v;
        }

        private double ReadDouble()
        {
            System.Diagnostics.Debug.Assert(ls.CurrentType == LexemeType.Double);
            double d = double.Parse(ls.CurrentString);
            ls.Next();
            return d;
        }

        private SchemeString ReadString()
        {
            System.Diagnostics.Debug.Assert(ls.CurrentType == LexemeType.BeginString);
            ls.Next(); // drop BeginString
            StringBuilder sb = new StringBuilder();
            bool building = true;
            while (building)
            {
                switch(ls.CurrentType)
                {
                    case LexemeType.CharEscape:
                        sb.Append(ReadCharEscape());
                        break;
                    case LexemeType.HexEscape:
                        sb.Append(ReadHexEscape());
                        break;
                    case LexemeType.OctEscape:
                        sb.Append(ReadOctEscape());
                        break;
                    case LexemeType.UnicodeEscape:
                        sb.Append(ReadUnicodeEscape());
                        break;
                    case LexemeType.StrChars:
                        sb.Append(ls.CurrentString);
                        ls.Next();
                        break;
                    case LexemeType.EndString:
                        building = false;
                        break;
                    default:
                        throw new ParsingException("ReadString: Error parsing string");
                }
            }
            ls.Next(); // drop EndString
            return new SchemeString(sb.ToString());
        }

        private Symbol ReadEscapedSymbol()
        {
            System.Diagnostics.Debug.Assert(ls.CurrentType == LexemeType.BeginSymbol);
            ls.Next(); // drop BeginSymbol
            StringBuilder sb = new StringBuilder();
            bool building = true;
            while (building)
            {
                switch(ls.CurrentType)
                {
                case LexemeType.CharEscape:
                    sb.Append(ReadCharEscape());
                    break;
                case LexemeType.HexEscape:
                    sb.Append(ReadHexEscape());
                    break;
                case LexemeType.OctEscape:
                    sb.Append(ReadOctEscape());
                    break;
                case LexemeType.UnicodeEscape:
                    sb.Append(ReadUnicodeEscape());
                    break;
                case LexemeType.StrChars:
                    sb.Append(ls.CurrentString);
                    ls.Next();
                    break;
                case LexemeType.EndSymbol:
                    building = false;
                    break;
                default:
                    throw new ParsingException("ReadEscapedSymbol: Error parsing escaped symbol");
                }
            }
            ls.Next(); // drop EndString
            Symbol s = new Symbol(sb.ToString());
            return s;
        }

        private BigInteger ReadInteger()
        {
            System.Diagnostics.Debug.Assert(ls.CurrentType == LexemeType.Integer);
            BigInteger b = BigInteger.Parse(ls.CurrentString, 10u);
            ls.Next();
            return b;
        }

        private static Regex hexIntegerRegex = new Regex(@"\G((?:\+|-|~)?)#x([0-9A-Fa-f]+)", RegexOptions.Compiled);

        private BigInteger ReadHexInteger()
        {
            System.Diagnostics.Debug.Assert(ls.CurrentType == LexemeType.HexInteger);
            Match m = hexIntegerRegex.Match(ls.CurrentString);
            string sign = m.Groups[1].Value;
            string digits = m.Groups[2].Value;
            BigInteger b;
            if (sign == "-")
            {
                b = -BigInteger.Parse(digits, 16u);
            }
            else if (sign == "~")
            {
                b = ~BigInteger.Parse(digits, 16u);
            }
            else
            {
                System.Diagnostics.Debug.Assert(sign == "+" || sign == "");
                b = BigInteger.Parse(digits, 16u);
            }
            ls.Next();
            return b;
        }

        private object ReadRational()
        {
            System.Diagnostics.Debug.Assert(ls.CurrentType == LexemeType.Numerator);
            BigInteger n = BigInteger.Parse(ls.CurrentString, 10u);
            ls.Next();
            if (ls.CurrentType != LexemeType.FractionBar)
                throw new ParsingException("Fraction Bar Expected after Numerator");
            ls.Next();
            if (ls.CurrentType != LexemeType.Denominator)
                throw new ParsingException("Denominator Expected after Fraction Bar");
            BigInteger d = BigInteger.Parse(ls.CurrentString, 10u);
            ls.Next();
            if (d == BigInteger.One) return n;
            return new BigRational(n, d);
        }

        private char ReadCharacter()
        {
            System.Diagnostics.Debug.Assert(ls.CurrentType == LexemeType.Char);
            string z = ls.CurrentString;
            z = z.Substring(2, z.Length-2);
            char ch;
            if (z.Length == 1) ch = z[0];
            else if (string.Compare(z, "nul", true) == 0) ch = (char)0;
            else if (string.Compare(z, "bel", true) == 0) ch = '\a';
            else if (string.Compare(z, "backspace", true) == 0) ch = '\b';
            else if (string.Compare(z, "tab", true) == 0) ch = '\t';
            else if (string.Compare(z, "newline", true) == 0) ch = '\n';
            else if (string.Compare(z, "vt", true) == 0) ch = '\v';
            else if (string.Compare(z, "page", true) == 0) ch = '\f';
            else if (string.Compare(z, "return", true) == 0) ch = '\r';
            else if (string.Compare(z, "space", true) == 0) ch = ' ';
            else throw new ParsingException("Unknown character "+z);
            ls.Next();
            return ch;
        }

        private char ReadHexChar()
        {
            System.Diagnostics.Debug.Assert(ls.CurrentType == LexemeType.HexChar);
            string z = ls.CurrentString;
            int v = NumericValue(z[3]) * 4096 + NumericValue(z[4]) * 256 + NumericValue(z[5]) * 16 + NumericValue(z[6]);
            ls.Next();
            return (char)v;
        }

        private static Regex guidRegex = new Regex(@"\G#g\{([0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12})\}", RegexOptions.Compiled);

        private Guid ReadGuid()
        {
            System.Diagnostics.Debug.Assert(ls.CurrentType == LexemeType.Guid);
            string z = ls.CurrentString;
            Match m = guidRegex.Match(z);
            System.Diagnostics.Debug.Assert(m.Success);
            ls.Next();
            return new Guid(m.Groups[1].Value);
        }

        private object ReadIPV4Address()
        {
            System.Diagnostics.Debug.Assert(ls.CurrentType == LexemeType.IPV4Address);
            Regex r = new Regex("#ipv4\\[(.*)\\]", RegexOptions.None);
            Match m = r.Match(ls.CurrentString);
            System.Diagnostics.Debug.Assert(m.Success);
            try
            {
                System.Net.IPAddress ipAddr = System.Net.IPAddress.Parse(m.Groups[1].Value);
                return ipAddr;
            }
            finally
            {
                ls.Next();
            }
        }

        private object ReadIPV4EndPoint()
        {
            System.Diagnostics.Debug.Assert(ls.CurrentType == LexemeType.IPV4EndPoint);
            Regex r = new Regex("#ipv4\\[(.*)\\]:([0-9]+)", RegexOptions.None);
            Match m = r.Match(ls.CurrentString);
            System.Diagnostics.Debug.Assert(m.Success);
            try
            {
                System.Net.IPAddress ipAddr = System.Net.IPAddress.Parse(m.Groups[1].Value);
                int port = int.Parse(m.Groups[2].Value);
                return new System.Net.IPEndPoint(ipAddr, port);
            }
            finally
            {
                ls.Next();
            }
        }

        private object ReadIPV6Address()
        {
            System.Diagnostics.Debug.Assert(ls.CurrentType == LexemeType.IPV6Address);
            Regex r = new Regex("#ipv6\\[(.*)\\]", RegexOptions.None);
            Match m = r.Match(ls.CurrentString);
            System.Diagnostics.Debug.Assert(m.Success);
            try
            {
                System.Net.IPAddress ipAddr = System.Net.IPAddress.Parse(m.Groups[1].Value);
                return ipAddr;
            }
            finally
            {
                ls.Next();
            }
        }

        private object ReadIPV6EndPoint()
        {
            System.Diagnostics.Debug.Assert(ls.CurrentType == LexemeType.IPV6EndPoint);
            Regex r = new Regex("#ipv6\\[(.*)\\]:([0-9]+)", RegexOptions.None);
            Match m = r.Match(ls.CurrentString);
            System.Diagnostics.Debug.Assert(m.Success);
            try
            {
                System.Net.IPAddress ipAddr = System.Net.IPAddress.Parse(m.Groups[1].Value);
                int port = int.Parse(m.Groups[2].Value);
                return new System.Net.IPEndPoint(ipAddr, port);
            }
            finally
            {
                ls.Next();
            }
        }

        private ExprObjModel.ObjectSystem.Signature ReadSignature()
        {
            System.Diagnostics.Debug.Assert(ls.CurrentType == LexemeType.PoundSigLeftParen);
            ls.Next();
            ReadUnimportant();
            Symbol type = null;
            if (ls.CurrentType == LexemeType.Symbol)
            {
                type = ReadSymbol();
            }
            else if (ls.CurrentType == LexemeType.BeginSymbol)
            {
                type = ReadEscapedSymbol();
            }
            else
            {
                throw new ParsingException("ReadSignature: Error reading type, expected symbol, got " + ls.CurrentType);
            }
            ReadUnimportant();
            if (ls.CurrentType == LexemeType.RightParen)
            {
                ls.Next();
                return new ExprObjModel.ObjectSystem.Signature(type, Enumerable.Empty<Symbol>());
            }
            else if (ls.CurrentType != LexemeType.Dot)
            {
                throw new ParsingException("ReadSignature: Expected Dot or RightParen, got " + ls.CurrentType);
            }
            ls.Next();
            ReadUnimportant();
            List<Symbol> ps = new List<Symbol>();
            while (true)
            {
                if (ls.CurrentType == LexemeType.Symbol)
                {
                    Symbol p = ReadSymbol();
                    ps.Add(p);
                }
                else if (ls.CurrentType == LexemeType.BeginSymbol)
                {
                    Symbol p = ReadEscapedSymbol();
                    ps.Add(p);
                }
                else if (ls.CurrentType == LexemeType.RightParen)
                {
                    ls.Next();
                    break;
                }
                else
                {
                    throw new ParsingException("ReadSignature: Error reading type, expected Symbol or RightParen, got " + ls.CurrentType);
                }
                ReadUnimportant();
            }
            if (ps.Count == 0) throw new ParsingException("ReadSignature: Dot without parameters");
            return new ExprObjModel.ObjectSystem.Signature(type, ps);
        }

        private ExprObjModel.ObjectSystem.Message<object> ReadMessage()
        {
            System.Diagnostics.Debug.Assert(ls.CurrentType == LexemeType.PoundMsgLeftParen);
            ls.Next();
            ReadUnimportant();
            Symbol type = null;
            if (ls.CurrentType == LexemeType.Symbol)
            {
                type = ReadSymbol();
            }
            else if (ls.CurrentType == LexemeType.BeginSymbol)
            {
                type = ReadEscapedSymbol();
            }
            else
            {
                throw new ParsingException("ReadMessage : Error reading type, expected symbol, got " + ls.CurrentType);
            }
            ReadUnimportant();
            if (ls.CurrentType == LexemeType.RightParen)
            {
                ls.Next();
                return new ExprObjModel.ObjectSystem.Message<object>(type, Enumerable.Empty<Tuple<Symbol, object>>());
            }
            else if (ls.CurrentType != LexemeType.Dot)
            {
                throw new ParsingException("ReadMessage: Expected Dot or RightParen, got " + ls.CurrentType);
            }
            ls.Next();
            ReadUnimportant();
            List<Tuple<Symbol, object>> args = new List<Tuple<Symbol, object>>();
            while (true)
            {
                Symbol p = null;
                if (ls.CurrentType == LexemeType.Symbol)
                {
                    p = ReadSymbol();
                }
                else if (ls.CurrentType == LexemeType.BeginSymbol)
                {
                    p = ReadEscapedSymbol();
                }
                else if (ls.CurrentType == LexemeType.RightParen)
                {
                    ls.Next();
                    break;
                }
                else
                {
                    throw new ParsingException("ReadMessage: Error reading key, expected symbol, got " + ls.CurrentType);
                }
                ReadUnimportant();
                if (ls.CurrentType == LexemeType.RightParen)
                {
                    throw new ParsingException("ReadMessage: Error reading value, got RightParen");
                }
                object val = ReadItem();
                ReadUnimportant();
                args.Add(new Tuple<Symbol, object>(p, val));
            }
            if (args.Count == 0) throw new ParsingException("ReadMessage: Dot without arguments");
            return new ExprObjModel.ObjectSystem.Message<object>(type, args);
        }

        private object ReadList()
        {
            System.Diagnostics.Debug.Assert(ls.CurrentType == LexemeType.LeftParen);
            ConsCell beginList = new ConsCell("dummy", SpecialValue.EMPTY_LIST);
            ConsCell endList = beginList;
            ls.Next(); // drop LeftParen
            while(true)
            {
                ReadUnimportant();
                if (ls.CurrentType == LexemeType.EndOfInput)
                {
                    throw new ParsingException("ReadList: Unexpected end of input");
                }
                else if (ls.CurrentType == LexemeType.RightParen)
                {
                    ls.Next(); // drop RightParen
                    return beginList.cdr;
                }
                else if (ls.CurrentType == LexemeType.Dot)
                {
                    ls.Next(); // drop Dot
                    endList.cdr = ReadItem();
                    ReadUnimportant();
                    if (ls.CurrentType != LexemeType.RightParen)
                    {
                        throw new ParsingException("ReadList: Improperly dotted list");
                    }
                    ls.Next(); // drop RightParen
                    return beginList.cdr;
                }
                else
                {
                    ConsCell k = new ConsCell();
                    k.car = ReadItem();
                    k.cdr = SpecialValue.EMPTY_LIST;
                    endList.cdr = k;
                    endList = k;
                }
            }
        }

        private object ReadVector()
        {
            System.Diagnostics.Debug.Assert(ls.CurrentType == LexemeType.PoundLeftParen);
            Deque<object> d = new Deque<object>();
            ls.Next(); // drop PoundLeftParen
            while(true)
            {
                ReadUnimportant();
                if (ls.CurrentType == LexemeType.EndOfInput)
                {
                    throw new ParsingException("ReadVector: Unexpected end of input");
                }
                else if (ls.CurrentType == LexemeType.RightParen)
                {
                    ls.Next(); // drop RightParen
                    return d;
                }
                else
                {
                    ReadUnimportant();
                    d.PushBack(ReadItem());
                }
            }
        }

        private object ReadHashSet()
        {
            System.Diagnostics.Debug.Assert(ls.CurrentType == LexemeType.PoundSLeftParen);
            ls.Next();
            SchemeHashSet hs = new SchemeHashSet();
            while (true)
            {
                ReadUnimportant();
                if (ls.CurrentType == LexemeType.EndOfInput)
                {
                    throw new ParsingException("ReadHashSet: Unexpected end of input");
                }
                else if (ls.CurrentType == LexemeType.RightParen)
                {
                    ls.Next();
                    return hs;
                }
                else
                {
                    object r = ReadItem();
                    if (!(Procedures.ProxyDiscovery.IsHashable(r))) throw new ParsingException("ReadHashSet: Un-hashable item in set");
                    hs.Add(r);
                }
            }
        }

        private object ReadHashMap()
        {
            System.Diagnostics.Debug.Assert(ls.CurrentType == LexemeType.PoundMLeftParen);
            ls.Next();
            SchemeHashMap hm = new SchemeHashMap();
            while (true)
            {
                ReadUnimportant();
                if (ls.CurrentType == LexemeType.EndOfInput)
                {
                    throw new ParsingException("ReadHashMap: Unexpected end of input");
                }
                else if (ls.CurrentType == LexemeType.RightParen)
                {
                    ls.Next();
                    return hm;
                }
                else
                {
                    object r = ReadItem();
                    if (!(r is ConsCell)) throw new ParsingException("ReadHashMap: items must be pairs");
                    ConsCell ccr = (ConsCell)r;
                    if (!(Procedures.ProxyDiscovery.IsHashable(ccr.car))) throw new ParsingException("ReadHashMap: Un-hashable key in map");
                    hm[ccr.car] = ccr.cdr;
                }
            }
        }

        private BigRational ReadVec3Part()
        {
            ReadUnimportant(); 
            if (ls.CurrentType == LexemeType.Integer)
            {
                return new BigRational((BigInteger)ReadInteger(), BigInteger.One);
            }
            else if (ls.CurrentType == LexemeType.HexInteger)
            {
                return new BigRational((BigInteger)ReadHexInteger(), BigInteger.One);
            }
            else if (ls.CurrentType == LexemeType.Numerator)
            {
                return (BigRational)ReadRational();
            }
            else
            {
                throw new ParsingException("Vec3 component must be integer or rational");
            }
        }

        private object ReadVector2()
        {
            System.Diagnostics.Debug.Assert(ls.CurrentType == LexemeType.PoundVector2LeftParen);
            ls.Next();
            BigRational x = ReadVec3Part();
            BigRational y = ReadVec3Part();
            ReadUnimportant();
            if (ls.CurrentType == LexemeType.RightParen)
            {
                ls.Next();
                return new Vector2(x, y);
            }
            else
            {
                throw new ParsingException("Right parenthesis expected");
            }
        }

        private object ReadVertex2()
        {
            System.Diagnostics.Debug.Assert(ls.CurrentType == LexemeType.PoundVertex2LeftParen);
            ls.Next();
            BigRational x = ReadVec3Part();
            BigRational y = ReadVec3Part();
            ReadUnimportant();
            if (ls.CurrentType == LexemeType.RightParen)
            {
                ls.Next();
                return new Vertex2(x, y);
            }
            else
            {
                throw new ParsingException("Right parenthesis expected");
            }
        }

        private object ReadVector3()
        {
            System.Diagnostics.Debug.Assert(ls.CurrentType == LexemeType.PoundVector3LeftParen);
            ls.Next();
            BigRational x = ReadVec3Part();
            BigRational y = ReadVec3Part();
            BigRational z = ReadVec3Part();
            ReadUnimportant();
            if (ls.CurrentType == LexemeType.RightParen)
            {
                ls.Next();
                return new Vector3(x, y, z);
            }
            else
            {
                throw new ParsingException("Right parenthesis expected");
            }
        }

        private object ReadVertex3()
        {
            System.Diagnostics.Debug.Assert(ls.CurrentType == LexemeType.PoundVertex3LeftParen);
            ls.Next();
            BigRational x = ReadVec3Part();
            BigRational y = ReadVec3Part();
            BigRational z = ReadVec3Part();
            ReadUnimportant();
            if (ls.CurrentType == LexemeType.RightParen)
            {
                ls.Next();
                return new Vertex3(x, y, z);
            }
            else
            {
                throw new ParsingException("Right parenthesis expected");
            }
        }

        private object ReadQuaternion()
        {
            System.Diagnostics.Debug.Assert(ls.CurrentType == LexemeType.PoundQuatLeftParen);
            ls.Next();
            BigRational w = ReadVec3Part();
            BigRational x = ReadVec3Part();
            BigRational y = ReadVec3Part();
            BigRational z = ReadVec3Part();
            ReadUnimportant();
            if (ls.CurrentType == LexemeType.RightParen)
            {
                ls.Next();
                return new Quaternion(w, x, y, z);
            }
            else
            {
                throw new ParsingException("Right parenthesis expected");
            }
        }

        private object Quoted(string quote, object obj)
        {
            Symbol s = new Symbol(quote);
            ConsCell c2 = new ConsCell(obj, SpecialValue.EMPTY_LIST);
            ConsCell c1 = new ConsCell(s, c2);
            return c1;
        }

        private object ReadQuoted(string quote)
        {
            // ls.CurrentType could be Quote, QuasiQuote, Unquote, or UnquoteSplicing
            ls.Next(); // drop it
            ReadUnimportant();
            if (ls.CurrentType == LexemeType.EndOfInput)
                throw new ParsingException("ReadQuoted: Unexpected end of input");
            return Quoted(quote, ReadItem());
        }

        private Symbol ReadSymbol()
        {
            System.Diagnostics.Debug.Assert(ls.CurrentType == LexemeType.Symbol);
            Symbol s = new Symbol(ls.CurrentString); ls.Next(); return s;
        }

        public object ReadItem()
        {
            ReadUnimportant();
            switch(ls.CurrentType)
            {
                case LexemeType.LeftParen: return ReadList();
                case LexemeType.PoundLeftParen: return ReadVector();
                case LexemeType.PoundSLeftParen: return ReadHashSet();
                case LexemeType.PoundMLeftParen: return ReadHashMap();
                case LexemeType.PoundSigLeftParen: return ReadSignature();
                case LexemeType.PoundMsgLeftParen: return ReadMessage();
                case LexemeType.PoundVector3LeftParen: return ReadVector3();
                case LexemeType.PoundVertex3LeftParen: return ReadVertex3();
                case LexemeType.PoundVector2LeftParen: return ReadVector2();
                case LexemeType.PoundVertex2LeftParen: return ReadVertex2();
                case LexemeType.PoundQuatLeftParen: return ReadQuaternion();
                case LexemeType.Quote: return ReadQuoted("quote");
                case LexemeType.Unquote: return ReadQuoted("unquote");
                case LexemeType.QuasiQuote: return ReadQuoted("quasiquote");
                case LexemeType.UnquoteSplicing: return ReadQuoted("unquote-splicing");
                case LexemeType.BeginString: return ReadString();
                case LexemeType.BeginSymbol: return ReadEscapedSymbol();
                case LexemeType.Symbol: return ReadSymbol();
                case LexemeType.Integer: return ReadInteger();
                case LexemeType.HexInteger: return ReadHexInteger();
                case LexemeType.Double: return ReadDouble();
                case LexemeType.Char: return ReadCharacter();
                case LexemeType.HexChar: return ReadHexChar();
                case LexemeType.BooleanTrue: ls.Next();  return true;
                case LexemeType.BooleanFalse: ls.Next(); return false;
                case LexemeType.Guid: return ReadGuid();
                case LexemeType.Numerator: return ReadRational();
                case LexemeType.IPV4Address: return ReadIPV4Address();
                case LexemeType.IPV4EndPoint: return ReadIPV4EndPoint();
                case LexemeType.IPV6Address: return ReadIPV6Address();
                case LexemeType.IPV6EndPoint: return ReadIPV6EndPoint();
                case LexemeType.EndOfInput: return null;
                default: LexemeType erroneousType = ls.CurrentType; ls.Next(); throw new ParsingException("ReadItem: Unexpected " + erroneousType.ToString());
            }
        }

        public static object ReadItem(string str)
        {
            LexemeSource ls = new LexemeSource(new SingleString(str));
            SchemeDataReader sdr = new SchemeDataReader(ls);
            object o = sdr.ReadItem();
            return o;
        }
    }
}
