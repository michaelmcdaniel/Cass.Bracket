using Cass.Bracket.Web.Models;
using Cass.Bracket.Web.Models.Views;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Cass.Bracket.Web.Controllers
{
    [ApiController]
    public class ApiController(UserManager _users, BracketManager _brackets) : ControllerBase
    {
        [HttpPost("/api/user/login")]
        public async Task<IActionResult> Signin([FromBody] AuthenticationModel model)
        {
            var user = _users.Signin(model.Username, model.Password);
            if (user == null) return new JsonResult(new { success=false, error = "Invalid username or password" });

            var identity = new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.Name, user!.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), new AuthenticationProperties { IsPersistent = model.RememberMe });
            return new JsonResult(new { success = true, error = "" });
        }

        [HttpPost("/api/user/create")]
        public async Task<IActionResult> Signup([FromBody] AuthenticationModel model)
        {
            try
            {

                var user = new Models.User()
                {
                    Email = model.Username,
                    Name = model.Name,
                    Password = model.Password
                };
                _users.Save(user);
                user = _users.Signin(model.Username, model.Password);
                if (user == null) new JsonResult(new { success = false, error = "Invalid username or password" });

                var identity = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.Name, user!.Name),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                }, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), new AuthenticationProperties { IsPersistent = model.RememberMe });

                return new JsonResult(new { success = true, error = "" });
            }
            catch (UsernameTakenException)
            {
                return new JsonResult(new { success = false, error = "Username is already taken" });
            }
        }

        [HttpPost("/api/bracket/create")]
        public async Task<IActionResult> SaveBracket([FromBody] Models.Bracket model)
        {
			try
            {
                if (model.Opponents.Count < 2) return new JsonResult(new { id = 0, success = false, error = "At least 2 teams are required" });
                _brackets.Save(User, model);
                return new JsonResult(new { id=model.Id, success = true, error = "" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { id = 0, success = false, error = ex.Message });
            }
        }
    }
}
