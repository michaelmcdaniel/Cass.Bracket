namespace Cass.Bracket.Web.Models.Views
{
    public class BracketVoteModel
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool Joined { get; set; } = false;

        public string Round { get; set; } = string.Empty;
        public IEnumerable<Models.Match> Matches { get; set; } = new Models.Match[0];
    }
}
