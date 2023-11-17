using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Core.LanguageIdentifier
{
    public class CompileDistro
    {
        public static void ProcesCmdLine(string[] args)
        {
            if (args.Length != 8 && args.Length != 10)
            {
                Console.Error.WriteLine("Usage: CompileDistro [char|word] lo hi [ncf|tlc] maxTokens [wiki|twit|none] files.txt distros.bin.gz [nfolds fold]");
                Console.Error.WriteLine("  where [char|word] indicates whether to use character- or word-based n-grams");
                Console.Error.WriteLine("        lo is the minimum gram length");
                Console.Error.WriteLine("        lo is the maximum gram length");
                Console.Error.WriteLine("        [ncf|tlc] incicate whether to do no-case-folding or to-lower-case");
                Console.Error.WriteLine("        maxTokens is the maximum number of most-frequent tokens that should be retained per language");
                Console.Error.WriteLine("        [wiki|twit|none] incicates which text cleaning scheme to use");
                Console.Error.WriteLine("        file.txt contains a list of training file names, one per language, each file name of the form xx_*,");
                Console.Error.WriteLine("           where xx is the ISO 639-1 language code");
                Console.Error.WriteLine("        distros.bin.gz is the name of the languages profile file that is to be generated");
                Console.Error.WriteLine("        nfolds (optionally) indicates the number of folds for n-fold cross validation");
                Console.Error.WriteLine("        fold (optionally) indicates the number of the fold to exclude from the profile");
            }
            else
            {
                const string tmpFile = "xml-to-txt.tmp";
                var tokenizer = Tokenization.Tokenizer(args[0]);
                int lo = int.Parse(args[1]);
                int hi = int.Parse(args[2]);
                bool tlc = args[3] == "tlc";
                int n = int.Parse(args[4]);
                var cleaner = Cleaning.MakeCleaner(args[5]);
                var inFileNames = File.ReadAllLines(args[6]);
                var nfolds = args.Length == 8 ? -1 : int.Parse(args[8]);
                var fold = args.Length == 8 ? -1 : int.Parse(args[9]);
                using (var bw = new BinaryWriter(new GZipStream(new FileStream(args[7], FileMode.Create, FileAccess.Write), CompressionMode.Compress)))
                {
                    bw.Write(args[0]);
                    bw.Write(lo);
                    bw.Write(hi);
                    bw.Write(tlc);
                    bw.Write(inFileNames.Length);
                    foreach (var inFileName in inFileNames)
                    {
                        var langCode = inFileName.Substring(0, inFileName.IndexOf("_"));
                        long absCnt = 0;
                        using (var rd = new StreamReader(inFileName))
                        {
                            using (var wr = new StreamWriter(tmpFile))
                            {
                                for (; ; )
                                {
                                    var text = rd.ReadLine();
                                    if (text == null) break;
                                    if (fold == -1 || (absCnt % nfolds) != fold)
                                    {
                                        wr.WriteLine(cleaner(text));
                                    }
                                    absCnt++;
                                }
                            }
                        }
                        using (var rd = new StreamReader(tmpFile))
                        {
                            var distro = new Dictionary<string, long>();
                            foreach (var tok in tokenizer(EnumFromRd(rd), tlc, lo, hi))
                            {
                                if (!distro.ContainsKey(tok))
                                {
                                    distro[tok] = 1;
                                }
                                else
                                {
                                    distro[tok]++;
                                }
                            }
                            var orderedDistro = n > 0
                              ? distro.OrderByDescending(x => x.Value).Take(n)
                              : distro.OrderByDescending(x => x.Value);
                            bw.Write(langCode);
                            bw.Write(orderedDistro.LongCount());
                            long grams = 0;
                            long occs = 0;
                            foreach (var kv in orderedDistro)
                            {
                                bw.Write(kv.Key);
                                bw.Write(kv.Value);
                                grams++;
                                occs += kv.Value;
                            }
                            Console.WriteLine("{0}\t{1}\t{2}\t{3}", langCode, absCnt, grams, occs);
                        }
                    }
                }
            }
        }

        public static void Main(string[] args)
        {
            //ProcesCmdLine(args);
            ProcessInline();
        }

        public static void ProcessInline()
        {
            string[] args = new string[]
            {
                //"char",
                "word",

                "1",
                "5",
                "ncf",
                "-1",
                "none",
                "langprofiles-char-1_5-nfc-all.bin.gz"
            };

            //var infils = new Dictionary<string, string>()
            //{
            //    { "aa", "Abkhazian.txt" },
            //    { "ab", "Afar.txt" },
            //};

            //const string tmpFile = "xml-to-txt.tmp";
            var tokenizer = Tokenization.Tokenizer(args[0]);
            int lo = int.Parse(args[1]);
            int hi = int.Parse(args[2]);
            bool tlc = args[3] == "tlc";
            int n = int.Parse(args[4]);
            var cleaner = Cleaning.MakeCleaner(args[5]);
            //var inFileNames = File.ReadAllLines(args[6]);
            var inFileNames = Directory.EnumerateFiles(Path.Combine(Directory.GetCurrentDirectory(), "Data"), "*", SearchOption.AllDirectories);
            var nfolds = args.Length == 7 ? -1 : int.Parse(args[7]);
            var fold = args.Length == 7 ? -1 : int.Parse(args[8]);
            string out_profile = Path.Combine(Directory.GetCurrentDirectory(), args[6]);
            using (var bw = new BinaryWriter(new GZipStream(new FileStream(out_profile, FileMode.Create, FileAccess.Write), CompressionMode.Compress)))
            {
                bw.Write(args[0]);
                bw.Write(lo);
                bw.Write(hi);
                bw.Write(tlc);
                bw.Write(inFileNames.Count());
                foreach (var inFileName in inFileNames)
                {
                    //var langCode = inFileName.Substring(0, inFileName.IndexOf("_"));
                    var langCode = Path.GetFileNameWithoutExtension(inFileName).Substring(0, Path.GetFileNameWithoutExtension(inFileName).IndexOf("_"));
                    long absCnt = 0;
                    MemoryStream tmpFile = new MemoryStream();
                    using (var rd = new StreamReader(inFileName))
                    {
                        using (var wr = new StreamWriter(tmpFile))
                        {
                            for (; ; )
                            {
                                var text = rd.ReadLine();
                                if (text == null) break;
                                if (fold == -1 || (absCnt % nfolds) != fold)
                                {
                                    wr.WriteLine(cleaner(text));
                                }
                                absCnt++;
                            }
                        }
                    }
                    using (var rd = new StreamReader(new MemoryStream(tmpFile.ToArray())))
                    {
                        var distro = new Dictionary<string, long>();
                        foreach (var tok in tokenizer(EnumFromRd(rd), tlc, lo, hi))
                        {
                            if (!distro.ContainsKey(tok))
                            {
                                distro[tok] = 1;
                            }
                            else
                            {
                                distro[tok]++;
                            }
                        }
                        var orderedDistro = n > 0
                          ? distro.OrderByDescending(x => x.Value).Take(n)
                          : distro.OrderByDescending(x => x.Value);
                        bw.Write(langCode);
                        bw.Write(orderedDistro.LongCount());
                        long grams = 0;
                        long occs = 0;
                        foreach (var kv in orderedDistro)
                        {
                            bw.Write(kv.Key);
                            bw.Write(kv.Value);
                            grams++;
                            occs += kv.Value;
                        }
                        Console.WriteLine("{0}\t{1}\t{2}\t{3}", langCode, absCnt, grams, occs);
                    }
                }
            }
        }

        private static IEnumerable<char> EnumFromRd(TextReader rd)
        {
            for (; ; )
            {
                int x = rd.Read();
                if (x == -1) break;
                yield return Convert.ToChar(x);
            }
        }
    }
}
