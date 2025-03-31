using System.Diagnostics;
using Cass.Bracket.Web.Models;
using Cass.Bracket.Web.Models.Views;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cass.Bracket.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (!(User.Identity?.IsAuthenticated??false)) return RedirectToAction("Login");
            return View();
        }

        [AllowAnonymous]
        //[Route("/login")]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        //[Route("/logout")]
        public IActionResult Logout()
        {
            return new SignOutResult(CookieAuthenticationDefaults.AuthenticationScheme, new Microsoft.AspNetCore.Authentication.AuthenticationProperties() {  RedirectUri = "/"});
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [AllowAnonymous]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
