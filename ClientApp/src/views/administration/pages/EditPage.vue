<template>
	<div class="edit-page-container">
		<div class="stack-layout-container">
			<div class="control-container label-container">
				<span class="table-title">{{ page.name }} Roles</span>
			</div>
		</div>
		<form @submit.prevent="submitForm">
			<div class="stack-layout-container">
				<SelectionDataGrid id="roles-table" :columnHeaders="['Role Name']" :rows="rolesRows" v-model:selectedRowNames="page.roles" />
			</div>

			<div class="stack-layout-container save-cancel-button-container top-border">
				<div class="control-container button-container">
					<button id="save-button" :disabled="isSubmittingForm" type="submit" class="btn btn-lg btn-default">Save</button>
				</div>
				<div class="control-container button-container">
					<button id="cancel-button" type="button" class="btn btn-lg btn-default" @click="cancel">Cancel</button>
				</div>
			</div>
		</form>
	</div>
</template>

<script>
	import { mapState, mapActions } from 'pinia';
	import { usePagesStore } from '@/stores/pages.js';
	import { useRolesStore } from '@/stores/roles.js';
	import { useAuthenticationStore } from '@/stores/authentication.js';

	import SelectionDataGrid from '@/views/administration/SelectionDataGrid.vue';

	export default {
		name: 'EditPage',
		components: {
			SelectionDataGrid
		},

		data() {
			return {
				page: {
					name: null,
					isStartPage: false,
					roles: []
				},
				isSubmittingForm: false
			};
		},

		computed: {
			...mapState(useRolesStore, ['roles']),

			rolesRows() {
				return (
					this.roles?.map(r => {
						return {
							id: r.id,
							name: r.name,
							isReadOnly: r.isDefault && this.page.isStartPage,
							readOnlyRowTitle: `Access to the Start Page cannot be removed from the ${r.name} role.`
						};
					}) ?? []
				);
			}
		},

		async created() {
			const rolesStore = useRolesStore();
			if (!rolesStore.roles) await rolesStore.refreshRolesState();
			
			const pagesStore = usePagesStore();
			if (!pagesStore.pages) await pagesStore.refreshPagesState();

			let pageId = this.$route.query.pageId;
			let pageToEdit = pagesStore.pages.find(p => p.id === pageId);

			if (!pageToEdit) throw new Error('Unable to find page to edit.');

			this.page.name = pageToEdit.name;
			this.page.isStartPage = pageToEdit.isStartPage;
			for (let role of pageToEdit.roles) {
				this.page.roles.push(role);
			}
		},

		methods: {
			...mapActions(usePagesStore, ['editPage']),
			...mapActions(useRolesStore, ['refreshRolesState']),
			...mapActions(useAuthenticationStore, ['refreshAuthenticationState']),

			async submitForm() {
				this.isSubmittingForm = true;

				try {
					await this.editPage({ id: this.$route.query.pageId, roles: this.page.roles });

					let refreshAuthenticationStatePromise = this.refreshAuthenticationState();
					let refreshRolesStatePromise = this.refreshRolesState();
					await Promise.all([refreshAuthenticationStatePromise, refreshRolesStatePromise]);

					this.navigateToPages();
				} finally {
					this.isSubmittingForm = false;
				}
			},

			cancel() {
				this.navigateToPages();
			},

			navigateToPages() {
				this.$router.push({ name: 'Pages' });
			}
		}
	};
</script>
