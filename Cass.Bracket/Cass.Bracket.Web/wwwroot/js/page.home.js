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
		await fetch('/api/brackets/search/0?q=in:2', {
			method: 'post', headers: { "Content-Type": "application/json" }, body: JSON.stringify({

			})
		}).then(r => r.json()).then(r => {
			me.open = r.brackets;
		})
		await fetch('/api/my/brackets/search/0', {
			method: 'post', headers: { "Content-Type": "application/json" }, body: JSON.stringify({

			})
		}).then(r =>r.json()).then(r => {
			me.brackets = [];
			me.active = [];
			for(var i = 0; i < r.brackets.length; i++) 
			{
				if (r.brackets[i].joined && (r.brackets[i].status == 2 || r.brackets[i].status == 3)) {
					me.active.push(r.brackets[i]);
				} else {
					me.brackets.push(r.brackets[i]);
				}
			}
			
		})
	},
	computed: {
		shortName: function() {
			if (this.name.length > 18) return this.name.substr(0,15)+'...';
			return this.name;
		}
	},
	watch: {
	},
	methods: {

	}
}

