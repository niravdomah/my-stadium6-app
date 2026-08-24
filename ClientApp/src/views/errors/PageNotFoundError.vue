<template>
	<div class="container">
		<div class="error-page-container">
			<div class="error-content-container">
				<h1 id="error-page-header">404 - This page does not exist</h1>
				<router-link to="/" class="btn btn-lg btn-link"> BACK TO {{applicationUrl}} </router-link>
			</div>
			<div class="error-image page-not-found-image"></div>
		</div>
	</div>
</template>

<script>
	import { useApplicationStore } from '@/stores/application.js';
	import navigation from '@/utils/navigation.js';

	export default {
		name: 'PageNotFoundError',

		mounted() {
			const applicationStore = useApplicationStore();
			applicationStore.isAppBusy = false;
		},

		computed: {
			applicationUrl() {
				return navigation.getWebAppUrl();
			}
		}
	};
</script>
