import { isNavigationFailure, NavigationFailureType } from 'vue-router';
import { getCaseInsensitiveValue } from '@/utils/object-value-case-insensitive.js';

function getWebAppUrl() {
	return `${location.origin}${import.meta.env.BASE_URL}`;
}

function isRelativeUrl(url) {
	return url != null && url.startsWith('/') && !url.startsWith('//');
}

function getCompleteUrl(url) {
	if (!url) return url;

	validateUrl(url);

	if (url.indexOf(':') !== -1) return url;

	if (url.startsWith('www.')) return '//' + url;

	return url;
}

function getQueryParameter(route, parameterName) {
	let queryParameters = route.query;
	return getCaseInsensitiveValue(queryParameters, parameterName);
}

function encodeURIParameter(parameterValue) {
	return parameterValue || parameterValue === false ? encodeURIComponent(parameterValue) : '';
}

function browseTo(router, url, openInNewWindow) {
	let resolvedUrl = url ?? 'about:blank';

	if (openInNewWindow) {
		let newTab = window.open();

		if (!newTab) {
			throw new Error('A tab or window could not be opened (Please check whether the browser allows pop-ups for this site.)');
		}

		newTab.opener = null;

		newTab.location = isRelativeUrl(resolvedUrl) ? router.resolve(resolvedUrl).href : resolvedUrl;
	} else if (isRelativeUrl(resolvedUrl)) {
		router.push(resolvedUrl).catch(error => {
			if (!isNavigationFailure(error, NavigationFailureType.duplicated)) throw error;
		});
	} else {
		window.location = resolvedUrl;
	}
}

function validateUrl(url) {
	let urlValidatorRegex = /[a-z]{1}/i;

	if (!urlValidatorRegex.test(url)) {
		throw new Error(`${url} is not a valid url.`);
	}
}

export default {
	getWebAppUrl,
	isRelativeUrl,
	getCompleteUrl,
	getQueryParameter,
	encodeURIParameter,
	browseTo
};
