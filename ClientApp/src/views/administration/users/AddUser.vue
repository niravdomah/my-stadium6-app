<template>
	<div class="add-user-container">
		<div class="stack-layout-container bottom-border">
			<div class="control-container label-container">
				<span class="table-title">Add User</span>
			</div>
		</div>

		<UserForm operation="add" v-model:user="user" />
	</div>
</template>

<script>
	import { useRolesStore } from '@/stores/roles.js';
	import UserForm from '@/views/administration/users/UserForm.vue';

	export default {
		name: 'AddUser',
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

			for (let role of rolesStore.roles) {
				if (role.isDefault) this.user.roles.push(role.name);
			}
		}
	};
</script>
