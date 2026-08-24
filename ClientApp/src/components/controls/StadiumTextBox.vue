<template>
	<div v-show="visible" :id="`${controlId}-container`" :class="resolvedClasses">
		<div v-if="isPassword" class="password-input-container">
			<input
				:id="controlId"
				:value="text"
				:placeholder="hint"
				:readonly="readonly"
				:type="showPassword ? 'text' : 'password'"
				class="form-control error-border password-control text-box-input"
				@input="onInput($event.target.value)"
				@blur="onBlur()"
			/>
			<span :class="{ 'hide-password-icon': showPassword }" class="show-password-icon" @click="toggleShowPassword"></span>
		</div>
		<input
			v-else-if="parsedVisibleLines == 1"
			:id="controlId"
			:value="text"
			:placeholder="hint"
			:readonly="readonly"
			type="text"
			class="form-control error-border text-box-input"
			@input="onInput($event.target.value)"
			@blur="onBlur()"
		/>
		<textarea
			v-else
			:id="controlId"
			:value="text"
			:rows="parsedVisibleLines"
			:placeholder="hint"
			:readonly="readonly"
			class="form-control error-border text-box-input"
			@input="onInput($event.target.value)"
			@blur="onBlur()"
		></textarea>
		<span v-show="!validationState.isValid || !isValid" class="validation-error">{{ validationMessage }}</span>
	</div>
</template>

<script>
	import { mapState, mapActions } from 'pinia';
	import { useValidationsStore } from '@/stores/validations.js';

	export default {
		name: 'StadiumTextBox',

		props: {
			controlId: String,
			hint: String,
			isPassword: Boolean,
			readonly: Boolean,
			text: String,
			visible: Boolean,
			visibleLines: [String, Number],
			validators: Array,
			classes: null,
			validation: null,
			validationMessage: String,
			isValid: Boolean,
			required: Boolean
		},

		computed: {
			...mapState(useValidationsStore, ['controlIdsToValidate']),

			parsedVisibleLines() {
				return typeof this.visibleLines === 'string' ? parseInt(this.visibleLines) : this.visibleLines;
			},

			resolvedClasses() {
				let resolvedClasses = this.classes;

				if (this.required) resolvedClasses += ' required-indicator';

				if (this.readonly) resolvedClasses += ' read-only-control';

				if (!this.validationState.isValid || !this.isValid) resolvedClasses += ' has-validation-error';

				return resolvedClasses;
			},

			validationState() {
				let validationState = { isValid: true };
				if (!this.controlIdsToValidate.includes(this.controlId)) return validationState;

				if (this.text == null || this.text === '') {
					if (!this.required) return validationState;
					else validationState.isValid = false;
				} else if (!this.validation()) {
					validationState.isValid = false;
				}

				return validationState;
			}
		},

		watch: {
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

		data() {
			return {
				showPassword: false
			};
		},

		methods: {
			...mapActions(useValidationsStore, ['validateControl', 'addInvalidControlId', 'removeInvalidControlId', 'removeControlIdToValidate']),

			toggleShowPassword() {
				this.showPassword = !this.showPassword;
			},

			onInput(newText) {
				this.$emit('update:text', newText);

				if (!this.validationState.isValid) {
					this.validateControl(this.controlId);
				}
			},

			onBlur() {
				this.validateControl(this.controlId);
			}
		}
	};
</script>
