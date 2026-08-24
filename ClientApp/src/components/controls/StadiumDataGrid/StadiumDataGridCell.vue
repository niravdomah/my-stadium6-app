<template>
	<button
		v-if="columnDefinition.hasClickEvent"
		:id="`${controlId}_${columnDefinition.name}-row${rowIndex}`"
		:disabled="disabled"
		:class="`grid-cell-display-${columnDefinition.cellDisplay}`"
		class="btn btn-lg btn-link"
		@click="onClick"
	>
		<span v-if="columnDefinition.cellDisplay == 'ellipsis'" class="grid-cell-display-ellipsis" :title="getCellContent(columnDefinition, row)">
			{{ getCellContent(columnDefinition, row) }}
		</span>
		<template v-else>{{ getCellContent(columnDefinition, row) }}</template>
	</button>

	<span v-else-if="columnDefinition.cellDisplay == 'ellipsis'" class="grid-cell-display-ellipsis" :title="getCellContent(columnDefinition, row)">
		{{ getCellContent(columnDefinition, row) }}
	</span>

	<template v-else>{{ getCellContent(columnDefinition, row) }}</template>
</template>

<script>
	import { toDisplayText } from '@/utils/data-grid-display-text.js';
	import { getCaseInsensitiveValue } from '@/utils/object-value-case-insensitive.js';

	export default {
		name: 'StadiumDataGridCell',
		props: {
			controlId: String,
			columnDefinition: Object,
			row: Object,
			rowIndex: Number
		},
		emits: ['cell-click'],

		data() {
			return {
				disabled: false
			};
		},

		methods: {
			getCellContent(columnDefinition, row) {
				return (
					columnDefinition.staticText ??
					toDisplayText(getCaseInsensitiveValue(row, columnDefinition.name))
				);
			},

			onClick() {
				this.disabled = true;
				this.$emit('cell-click', () => (this.disabled = false));
			}
		}
	};
</script>
