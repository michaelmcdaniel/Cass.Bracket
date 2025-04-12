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
		},
        rounds: function() {
            return this.mergeByesIntoNextRound(this.flattenRounds(this.createBracket(this.bracket.opponents)));
        },
        roundsAsTableRow: function () {
            return this.makeTable(this.mergeByesIntoNextRound(this.flattenRounds(this.createBracket(this.bracket.opponents))));
        }
	},
	watch: {
		'teamsText' : function(v) {
			this.bracket.opponents = v.split('\n').map(s => s.trim()).filter(s => s !== "")
		}
	},
	methods: {
		save: function() {
            var me = this;
            fetch('/api/bracket/create', {
                method: 'post', headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    Id: this.bracket.id,
                    Name: this.bracket.name,
                    Opponents: this.bracket.opponents
                })
                }).then(async r=>{
                    if (r.status == 400) {
                        var data = await r.json();
                        var error = '';
                        var i = 0;
                        for(var prop in data.errors) {
                            for (var j = 0; j < data.errors[prop].length; j++) error += (i == 0 && j == 0 ? '' : '\n') + prop + ": " + data.errors[prop][j];
                            i++;
                        }
                        return { success: false, error: error };
                    }
                    return r.json();
                }).then(async data => {
                    if (!data.success) {
                        alert(data.error);
                    } else {
                        me.bracket.id = data.id;
                    }
                })
		},
        generateRound: function(teams) {
            if (teams.length < 2) { return []; }
            let gameId = 1;
            function createOpponent(opponent, matchId=null) {
                if (!opponent) return {id:null,name:null,score:0,matchId};
                return {id:opponent.id,name:opponent.name,score:0,matchId};
            }
            function createGame(round) {
                var opponents=[];
                for(var i = 1; i < arguments.length; i++) if (arguments[i]) { opponents.push(arguments[i]); }
                return { id: gameId++, round, opponents,winner:null };
            }
            function highestRank(game) {
                var r = game.opponents[0].id;
                for (var i = 0; i < game.opponents.length; i++) r = Math.min(r, game.opponents[i].id);
                return r;
            }
            var perfectBracket = 0, previousPerfectBracket = 0;
            for (var i = 2; perfectBracket < teams.length; i*=2) { previousPerfectBracket = perfectBracket; perfectBracket=i; }
            var teamsWithBye = [];
            var remainingTeams = [];
            var round = [];
            var numberOfTeamsWithBye = Math.max(0,previousPerfectBracket - (teams.length - previousPerfectBracket));
            var numberOfRemainingTeamsInFirstRound = teams.length-numberOfTeamsWithBye;
            for (var i = 0; i < numberOfTeamsWithBye; i++) teamsWithBye.push({id:i,name:teams[i]});
            for (var i = numberOfTeamsWithBye; i < teams.length; i++) remainingTeams.push({ id: i, name: teams[i] });
            while (teamsWithBye.length > 0 && remainingTeams.length > 0) { round.push(createGame(1, teamsWithBye.shift(), remainingTeams.pop(), remainingTeams.pop())); }
            while (remainingTeams.length > 0) { round.push(createGame(1, remainingTeams.shift(), remainingTeams.pop())) }
            while (teamsWithBye.length > 0) { round.push(createGame(1, teamsWithBye.shift(), teamsWithBye.pop())) }
            // reorganize
            round.sort((a,b)=>highestRank(a)-highestRank(b));
            return round;
        }
	}
}

