<template>
	<div class="edit-user-container">
		<div class="stack-layout-container bottom-border">
			<div class="control-container label-container">
				<span class="table-title">Edit User</span>
			</div>
		</div>

		<UserForm operation="edit" v-model:user="user" />
	</div>
</template>

<script>
	import { useUsersStore } from '@/stores/users.js';
	import { useRolesStore } from '@/stores/roles.js';
	import UserForm from '@/views/administration/users/UserForm.vue';

	export default {
		name: 'EditUser',
		components: {
			UserForm
		},

		data() {
			return {
				user: {
					email: null,
					password: null,
					name: null,
					userName: null,
					isAdministrator: false,
					isLastAdministrator: false,
					isLoggedIn: false,
					roles: []
				}
			};
		},

		async created() {
			const rolesStore = useRolesStore();
			if (!rolesStore.roles) await rolesStore.refreshRolesState();

			const usersStore = useUsersStore();
			if (!usersStore.users) await usersStore.refreshUsersState();

			let userId = this.$route.query.userId;
			let userToEdit = usersStore.users.find(u => u.id === userId);

			if (!userToEdit) throw new Error('Unable to find user to edit.');

			this.user.email = userToEdit.email;
			this.user.password = null;
			this.user.name = userToEdit.name;
			this.user.userName = userToEdit.userName;
			this.user.isAdministrator = userToEdit.isAdministrator;
			this.user.isLastAdministrator = userToEdit.isLastAdministrator;
			this.user.isLoggedIn = userToEdit.isLoggedIn;
			for (let role of userToEdit.roles) {
				this.user.roles.push(role);
			}
		}
	};
</script>
