using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.LanguageIdentifier;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Immanuel.Core.Language.Controllers
{
    public class LanguageController : Controller
    {
        LanguageIdentifier _li;
        Microsoft.AspNetCore.Hosting.IHostingEnvironment _env;
        Microsoft.AspNetCore.SignalR.IHubContext<Hubs.ProgressHub> _hub;
        public LanguageController(LanguageIdentifier li, Microsoft.AspNetCore.Hosting.IHostingEnvironment env, Microsoft.AspNetCore.SignalR.IHubContext<Hubs.ProgressHub> hub)
        {
            _li = li;
            _env = env;
            _hub = hub;
        }

        [HttpGet]
        [Route("language/identify/{text}")]
        public string Check(string text)
        {
            var cleaner = Cleaning.MakeCleaner("none");
            List<string> wrds = new List<string>();
            (text ?? "").Split('.', StringSplitOptions.RemoveEmptyEntries).ToList().ForEach(t => wrds.AddRange(t.Split(' ', StringSplitOptions.RemoveEmptyEntries)));
            return string.Join(',', wrds.Select(t => _li.Identify(cleaner(t))).Distinct());
        }

        [HttpPost]
        [Route("language/identify")]
        public List<Models.LangModel> CheckPost()
        {
            string text = HttpContext.Request.Form["text"];
            var cleaner = Cleaning.MakeCleaner("none");
            List<string> wrds = new List<string>();
            (text ?? "").Split('.', StringSplitOptions.RemoveEmptyEntries).ToList().ForEach(t => wrds.AddRange(t.Split(' ', StringSplitOptions.RemoveEmptyEntries)));
            return wrds.Select(t => _li.Identify(cleaner(t))).GroupBy(t => t).Select(t1 => new Models.LangModel() { lang = t1.Key, count = t1.Count() }).ToList();
        }

        [HttpPost]
        [Route("language/learn")]
        public Models.Owner LearnPost()
        {
            var ip = HttpContext.Connection.RemoteIpAddress.ToString();
            string email = HttpContext.Request.Form["useremail"];
            var own = Bll.Util.CreateOrGetOwner(_env, new Models.Owner() { Email = email, Ip = ip });
            bool pvtchk = HttpContext.Request.Form["checkprivate"] == "checked";
            string profname = HttpContext.Request.Form["ccrprofname"];
            string tags = HttpContext.Request.Form["tags"];
            own.langProfiles = new List<Models.LangProfile>()
            {
                new Models.LangProfile()
                {
                    Tags = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(tags),
                    IsPrivate = pvtchk,
                    ProfileName = profname,
                    Ip = ip,
                    PType = HttpContext.Request.Form["ptype"],
                    MaxGram = HttpContext.Request.Form["maxgram"],
                    MinGram = HttpContext.Request.Form["mingram"],
                    CaseSensitive = HttpContext.Request.Form["casesense"]
                }
            };
            Bll.Util.CreateLangProfile(HttpContext, own);
            Bll.Trainer.Train(own.langProfiles[0]);
            return own;
        }

        [HttpGet]
        [Route("language/profile/{owner}/{lang_prof}/{text}")]
        public string ProfileCheck(string owner, string lang_prof, string text)
        {
            var path = System.IO.Path.Combine(_env.WebRootPath, "Content", "UserData", owner, lang_prof, (lang_prof ?? "").Substring(0, lang_prof.LastIndexOf('_')) + ".bin.gz");
            var lip = LanguageIdentifier.New(path, "Vector", -1);
            var cleaner = Cleaning.MakeCleaner("none");
            List<string> wrds = new List<string>();
            (text ?? "").Split('.', StringSplitOptions.RemoveEmptyEntries).ToList().ForEach(t => wrds.AddRange(t.Split(' ', StringSplitOptions.RemoveEmptyEntries)));
            return string.Join(',', wrds.Select(t => lip.Identify(cleaner(t))).Distinct());
        }

        [HttpPost]
        [Route("language/profile")]
        public List<Models.LangModel> ProfilePost()
        {
            string owner = HttpContext.Request.Form["owner"];
            string lang_prof = HttpContext.Request.Form["lang_prof"];
            var path = System.IO.Path.Combine(_env.WebRootPath, "Content", "UserData", owner, lang_prof, (lang_prof ?? "").Substring(0, lang_prof.LastIndexOf('_')) + ".bin.gz");
            var lip = LanguageIdentifier.New(path, "Vector", -1);
            string text = HttpContext.Request.Form["text"];
            var cleaner = Cleaning.MakeCleaner("none");
            List<string> wrds = new List<string>();
            (text ?? "").Split('.', StringSplitOptions.RemoveEmptyEntries).ToList().ForEach(t => wrds.AddRange(t.Split(' ', StringSplitOptions.RemoveEmptyEntries)));
            return wrds.Select(t => lip.Identify(cleaner(t))).GroupBy(t => t).Select(t1 => new Models.LangModel() { lang = t1.Key, count = t1.Count() }).ToList();
        }
    }
}
