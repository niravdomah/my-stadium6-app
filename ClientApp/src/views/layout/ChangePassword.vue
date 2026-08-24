<template>
	<Form ref="form" :id="id" @submit="submitForm" @reset="cancel">
		<div id="change-password-validation" v-if="changePasswordErrorMessage" class="validation-summary-errors validation-error">
			<ul>
				<li>{{ changePasswordErrorMessage }}</li>
			</ul>
		</div>

		<div class="stack-layout-container">
			<div class="control-container label-container">
				<label>Old Password</label>
			</div>
		</div>

		<div class="stack-layout-container">
			<Field name="Old Password" v-model="oldPassword" v-slot="{ field, errorMessage }" rules="required">
				<div :class="errorMessage ? 'has-validation-error' : ''" class="control-container text-box-container required-indicator">
					<div class="password-input-container">
						<input id="old-password"
							   ref="oldPasswordInput"
							   v-bind="field"
							   :type="showOldPassword ? 'text' : 'password'"
							   placeholder="Password"
							   autocomplete="new-password"
							   class="form-control error-border password-control" />
						<span :class="{ 'hide-password-icon': showOldPassword }" class="show-password-icon" @click="toggleShowOldPassword"></span>
					</div>
				</div>
				<span id="old-password-validation" class="validation-error">{{ errorMessage }}</span>
			</Field>
		</div>

		<div class="stack-layout-container">
			<div class="control-container label-container">
				<label>New Password</label>
			</div>
		</div>

		<div class="stack-layout-container">
			<Field name="New Password" v-model="newPassword" v-slot="{ field, errorMessage }" rules="required">
				<div :class="errorMessage ? 'has-validation-error' : ''" class="control-container text-box-container required-indicator">
					<div class="password-input-container">
						<input id="new-password"
							   v-bind="field"
							   :type="showNewPassword ? 'text' : 'password'"
							   placeholder="Password"
							   autocomplete="new-password"
							   class="form-control error-border password-control" />
						<span :class="{ 'hide-password-icon': showNewPassword }" class="show-password-icon" @click="toggleShowNewPassword"></span>
					</div>
				</div>
				<span id="new-password-validation" class="validation-error">{{ errorMessage }}</span>
			</Field>
		</div>
	</Form>
</template>

<script>
	import { Form, Field } from 'vee-validate';
	import { mapWritableState, mapActions } from 'pinia';
	import { usePasswordStore } from '@/stores/password.js';

	export default {
		name: 'ChangePassword',
		inject: ['$notification'],
		components: {
			Form,
			Field
		},

		props: {
			id: String,
			showChangePasswordModal: Boolean,
			isSubmittingForm: Boolean
		},

		data() {
			return {
				showOldPassword: false,
				showNewPassword: false,
				oldPassword: null,
				newPassword: null
			};
		},

		computed: {
			...mapWritableState(usePasswordStore, ['changePasswordErrorMessage'])
		},

		mounted() {
			this.$refs.oldPasswordInput.focus();
		},

		methods: {
			...mapActions(usePasswordStore, ['changePassword']),

			async submitForm() {
				this.$emit('update:isSubmittingForm', true);

				try {
					await this.changePassword({
						oldPassword: this.oldPassword,
						newPassword: this.newPassword
					});

					this.resetForm();

					if (this.changePasswordErrorMessage) {
						this.$refs.oldPasswordInput.focus();
						return;
					}

					this.$notification.showSuccess('Password was changed successfully');

					this.$emit('update:showChangePasswordModal', false);
				} finally {
					this.$emit('update:isSubmittingForm', false);
				}
			},

			cancel() {
				this.changePasswordErrorMessage = null;
				this.resetForm();

				this.$emit('update:showChangePasswordModal', false);
			},

			resetForm() {
				this.oldPassword = null;
				this.newPassword = null;
				this.$refs.form.resetForm();
			},

			toggleShowOldPassword() {
				this.showOldPassword = !this.showOldPassword;
			},

			toggleShowNewPassword() {
				this.showNewPassword = !this.showNewPassword;
			}
		}
	};
</script>
