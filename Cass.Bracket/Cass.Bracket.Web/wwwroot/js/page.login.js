if (!window.page) window.page = {}
if (!window.page.model) window.page.model = {};
if (!window.page.defaults) window.page.defaults = {};

window.page.vue = {
	data: function() {
		return Object.assign({
			error: '',
			mouth: false,
			_cookieDelay:null,
			initialized: false,
			talking: 0
		}, window.page.model);
	},
	mounted: function() {
		var me = this;
		var creds = this.getCookie('login');
		if (creds && /^(true|false)\|.*$/gi.test(creds)) {
            this.username = /\|.*$/gi.exec(creds)[0].substring(1);
			this.rememberMe = /^true/gi.test(creds);
			this.$nextTick(function () { try { document.getElementById(this.username == '' ? 'username' : 'password').focus(); }catch{} });
		}
		else
		{
			this.$nextTick(function () {  try { document.getElementById('username').focus(); }catch { } });
		}
		this.$nextTick(function () {  me.initialized=true; });
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
		username: function (v,ov) { 
			if (this.initialized) { 
				this.delayCookie('login', `${this.rememberMe}|${v}`)} 
			if (this.error == 'Enter your EMAIL!' && !(this.username.trim() == '' || !/.+@.+\..+$/gi.test(this.username))) this.error = '';
		},
		name: function(v) {
			if (this.error == 'Enter your NAME!' && !(this.name.trim() == '')) this.error = '';
		},
		password: function(v) {
			if (this.error == 'Enter your PASSWORD!' && !(this.password.length == 0)) this.error = '';
		},
		rememberMe: function (v, ov) { 
			if (this.initialized) { 
				this.delayCookie('login', `${v}|${this.username}`) 
				var me = this; this.mouth=true; setTimeout(()=>{me.mouth=false;}, 100);
			}
		},
		error: function(v, ov) {
			this.talk();
		}
	},
	methods: {
		delayCookie: function(name, value) {
			var me = this;
			if (me._cookieDelay) clearTimeout(me._cookieDelay);
			me._cookieDelay = setTimeout(() => {
				me.setCookie(name, value);
				me._cookieDelay = null;
            }, 500);
		},
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
		talk: function(s) {
			var me = this;
			me.talking = (s||(this.error.match(/\s+/g) || []).length)+1;
			if (me.talking > 0)
			{
				var talk = null;
				talk = () => {
					if (me.talking > 0)
					{
						var speed = (Math.random() * 300) + 150;

						me.mouth = true; setTimeout(() => { me.mouth = false; }, Math.random()*80);
						me.talking--;
						setTimeout(talk, speed + (Math.random() * 60));
					}
				}
				talk();
			}
		},
		trysubmit: function() {
			if (this.create) {
				if (this.name.trim() == '') { this.error = 'Enter your NAME!'; this.talk(4); this.$nextTick(function () { try { document.getElementById('yourname').focus(); } catch { } }); }
				else if (this.username.trim() == '' || !/.+@.+\..+$/gi.test(this.username)) { this.error = 'Enter your EMAIL!'; this.talk(5); this.$nextTick(function () { try { document.getElementById('username').focus(); } catch { } }); }
				else if (this.password == '') { this.error = 'Enter your PASSWORD!'; this.talk(5); this.$nextTick(function () { try { document.getElementById('password').focus(); } catch { } }); }
				else this.submit();
			} else {
				if (this.username.trim() == '' || !/.+@.+\..+$/gi.test(this.username)) { this.error = 'Enter your EMAIL!'; this.talk(5); this.$nextTick(function () { try { document.getElementById('username').focus(); } catch { } }); }
				else if (this.password == '') { this.error = 'Enter your PASSWORD!'; this.talk(5); this.$nextTick(function () { try { document.getElementById('password').focus(); } catch { } }); }
				else this.submit();
			}
			
		},
        submit: function() {
			var me = this;
			if (!this.isValid) return;
			
			fetch(me.create ? '/api/user/create' : '/api/user/login', {
                method: 'POST',
				headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
					Username: me.username.trim(),
					Password: me.password,
					RememberMe: me.rememberMe,
					Name: me.name.trim()
                })
			}).then(async r=> {
				if (r.status == 200) return r.json();
				var error = '';
				if (r.status == 400) {
					error = 'Invalid - Try Again.';
				} else {
					error = 'login unavailable'
				}

				return { success: false, error: `${error}` };
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

