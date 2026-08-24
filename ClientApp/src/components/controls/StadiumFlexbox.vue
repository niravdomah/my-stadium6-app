<template>
	<div v-show="visible" :id="`${controlId}-container`" :class="resolvedClasses">
		<slot />
	</div>
</template>

<script>
	export default {
		name: 'StadiumFlexbox',

		props: {
			controlId: String,
			direction: String,
			inline: Boolean,
			visible: Boolean,
			classes: null
		},

		computed: {
			resolvedClasses() {
				let resolvedClasses = this.classes;

				if (resolvedClasses) resolvedClasses += ' ';

				if (this.inline) resolvedClasses += 'inline-flex-element';
				else resolvedClasses += 'flex-element';

				if (this.direction === 'row') resolvedClasses += ' row-flex-direction-element';
				else resolvedClasses += ' column-flex-direction-element';

				return resolvedClasses;
			}
		}
	};
</script>
