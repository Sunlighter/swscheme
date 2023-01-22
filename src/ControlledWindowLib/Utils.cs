using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ControlledWindowLib
{
    static class Utils
    {
        public static string Concatenate(this IEnumerable<string> strings, string delimiter)
        {
            StringBuilder sb = new StringBuilder();
            bool needDelim = false;
            foreach (string str in strings)
            {
                if (needDelim) sb.Append(delimiter);
                sb.Append(str);
                needDelim = true;
            }
            return sb.ToString();
        }
    }
}
