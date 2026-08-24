<template>
	<div v-show="visible" :id="`${controlId}-container`" :class="resolvedClasses">
		<form
			:id="controlId"
			class="form-group input-group"
			:class="{ 'is-dragover': isDragOver }"
			@submit.prevent
			@dragover.prevent.stop="isDragOver = true"
			@dragenter.prevent.stop="isDragOver = true"
			@dragleave.prevent.stop="isDragOver = false"
			@drop.prevent.stop="onDrop"
		>
			<label>
				<input :id="`${controlId}-input-file`" :value="fileInputValue" type="file" name="file" multiple @change="onChange" />
			</label>
			<div :id="`${controlId}-container-drop-div`" class="upload-file-container-drop-div">
				<i class="glyphicon glyphicon-plus"></i>
				Drop files here
			</div>
		</form>

		<span :id="`${controlId}-control-status`" class="control-status-span validation-error">{{ errorMessage }}</span>

		<div
			v-for="fileInfo in fileInfos"
			:key="fileInfo.key"
			:id="`${controlId}-${fileInfo.key}`"
			:title="fileInfo.name"
			:class="getFileContainerClasses(fileInfo)"
			class="single-file-upload-container"
			@click="onFileContainerClick(fileInfo)"
		>
			<div class="single-file-file-name-container">
				<span v-if="fileInfo.errorMessage" class="single-file-upload-icon single-file-upload-error-icon" />
				<span
					v-if="!fileInfo.errorMessage && fileInfo.progressPercentage !== 100"
					:id="`${controlId}-${fileInfo.key}-cancel-upload`"
					class="single-file-upload-icon single-file-upload-cancel-icon"
					@click.stop="cancelUpload(fileInfo)"
				/>
				<span
					v-if="!fileInfo.errorMessage && !fileInfo.isSuccess"
					:id="`${controlId}-${fileInfo.key}-progress-percentage`"
					class="single-file-upload-percentage"
					>{{ fileInfo.progressPercentage + '%' }}</span
				>
				<span v-if="fileInfo.isSuccess" class="single-file-upload-delete-icon" @click="deleteFile(fileInfo)" />
				<span v-if="fileInfo.isSuccess && !fileInfo.errorMessage" class="single-file-upload-icon single-file-upload-success-icon" />
				<span :id="`${controlId}-${fileInfo.key}-upload-filename`" class="single-file-upload-filename">{{ fileInfo.name }}</span>
			</div>
			<div class="progress">
				<div
					:style="{ width: fileInfo.progressPercentage + '%' }"
					:class="getFileProgressBarClass(fileInfo)"
					class="progress-bar"
					role="progressbar"
					aria-valuemin="0"
					aria-valuemax="100"
				></div>
			</div>
			<div :id="`${controlId}-${fileInfo.key}-status`" class="single-file-upload-status">{{ fileInfo.errorMessage }}</div>
		</div>
	</div>
</template>

