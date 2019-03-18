using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Core.LanguageIdentifier
{
    abstract public class LanguageIdentifier
    {
        public static LanguageIdentifier New(string distro, string spec, int cap)
        {
            var liSpec = spec.Split(',');
            switch (liSpec[0])
            {
                case "Likely": return new LanguageIdentifierLikelyUni(distro, cap);
                case "LikelyAp": return new LanguageIdentifierLikelyAp(distro, cap);
                case "LikelySp": return new LanguageIdentifierLikelySp(distro, cap);
                case "Rank": return new LanguageIdentifierRank(distro, cap, liSpec[1]);
                case "Vector": return new LanguageIdentifierVector(distro, cap);
                default: throw new Exception("Illegal value for li");
            }
        }

        protected string[] langs;
        protected bool tlc;
        protected int lo;
        protected int hi;
        protected System.Func<IEnumerable<char>, bool, int, int, IEnumerable<string>> tokenizer;
        protected Dictionary<string, double>[] distros;
        protected long[] numOccs;

        public LanguageIdentifier(string distroFile, int cap)
        {
            using (var br = new BinaryReader(new GZipStream(new FileStream(distroFile, FileMode.Open, FileAccess.Read), CompressionMode.Decompress)))
            {
                this.tokenizer = Tokenization.Tokenizer(br.ReadString());
                this.lo = br.ReadInt32();
                this.hi = br.ReadInt32();
                this.tlc = br.ReadBoolean();
                var numLangs = br.ReadInt32();
                this.langs = new string[numLangs];
                this.distros = new Dictionary<string, double>[numLangs];
                this.numOccs = new long[numLangs];
                for (int i = 0; i < numLangs; i++)
                {
                    var distro = new Dictionary<string, double>();
                    this.langs[i] = br.ReadString();
                    var numGrams = br.ReadInt64();
                    var maxRank = (cap == -1 || numGrams < cap) ? numGrams : cap;
                    var arr = new GramFreq[maxRank];
                    for (int j = 0; j < numGrams; j++)
                    {
                        var gram = br.ReadString();
                        var occs = br.ReadInt64();
                        if (j < maxRank)
                        {
                            this.numOccs[i] += occs;
                            arr[j] = new GramFreq { gram = gram, freq = (double)occs };
                        }
                    }
                    this.distros[i] = PopulateDistro(arr);
                }
            }
        }

        protected struct GramFreq : IComparable<GramFreq>
        {
            internal string gram;
            internal double freq;
            public int CompareTo(GramFreq that)
            {
                return that.freq.CompareTo(this.freq);
            }
        }

        protected abstract Dictionary<string, double> PopulateDistro(GramFreq[] gramFreqs);

        public LangSim[] CompareToLangs(string s)
        {
            LangSim[] res = new LangSim[this.langs.Length];
            for (int i = 0; i < this.langs.Length; i++)
            {
                res[i].lang = this.langs[i];
            }
            this.FillInSims(res, s);
            Array.Sort<LangSim>(res);
            return res;
        }

        /// <summary>
        /// Any subclass must implement this method plus a constructor.
        /// </summary>
        /// <param name="res">The result array whose "sim" fields are to be filled in</param>
        /// <param name="s">The text whose language is to be identified</param>
        abstract protected void FillInSims(LangSim[] res, string s);

        public string Identify(string s)
        {
            return this.CompareToLangs(s)[0].lang;
        }

        public string[] KnownLangs
        {
            get { return (string[])this.langs.Clone(); }
        }

        public static string LStoString(LangSim[] ls)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < ls.Length; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append(ls[i].lang);
                sb.Append(':');
                sb.Append(ls[i].sim);
            }
            return sb.ToString();
        }

        public struct LangSim : IComparable<LangSim>
        {
            public string lang;
            public double sim;
            public int CompareTo(LangSim that)
            {
                return that.sim.CompareTo(this.sim);
            }
        }
    }
}
