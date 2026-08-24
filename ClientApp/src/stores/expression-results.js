import { defineStore } from 'pinia';

export const useExpressionResultsStore = defineStore('expressionResults', {
	state: () => ({
		resultsByExpressionId: {}
	}),
	actions: {
		addToCache({ expressionId, expressionResult }) {
			this.resultsByExpressionId[expressionId] = expressionResult;
		},
		removeFromCache(expressionId) {
			delete this.resultsByExpressionId[expressionId];
		},
		clearExpressionResultsState() {
			this.$reset();
		}
	}
});
