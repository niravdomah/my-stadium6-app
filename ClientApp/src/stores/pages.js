import { defineStore } from 'pinia';
import http from '@/setup/http.js';
import antiforgery from '@/utils/antiforgery.js';

export const usePagesStore = defineStore('pages', {
	state: () => ({
		pages: null,
		pageErrors: null
	}),
	actions: {
		async refreshPagesState() {
			this.pageErrors = null;
			this.pages = await http.get('api/Administration/Pages/All');
		},
		async editPage(page) {
			this.pageErrors = null;

			let response = await http.put('api/Administration/Pages/Edit', page, {
				headers: antiforgery.getAntiforgeryToken()
			});

			if (response.status === 400) {
				this.pageErrors = response.errors;
				return;
			}

			await this.refreshPagesState();
		},
		clearPagesState() {
			this.$reset();
		}
	}
});
