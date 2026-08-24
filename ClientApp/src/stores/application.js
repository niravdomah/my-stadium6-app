import { defineStore } from 'pinia';
import { useDeviceStore } from '@/stores/device.js';
import { useConfigStore } from '@/stores/config.js';
import { useSettingsStore } from '@/stores/settings.js';
import { useSessionVariablesStore } from '@/stores/session-variables.js';
import { useEventsStore } from '@/stores/events.js';
import { useValidationsStore } from '@/stores/validations.js';
import { useUsersStore } from '@/stores/users.js';
import { useRolesStore } from '@/stores/roles.js';
import { usePagesStore } from '@/stores/pages.js';

export const useApplicationStore = defineStore('application', {
	state: () => ({
		isAppBusy: false
	}),
	actions: {
		async refreshApplicationState() {
			const configStore = useConfigStore();
			let configPromise = configStore.refreshConfigState();

			const settingsStore = useSettingsStore();
			let settingPromise = settingsStore.refreshSettingsState();

			const sessionVariablesStore = useSessionVariablesStore();
			let sessionVariablesPromise = sessionVariablesStore.refreshSessionVariablesState();

			await Promise.all([configPromise, settingPromise, sessionVariablesPromise]);
		},
		clearApplicationState() {
			const deviceStore = useDeviceStore();
			deviceStore.clearDeviceState();

			const configStore = useConfigStore();
			configStore.clearConfigState();

			const settingsStore = useSettingsStore();
			settingsStore.clearSettingsState();

			const sessionVariablesStore = useSessionVariablesStore();
			sessionVariablesStore.clearSessionVariablesState();

			const eventsStore = useEventsStore();
			eventsStore.clearEventsState();

			const validationsStore = useValidationsStore();
			validationsStore.clearValidationsState();

			const usersStore = useUsersStore();
			usersStore.clearUsersState();

			const rolesStore = useRolesStore();
			rolesStore.clearRolesState();

			const pagesStore = usePagesStore();
			pagesStore.clearPagesState();

			this.$reset();
		}
	}
});
