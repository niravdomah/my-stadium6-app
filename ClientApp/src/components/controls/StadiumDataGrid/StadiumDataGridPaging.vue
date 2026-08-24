<template>
	<ul class="pagination">
		<li v-if="pagingStart > 1">
			<a :id="`${controlId}-page-previous`" @click="setCurrentPageNumber(pagingStart - 1)"> « </a>
		</li>

		<li v-for="p in pagingRange" :key="p" :class="p === currentPageNumber ? 'active' : ''">
			<a :id="`${controlId}-page${p}`" @click="setCurrentPageNumber(p)">
				{{ p }}
			</a>
		</li>

		<li v-if="pagingEnd < pageCount">
			<a :id="`${controlId}-page-next`" @click="setCurrentPageNumber(pagingEnd + 1)"> » </a>
		</li>
	</ul>
</template>

<script>
	import { range } from 'lodash';

	export default {
		name: 'StadiumDataGridPaging',
		props: {
			controlId: String,
			maxNumberOfPagesDisplayed: Number,
			numberOfRowsPerPage: Number,
			totalNumberOfRows: Number,
			currentPageNumber: Number
		},
		emits: ['update:currentPageNumber'],

		computed: {
			pageCount() {
				return Math.ceil(this.totalNumberOfRows / this.numberOfRowsPerPage);
			},
			pagingStart() {
				return Math.floor((this.currentPageNumber - 1) / this.maxNumberOfPagesDisplayed) * this.maxNumberOfPagesDisplayed + 1;
			},
			pagingEnd() {
				return Math.min(this.pageCount, this.pagingStart + this.maxNumberOfPagesDisplayed - 1);
			},
			pagingRange() {
				return range(this.pagingStart, this.pagingEnd + 1);
			}
		},

		methods: {
			setCurrentPageNumber(newCurrentPageNumber) {
				this.$emit('update:currentPageNumber', newCurrentPageNumber);
			}
		}
	};
</script>
