<template>
	<Form ref="form" @submit="submitForm">
		<div class="stack-layout-container">
			<div class="control-container label-container">
				<label>Name</label>
			</div>
		</div>

		<div class="stack-layout-container">
			<Field
				name="Name"
				:modelValue="role.name"
				@update:modelValue="newModelValue => $emit('update:role', { name: newModelValue, isDefault: role.isDefault, pages: role.pages })"
				v-slot="{ field, errorMessage }"
				rules="required"
			>
				<div :class="errorMessage ? 'has-validation-error' : ''" class="control-container text-box-container required-indicator">
					<input
						id="role-name"
						ref="nameInputField"
						v-bind="field"
						:disabled="role.isDefault"
						placeholder="Name"
						type="text"
						class="form-control error-border"
					/>
				</div>
				<span id="role-name-validation" class="validation-error">{{ errorMessage }}</span>
			</Field>
		</div>

		<div class="stack-layout-container">
			<SelectionDataGrid
				:heading="'Pages assigned to roles are only accessible to users with those roles'"
				:headingClass="'role-selection-info-label'"
				:columnHeaders="['Page Name', 'Menu Items']"
				:rows="pagesRows"
				:additionalProperties="['menuItems']"
				:selectedRowNames="role.pages"
				@update:selectedRowNames="
					newSelectedRowNames => $emit('update:role', { name: role.name, isDefault: role.isDefault, pages: newSelectedRowNames })
				"
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
	import { mapState, mapActions } from 'pinia';
	import { usePagesStore } from '@/stores/pages.js';
	import { useRolesStore } from '@/stores/roles.js';
	import { useAuthenticationStore } from '@/stores/authentication.js';

	import { Form, Field } from 'vee-validate';
	import SelectionDataGrid from '@/views/administration/SelectionDataGrid.vue';

	export default {
		name: 'RoleForm',
		components: {
			Form,
			Field,
			SelectionDataGrid
		},

		props: {
			role: Object,
			operation: String
		},

		data() {
			return {
				isSubmittingForm: false
			};
		},

		computed: {
			...mapState(usePagesStore, ['pages']),
			...mapState(useRolesStore, ['roleErrors']),

			pagesRows() {
				return (
					this.pages?.map(p => {
						return {
							id: p.id,
							name: p.name,
							isReadOnly: this.role.isDefault && p.isStartPage,
							readOnlyRowTitle: 'The start page cannot be removed from this role.',
							menuItems: p.menuItems.join(', ')
						};
					}) ?? []
				);
			}
		},

		mounted() {
			this.$refs.nameInputField.focus();
		},

		methods: {
			...mapActions(useRolesStore, ['addRole', 'editRole']),
			...mapActions(usePagesStore, ['refreshPagesState']),
			...mapActions(useAuthenticationStore, ['refreshAuthenticationState']),

			async submitForm() {
				this.isSubmittingForm = true;

				try {
					if (this.operation === 'add') {
						await this.addRole(this.role);
					} else if (this.operation === 'edit') {
						await this.editRole({ id: this.$route.query.roleId, ...this.role });
					}

					if (this.roleErrors) {
						this.$refs.form.setErrors(this.roleErrors);
						return;
					}

					let refreshAuthenticationStatePromise = this.refreshAuthenticationState();
					let refreshPagesStatePromise = this.refreshPagesState();
					await Promise.all([refreshAuthenticationStatePromise, refreshPagesStatePromise]);

					this.navigateToRoles();
				} finally {
					this.isSubmittingForm = false;
				}
			},

			cancel() {
				this.navigateToRoles();
			},

			navigateToRoles() {
				this.$router.push({ name: 'Roles' });
			}
		}
	};
</script>
