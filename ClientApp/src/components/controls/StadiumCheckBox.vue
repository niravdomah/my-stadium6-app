<template>
	<div v-show="visible" :id="`${controlId}-container`" :class="classes">
		<input :id="controlId" :checked="checked" :disabled="resolvedDisabled" type="checkbox" @change="onChange" />
		<label :for="controlId" />
	</div>
</template>

<script>
	export default {
		name: 'StadiumCheckBox',

		props: {
			controlId: String,
			checked: Boolean,
			readonly: Boolean,
			visible: Boolean,
			hasChangeEvent: Boolean,
			classes: null
		},
		emits: ['change'],

		data() {
			return {
				disabled: false
			};
		},

		computed: {
			resolvedDisabled() {
				return this.readonly || this.disabled;
			}
		},

		methods: {
			onChange(event) {
				this.$emit('update:checked', event.target.checked);

				if (!this.hasChangeEvent) return;

				this.disabled = true;
				this.$emit('change', () => (this.disabled = false));
			}
		}
	};
</script>
