using System;
using System.IO;
using System.Xml;

namespace Core.LanguageIdentifier
{
    /* This program comiles a distribution of n-grams for n ranging from 1 to N in a corpus of Wikipedia abstracts.*/
    public static class ParseWikipedia
    {

        public static void ProcessCmdline(string[] args)
        {
            if (args.Length != 2 || (args[1] != "abstract" && args[1] != "content"))
            {
                Console.Error.WriteLine("Usage: ParseWikipedia xml-files.txt [abstract|content]");
            }
            else
            {
                var inFileNames = File.ReadAllLines(args[0]);
                foreach (var inFileName in inFileNames)
                {
                    var langCode = inFileName.Substring(0, inFileName.IndexOf("wiki"));
                    var outFile = langCode + "_parsed.txt";
                    long absCnt = 0;
                    if (args[1] == "content")
                    {
                        using (var rd = new XmlContentReader(new StreamReader(inFileName, System.Text.Encoding.UTF8, false)))
                        {
                            using (var wr = new StreamWriter(outFile, false, System.Text.Encoding.UTF8))
                            {
                                for (; ; )
                                {
                                    var text = rd.Read();
                                    if (text == null) break;
                                    wr.WriteLine(Cleaning.CleanWiki(text));
                                    absCnt++;
                                }
                            }
                        }
                    }
                    else
                    {
                        using (var rd = new XmlTextReader(inFileName))
                        {
                            using (var wr = new StreamWriter(outFile, false, System.Text.Encoding.UTF8))
                            {
                                while (rd.Read())
                                {
                                    if (rd.IsStartElement("abstract") && !rd.IsEmptyElement)
                                    {
                                        var text = rd.ReadElementString("abstract");
                                        wr.WriteLine(Cleaning.CleanWiki(text));
                                        absCnt++;
                                    }
                                }
                            }
                        }
                    }
                    Console.Error.WriteLine("Done with {0}. Wrote {1} docs.", langCode, absCnt);
                }
            }
        }

        public static void ProcessInline()
        {
            //string[] args = new string[]
            //{
            //    "content"
            //};
            //var inFileNames = File.ReadAllLines(args[0]);
            //var inFileNames = new string[]
            //{

            //};
            foreach (var inFileName in Directory.EnumerateFiles(Path.Combine(Directory.GetCurrentDirectory(), "Data"), "*", SearchOption.AllDirectories))
            //foreach (var inFileName in inFileNames)
            {
                var langCode = Path.GetFileNameWithoutExtension(inFileName).Substring(0, Path.GetFileNameWithoutExtension(inFileName).IndexOf("wiki"));
                var outFile = Path.Combine(Path.GetDirectoryName(inFileName),  langCode + "_" + Path.GetFileNameWithoutExtension(inFileName) + "_parsed.txt");
                var wikiformat = !inFileName.Contains("abst") ? "content" : "abstract";
                //var wikiformat = "abstract";
                long absCnt = 0;
                if (wikiformat == "content")
                {
                    using (var rd = new XmlContentReader(new StreamReader(inFileName, System.Text.Encoding.UTF8, false)))
                    {
                        using (var wr = new StreamWriter(outFile, false, System.Text.Encoding.UTF8))
                        {
                            for (; ; )
                            {
                                var text = rd.Read();
                                if (text == null) break;
                                wr.WriteLine(Cleaning.CleanWiki(text));
                                absCnt++;
                            }
                        }
                    }
                }
                else
                {
                    using (var rd = new XmlTextReader(inFileName))
                    {
                        using (var wr = new StreamWriter(outFile, false, System.Text.Encoding.UTF8))
                        {
                            while (rd.Read())
                            {
                                if (rd.IsStartElement("abstract") && !rd.IsEmptyElement)
                                {
                                    var text = rd.ReadElementString("abstract");
                                    wr.WriteLine(Cleaning.CleanWiki(text));
                                    absCnt++;
                                }
                            }
                        }
                    }
                }
                Console.Error.WriteLine("Done with {0}. Wrote {1} docs.", langCode, absCnt);
            }
        }

        public static void Main(string[] args)
        {
            //ProcessCmdline(args)
            ProcessInline();
        }

        private class XmlContentReader : IDisposable
        {
            private StreamReader rd;
            private string cache;
            internal XmlContentReader(StreamReader rd)
            {
                this.rd = rd;
                this.cache = "";
            }

            public void Dispose()
            {
                this.rd.Dispose();
            }

            private const string OPEN = "<content>";
            private const string CLOS = "</content>";

            /// <summary>
            /// Extracts the text between content tags.
            /// </summary>
            /// <returns>the next content element in the Xml stream, or null if the stream is exhausted</returns>
            internal string Read()
            {
                // Read lines until opening tag is found
                while (cache != null && !cache.ToLowerInvariant().Contains(OPEN))
                {
                    var line = rd.ReadLine();
                    if (line == null)
                    {
                        cache = null;
                    }
                    else
                    {
                        cache += line;
                    }
                }
                if (cache == null) return null;
                var p0 = cache.ToLowerInvariant().IndexOf(OPEN);
                cache = cache.Substring(p0 + OPEN.Length);
                // Read lines until closing tag is found
                while (cache != null && !cache.ToLowerInvariant().Contains(CLOS))
                {
                    var line = rd.ReadLine();
                    if (line == null)
                    {
                        cache = null;
                    }
                    else
                    {
                        cache += line;
                    }
                }
                if (cache == null) return null;
                var p1 = cache.ToLowerInvariant().IndexOf(CLOS);
                var res = cache.Substring(0, p1);
                cache = cache.Substring(p1 + CLOS.Length);
                return res;
            }
        }
    }
}
