import { defineStore } from 'pinia';
import http from '@/setup/http.js';
import antiforgery from '@/utils/antiforgery.js';

export const useRolesStore = defineStore('roles', {
	state: () => ({
		roles: null,
		roleErrors: null,
		deleteRoleErrorMessage: null
	}),
	actions: {
		async refreshRolesState() {
			this.roleErrors = null;
			this.roles = await http.get('api/Administration/Roles/All');
		},
		async addRole(role) {
			this.roleErrors = null;

			let response = await http.post('api/Administration/Roles/Add', role, {
				headers: antiforgery.getAntiforgeryToken()
			});

			if (response.status === 400) {
				this.roleErrors = response.errors;
				return;
			}

			await this.refreshRolesState();
		},
		async editRole(role) {
			this.roleErrors = null;

			let response = await http.put('api/Administration/Roles/Edit', role, {
				headers: antiforgery.getAntiforgeryToken()
			});

			if (response.status === 400) {
				this.roleErrors = response.errors;
				return;
			}

			await this.refreshRolesState();
		},
		async deleteRole(roleId) {
			this.deleteRoleErrorMessage = null;

			let response = await http.delete(`api/Administration/Roles/Delete?id=${roleId}`, {
				headers: antiforgery.getAntiforgeryToken()
			});

			if (response.status === 400) {
				this.deleteRoleErrorMessage = response.detail;
				return;
			}

			await this.refreshRolesState();
		},
		clearRolesState() {
			this.$reset();
		}
	}
});
