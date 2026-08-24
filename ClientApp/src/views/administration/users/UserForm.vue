<template>
	<Form ref="form" @submit="submitForm">
		<div class="stack-layout-container">
			<div class="control-container label-container">
				<label>Email</label>
			</div>
		</div>

		<div class="stack-layout-container">
			<Field
				name="Email"
				:modelValue="user.email"
				@update:modelValue="newModelValue => updateUserEmail(newModelValue)"
				v-slot="{ field, errorMessage }"
				rules="required|email"
			>
				<div :class="errorMessage ? 'has-validation-error' : ''" class="control-container text-box-container required-indicator">
					<input
						id="email"
						ref="emailInputField"
						v-bind="field"
						autocomplete="off"
						placeholder="Email"
						type="text"
						class="form-control error-border"
					/>
					<span v-if="errorMessage" id="email-validation" class="validation-error">{{ errorMessage }}</span>
				</div>
			</Field>
		</div>

		<div v-if="isWindowsAuthentication" class="stack-layout-container">
			<div class="control-container label-container">
				<label>UserName</label>
			</div>
		</div>

		<div v-if="isWindowsAuthentication" class="stack-layout-container">
			<Field
				name="UserName"
				:modelValue="user.userName"
				@update:modelValue="newModelValue => updateUserUserName(newModelValue)"
				v-slot="{ field, errorMessage }"
				rules="required"
			>
				<div :class="errorMessage ? 'has-validation-error' : ''" class="control-container text-box-container required-indicator">
					<input
						id="userName"
						v-bind="field"
						autocomplete="off"
						placeholder="Domain\UserName"
						type="text"
						class="form-control error-border"
					/>
					<span v-if="errorMessage" id="userName-validation" class="validation-error">{{ errorMessage }}</span>
				</div>
			</Field>
		</div>

		<div v-if="!isOAuthAuthentication && !isWindowsAuthentication" class="stack-layout-container">
			<div class="control-container label-container">
				<label>Password</label>
			</div>
		</div>

		<div v-if="!isOAuthAuthentication && !isWindowsAuthentication" class="stack-layout-container">
			<div v-show="showPasswordField">
				<Field
					name="Password"
					:modelValue="user.password"
					@update:modelValue="newModelValue => updateUserPassword(newModelValue)"
					v-slot="{ field, errorMessage }"
					:rules="this.operation === 'edit' && !this.showPasswordField ? '' : 'required'"
				>
					<div :class="errorMessage ? 'has-validation-error' : ''" class="control-container text-box-container required-indicator">
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
					</div>
					<span id="password-validation" class="validation-error">{{ errorMessage }}</span>
				</Field>
			</div>

			<div v-show="!showPasswordField" class="control-container">
				<a id="show-password-field-link" @click.prevent="changePassword = true">Change</a>
			</div>
		</div>

		<div class="stack-layout-container">
			<div class="control-container label-container">
				<label>Name</label>
			</div>
		</div>

		<div class="stack-layout-container">
			<div class="control-container text-box-container">
				<input
					:value="user.name"
					@input="updateUserName($event.target.value)"
					autocomplete="off"
					placeholder="Name"
					type="text"
					class="form-control"
				/>
			</div>
		</div>

		<div class="stack-layout-container">
			<div class="control-container check-box-container is-administrator-container" :title="isAdministratorTitle">
				<div :class="{ disabled: isAdministratorDisabled }" class="checkbox">
					<input
						id="is-administrator"
						:checked="user.isAdministrator"
						@change="updateUserIsAdministrator($event.target.checked)"
						:disabled="isAdministratorDisabled"
						type="checkbox"
					/>
					<label
						id="is-administrator-label"
						for="is-administrator"
						:disabled="isAdministratorDisabled"
						:title="isAdministratorDisabled ? 'The currently logged-in user cannot be removed as administrator' : ''"
						>Administrator</label
					>
				</div>
			</div>
		</div>

		<div class="stack-layout-container top-border">
			<SelectionDataGrid
				:heading="'Assign roles to this user'"
				:columnHeaders="['Role Name']"
				:rows="rolesRows"
				:selectedRowNames="user.roles"
				@update:selectedRowNames="newSelectedRowNames => updateUserRoles(newSelectedRowNames)"
			/>
		</div>

		<div class="stack-layout-container save-cancel-button-container top-border">
			<div class="control-container button-container">
				<button id="save-button" :disabled="isSubmittingForm" type="submit" class="btn btn-lg btn-default">Save</button>
			</div>
			<div class="control-container button-container">
				<button id="cancel-button" type="button" class="btn btn-lg btn-default" @click="cancel">Cancel</button>
			</div>
		</div>
	</Form>
