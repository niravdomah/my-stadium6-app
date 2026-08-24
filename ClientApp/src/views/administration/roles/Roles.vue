<template>
	<div class="stack-layout-container">
		<div class="control-container data-grid-container">
			<div class="title-container">
				<div class="control-container label-container">
					<span class="table-title">Roles</span>
				</div>
				<div class="control-container button-container add-item-button">
					<button type="button" class="btn btn-lg btn-default" id="add-role-button" @click="navigateToAddRole">Add Role</button>
				</div>
			</div>

			<table class="table table-striped">
				<thead>
					<tr>
						<th>Role Name</th>
						<th>Pages</th>
						<th class="action-column">Edit</th>
						<th class="action-column">Delete</th>
					</tr>
				</thead>
				<tbody>
					<tr v-for="(role, roleIndex) in roles" :key="roleIndex">
						<td class="grid-cell-display-wrap">{{ role.name }}</td>
						<td class="grid-cell-display-wrap">{{ role.pages.join(', ') }}</td>
						<td class="grid-cell-display-wrap action-column">
							<a :id="'edit-role-' + role.id" @click="navigateToEditRole(role.id)">Edit</a>
						</td>
						<td class="grid-cell-display-wrap action-column">
							<a v-if="!role.isDefault" :id="'delete-role-' + role.id" @click="confirmDeleteRole(role)">Delete</a>
						</td>
					</tr>
				</tbody>
			</table>
		</div>

		<Modal :id="'delete-role-modal'" :clickOutsideToClose="false" v-model:showModal="showDeleteRoleModal">
			<div>Delete role {{ roleNameToDelete }}?</div>
			<span v-if="deleteRoleErrorMessage">{{ deleteRoleErrorMessage }}</span>
			<template #footer>
				<button :disabled="isDeletingRole" class="btn btn-default" @click="executeDeleteRole">Yes</button>
				<button class="btn btn-default" @click="cancelDeleteRole">No</button>
			</template>
		</Modal>
	</div>
</template>

<script>
	import { mapState, mapActions } from 'pinia';
	import { useRolesStore } from '@/stores/roles.js';
	import { useUsersStore } from '@/stores/users.js';
	import { usePagesStore } from '@/stores/pages.js';
	import Modal from '@/components/Modal.vue';

	export default {
		name: 'Roles',
		components: {
			Modal
		},

		data() {
			return {
				showDeleteRoleModal: false,
				roleToDelete: null,
				isDeletingRole: false
			};
		},

		computed: {
			...mapState(useRolesStore, ['roles', 'deleteRoleErrorMessage']),

			roleNameToDelete() {
				return this.roleToDelete?.name;
			}
		},

		async created() {
			const rolesStore = useRolesStore();
			if (!rolesStore.roles) await rolesStore.refreshRolesState();
		},

		methods: {
			...mapActions(useRolesStore, ['deleteRole']),
			...mapActions(useUsersStore, ['refreshUsersState']),
			...mapActions(usePagesStore, ['refreshPagesState']),

			navigateToAddRole() {
				this.$router.push({ name: 'AddRole' });
			},

			navigateToEditRole(roleId) {
				this.$router.push({
					name: 'EditRole',
					query: {
						roleId: roleId
					}
				});
			},

			confirmDeleteRole(role) {
				this.roleToDelete = role;
				this.showDeleteRoleModal = true;
			},

			cancelDeleteRole() {
				this.showDeleteRoleModal = false;
			},

			async executeDeleteRole() {
				this.isDeletingRole = true;

				try {
					await this.deleteRole(this.roleToDelete.id);

					let refreshUsersStatePromise = this.refreshUsersState();
					let refreshPagesStatePromise = this.refreshPagesState();
					await Promise.all([refreshUsersStatePromise, refreshPagesStatePromise]);

					this.showDeleteRoleModal = false;
				} finally {
					this.isDeletingRole = false;
				}
			}
		}
	};
</script>
