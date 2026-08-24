import { defineStore } from 'pinia';
import http from '@/setup/http.js';

export const useConfigStore = defineStore('config', {
	state: () => ({
		dateTimeSerializationFormat: null,
		timeSpanSerializationFormat: null,
		dateFormat: null,
		timeFormat: null,
		dataGridDisplayFormats: null,
		maxFileUploadSizeInKB: null
	}),
	actions: {
		async refreshConfigState() {
			let config = await http.get('api/Application/Config/All');

			this.dateTimeSerializationFormat = config.dateTimeSerializationFormat;
			this.timeSpanSerializationFormat = config.timeSpanSerializationFormat;
			this.dateFormat = config.dateFormat;
			this.timeFormat = config.timeFormat;
			this.dataGridDisplayFormats = config.dataGridDisplayFormats;
			this.maxFileUploadSizeInKB = config.maxFileUploadSizeInKB;
		},
		clearConfigState() {
			this.$reset();
		}
	}
});
