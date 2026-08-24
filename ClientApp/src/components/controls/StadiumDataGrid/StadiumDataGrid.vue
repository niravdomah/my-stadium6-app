<template>
	<div v-show="visible" :id="`${controlId}-container`" :class="classes">
		<StadiumDataGridHeader
			:controlId="controlId"
			:allowExport="allowExport"
			:displaySearchBar="displaySearchBar"
			:searchTerm="searchTerm"
			@update:searchTerm="setSearchTerm($event)"
			@export-data="exportData"
		/>

		<div v-if="errorMessage" :id="`${controlId}-error-box`" class="alert alert-danger">{{ errorMessage }}</div>

		<div :id="`${controlId}-selection-status-bar`" :class="selectedData.length > 0 ? 'show-status-bar' : ''" class="selection-status-bar">
			<span :id="`${controlId}-selection-status-bar-text`">
				{{ `${selectedData.length} row${selectedData.length === 1 ? '' : 's'} selected` }}
			</span>
			<span>(</span>
			<a :id="`${controlId}-selection-status-bar-clear-selection`" @click="clearSelection">clear</a>
			<span>). View: </span>
			<a :id="`${controlId}-selection-status-bar-view-all`" @click="setSearchTerm(null)">all</a>
			<span class="separator"></span>
			<a :id="`${controlId}-selection-status-bar-view-selected`" @click="setSearchTerm('selected:yes')">selected</a>
		</div>

		<table :id="controlId" data-sortable class="table table-striped">
			<thead>
				<tr>
					<th v-if="hasSelectableData && columnDefinitions.some(c => c.visible)">
						<input
							:id="`${controlId}-selected-rows-header`"
							:checked="areAllPageRowsSelected"
							:disabled="pageData == null || pageData.length == 0"
							type="checkbox"
							@click="toggleSelectAllPageRows"
						/>
						<label :for="`${controlId}-selected-rows-header`" />
					</th>
					<th
						v-for="columnDefinition in columnDefinitions"
						v-show="columnDefinition.visible"
						:key="columnDefinition.name"
						:sort="getHeaderSortDirection(columnDefinition.name)"
						:class="[lastVisibleColumnName == columnDefinition.name ? 'last-visible-column' : '']"
					>
						<a :id="`${controlId}_${columnDefinition.name}-header`" @click="setSortColumn(columnDefinition.name)">
							{{ columnDefinition.headerText }}
						</a>
					</th>
				</tr>
			</thead>

			<tbody v-if="emptyDataGridMessage">
				<tr>
					<td :colspan="columnDefinitions.length">
						{{ emptyDataGridMessage }}
					</td>
				</tr>
			</tbody>

			<tbody v-else>
				<tr v-for="(row, rowIndex) in pageData" :id="`${controlId}-row${rowIndex}`" :key="rowIndex">
					<td v-if="hasSelectableData" class="grid-cell-display-default">
						<input
							:id="`${controlId}-selected-row${rowIndex}`"
							:value="getSelectedValue(row)"
							@change="toggleSelected(row)"
							v-model="row[selectionColumnName]"
							type="checkbox"
							class="select-row-check-box"
						/>
						<label :for="`${controlId}-selected-row${rowIndex}`" />
					</td>
					<td
						v-for="columnDefinition in columnDefinitions"
						v-show="columnDefinition.visible"
						:key="columnDefinition.name"
						:class="[
							`grid-cell-display-${columnDefinition.cellDisplay}`,
							columnDefinition.classes,
							lastVisibleColumnName == columnDefinition.name ? 'last-visible-column' : ''
						]"
						:style="{ textAlign: columnDefinition.cellAlignment }"
					>
						<StadiumDataGridCell
							:controlId="controlId"
							:columnDefinition="columnDefinition"
							:row="row"
							:rowIndex="rowIndex"
							@cell-click="doneCallback => $emit(`${columnDefinition.name}click`, doneCallback, row)"
						/>
					</td>
				</tr>
			</tbody>
			<tfoot>
				<tr v-if="showPaging">
					<td :colspan="columnDefinitions.length">
						<StadiumDataGridPaging
							:controlId="controlId"
							:maxNumberOfPagesDisplayed="maxNumberOfPagesDisplayed"
							:numberOfRowsPerPage="numberOfRowsPerPage"
							:totalNumberOfRows="totalNumberOfRows"
							v-model:currentPageNumber="currentPageNumber"
						/>
					</td>
				</tr>
			</tfoot>
		</table>
	</div>
</template>

