<template>
	<div v-show="visible" :id="`${controlId}-container`" :class="resolvedClasses" :disabled="resolvedDisabled ? 'disabled' : null">
		<div :id="`${controlId}-items`" class="error-border">
			<div v-for="(option, index) in options" :key="index + JSON.stringify(option)" :class="direction == 'lefttoright' ? 'radio-inline' : 'radio'">
				<input
					:id="`${controlId}_item${index}`"
					:value="getOptionValue(option)"
					:name="controlId"
					:disabled="resolvedDisabled ? 'disabled' : null"
					v-model="boundSelectedValue"
					type="radio"
					@change="onChange"
					@focus="onFocus"
				/>
				<label :for="`${controlId}_item${index}`">{{ getOptionText(option) }}</label>
			</div>
		</div>
		<span v-show="!validationState.isValid || !isValid" class="validation-error">{{ validationMessage }}</span>
	</div>
</template>

<script>
	import { mapState, mapActions } from 'pinia';
	import { useValidationsStore } from '@/stores/validations.js';
	import { getCaseInsensitiveValue } from '@/utils/object-value-case-insensitive.js';

	export default {
		name: 'StadiumRadioButtonList',

		props: {
			controlId: String,
			direction: String,
			options: Array,
			selectedValue: null,
			readonly: Boolean,
			visible: Boolean,
			hasChangeEvent: Boolean,
			classes: null,
			validation: null,
			validationMessage: String,
			isValid: Boolean,
			required: Boolean
		},

		emits: ['change', 'update:options', 'update:selectedOption', 'update:selectedValue'],

		data() {
			return {
				disabled: false,
				previousSelectedOption: { text: '', value: '' }
			};
		},

		watch: {
			options: {
				handler: function (newOptions, oldOptions) {
					if (newOptions instanceof Array) {
						let missingProperties = new Set();

						newOptions.forEach(option => {
							if (this.getOptionText(option) === undefined) missingProperties.add('text');
							if (this.getOptionValue(option) === undefined) missingProperties.add('value');

							if (typeof this.getOptionValue(option) === 'object') {
								this.$emit('update:options', oldOptions);
								throw new Error(this.controlId + ' does not support objects in option values.');
							}
						});

						if (missingProperties.size) {
							const propertyListString = Array.from(missingProperties)
								.map(p => `[${p}]`)
								.join(' and/or ');

							this.$emit('update:options', oldOptions);
							throw new Error(`Some options assigned to ${this.controlId} are missing a ${propertyListString} property.`);
						}
					}
				},
				immediate: true
			},

			selectedOption: {
				handler: function (_newValue, _oldValue) {
					this.$emit('update:selectedOption', this.selectedOption);
				},
				immediate: true
			},

			selectedValue: {
				handler: function (newValue, oldValue) {
					if (newValue != null && !this.options.some(option => this.getOptionValue(option) == newValue)) {
						this.$emit('update:selectedValue', oldValue != null ? oldValue : null);
						throw new Error(`No such value [${newValue}] exists for RadioButtonList. Contact your administrator.`);
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

			boundSelectedValue: {
				get() {
					return this.selectedValue;
				},
				set(value) {
					this.$emit('update:selectedValue', value);
				}
			},

			selectedOption() {
				return this.options?.find(o => this.getOptionValue(o) == this.boundSelectedValue) ?? { text: '', value: '' };
			},

			resolvedDisabled() {
				return this.readonly || this.disabled;
			},

			resolvedClasses() {
				let resolvedClasses = this.classes;

				if (this.required) resolvedClasses += ' required-indicator';

				if (this.resolvedDisabled) resolvedClasses += ' read-only-control';

				if (!this.validationState.isValid || !this.isValid) resolvedClasses += ' has-validation-error';

				return resolvedClasses;
			},

			validationState() {
				let validationState = { isValid: true };
				if (!this.controlIdsToValidate.includes(this.controlId)) return validationState;

				if (this.options == null || this.options.length === 0) return validationState;

				if (this.selectedValue == null || this.selectedValue === '') {
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

			onChange() {
				if (this.hasChangeEvent) {
					this.disabled = true;

					this.$nextTick(function () {
						this.$emit('change', () => (this.disabled = false), this.previousSelectedOption, this.selectedOption);
					});
				}

				this.validateControl(this.controlId);
			},

			onFocus() {
				this.previousSelectedOption = this.selectedOption;
			},

			getOptionText(option) {
				return getCaseInsensitiveValue(option, 'text');
			},

			getOptionValue(option) {
				return getCaseInsensitiveValue(option, 'value');
			}
		}
	};
</script>
