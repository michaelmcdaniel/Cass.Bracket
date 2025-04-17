using Microsoft.Extensions.Options;
using System.Security.Claims;
using Cass.Bracket.Web.Models;
using mcZen.Data;
using Microsoft.Data.SqlClient;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text;

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
						Action processRound = () =>
						{
							foreach(var match in GenerateRound(bracket.Opponents.Select((o,i) => new MatchOpponent() { Opponent=o })))
							{
								if (match.Opponents.Count < 1 || match.Opponents.Count>3) throw new ArgumentOutOfRangeException("Round creation error.");
								List<SqlParameter> parameters = new List<SqlParameter>();
								parameters.Add(new SqlParameter("@Round", 1));
								parameters.Add(new SqlParameter("@MatchIndex", i));
								parameters.Add(new SqlParameter("@BracketId", id));
								for(int mi = 1; mi <= match.Opponents.Count; mi++)
								{
									parameters.Add(new SqlParameter($"@Opponent{mi}Id", match.Opponents[mi-1].Opponent!.Id));
								}
								factory.Register(Commands.Insert<long>((mId) => { match.Id=mId; }, "BracketMatch", "Id", parameters.ToArray()));
							}
						};

						for(int oi = 0; oi < bracket.Opponents.Count; oi++)
						{
							bool last = oi == bracket.Opponents.Count-1;
							var opponent = bracket.Opponents[oi];
							factory.Register(Commands.Insert<long>(
								(oid) => { 
									opponent.Id=oid;
									if (last) processRound();
								}, "BracketOpponents", "Id", 
								new SqlParameter("@BracketId", bracket.Id),
								new SqlParameter("@Name", opponent.Name),
								new SqlParameter("@Url", opponent.Url),
								new SqlParameter("@Rank", oi)));
						}
						
					}, "Bracket", "Id",
					new SqlParameter("@Name", bracket.Name),
					new SqlParameter("@Description", bracket.Description),
					new SqlParameter("@UserId", userId),
					new SqlParameter("@Private", bracket.Private),
					new SqlParameter("@MinUsers", bracket.MinUsers),
					new SqlParameter("@MaxUsers", bracket.MaxUsers),
					new SqlParameter("@Status", (byte)bracket.Status),
					new SqlParameter("@Cutoff", bracket.Cutoff)));

				
			}
			else
			{
				factory.Register(Commands.Update("Bracket",
					new SqlParameter("@Id", bracket.Id),
					new SqlParameter("@Name", bracket.Name),
					new SqlParameter("@Description", bracket.Description),
					new SqlParameter("@UserId", userId),
					new SqlParameter("@Private", bracket.Private),
					new SqlParameter("@MinUsers", bracket.MinUsers),
					new SqlParameter("@MaxUsers", bracket.MaxUsers),
					new SqlParameter("@Status", (byte)bracket.Status),
					new SqlParameter("@Cutoff", bracket.Cutoff)));

				factory.Register(new CommandReader(
					(r) => {
						if (r.GetInt32(0) != bracket.Opponents.Count)
						{
							factory.Register(new Command("DELETE FROM BracketMatch WHERE BracketId=@BracketId", new SqlParameter("@BracketId", bracket.Id)));
							factory.Register(new Command("DELETE FROM BracketOpponents WHERE BracketId=@BracketId", new SqlParameter("@BracketId", bracket.Id)));

							Action processRound = () =>
							{
								int i = 0;
								foreach(var match in GenerateRound(bracket.Opponents.Select((o,i) => new MatchOpponent() { Opponent=o })))
								{
									if (match.Opponents.Count < 1 || match.Opponents.Count>3) throw new ArgumentOutOfRangeException("Round creation error.");
									List<SqlParameter> parameters = new List<SqlParameter>();
									parameters.Add(new SqlParameter("@Round", 1));
									parameters.Add(new SqlParameter("@MatchIndex", i++));
									parameters.Add(new SqlParameter("@BracketId", bracket.Id));
									for(int mi = 1; mi <= match.Opponents.Count; mi++)
									{
										parameters.Add(new SqlParameter($"@Opponent{mi}Id", match.Opponents[mi-1].Opponent!.Id));
									}
									factory.Register(Commands.Insert<long>((mId) => { match.Id=mId; }, "BracketMatch", "Id", parameters.ToArray()));
								}
							};
							for(int oi = 0; oi < bracket.Opponents.Count; oi++)
							{
								bool last = oi == bracket.Opponents.Count-1;
								var opponent = bracket.Opponents[oi];
								factory.Register(Commands.Insert<long>(
									(oid) => { 
										opponent.Id=oid; 
										if (last) processRound();
									}, "BracketOpponents", "Id", 
									new SqlParameter("@BracketId", bracket.Id),
									new SqlParameter("@Name", opponent.Name),
									new SqlParameter("@Url", opponent.Url),
									new SqlParameter("@Rank", oi)));
							}

						}
						return true;
					}, "SELECT SUM((CASE WHEN Opponent1Id IS NULL THEN 0 ELSE 1 END) + (CASE WHEN Opponent2Id IS NULL THEN 0 ELSE 1 END) + (CASE WHEN Opponent3Id IS NULL THEN 0 ELSE 1 END)) [sum] FROM BracketMatch (nolock) WHERE BracketId=@BracketId AND Round = 1", new SqlParameter("@BracketId", bracket.Id)));
			}


			factory.Execute();
			return true;
		}

		public IEnumerable<Models.Match> GetBracketRound(long id, int round)
		{
			ConnectionFactory factory = new ConnectionFactory(_options.Value.ConnectionString);
			List<Models.Match> retVal = new List<Match>();
			var bracket = Get(id);
			if (bracket == null) return retVal;
			factory.Register(new CommandReader(
				(r)=> {
					List<Models.MatchOpponent> opponents = new List<MatchOpponent>();
					Opponent? op = null;
					if ((op = bracket.Opponents.FirstOrDefault(o=>o.Id == r.GetValue<long>("Opponent1Id",0)))!=null)
					{
						opponents.Add(new MatchOpponent()
						{
							Opponent = op,
							ParentMatchId = r.GetValue<long>("Opponent1ParentMatchId",0),
							Score = r.GetValue<double>("Opponent1Score",0.0)
						});
					}
					if ((op = bracket.Opponents.FirstOrDefault(o=>o.Id == r.GetValue<long>("Opponent2Id",0)))!=null)
					{
						opponents.Add(new MatchOpponent()
						{
							Opponent = op,
							ParentMatchId = r.GetValue<long>("Opponent2ParentMatchId",0),
							Score = r.GetValue<double>("Opponent2Score",0.0)
						});
					}
					if ((op = bracket.Opponents.FirstOrDefault(o=>o.Id == r.GetValue<long>("Opponent3Id",0)))!=null)
					{
						opponents.Add(new MatchOpponent()
						{
							Opponent = op,
							ParentMatchId = r.GetValue<long>("Opponent3ParentMatchId",0),
							Score = r.GetValue<double>("Opponent3Score",0.0)
						});
					}

					var match = new Match()
					{
						Id = r.GetValue<long>("Id", 0),
						Round = r.GetValue("Round", 0),
						Opponents = opponents
					};
					retVal.Add(match);
					return true;
				}, 
				"SELECT * FROM BracketMatch WHERE BracketId=@BracketId AND Round=@Round", 
				new SqlParameter("@BracketId", id), 
				new SqlParameter("@Round", round)
			));
			factory.Execute();
			return retVal;
		}

		public Models.Bracket? Get(long id)
		{
			Models.Bracket? retVal = null;
			ConnectionFactory factory = new ConnectionFactory(_options.Value.ConnectionString);
			factory.Register(new CommandReader(
				(r) =>
				{
					retVal = new Models.Bracket()
					{
						Id = r.GetInt64(0),
						Name = r.GetString(1),
						UserId = r.GetInt32(2),
						Private = r.GetBoolean(3),
						Status = (BracketStatus)r.GetByte(4),
						Cutoff = r.IsDBNull(5) ? DateTimeOffset.MaxValue : r.GetDateTimeOffset(5),
						MinUsers = r.GetInt16(6),
						MaxUsers = r.GetInt16(7),
						Description = r.IsDBNull(8) ? "":r.GetString(8)
					};
					
					factory.Register(new CommandReader(or =>
					{
						retVal.Opponents.Add(new Opponent()
						{
							Id =or.GetValue("Id", 0L),
							Name = or.GetValue("Name", ""),
							Rank = or.GetValue<Int16>("Rank", 0),
							Url = or.GetValue("Url", "")
						});
					}, "SELECT * FROM BracketOpponents (nolock) WHERE BracketId=@BracketId", System.Data.CommandType.Text, _options.Value.Timeout, new SqlParameter("@BracketId", retVal.Id)));


					factory.Register(new CommandReader(pr =>
					{
						retVal.Registered.Add(pr.GetInt32(0));
					}, "SELECT UserId FROM BracketParticipant (nolock) WHERE BracketId=@BracketId", System.Data.CommandType.Text, _options.Value.Timeout, new SqlParameter("@BracketId", retVal.Id)));
					return false;
				},
				"SELECT Id, Name, UserId, Private, Status, Cutoff, MinUsers, MaxUsers, Description FROM Bracket (nolock) WHERE Id=@Id", System.Data.CommandType.Text, _options.Value.Timeout,
				new SqlParameter("@Id", id)));
			factory.Execute();
			return retVal;
		}

		public bool Join(ClaimsPrincipal user, Models.Bracket bracket)
		{
			if (user.Identity == null || !user.Identity.IsAuthenticated) { throw new UnauthorizedAccessException(); }
			ConnectionFactory factory = new ConnectionFactory(_options.Value.ConnectionString);
			factory.Register(new CommandReader(
				r=>
				{
					_logger.LogDebug("User {user} joined {Id}", user.Id(), bracket.Id);
					factory.Register(Commands.Insert("BracketParticipant",
						new SqlParameter("@BracketId", bracket.Id),
						new SqlParameter("@UserId", user.Id())));
					if (r.GetInt16(0) == 0 || r.GetInt16(1) + 1 >= r.GetInt16(0))
					{
						factory.Register(new Command("UPDATE Bracket SET Status=2 WHERE Id=@Id", new SqlParameter("@Id", bracket.Id)));
						_logger.LogDebug("Start game {Id}", bracket.Id);
					}
				}, "SELECT MaxUsers, (SELECT Count(*) FROM BracketParticipant WHERE BracketId=@Id) [participants] FROM Bracket WHERE Id=@Id AND Status=2 AND (Cutoff < '2000-01-01' OR Cutoff>sysdatetimeoffset()) AND NOT EXISTS(SELECT * FROM BracketParticipant WHERE BracketId=@Id AND UserId=@UserId) AND (MaxUsers=0 OR MaxUsers < (SELECT Count(*) FROM BracketParticipant WHERE BracketId=@Id))", System.Data.CommandType.Text, _options.Value.Timeout, new SqlParameter("@Id", bracket.Id), new SqlParameter("@UserId", user.Id())));

			var response = factory.Execute();
			if (response.Count == 3)
			{
				//TODO: notify of game start!
			}
			if (response.Count == 1) return false;
			return response[1]==1;
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
					int maxUsers = r.GetInt16(4);
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
				}, "SELECT Opponent1Id, Opponent2Id, Opponent3Id, Complete, MaxUsers FROM BracketMatch JOIN Bracket ON Bracket.Id=BracketMatch.BracketId WHERE BracketMatch.Id=@MatchId AND BracketMatch.BracketId=@BracketId AND EXISTS(SELECT * FROM BracketParticipant WHERE BracketId=@BracketId AND UserId=@UserId)",
				System.Data.CommandType.Text, _options.Value.Timeout,
				new SqlParameter("@MatchId", vote.MatchId),
				new SqlParameter("@BracketId", vote.BracketId),
				new SqlParameter("@UserId", userId))
			);

			factory.Execute();
			return isComplete?.Value ?? false;
		}

		public BracketSearchResults Search(BracketSearch search)
		{
			List<string> joins = new List<string>();
			StringBuilder filter = new StringBuilder();
			bool first = true;
			List<SqlParameter> queryParameters = new List<SqlParameter>();
			List<SqlParameter> totalParameters = new List<SqlParameter>();
			if ((search.OwnerId??0) != 0)
			{
				if (first) { filter.Append(" WHERE "); first = false; }
				else { filter.Append(" AND "); }
				filter.Append(" Bracket.UserId=@OwnerId");
				queryParameters.Add(new SqlParameter("@OwnerId", search.OwnerId));
				totalParameters.Add(new SqlParameter("@OwnerId", search.OwnerId));
			}
			if (!string.IsNullOrWhiteSpace(search.Name))
			{
				if (first) { filter.Append(" WHERE "); first = false; }
				else { filter.Append(" AND "); }
				filter.Append(" Bracket.Name LIKE @Name");
				string name = search.Name.Replace("\\", "\\\\").Replace("%", "\\%").Replace("*", "%");
				queryParameters.Add(new SqlParameter("@Name", search.Name));
				totalParameters.Add(new SqlParameter("@Name", search.Name));
			}

			if (search.Private.HasValue)
			{
				if (first) { filter.Append(" WHERE "); first = false; }
				else { filter.Append(" AND "); }
				filter.Append(" Bracket.Private=@Private");
				queryParameters.Add(new SqlParameter("@Private", search.Private));
				totalParameters.Add(new SqlParameter("@Private", search.Private));
			}
			if (search.Status.HasValue)
			{
				if (first) { filter.Append(" WHERE "); first = false; }
				else { filter.Append(" AND "); }
				filter.Append(" Bracket.Status in (");
				bool firstStatus = true;
				foreach (byte b in search.Status.Value.MaskToList<byte, BracketStatus>())
				{
					if (firstStatus) { firstStatus = false; }
					else { filter.Append(", "); }
					filter.Append(b);
				}
				filter.Append(")");
			}
			List<string> columns = new List<string>(new string[] { "Bracket.Id", "Bracket.Name", "Bracket.Private", "Bracket.Status", "Bracket.Description"  });
			bool joined = false;
			if ((search.UserId??0) != 0)
			{
				joins.Add(" LEFT OUTER JOIN BracketParticipant (nolock) ON Bracket.Id=BracketParticipant.BracketId AND BracketParticipant.UserId=@UserId");
				queryParameters.Add(new SqlParameter("@UserId", search.UserId));
				totalParameters.Add(new SqlParameter("@UserId", search.UserId));
				columns.Add("CASE WHEN BracketParticipant.Id IS NULL THEN 0 ELSE 1 END [Joined]");
				joined = true;
				first = false;
			}
			string totalQuery = mcZen.Data.Queries.SelectCount("Bracket", filter.ToString(), joins, true); 
			string query = mcZen.Data.Queries.PagedQuery(columns, "Bracket", joins, filter.ToString(), search.OrderBy, search.Page, search.Size, true);
			BracketSearchResults results = new BracketSearchResults();
			ConnectionFactory factory = new ConnectionFactory(_options.Value.ConnectionString);
			factory.Register(new ScalarCommand<int>(t=>results.Total=t, totalQuery, totalParameters.ToArray()));
			factory.Register(new CommandReader(
				(r)=>
				{
					var bracket = new BracketSearchResult()
					{
						Id = r.GetInt64(1),
						Name = r.GetString(2),
						Private = r.GetBoolean(3),
						Status = (BracketStatus)r.GetByte(4),
						Description = r.IsDBNull(5) ? "":r.GetString(5)
					};
					if (joined && r.GetInt32(6)==1)
					{
						bracket.Joined = true;
					}
					results.Brackets.Add(bracket);

				},
				query, System.Data.CommandType.Text, _options.Value.Timeout, queryParameters.ToArray()
			));
			factory.Execute();
			return results;
		}

		public class Options
		{
			public string ConnectionString { get; set; } = "";
			public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
		}
	}
}
