if (!window.page) window.page = {}
if (!window.page.model) window.page.model = {};
if (!window.page.defaults) window.page.defaults = {};

window.page.vue = {
	data: function() {
		return Object.assign({
			error: '',
			mouth: false,
		}, window.page.model);
	},
	mounted: function() {
		var rememberMe = this.getCookie('rememberMe');
		if (rememberMe) {
			this.username=rememberMe;
			this.rememberMe = true;
			this.$nextTick(function() { try { document.getElementById('password').focus(); }catch{} });
		}
		else 
		{
			this.$nextTick(function () {  try { document.getElementById('username').focus(); }catch { } });
		}
	},
	computed: {
        submitText: function() { return this.create?'ENTER':'ENTER'; },
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
		getCookie: function (name) {
			const cookies = document.cookie.split(';');
			for (let cookie of cookies)
			{
				let [key, value] = cookie.trim().split('=');
				if (key === name)
				{
					return decodeURIComponent(value);
				}
			}
			return null;
		},
		setCookie: function(name, value) {
			const days = 365; // how long to keep the cookie
			const date = new Date();
			date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000)); // 30 days from now
			const expires = "expires=" + date.toUTCString();
			document.cookie = `${name}=${encodeURIComponent(value)}; ${expires}; path=/`; //; 
		},
		toggleCreate: function() {
			this.create = !this.create;
			if (!this.create) {
				this.$nextTick(function () { try { document.getElementById(this.username == '' ? 'username' : 'password').focus(); } catch { } });
			} else {
				this.$nextTick(function () { try { document.getElementById('yourname').focus(); } catch { } });
			}
		},
        submit: function() {
			var me = this;
			if (!this.isValid) return;
			this.setCookie('rememberMe', me.username);
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
					try { document.getElementById('password').focus(); } catch { } 
                }
				else {
					window.location.href = '/';
				}
			}).catch(err=> { me.error = err.message });
        }
	}
}

