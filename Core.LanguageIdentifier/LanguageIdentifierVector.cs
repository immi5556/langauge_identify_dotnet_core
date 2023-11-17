using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.LanguageIdentifier
{
    public class LanguageIdentifierVector : LanguageIdentifier
    {

        public LanguageIdentifierVector() { }
        public LanguageIdentifierVector(string distroFile, int cap) : base(distroFile, cap) { }

        /// <summary>
        /// Populate the profile for a particular language.  
        /// </summary>
        /// <param name="gramFreqs">An array of gram-frequency pairs</param>
        /// <returns>A mapping from grams to frequencies normalized for cosine similarity computation</returns>
        protected override Dictionary<string, double> PopulateDistro(GramFreq[] gramFreqs)
        {
            var distro = new Dictionary<string, double>();
            double sum = 0.0;
            for (int i = 0; i < gramFreqs.Length; i++)
            {
                var freq = gramFreqs[i].freq;
                sum += freq * freq;
            }
            var norm = Math.Sqrt(sum);
            for (int i = 0; i < gramFreqs.Length; i++)
            {
                distro[gramFreqs[i].gram] = gramFreqs[i].freq / norm;
            }
            return distro;
        }

        override protected void FillInSims(LangSim[] res, string s)
        {
            double cnt = 0;
            var textDistro = new Dictionary<string, double>();
            foreach (var tok in this.tokenizer(s, this.tlc, this.lo, this.hi))
            {
                if (!textDistro.ContainsKey(tok))
                {
                    textDistro[tok] = 1;
                }
                else
                {
                    textDistro[tok]++;
                }
                cnt++;
            }
            var sum = 0.0;
            var keys = textDistro.Keys.ToList();
            foreach (var k in keys)
            {
                var d = textDistro[k] / cnt;
                textDistro[k] = d;
                sum += d * d;
            }
            var textNorm = Math.Sqrt(sum);
            foreach (var kv in textDistro)
            {
                for (int i = 0; i < this.langs.Length; i++)
                {
                    double gramProb = 0.0;
                    distros[i].TryGetValue(kv.Key, out gramProb);
                    res[i].sim += kv.Value * gramProb;
                }
            }
            for (int i = 0; i < this.langs.Length; i++)
            {
                res[i].sim = res[i].sim == 0.0 ? 0.0 : res[i].sim / textNorm;
            }
        }
    }
}
