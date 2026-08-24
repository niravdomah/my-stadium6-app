import { defineStore } from 'pinia';

export const useMessageBoxStore = defineStore('messageBox', {
	state: () => ({
		show: false,
		message: null,
		buttons: []
	}),
	actions: {
		showMessageBox(properties) {
			this.message = properties.message;
			this.buttons = properties.buttons;
			this.show = true;
		},
		hideMessageBox() {
			this.$reset();
		}
	}
});
