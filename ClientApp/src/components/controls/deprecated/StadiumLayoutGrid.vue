<template>
	<div v-show="visible" :id="`${controlId}-container`" :class="classes">
		<div class="row">
			<div v-for="(cell, cellIndex) in cells" :key="cellIndex" :class="`col-md-${cell.span}`">
				<slot :name="`cell${cellIndex}`" />
			</div>
		</div>
	</div>
</template>

<script>
	export default {
		name: 'StadiumLayoutGrid',

		props: {
			controlId: String,
			cells: Array,
			visible: Boolean,
			classes: null
		},

		mounted() {
			let pageContent = document.querySelector('.page-content');
			if (pageContent != null && !pageContent.classList.contains('width-100-percent')) {
				pageContent.classList.add('width-100-percent');
				pageContent.parentElement.classList.add('display-flex');
			}
		}
	};
</script>
