<template>
	<nav v-show="visible" :id="`${controlId}-container`" :class="classes">
		<div class="container-fluid">
			<div class="navbar-header" :class="isMobileMenuOpen ? '' : 'collapsed'">
				<div class="hamburger navbar-toggle" @click="toggleMobileMenu">
					<span class="caret"></span>
				</div>
			</div>

			<div class="collapse navbar-collapse" :class="isMobileMenuOpen ? 'in' : ''">
				<ul :class="directionClass" class="nav navbar-nav">
					<StadiumMenuItem
						v-for="(menuItem, index) in allowedItems"
						:key="index"
						:text="menuItem.text"
						:url="menuItem.url"
						:isRoot="menuItem.isRoot"
						:isSeparator="menuItem.isSeparator"
						:expandDirection="'right'"
						:items="menuItem.items"
					/>
				</ul>
			</div>
		</div>
	</nav>
</template>

<script>
	import { cloneDeep } from 'lodash';
	import { mapState } from 'pinia';
	import { useAuthenticationStore } from '@/stores/authentication.js';
	import navigation from '@/utils/navigation.js';
	import StadiumMenuItem from '@/components/controls/StadiumMenu/StadiumMenuItem.vue';

	export default {
		name: 'StadiumMenu',
		components: {
			StadiumMenuItem
		},

		props: {
			controlId: String,
			direction: String,
			items: Array,
			visible: Boolean,
			classes: null
		},

		data() {
			return {
				isMobileMenuOpen: false
			};
		},

		computed: {
			...mapState(useAuthenticationStore, ['isAnonymousAuthentication', 'accessiblePagesSet']),

			directionClass() {
				return this.direction === 'TopToBottom' ? 'navbar-left' : '';
			},

			allowedItems() {
				if (this.isAnonymousAuthentication) return this.items;

				let itemsCopy = cloneDeep(this.items);
				return this.removeItemsWithNoChildren(this.removeUnauthorizedItems(itemsCopy));
			}
		},

		methods: {
			removeUnauthorizedItems(menuItems) {
				return menuItems.filter(i => {
					if (i.items.length > 0) {
						i.items = this.removeUnauthorizedItems(i.items);
					}

					if (!navigation.isRelativeUrl(i.url)) return true;

					let pageName = i.url.substring(1).split('?')[0].split('/')[0];

					let isAllowed = this.accessiblePagesSet.has(pageName.toLowerCase());
					if (isAllowed) return true;

					if (i.items.length > 0) {
						i.url = null;
						return true;
					}

					return false;
				});
			},

			removeItemsWithNoChildren(menuItems) {
				return menuItems.filter(i => this.removeItemsWithNoChildren(i.items).length > 0 || (!i.isSeparator && i.url != null));
			},

			toggleMobileMenu() {
				this.isMobileMenuOpen = !this.isMobileMenuOpen;
			}
		}
	};
</script>
