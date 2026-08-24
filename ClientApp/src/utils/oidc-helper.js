import { UserManager, WebStorageStateStore, InMemoryWebStorage } from 'oidc-client-ts';
import { useAuthenticationStore } from '@/stores/authentication.js';

async function redirectToLoginAsync() {
	let userManager = getUserManager();
	await userManager.signinRedirect({ state: window.location.href });
}

async function requestTokensAsync() {
	let userManager = getUserManager();
	return userManager.signinRedirectCallback();
}

async function redirectToLogoutAsync() {
	const authenticationStore = useAuthenticationStore();
	let oidcConfig = authenticationStore.oidcConfig;
	if (oidcConfig.oidcProvider === 'Google' || oidcConfig.oidcProvider === 'Auth0') {
		localStorage.clear();
	}

	let userManager = getUserManager();
	let user = await userManager.getUser();
	return userManager.signoutRedirect({
		id_token_hint: user?.id_token
	});
}

function getUserManager() {
	const authenticationStore = useAuthenticationStore();

	let userManager = authenticationStore.oidcUserManager;
	if (!userManager) {
		userManager = new UserManager(getUserManagerSettings())
		authenticationStore.oidcUserManager = userManager;
	}

	return userManager;
}

function getUserManagerSettings() {
	const authenticationStore = useAuthenticationStore();
	let oidcConfig = authenticationStore.oidcConfig;

	let userManagerSettings = {
		authority: oidcConfig.authority,
		client_id: oidcConfig.clientId,
		redirect_uri: oidcConfig.redirectUrl,
		post_logout_redirect_uri: oidcConfig.logoutRedirectUrl,
		response_type: oidcConfig.responseType,
		scope: oidcConfig.scope,
		userStore: new WebStorageStateStore({ store: new InMemoryWebStorage() })
	};

	if (oidcConfig.oidcProvider === 'Auth0') {
		userManagerSettings.extraQueryParams = {
			audience: oidcConfig.audience
		};
	}

	if (oidcConfig.oidcProvider === 'Google' || oidcConfig.oidcProvider === 'Auth0') {
		userManagerSettings.metadataSeed = {
			end_session_endpoint: oidcConfig.endSessionEndpoint
		};
	}

	return userManagerSettings;
}

export default {
	redirectToLoginAsync,
	requestTokensAsync,
	redirectToLogoutAsync
};
