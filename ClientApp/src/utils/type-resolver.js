function toString(value) {
	if (value == null) return value;

	return typeof value === 'object' ? JSON.stringify(value) : String(value);
}

function toBoolean(value) {
	if (!value) return false;

	if (value === '0') return false;

	if (value === 1 || value === '1') return true;

	try {
		const parsedValue = JSON.parse(value.toString().toLowerCase());
		if (typeof parsedValue === 'boolean') return parsedValue;

		throw new Error();
	} catch (_error) {
		throw new Error(`Trying to parse non-boolean value ${value} to boolean.`);
	}
}

function toArray(value) {
	if (value === '') return [];

	return typeof value === 'string' && value.startsWith('[') ? JSON.parse(value) : value;
}

function toArrayCoerce(value) {
	if (value == null) return [];

	if (typeof value === 'string') {
		return value.startsWith('[') ? JSON.parse(value) : value.split(',');
	}

	if (value instanceof Array) return value;

	return [String(value)];
}

function toObject(value) {
	if (value === '') return {};

	return typeof value === 'string' && value.startsWith('{') ? JSON.parse(value) : value;
}

export default {
	toString,
	toBoolean,
	toArray,
	toArrayCoerce,
	toObject
};