<script>
	import { cloneDeep } from 'lodash';
	import { useApplicationStore } from '@/stores/application.js';
	import dayjs from 'dayjs';

	import StadiumDataGridHeader from '@/components/controls/StadiumDataGrid/StadiumDataGridHeader.vue';
	import StadiumDataGridCell from '@/components/controls/StadiumDataGrid/StadiumDataGridCell.vue';
	import StadiumDataGridPaging from '@/components/controls/StadiumDataGrid/StadiumDataGridPaging.vue';

	const SELECTION_COLUMN_NAME = 'selected';
	const SELECTION_COLUMN_ID = 'selection-id';
	const ASCENDING = 'asc';
	const DESCENDING = 'desc';
	const PAGE_SIZE = 20;
	const PAGING_MAX = 10;
	const ROWS_PER_CHUNK = 25000;
	const SELECTED_SEARCH_TERM = 'selected:yes';
	const NOT_SELECTED_SEARCH_TERM = 'selected:no';

	export default {
		name: 'StadiumDataGrid',
		inject: ['$http'],
		props: {
			controlId: String,
			allowExport: Boolean,
			displaySearchBar: Boolean,
			columnDefinitions: Array,
			data: Array,
			hasSelectableData: Boolean,
			searchTerm: String,
			visible: Boolean,
			classes: null,
			selectedPage: null
		},
		emits: ['update:selectedData', 'update:selectedPage', 'update:searchTerm'],

		components: {
			StadiumDataGridHeader,
			StadiumDataGridCell,
			StadiumDataGridPaging
		},

		data() {
			return {
				localData: null,
				filteredData: [],
				pageData: [],

				selectionColumnName: SELECTION_COLUMN_NAME,
				selectionColumnId: SELECTION_COLUMN_ID,
				sortColumnName: null,
				sortDirectionByColumnMap: {},

				numberOfRowsPerPage: PAGE_SIZE,
				maxNumberOfPagesDisplayed: PAGING_MAX,
				currentPageNumber: 1,

				isExporting: false,
				isUpdatingData: false,
				errorMessage: null
			};
		},

		computed: {
			emptyDataGridMessage() {
				if (this.columnDefinitions == null || this.columnDefinitions.length == 0) return 'No columns configured.';

				if (this.columnDefinitions.every(c => !c.visible)) return 'No visible columns.';

				if (this.filteredData == null || this.filteredData.length === 0) return 'No data to display.';

				return null;
			},

			columnNameHeaderMap() {
				let map = Object.assign({}, ...this.columnDefinitions.filter(c => c.visible).map(c => ({ [c.name]: c.headerText ?? '' })));

				if (this.hasSelectableData) map[SELECTION_COLUMN_NAME] = SELECTION_COLUMN_NAME;

				return map;
			},

			selectedData() {
				return this.localData == null ? [] : this.localData.filter(r => r[SELECTION_COLUMN_NAME]);
			},

			areAllPageRowsSelected() {
				return this.pageData.length > 0 && this.pageData.every(r => r[SELECTION_COLUMN_NAME]);
			},

			totalNumberOfRows() {
				return this.filteredData.length;
			},

			showPaging() {
				return this.totalNumberOfRows > this.numberOfRowsPerPage && this.columnDefinitions.some(c => c.visible);
			},

			lastVisibleColumnName() {
				return (
					this.columnDefinitions
						.slice()
						.reverse()
						.find(columnDefinition => columnDefinition.visible)?.name ?? ''
				);
			}
		},

		watch: {
			data: {
				immediate: true,
				handler: async function (newData) {
					try {
						this.isUpdatingData = true;

						this.localData = cloneDeep(newData);

						this.setSelectionColumn(this.hasSelectableData);
						await this.filterData(this.searchTerm === SELECTED_SEARCH_TERM ? null : this.searchTerm);
						this.sortData(this.sortColumnName, this.sortDirectionByColumnMap[this.sortColumnName]);
						this.navigateToPage(1);
						if (this.searchTerm === SELECTED_SEARCH_TERM) this.setSearchTerm(null);
					} finally {
						this.isUpdatingData = false;
					}
				}
			},

			async hasSelectableData(newHasSelectionColumn) {
				this.setSelectionColumn(newHasSelectionColumn);

				await this.filterData(this.searchTerm);
				this.sortData(this.sortColumnName, this.sortDirectionByColumnMap[this.sortColumnName]);
				this.navigateToPage(this.currentPageNumber);
			},

			async searchTerm(newSearchTerm) {
				await this.filterData(newSearchTerm);

				this.sortData(this.sortColumnName, this.sortDirectionByColumnMap[this.sortColumnName]);
				this.navigateToPage(1);
			},

			sortDirectionByColumnMap: {
				deep: true,
				handler: function (newSortDirectionByColumnMap) {
					this.sortData(this.sortColumnName, newSortDirectionByColumnMap[this.sortColumnName]);

					this.navigateToPage(this.currentPageNumber);
				}
			},

			currentPageNumber(newCurrentPageNumber) {
				this.$emit('update:selectedPage', newCurrentPageNumber);
			},

			selectedPage(newSelectedPageInfo) {
				this.setSelectedPage(newSelectedPageInfo);
			},

			async selectedData() {
				this.$emit(
					'update:selectedData',
					this.selectedData.map(({ selected, ...rowData }) => rowData)
				);
				if (this.selectedData.length == 0) {
					if (this.searchTerm === SELECTED_SEARCH_TERM) this.setSearchTerm(null);
				} else if (this.searchTerm?.includes(SELECTED_SEARCH_TERM) || this.searchTerm?.includes(NOT_SELECTED_SEARCH_TERM)) {
					await this.filterData(this.searchTerm);
					this.navigateToPage(this.currentPageNumber);
				}
			}
		},

		methods: {
			setSelectedPage(newSelectedPageInfo) {
				if (this.isUpdatingData) {
					const unwatch = this.$watch('isUpdatingData', () => {
						unwatch();
						this.setSelectedPage(newSelectedPageInfo);
					});

					return;
				}

				let pageNumber = parseInt(newSelectedPageInfo.Page);

				if (isNaN(pageNumber)) pageNumber = 1;

				let totalNumberOfPages = Math.ceil(this.totalNumberOfRows / this.numberOfRowsPerPage);
				pageNumber = Math.min(Math.max(1, pageNumber), totalNumberOfPages);

				if (this.selectedPage.Page !== pageNumber) this.$emit('update:selectedPage', pageNumber);
				else this.navigateToPage(pageNumber);
			},

			setSelectionColumn(doAddSelectionColumn) {
				if (!this.localData) return;

				let selectionColumnId = 1;

				if (doAddSelectionColumn)
					this.localData.forEach(r => {
						r[SELECTION_COLUMN_NAME] = false;
						r[SELECTION_COLUMN_ID] = selectionColumnId++;
					});
				else
					this.localData.forEach(r => {
						delete r[SELECTION_COLUMN_NAME];
						delete r[SELECTION_COLUMN_ID];
					});
			},

			async filterData(searchQuery) {
				this.errorMessage = null;
				const applicationStore = useApplicationStore();
				applicationStore.isAppBusy = true;

				if (!this.localData) {
					this.filteredData = [];
					applicationStore.isAppBusy = false;
					return;
				}

				if (searchQuery === null || searchQuery === '') {
					this.filteredData = [];
					this.localData.forEach(r => this.filteredData.push(r));
					applicationStore.isAppBusy = false;
					return;
				}

				try {
					const dataToFilter =
						this.localData == null
							? []
							: this.localData.map(rowData => {
									let rowToFilter = new Object();

									this.columnDefinitions.forEach(columnDefinition => {
										rowToFilter[columnDefinition.name] = columnDefinition.staticText ?? rowData[columnDefinition.name];
									});

									if (this.hasSelectableData) {
										rowToFilter[SELECTION_COLUMN_NAME] = rowData[SELECTION_COLUMN_NAME];
										rowToFilter[SELECTION_COLUMN_ID] = rowData[SELECTION_COLUMN_ID];
									}

									return rowToFilter;
							  });

					await this.sendData(dataToFilter);

					let results = await this.$http
						.post(
							'api/Controls/DataGrid/Search',
							searchQuery,
							{
								params: { controlId: this.controlId }
							}
						)
						.catch(error => {
							this.errorMessage = error.response?.data?.title ?? error.message ?? error;
							this.localData.forEach(r => this.filteredData.push(r));
						});

					if (!results || !Array.isArray(results)) return;

					let resultsSet = new Set(results);

					this.filteredData = [];
					dataToFilter.forEach(r => {
						if (resultsSet.has(JSON.stringify(r))) this.filteredData.push(r);
					});
				} catch (error) {
					this.errorMessage = error.message ?? error;
				} finally {
					applicationStore.isAppBusy = false;
				}
			},

			sortAscending(elementA, elementB) {
				if (elementA > elementB) return 1;
				else if (elementA < elementB) return -1;
				else return 0;
			},

			sortDescending(elementA, elementB) {
				if (elementA > elementB) return -1;
				else if (elementA < elementB) return 1;
				else return 0;
			},

			sortData(columnName, sortDirection) {
				if (this.filteredData.length === 0 || columnName == null) return;

				if (sortDirection === DESCENDING) {
					if (this.hasSelectableData) {
						this.filteredData.sort(
							(a, b) => this.sortDescending(a[columnName], b[columnName]) || b[SELECTION_COLUMN_NAME] - a[SELECTION_COLUMN_NAME]
						);
					} else {
						this.filteredData.sort((a, b) => this.sortDescending(a[columnName], b[columnName]));
					}
				} else {
					if (this.hasSelectableData) {
						this.filteredData.sort(
							(a, b) => this.sortAscending(a[columnName], b[columnName]) || b[SELECTION_COLUMN_NAME] - a[SELECTION_COLUMN_NAME]
						);
					} else {
						this.filteredData.sort((a, b) => this.sortAscending(a[columnName], b[columnName]));
					}
				}
			},

			navigateToPage(pageNumber) {
				if (this.currentPageNumber !== pageNumber) this.currentPageNumber = pageNumber;

				if (this.filteredData.length === 0 || !this.showPaging) {
					this.pageData = this.filteredData.slice(0);
					return;
				}

				let rowStartIndex = (pageNumber - 1) * this.numberOfRowsPerPage;
				let rowEndIndex = Math.min(this.filteredData.length, rowStartIndex + this.numberOfRowsPerPage);

				this.pageData = this.filteredData.slice(rowStartIndex, rowEndIndex);
			},

			async exportData() {
				const dataToExport =
					this.filteredData == null
						? []
						: this.filteredData.map(rowData => {
								let rowToExport = new Object();

								this.columnDefinitions.forEach(columnDefinition => {
									rowToExport[columnDefinition.name] = columnDefinition.staticText ?? rowData[columnDefinition.name];
								});

								return rowToExport;
						  });

				let controlIdForFileName = this.controlId.includes('-item')
					? this.controlId.slice(0, this.controlId.indexOf('-item'))
					: this.controlId;
				let fileName = `Exported_${controlIdForFileName}_${dayjs().format('YYYYMMDD')}.xlsx`;

				if (!this.isExporting) {
					this.isExporting = true;

					try {
						await this.sendData(dataToExport);

						let blob = await this.$http.get('api/Controls/DataGrid/Export', {
							params: { controlId: this.controlId },
							responseType: 'blob'
						});

						const url = window.URL.createObjectURL(blob);
						const a = document.createElement('a');
						a.style.display = 'none';
						a.href = url;
						a.download = fileName;
						document.body.append(a);
						a.click();
						a.remove();
						window.URL.revokeObjectURL(url);
					} catch (error) {
						this.errorMessage = error.response?.data?.title ?? error.message ?? error;
					} finally {
						this.isExporting = false;
					}
				}
			},

			async sendData(data) {
				let chunks = [];
				if (!data || (Array.isArray(data) && data.length == 0)) {
					chunks.push([]);
				} else {
					for (let i = 0; i < data.length; i += ROWS_PER_CHUNK) {
						chunks.push(data.slice(i, i + ROWS_PER_CHUNK));
					}
				}

				for (let i = 0; i < chunks.length; i++) {
					await this.$http
						.post(
							'api/Controls/DataGrid/SetData',
							{
								jsonData: JSON.stringify(chunks[i]),
								columnNameHeaderMap: this.columnNameHeaderMap
							},
							{
								params: {
									controlId: this.controlId,
									isChunkStart: i === 0
								}
							}
						);
				}
			},

			setSearchTerm(newSearchTerm) {
				this.$emit('update:searchTerm', newSearchTerm);
			},

			setSortColumn(newSortColumnName) {
				this.sortColumnName = newSortColumnName;
				let newSortDirection = this.sortDirectionByColumnMap[newSortColumnName] === ASCENDING ? DESCENDING : ASCENDING;
				this.sortDirectionByColumnMap[newSortColumnName] = newSortDirection;
			},

			toggleSelectAllPageRows() {
				const previousSelection = this.areAllPageRowsSelected;
				this.pageData.forEach(pageRow => {
					pageRow[SELECTION_COLUMN_NAME] = !previousSelection;
					this.localData.find(r => r[SELECTION_COLUMN_ID] === pageRow[SELECTION_COLUMN_ID])[SELECTION_COLUMN_NAME] = !previousSelection;
				});
			},

			clearSelection() {
				this.localData.forEach(r => (r[SELECTION_COLUMN_NAME] = false));
			},

			getHeaderSortDirection(columnName) {
				return columnName === this.sortColumnName ? this.sortDirectionByColumnMap[columnName] : '';
			},

			toggleSelected(row) {
				this.$nextTick(function () {
					const selectionValue = row[SELECTION_COLUMN_NAME];
					this.localData.find(r => r[SELECTION_COLUMN_ID] === row[SELECTION_COLUMN_ID])[SELECTION_COLUMN_NAME] = selectionValue;
				});
			},

			getSelectedValue(row) {
				return this.localData.find(r => r[SELECTION_COLUMN_ID] === row[SELECTION_COLUMN_ID])?.[SELECTION_COLUMN_NAME] ?? false;
			}
		}
	};
</script>
