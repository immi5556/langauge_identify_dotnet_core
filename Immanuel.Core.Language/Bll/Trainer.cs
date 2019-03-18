using Core.LanguageIdentifier;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Immanuel.Core.Language.Bll
{
    public class Trainer
    {
        public static void Train(Models.LangProfile profile)
        {
            var tokenizer = Tokenization.Tokenizer(profile.PType);
            int lo = int.Parse(profile.MinGram);
            int hi = int.Parse(profile.MaxGram);
            bool tlc = profile.CaseSensitive == "tlc";
            int n = -1; // int.Parse(args[4]);
            var cleaner = Cleaning.MakeCleaner("none");
            //var inFileNames = File.ReadAllLines(args[6]);
            //var inFileNames = profile.Files.Select(t => t. Directory.EnumerateFiles(Path.Combine(Directory.GetCurrentDirectory(), "Data"), "*", SearchOption.AllDirectories);
            var nfolds = -1;
            var fold = -1;
            //string out_profile = Path.Combine(Directory.GetCurrentDirectory(), profile.ProfileName + ".bin.gz");
            //string out_profile = Path.Combine(profile.Path, profile.ProfileName + ".bin.gz");
            string out_profile = profile.ProfileFilePath;
            using (var bw = new BinaryWriter(new GZipStream(new FileStream(out_profile, FileMode.Create, FileAccess.Write), CompressionMode.Compress)))
            {
                bw.Write(profile.PType);
                bw.Write(lo);
                bw.Write(hi);
                bw.Write(tlc);
                bw.Write(profile.Files.Count());
                foreach (var eafile in profile.Files)
                {
                    //var langCode = inFileName.Substring(0, inFileName.IndexOf("_"));
                    var langCode = eafile.Label;
                    long absCnt = 0;
                    MemoryStream tmpFile = new MemoryStream();
                    using (var rd = new StreamReader(eafile.FilePath))
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
