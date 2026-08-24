<template>
	<div>Logging in...</div>
</template>

<script>
	import oidcHelper from '@/utils/oidc-helper.js';
	import navigation from '@/utils/navigation.js';
	import antiforgery from '@/utils/antiforgery.js';
	import { mapState, mapActions } from 'pinia';
	import { useApplicationStore } from '@/stores/application.js';
	import { useAuthenticationStore } from '@/stores/authentication.js';
	import { trimEnd } from 'lodash';

	export default {
		name: 'callback',
		inject: ['$http'],

		async mounted() {
			try {
				let user = await oidcHelper.requestTokensAsync();
				let returnUrl = user.state;
				let bearerToken =
					this.oidcConfig.oidcProvider === 'Google' || this.oidcConfig.oidcProvider === 'AzureAD' ? user.id_token : user.access_token;

				this.$http.defaults.headers.common['Authorization'] = `Bearer ${bearerToken}`;
				await this.refreshOidcUser();
				await this.refreshAuthenticationState();
				await this.renewAntiforgeryToken();

				let antiforgeryToken = antiforgery.getAntiforgeryToken();
				this.$http.defaults.headers.common['X-CSRF-TOKEN'] = antiforgeryToken['X-CSRF-TOKEN'];

				const applicationStore = useApplicationStore();
				await applicationStore.refreshApplicationState();

				let relativeReturnUrl = this.getRelativeReturnUrl(returnUrl);
				window.history.replaceState({}, '', relativeReturnUrl);
				this.$router.replace(relativeReturnUrl);
			} catch (error) {
				if (error?.message == 'login_required' || error?.message == 'No matching state found in storage') {
					window.top.location.replace(navigation.getWebAppUrl());
					return;
				}

				throw error;
			}
		},

		computed: {
			...mapState(useAuthenticationStore, ['oidcConfig'])
		},

		methods: {
			...mapActions(useAuthenticationStore, ['refreshAuthenticationState', 'refreshOidcUser', 'renewAntiforgeryToken']),

			getRelativeReturnUrl(returnUrl) {
				let webAppUrl = trimEnd(navigation.getWebAppUrl(), '/');
				let relativeReturnUrl = returnUrl?.replace(webAppUrl, '') ?? '/';
				if (!relativeReturnUrl.startsWith('/')) relativeReturnUrl = '/' + relativeReturnUrl;

				return relativeReturnUrl;
			}
		}
	};
</script>
