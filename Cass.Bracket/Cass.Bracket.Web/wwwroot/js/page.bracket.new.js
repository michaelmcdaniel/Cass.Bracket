if (!window.page) window.page = {}
if (!window.page.model) window.page.model = {};
if (!window.page.defaults) window.page.defaults = {};

window.page.vue = {
	data: function () {
		return Object.assign({
			error: '',
			teamsText: ''
		}, window.page.model);
	},
	mounted: function () {
        this.teamsText = this.bracket.opponents.join("\n");
	},
	computed: {
		totalRounds: function() {
			if (this.bracket.opponents.length < 2) return 0;
			return Math.ceil(Math.log2(this.bracket.opponents.length));
		}
	},
	watch: {
		'teamsText' : function(v) {
			this.bracket.opponents = v.split('\n').map(s => s.trim()).filter(s => s !== "")
		}
	},
	methods: {
		save: function() {

		},
		createBracket: function(teams) {
            if (teams.length < 2)
            {
                return null;
            }
            let gameIdCounter = 1;
            function createGame(round, opponent1, opponent2) {
                return {
                    id: gameIdCounter++,
                    round,
                    opponents: [opponent1, opponent2],
                    winner: null,
                    children: []
                };
            }

            function createOpponent(team, matchId = null) {
                if (!team)
                {
                    return { id: null, name: null, score: 0, matchId };
                }
                return {
                    id: team.seed,
                    name: team.name,
                    score: 0,
                    matchId
                };
            }

            function createNextRoundGames(teams, previousGames, round) {
                const games = [];

                while (teams.length > 1)
                {
                    const high = teams.shift();
                    const low = teams.pop();

                    let child1 = high === null ? previousGames.shift() : null;
                    let child2 = low === null ? previousGames.pop() : null;

                    const game = createGame(
                        round,
                        high ? createOpponent(high) : createOpponent(null, child1?.id ?? null),
                        low ? createOpponent(low) : createOpponent(null, child2?.id ?? null)
                    );

                    game.children = [child1, child2].filter(Boolean);
                    games.push(game);
                }

                if (teams.length === 1)
                {
                    const leftover = teams.pop();
                    const game = createGame(
                        round,
                        createOpponent(leftover),
                        createOpponent(null)
                    );
                    games.push(game);
                }

                return games;
            }

            // Seeded team objects
            const seededTeams = teams.map((name, seed) => ({ name, seed }));

            // Next power of 2
            const totalSlots = Math.pow(2, Math.ceil(Math.log2(seededTeams.length)));
            const numByes = totalSlots - seededTeams.length;

            const teamsWithByes = [...seededTeams];
            const byeTeams = teamsWithByes.slice(0, numByes);
            const roundOneTeams = teamsWithByes.slice(numByes);

            const firstRoundGames = [];

            while (roundOneTeams.length >= 2)
            {
                const low = roundOneTeams.pop();
                const high = roundOneTeams.shift();
                firstRoundGames.push(createGame(1, createOpponent(high), createOpponent(low)));
            }

            if (roundOneTeams.length === 1)
            {
                byeTeams.push(roundOneTeams.pop());
            }

            let currentRound = 2;
            let currentTeams = [...byeTeams, ...firstRoundGames.map(() => null)];
            let games = firstRoundGames;

            while (currentTeams.length > 1 || games.length > 1)
            {
                const nextRoundGames = createNextRoundGames(currentTeams, games, currentRound);
                currentTeams = nextRoundGames.map(() => null);
                games = nextRoundGames;
                currentRound++;
            }

            return games[0]; // root game (championship)

        },
        flattenRounds: function (node, rounds = []) {
            if (!node) return rounds;
            if (!rounds[node.round - 1]) rounds[node.round - 1] = [];
            rounds[node.round - 1].push(node);
            node.children.forEach((child) => this.flattenRounds(child, rounds));
            return rounds;
        },
        mergeByesIntoNextRound: function (rounds) {
            var merged = [[]], hasMerged = false;
            var idMap = {}, gameIndex = 1;
            for(var i = 0; i < rounds[1].length; i++) {
                if (rounds[1][i].opponents[0].id != null && rounds[1][i].opponents[1].id == null) {
                    var g = rounds[0].find(m => m.id == rounds[1][i].opponents[1].matchId);
                    if (g) {
                        hasMerged = true;
                        g.opponents.unshift(rounds[1][i].opponents[0]);
                        merged[0].push(g);
                        idMap[rounds[1][i].id] = g.id;
                    } else {
                        merged[0].push(Object.assign({}, rounds[1][i], { round: 1, children: null }));
                    }
                } else merged[0].push(Object.assign({}, rounds[1][i], { round: 1, children: null }));
                
            }
            if (!hasMerged) return rounds;
            for(var i = 2; i < rounds.length; i++) {
                var array = [];
                for(var g = 0; g < rounds[i].length; g++)
                {
                    var ng = Object.assign({}, rounds[i][g], { round: i, children: null });
                    for(var o = 0; o < ng.opponents.length; o++) {
                        if (idMap.hasOwnProperty(ng.opponents[o].matchId)) {
                            ng.opponents[o].matchId = idMap[ng.opponents[o].matchId];
                        }
                    }
                    array.push(ng);
                }
                merged.push(array);
            }

            // rebuild game id references
            idMap = {};
            for (var i = 0; i < merged.length; i++)
            {
                for (var g = 0; g < merged[i].length; g++)
                {
                    idMap[merged[i][g].id] = gameIndex;
                    merged[i][g].id = gameIndex;
                    gameIndex++;
                    for (var o = 0; o < merged[i][g].opponents.length; o++) {
                        if (merged[i][g].opponents[o].matchId != null) {
                            merged[i][g].opponents[o].matchId = idMap[merged[i][g].opponents[o].matchId];
                        }
                    }
                }
            }
            return merged;
        }
	}
}

