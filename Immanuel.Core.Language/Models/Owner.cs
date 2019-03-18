using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Immanuel.Core.Language.Models
{
    public class Owner
    {
        public Owner()
        {
            langProfiles = new List<LangProfile>();
            langModels = new List<LangModel>();
        }

        //Used For Training
        public string Email { set; get; }
        public string DosName { set; get; }
        public string Path { set; get; }
        public string Uid { set; get; }
        public string Ip { set; get; }
        public DateTime CreatedAt { get; set; }
        public DateTime AccessedAt { get; set; }
        public List<LangProfile> langProfiles { set; get; }

        //Used for Get
        public List<LangModel> langModels { set; get; }
    }
}
