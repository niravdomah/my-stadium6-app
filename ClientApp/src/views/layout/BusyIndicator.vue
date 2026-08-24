<template>
	<div v-show="isAppBusy || hasOngoingEvents" class="busy-indicator-container">
		<div class="busy-indicator"></div>
	</div>
</template>

<script>
	import { mapState } from 'pinia';
	import { useApplicationStore } from '@/stores/application.js';
	import { useEventsStore } from '@/stores/events.js';

	export default {
		name: 'BusyIndicator',

		computed: {
			...mapState(useApplicationStore, ['isAppBusy']),
			...mapState(useEventsStore, ['hasOngoingEvents'])
		}
	};
</script>

<style lang="scss" scoped>
	@import '@/assets/themes/material-design-style-sheet.scss';

	.busy-indicator-container {
		position: fixed;
		height: var(--LOADER-BAR-HEIGHT);
		top: 0;
		left: 0;
		width: 100%;
		z-index: 1000;
	}

	.busy-indicator {
		position: absolute;
		top: 0;
		left: 0;
		height: var(--LOADER-BAR-HEIGHT);
		background-color: var(--LOADER-BAR-COLOR);
		width: var(--LOADER-BAR-WIDTH);
		animation: loadingBarAnimation 2s ease-out infinite;
		z-index: 100;
	}

	@keyframes loadingBarAnimation {
		0% {
			left: 0;
		}

		50% {
			left: calc(100% - var(--LOADER-BAR-WIDTH));
		}

		100% {
			left: 0;
		}
	}
</style>
