<template>
	<div class="container">
		<div class="error-page-container">
			<div class="error-content-container">
				<h1 id="error-page-header">Page access not allowed</h1>
				<p id="error-page-message">{{ displayMessage }}</p>
				<p v-if="pageName">
					Page: <span id="error-page-page-name">{{ pageName }}</span>
				</p>
				<router-link to="/" class="btn btn-lg btn-link"> BACK TO {{applicationUrl}} </router-link>
			</div>
			<div class="error-image unauthorized-error-image"></div>
		</div>
	</div>
</template>

<script>
	import { useApplicationStore } from '@/stores/application.js';
	import navigation from '@/utils/navigation.js';

	export default {
		name: 'ForbiddenError',

		computed: {
			pageName() {
				return this.$route.query.pageName;
			},
			errorMessage() {
				return this.$route.query.errorMessage;
			},
			displayMessage() {
				return (
					this.errorMessage ??
					"You don't have any roles that can view this page. If you need to get access to this page, please contact your administrator."
				);
			},
			applicationUrl() {
				return navigation.getWebAppUrl();
			}
		},

		mounted() {
			const applicationStore = useApplicationStore();
			applicationStore.isAppBusy = false;
		}
	};
</script>
