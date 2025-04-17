namespace Cass.Bracket.Web.Models
{
    public class BracketSearchResults
    {
        public long Total { get; set; }
        public List<BracketSearchResult> Brackets { get; set; } = new List<BracketSearchResult>();

        public int Page { get; set; }
        public int Size { get; set; }
    }

    public class BracketSearchResult
    {
        public long Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public bool Private { get; set; } = false;

        public bool Joined { get; set; } = false;

        public BracketStatus Status {  get; set; } = BracketStatus.Open;
	}
}
