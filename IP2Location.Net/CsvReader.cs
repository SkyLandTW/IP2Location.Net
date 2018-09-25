using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace IP2Location.Net
{
    internal static class CsvReader
    {
        [Pure]
        public static List<string> ParseLine(string line)
        {
            if (string.IsNullOrEmpty(line))
                return new List<string>(0);
            var list = new List<string>(10);
            var start = 0;
            var end = line.Length;
            string token = null;
            while (start < end)
            {
                var c = line[start];
                switch (c)
                {
                    case '"':
                        var dqTokenEnd = FindTokenEnd(line, start + 1, '"');
                        token += Unescape(line, start + 1, dqTokenEnd);
                        start = dqTokenEnd + 1;
                        continue;
                    case '\'':
                        var sqTokenEnd = FindTokenEnd(line, start + 1, '\'');
                        token += Unescape(line, start + 1, sqTokenEnd);
                        start = sqTokenEnd + 1;
                        continue;
                    case ',':
                        list.Add(token);
                        token = null;
                        start += 1;
                        continue;
                    default:
                        var tokenEnd = FindTokenEnd(line, start + 1, ',');
                        token += line.Substring(start, tokenEnd);
                        list.Add(token);
                        token = null;
                        start = tokenEnd + 1;
                        continue;
                }
            }
            if (token != null)
                list.Add(token);
            return list;
        }

        [Pure]
        private static object Unescape(string line, int i, int tokenEnd)
        {
            var s = line.Substring(i, tokenEnd - i);
            var u = s.Replace("\\\"", "\"").Replace("\\'", "'").Replace("\\\\", "\\");
            return u;
        }

        [Pure]
        private static int FindTokenEnd(string line, int start, char target)
        {
            for (var i = start; i < line.Length; i++)
            {
                if (line[i] == target)
                    return i;
            }
            return line.Length;
        }
    }
}