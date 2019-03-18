using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Immanuel.Core.Language.Bll
{
    public class Util
    {
        public static List<Models.Owner> GetLangProfiles(Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            List<Models.Owner> ownlst = new List<Models.Owner>();
            var path = System.IO.Path.Combine(env.WebRootPath, "Content", "UserData");
            foreach (var v in System.IO.Directory.EnumerateDirectories(path))
            {
                var own = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.Owner>(System.IO.File.ReadAllText(System.IO.Path.Combine(v, "_rootuser.json")));
                foreach (var v1 in System.IO.Directory.EnumerateDirectories(v))
                {
                    var lng = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.LangProfile>(System.IO.File.ReadAllText(System.IO.Path.Combine(v1, "_langprofile.json")));
                    own.langProfiles = own.langProfiles ?? new List<Models.LangProfile>();
                    own.langProfiles.Add(lng);
                }
                ownlst.Add(own);
                own.langProfiles.Sort((a, b) => a.CreatedAt >= b.CreatedAt ? 1 : -1);
            }
            return ownlst;
        }

        public static Models.Owner CreateOrGetOwner(Microsoft.AspNetCore.Hosting.IHostingEnvironment env, Models.Owner owner)
        {
            var dosname = string.Join("_", (owner.Email ?? "").Split(System.IO.Path.GetInvalidFileNameChars()));
            var path = System.IO.Path.Combine(env.WebRootPath, "Content", "UserData", dosname);
            var fpath = System.IO.Path.Combine(path, "_rootuser.json");
            if (System.IO.Directory.Exists(path))
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<Models.Owner>(System.IO.File.ReadAllText(fpath));
                //owner.AccessedAt = DateTime.UtcNow;
                //System.IO.File.WriteAllText(fpath, Newtonsoft.Json.JsonConvert.SerializeObject(owner));
            }
            else
            {
                System.IO.Directory.CreateDirectory(path);
                owner.Path = path;
                owner.DosName = dosname;
                owner.CreatedAt = DateTime.UtcNow;
                owner.AccessedAt = DateTime.UtcNow;
                System.IO.File.WriteAllText(fpath, Newtonsoft.Json.JsonConvert.SerializeObject(owner));
            }

            return owner;
        }



        public static void CreateLangProfile(HttpContext ctx, Models.Owner owner)
        {
            var prof = owner.langProfiles[0];
            var path = System.IO.Path.Combine(owner.Path, $"{prof.ProfileName}_{DateTime.Now.ToString("yyyyMMMddHHmmss")}");
            var fpath = System.IO.Path.Combine(path, "_langprofile.json");
            System.IO.Directory.CreateDirectory(path);
            string keyidx = ctx.Request.Form["keyidxs"];
            var lst = Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(keyidx);
            lst.ForEach(t =>
            {
                string inputtype = ctx.Request.Form["inptdata" + t.ToString()];
                string inputlabel = ctx.Request.Form["inptlabel" + t.ToString()];
                if (inputtype == "filesrc")
                {
                    Microsoft.AspNetCore.Http.IFormFile fll = ctx.Request.Form.Files["uplfile" + t.ToString()];
                    string llpath = System.IO.Path.Combine(path, "LangData", "upl_File_" + (inputlabel ?? "") + "_" + fll.FileName);
                    if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(llpath)))
                        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(llpath));
                    using (var stream = new System.IO.FileStream(llpath, System.IO.FileMode.Create))
                    {
                        fll.CopyTo(stream);
                    }
                    prof.Files.Add(new Models.LabelInput()
                    {
                        Label = inputlabel,
                        FilePath = llpath
                    });
                }
                else if (inputtype == "textsrc")
                {
                    string txt = ctx.Request.Form["upltext" + t.ToString()];
                    string llpath = System.IO.Path.Combine(path, "LangData", "upl_Text_" + (inputlabel ?? "") + "_" + Guid.NewGuid().ToString("n").Substring(0, 4) + ".txt");
                    if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(llpath)))
                        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(llpath));
                    System.IO.File.WriteAllText(llpath, txt);
                    prof.Files.Add(new Models.LabelInput()
                    {
                        Label = inputlabel,
                        FilePath = llpath
                    });
                }
            });

            prof.Path = path;
            prof.ProfileFilePath = System.IO.Path.Combine(prof.Path, prof.ProfileName + ".bin.gz");
            prof.CreatedAt = DateTime.UtcNow;
            prof.AccessedAt = DateTime.UtcNow;
            System.IO.File.WriteAllText(fpath, Newtonsoft.Json.JsonConvert.SerializeObject(prof));
        }
    }
}