</template>

<script>
	import { cloneDeep } from 'lodash';

	import { mapState, mapActions } from 'pinia';
	import { useUsersStore } from '@/stores/users.js';
	import { useRolesStore } from '@/stores/roles.js';
	import { useAuthenticationStore } from '@/stores/authentication.js';

	import { Form, Field } from 'vee-validate';

	import SelectionDataGrid from '@/views/administration/SelectionDataGrid.vue';

	export default {
		name: 'UserForm',
		components: {
			Form,
			Field,
			SelectionDataGrid
		},

		props: {
			user: Object,
			operation: String
		},

		data() {
			return {
				isSubmittingForm: false,
				showPassword: false,
				changePassword: false
			};
		},

		computed: {
			...mapState(useRolesStore, ['roles']),
			...mapState(useUsersStore, ['userErrors']),
			...mapState(useAuthenticationStore, ['isOAuthAuthentication', 'isWindowsAuthentication']),

			showPasswordField() {
				return this.operation === 'add' || this.changePassword;
			},

			isAdministratorDisabled() {
				return this.user.isLastAdministrator || this.user.isLoggedIn;
			},

			isAdministratorTitle() {
				if (this.user.isLoggedIn) return 'The currently logged-in user cannot be removed as administrator.';

				if (this.user.isLastAdministrator) return 'This user is the only administrator and can therefore not be removed as administrator.';

				return null;
			},

			rolesRows() {
				return (
					this.roles?.map(r => {
						return {
							id: r.id,
							name: r.name,
							isReadOnly: r.isDefault,
							readOnlyRowTitle: `The ${r.name} role cannot be removed from a user.`
						};
					}) ?? []
				);
			}
		},

		mounted() {
			this.$refs.emailInputField.focus();
		},

		methods: {
			...mapActions(useUsersStore, ['addUser', 'editUser']),
			...mapActions(useAuthenticationStore, ['refreshAuthenticationState']),

			async submitForm() {
				this.isSubmittingForm = true;

				try {
					if (this.operation === 'add') {
						await this.addUser(this.user);
					} else if (this.operation === 'edit') {
						await this.editUser({
							id: this.$route.query.userId,
							...this.user,
							password: this.showPasswordField ? this.user.password : null
						});
					}

					if (this.userErrors) {
						this.$refs.form.setErrors(this.userErrors);
						return;
					}

					if (this.user.isLoggedIn) {
						this.refreshAuthenticationState();
					}

					this.navigateToUsers();
				} finally {
					this.isSubmittingForm = false;
				}
			},

			cancel() {
				this.navigateToUsers();
			},

			navigateToUsers() {
				this.$router.push({ name: 'Users' });
			},

			toggleShowPassword() {
				this.showPassword = !this.showPassword;
			},

			updateUserName(newName) {
				let userCopy = cloneDeep(this.user);
				userCopy.name = newName;
				this.$emit('update:user', userCopy);
			},

			updateUserUserName(newUserName) {
				let userCopy = cloneDeep(this.user);
				userCopy.userName = newUserName;
				this.$emit('update:user', userCopy);
			},

			updateUserIsAdministrator(newIsAdministrator) {
				let userCopy = cloneDeep(this.user);
				userCopy.isAdministrator = newIsAdministrator;
				this.$emit('update:user', userCopy);
			},

			updateUserRoles(newRoles) {
				let userCopy = cloneDeep(this.user);
				userCopy.roles = newRoles;
				this.$emit('update:user', userCopy);
			},

			updateUserEmail(newEmail) {
				let userCopy = cloneDeep(this.user);
				userCopy.email = newEmail;
				this.$emit('update:user', userCopy);
			},

			updateUserPassword(newPassword) {
				let userCopy = cloneDeep(this.user);
				userCopy.password = newPassword;
				this.$emit('update:user', userCopy);
			}
		}
	};
</script>
