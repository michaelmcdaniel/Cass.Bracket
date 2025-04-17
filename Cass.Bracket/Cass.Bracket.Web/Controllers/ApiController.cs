using Cass.Bracket.Web.Models;
using Cass.Bracket.Web.Models.Api;
using Cass.Bracket.Web.Models.Views;
using mcdaniel.ws.AspNetCore.Authentication.SASToken;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace Cass.Bracket.Web.Controllers
{
	[ApiController]
    public class ApiController(UserManager _users, BracketManager _brackets, ILogger<ApiController> _logger) : ControllerBase
    {
        [HttpPost("/api/user/login")]
        public async Task<IActionResult> Signin([FromBody] AuthenticationModel model)
        {
            var user = _users.Signin(model.Username, model.Password);
            if (user == null) return new JsonResult(new { success=false, error = "Invalid username or password" });

                var claims = new List<Claim>(new[] {
                    new Claim(ClaimTypes.Name, user!.Name),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) });
                if (user.IsAdmin) claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
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
                var claims = new List<Claim>(new[] {
                    new Claim(ClaimTypes.Name, user!.Name),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) });
                if (user.IsAdmin) claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), new AuthenticationProperties { IsPersistent = model.RememberMe });

                return new JsonResult(new { success = true, error = "" });
            }
            catch (UsernameTakenException)
            {
                return new JsonResult(new { success = false, error = "Username is already taken" });
            }
        }

        [HttpPost("/api/user/update")]
        public async Task<IActionResult> UpdateAccount([FromBody] AccountModel model)
        {
            try
            {
                var existing = _users.Get(User.FindFirstValue(ClaimTypes.Email)!);
                var user = new Models.User()
                {
                    Id = existing!.Id,
                    Email = existing.Email,
                    Name = model.Name,
                    Password = model.Password??""
                };
                _users.Save(user);
                
                return new JsonResult(new { success = true, error = "" });
            }
            catch (UsernameTakenException)
            {
                return new JsonResult(new { success = false, error = "Username is already taken" });
            }
        }

        [HttpPost("/api/bracket/save")]
        public async Task<IActionResult> SaveBracket([FromBody] Models.Api.Bracket model)
        {
			try
            {
                if (model.Opponents.Count < 2) return new JsonResult(new { id = 0, success = false, error = "At least 2 teams are required" });
                Models.Bracket? existing = model.Id==0?null:_brackets.Get(model.Id);
                if (!(existing == null || existing?.UserId == User.Id() || User.IsInRole("Admin"))) return StatusCode(StatusCodes.Status403Forbidden);
                var bracket = new Models.Bracket()
                {
                    Id = model.Id,
                    MaxUsers = model.MaxUsers,
                    MinUsers = model.MinUsers,
                    Name = model.Name,
                    Description = model.Description??"",
                    Private = model.Private,
                    Opponents = model.Opponents.Select((o,i)=>new Opponent() {  Name = o, Rank = i + 1 }).ToList(),
                    Status = model.Publish?BracketStatus.Open:BracketStatus.Pending,
                    UserId = User.Id()
				};
                _brackets.Save(User,bracket);
                return new JsonResult(new { id=bracket.Id, locked=bracket.Status!=BracketStatus.Pending, success = true, error = "" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { id = 0, success = false, error = ex.Message });
            }
        }

        [HttpPost("/api/brackets/search/{page}")]
        public IActionResult Search([FromQuery]string q, int page = 0)
        {
            var search = BracketSearch.Parse(q);
            search.Size = 20;
            search.Page = page;
            search.UserId = User.Id();
            var results = _brackets.Search(search);
            return new JsonResult(results);
        }

        [HttpPost("/api/my/brackets/search/{page}")]
        public IActionResult SearchMy([FromQuery]string? q = null, int page = 0)
        {
            var search = BracketSearch.Parse(q);
            search.Size = 20;
            search.Page = page;
            search.OwnerId = User.Id();
            var results = _brackets.Search(search);
            return new JsonResult(results);
        }

        [HttpPost("/api/bracket/cast/{id}")]
        public IActionResult Cast(long id, [FromBody]CastVoteModel votes, [FromServices]mcdaniel.ws.AspNetCore.Authentication.SASToken.ISASTokenKeyStore tokenStore)
        {
            var bracket =  _brackets.Get(id);
            if (bracket == null) return NotFound();
            var userId = User.Id();
            bool alreadyJoined = bracket.Registered.Contains(userId);

			if (bracket.Status == BracketStatus.Complete) return BadRequest("Bracket is not open");

			if (!alreadyJoined && bracket.MaxUsers > 0 && bracket.Registered.Count >= bracket.MaxUsers) return BadRequest("Bracket is full");
			if (!alreadyJoined) _brackets.Join(User!, bracket);
            if (votes.Winners == null) return Ok();
            foreach(var vote in votes.Winners)
            {
                _brackets.Vote(User, new MatchVote()
                {
                    BracketId = id,
                    MatchId = vote.Key,
                    RoundNumber = votes.Round,
                    UserId = userId,
                    Winner = vote.Value,
                    CastAt = DateTimeOffset.UtcNow
				});
            }
            return new JsonResult(new { success=true, error="" });
		}

        
        [HttpGet("/api/bracket/join/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Join(long id, [FromServices]mcdaniel.ws.AspNetCore.Authentication.SASToken.ISASTokenKeyStore tokenStore)
        {
            var bracket =  _brackets.Get(id);
			if (bracket == null) return NotFound();
            var token = base.HttpContext.GetSASToken();
            if (!User?.Identity?.IsAuthenticated??false)
            {
                if (Request.Method == "POST") return StatusCode(StatusCodes.Status403Forbidden);
				return Redirect("/login?returnUrl=/api/bracket/join/"+id+"?" + token.ToString());
            }
            var userId = User!.Id();
            if (bracket.Private && bracket.UserId != userId)
            {
                var tokenKey = await tokenStore.GetAsync(token);
                if (!tokenKey?.Validate(token, HttpContext.Request, new string[] { id.ToString() }, null, null, _logger)??false) return StatusCode(StatusCodes.Status403Forbidden);
            }
			if (bracket.Status != BracketStatus.Open) return BadRequest("Bracket is not open");
			if (bracket.Registered.Contains(userId)) return BadRequest("You are already registered for this bracket");
			if (bracket.MaxUsers > 0 && bracket.Registered.Count >= bracket.MaxUsers) return BadRequest("Bracket is full");
			_brackets.Join(User!, bracket);
            if (Request.Method == "POST") return Ok();
			return Redirect("/");
		}

    }
}
