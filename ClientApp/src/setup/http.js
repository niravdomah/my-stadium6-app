import axios from 'axios';
import router from '@/router';
import navigation from '@/utils/navigation.js';
import { useAuthenticationStore } from '@/stores/authentication.js';

axios.defaults.baseURL = navigation.getWebAppUrl();
axios.defaults.headers.post['Content-Type'] = 'application/json';
axios.defaults.headers.put['Content-Type'] = 'application/json';
axios.defaults.withCredentials = true;
axios.defaults.adapter = 'xhr';

axios.defaults.transformRequest = [
	function (data, headers) {
		if (data == null) {
			return data;
		}

		let hasDefaultContentType = headers['Content-Type'] == null;

		if (hasDefaultContentType || headers['Content-Type'] === 'application/json') {
			return JSON.stringify(data);
		}

		return data;
	}
];

axios.interceptors.response.use(
	function (response) {
		let cookieAuthenthenticationTimeout = response?.headers?.['cookie-authentication-timeout'];
		if (cookieAuthenthenticationTimeout) {
			let sessionStateTimeout = parseInt(cookieAuthenthenticationTimeout, 10);
			if (Number.isInteger(sessionStateTimeout)) {
				const authenticationStore = useAuthenticationStore();
				authenticationStore.scheduleCheckValidSession(sessionStateTimeout);
			}
		}

		const contentLengthHeader = response.headers['content-length'];
		if (!response.data && contentLengthHeader) {
			const contentLength = parseInt(contentLengthHeader, 10);
			if (contentLength > 0) {
				return Promise.reject(new Error('The response was too large for the browser to load. Please narrow down your request.'));
			}
		}

		if (response.config?.responseType === 'arraybuffer' || response.config?.responseType === 'blob') {
			return response.data;
		}

		return response?.data;
	},
	async function (error) {
		if (error.response) {
			if (error.response.status === 400) {
				return error.response?.data;
			}

			if (error.response.status === 401) {
				const authenticationStore = useAuthenticationStore();
				authenticationStore.logout();
				authenticationStore.refreshAuthenticationState();

				router.go();
				throw error;
			}

			if (error.response.status === 403) {
				router.push({
					name: 'Forbidden',
					params: {
						pageName: getPageName(error.response.config?.url),
						errorMessage: error.response.data?.title
					}
				});
				throw error;
			}

			if (error.response.status === 413) {
				throw new Error(`The amount of data to be transferred exceeds the allowed limit. Please contact the application administrator.
Possible solutions to this problem are:
* Increase the limit using StadiumApplicationManager -> Application -> Configuration -> Maximum File Upload Size`);
			}

			if (error.response.status === 500) {
				const responseData = error.response.data;

				if (responseData && typeof responseData === 'object' && !Array.isArray(responseData) && responseData.message) {
					error.message = responseData.message;
					if (responseData.detail) {
						error.message += '\n' + responseData.detail;
					}
				}
				throw error;
			}
		}

		throw error;
	}
);

function getPageName(apiUrl) {
	if (!apiUrl || !apiUrl.startsWith('api/pages/')) return null;

	let startIndex = apiUrl.toLowerCase().indexOf('api/pages/') + 'api/pages/'.length;

	if (startIndex >= 0) {
		let endIndex = apiUrl.indexOf('/', startIndex);

		return endIndex >= 0 ? apiUrl.substring(startIndex, endIndex) : apiUrl.substring(startIndex);
	}

	return null;
}

export default axios;
