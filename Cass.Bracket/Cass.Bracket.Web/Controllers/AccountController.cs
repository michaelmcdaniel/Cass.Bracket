using Microsoft.AspNetCore.Mvc;

namespace Cass.Bracket.Web.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
