import { createApp } from 'vue';
import { createPinia } from 'pinia';
import { useApplicationStore } from '@/stores/application.js';
import { useAuthenticationStore } from '@/stores/authentication.js';

import App from '@/App.vue';
import router from '@/router';
import errorHandling from '@/utils/error-handling.js';

import http from '@/setup/http.js';
import Toast from 'vue-toastification';
import notification from '@/setup/notification.js';
import globalScripts from '@/global-scripts.js';

import '@/setup/validation.js';
import '@/setup/date-time.js';
import '@/setup/array-extensions.js';

const app = createApp(App);

app.use(createPinia());
app.use(Toast, notification.options);
app.use(globalScripts);

app.provide('$notification', notification);
app.provide('$http', http);

const applicationStore = useApplicationStore();

window.onerror = function (message, _source, _lineno, _colno, error) {
	applicationStore.isAppBusy = false;

	console.error(error?.message || error);
	if (error.response) console.error(error.response);

	notification.showError(errorHandling.getErrorMessage(error) ?? message);
};

app.config.errorHandler = function (error, _vm, info) {
	applicationStore.isAppBusy = false;

	console.error(error?.message || error);
	if (error.response) console.error(error.response);

	notification.showError(errorHandling.getErrorMessage(error) ?? info);
};

window.addEventListener('pageshow', function (event) {
	if (event.persisted) {
		const authenticationStore = useAuthenticationStore();
		if (authenticationStore.isOAuthAuthentication) window.location.reload();
	}
});

let setupPromises = [];

if (import.meta.env.VITE_APP_DEBUG === 'true') {
	let debugLoggerSetupPromise = import('@/setup/debug-logger.js').then(debugLoggerModule => {
		app.provide('$debugLogger', debugLoggerModule.default);
	});
	setupPromises.push(debugLoggerSetupPromise);
}

const authenticationStore = useAuthenticationStore();
let refreshAuthenticationStatePromise = authenticationStore.refreshAuthenticationState();

setupPromises.push(refreshAuthenticationStatePromise);

await Promise.all(setupPromises);

app.use(router);
app.mount('#app');
