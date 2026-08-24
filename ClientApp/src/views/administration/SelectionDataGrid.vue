<template>
	<div>
		<div v-if="heading" class="stack-layout-container">
			<div :class="headingClass" class="control-container label-container admin-text-formatter">
				<span>{{ heading }}</span>
			</div>
			<div class="control-container drop-down-container table-filter">
				<select v-model="rowSelection" class="form-control">
					<option>All</option>
					<option>Selected</option>
					<option>Unselected</option>
				</select>
			</div>
		</div>

		<div class="stack-layout-container">
			<div class="control-container data-grid-container">
				<table class="table table-striped rows-table">
					<thead>
						<tr>
							<th class="action-column">
								<input id="check-all-checkbox" :checked="areAllRowsSelected" type="checkbox" @change="toggleSelectAllRows($event.target.checked)" />
								<label for="check-all-checkbox" />
							</th>
							<th v-for="(columnHeader, columnHeaderIndex) in columnHeaders" :key="columnHeaderIndex">{{ columnHeader }}</th>
						</tr>
					</thead>
					<tbody>
						<tr
							v-for="row in filteredRows"
							:key="row.id"
							:title="row.isReadOnly ? row.readOnlyRowTitle : null"
							:class="{ 'default-row': row.isReadOnly }"
							class="selection-row"
						>
							<td class="grid-cell-display-default action-column">
								<input
									:id="row.id"
									:checked="isRowSelected(row.name)"
									:disabled="row.isReadOnly"
									@change="toggleSelectRow($event.target.checked, row.name)"
									class="select-row-check-box"
									type="checkbox"
								/>
								<label 
									:for="row.id"
									:disabled="row.isReadOnly"/>
							</td>
							<td class="grid-cell-display-default">
								<label :for="row.id" class="row-name">{{ row.name }}</label>
							</td>
							<td v-for="(property, propertyIndex) in additionalProperties" :key="propertyIndex" class="grid-cell-display-default">
								<label>{{ row[property] }}</label>
							</td>
						</tr>
					</tbody>
				</table>
			</div>
		</div>
	</div>
</template>

<script>
	export default {
		name: 'SelectionDataGrid',
		props: {
			columnHeaders: Array,
			rows: Array,
			selectedRowNames: Array,
			heading: String,
			headingClass: String,
			additionalProperties: {
				type: Array,
				default: () => []
			}
		},

		data() {
			return {
				rowSelection: 'All'
			};
		},

		computed: {
			filteredRows() {
				switch (this.rowSelection) {
					case 'Selected':
						return this.rows.filter(r => this.isRowSelected(r.name));
					case 'Unselected':
						return this.rows.filter(r => !this.isRowSelected(r.name));
					case 'All':
					default:
						return this.rows;
				}
			},

			areAllRowsSelected() {
				return this.filteredRows.length > 0 ? this.filteredRows.every(r => this.isRowSelected(r.name)) : false;
			}
		},

		methods: {
			isRowSelected(rowName) {
				return this.selectedRowNames.includes(rowName);
			},

			getUpdatedSelectedRowNames(rowName, selected, updatedSelectedRowNames) {
				let rowIndex = updatedSelectedRowNames.indexOf(rowName);

				if (selected && rowIndex < 0) return updatedSelectedRowNames.concat([rowName]);

				if (!selected && rowIndex >= 0)
					return updatedSelectedRowNames.slice(0, rowIndex).concat(updatedSelectedRowNames.slice(rowIndex + 1));

				return updatedSelectedRowNames;
			},

			toggleSelectRow(doSelect, rowName) {
				let updatedSelectedRowNames = this.selectedRowNames;

				updatedSelectedRowNames = this.getUpdatedSelectedRowNames(rowName, doSelect, updatedSelectedRowNames);

				this.$emit('update:selectedRowNames', updatedSelectedRowNames);
			},

			toggleSelectAllRows(selected) {
				let updatedSelectedRowNames = this.selectedRowNames;

				for (let i = 0; i < this.filteredRows.length; i++) {
					let row = this.filteredRows[i];
					if (row.isReadOnly) continue;

					updatedSelectedRowNames = this.getUpdatedSelectedRowNames(row.name, selected, updatedSelectedRowNames);
				}

				this.$emit('update:selectedRowNames', updatedSelectedRowNames);
			}
		}
	};
</script>
