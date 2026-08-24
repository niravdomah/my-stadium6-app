function getCaseInsensitiveValue(object, caseInsensitiveKey) {
	return object[getCaseInsensitiveField(object, caseInsensitiveKey)];
}

function getCaseInsensitiveField(object, caseInsensitiveKey) {
	return Object.keys(object).find(k => k.toLowerCase() === caseInsensitiveKey.toLowerCase());
}

export { getCaseInsensitiveValue, getCaseInsensitiveField };