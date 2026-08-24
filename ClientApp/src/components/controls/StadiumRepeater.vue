<template>
	<div v-show="visible" :id="`${controlId}-container`" :class="resolvedClasses">
		<slot />
	</div>
</template>

<script>
	export default {
		name: 'StadiumRepeater',

		props: {
			controlId: String,
			list: null,
			listItem: null,
			inline: Boolean,
			visible: Boolean,
			classes: null
		},

		emits: ['itemload'],

		watch: {
			list: {
				handler: function (_newList, oldList) {
					if (oldList !== null && oldList !== undefined && !oldList.includes(this.listItem))
						this.$emit('itemload', null, this.listItem);
				},
				immediate: true
			}
		},

		computed: {
			resolvedClasses() {
				let resolvedClasses = this.classes;

				if (this.inline) resolvedClasses += ' inline-block-element';
				else resolvedClasses += ' block-element';

				return resolvedClasses;
			}
		},

		mounted() {
			this.$emit('itemload', null, this.listItem);
		}
	};
</script>
