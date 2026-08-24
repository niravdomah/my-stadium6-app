<template>
	<div class="add-role-container">
		<div class="stack-layout-container bottom-border">
			<div class="control-container label-container">
				<span class="table-title">Add Role</span>
			</div>
		</div>

		<RoleForm operation="add" v-model:role="role" />
	</div>
</template>

<script>
	import { usePagesStore } from '@/stores/pages.js';
	import RoleForm from '@/views/administration/roles/RoleForm.vue';

	export default {
		name: 'AddRole',
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
		}
	};
</script>
