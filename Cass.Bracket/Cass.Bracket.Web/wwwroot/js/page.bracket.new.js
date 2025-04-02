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

		}
	}
}

