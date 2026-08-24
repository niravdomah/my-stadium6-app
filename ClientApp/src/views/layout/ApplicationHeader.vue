<template>
	<div>
		<div class="header">
			<span v-if="hasProfile">
				<button id="profile-icon" class="profile-icon" @click="toggleProfileModal"></button>
			</span>
		</div>

		<Modal :id="'profile-modal'" v-model:showModal="showProfileModal">
			<router-link id="users-and-roles-link" v-if="isAdministrator" :to="'/Admin'">Users &amp; Roles</router-link>
			<a id="change-password-link" v-if="hasChangePassword" @click="showChangePasswordModal = true">Change Password</a>
			<a v-if="canLogout" id="logout-link" @click="logoutUser">Logout</a>
		</Modal>

		<Modal
			:id="'change-password-modal'"
			v-model:showModal="showChangePasswordModal"
			:classes="'auto-width-dialog'"
			:clickOutsideToClose="false"
		>
			<ChangePassword
				:id="'change-password-form'"
				v-model:showChangePasswordModal="showChangePasswordModal"
				v-model:isSubmittingForm="isSubmittingChangePasswordForm"
			/>

			<template #footer>
				<div class="stack-layout-container">
					<div class="control-container button-container">
						<button id="save-button" :disabled="isSubmittingChangePasswordForm" form="change-password-form" type="submit" class="btn btn-lg btn-default">
							Save
						</button>
					</div>
					<div class="control-container button-container">
						<button id="cancel-button" form="change-password-form" class="btn btn-lg btn-default" type="reset">Cancel</button>
					</div>
				</div>
			</template>
		</Modal>
	</div>
</template>

<script>
	import { mapState, mapActions } from 'pinia';
	import { useApplicationStore } from '@/stores/application.js';
	import { useAuthenticationStore } from '@/stores/authentication.js';

	import Modal from '@/components/Modal.vue';
	import ChangePassword from '@/views/layout/ChangePassword.vue';

	export default {
		name: 'Header',
		components: {
			Modal,
			ChangePassword
		},

		data() {
			return {
				showProfileModal: false,
				showChangePasswordModal: false,
				isSubmittingChangePasswordForm: false
			};
		},

		computed: {
			...mapState(useAuthenticationStore, ['hasProfile', 'isAdministrator', 'isCookieAuthentication', 'hasChangePassword', 'canLogout'])
		},

		methods: {
			...mapActions(useApplicationStore, ['clearApplicationState']),
			...mapActions(useAuthenticationStore, ['logout']),

			toggleProfileModal() {
				this.showProfileModal = !this.showProfileModal;
			},

			async logoutUser() {
				let isCookieAuthentication = this.isCookieAuthentication;
				await this.logout();

				if (isCookieAuthentication) {
					this.$router.push({
						name: 'Login'
					});
				}
			}
		}
	};
</script>
