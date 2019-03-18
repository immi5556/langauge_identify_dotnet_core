using System;
using System.Collections.Generic;
using System.Text;

namespace Core.LanguageIdentifier
{
    public abstract class LanguageIdentifierLikely : LanguageIdentifier
    {
        public LanguageIdentifierLikely(string distroFile, int cap) : base(distroFile, cap)
        {
            if (this.tokenizer != Tokenization.TokenizeChar || this.lo != this.hi)
            {
                throw new Exception("LanguageIdentifierLikely only defined for character-grams of uniform length");
            }
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
                distro[gramFreqs[i].gram] = gramFreqs[i].freq;
            }
            return distro;
        }

        protected abstract double Probability(string gram, int i, bool first);

        override protected void FillInSims(LangSim[] res, string s)
        {
            bool first = true;
            foreach (var gram in this.tokenizer(s, this.tlc, this.lo, this.hi))
            {
                for (int i = 0; i < this.langs.Length; i++)
                {
                    res[i].sim += Math.Log(this.Probability(gram, i, first), 2.0);
                }
                first = false;
            }
        }

        protected static double GetOrDef(Dictionary<string, double> distro, string key, double def)
        {
            double res = 0.0;
            return distro.TryGetValue(key, out res) ? res : def;
        }
    }
}
