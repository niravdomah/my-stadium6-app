<template>
	<div v-show="visible" :id="`${controlId}-container`" :class="classes">
		<span :id="controlId">{{ text }}</span>
	</div>
</template>

<script>
	export default {
		name: 'StadiumLabel',

		props: {
			controlId: String,
			text: null,
			visible: Boolean,
			classes: null
		}
	};
</script>
