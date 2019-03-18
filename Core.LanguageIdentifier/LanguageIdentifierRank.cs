using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.LanguageIdentifier
{
    public class LanguageIdentifierRank : LanguageIdentifier
    {
        private long maxPos = 0;
        private int cap;
        private System.Func<GramFreq[], int, Dictionary<string, double>, long, double> rankSim;

        /// <summary>
        /// Create a new LanguageIdentifierrank object, loading a set of language profiles from a file.
        /// </summary>
        /// <param name="distroFile">name of file containing language profiles</param>
        /// <param name="cap">limit on the number of per-language tokens considered</param>
        /// <param name="rankSimSpec">specifier for rank similarity method to use ("sr" or "sf")</param>
        public LanguageIdentifierRank(string distroFile, int cap, string rankSimSpec) : base(distroFile, cap)
        {
            this.cap = cap;
            this.rankSim = RankSimFn(rankSimSpec);
        }

        /// <summary>
        /// Populate the profile for a particular language.  Assumes gramFreqs is sorted by decreasing
        /// gram frequency.
        /// </summary>
        /// <param name="gramFreqs">An array of gram-frequency pairs</param>
        /// <returns>A mapping from grams to ranks</returns>
        protected override Dictionary<string, double> PopulateDistro(GramFreq[] gramFreqs)
        {
            var distro = new Dictionary<string, double>();
            for (int i = 0; i < gramFreqs.Length; i++)
            {
                distro[gramFreqs[i].gram] = i;
            }
            this.maxPos = Math.Max(this.maxPos, gramFreqs.Length);
            return distro;
        }

        override protected void FillInSims(LangSim[] res, string s)
        {
            var docFreqs = new Dictionary<string, long>();
            foreach (var tok in this.tokenizer(s, this.tlc, this.lo, this.hi))
            {
                if (!docFreqs.ContainsKey(tok)) docFreqs[tok] = 0;
                docFreqs[tok]++;
            }
            var keys = docFreqs.Keys.ToArray();
            var arr = new GramFreq[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                var token = keys[i];
                arr[i] = new GramFreq { gram = token, freq = docFreqs[token] };
            }
            Array.Sort<GramFreq>(arr);
            var docMaxRank = (cap == -1 || arr.Length < cap) ? arr.Length : cap;
            for (int j = 0; j < res.Length; j++)
            {
                res[j].sim = this.rankSim(arr, docMaxRank, this.distros[j], this.maxPos);
            }
        }

        /// <summary>
        /// RankSim.New is a static method that creates and returns an instance of a subclass of RankSim.
        /// </summary>
        /// <param name="spec">A string specifying which subclass to create</param>
        /// <returns>a RankSim object</returns>
        private static System.Func<GramFreq[], int, Dictionary<string, double>, long, double> RankSimFn(string spec)
        {
            switch (spec)
            {
                case "sf": return SpearmanFootrule;
                case "sr": return SpearmanRho;
                case "kt": return KendallTau;
                default: throw new Exception("Unknown RankSim spec");
            }
        }

        private static double GetOrDef(Dictionary<string, double> distro, string key, double def)
        {
            double res = 0;
            return distro.TryGetValue(key, out res) ? res : def;
        }

        private static double SpearmanFootrule(GramFreq[] docTokens, int docMaxRank, Dictionary<string, double> langRanks, long langsMaxRank)
        {
            double sim = 0.0;
            for (int i = 0; i < docMaxRank; i++)
            {
                var gram = docTokens[i].gram;
                sim -= Math.Abs((long)i - GetOrDef(langRanks, gram, langsMaxRank));
            }
            return sim;
        }

        private static double SpearmanRho(GramFreq[] docTokens, int docMaxRank, Dictionary<string, double> langRanks, long langsMaxRank)
        {
            double sum = 0.0;
            for (int i = 0; i < docMaxRank; i++)
            {
                var diff = (double)((long)i - GetOrDef(langRanks, docTokens[i].gram, langsMaxRank));
                sum += diff * diff;
            }
            var rs = 1.0 - (6.0 * sum) / (1.0 * docMaxRank * docMaxRank * docMaxRank - docMaxRank);
            var t = rs * Math.Sqrt((docMaxRank - 2.0) / (1.0 - rs * rs));
            return rs;
        }


        private static double TrueKendallTau(GramFreq[] docTokens, int docMaxRank, Dictionary<string, double> langRanks, long langsMaxRank)
        {
            var langArr = new double[docMaxRank];
            for (int i = 0; i < docMaxRank; i++)
            {
                langArr[i] = GetOrDef(langRanks, docTokens[i].gram, langsMaxRank);
            }
            long n0 = 0;
            long n1 = 0;
            long n2 = 0;
            for (int m = 0; m < docMaxRank; m++)
            {
                for (int n = 0; n < m; n++)
                {
                    var a1 = m - n;
                    var a2 = langArr[m] - langArr[n];
                    var aa = a1 * a2;
                    if (aa != 0)
                    {
                        n1++;
                        n2++;
                        if (aa > 0)
                        {
                            n0++;
                        }
                        else
                        {
                            n0--;
                        }
                    }
                    else
                    {
                        if (a1 != 0) n1++;
                        if (a2 != 0) n2++;
                    }
                }
            }
            var tau = n0 / Math.Sqrt(n1 * n2);
            return tau;
        }

        private static double KendallTau(GramFreq[] docTokens, int docMaxRank, Dictionary<string, double> langRanks, long langsMaxRank)
        {
            var langArr = new double[docMaxRank];
            for (int i = 0; i < docMaxRank; i++)
            {
                langArr[i] = GetOrDef(langRanks, docTokens[i].gram, langsMaxRank);
            }
            long cp = 0;  // the number of concorfdant pairs
            for (int m = 0; m < docMaxRank; m++)
            {
                for (int n = 0; n < m; n++)
                {
                    if (langArr[m] > langArr[n]) cp++;
                }
            }
            return cp;
        }
    }
}
