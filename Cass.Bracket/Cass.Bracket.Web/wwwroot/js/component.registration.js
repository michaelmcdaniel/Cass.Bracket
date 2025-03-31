var VueComponents = [];
if (!Vue.component) {
    Vue.component = function (id, component) {
        VueComponents.push({ ID: id, Component: component });
    };
}
