<template>
	<div class="admin">
		<div class="stack-layout-container admin-header">
			<div class="back-to-application">
				<router-link to="/" class="application-link menu-link">{{ webAppName }}</router-link>
				<span>Administration</span>
			</div>
			<nav class="navbar navbar-default control-container menu-container">
				<div class="container-fluid">
					<ul class="nav navbar-nav">
						<li>
							<router-link id="users-link" :to="{ name: 'Users' }" class="menu-link">Users</router-link>
						</li>
						<li>
							<router-link id="pages-link" :to="{ name: 'Pages' }" class="menu-link">Pages</router-link>
						</li>
						<li>
							<router-link id="roles-link" :to="{ name: 'Roles' }" class="menu-link">Roles</router-link>
						</li>
					</ul>
				</div>
			</nav>
		</div>
		<router-view />
	</div>
</template>

<script>
	import { useApplicationStore } from '@/stores/application.js';

	export default {
		name: 'Administration',

		data() {
			return {
				webAppName: import.meta.env.VITE_APP_TITLE
			};
		},

		mounted() {
			const applicationStore = useApplicationStore();
			applicationStore.isAppBusy = false;
		}
	};
</script>
