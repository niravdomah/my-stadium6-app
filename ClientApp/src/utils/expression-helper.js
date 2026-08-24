import { useExpressionResultsStore } from '@/stores/expression-results.js';

async function evaluate(expressionId, expressionFunction, logDebugInfo) {
	const expressionResultsStore = useExpressionResultsStore();
	if (logDebugInfo) {
		await expressionResultsStore.addToCache({
			expressionId: expressionId,
			expressionResult: await expressionFunction()
		});

		return expressionResultsStore.resultsByExpressionId[expressionId];
	}

	if (expressionId in expressionResultsStore.resultsByExpressionId) {
		let cachedResult = expressionResultsStore.resultsByExpressionId[expressionId];
		await expressionResultsStore.removeFromCache(expressionId);

		return cachedResult;
	}

	return await expressionFunction();
}

export default {
	evaluate
};
