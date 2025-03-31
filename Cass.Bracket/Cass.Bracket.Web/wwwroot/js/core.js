if (typeof window.baseUrl == 'undefined') { window.baseUrl = /^[^\/]*(\/.*)js\/core\.js/gi.exec(document.currentScript.src)[1]; }
window._initVuePage = function() {
	let mixin = {
		methods: {
			mapPath:function(url) {
				if (/^\~\//g.test(url)) {
					if (window.baseUrl) url = window.baseUrl + url.substring(2);
					else url = url.substring(1);
				}
				return url;
			}
		}
	};
	let app = Vue.createApp(window.page.vue);
	//app.config.compilerOptions.whitespace = 'preserve';
	VueComponents.forEach(function (o) {
		app.component(o.ID, o.Component);
	});
	app.mixins = [mixin];

	// Mount
	window.page.app = app.mount('#vueRoot');
	return;
}

document.addEventListener("DOMContentLoaded", window._initVuePage);