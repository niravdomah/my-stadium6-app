<template>
	<div class="stack-layout-container">
		<div class="control-container data-grid-container">
			<div class="title-container">
				<div class="control-container label-container">
					<span class="table-title">Users</span>
				</div>
				<div class="control-container button-container add-item-button">
					<button type="button" class="btn btn-lg btn-default" id="add-user" @click="navigateToAddUser">Add User</button>
				</div>
			</div>

			<table class="table table-striped">
				<thead>
					<tr>
						<th>Email</th>
						<th>Name</th>
						<th class="action-column">Administrator</th>
						<th class="action-column">Edit</th>
						<th class="action-column">Delete</th>
					</tr>
				</thead>
				<tbody>
					<tr v-for="(user, userIndex) in users" :key="userIndex">
						<td class="grid-cell-display-wrap">{{ user.email }}</td>
						<td class="grid-cell-display-wrap">{{ user.name }}</td>
						<td class="grid-cell-display-wrap action-column text-center">
							<input
								:disabled="user.isLastAdministrator || user.isLoggedIn || pendingIsAdministratorIds.includes(user.id)"
								:title="getIsAdministratorTitle(user)"
								:checked="user.isAdministrator"
								:id="'update-user_' + user.id"
								class="select-row-check-box is-administrator-check-box"
								type="checkbox"
								@change="toggleIsAdministrator(user, $event.target.checked)"
							/>
							<label
								:for="'update-user_' + user.id"
								:disabled="user.isLastAdministrator || user.isLoggedIn || pendingIsAdministratorIds.includes(user.id)"
								:title="getIsAdministratorTitle(user)"
							/>
						</td>
						<td class="grid-cell-display-wrap action-column">
							<a :id="'edit-user_' + user.id" @click="navigateToEditUser(user.id)">Edit</a>
						</td>
						<td class="grid-cell-display-wrap action-column">
							<a :id="'delete-user_' + user.id" v-if="!user.isLoggedIn" @click="confirmDeleteUser(user)">Delete</a>
						</td>
					</tr>
				</tbody>
			</table>
		</div>

		<Modal :id="'delete-user-modal'" :clickOutsideToClose="false" v-model:showModal="showDeleteUserModal">
			<div>Delete user {{ usernameToDelete }}?</div>
			<span v-if="deleteUserErrorMessage">{{ deleteUserErrorMessage }}</span>
			<template #footer>
				<button :disabled="isDeletingUser" class="btn btn-default" @click="executeDeleteUser">Yes</button>
				<button class="btn btn-default" @click="cancelDeleteUser">No</button>
			</template>
		</Modal>
	</div>
</template>

<script>
	import { mapState, mapActions } from 'pinia';
	import { useUsersStore } from '@/stores/users.js';
	import Modal from '@/components/Modal.vue';

	export default {
		name: 'Users',
		components: {
			Modal
		},

		data() {
			return {
				showDeleteUserModal: false,
				userToDelete: null,
				isDeletingUser: false,
				pendingIsAdministratorIds: []
			};
		},

		computed: {
			...mapState(useUsersStore, ['users', 'deleteUserErrorMessage']),

			usernameToDelete() {
				return this.userToDelete?.userName;
			}
		},

		async created() {
			const usersStore = useUsersStore();
			if (!usersStore.users) await usersStore.refreshUsersState();
		},

		methods: {
			...mapActions(useUsersStore, ['updateIsAdministrator', 'deleteUser']),

			getIsAdministratorTitle(user) {
				if (user.isLoggedIn) return 'The currently logged-in user cannot be removed as administrator';

				if (user.isLastAdministrator) return 'This user is the only administrator and can therefore not be removed as administrator';

				return null;
			},

			async toggleIsAdministrator(user, isAdministrator) {
				this.pendingIsAdministratorIds.push(user.id);

				try {
					await this.updateIsAdministrator({ id: user.id, isAdministrator: isAdministrator });
				} finally {
					let i = this.pendingIsAdministratorIds.indexOf(user.id);
					if (i >= 0) this.pendingIsAdministratorIds.splice(i, 1);
				}
			},

			navigateToAddUser() {
				this.$router.push({ name: 'AddUser' });
			},

			navigateToEditUser(userId) {
				this.$router.push({
					name: 'EditUser',
					query: {
						userId: userId
					}
				});
			},

			confirmDeleteUser(user) {
				this.userToDelete = user;
				this.showDeleteUserModal = true;
			},

			cancelDeleteUser() {
				this.showDeleteUserModal = false;
			},

			async executeDeleteUser() {
				this.isDeletingUser = true;

				try {
					await this.deleteUser(this.userToDelete.id);
					this.showDeleteUserModal = false;
				} finally {
					this.isDeletingUser = false;
				}
			}
		}
	};
</script>
