import notification from '@/setup/notification.js';
import { useApplicationStore } from '@/stores/application.js';

function getErrorMessage(error) {
	if (!error) return null;

	let errorMessage;
	if (error.response) {
		errorMessage = error.response.data?.title ?? error.message ?? error;

		if (!errorMessage && error.response.status) {
			errorMessage = `${error.response.status} error`;
		}

		if (error.response.data?.detail) {
			errorMessage += `\n${error.response.data.detail}`;
		}
	} else if (error.request) {
		errorMessage = error.request;
	} else if (error.message) {
		errorMessage = error.message;
	} else {
		errorMessage = error;
	}

	if (error instanceof TypeError) {
		errorMessage = 'Type error: ' + errorMessage;
	}

	if (error.breadcrumb) {
		return `${errorMessage}

At ${error.breadcrumb}`;
	}

	return String(errorMessage);
}

function invoke(callback) {
	try {
		return callback();
	} catch (error) {
		const applicationStore = useApplicationStore();
		applicationStore.isAppBusy = false;

		console.error(error);
		if (error.response) console.error(error.response);

		notification.showError(getErrorMessage(error));
	}
}

export default {
	getErrorMessage,
	invoke
};
