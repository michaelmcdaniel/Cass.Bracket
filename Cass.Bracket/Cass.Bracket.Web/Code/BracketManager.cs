using Microsoft.Extensions.Options;
using System.Security.Claims;
using Cass.Bracket.Web.Models;
using mcZen.Data;
using Microsoft.Data.SqlClient;

namespace Cass.Bracket.Web
{
	public class BracketManager(IOptions<BracketManager.Options> _options, ILogger<BracketManager> _logger)
	{

		public static Round GenerateRounds(Models.Bracket bracket)
		{
            int teamCount = bracket.Opponents.Count;
            if (teamCount < 2)
                throw new ArgumentException("Bracket must have at least 2 opponents.");

            var allMatches = new List<Match>();
            var matchIdCounter = 1;
            var roundMap = new Dictionary<int, List<Match>>();

            // Step 1: Create initial queue of seeds as players
            var queue = new Queue<(int Id, bool IsSeed)>();
            foreach (int seed in Enumerable.Range(1, teamCount))
                queue.Enqueue((seed, true));

            int round = 1;

            while (queue.Count > 1)
            {
                var roundMatches = new List<Match>();
                var nextQueue = new Queue<(int Id, bool IsSeed)>();

                while (queue.Count > 1)
                {
                    var p1 = queue.Dequeue();
                    var p2 = queue.Dequeue();

                    var match = new Match
                    {
                        Id = matchIdCounter++,
                        Round = round,
                        Opponent1 = new MatchOpponent
                        {
                            Id = p1.IsSeed ? p1.Id : 0,
                            ParentMatchId = p1.IsSeed ? 0 : p1.Id,
                            Score = 0
                        },
                        Opponent2 = new MatchOpponent
                        {
                            Id = p2.IsSeed ? p2.Id : 0,
                            ParentMatchId = p2.IsSeed ? 0 : p2.Id,
                            Score = 0
                        },
                        Winner = -1
                    };

                    roundMatches.Add(match);
                    allMatches.Add(match);
                    nextQueue.Enqueue((match.Id, false)); // Next round will reference this match
                }

                // Handle bye (odd participant)
                if (queue.Count == 1)
                {
                    var bye = queue.Dequeue();
                    nextQueue.Enqueue(bye); // They advance automatically
                }

                roundMap[round] = roundMatches;
                queue = nextQueue;
                round++;
            }

            // Step 2: Build linked Round objects
            Round? next = null;
            foreach (var r in roundMap.OrderByDescending(r => r.Key))
            {
                var roundObj = new Round
                {
                    RoundNumber = r.Key,
                    Points = (int)Math.Pow(2, r.Key - 1),
                    Matches = r.Value,
                    Completed = false,
                    Next = next,
                    Quadrant = 0
                };
                next = roundObj;
            }

            return next!;
		}

		public bool Save(ClaimsPrincipal user, Models.Bracket bracket)
		{
			if (user.Identity == null || !user.Identity.IsAuthenticated) { throw new UnauthorizedAccessException(); }
			var userId = long.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
			ConnectionFactory factory = new ConnectionFactory(_options.Value.ConnectionString);
			if (bracket.Id == 0)
			{
				factory.Register(Commands.Insert<long>((id) => { bracket.Id = id; }, "Bracket", "Id",
					new SqlParameter("@Name", bracket.Name),
					new SqlParameter("@UserId", userId),
					new SqlParameter("@Private", bracket.Private),
					new SqlParameter("@MinUsers", bracket.MinUsers),
					new SqlParameter("@MaxUsers", bracket.MaxUsers),
					new SqlParameter("@Cutoff", bracket.Cutoff)));


			}
			else
			{
				factory.Register(Commands.Update("Bracket",
					new SqlParameter("@Id", bracket.Id),
					new SqlParameter("@Name", bracket.Name),
					new SqlParameter("@UserId", userId),
					new SqlParameter("@Private", bracket.Private),
					new SqlParameter("@MinUsers", bracket.MinUsers),
					new SqlParameter("@MaxUsers", bracket.MaxUsers),
					new SqlParameter("@Cutoff", bracket.Cutoff)));
			}


			factory.Execute();
			return true;
		}

		public bool Join(ClaimsPrincipal user, Models.Bracket bracket)
		{
			if (user.Identity == null || !user.Identity.IsAuthenticated) { throw new UnauthorizedAccessException(); }
			var userId = long.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

			ConnectionFactory.Execute(Commands.Insert("BracketParticipant",
				new SqlParameter("@BracketId", bracket.Id),
				new SqlParameter("@UserId", userId)),
				_options.Value.ConnectionString);
			return true;
		}

