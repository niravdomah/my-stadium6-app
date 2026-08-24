<template>
	<div class="login-form-container">
		<div>
			<h3>Update Password</h3>
		</div>
		<Form @submit="submitForm">
			<div v-if="updatePasswordErrorMessage" class="validation-summary-errors validation-error">
				<ul>
					<li>{{ updatePasswordErrorMessage }}</li>
				</ul>
			</div>

			<div class="stack-layout-container">
				<div class="control-container label-container">
					<span>New Password</span>
				</div>
			</div>

			<div class="stack-layout-container">
				<Field name="New Password" v-model="newPassword" v-slot="{ field, errorMessage }" rules="required">
					<div :class="errorMessage ? 'has-validation-error' : ''" class="control-container text-box-container required-indicator">
						<div class="password-input-container">
							<input id="password"
								   v-bind="field"
								   :type="showPassword ? 'text' : 'password'"
								   placeholder="Password"
								   autocomplete="new-password"
								   class="form-control error-border password-control" />
							<span :class="{ 'hide-password-icon': showPassword }" class="show-password-icon" @click="toggleShowPassword"></span>
						</div>
					</div>
					<span class="validation-error">{{ errorMessage }}</span>
				</Field>
			</div>

			<div class="stack-layout-container">
				<div class="control-container label-container"></div>
			</div>

			<div class="stack-layout-container button-row">
				<div class="control-container button-container">
					<button id="submit-button" :disabled="isSubmittingForm" type="submit" class="btn btn-lg btn-default">Update password</button>
				</div>
			</div>
		</Form>
	</div>
</template>

<script>
	import { Form, Field } from 'vee-validate';
	import { mapState, mapActions } from 'pinia';
	import { usePasswordStore } from '@/stores/password.js';
	import { isNavigationFailure, NavigationFailureType } from 'vue-router';
	import { useApplicationStore } from '@/stores/application.js';

	export default {
		name: 'UpdatePassword',
		components: {
			Form,
			Field
		},

		data() {
			return {
				showPassowrd: false,
				newPassword: null,
				isSubmittingForm: false
			};
		},

		computed: {
			...mapState(usePasswordStore, ['updatePasswordErrorMessage'])
		},

		mounted() {
			const applicationStore = useApplicationStore();
			applicationStore.isAppBusy = false;
		},

		methods: {
			...mapActions(usePasswordStore, ['updatePassword']),

			async submitForm() {
				this.isSubmittingForm = true;

				try {
					await this.updatePassword({
						userId: this.$route.query.userId,
						passwordResetToken: this.$route.query.passwordResetToken,
						newPassword: this.newPassword
					});

					if (this.updatePasswordErrorMessage) return;

					this.$router.push('/').catch(error => {
						if (!isNavigationFailure(error, NavigationFailureType.redirected)) throw error;
					});
				} finally {
					this.isSubmittingForm = false;
				}
			},

			toggleShowPassword() {
				this.showPassword = !this.showPassword;
			}
		}
	};
</script>
