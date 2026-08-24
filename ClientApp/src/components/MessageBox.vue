<template>
	<Modal id="display-message-box-modal" :showModal="show" :clickOutsideToClose="false">
		<div>
			<div class="dialog-message">{{ message }}</div>
		</div>

		<template #footer>
			<div>
				<div>
					<button v-for="(button, index) in buttons" :key="index" class="btn btn-default" @click="button.eventHandler">
						{{ button.label }}
					</button>
				</div>
			</div>
		</template>
	</Modal>
</template>

<script>
	import { mapState } from 'pinia';
	import { useMessageBoxStore } from '@/stores/message-box.js';
	import Modal from '@/components/Modal.vue';

	export default {
		name: 'MessageBox',
		components: {
			Modal
		},

		computed: {
			...mapState(useMessageBoxStore, ['show', 'message', 'buttons'])
		}
	};
</script>
