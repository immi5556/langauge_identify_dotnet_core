using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Core.LanguageIdentifier
{
    public class LabeledData
    {
        public struct LabeledText
        {
            public string lang;
            public string text;
        }

        public enum FileKind
        {
            Unknown,
            ListOfFiles,
            LabeledTexts
        };

        public static IEnumerable<LabeledText> Read(string file, int nfolds, int fold, bool train)
        {
            var langCnts = new Dictionary<string, int>();
            foreach (var pair in SubRead(file))
            {
                if (fold == -1)
                {
                    yield return pair;
                }
                else
                {
                    if (!langCnts.ContainsKey(pair.lang)) langCnts[pair.lang] = 0;
                    var mod = langCnts[pair.lang] % nfolds;
                    if ((train && mod != fold) || (!train && mod == fold))
                    {
                        yield return pair;
                    }
                    langCnts[pair.lang]++;
                }
            }
        }

        private static IEnumerable<LabeledText> SubRead(string file)
        {
            FileKind fileKind = FileKind.Unknown;
            using (var rd = new StreamReader(file))
            {
                var line = rd.ReadLine();
                //for (; ; )
                while (line != null)
                {
                    //if (line == null) break;
                    if (fileKind == FileKind.Unknown)
                    {
                        fileKind = line.Contains('\t') ? FileKind.LabeledTexts : FileKind.ListOfFiles;
                    }
                    if (fileKind == FileKind.ListOfFiles)
                    {
                        var langCode = line.Substring(0, line.IndexOf("_"));
                        using (var rd2 = new StreamReader(line))
                        {
                            for (; ; )
                            {
                                var text = rd2.ReadLine();
                                if (text == null) break;
                                yield return new LabeledText { lang = langCode, text = text };
                            }
                        }
                    }
                    else
                    {
                        var f = line.Split('\t');
                        //Debug.Assert(f.Length == 2);
                        yield return new LabeledText { lang = f[1], text = f[0] };
                    }
                    line = rd.ReadLine();
                }
            }
        }
    }
}
