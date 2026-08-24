<template>
	<div class="login-form-container">
		<div>
			<h3>Reset Password</h3>
		</div>
		<Form @submit="submitForm">
			<div v-if="sendPasswordResetEmailErrorMessage" class="validation-summary-errors validation-error">
				<ul>
					<li>{{ sendPasswordResetEmailErrorMessage }}</li>
				</ul>
			</div>

			<div class="stack-layout-container">
				<div class="control-container label-container">
					<span>Email</span>
				</div>
			</div>

			<div class="stack-layout-container">
				<Field name="Email" v-model="passwordResetEmail" v-slot="{ field, errorMessage }" rules="required|email">
					<div :class="errorMessage ? 'has-validation-error' : ''" class="control-container text-box-container required-indicator">
						<input id="email" ref="emailInputField" v-bind="field" placeholder="Email" type="text" class="form-control error-border" />
					</div>
					<span id="email-validation" class="validation-error">{{ errorMessage }}</span>
				</Field>
			</div>

			<div class="stack-layout-container">
				<div class="control-container label-container"></div>
			</div>

			<div class="stack-layout-container button-row">
				<div class="control-container button-container">
					<button id="submit-button" :disabled="isSubmittingForm" type="submit" class="btn btn-lg btn-default">Send password reset email</button>
				</div>
			</div>
		</Form>
	</div>
</template>

<script>
	import { Form, Field } from 'vee-validate';
	import { mapState, mapActions } from 'pinia';
	import { usePasswordStore } from '@/stores/password.js';

	export default {
		name: 'ResetPassword',
		components: {
			Form,
			Field
		},

		computed: {
			email() {
				return this.$route.query.email;
			},
			...mapState(usePasswordStore, ['sendPasswordResetEmailErrorMessage'])
		},

		data() {
			return {
				passwordResetEmail: this.$route.query.email,
				isSubmittingForm: false
			};
		},

		mounted() {
			this.$refs.emailInputField.focus();
		},

		methods: {
			...mapActions(usePasswordStore, ['sendPasswordResetEmail']),

			async submitForm() {
				this.isSubmittingForm = true;

				try {
					await this.sendPasswordResetEmail({
						email: this.passwordResetEmail,
						isResend: false
					});

					if (this.sendPasswordResetEmailErrorMessage) return;

					this.$router.push({
						name: 'ResetPasswordEmailConfirmation',
						query: {
							email: this.passwordResetEmail
						}
					});
				} finally {
					this.isSubmittingForm = false;
				}
			}
		}
	};
</script>
