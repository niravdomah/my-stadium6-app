<template>
	<div v-show="visible" :id="`${controlId}-container`" :class="classes">
		<slot />
	</div>
</template>

<script>
	export default {
		name: 'StadiumGridRepeaterItem',

		props: {
			controlId: String,
			list: null,
			listItem: null,
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

		mounted() {
			this.$emit('itemload', null, this.listItem);
		}
	};
</script>
