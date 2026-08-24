import http from '@/setup/http.js';

function isFileToken(value) {
	return value?.['$type']?.startsWith('Twenty57.Stadium.WebApp.Spa.Core.Entities.FileToken');
}

function getFileAsync(fileToken) {
	const contentParam = typeof fileToken === 'string' ? fileToken : JSON.stringify(fileToken);

	return http
		.get('api/FileHandler', {
			params: { content: contentParam },
			responseType: 'arraybuffer'
		})
		.then(arrayBufferResult => {
			var binary = '';
			let bytes = new Uint8Array(arrayBufferResult);
			for (var i = 0; i < bytes.byteLength; i++) {
				binary += String.fromCharCode(bytes[i]);
			}
			return binary;
		});
}

function downloadFileAsync(content, fileName) {
	return http.post(
		'api/FileHandler/Download',
		{
			content: content,
			fileName: fileName
		},
		{
			responseType: 'blob'
		}
	);
}

export default {
	isFileToken,
	getFileAsync,
	downloadFileAsync
};
