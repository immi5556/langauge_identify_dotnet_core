using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Immanuel.Core.Language.Controllers
{
    public class HomeController : Controller
    {
        Microsoft.AspNetCore.Hosting.IHostingEnvironment _env;
        Microsoft.AspNetCore.SignalR.IHubContext<Hubs.ProgressHub> _hub;

        public HomeController(Microsoft.AspNetCore.Hosting.IHostingEnvironment env, Microsoft.AspNetCore.SignalR.IHubContext<Hubs.ProgressHub> hub)
        {
            _env = env;
            _hub = hub;
        }

        public IActionResult Index()
        {
            ViewBag.LstItems = Bll.Util.GetLangProfiles(_env);
            return View();
        }
    }
}
