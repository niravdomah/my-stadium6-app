<template>
	<div v-show="visible" :id="`${controlId}-container`" :style="containerStyle" :class="classes">
		<img
			:id="controlId"
			:src="src"
			:alt="altText"
			:style="imageStyle"
			:class="hasClickEvent ? 'clickable' : ''"
			class="img-thumbnail"
			@click="onClick"
		/>
	</div>
</template>

<script>
	import { trimEnd, isNumber, isString } from 'lodash';
	import fileHandler from '@/utils/file-handler.js';
	import navigation from '@/utils/navigation.js';

	export default {
		name: 'StadiumImage',

		props: {
			controlId: String,
			image: null,
			width: null,
			height: null,
			altText: String,
			visible: Boolean,
			hasClickEvent: Boolean,
			classes: null
		},

		emits: ['click'],

		computed: {
			src() {
				if (Array.isArray(this.image)) {
					return `data:application/octet-stream;base64,${this.convertArrayToBase64String(this.image)}`;
				}

				if (fileHandler.isFileToken(this.image)) {
					let webAppUrl = trimEnd(navigation.getWebAppUrl(), '/');
					return `${webAppUrl}/api/FileHandler?content=${encodeURIComponent(JSON.stringify(this.image))}`;
				}

				if (this.image?.startsWith('~/')) {
					return this.image.substring(2);
				}

				return this.image;
			},

			containerStyle() {
				let containerStyle = {};

				let imageWidth = this.resolveContainerDimension(this.width);
				if (imageWidth) containerStyle.width = imageWidth;

				let imageHeight = this.resolveContainerDimension(this.height);
				if (imageHeight) containerStyle.height = imageHeight;

				return containerStyle;
			},

			imageStyle() {
				let imageStyle = {};

				let imageWidth = this.resolveImageDimension(this.width);
				if (imageWidth) imageStyle.width = imageWidth;

				let imageHeight = this.resolveImageDimension(this.height);
				if (imageHeight) imageStyle.height = imageHeight;

				return imageStyle;
			}
		},

		methods: {
			onClick() {
				if (this.hasClickEvent) this.$emit('click');
			},

			convertArrayToBase64String(array) {
				let uint8Array = new Uint8Array(array);
				let subarrayLength = 0x8000;
				let characters = [];
				for (let index = 0; index < uint8Array.length; index += subarrayLength) {
					characters.push(String.fromCharCode.apply(null, uint8Array.subarray(index, index + subarrayLength)));
				}
				return window.btoa(characters.join(''));
			},

			isNumeric(stringValue) {
				if (typeof stringValue != 'string') return false;

				return !isNaN(stringValue) && !isNaN(parseFloat(stringValue));
			},

			resolveImageDimension(dimension) {
				let imageDimension = this.resolveDimension(dimension);

				if (imageDimension?.endsWith('%')) return '100%';

				return imageDimension;
			},

			resolveContainerDimension(dimension) {
				let containerDimension = this.resolveDimension(dimension);

				if (containerDimension?.endsWith('%')) return containerDimension;

				return null;
			},

			resolveDimension(dimension) {
				if (dimension == null || dimension === '') return dimension;

				if (isNumber(dimension)) return dimension + 'px';

				if (isString(dimension)) {
					if (this.isNumeric(dimension)) return dimension + 'px';

					return dimension;
				}

				throw new Error(`Invalid dimension: ${dimension}`);
			}
		}
	};
</script>
