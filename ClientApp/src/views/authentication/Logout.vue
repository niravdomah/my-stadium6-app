<template>
	<div class="logout-page">
		<div class="logout-page-container">
			<div class="logout-content-container">
				<h1>
					<b>You have logged out</b>
				</h1>
				<p>
					<b>Log back in if you want</b>
				</p>
				<router-link to="/" class="btn text-uppercase back-button">Login</router-link>
			</div>
			<div class="logout-image"></div>
		</div>
	</div>
</template>

<script>
	import { useApplicationStore } from '@/stores/application.js';

	export default {
		name: 'logout',

		mounted() {
			const applicationStore = useApplicationStore();
			applicationStore.isAppBusy = false;
		}
	};
</script>
