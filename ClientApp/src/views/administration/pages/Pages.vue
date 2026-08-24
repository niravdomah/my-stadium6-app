<template>
	<div class="stack-layout-container">
		<div class="control-container data-grid-container">
			<div class="title-container page-title-container">
				<div class="control-container label-container">
					<span class="table-title">Pages</span>
				</div>
			</div>

			<table class="table table-striped">
				<thead>
					<tr>
						<th>Page Name</th>
						<th>Menu Items</th>
						<th>Roles</th>
						<th class="action-column">Edit</th>
					</tr>
				</thead>
				<tbody>
					<tr v-for="(page, pageIndex) in pages" :key="pageIndex">
						<td class="grid-cell-display-wrap">{{ page.name }}</td>
						<td class="grid-cell-display-wrap">{{ page.menuItems.join(', ') }}</td>
						<td class="grid-cell-display-wrap">{{ page.roles.join(', ') }}</td>
						<td class="grid-cell-display-wrap action-column">
							<a :id="'edit-page-access-' + page.id" @click="navigateToEditPage(page.id)">Edit</a>
						</td>
					</tr>
				</tbody>
			</table>
		</div>
	</div>
</template>

<script>
	import { mapState } from 'pinia';
	import { usePagesStore } from '@/stores/pages.js';

	export default {
		name: 'Pages',

		computed: {
			...mapState(usePagesStore, ['pages'])
		},

		async created() {
			const pagesStore = usePagesStore();
			if (!pagesStore.pages) await pagesStore.refreshPagesState();
		},

		methods: {
			navigateToEditPage(pageId) {
				this.$router.push({
					name: 'EditPage',
					query: {
						pageId: pageId
					}
				});
			}
		}
	};
</script>
