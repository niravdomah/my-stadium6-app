import { useConfigStore } from '@/stores/config.js';

function getDayjsDateFormat() {
	const configStore = useConfigStore();
	const dateFormat = configStore.dateFormat;
	return convertToDayjsFormat(dateFormat);
}

function getDayjsTimeFormat() {
	const configStore = useConfigStore();
	const timeFormat = configStore.timeFormat;
	return convertToDayjsFormat(timeFormat);
}

function convertToDayjsFormat(dateTimeFormat) {
	return dateTimeFormat
		?.replace(/\\\//g, '/')
		.replace(/\\\./g, '.')
		.replace(/\\:/g, ':')
		.replace(/\\T/g, 'T')
		.replace(/\\Z/g, '[Z]')
		.replace(/y/g, 'Y')
		.replace(/d/g, 'D')
		.replace(/f/g, 'S')
		.replace('tt', 'A');
}

export default {
	getDayjsDateFormat,
	getDayjsTimeFormat,
	convertToDayjsFormat
};
