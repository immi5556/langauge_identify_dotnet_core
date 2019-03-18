using System;
using System.Collections.Generic;
using System.Text;

namespace Core.LanguageIdentifier
{
    public class Tokenization
    {
        public static Func<IEnumerable<char>, bool, int, int, IEnumerable<string>> Tokenizer(string spec)
        {
            switch (spec)
            {
                case "char": return TokenizeChar;
                case "word": return TokenizeWord;
                default: return null;
            }
        }

        internal static IEnumerable<string> TokenizeChar(IEnumerable<char> s, bool tlc, int lo, int hi)
        {
            var lastWasLetter = false;
            var buf = new char[hi];
            long j = 0;
            foreach (char c in s)
            {
                var isLetter = char.IsLetter(c);
                if (isLetter || lastWasLetter)
                {
                    int mgl = (int)Math.Min(j + 1, hi);  // mgl = maximal gram length
                    buf[mgl - 1] = isLetter ? (tlc ? Char.ToLowerInvariant(c) : c) : ' ';
                    for (int i = lo; i <= mgl; i++)
                    {
                        yield return new string(buf, mgl - i, i);
                    }
                    if (j >= hi - 1)
                    {
                        for (int i = 1; i < hi; i++)
                        {
                            buf[i - 1] = buf[i];
                        }
                    }

                }
                lastWasLetter = isLetter;
                j++;
            }
        }

        internal static IEnumerable<string> TokenizeWord(IEnumerable<char> s, bool tlc, int lo, int hi)
        {
            var buf = new StringBuilder();
            var words = new string[hi];
            int j = 0;
            int nextWord = 0;
            foreach (char c in s)
            {
                var isLetter = char.IsLetter(c);
                if (isLetter)
                {
                    buf.Append(tlc ? Char.ToLowerInvariant(c) : c);
                }
                else if (buf.Length > 0)
                {
                    words[nextWord] = buf.ToString();
                    buf.Clear();
                    for (int i = lo; i <= nextWord + 1; i++)
                    {
                        var res = new StringBuilder();
                        for (int k = 0; k < i; k++)
                        {
                            if (k > 0) res.Append(' ');
                            res.Append(words[nextWord + 1 - i + k]);
                        }
                        yield return res.ToString();
                    }
                    if (nextWord < hi - 1)
                    {
                        nextWord++;
                    }
                    else
                    {
                        for (int i = 1; i < hi; i++)
                        {
                            words[i - 1] = words[i];
                        }
                    }
                }
                j++;
            }
        }
    }
}