<script>
	import { trimStart } from 'lodash';
	import micromatch from 'micromatch';
	import errorHandling from '@/utils/error-handling.js';
	import { mapState, mapActions } from 'pinia';
	import { useConfigStore } from '@/stores/config.js';
	import { useValidationsStore } from '@/stores/validations.js';

	export default {
		name: 'StadiumUploadFile',

		inject: ['$http'],

		props: {
			controlId: String,
			visible: Boolean,
			allowedExtensions: String,
			classes: null
		},

		emits: ['fileuploaded', 'filedeleted', 'update:files'],

		data() {
			return {
				isDragOver: false,
				errorMessage: null,
				fileInputValue: null,
				uploadedFilesCountKey: 0,
				fileInfos: [],
				uploadingFiles: [],
				persistValidationMessage: false
			};
		},

		computed: {
			...mapState(useValidationsStore, ['controlIdsToValidate']),

			resolvedClasses() {
				let resolvedClasses = this.classes;

				let hasError = this.errorMessage || this.fileInfos.some(f => f.errorMessage);
				if (hasError) resolvedClasses += ' has-validation-error';

				return resolvedClasses;
			},

			allowedExtensionsArray() {
				if (this.allowedExtensions == null || this.allowedExtensions.trim() === '') {
					return [];
				}

				return this.allowedExtensions.split(',').map(s => trimStart(s.trim(), '.'));
			},

			files() {
				return this.fileInfos
					.filter(f => f.isSuccess)
					.map(f => {
						return {
							FileName: f.name,
							FileContents: f.fileToken
						};
					});
			}
		},

		watch: {
			files: {
				handler: function () {
					this.$emit('update:files', this.files);
				},
				immediate: true,
				deep: true
			},

			uploadingFiles: {
				handler: function () {
					if (this.uploadingFiles.length > 0) {
						this.addInvalidControlId(this.controlId);
					} else {
						this.removeControlIdToValidate(this.controlId);
						this.removeInvalidControlId(this.controlId);
					}
				},
				deep: true
			},

			controlIdsToValidate: {
				handler: function () {
					if (this.controlIdsToValidate.includes(this.controlId) && this.uploadingFiles.length === 0) {
						this.errorMessage = null;
					} else if (
						(this.controlIdsToValidate.includes(this.controlId) && this.uploadingFiles.length > 0) ||
						this.persistValidationMessage
					) {
						this.persistValidationMessage = true;
						this.errorMessage = 'Files are being uploaded. Please wait for all files to finish.';
					}
				},
				deep: true
			}
		},

		methods: {
			...mapActions(useValidationsStore, ['addInvalidControlId', 'removeInvalidControlId', 'removeControlIdToValidate']),

			onChange(event) {
				this.uploadFiles(event.target.files);
			},

			onDrop(event) {
				this.isDragOver = false;
				this.uploadFiles(event.dataTransfer.files);
			},

			uploadFiles(inputFileList) {
				if (!inputFileList || !inputFileList.length) return;

				if (inputFileList.length > 20) {
					this.errorMessage = 'Only 20 files are allowed to be uploaded simultaneously.';
					return;
				}

				this.persistValidationMessage = false;
				this.errorMessage = null;
				this.removeControlIdToValidate(this.controlId);

				for (let i = 0; i < inputFileList.length; i++) {
					let fileInfo = {
						key: this.uploadedFilesCountKey,
						fileToken: null,
						name: inputFileList[i].name,
						size: inputFileList[i].size,
						inputFile: inputFileList[i],
						progressPercentage: 0,
						cancelTokenSource: this.$http.CancelToken.source(),
						isSuccess: false,
						errorMessage: null
					};

					this.uploadedFilesCountKey++;

					this.fileInfos.push(fileInfo);
					this.uploadFile(fileInfo.key);
				}

				this.fileInputValue = null;
			},

			async uploadFile(fileKey) {
				let fileInfo = this.fileInfos.find(f => f.key === fileKey);

				if (!fileInfo) return;

				if (fileInfo.size === 0) {
					fileInfo.errorMessage = 'Folders and zero-sized files cannot be uploaded.';
					return;
				}

				if (!this.isFileExtensionAllowed(fileInfo.name)) {
					fileInfo.errorMessage = this.getInvalidFileExtensionErrorMessage(fileInfo.name);
					return;
				}

				const configStore = useConfigStore();
				if (fileInfo.size > configStore.maxFileUploadSizeInKB * 1024) {
					fileInfo.errorMessage = this.getMaxFileUploadSizeErrorMessage(configStore.maxFileUploadSizeInKB);
					return;
				}

				let formData = new FormData();
				formData.append('file', fileInfo.inputFile);

				try {
					this.uploadingFiles.push(fileInfo.key);

					let result = await this.$http.post('api/Controls/UploadFile/Upload', formData, {
						params: { controlId: this.controlId },
						headers: { 'Content-Type': 'multipart/form-data' },
						cancelToken: fileInfo.cancelTokenSource.token,
						onUploadProgress: progressEvent => {
							fileInfo.progressPercentage = Math.round((progressEvent.loaded * 100) / progressEvent.total);
						}
					});

					if (result.isSuccess) {
						fileInfo.isSuccess = true;
						fileInfo.fileToken = result.fileToken;

						this.$emit('fileuploaded', null, fileInfo.fileToken.FileID, {
							FileName: fileInfo.fileToken.FileName,
							FileContents: fileInfo.fileToken
						});
					} else {
						fileInfo.errorMessage = result.status;
					}
				} catch (error) {
					fileInfo.errorMessage = errorHandling.getErrorMessage(error);
				} finally {
					let index = this.uploadingFiles.indexOf(fileInfo.key);
					if (index >= 0) this.uploadingFiles.splice(index, 1);
				}
			},

			getFileContainerClasses(fileInfo) {
				if (fileInfo.errorMessage) return 'file-validation-error';

				if (fileInfo.isSuccess) return 'upload-successful';
			},

			getFileProgressBarClass(fileInfo) {
				if (fileInfo.isSuccess) return 'progress-bar-success';

				if (fileInfo.errorMessage) return 'progress-bar-danger';

				return '';
			},

			isFileExtensionAllowed(fileName) {
				if (this.allowedExtensionsArray.length === 0) return true;

				if (!fileName.includes('.')) return false;

				let fileExtension = fileName.split('.').pop();
				return micromatch.isMatch(fileExtension, this.allowedExtensionsArray);
			},

			getInvalidFileExtensionErrorMessage(fileName) {
				let fileExtension = fileName.split('.').pop();
				let errorMessage = `Unallowed file extension '${fileExtension}'. Allowed extension`;

				if (this.allowedExtensionsArray.length > 1) errorMessage += 's';

				errorMessage += `: ${this.allowedExtensionsArray.map(e => `.${e}`).join(', ')}`;

				return errorMessage;
			},

			getMaxFileUploadSizeErrorMessage(maxFileUploadSizeInKB) {
				let maxFileUploadSizeText =
					maxFileUploadSizeInKB < 1024
						? Number(Math.round(maxFileUploadSizeInKB + 'e+2') + 'e-2') + ' KB'
						: Number(Math.round(maxFileUploadSizeInKB / 1024 + 'e+2') + 'e-2') + ' MB';

				return `The file must be smaller than ${maxFileUploadSizeText}.`;
			},

			cancelUpload(fileInfo) {
				fileInfo.cancelTokenSource.cancel();
				fileInfo.errorMessage = 'File upload cancelled';
			},

			async deleteFile(fileInfo) {
				if (fileInfo.isSuccess) {
					try {
						let result = await this.$http.post('api/Controls/UploadFile/Delete', {}, {
							params: {
								fileToken: JSON.stringify(fileInfo.fileToken)
							}
						});

						if (result.isSuccess) {
							this.$emit('filedeleted', null, fileInfo.fileToken.FileID);
						} else {
							fileInfo.errorMessage = result.status;
							return;
						}
					} catch (error) {
						fileInfo.errorMessage = errorHandling.getErrorMessage(error);
					}
				}

				this.fileInfos.splice(
					this.fileInfos.findIndex(f => f.key === fileInfo.key),
					1
				);
			},

			onFileContainerClick(fileInfo) {
				if (fileInfo.errorMessage) this.deleteFile(fileInfo);
			}
		}
	};
</script>
