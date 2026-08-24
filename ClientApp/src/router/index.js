import { createRouter, createWebHistory } from 'vue-router';
import { trimStart } from 'lodash';
import { useAuthenticationStore } from '@/stores/authentication.js';
import routes from '@/router/routes.js';
import oidcHelper from '@/utils/oidc-helper.js';
import errorHandling from '@/utils/error-handling.js';
import notification from '@/setup/notification.js';
import { useApplicationStore } from '@/stores/application.js';

const router = createRouter({
	history: createWebHistory(import.meta.env.BASE_URL),
	routes: routes
});

router.beforeEach(async (to, _from, next) => {
	const authenticationStore = useAuthenticationStore();
	let pageName = to.name ? to.name : trimStart(to.path, '/');

	let isAnonymousAuthentication = authenticationStore.isAnonymousAuthentication;
	if (isAnonymousAuthentication) {
		let isAdminPage = to.meta.requiresAdministrator;
		let isLoginPage = to.name === 'Login';
		if (isLoginPage || isAdminPage) {
			next({ name: 'PageNotFound', params: [to.path] });
			return;
		}

		document.title = getPageTitle(pageName, to);
		next();
		return;
	}

	let isCookieAuthentication = authenticationStore.isCookieAuthentication;
	if (isCookieAuthentication) {
		if (!to.meta.allowAnonymous) {
			let isAuthenticated = authenticationStore.isAuthenticated;
			if (!isAuthenticated) {
				document.title = getPageTitle('Login');
				next({
					name: 'Login',
					query: { pageRoute: to.fullPath }
				});
				return;
			}

			if (!isAccessAllowed(to, authenticationStore)) {
				document.title = getPageTitle('Forbidden');
				next({
					name: 'Forbidden',
					query: { pageName: pageName }
				});
				return;
			}
		}
	}

	let isOAuthAuthentication = authenticationStore.isOAuthAuthentication;
	if (isOAuthAuthentication) {
		if (!to.meta.allowAnonymous) {
			let isAuthenticated = authenticationStore.isAuthenticated;
			if (!isAuthenticated) {
				await oidcHelper.redirectToLoginAsync().catch(error => {
					const applicationStore = useApplicationStore();
					applicationStore.isAppBusy = false;
					notification.showError('Could not redirect to OIDC signin: ' + errorHandling.getErrorMessage(error), 'OAuth Authority Error');
					throw error;
				});
				return;
			}

			if (!isAccessAllowed(to, authenticationStore)) {
				document.title = getPageTitle('Forbidden');
				next({
					name: 'Forbidden',
					query: { pageName: pageName }
				});
				return;
			}
		}
	}

	let isWindowsAuthentication = authenticationStore.isWindowsAuthentication;
	if (isWindowsAuthentication) {
		if (!to.meta.allowAnonymous) {
			let isAuthenticated = authenticationStore.isAuthenticated;

			if (!isAuthenticated) {
				document.title = getPageTitle('Unauthenticated');
				next({
					name: 'Unauthenticated'
				});
				return;
			}

			if (!isAccessAllowed(to, authenticationStore)) {
				document.title = getPageTitle('Forbidden');
				next({
					name: 'Forbidden',
					query: { pageName: pageName }
				});
				return;
			}
		}
	}

	document.title = getPageTitle(pageName, to);
	next();
});

function getPageTitle(pageName, routeObject) {
	let title = routeObject?.matched.find(record => record.meta.title)?.meta.title;
	return `${title ?? pageName} - ${import.meta.env.VITE_APP_TITLE}`;
}

function isAccessAllowed(to, authenticationStore) {
	let isAdminPage = to.matched.some(record => record.meta.requiresAdministrator);
	if (isAdminPage) {
		let isUserAdministrator = authenticationStore.isAdministrator;
		return isUserAdministrator;
	}

	let pageName = trimStart(to.path, '/');
	let isAccessiblePage = authenticationStore.accessiblePagesSet.has(pageName.toLowerCase());
	return isAccessiblePage;
}

export default router;
