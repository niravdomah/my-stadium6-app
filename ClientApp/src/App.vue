<template>
	<BusyIndicator id="busy-indicator" />
	<ApplicationHeader />

	<router-view v-if="!isInitializing && !isError"
				 :settings="settings"
				 :loggedInUser="loggedInUser"
				 :class="isMobile ? 'mobile' : 'desktop'"
				 class="container" />

	<div id="footer" class="footer">
		<span id="logged-in-user" v-if="isAuthenticated">Logged in{{ displayedUsername ? ` as ${displayedUsername}` : '' }}</span>
		<a href="https://stadium.software" class="created-by" target="_blank">Created in Stadium 6<span class="stadium-logo-blue"></span></a>
	</div>

	<MessageBox />
</template>

<script>
	import { mapState, mapWritableState } from 'pinia';
	import { useDeviceStore } from '@/stores/device.js';
	import { useApplicationStore } from '@/stores/application.js';
	import { useSettingsStore } from '@/stores/settings.js';
	import { useAuthenticationStore } from '@/stores/authentication.js';

	import ApplicationHeader from '@/views/layout/ApplicationHeader.vue';
	import BusyIndicator from '@/views/layout/BusyIndicator.vue';
	import MessageBox from '@/components/MessageBox.vue';

	import consoleLogGroupHelper from '@/utils/console-log-group-helper.js';

	export default {
		name: 'App',
		components: {
			ApplicationHeader,
			BusyIndicator,
			MessageBox
		},

		data() {
			return {
				isInitializing: false,
				isError: false
			};
		},

		computed: {
			...mapState(useSettingsStore, ['settings']),
			...mapState(useAuthenticationStore, ['user', 'isAuthenticated']),
			...mapWritableState(useDeviceStore, ['isMobile']),

			loggedInUser() {
				return {
					id: this.user?.id,
					email: this.user?.email,
					name: this.user?.name,
					roles: this.user?.roles || []
				};
			},

			displayedUsername() {
				if (!this.user) return null;

				if (this.user.email) return this.user.email;

				if (this.user.username) return this.user.username;

				return null;
			}
		},

		async created() {
			if (import.meta.env.VITE_APP_DEBUG === 'true') {
				console.warn('Application is running in Debug mode.');
				console.log('Executing... Please wait for the Preview Logs to be written.');
				consoleLogGroupHelper.pollClosedGroups();
			}

			const applicationStore = useApplicationStore();
			const authenticationStore = useAuthenticationStore();
			try {
				applicationStore.isAppBusy = true;
				this.isInitializing = true;

				let isAnonymousAuthentication = authenticationStore.isAnonymousAuthentication;
				let isAuthenticated = authenticationStore.isAuthenticated;
				if (isAnonymousAuthentication || isAuthenticated) await applicationStore.refreshApplicationState();
			} catch (error) {
				this.isError = true;
				applicationStore.isAppBusy = false;

				throw error;
			} finally {
				this.isInitializing = false;
			}
		},

		mounted() {
			let mediaQueryList = window.matchMedia('(max-width: 750px)');
			this.refreshIsMobile(mediaQueryList);
			mediaQueryList.addEventListener('change', this.refreshIsMobile);
		},

		methods: {
			refreshIsMobile(mediaQueryList) {
				this.isMobile = mediaQueryList.matches;
			}
		}
	};
</script>

<style lang="scss">
	@import './assets/fontawesome-all.css';
	@import './assets/bootstrap-substitute.css';
	@import 'vue-datepicker-next/index.css';
	@import './assets/stadium-layout.css';
	@import './assets/themes/current-theme.scss';
	@import './assets/application-custom.scss';
</style>
