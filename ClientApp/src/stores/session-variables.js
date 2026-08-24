import { defineStore } from 'pinia';
import { isEmpty, mapValues } from 'lodash';
import http from '@/setup/http.js';

export const useSessionVariablesStore = defineStore('sessionVariables', {
	state: () => ({
		sessionVariables: null
	}),
	actions: {
		async refreshSessionVariablesState() {
			let serializedSessionVariables = await http.get('api/Application/SessionVariables/All');
			this.sessionVariables =
				serializedSessionVariables && !isEmpty(serializedSessionVariables)
					? mapValues(serializedSessionVariables, v => JSON.parse(v))
					: null;
		},
		async setSessionVariable({ name, value }) {
			await http.post('api/Application/SessionVariables/Set', {
				name: name,
				serializedValue: JSON.stringify(value)
			});

			await this.refreshSessionVariablesState();
		},
		clearSessionVariablesState() {
			this.$reset();
		}
	}
});
