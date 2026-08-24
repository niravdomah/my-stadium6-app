<template>
	<div class="login-form-container">
		<Form ref="form" @submit="submitForm">
			<div v-if="isPreview" class="credentials-container">
				<span>Use the following credentials to login:</span>
				<ul>
					<li>Email: admin@test.com</li>
					<li>Password: admin</li>
				</ul>
			</div>

			<div v-if="loginErrorMessage" class="validation-summary-errors validation-error">
				<ul>
					<li>{{ loginErrorMessage }}</li>
				</ul>
			</div>

			<div class="stack-layout-container">
				<div class="control-container label-container">
					<span>Email</span>
				</div>
			</div>

			<div class="stack-layout-container">
				<Field name="Email" v-model="user.email" v-slot="{ field, errorMessage }" rules="required|email">
					<div :class="errorMessage ? 'has-validation-error' : ''" class="control-container text-box-container">
						<input id="email" ref="emailInputField" v-bind="field" placeholder="Email" type="text" class="form-control error-border" />
						<span id="email-validation" class="validation-error">{{ errorMessage }}</span>
					</div>
				</Field>
			</div>

			<div class="stack-layout-container">
				<div class="control-container label-container">
					<span>Password</span>
				</div>
			</div>

			<div class="stack-layout-container">
				<Field name="Password" v-model="user.password" v-slot="{ field, errorMessage }" rules="required">
					<div :class="errorMessage ? 'has-validation-error' : ''" class="control-container text-box-container">
						<div class="password-input-container">
							<input
								id="password"
								v-bind="field"
								:type="showPassword ? 'text' : 'password'"
								placeholder="Password"
								autocomplete="on"
								class="form-control error-border password-control"
							/>
							<span :class="{ 'hide-password-icon': showPassword }" class="show-password-icon" @click="toggleShowPassword"></span>
						</div>

						<span id="password-validation" class="validation-error">{{ errorMessage }}</span>
					</div>
				</Field>
			</div>

			<div class="stack-layout-container">
				<div class="control-container link-container">
					<a class="btn-link" id="forgot-password-link" @click="navigateToResetPassword">Forgot Password</a>
				</div>
			</div>
			<div class="stack-layout-container">
				<div class="control-container label-container">
					<span id="forgot-password-message">{{ canResetPasswordErrorMessage }}</span>
				</div>
			</div>
			<div class="stack-layout-container button-row">
				<div class="control-container button-container">
					<button id="login-button" :disabled="isLoggingIn" type="submit" class="btn btn-lg btn-default">Login</button>
				</div>
			</div>
		</Form>
	</div>
</template>

<script>
	import { Form, Field } from 'vee-validate';

	import { mapState, mapActions } from 'pinia';
	import { useApplicationStore } from '@/stores/application.js';
	import { useAuthenticationStore } from '@/stores/authentication.js';
	import { usePasswordStore } from '@/stores/password.js';

	export default {
		name: 'Login',
		components: {
			Form,
			Field
		},

		data() {
			return {
				user: {
					email: null,
					password: null
				},
				showPassword: false,
				isLoggingIn: false,
				isNavigatingToResetPassword: false,
				isPreview: import.meta.env.DEV
			};
		},

		computed: {
			pageRoute() {
				return this.$route.query.pageRoute;
			},
			...mapState(useAuthenticationStore, ['isAuthenticated', 'loginErrorMessage']),
			...mapState(usePasswordStore, ['canResetPassword', 'canResetPasswordErrorMessage'])
		},

		mounted() {
			this.$refs.emailInputField.focus();

			const applicationStore = useApplicationStore();
			applicationStore.isAppBusy = false;
		},

		methods: {
			...mapActions(useAuthenticationStore, ['login']),
			...mapActions(usePasswordStore, ['refreshCanResetPassword']),

			async submitForm() {
				this.isLoggingIn = true;

				try {
					await this.login(this.user);

					if (this.isAuthenticated) {
						const applicationStore = useApplicationStore();
						await applicationStore.refreshApplicationState();

						this.resetForm();
						this.$router.push(this.pageRoute ?? '/');
						return;
					}

					this.resetForm();
				} finally {
					this.isLoggingIn = false;
				}
			},

			resetForm() {
				this.user.email = null;
				this.user.password = null;
				this.$refs.form.resetForm();
			},

			async navigateToResetPassword() {
				if (this.isNavigatingToResetPassword) return;
				this.isNavigatingToResetPassword = true;

				try {
					await this.refreshCanResetPassword();

					if (!this.canResetPassword || this.canResetPasswordErrorMessage) return;

					const routeConfig = this.user.email && this.user.email.trim()
						? { name: 'ResetPassword', query: { email: this.user.email } }
						: { name: 'ResetPassword' };

					this.$router.push(routeConfig);
				} finally {
					this.isNavigatingToResetPassword = false;
				}
			},

			toggleShowPassword() {
				this.showPassword = !this.showPassword;
			}
		}
	};
</script>
