<template>
	<div v-show="visible" :id="`${controlId}-container`" :class="classes">
		<router-link v-if="isRelativeUrl && !hasClickEvent" :id="controlId" :target="target" :to="url" class="btn btn-lg btn-link">{{
			text
		}}</router-link>
		<a
			v-else
			:id="controlId"
			:disabled="disabled"
			:target="target"
			:href="externalUrl"
			rel="noopener noreferrer"
			class="btn btn-lg btn-link"
			@click="onClick"
			>{{ text }}</a
		>
	</div>
</template>

<script>
	import navigation from '@/utils/navigation.js';

	export default {
		name: 'StadiumLink',

		props: {
			controlId: String,
			hasClickEvent: Boolean,
			openInNewWindow: Boolean,
			text: null,
			url: String,
			visible: Boolean,
			classes: null
		},

		emits: ['click'],

		data() {
			return {
				disabled: null
			};
		},

		computed: {
			isRelativeUrl() {
				return navigation.isRelativeUrl(this.url);
			},
			target() {
				return this.openInNewWindow ? '_blank' : null;
			},
			externalUrl() {
				if (this.hasClickEvent) return null;

				if (this.url == null || this.url === '') return '#';

				return this.url;
			}
		},

		methods: {
			onClick() {
				if (this.hasClickEvent && !this.disabled) {
					this.disabled = true;
					this.$emit('click', () => {
						if (this.url) this.navigate();
						this.disabled = null;
					});
				}
			},

			navigate() {
				navigation.browseTo(this.$router, this.url, this.openInNewWindow);
			}
		}
	};
</script>
