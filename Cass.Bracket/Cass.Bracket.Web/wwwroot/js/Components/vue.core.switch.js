Vue.component('core-switch', {
	props: {
		modelValue: { type:Boolean, default:false },
		label: { type:String },
		labelOn: { type:String, default: '' },
		labelOff: { type: String, default: '' },
		reverse: { type: Boolean, default: false },
		disabled: { type: Boolean, default: false },
		readonly: { type: Boolean, default: false },
		tabindex: { type: Number, default:0 }
	},
	emits: ['update:modelValue'],
	data: function () {
		return { ivalue: false };
	},
	mounted: function () {
		this.ivalue = this.modelValue;
	},
	computed: {
		rightLabel: function () { return this.reverse ? this.labelOn : this.labelOff; },
		rightLabelClass: function () { return this.reverse ? 'on' : 'off'; },
		leftLabel: function () { return this.reverse ? this.labelOff : this.labelOn; },
		leftLabelClass: function () { return this.reverse ? 'off' : 'on'; },
		realTabIndex: function () {
			return this.disabled || this.readonly ? '' : this.tabindex; 
		}
	},
	watch: {
		ivalue: function (ov, nv) { this.$emit('update:modelValue', ov) },
		modelValue: function (v) { this.ivalue = v; }
	},
	methods: {
		setValue: function (v) {
			if (!this.readonly && !this.disabled) {
				this.ivalue = (typeof v == 'boolean') ? v : !this.ivalue;
				this.$refs.knob.focus();
			}
		}
	},
	template: '<div :class="[\'switch\', {reverse:reverse, disabled:disabled, readonly:readonly}]"><label class="main">{{label}}</label><span class="switch-container"><input type="checkbox" :disabled="disabled" v-model="ivalue"/><label @click.prevent.stop="setValue(!reverse)" :class="leftLabelClass">{{leftLabel}}</label><span ref="knob" :tabindex="realTabIndex" @click="setValue" @keypress.space="setValue" class="switch-knob"></span><label @click.prevent.stop="setValue(reverse)" :class="rightLabelClass">{{rightLabel}}</label></span></div>'
})
