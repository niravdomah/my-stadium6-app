<template>
	<div v-show="visible" :id="`${controlId}-container`" :class="resolvedClasses">
		<date-picker
			:input-attr="{ id: controlId }"
			input-class="mx-input form-control error-border"
			:placeholder="hint"
			:disabled="resolvedDisabled"
			v-model:value="boundDateString"
			v-model:open="isOpen"
			valueType="format"
			:format="dateFormat"
			:title-format="dateFormat"
			:clearable="false"
			popup-class="datepicker datepicker-dropdown"
			@open="setPreviousDate"
			@change="onChange"
			@close="onClose"
		>
			<template #footer="{ emit }">
				<div class="date-picker-popup-footer">
					<button class="today-btn mx-btn mx-btn-text" @click="setToday(emit)">Today</button>
					<button class="clear-btn mx-btn mx-btn-text" @click="clear(emit)">Clear</button>
				</div>
			</template>
			<template #icon-calendar>
				<button class="btn btn-default datepicker-btn" :disabled="resolvedDisabled"></button>
			</template>
		</date-picker>
		<span v-show="!validationState.isValid || !isValid" class="validation-error">{{ validationMessage }}</span>
	</div>
</template>

<script>
	import { mapState, mapActions } from 'pinia';
	import { useValidationsStore } from '@/stores/validations.js';
	import DatePicker from 'vue-datepicker-next';
	import dayjs from 'dayjs';
	import dayjsHelper from '@/utils/dayjs-helper.js';

	export default {
		name: 'StadiumDatePicker',
		components: {
			DatePicker
		},

		props: {
			controlId: String,
			date: null,
			hint: String,
			readonly: Boolean,
			visible: Boolean,
			hasChangeEvent: Boolean,
			classes: null,
			validation: null,
			validationMessage: String,
			isValid: Boolean,
			required: Boolean
		},

		emits: ['change', 'update:date'],

		data() {
			return {
				disabled: false,
				isOpen: false,
				previousDate: null,
				internalDate: this.date
			};
		},

		watch: {
			date: {
				handler: function (newDate, _oldDate) {
					this.internalDate = newDate;
					if (newDate && !dayjs.isDayjs(newDate)) {
						this.$emit('update:date', dayjs(newDate).dateOnly());
					}
				},
				immediate: true
			},

			validationState: {
				handler: function (newValidationState) {
					if (newValidationState.isValid) {
						this.removeControlIdToValidate(this.controlId);
						this.removeInvalidControlId(this.controlId);
					} else {
						this.addInvalidControlId(this.controlId);
					}
				}
			}
		},

		computed: {
			...mapState(useValidationsStore, ['controlIdsToValidate']),

			boundDateString: {
				get() {
					return this.toFormatString(this.internalDate);
				},
				set(value) {
					let newDate = value == null || value == '' ? null : dayjs(value).dateOnly();
					this.internalDate = newDate;
					this.$emit('update:date', newDate);
				}
			},

			dateFormat() {
				return dayjsHelper.getDayjsDateFormat();
			},

			resolvedDisabled() {
				return this.readonly || this.disabled;
			},

			resolvedClasses() {
				let resolvedClasses = this.classes;

				if (this.required) resolvedClasses += ' required-indicator';

				if (!this.validationState.isValid || !this.isValid) resolvedClasses += ' has-validation-error';

				return resolvedClasses;
			},

			validationState() {
				let validationState = { isValid: true };
				if (!this.controlIdsToValidate.includes(this.controlId)) return validationState;

				if (this.internalDate == null) {
					if (!this.required) return validationState;
					else validationState.isValid = false;
				} else if (!this.validation()) {
					validationState.isValid = false;
				}

				return validationState;
			}
		},

		methods: {
			...mapActions(useValidationsStore, ['validateControl', 'addInvalidControlId', 'removeInvalidControlId', 'removeControlIdToValidate']),

			setToday(emit) {
				this.boundDateString = new Date();
				emit(new Date());
				this.isOpen = false;
			},

			clear(emit) {
				this.boundDateString = null;
				emit(null);
				this.isOpen = false;
			},

			setPreviousDate() {
				this.previousDate = this.internalDate;
			},

			toFormatString(date) {
				if (!date) return null;

				if (dayjs.isDayjs(date)) return date.format(this.dateFormat);

				return dayjs(date).format(this.dateFormat);
			},

			onChange() {
				if (this.hasChangeEvent) {
					this.disabled = true;

					this.$nextTick(function () {
						let previousDateString = this.toFormatString(this.previousDate);
						let selectedDateString = this.toFormatString(this.internalDate);

						if (previousDateString == selectedDateString) {
							this.disabled = false;
						} else {
							this.$emit('change', () => (this.disabled = false), previousDateString, selectedDateString);
						}
					});
				}

				this.validateControl(this.controlId);
			},

			onClose() {
				this.$nextTick(() => this.validateControl(this.controlId));
			}
		}
	};
</script>
