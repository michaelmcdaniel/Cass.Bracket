namespace Cass.Bracket.Web.Models.Views
{
    public class BracketVoteModel
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool Joined { get; set; } = false;

        public int Round { get; set; } = 1;
        public IEnumerable<Models.Match> Matches { get; set; } = new Models.Match[0];
    }
}
