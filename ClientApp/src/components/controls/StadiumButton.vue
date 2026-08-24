<template>
	<div v-show="visible" :id="`${controlId}-container`" :class="classes">
		<button :id="controlId" :disabled="disabled" :title="tooltip" type="button" class="btn btn-lg btn-default" @click="onClick">
			{{ text }}
		</button>
	</div>
</template>

<script>
	export default {
		name: 'StadiumButton',

		props: {
			controlId: String,
			text: String,
			tooltip: String,
			visible: Boolean,
			hasClickEvent: Boolean,
			classes: null
		},
		emits: ['click'],

		data() {
			return {
				disabled: false
			};
		},

		methods: {
			onClick() {
				if (!this.hasClickEvent) return;

				this.disabled = true;
				this.$emit('click', () => (this.disabled = false));
			}
		}
	};
</script>
