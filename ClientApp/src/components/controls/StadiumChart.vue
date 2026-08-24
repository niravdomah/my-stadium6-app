<template>
	<div :id="`${controlId}-container`" :class="classes">
		<v-chart class="chart" :option="resolvedOption" :autoresize="true" />
	</div>
</template>

<script>
	import VChart from 'vue-echarts';
	import 'echarts';

	export default {
		name: 'StadiumChart',
		components: {
			VChart
		},

		props: {
			controlId: String,
			option: null,
			classes: null
		},

		computed: {
			resolvedOption() {
				if (this.option == null) return {};

				if (typeof this.option === 'string') return JSON.parse(this.option);

				return this.option;
			}
		},

		mounted() {
			let pageContent = document.querySelector('.page-content');
			if (!pageContent.classList.contains('width-100-percent')) {
				pageContent.classList.add('width-100-percent');
				pageContent.parentElement.classList.add('display-flex');
			}
		}
	};
</script>
