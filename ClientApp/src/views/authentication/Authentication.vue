<template>
	<div>
		<div class="login-page-top-bar">
			<div class="stadium-logo-white"></div>
			<h1>{{ webAppName }}</h1>
		</div>
		<router-view />
	</div>
</template>

<script>
	export default {
		name: 'Authentication',

		data() {
			return {
				webAppName: import.meta.env.VITE_APP_TITLE
			};
		}
	};
</script>
