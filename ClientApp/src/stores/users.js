import { defineStore } from 'pinia';
import http from '@/setup/http.js';
import antiforgery from '@/utils/antiforgery.js';

export const useUsersStore = defineStore('users', {
	state: () => ({
		users: null,
		userErrors: null,
		deleteUserErrorMessage: null
	}),
	actions: {
		async refreshUsersState() {
			this.userErrors = null;
			this.users = await http.get('api/Administration/Users/All');
		},
		async addUser(user) {
			this.userErrors = null;

			let response = await http.post('api/Administration/Users/Add', user, {
				headers: antiforgery.getAntiforgeryToken()
			});

			if (response.status === 400) {
				this.userErrors = response.errors;
				return;
			}

			await this.refreshUsersState();
		},
		async editUser(user) {
			this.userErrors = null;

			let response = await http.put('api/Administration/Users/Edit', user, {
				headers: antiforgery.getAntiforgeryToken()
			});

			if (response.status === 400) {
				this.userErrors = response.errors;
				return;
			}

			await this.refreshUsersState();
		},
		async updateIsAdministrator(user) {
			await http.put('api/Administration/Users/EditIsAdministrator', user, {
				headers: antiforgery.getAntiforgeryToken()
			});

			await this.refreshUsersState();
		},
		async deleteUser(userId) {
			this.deleteUserErrorMessage = null;

			let response = await http.delete(`api/Administration/Users/Delete?id=${userId}`, {
				headers: antiforgery.getAntiforgeryToken()
			});

			if (response.status === 400) {
				this.deleteUserErrorMessage = response.detail;
				return;
			}

			await this.refreshUsersState();
		},
		clearUsersState() {
			this.$reset();
		}
	}
});
