import { isBoolean, isNumber, isArray, isString } from 'lodash';
import fileHandler from '@/utils/file-handler.js';
import dayjsHelper from '@/utils/dayjs-helper.js';
import dayjs from 'dayjs';

import { useConfigStore } from '@/stores/config.js';

function toDisplayText(value) {
	const configStore = useConfigStore();

	if (!configStore.dataGridDisplayFormats) return '';

	const { booleanFormat, arrayFormat, numberSeparator, decimalSeparator, maximumFractionDigits } = configStore.dataGridDisplayFormats;
	const dateFormat = configStore.dateFormat;
	const timeFormat = configStore.timeFormat;

	if (value == null || value === '') {
		return '';
	}

	if (isBoolean(value)) {
		const booleanFormats = booleanFormat.split(';;', 2);
		const trueDisplayText = booleanFormats[0];
		const falseDisplayText = booleanFormats[1];

		return value ? trueDisplayText : falseDisplayText;
	}

	if (isArray(value) || fileHandler.isFileToken(value)) {
		return arrayFormat;
	}

	if (isNumber(value)) {
		const parts = new Intl.NumberFormat('en-GB', {
			maximumFractionDigits
		}).formatToParts(value);

		return parts
			.map(p => {
				switch (p.type) {
					case 'decimal':
						return decimalSeparator;
					case 'group':
						return numberSeparator;
					default:
						return p.value;
				}
			})
			.join('');
	}

	if (isString(value)) {
		let dateTime = dayjs(value, dayjsHelper.convertToDayjsFormat(configStore.dateTimeSerializationFormat), true);
		if (dateTime.isValid()) {
			return dateTime.format(dayjsHelper.convertToDayjsFormat(dateFormat));
		}

		dateTime = dayjs(value, dayjsHelper.convertToDayjsFormat(configStore.timeSpanSerializationFormat), true);
		if (dateTime.isValid()) {
			return dateTime.format(dayjsHelper.convertToDayjsFormat(timeFormat));
		}
	}

	return value;
}

export { toDisplayText };
