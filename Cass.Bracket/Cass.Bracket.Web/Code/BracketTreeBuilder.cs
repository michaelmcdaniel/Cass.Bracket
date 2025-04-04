using Cass.Bracket.Web.Models;

namespace Cass.Bracket.Web.Code
{
    public static class BracketTreeBuilder
    {
        public static Round GenerateBracketRounds(Models.Bracket bracket)
        {
            int teamCount = bracket.Opponents.Count;
            if (teamCount < 2)
                throw new ArgumentException("Bracket requires at least 2 opponents.");

            int nextPowerOfTwo = 1;
            while (nextPowerOfTwo < teamCount) nextPowerOfTwo <<= 1;
            int totalRounds = (int)Math.Log2(nextPowerOfTwo);
            int totalMatches = nextPowerOfTwo - 1;

            // Step 1: Generate seed positions
            var seeds = Enumerable.Range(1, teamCount).ToList();
            var bracketSize = nextPowerOfTwo;

            // Seed positions (standard mirrored seeding)
            var positions = GenerateSeedPositions(bracketSize);

            // Step 2: Map seeds to positions (fill with 0s for empty slots)
            var seedByPosition = new Dictionary<int, int>();
            for (int i = 0; i < bracketSize; i++)
            {
                if (i < seeds.Count)
                    seedByPosition[positions[i]] = seeds[i];
                else
                    seedByPosition[positions[i]] = 0; // empty slot
            }

            // Step 3: Build first round
            int matchId = 1;
            var allMatches = new List<Match>();
            var matchQueue = new Queue<Match>();

            var firstRoundMatches = new List<Match>();
            for (int i = 0; i < bracketSize; i += 2)
            {
                int seed1 = seedByPosition.ContainsKey(i + 1) ? seedByPosition[i + 1] : 0;
                int seed2 = seedByPosition.ContainsKey(i + 2) ? seedByPosition[i + 2] : 0;

                if (seed1 == 0 && seed2 == 0) continue; // skip empty

                var match = new Match
                {
                    Id = matchId++,
                    Round = 1,
                    Opponent1 = new MatchOpponent { Id = seed1, Score = 0, ParentMatchId = 0 },
                    Opponent2 = new MatchOpponent { Id = seed2, Score = 0, ParentMatchId = 0 },
                    Winner = -1
                };

                firstRoundMatches.Add(match);
                matchQueue.Enqueue(match);
                allMatches.Add(match);
            }

            var roundMap = new Dictionary<int, List<Match>> { [1] = firstRoundMatches };

            // Step 4: Recursively build the rest of the tree
            int roundNumber = 2;
            while (matchQueue.Count > 1)
            {
                var currentRound = new List<Match>();
                while (matchQueue.Count > 1)
                {
                    var m1 = matchQueue.Dequeue();
                    var m2 = matchQueue.Dequeue();

                    var match = new Match
                    {
                        Id = matchId++,
                        Round = roundNumber,
                        Opponent1 = new MatchOpponent { Id = 0, Score = 0, ParentMatchId = m1.Id },
                        Opponent2 = new MatchOpponent { Id = 0, Score = 0, ParentMatchId = m2.Id },
                        Winner = -1
                    };

                    currentRound.Add(match);
                    allMatches.Add(match);
                    matchQueue.Enqueue(match);
                }

                // Handle odd match forward
                if (matchQueue.Count == 1)
                {
                    matchQueue.Enqueue(matchQueue.Dequeue());
                }

                roundMap[roundNumber] = currentRound;
                roundNumber++;
            }

            // Step 5: Link rounds
            Round? next = null;
            for (int r = roundMap.Keys.Max(); r >= 1; r--)
            {
                next = new Round
                {
                    RoundNumber = r,
                    Points = (int)Math.Pow(2, r - 1),
                    Matches = roundMap[r],
                    Completed = false,
                    Next = next,
                    Quadrant = 0
                };
            }

            return next!;
        }

        // Standard mirrored seeding for S-team brackets
        private static List<int> GenerateSeedPositions(int size)
        {
            List<int> positions = new List<int> { 1 };

            while (positions.Count < size)
            {
                var mirror = positions.Select(p => size + 1 - p).Reverse();
                positions.AddRange(mirror);
            }

            return positions;
        }
    }

}
