import { defineStore } from 'pinia';
import { useAuthenticationStore } from '@/stores/authentication.js';
import http from '@/setup/http.js';
import antiforgery from '@/utils/antiforgery.js';

export const usePasswordStore = defineStore('password', {
	state: () => ({
		canResetPassword: null,
		canResetPasswordErrorMessage: null,
		changePasswordErrorMessage: null,
		updatePasswordErrorMessage: null,
		sendPasswordResetEmailErrorMessage: null
	}),
	actions: {
		async refreshCanResetPassword() {
			this.canResetPasswordErrorMessage = null;

			let response = await http.get('api/Authentication/Password/CanResetPassword');

			if (response.status === 400) {
				this.canResetPassword = false;
				this.canResetPasswordErrorMessage = response.detail;
				return;
			}

			this.canResetPassword = response;
		},
		async changePassword(changePasswordInfo) {
			this.changePasswordErrorMessage = null;

			let response = await http.post('api/Authentication/Password/ChangePassword', changePasswordInfo, {
				headers: antiforgery.getAntiforgeryToken()
			});

			if (response.status === 400) {
				this.changePasswordErrorMessage = response.detail;
				return;
			}

			const authenticationStore = useAuthenticationStore();
			await authenticationStore.renewAntiforgeryToken();
		},
		async sendPasswordResetEmail(resetPasswordInfo) {
			this.sendPasswordResetEmailErrorMessage = null;

			let response = await http.post(
				'api/Authentication/Password/SendPasswordResetEmail',
				{
					...resetPasswordInfo,
					passwordResetUrl: `${http.defaults.baseURL}Authentication/Login/UpdatePassword`
				},
				{
					headers: antiforgery.getAntiforgeryToken()
				}
			);

			if (response.status === 400) {
				this.sendPasswordResetEmailErrorMessage = response.detail;
			}
		},
		async updatePassword(updatePasswordInfo) {
			this.updatePasswordErrorMessage = null;

			let response = await http.post('api/Authentication/Password/UpdatePassword', updatePasswordInfo, {
				headers: antiforgery.getAntiforgeryToken()
			});

			if (response.status >= 400) {
				this.updatePasswordErrorMessage = response.detail;
				return;
			}

			const authenticationStore = useAuthenticationStore();
			await authenticationStore.renewAntiforgeryToken();
		}
	}
});
