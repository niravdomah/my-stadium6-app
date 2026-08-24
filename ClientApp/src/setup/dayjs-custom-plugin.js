import dayjsHelper from '@/utils/dayjs-helper.js';

export default (_option, dayjsClass, _dayjsFactory) => {
	dayjsClass.prototype.dateOnly = function () {
		let dateString = this.format('YYYY/MM/DD');
		return this.utc(dateString);
	};

	dayjsClass.prototype.toDateTimeString = function () {
		const dayjsDateFormat = dayjsHelper.getDayjsDateFormat();
		const dayjsTimeFormat = dayjsHelper.getDayjsTimeFormat();
		return this.format(`${dayjsDateFormat} ${dayjsTimeFormat}`);
	};

	dayjsClass.prototype.toDateString = function () {
		const dayjsDateFormat = dayjsHelper.getDayjsDateFormat();
		return this.format(dayjsDateFormat);
	};

	dayjsClass.prototype.toTimeString = function () {
		const dayjsTimeFormat = dayjsHelper.getDayjsTimeFormat();
		return this.format(dayjsTimeFormat);
	};
};
