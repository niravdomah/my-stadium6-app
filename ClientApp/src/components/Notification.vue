<template>
	<div>
		<div v-if="title" class="toast-title">{{ title }}</div>
		<div class="toast-message" v-html="encodedMessage"></div>
	</div>
</template>

<script>
	export default {
		name: 'Notification',

		props: {
			message: {
				type: String,
				required: true
			},
			title: String
		},

		computed: {
			encodedMessage: function () {
				return this.message.replace(/\r\n|\r|\n/g, '<br />');
			}
		}
	};
</script>
