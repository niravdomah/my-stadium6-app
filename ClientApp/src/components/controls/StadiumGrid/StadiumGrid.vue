<template>
	<div v-show="visible" :id="`${controlId}-container`" :class="classes">
		<slot />
	</div>
</template>

<script>
	export default {
		name: 'StadiumGrid',

		props: {
			controlId: String,
			visible: Boolean,
			classes: null
		}
	};
</script>
