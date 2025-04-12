using System.ComponentModel.DataAnnotations;

namespace Cass.Bracket.Web.Models
{
    public class Bracket
    {
        public long Id { get; set; } = 0;

        [Required]
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
        public Match()
        { }

        public Match(int round, params MatchOpponent[] opponents)
        {
            Round = round;
            Opponents.AddRange(opponents);
        }
        public long Id { get; set; } = 0;
        public int Round { get; set; } = 0;
        public List<MatchOpponent> Opponents { get; set; } = new List<MatchOpponent>();
        public int Winner { get; set; } = -1;

        public int HighestRank { get=> Opponents.OrderBy(o=>o.Id).Last().Id; }

		public override string ToString()
		{
            string output = "";
            for(var i = 0; i < Opponents.Count; i++)
            {
                if (i > 0) output += " vs";
                output += $" {Opponents[i].ParentMatchId}.{Opponents[i].Id} {Opponents[i].Score:0} ";
                if (Opponents[i].Id == Winner) output += " (winner)";
            }
			return $"{Round}.{Id}: {output}";
		}
	}

    public class MatchOpponent
    {
        public int Id {get; set; } = 0;
        public double Score { get; set; } = 0.0;
        public long ParentMatchId { get; set;} = 0;
    }

    public class MatchVote
    {
        public long Id { get; set; } = 0;
		public long BracketId { get; set; }
        public long MatchId { get; set; } = 0;
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
