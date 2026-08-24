import { defineStore } from 'pinia';
import { useApplicationStore } from '@/stores/application.js';

import http from '@/setup/http.js';
import antiforgery from '@/utils/antiforgery.js';
import oidcHelper from '@/utils/oidc-helper.js';

export const useAuthenticationStore = defineStore('authentication', {
	state: () => ({
		authenticationType: null,
		user: null,
		oidcConfig: null,
		loginErrorMessage: null,
		checkValidSessionSetTimeoutId: null,
		oidcUserManager: null
	}),
	getters: {
		isAnonymousAuthentication: state => state.authenticationType === 'Anonymous',
		isCookieAuthentication: state => state.authenticationType === 'Cookie',
		isOAuthAuthentication: state => state.authenticationType === 'OAuth',
		isWindowsAuthentication: state => state.authenticationType === 'Windows',

		isAuthenticated: state => state.user?.id != null,
		isAdministrator: state => state.user?.isAdministrator === true,

		canLogout() {
			return this.isAuthenticated && (this.isCookieAuthentication || this.isOAuthAuthentication);
		},
		hasProfile() {
			return this.isAuthenticated && (this.isCookieAuthentication || this.isOAuthAuthentication || this.isWindowsAuthentication);
		},
		hasChangePassword() {
			return this.isAuthenticated && this.isCookieAuthentication;
		},

		accessiblePagesSet: state =>
			typeof state.user?.accessiblePages !== 'undefined'
				? new Set(Array.from(state.user?.accessiblePages, value => value.toLowerCase()))
				: new Set(state.user?.accessiblePages)
	},
	actions: {
		async refreshAuthenticationState() {
			let authenticationState = await http.get('api/Authentication/State');
			this.authenticationType = authenticationState.authenticationType;

			if (this.isCookieAuthentication) {
				if (authenticationState.userClaims?.id) {
					this.user = authenticationState.userClaims;
				} else {
					await this.logout();
				}
			} else if (this.isOAuthAuthentication) {
				this.oidcConfig = authenticationState.authenticationSettings;

				if (authenticationState.userClaims?.id) {
					this.user = authenticationState.userClaims;
				}
			} else if (this.isWindowsAuthentication) {
				if (authenticationState.userClaims?.id) {
					this.user = authenticationState.userClaims;
					await this.renewAntiforgeryToken();
				}
			}
		},
		async refreshOidcUser() {
			await http.post('api/Authentication/RefreshOidcUser');
		},
		async checkValidSession() {
			await http.get('api/Authentication/CheckValidSession');
		},
		scheduleCheckValidSession(sessionStateTimeout) {
			if (this.isCookieAuthentication && this.isAuthenticated) {
				if (this.checkValidSessionSetTimeoutId) clearTimeout(this.checkValidSessionSetTimeoutId);

				const timeoutErrorMargin = 5000;
				this.checkValidSessionSetTimeoutId = setTimeout(() => {
					this.checkValidSession();
				}, sessionStateTimeout + timeoutErrorMargin);
			}
		},
		async renewAntiforgeryToken() {
			await http.post('api/Authentication/RenewAntiforgeryToken');
		},
		async login(credentials) {
			this.loginErrorMessage = null;

			let response = await http.post('api/Authentication/Login', credentials, {
				headers: antiforgery.getAntiforgeryToken()
			});

			if (!response.username || response.status === 400) {
				this.user = null;
				this.loginErrorMessage = response.detail;

				return;
			}

			await this.renewAntiforgeryToken();

			this.user = response;
		},
		async logout() {
			await http.post('api/Authentication/Logout');

			await this.renewAntiforgeryToken();

			let isOAuthAuthentication = this.isOAuthAuthentication;

			this.user = null;
			const applicationStore = useApplicationStore();
			applicationStore.clearApplicationState();

			if (isOAuthAuthentication) {
				http.defaults.headers.common['Authorization'] = null;
				await oidcHelper.redirectToLogoutAsync();
			}
		}
	}
});
