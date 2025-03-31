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
        submitText: function() { return this.create?'Sign up':'Sign in'; },
		isValid: function() {
			const passwordOk = this.password.length > 0;
			const emailOk = /^.+@.+$/gi.test(this.username);
			const nameOk = !this.create || /^\s*.+/gi.test(this.name);
			return passwordOk && emailOk && nameOk;
		}
	},
	watch: {
	},
	methods: {
        submit: function() {
			var me = this;
			if (!this.isValid) return;
			fetch(me.create ? '/api/user/create' : '/api/user/login', {
                method: 'POST',
				headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
					Username: me.username,
					Password: me.password,
					RememberMe: me.rememberMe,
					Name: me.name
                })
			}).then(async r=> {
				if (r.status == 200) return r.json();
				var error = '';
				if (r.status == 400) {
					var jerr = await r.json();
					error = JSON.stringify(jerr);
				}
				return { success: false, error: `${r.status}: ${r.statusText}: ${error}` };
			}).then(r=> {
                if (r.error) {
                    me.error = r.error;
                }
				else {
					window.location.href = '/';
				}
			}).catch(err=> { me.error = err.message });
        }
	}
}

