using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.LanguageIdentifier
{
    public class LanguageIdentifierLikelySp : LanguageIdentifierLikely
    {
        private Dictionary<string, double>[] condDistros;
        private Dictionary<string, double>[] prefixDistros;
        private double[] unknownProbs;
        private double[] smooth;

        public LanguageIdentifierLikelySp(string distroFile, int cap) : base(distroFile, cap)
        {
            var numLangs = this.langs.Length;
            this.condDistros = new Dictionary<string, double>[numLangs];
            this.prefixDistros = new Dictionary<string, double>[numLangs];
            this.smooth = new double[numLangs];
            this.unknownProbs = new double[numLangs];
            for (int i = 0; i < numLangs; i++)
            {
                var ucndDistro = this.distros[i];
                var prefDistro = new Dictionary<string, double>();
                var condDistro = new Dictionary<string, double>();
                long oneOcc = 0;
                long twoOcc = 0;
                var grams = ucndDistro.Keys.ToList();
                foreach (var gram in grams)
                {
                    var occs = ucndDistro[gram];
                    if (occs == 1) oneOcc++;
                    if (occs == 2) twoOcc++;
                    var prefix = gram.Substring(0, gram.Length - 1);
                    if (!prefDistro.ContainsKey(prefix)) prefDistro[prefix] = 0.0;
                    prefDistro[prefix] += occs;
                }
                prefixDistros[i] = prefDistro;

                smooth[i] = twoOcc == 0 ? 0.5 * oneOcc : ((double)oneOcc * (double)oneOcc) / (2.0 * twoOcc);
                foreach (var gram in grams)
                {
                    condDistro[gram] = ucndDistro[gram] / (prefDistro[gram.Substring(0, gram.Length - 1)] + smooth[i]);
                    ucndDistro[gram] = ucndDistro[gram] / (numOccs[i] + smooth[i]);
                }
                this.condDistros[i] = condDistro;
                this.unknownProbs[i] = 1.0 / (numOccs[i] + smooth[i]);
            }
        }

        protected override double Probability(string gram, int i, bool first)
        {
            return first
              ? GetOrDef(distros[i], gram, this.unknownProbs[i])
              : GetOrDef(condDistros[i], gram, 1.0 / (GetOrDef(prefixDistros[i], gram.Substring(0, gram.Length - 1), 0.0) + smooth[i]));
        }
    }
}
