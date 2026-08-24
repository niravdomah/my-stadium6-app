<template>
	<div class="container">
		<div class="error-page-container">
			<div class="error-content-container">
				<h1 id="error-page-header">Unauthorized</h1>
				<p id="error-page-message">You are not authenticated. Please contact your administrator.</p>
			</div>
			<div class="error-image unauthorized-error-image"></div>
		</div>
	</div>
</template>

<script>
	import { useApplicationStore } from '@/stores/application.js';

	export default {
		name: 'UnauthenticatedError',

		mounted() {
			const applicationStore = useApplicationStore();
			applicationStore.isAppBusy = false;
		}
	};
</script>
