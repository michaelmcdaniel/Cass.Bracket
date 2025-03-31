using Cass.Bracket.Web.Models.Views;
using Microsoft.AspNetCore.Mvc;

namespace Cass.Bracket.Web.Controllers
{
    [ApiController]
    public class ApiController : ControllerBase
    {
        [HttpPost("/api/signin")]
        public async Task<IActionResult> Signin([FromBody] AuthenticationModel model)
        {
            Microsoft.AspNetCore.Authentication.Cookies.
        }

        [HttpPost("/api/signup")]
        public async Task<IActionResult> Signup([FromBody] AuthenticationModel model)
        {

        }
    }
}
