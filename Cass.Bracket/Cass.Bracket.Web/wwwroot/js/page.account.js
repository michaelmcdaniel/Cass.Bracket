if (!window.page) window.page = {}
if (!window.page.model) window.page.model = {};
if (!window.page.defaults) window.page.defaults = {};

window.page.vue = {
	data: function() {
		return Object.assign({
			error: '',
			initialized: false
		}, window.page.model);
	},
	mounted: function() {
		var me = this;
		this.$nextTick(function () {  me.initialized=true; });
	},
	computed: {
		shortName: function () {
			if (this.name.length > 18) return this.name.substr(0, 15) + '...';
			return this.name;
		},
		isValid: function() {
			const nameOk = /^\s*.+/gi.test(this.name);
			return nameOk;
		}
	},
	watch: {
		name: function(v) {
			if (this.error == 'Enter your NAME!' && !(this.name.trim() == '')) this.error = '';
		},
		password: function(v) {
			if (this.error == 'Enter your PASSWORD!' && !(this.password.length == 0)) this.error = '';
		}
	},
	methods: {
		trysubmit: function() {
			if (this.name.trim() == '') { this.error = 'Enter your NAME!'; this.talk(4); this.$nextTick(function () { try { document.getElementById('yourname').focus(); } catch { } }); }
			else this.submit();
		},
        submit: function() {
			var me = this;
			if (!this.isValid) return;
			
			fetch('/api/user/update', {
                method: 'POST',
				headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
					Password: me.password,
					Name: me.name.trim()
                })
			}).then(async r=> {
				if (r.status == 200) return r.json();
				var error = '';
				if (r.status == 400) {
					error = 'Invalid - Try Again.';
				} else {
					error = 'account updates unavailable'
				}

				return { success: false, error: `${error}` };
			}).then(r=> {
                if (r.error) {
                    me.error = r.error;
					try { document.getElementById('password').focus(); } catch { } 
                }
				else {
					me.success=true;
					me.error = '';
					setTimeout(()=> { me.success=false; }, 5000);
				}
			}).catch(err=> { me.error = err.message });
        }
	}
}

