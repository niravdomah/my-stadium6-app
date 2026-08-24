<template>
	<div class="edit-role-container">
		<div class="stack-layout-container bottom-border">
			<div class="control-container label-container">
				<span class="table-title">Edit Role</span>
			</div>
		</div>

		<RoleForm operation="edit" v-model:role="role" />
	</div>
</template>

<script>
	import { useRolesStore } from '@/stores/roles.js';
	import { usePagesStore } from '@/stores/pages.js';
	import RoleForm from '@/views/administration/roles/RoleForm.vue';

	export default {
		name: 'EditRole',
		components: {
			RoleForm
		},

		data() {
			return {
				role: {
					name: null,
					isDefault: false,
					pages: []
				}
			};
		},

		async created() {
			const pagesStore = usePagesStore();
			if (!pagesStore.pages) await pagesStore.refreshPagesState();

			const rolesStore = useRolesStore();
			if (!rolesStore.roles) await rolesStore.refreshRolesState();

			let roleId = this.$route.query.roleId;
			let roleToEdit = rolesStore.roles.find(r => r.id === roleId);

			if (!roleToEdit) throw new Error('Unable to find role to edit.');

			this.role.name = roleToEdit.name;
			this.role.isDefault = roleToEdit.isDefault;
			for (let page of roleToEdit.pages) {
				this.role.pages.push(page);
			}
		}
	};
</script>
