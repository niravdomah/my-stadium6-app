<template>
	<div v-show="visible" :id="`${controlId}-container`" :class="resolvedClasses">
			<select :id="controlId"
				:disabled="resolvedDisabled"
				v-model="boundSelectedValue"
				:class="hint && selectedValue == null ? 'select-option-hint' : ''"
				class="form-control error-border"
				@change="onChange"
				@focus="onFocus">
				<option v-if="hint" class="option-hint" :value="null">{{ hint }}</option>
				<option v-for="(option, index) in options" :key="index + JSON.stringify(option)" :value="getOptionValue(option)">
					{{ getOptionText(option) }}
				</option>
			</select>
		<span v-show="!validationState.isValid || !isValid" class="validation-error">{{ validationMessage }}</span>
	</div>
</template>

<script>
	import { mapState, mapActions } from 'pinia';
	import { useValidationsStore } from '@/stores/validations.js';
	import { getCaseInsensitiveValue } from '@/utils/object-value-case-insensitive.js';

	export default {
		name: 'StadiumDropDown',

		props: {
			controlId: String,
			options: Array,
			selectedValue: null,
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

		emits: ['change', 'update:options', 'update:selectedOption', 'update:selectedValue'],

		data() {
			return {
				disabled: false,
				previousSelectedOption: { text: '', value: '' }
			};
		},

		watch: {
			selectedOption: {
				handler: function (_newSelectedOption, _oldSelectedOption) {
					this.$emit('update:selectedOption', this.selectedOption);
				},
				immediate: true
			},

			selectedValue: {
				handler: function (newValue, oldValue) {
					if (newValue != null && !this.options.some(option => this.getOptionValue(option) == newValue)) {
						this.$emit('update:selectedValue', oldValue ?? null);
						throw new Error(`No such value [${newValue}] exists for DropDown. Contact your administrator.`);
					}
				},
				immediate: true
			},

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

					if (this.selectedValue != null && newOptions != null) {
						let selectedIndex = newOptions.findIndex(o => this.getOptionValue(o) == this.selectedValue);
						let newSelectedValue = selectedIndex < 0 ? null : this.getOptionValue(newOptions[selectedIndex]);
						this.$emit('update:selectedValue', newSelectedValue);
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
					if (this.selectedValue == null) {
						return this.hint ? null : this.options?.[0] ? this.getOptionValue(this.options[0]) : undefined;
					}

					return this.selectedValue;
				},
				set(value) {
					this.$emit('update:selectedValue', value);
				}
			},

			selectedOption() {
				let currentSelectedOption = this.options?.find(o => this.getOptionValue(o) == this.boundSelectedValue);
				if (this.boundSelectedValue === '' || currentSelectedOption == null) return { text: '', value: '' };

				return currentSelectedOption;
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

				if (this.options == null || this.options.length === 0) return validationState;

				if (this.selectedOption?.value == null || this.selectedOption.value === '') {
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
