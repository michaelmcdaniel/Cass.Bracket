using System.ComponentModel.DataAnnotations;

namespace Cass.Bracket.Web.Models
{
    public class Bracket
    {
        public long Id { get; set; } = 0;

        [Required]
        public required string Name { get; set; }

        public string Description { get; set; }= string.Empty;

        public long UserId { get; set; } = 0;

        public bool Private { get; set; } = false;

        public int MinUsers { get; set; } = 2;
        public int MaxUsers { get; set; } = 0;

		public List<Opponent> Opponents { get; set; } = new List<Opponent>();

        public DateTimeOffset Created { get; set; }

        public DateTimeOffset Cutoff { get; set; }

        public BracketStatus Status { get; set; } = BracketStatus.Pending;

		public List<long> Registered { get; set; } = new List<long>();
    }

    public class Opponent
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public int Rank { get; set;}

		public override string ToString()
		{
			return $"#{Rank} {Name} ({Id})";
		}
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
        public long Winner { get; set; } = -1;

        public int HighestRank { get=> Opponents.Where(o=>o.Opponent != null).OrderBy(o=>o.Opponent!.Rank).LastOrDefault()?.Opponent?.Rank??0; }

		public override string ToString()
		{
            string output = "";
            for(var i = 0; i < Opponents.Count; i++)
            {
                if (i > 0) output += " vs";
                output += $" {Opponents[i].ParentMatchId}.{Opponents[i]} {Opponents[i].Score:0} ";
                if (Opponents[i].Opponent!.Id == Winner) output += " (winner)";
            }
			return $"{Round}.{Id}: {output}";
		}
	}

    public class MatchOpponent
    {
        public Opponent? Opponent { get; set; }
        public double Score { get; set; } = 0.0;
        public long ParentMatchId { get; set;} = 0;
    }

    public class MatchVote
    {
        public long Id { get; set; } = 0;
		public long BracketId { get; set; }
        public long MatchId { get; set; } = 0;
        public int RoundNumber { get; set; } = 0;
        public long Winner { get; set; } = -1;
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
