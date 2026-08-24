<template>
	<div class="data-grid-header">
		<div v-if="displaySearchBar" class="form-group input-group">
			<input
				:id="`${controlId}-search-box`"
				ref="searchInput"
				:value="searchTerm"
				type="text"
				placeholder="Search"
				class="form-control data-grid-search-box"
				@keydown.enter="setSearchTerm($refs.searchInput.value)"
			/>
			<span
				v-show="searchTerm"
				:id="`${controlId}-search-box-clear-filter-button`"
				class="clear-filter"
				@click="setSearchTerm(null)"
			></span>
			<span class="input-group-btn">
				<button
					:id="`${controlId}-search-box-button`"
					type="button"
					class="btn btn-default data-grid-search-button"
					@click="setSearchTerm($refs.searchInput.value)"
				></button>
			</span>
		</div>
		<div v-if="displaySearchBar" class="data-grid-search-help-button" @click="showSearchHelp"></div>

		<span v-if="displaySearchBar && allowExport" class="data-grid-quick-help-divider"></span>

		<div v-if="allowExport" :id="`${controlId}-export-button`" class="data-grid-export-button" @click="$emit('export-data')"></div>
	</div>
</template>

<script>
	const SEARCH_HELP_URL = 'https://docs.stadium.software/controls/data-grid-search';

	export default {
		name: 'StadiumDataGridHeader',
		props: {
			controlId: String,
			allowExport: Boolean,
			displaySearchBar: Boolean,
			searchTerm: String
		},
		emits: ['update:searchTerm', 'export-data'],

		methods: {
			setSearchTerm(newSearchTerm) {
				this.$emit('update:searchTerm', newSearchTerm);
				this.$refs.searchInput.focus();
			},

			showSearchHelp() {
				window.open(SEARCH_HELP_URL, '_blank');
			},

			focusSearchBox() {
				this.$refs.searchInput.focus();
			},
			setSearchBoxCaretPositionToEnd() {
				let searchTextLength = this.$refs.searchInput.value.length;
				this.$refs.searchInput.setSelectionRange(searchTextLength, searchTextLength);
			}
		}
	};
</script>
