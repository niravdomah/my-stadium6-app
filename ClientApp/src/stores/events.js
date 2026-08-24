import { defineStore } from 'pinia';
import { useValidationsStore } from '@/stores/validations.js';
import debugLogger from '@/setup/debug-logger.js';
import htmlDomHelper from '@/utils/html-dom-helper.js';
import * as signalR from '@microsoft/signalr';

export const useEventsStore = defineStore('events', {
	state: () => ({
		ongoingEvents: [],
		debugLoggerConnectionStartPromise: null,
		debugLoggerConnectionStopPromise: null
	}),
	getters: {
		hasOngoingEvents: state => state.ongoingEvents.length > 0
	},
	actions: {
		addEvent(eventId) {
			this.ongoingEvents.push(eventId);
		},
		removeEvent(eventId) {
			let index = this.ongoingEvents.indexOf(eventId);
			if (index >= 0) this.ongoingEvents.splice(index, 1);
		},
		async executeEvent({ handler, eventId, doneCallback, controlIdsToValidate }) {
			try {
				if (controlIdsToValidate) {
					let visibleControlIdsToValidate = controlIdsToValidate.filter(id => {
						return htmlDomHelper.isVisible(document.getElementById(`${id}-container`));
					});

					const validationsStore = useValidationsStore();
					await validationsStore.validateControls(visibleControlIdsToValidate);

					let invalidControlIds = validationsStore.invalidControlIdsIn(visibleControlIdsToValidate);
					if (invalidControlIds.length > 0) {
						htmlDomHelper.scrollTo(document.getElementById(`${invalidControlIds[0]}-container`));
						return;
					}
				}

				this.addEvent(eventId);

				if (import.meta.env.VITE_APP_DEBUG === 'true') {
					if (this.debugLoggerConnectionStopPromise) await this.debugLoggerConnectionStopPromise;
					if (this.debugLoggerConnectionStartPromise) await this.debugLoggerConnectionStartPromise;

					if (debugLogger.state === signalR.HubConnectionState.Disconnected) {
						this.debugLoggerConnectionStartPromise = debugLogger.start();
						await this.debugLoggerConnectionStartPromise;
						this.debugLoggerConnectionStartPromise = null;
					}
				}

				await handler();
			} finally {
				try {
					if (doneCallback) doneCallback();
				} finally {
					this.removeEvent(eventId);

					if (import.meta.env.VITE_APP_DEBUG === 'true') {
						if (!this.hasOngoingEvents && debugLogger.state === signalR.HubConnectionState.Connected) {
							this.debugLoggerConnectionStopPromise = debugLogger.stop();
							await this.debugLoggerConnectionStopPromise;
							this.debugLoggerConnectionStopPromise = null;
						}
					}
				}
			}
		},
		clearEventsState() {
			this.$reset();
		}
	}
});
