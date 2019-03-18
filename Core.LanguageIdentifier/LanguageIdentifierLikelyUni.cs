using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.LanguageIdentifier
{
    public class LanguageIdentifierLikelyUni : LanguageIdentifierLikely
    {
        private Dictionary<string, double>[] condDistros;
        private double unknownProb = 1;

        public LanguageIdentifierLikelyUni(string distroFile, int cap) : base(distroFile, cap)
        {
            var numLangs = this.langs.Length;
            this.condDistros = new Dictionary<string, double>[numLangs];
            for (int i = 0; i < numLangs; i++)
            {
                var prefDistro = new Dictionary<string, double>();
                var condDistro = new Dictionary<string, double>();
                double uProb = 1.0 / (double)this.numOccs[i];
                var ucndDistro = this.distros[i];
                var grams = ucndDistro.Keys.ToList();
                foreach (var gram in grams)
                {
                    var occs = ucndDistro[gram];
                    ucndDistro[gram] = occs * uProb;
                    var prefix = gram.Substring(0, gram.Length - 1);
                    if (!prefDistro.ContainsKey(prefix)) prefDistro[prefix] = 0.0;
                    prefDistro[prefix] += occs * uProb;
                }
                foreach (var gram in ucndDistro.Keys)
                {
                    var prefix = gram.Substring(0, gram.Length - 1);
                    condDistro[gram] = ucndDistro[gram] / prefDistro[prefix];
                }
                this.condDistros[i] = condDistro;
                this.unknownProb = Math.Min(this.unknownProb, uProb);
            }
        }

        protected override double Probability(string gram, int i, bool first)
        {
            return first
              ? GetOrDef(distros[i], gram, this.unknownProb)
              : GetOrDef(condDistros[i], gram, this.unknownProb);
        }
    }
}
