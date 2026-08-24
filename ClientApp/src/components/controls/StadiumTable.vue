<template>
	<table v-show="visible" :id="`${controlId}-container`" :class="classes">
		<tbody>
			<tr v-for="(_, rowIndex) in rowCount" :key="rowIndex">
				<td v-for="(_, columnIndex) in columnCount" :key="columnIndex">
					<slot :name="`row${rowIndex}col${columnIndex}`" />
				</td>
			</tr>
		</tbody>
	</table>
</template>

<script>
	export default {
		name: 'StadiumTable',

		props: {
			controlId: String,
			columnCount: Number,
			rowCount: Number,
			visible: Boolean,
			classes: null
		}
	};
</script>
