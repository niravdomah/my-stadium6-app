<template>
	<li :class="resolvedListItemClasses" @mouseenter="onMouseEnter" :role="resolvedRole">
		<a v-if="!isSeparator" :class="resolvedLinkClass" @click.prevent="onLinkClick">
			<span class="dropdown-item-text">{{ resolvedText }}</span>
			<span v-if="isComplexRoot" class="caret"></span>
		</a>

		<ul ref="unorderedList" v-if="hasMenuItems" class="dropdown-menu" :class="resolvedUnorderedListClass">
			<StadiumMenuItem
				v-for="(menuItem, index) in items"
				:key="index"
				:text="menuItem.text"
				:url="menuItem.url"
				:isRoot="menuItem.isRoot"
				:isSeparator="menuItem.isSeparator"
				:expandDirection="childrenExpandDirection"
				:items="menuItem.items"
			/>
		</ul>
	</li>
</template>

<script>
	import { mapState } from 'pinia';
	import { useDeviceStore } from '@/stores/device.js';
	import navigation from '@/utils/navigation.js';
	import htmlDomHelper from '@/utils/html-dom-helper.js';

	export default {
		name: 'StadiumMenuItem',

		props: {
			text: String,
			url: String,
			isRoot: Boolean,
			isSeparator: Boolean,
			expandDirection: String,
			items: Array
		},

		data() {
			return {
				childrenExpandDirection: this.expandDirection,
				isMobileMenuOpen: false
			};
		},

		computed: {
			...mapState(useDeviceStore, ['isMobile']),

			hasMenuItems() {
				return this.items.length > 0;
			},

			isComplexRoot() {
				return this.isRoot && this.hasMenuItems;
			},

			resolvedText() {
				return this.isComplexRoot ? ` ${this.text} ` : this.text;
			},

			resolvedListItemClasses() {
				if (this.isSeparator) return 'divider';

				if (!this.hasMenuItems) return '';

				if (this.isComplexRoot) return 'dropdown' + (this.isMobileMenuOpen ? ' open' : '');

				return `dropdown-submenu expand-${this.expandDirection}` + (this.isMobileMenuOpen ? ' open' : '');
			},

			resolvedLinkClass() {
				return this.url == null || this.url === '' ? '' : 'menu-link';
			},

			resolvedUnorderedListClass() {
				return this.isMobileMenuOpen ? 'menu-open' : '';
			},

			resolvedRole() {
				return this.isSeparator ? 'separator' : '';
			}
		},

		methods: {
			onMouseEnter() {
				if (!this.hasMenuItems) return;

				let nestedLiElements = [...this.$refs['unorderedList'].children];

				let isOutOfWindow = false;
				for (let liElement of nestedLiElements) {
					let ulElement = htmlDomHelper.getChild(liElement, 'ul');
					if (!ulElement) continue;

					let ulElementWidth = htmlDomHelper.getHiddenElementWidth(ulElement);

					let liElementRect = liElement.getBoundingClientRect();
					let liElementLeftPosition = liElementRect.x;
					let liElementRightPosition = liElementRect.x + liElementRect.width;

					if (
						(this.expandDirection === 'left' && liElementLeftPosition - ulElementWidth < 0) ||
						(this.expandDirection === 'right' && liElementRightPosition + ulElementWidth > document.body.clientWidth)
					) {
						isOutOfWindow = true;
						break;
					}
				}

				this.childrenExpandDirection = isOutOfWindow ? (this.expandDirection === 'left' ? 'right' : 'left') : this.expandDirection;
			},

			onLinkClick(event) {
				if (this.isMobile && ((this.hasMenuItems && !this.isRoot) || event.target.classList.contains('caret'))) {
					this.isMobileMenuOpen = !this.isMobileMenuOpen;
				} else if (this.url) {
					navigation.browseTo(this.$router, this.url);
				}
			}
		}
	};
</script>
