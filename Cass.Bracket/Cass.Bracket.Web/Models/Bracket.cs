namespace Cass.Bracket.Web.Models
{
    public class Bracket
    {
        public long Id { get; set; } = 0;
        public required string Name { get; set; }

        public long UserId { get; set; } = 0;

        public bool Private { get; set; } = false;

        public int MinUsers { get; set; } = 2;
        public int MaxUsers { get; set; } = 0;

		public List<string> Opponents { get; set; } = new List<string>();

        public DateTimeOffset Created { get; set; }

        public DateTimeOffset Cutoff { get; set; }

        public List<long> Registered { get; set; } = new List<long>();
    }

    public class Match
    {
        public int Id { get; set; } = 0;
        public int Round { get; set; } = 0;
        public MatchOpponent Opponent1 { get; set; } = new MatchOpponent();
        public MatchOpponent Opponent2 { get; set; } = new MatchOpponent();
        public int Winner { get; set; } = -1;

		public override string ToString()
		{
            string o1w = Winner==Opponent1.Id?"(winner)":"";
            string o2w = Winner==Opponent2.Id?"(winner)":"";
			return $"{Round}.{Id}: {Opponent1.ParentMatchId}.{Opponent1.Id} {Opponent1.Score:0} {o1w} vs {Opponent2.ParentMatchId}.{Opponent2.Id} {Opponent2.Score:0} {o2w}";
		}
	}

    public class MatchOpponent
    {
        public int Id {get; set; } = 0;
        public double Score { get; set; } = 0.0;
        public int ParentMatchId { get; set;} = 0;
    }

    public class MatchVote
    {
        public long Id { get; set; } = 0;
		public long BracketId { get; set; }
        public int MatchId { get; set; } = 0;
        public int RoundNumber { get; set; } = 0;
        public int Winner { get; set; } = -1;
        public long UserId { get; set; } = 0;
        public DateTimeOffset CastAt { get; set; } = DateTimeOffset.MinValue;
    }

    public class Round
    {
        public int RoundNumber { get; set; } = 0;

        public int Points { get; set; } = 1;

        public bool Completed { get; set; } = false;

        public List<Match> Matches { get; set; } = new List<Match>();

        public Round? Next { get; set; } = null;

        public int Quadrant { get; set; } = 0;

		public override string ToString()
		{
			return $"{RoundNumber} (+{Points}) {Matches.Count} matches" + (Completed ? " COMPLETE" : "") + 
               (Quadrant > 0 ? $" [Quadrant {Quadrant}]" : "");
		}
	}
}
