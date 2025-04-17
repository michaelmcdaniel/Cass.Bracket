if (!window.page) window.page = {}
if (!window.page.model) window.page.model = {};
if (!window.page.defaults) window.page.defaults = {};

window.page.vue = {
	data: function() {
		return Object.assign({
			error: '',
			openBrackets:''
		}, window.page.model);
	},
	mounted: async function() {
		var me = this;
	},
	computed: {
		shortName: function() {
			if (this.name.length > 18) return this.name.substr(0,15)+'...';
			return this.name;
		},
		canCastVote: function() {
			var retVal = true;
			for(var i = 0; retVal && i < this.bracket.Matches.length; i++) {
				if (this.bracket.Matches[i].Winner <= 0) retVal = false;
			}
			return retVal;
		}
	},
	watch: {
	},
	methods: {
		cast: async function() {
			var me = this, selections ={};
			for(var i = 0; i < this.bracket.Matches.length; i++) {
				if (this.bracket.Matches[i].Winner <= 0) retVal = false;
				selections[this.bracket.Matches[i].Id] = this.bracket.Matches[i].Winner;
			}
			await fetch('/api/bracket/cast/'+this.bracket.Id, {
				method: 'post', 
				headers: { "Content-Type": "application/json" }, 
				body: JSON.stringify({
					Round: me.Round,
					Winners: selections
				})
			}).then(async r => {
				if (r.status == 200 || r.status == 201) { 
					return r.json()
				}
                    if (r.status == 400) {
                        var data = await r.json();
                        var error = '';
                        var i = 0;
                        for(var prop in data.errors) {
                            for (var j = 0; j < data.errors[prop].length; j++) error += (i == 0 && j == 0 ? '' : '\n') + prop + ": " + data.errors[prop][j];
                            i++;
                        }
                        return { success: false, error: error };
                    } else {
						return {success: false, error: r.status + ' something bad...' }
					}
				
			}).then(async r => {
				if (!r.success) {
					me.error = r.error;
				} else { me.error = ''; }
				//me.open = r.brackets;
			})
		}
	}
}

