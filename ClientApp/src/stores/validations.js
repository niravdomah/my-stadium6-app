import { defineStore } from 'pinia';

export const useValidationsStore = defineStore('validations', {
	state: () => ({
		controlIdsToValidate: [],
		invalidControlIds: []
	}),
	getters: {
		invalidControlIdsIn: state => controlIds => {
			return controlIds.filter(id => state.invalidControlIds.includes(id));
		}
	},
	actions: {
		validateControl(controlId) {
			if (!this.controlIdsToValidate.includes(controlId)) {
				this.controlIdsToValidate.push(controlId);
			}
		},
		validateControls(controlIds) {
			controlIds.forEach(id => this.validateControl(id));
		},
		removeControlIdToValidate(controlId) {
			let index = this.controlIdsToValidate.indexOf(controlId);
			if (index >= 0) this.controlIdsToValidate.splice(index, 1);
		},
		addInvalidControlId(invalidControlId) {
			if (!this.invalidControlIds.includes(invalidControlId)) {
				this.invalidControlIds.push(invalidControlId);
			}
		},
		removeInvalidControlId(invalidControlId) {
			let index = this.invalidControlIds.indexOf(invalidControlId);
			if (index >= 0) this.invalidControlIds.splice(index, 1);
		},
		clearValidationsState() {
			this.$reset();
		}
	}
});
