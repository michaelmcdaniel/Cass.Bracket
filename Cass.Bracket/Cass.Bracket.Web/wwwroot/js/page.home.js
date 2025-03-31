if (!window.page) window.page = {}
if (!window.page.model) window.page.model = {};
if (!window.page.defaults) window.page.defaults = {};

window.page.vue = {
	data: function() {
		return Object.assign({
		}, window.page.model);
	},
	mounted: function() {
	},
	computed: {
        submitText: function() { return this.create?'Sign up':'Sign in'; }
	},
	watch: {
	},
	methods: {
        submit: function() {
            alert(this.create?'create':'signin')
        }
	}
}

