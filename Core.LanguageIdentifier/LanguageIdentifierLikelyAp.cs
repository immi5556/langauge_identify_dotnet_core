using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.LanguageIdentifier
{
    public class LanguageIdentifierLikelyAp : LanguageIdentifierLikely
    {
        private Dictionary<string, double>[] condDistros;
        private Dictionary<string, double>[] prefixDistros;
        private double[] unknownProbs;
        private double[] smooth;
        private double[] unigramCnt;

        public LanguageIdentifierLikelyAp(string distroFile, int cap) : base(distroFile, cap)
        {
            var numLangs = this.langs.Length;
            this.condDistros = new Dictionary<string, double>[numLangs];
            this.prefixDistros = new Dictionary<string, double>[numLangs];
            this.smooth = new double[numLangs];
            this.unknownProbs = new double[numLangs];
            this.unigramCnt = new double[numLangs];
            for (int i = 0; i < numLangs; i++)
            {
                var ucndDistro = this.distros[i];
                var prefDistro = new Dictionary<string, double>();
                var condDistro = new Dictionary<string, double>();
                long oneOcc = 0;
                long twoOcc = 0;
                var unigramSet = new HashSet<char>();
                var grams = ucndDistro.Keys.ToList();
                var numGrams = grams.Count;
                foreach (var gram in grams)
                {
                    var occs = ucndDistro[gram];
                    if (occs == 1) oneOcc++;
                    if (occs == 2) twoOcc++;
                    for (int k = 0; k < gram.Length; k++) unigramSet.Add(gram[k]);
                    var prefix = gram.Substring(0, gram.Length - 1);
                    if (!prefDistro.ContainsKey(prefix)) prefDistro[prefix] = 0.0;
                    prefDistro[prefix] += occs;
                }
                prefixDistros[i] = prefDistro;
                this.unigramCnt[i] = unigramSet.Count;

                //smooth[i] = twoOcc == 0 ? 0.5 * oneOcc : ((double)oneOcc * (double)oneOcc) / (2.0 * twoOcc);
                smooth[i] = 0.1;  // Quite ad-hoc ...
                foreach (var gram in grams)
                {
                    // Moises says: Need to account for smoothing in the normatization!
                    condDistro[gram] = (ucndDistro[gram] + smooth[i]) / (prefDistro[gram.Substring(0, gram.Length - 1)] + smooth[i] * unigramCnt[i]);
                    ucndDistro[gram] = (ucndDistro[gram] + smooth[i]) / (numOccs[i] + smooth[i] * numGrams);
                }
                this.condDistros[i] = condDistro;
                this.unknownProbs[i] = smooth[i] / (numOccs[i] + smooth[i] * numGrams);
            }
        }

        protected override double Probability(string gram, int i, bool first)
        {
            return first
              ? GetOrDef(distros[i], gram, this.unknownProbs[i])
              : GetOrDef(condDistros[i], gram, smooth[i] / (GetOrDef(prefixDistros[i], gram.Substring(0, gram.Length - 1), 0.0) + smooth[i] * unigramCnt[i]));
        }
    }
}
