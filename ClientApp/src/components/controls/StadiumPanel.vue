<template>
	<div v-show="visible" :id="`${controlId}-container`" :class="classes">
		<div :id="controlId" :style="borderColour == null || borderColour === '' ? '' : borderStyle" class="panel panel-default">
			<div v-if="title != null" class="panel-heading">
				<span class="panel-title">{{ title }}</span>
			</div>
			<div class="panel-body">
				<slot />
			</div>
		</div>
	</div>
</template>

<script>
	export default {
		name: 'StadiumPanel',

		props: {
			controlId: String,
			borderColour: String,
			title: String,
			visible: Boolean,
			classes: null
		},

		computed: {
			borderStyle() {
				return {
					border: `1px ${this.borderColour} solid`
				};
			}
		}
	};
</script>
