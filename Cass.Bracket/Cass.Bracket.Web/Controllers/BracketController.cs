using Microsoft.AspNetCore.Mvc;

namespace Cass.Bracket.Web.Controllers
{
    public class BracketController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult New()
        {
            return View("new");
        }
    }
}
