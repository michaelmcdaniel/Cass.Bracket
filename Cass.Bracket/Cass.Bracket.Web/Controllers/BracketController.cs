using Microsoft.AspNetCore.Mvc;

namespace Cass.Bracket.Web.Controllers
{
    public class BracketController(BracketManager _brackets) : Controller
    {
        [Route("/bracket/{id?}")]
        public IActionResult Index(long? id = null)
        {
            Models.Bracket? model = id==null?new Models.Bracket() { Name="" } :_brackets.Get(id.Value);
            if (model == null) return NotFound();
            return View(model);
        }

        [Route("/bracket/vote/{id?}")]
        public IActionResult Vote(long id)
        {
            Models.Bracket? bracket = _brackets.Get(id);
            if (bracket == null) return NotFound();
            
            Models.Views.BracketVoteModel model = new Models.Views.BracketVoteModel()
            {
                Id = bracket.Id,
                Name = bracket.Name,
                Description = bracket.Description,
                Joined = bracket.Registered.Contains(User.Id()),
                Round = "Round 1",
                Matches = _brackets.GetBracketRound(id, 1)
            };
            return View(model);
        }
    }
}
