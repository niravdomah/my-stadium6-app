import * as types from '@/types/types.js';
import { isObject, isArray } from 'lodash';
import { getCaseInsensitiveField } from '@/utils/object-value-case-insensitive.js';

function coerceValueForType(typeName, source) {
	if (isObject(source) && !isArray(source) && types[typeName] && !(source instanceof types[typeName])) {
		let newTypeInstance = new types[typeName]();
		return bestEffortAssign(newTypeInstance, source);
	}

	return source;
}

function coerceListForType(itemTypeName, targetList, sourceList) {
	if (sourceList && !(sourceList instanceof Array)) throw new TypeError('Value must be an array.');

	if (
		!itemTypeName ||
		!sourceList ||
		sourceList.length === 0 ||
		(types[itemTypeName] && sourceList.every(i => i && i instanceof types[itemTypeName]))
	) {
		return sourceList;
	}

	for (let sourceItem of sourceList) {
		if (types[itemTypeName] && types[itemTypeName].prototype instanceof Array) {
			let nestedItemList = new types[itemTypeName]();
			targetList.push(coerceListForType(nestedItemList._getItemTypeName(), nestedItemList, sourceItem));
		} else {
			targetList.push(coerceValueForType(itemTypeName, sourceItem));
		}
	}

	return targetList;
}

function bestEffortAssign(target, source) {
	for (let targetField of Object.keys(target)) {
		let sourceField = getCaseInsensitiveField(source, targetField);
		if (sourceField === undefined) {
			target[targetField] = undefined;
		} else {
			let targetFieldTypeName = target._getFieldTypeName?.(targetField);

			if (targetFieldTypeName && types[targetFieldTypeName].prototype instanceof Array) {
				let nestedTargetList = new types[targetFieldTypeName]();
				target[targetField] = coerceListForType(nestedTargetList._getItemTypeName?.(), nestedTargetList, source[sourceField]);
			} else {
				target[targetField] = coerceValueForType(targetFieldTypeName, source[sourceField]);
			}
		}
	}

	return target;
}

export default {
	coerceValueForType,
	coerceListForType
};
