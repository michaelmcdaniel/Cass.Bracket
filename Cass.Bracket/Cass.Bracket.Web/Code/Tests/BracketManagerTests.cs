using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cass.Bracket.Web.Code.Tests
{
	[TestClass]
	public class BracketManagerTests
	{
		[TestMethod]
		public void TestBracketGeneration()
		{
			ServiceCollection sc = new ServiceCollection();
			sc.AddTransient<BracketManager>();
			var services = sc.BuildServiceProvider();

			//var bracket = new Models.Bracket
			//{
			//	Id = 1,
			//	Name = "Test Bracket",
			//	UserId = 1,
			//	Private = false,
			//	MinUsers = 2,
			//	MaxUsers = 0,
			//	Cutoff = DateTimeOffset.UtcNow.AddDays(7),
			//	Opponents = new List<string> { "Team 1", "Team 2", "Team 3", "Team 4" }
			//};
			//var round = BracketTreeBuilder.GenerateBracketRounds(bracket);
			//Assert.IsNotNull(round);
			//Assert.AreEqual(1, round.RoundNumber);
			//Assert.AreEqual(2, round.Matches.Count);
			//Assert.IsNotNull(round.Next);
			//Assert.AreEqual(round.Matches[0].Id, round.Next.Matches[0].Opponent1.ParentMatchId);
			//Assert.AreEqual(round.Matches[1].Id, round.Next.Matches[0].Opponent2.ParentMatchId);

			//bracket.Opponents.Add("Team 5");
			//round = BracketTreeBuilder.GenerateBracketRounds(bracket);
			//Assert.IsNotNull(round);
			//Assert.AreEqual(1, round.RoundNumber);
			//Assert.AreEqual(2, round.Matches.Count);
			//Assert.IsNotNull(round.Next);
			//Assert.AreEqual(round.Matches[0].Id, round.Next.Matches[0].Opponent1.ParentMatchId);
			//Assert.AreEqual(round.Matches[1].Id, round.Next.Matches[1].Opponent1.ParentMatchId);
		}
	}
}
