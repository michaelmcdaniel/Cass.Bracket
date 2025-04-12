using Microsoft.Extensions.Options;
using System.Security.Claims;
using Cass.Bracket.Web.Models;
using mcZen.Data;
using Microsoft.Data.SqlClient;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Cass.Bracket.Web
{
	public class BracketManager(IOptions<BracketManager.Options> _options, ILogger<BracketManager> _logger)
	{

		public static List<Match> GenerateRound(IEnumerable<MatchOpponent> opponents)
		{
			var cop = opponents.ToList();
            int perfectBracket = 0, previousPerfectBracket = 0;
            for (var i = 2; perfectBracket < cop.Count; i*=2) { previousPerfectBracket = perfectBracket; perfectBracket=i; }
            var teamsWithBye = new List<MatchOpponent>();
            var remainingTeams = new List<MatchOpponent>();
            List<Match> matches = new List<Match>();
            int numberOfTeamsWithBye = Math.Max(0,previousPerfectBracket - (cop.Count - previousPerfectBracket));
            int numberOfRemainingTeamsInFirstRound = cop.Count-numberOfTeamsWithBye;
            for (var i = 0; i < numberOfTeamsWithBye; i++) teamsWithBye.Add(cop[i]);
            for (var i = numberOfTeamsWithBye; i < cop.Count; i++) remainingTeams.Add(cop[i]);
            while (teamsWithBye.Count > 0 && remainingTeams.Count > 0) { matches.Add(new Match(1, teamsWithBye.Shift(), remainingTeams.Pop(), remainingTeams.Pop())); }
            while (remainingTeams.Count > 0) { matches.Add(new Match(1, remainingTeams.Shift(), remainingTeams.Pop())); }
            while (teamsWithBye.Count > 0) { matches.Add(new Match(1, teamsWithBye.Shift(), teamsWithBye.Pop())); }
            // reorganize
            matches.Sort((a,b)=>a.HighestRank-b.HighestRank);
			return matches;
		}

		public bool Save(ClaimsPrincipal user, Models.Bracket bracket)
		{
			if (user.Identity == null || !user.Identity.IsAuthenticated) { throw new UnauthorizedAccessException(); }
			var userId = long.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
			ConnectionFactory factory = new ConnectionFactory(_options.Value.ConnectionString);
			if (bracket.Id == 0)
			{
				factory.Register(Commands.Insert<long>(
					(id) => { 
						bracket.Id = id; 
						int i = 0;
						foreach(var match in GenerateRound(bracket.Opponents.Select((o,i) => new MatchOpponent() { Id=i })))
						{
							if (match.Opponents.Count < 1 || match.Opponents.Count>3) throw new ArgumentOutOfRangeException("Round creation error.");
							List<SqlParameter> parameters = new List<SqlParameter>();
							parameters.Add(new SqlParameter("@Round", 1));
							parameters.Add(new SqlParameter("@MatchIndex", i));
							parameters.Add(new SqlParameter("@BracketId", id));
							for(int mi = 1; mi <= match.Opponents.Count; mi++)
							{
								parameters.Add(new SqlParameter($"@Opponent{mi}Id", match.Opponents[mi-1].Id));
							}
							factory.Register(Commands.Insert<long>((mId) => { match.Id=mId; }, "BracketMatch", "Id", parameters.ToArray()));
						}
					}, "Bracket", "Id",
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

				factory.Register(new CommandReader(
					(r) => {
						if (r.GetInt32(0) != bracket.Opponents.Count)
						{
							factory.Register(new Command("DELETE FROM BracketMatch WHERE BracketId=@BracketId", new SqlParameter("@BracketId", bracket.Id)));
							int i = 0;
							foreach(var match in GenerateRound(bracket.Opponents.Select((o,i) => new MatchOpponent() { Id=i })))
							{
								if (match.Opponents.Count < 1 || match.Opponents.Count>3) throw new ArgumentOutOfRangeException("Round creation error.");
								List<SqlParameter> parameters = new List<SqlParameter>();
								parameters.Add(new SqlParameter("@Round", 1));
								parameters.Add(new SqlParameter("@MatchIndex", i));
								parameters.Add(new SqlParameter("@BracketId", bracket.Id));
								for(int mi = 1; mi <= match.Opponents.Count; mi++)
								{
									parameters.Add(new SqlParameter($"@Opponent{mi}Id", match.Opponents[mi-1].Id));
								}
								factory.Register(Commands.Insert<long>((mId) => { match.Id=mId; }, "BracketMatch", "Id", parameters.ToArray()));
							}
						}
						return true;
					}, "SELECT SUM((CASE WHEN Opponent1Id IS NULL THEN 0 ELSE 1 END) + (CASE WHEN Opponent2Id IS NULL THEN 0 ELSE 1 END) + (CASE WHEN Opponent3Id IS NULL THEN 0 ELSE 1 END)) [sum] FROM BracketMatch (nolock) WHERE BracketId=@BracketId AND Round = 1", new SqlParameter("@BracketId", bracket.Id)));
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
					long Opponent2Id = r.IsDBNull(1)? -1 : r.GetInt64(1);
					long Opponent3Id = r.IsDBNull(2)? -1 : r.GetInt64(2);
					int maxUsers = r.GetInt32(4);
					DateTimeOffset? completed = r.IsDBNull(3) ? null : r.GetDateTimeOffset(3);
					if (completed != null && completed <= DateTimeOffset.Now) throw new InvalidOperationException("Match is already complete.");
					if (!(Opponent1Id == vote.Winner || Opponent2Id == vote.Winner)) throw new InvalidOperationException("Invalid winner.");
					string column = Opponent1Id == vote.Winner ? "Opponent1Id" : Opponent2Id == vote.Winner ? "Opponent2Id" : "Opponent3Id";
					if (vote.Id == 0) // new vote
					{
						factory.Register(Commands.Insert<long>((id) => { vote.Id = id; }, "BracketMatchVote", "Id",
							new SqlParameter("@BracketId", vote.BracketId),
							new SqlParameter("@MatchId", vote.MatchId),
							//new SqlParameter("@RoundNumber", vote.RoundNumber),
							new SqlParameter("@OpponentId", vote.Winner),
							new SqlParameter("@UserId", userId)));

						factory.Register(new Command(
							$"UPDATE BracketMatch SET {column}={column}+1, Complete=CASE WHEN (ISNULL(Opponent1Score,0)+ISNULL(Opponent2Score,0)+ISNULL(Opponent3Score,0)+1 = @AllowedVotes) THEN sysdatetimeoffset() ELSE NULL END  WHERE Id=@Id AND Complete IS NULL AND (Cutoff IS NULL OR Cutoff < sysdatetimeoffset())",
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
								if (existingVote != vote.Winner) { decrementColumn = existingVote == Opponent1Id ? "Opponent1Id" : existingVote == Opponent2Id ? "Opponent2Id" : "Opponent3Id"; }
								string column = Opponent1Id == vote.Winner ? "Opponent1Id" : Opponent2Id == vote.Winner ? "Opponent2Id" : "Opponent3Id";

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
									incrementVotesQuery += ", Complete=CASE WHEN (ISNULL(Opponent1Score,0)+ISNULL(Opponent2Score,0)+ISNULL(Opponent3Score,0)+1 = @AllowedVotes) THEN sysdatetimeoffset() ELSE NULL END";
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
				}, "SELECT Opponent1Id, Opponent2Id, Opponent3Id, Complete, MaxUsers FROM BracketMatch JOIN Bracket ON Bracket.Id=BracketMatch.BracketId WHERE BracketMatch.Id=@MatchId AND BracketMatch.BracketId=@BracketId AND EXISTS(SELECT * FROM BracketParticipant WHERE BracketId=@BracketId AND UserId=@UserId)=1",
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
			public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
		}
	}
}
