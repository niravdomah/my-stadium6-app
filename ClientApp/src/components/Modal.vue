<template>
	<Teleport to="#modal-target">
		<Transition name="fade">
			<div v-if="showModal" class="modal-container">
				<div :id="id" :class="classes" class="modal" @click="toggleModal">
					<div class="modal-dialog">
						<div class="modal-content">
							<div class="modal-header">
								<slot name="header" />
							</div>

							<div class="modal-body">
								<slot />
							</div>

							<div class="modal-footer">
								<slot name="footer" />
							</div>
						</div>
					</div>
				</div>
				<div class="modal-backdrop"></div>
			</div>
		</Transition>
	</Teleport>
</template>

<script>
	export default {
		name: 'Modal',

		props: {
			id: String,
			showModal: Boolean,
			classes: String,
			clickOutsideToClose: {
				type: Boolean,
				default: true
			}
		},

		methods: {
			toggleModal() {
				if (this.clickOutsideToClose) this.$emit('update:showModal', !this.showModal);
			}
		}
	};
</script>

<style>
	.fade-enter-active,
	.fade-leave-active {
		transition: opacity 0.3s;
	}

	.fade-enter,
	.fade-leave-to {
		opacity: 0;
	}
</style>
