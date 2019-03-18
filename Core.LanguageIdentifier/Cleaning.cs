using System;
using System.Linq;

namespace Core.LanguageIdentifier
{
    public static class Cleaning
    {

        public static Func<string, string> MakeCleaner(string spec)
        {
            switch (spec)
            {
                case "wiki": return CleanWiki;
                case "twit": return CleanTweet;
                case "none": return x => x;
                default: throw new Exception(string.Format("Invalid MakeCleaner spec {0}", spec));
            }
        }

        public static string CleanWiki(string text)
        {
            return text.Replace("thumb|", "")
                       .Replace("left|", "")
                       .Replace("right|", "")
                       .Replace("px|", "")
                       .Replace("\n", "")
                       .RemoveAlphaNumWords();
        }

        private static readonly char[] CharWS = new char[] { ' ' };

        private static string RemoveAlphaNumWords(this string text)
        {
            var sb = new System.Text.StringBuilder();
            var cs = text.ToCharArray();
            for (int i = 0; i < cs.Length; i++)
            {
                if (!char.IsLetterOrDigit(cs[i])) cs[i] = ' ';
            }
            text = new string(cs);
            var words = text.Split(CharWS, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", words.Where(x => LettersOnly(x)));
        }

        private static bool LettersOnly(string word)
        {
            foreach (var c in word)
            {
                if (!char.IsLetter(c)) return false;
            }
            return true;
        }

        public static string CleanTweet(string tweet)
        {
            tweet = tweet.Replace("#N#", " ");   // Cosmos encoding for \n
            tweet = tweet.Replace("#R#", " ");   // Cosmos encoding for \r
            tweet = tweet.Replace("#TAB#", " "); // Cosmos encoding for \t
            tweet = tweet.Replace(@"\/", " ");   // OccasionalCosmos encoding for /
            tweet = System.Web.HttpUtility.HtmlDecode(tweet);
            tweet = tweet.Trim(new char[] { ' ', ',', '.', ':' });
            if (tweet == string.Empty)
            {
                return tweet;
            }
            System.Text.StringBuilder cleanedTweet = new System.Text.StringBuilder();
            bool ignoremode = false;
            for (int i = 0; i < tweet.Length; i++)
            {
                if (Char.IsWhiteSpace(tweet[i]))
                {
                    if (!ignoremode)
                    {
                        cleanedTweet.Append(' ');
                    }
                    ignoremode = false;
                }
                else if (tweet[i] == '@'  //look for username, hashtag, http, or RT @
                      || tweet[i] == '#'
                      || (String.Compare(tweet, i, "http", 0, 4, true, System.Globalization.CultureInfo.InvariantCulture) == 0)
                      || (String.Compare(tweet, i, "rt @", 0, 4, true, System.Globalization.CultureInfo.InvariantCulture) == 0))
                {
                    ignoremode = true;  // ignore all other characters
                }
                else if (!ignoremode)
                {
                    //default, add character
                    cleanedTweet.Append(tweet[i]);
                }
            }
            return cleanedTweet.ToString().Trim(new char[] { ' ', ',', '.', ':' });
        }
    }
}
