import { defineStore } from 'pinia';
import http from '@/setup/http.js';

export const useSettingsStore = defineStore('settings', {
	state: () => ({
		settings: null
	}),
	actions: {
		async refreshSettingsState() {
			this.settings = await http.get('api/Application/Settings/All');
		},
		clearSettingsState() {
			this.$reset();
		}
	}
});
