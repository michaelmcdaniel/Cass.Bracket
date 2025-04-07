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
        generateFirstRoundMatches: function(opponents, numQuadrants) {
            const totalPlayers = opponents.length;

            // Find the next power of 2 greater than or equal to totalPlayers
            const nextPowerOfTwo = Math.pow(2, Math.ceil(Math.log2(totalPlayers)));
            const byes = nextPowerOfTwo - totalPlayers;

            const matches = [];
            const firstRoundMatchCount = nextPowerOfTwo / 2;

            let matchId = 1;
            let round = 1;

            // Seeded matching using standard bracket logic:
            // Pair highest with lowest, second highest with second lowest, etc.
            let top = 0;
            let bottom = totalPlayers - 1;
            for (let i = 0; i < firstRoundMatchCount; i++) {
                let opponent1Id = null;
                let opponent2Id = null;

                // Determine who plays based on available players and byes
                if (i < byes) {
                    // This is a bye, top-seeded player advances automatically
                    opponent1Id = opponents[top++];
                    opponent2Id = null;
                } else {
                    opponent1Id = opponents[top++];
                    opponent2Id = opponents[bottom--];
                }

                const groupId = Math.floor(i * numQuadrants / firstRoundMatchCount) + 1;

                matches.push({
                    matchId: matchId++,
                    opponent1Id,
                    opponent2Id,
                    round,
                    groupId
                });
            }

            return matches;
        }
	}
}

