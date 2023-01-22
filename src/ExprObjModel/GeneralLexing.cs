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
using System.Text.RegularExpressions;

namespace ExprObjModel.Lexing
{
	public interface IMatcher<T>
	{
		bool Matches(string str, int pos);
		string Match { get; }
		int MatchLength { get; }
		T AcceptCode { get; }
	}
		
	public class StringMatcher<T> : IMatcher<T>
	{
		public StringMatcher(string strToMatch, T acceptCode)
		{
			this.strToMatch = strToMatch;
			this.acceptCode = acceptCode;
		}
			
		private string strToMatch;
		private T acceptCode;
			
		public bool Matches(string str, int pos)
		{
			if ((str.Length - pos) < strToMatch.Length) return false;
			return (str.Substring(pos, strToMatch.Length) == strToMatch);
		}

		public string Match { get { return strToMatch; } }
		public int MatchLength { get { return strToMatch.Length; } }
		public T AcceptCode { get { return acceptCode; } }
	}
		
	public class RegexMatcher<T> : IMatcher<T>
	{
		public RegexMatcher(string pattern, T acceptCode)
		{
			this.regex = new Regex(pattern);
			this.acceptCode = acceptCode;
		}
			
		private Regex regex;
		private string lastMatch;
		private int lastMatchLength;
		private T acceptCode;
			
		public bool Matches(string str, int pos)
		{
			Match m = regex.Match(str, pos);
			if (!m.Success) return false;
			if (m.Index != pos) return false;
			lastMatch = m.Value;
			lastMatchLength = m.Length;
			return true;
		}

		public string Match { get { return lastMatch; } }
		public int MatchLength { get { return lastMatchLength; } }
		public T AcceptCode { get { return acceptCode; } }
	}
		
	public class CompoundMatcherFactory<T>
	{
		public CompoundMatcherFactory()
		{
			a = new List<IMatcher<T>>();
		}

        List<IMatcher<T>> a;
			
		public void Add(IMatcher<T> matcher) { a.Add(matcher); }
			
		public void AddString(string str, T acceptCode)
		{
			a.Add(new StringMatcher<T>(str, acceptCode));
		}
			
		public void AddRegex(string pattern, T acceptCode)
		{
			a.Add(new RegexMatcher<T>(pattern, acceptCode));
		}
		
		public void Clear()
		{
			a.Clear();
		}
		
		public IMatcher<T> GetMatcher()
		{
            IMatcher<T>[] arr = a.ToArray();
			IMatcher<T> m = new CompoundMatcher(arr);
			Clear();
			return m;
		}
			
		private class CompoundMatcher : IMatcher<T>
		{
			public CompoundMatcher(IMatcher<T>[] arr)
			{
				this.arr = arr;
			}
				
			private IMatcher<T>[] arr;
			private string lastMatch;
			private int lastMatchLength;
			private T lastAcceptCode;
				
			public bool Matches(string str, int pos)
			{
				bool haveMatch = false;
				for (int i = 0; i < arr.Length; ++i)
				{
					IMatcher<T> m = arr[i];
					if (m.Matches(str, pos))
					{
						if (!haveMatch || (haveMatch && (lastMatchLength < m.MatchLength)))
						{
							haveMatch = true;
							lastMatch = m.Match;
							lastMatchLength = m.MatchLength;
							lastAcceptCode =  m.AcceptCode;
						}
					}
				}
				return haveMatch;
			}

			public string Match { get { return lastMatch; } }
			public int MatchLength { get { return lastMatchLength; } }
			public T AcceptCode { get { return lastAcceptCode; } }
		}
	}
}