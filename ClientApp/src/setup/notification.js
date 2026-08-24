import { useToast, TYPE } from 'vue-toastification';
import Notification from '@/components/Notification.vue';
import 'vue-toastification/dist/index.css';

const notificationContainer = document.getElementById('app-notifications');

const options = {
	container: notificationContainer,
	hideProgressBar: true,
	closeOnClick: false,
	draggable: false,
	transition: 'Vue-Toastification__fade',
	toastClassName: 'custom-toast',
	icon: false,
	toastDefaults: {
		[TYPE.SUCCESS]: {
			timeout: 5000,
			closeButton: false
		},
		[TYPE.INFO]: {
			timeout: 5000,
			closeButton: false
		},
		[TYPE.WARNING]: {
			timeout: false,
			closeButton: 'button'
		},
		[TYPE.ERROR]: {
			timeout: false,
			closeButton: 'button'
		}
	}
};

function getToastContent(message, title) {
	return {
		component: Notification,
		props: {
			message: message ?? '',
			title: title
		}
	};
}

function getCloseButtonOptions(closeButton) {
	if (closeButton == null) return {};

	return closeButton
		? {
				timeout: false,
				closeButton: 'button'
		  }
		: {
				timeout: 5000,
				closeButton: false
		  };
}

export default {
	options,
	showSuccess: function (message, title, closeButton) {
		const toast = useToast();
		toast.success(getToastContent(message, title), getCloseButtonOptions(closeButton));
	},
	showInfo: function (message, title, closeButton) {
		const toast = useToast();
		toast.info(getToastContent(message, title), getCloseButtonOptions(closeButton));
	},
	showWarning: function (message, title, closeButton) {
		const toast = useToast();
		toast.warning(getToastContent(message, title), getCloseButtonOptions(closeButton));
	},
	showError: function (message, title, closeButton) {
		const toast = useToast();
		toast.error(getToastContent(message, title), getCloseButtonOptions(closeButton));
	}
};