		public bool Vote(ClaimsPrincipal user, Models.MatchVote vote)
		{
			if (user.Identity == null || !user.Identity.IsAuthenticated) { throw new UnauthorizedAccessException(); }
			var userId = long.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
			// Check if the match is already closed.

			ConnectionFactory factory = new ConnectionFactory(_options.Value.ConnectionString);
			ScalarCommand<bool>? isComplete = null;
			string isCompleteQuery = "SELECT 1 FROM BracketMatch WHERE BracketId=@BracketId AND Complete <= sysdatetimeoffset()";
			factory.Register(new CommandReader(
				r =>
				{
					long Opponent1Id = r.GetInt64(0);
					long Opponent2Id = r.GetInt64(1);
					int maxUsers = r.GetInt32(3);
					DateTimeOffset? completed = r.IsDBNull(2) ? null : r.GetDateTimeOffset(2);
					if (completed != null && completed <= DateTimeOffset.Now) throw new InvalidOperationException("Match is already complete.");
					if (!(Opponent1Id == vote.Winner || Opponent2Id == vote.Winner)) throw new InvalidOperationException("Invalid winner.");
					string column = Opponent1Id == vote.Winner ? "Opponent1Id" : "Opponent2Id";
					if (vote.Id == 0) // new vote
					{
						factory.Register(Commands.Insert<long>((id) => { vote.Id = id; }, "BracketMatchVote", "Id",
							new SqlParameter("@BracketId", vote.BracketId),
							new SqlParameter("@MatchId", vote.MatchId),
							//new SqlParameter("@RoundNumber", vote.RoundNumber),
							new SqlParameter("@OpponentId", vote.Winner),
							new SqlParameter("@UserId", userId)));

						factory.Register(new Command(
							$"UPDATE BracketMatch SET {column}={column}+1, Complete=CASE WHEN (Opponent1Score+Opponent2Score+1 = @AllowedVotes) THEN sysdatetimeoffset() ELSE NULL END  WHERE Id=@Id AND Complete IS NULL AND (Cutoff IS NULL OR Cutoff < sysdatetimeoffset())",
							new SqlParameter("@Id", vote.MatchId), new SqlParameter("@AllowedVotes", maxUsers))
						);

						factory.Register(isComplete = new ScalarCommand<bool>(isCompleteQuery, new SqlParameter("@BracketId", vote.BracketId)));
					}
					else // existing vote change.
					{
						long existingVote = -1;
						string? decrementColumn = null;
						factory.Register(new CommandReader(
							r =>
							{
								existingVote = r.GetInt64(0);
								if (existingVote != vote.Winner) { decrementColumn = existingVote == Opponent1Id ? "Opponent1Id" : "Opponent2Id"; }
								string column = Opponent1Id == vote.Winner ? "Opponent1Id" : "Opponent2Id";

								factory.Register(Commands.Update("BracketMatchVote",
									new SqlParameter("@Id", vote.Id),
									new SqlParameter("@BracketId", vote.BracketId),
									new SqlParameter("@MatchId", vote.MatchId),
									//new SqlParameter("@RoundNumber", vote.RoundNumber),
									new SqlParameter("@OpponentId", vote.Winner),
									new SqlParameter("@UserId", userId)));


								string incrementVotesQuery = $"UPDATE BracketMatch SET {column}={column}+1 ";
								if (decrementColumn != null)
								{
									incrementVotesQuery += $", {decrementColumn}={decrementColumn}-1 ";
								}
								else
								{
									incrementVotesQuery += ", Complete=CASE WHEN (Opponent1Score+Opponent2Score+1 = @AllowedVotes) THEN sysdatetimeoffset() ELSE NULL END";
								}

								incrementVotesQuery += $" WHERE Id=@Id AND Complete IS NULL AND (Cutoff IS NULL OR Cutoff < sysdatetimeoffset())";
								factory.Register(new Command(incrementVotesQuery, System.Data.CommandType.Text, _options.Value.Timeout, new SqlParameter("@Id", vote.MatchId), new SqlParameter("@AllowedVotes", maxUsers)));
								factory.Register(isComplete = new ScalarCommand<bool>(isCompleteQuery, new SqlParameter("@BracketId", vote.BracketId)));

								return false;
							}, "SELECT OpponentId from BracketMatchVote WHERE Id=@Id", System.Data.CommandType.Text, _options.Value.Timeout,
							new SqlParameter("@Id", vote.Id)
						));
					}
					return false;
				}, "SELECT Opponent1Id, Opponent2Id, Complete, MaxUsers FROM BracketMatch JOIN Bracket ON Bracket.Id=BracketMatch.BracketId WHERE BracketMatch.Id=@MatchId AND BracketMatch.BracketId=@BracketId AND EXISTS(SELECT * FROM BracketParticipant WHERE BracketId=@BracketId AND UserId=@UserId)=1",
				System.Data.CommandType.Text, _options.Value.Timeout,
				new SqlParameter("@MatchId", vote.MatchId),
				new SqlParameter("@BracketId", vote.BracketId),
				new SqlParameter("@UserId", userId))
			);

			factory.Execute();
			return isComplete?.Value ?? false;
		}

		public class Options
		{
			public string ConnectionString { get; set; } = "";
			public int Timeout { get; set; } = 30;
		}
	}
}
