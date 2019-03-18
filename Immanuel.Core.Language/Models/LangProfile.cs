using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Immanuel.Core.Language.Models
{
    public class LangProfile
    {
        public LangProfile()
        {
            Files = new List<LabelInput>();
            Tags = new List<string>();
        }
        public bool IsPrivate { get; set; }
        public string ProfileName { get; set; }
        public string Email { get; set; }
        public string Path { get; set; }
        public string ProfileFilePath { get; set; }
        public List<string> Tags { get; set; }
        public List<LabelInput> Files { get; set; }

        //Advance
        public string PType { get; set; }
        public string MinGram { get; set; }
        public string MaxGram { get; set; }
        public string CaseSensitive { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime AccessedAt { get; set; }
        public string Ip { get; set; }
    }

    public class LabelInput
    {
        public string Label { get; set; }
        public string FilePath { get; set; }
    }
}
