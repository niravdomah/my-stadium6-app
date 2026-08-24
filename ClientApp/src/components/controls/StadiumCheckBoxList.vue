<template>
	<div v-show="visible" :id="`${controlId}-container`" :class="resolvedClasses" :disabled="resolvedDisabled ? 'disabled' : null">
		<div :id="`${controlId}-items`" class="error-border">
			<div v-for="(option, index) in options" :key="index + JSON.stringify(option)" :class="direction == 'lefttoright' ? 'checkbox-inline' : 'checkbox'">
				<input
					:id="`${controlId}_item${index}`"
					:value="getOptionValue(option)"
					:name="controlId"
					:disabled="resolvedDisabled ? 'disabled' : null"
					v-model="boundSelectedValues"
					type="checkbox"
					@change="onChange"
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
		name: 'StadiumCheckBoxList',

		props: {
			controlId: String,
			direction: String,
			options: Array,
			selectedValues: Array,
			readonly: Boolean,
			visible: Boolean,
			hasChangeEvent: Boolean,
			classes: null,
			validation: null,
			validationMessage: String,
			isValid: Boolean,
			required: Boolean
		},

		emits: ['change', 'update:options', 'update:selectedOptions', 'update:selectedValues'],

		data() {
			return {
				disabled: false
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

			selectedOptions: {
				handler: function (_newValue, _oldValue) {
					this.$emit('update:selectedOptions', this.selectedOptions);
				},
				immediate: true
			},

			selectedValues: {
				handler: function (newValues, oldValues) {
					const listOptions = this.options ?? [];
					const invalidValues = newValues?.filter(newValue => {
						return newValue !== '' && !listOptions.some(o => this.getOptionValue(o).toString() == newValue.toString());
					});
					if (invalidValues != null && invalidValues.length > 0) {
						this.$emit('update:selectedValues', oldValues);
						throw new Error(
							`No such value${invalidValues.length > 1 ? 's' : ''} [${invalidValues}] exist${
								invalidValues.length > 1 ? '' : 's'
							} for CheckBoxList. Contact your administrator.`
						);
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

			boundSelectedValues: {
				get() {
					return this.selectedValues;
				},
				set(value) {
					if (value !== undefined) {
						let selectedValues = value?.filter(v => this.options?.map(o => this.getOptionValue(o).toString()).includes(v.toString())) ?? [];
						this.$emit('update:selectedValues', selectedValues);
					}
				}
			},

			selectedOptions() {
				return this.options?.filter(o => this.boundSelectedValues?.includes(this.getOptionValue(o))) ?? [];
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

				if (this.selectedOptions.length === 0) {
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
						this.$emit('change', () => (this.disabled = false));
					});
				}

				this.removeControlIdToValidate(this.controlId);
				this.validateControl(this.controlId);
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
