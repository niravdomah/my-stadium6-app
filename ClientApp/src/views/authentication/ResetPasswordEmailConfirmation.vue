<template>
	<div class="login-form-container">
		<h3>Check your inbox</h3>
		<p>We {{ isResend ? 'resent' : 'sent' }} you a password reset link</p>
		<a id="resend-password-reset-link" @click="resendEmail">Resend</a>
		<div v-if="sendPasswordResetEmailErrorMessage" class="validation-summary-errors validation-error">
			<ul>
				<li>{{ sendPasswordResetEmailErrorMessage }}</li>
			</ul>
		</div>
	</div>
</template>

<script>
	import { mapState, mapActions } from 'pinia';
	import { usePasswordStore } from '@/stores/password.js';

	export default {
		name: 'ResetPasswordEmailConfirmation',

		computed: {
			email() {
				return this.$route.query.email;
			},
			...mapState(usePasswordStore, ['sendPasswordResetEmailErrorMessage'])
		},

		data() {
			return {
				isResendingEmail: false,
				isResend: false
			};
		},

		methods: {
			...mapActions(usePasswordStore, ['sendPasswordResetEmail']),

			async resendEmail() {
				this.isResendingEmail = true;
				this.isResend = true;

				try {
					await this.sendPasswordResetEmail({
						email: this.email,
						isResend: this.isResend
					});
				} finally {
					this.isResendingEmail = false;
				}
			}
		}
	};
</script>
