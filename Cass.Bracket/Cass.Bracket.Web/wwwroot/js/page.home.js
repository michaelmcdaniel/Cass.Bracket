if (!window.page) window.page = {}
if (!window.page.model) window.page.model = {};
if (!window.page.defaults) window.page.defaults = {};

window.page.vue = {
	data: function() {
		return Object.assign({
			error: '',
		}, window.page.model);
	},
	mounted: function() {
	},
	computed: {
	},
	watch: {
	},
	methods: {

	}
}

