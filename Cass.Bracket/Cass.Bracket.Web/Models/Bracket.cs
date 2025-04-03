namespace Cass.Bracket.Web.Models
{
    public class Bracket
    {
        public long Id { get; set; } = 0;
        public required string Name { get; set; }

        public long UserId { get; set; } = 0;

        public List<string> Opponents { get; set; } = new List<string>();

        public DateTimeOffset Created { get; set; }

        public DateTimeOffset Cutoff { get; set; }

        public List<long> Registered { get; set; } = new List<long>();
    }

    public class Match
    {
        public int MatchNumber { get; set; } = 0;
        public int RoundNumber { get; set; } = 0;
        public int Opponent1 { get; set; } = -1;
        public int Opponent2 { get; set; } = -1;
        public int Winner { get; set; } = -1;
    }

    public class MatchVote
    {
        public long BracketId { get; set; }
        public int MatchNumber { get; set; } = 0;
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
    }
}
