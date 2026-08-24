<template>
	<div v-show="visible" :id="`${controlId}-container`" :class="resolvedClasses">
		<slot />
	</div>
</template>

<script>
	export default {
		name: 'StadiumContainer',

		props: {
			controlId: String,
			inline: Boolean,
			visible: Boolean,
			classes: null
		},

		computed: {
			resolvedClasses() {
				let resolvedClasses = this.classes;

				if (resolvedClasses) resolvedClasses += ' ';

				if (this.inline) resolvedClasses += 'inline-block-element';
				else resolvedClasses += 'block-element';

				return resolvedClasses;
			}
		}
	};
</script>
