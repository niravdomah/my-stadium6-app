<template>
	<div>
	<StackLayout>
<div class="page-content">
	<slot />
</div>
</StackLayout>
</div>
</template>

<script>
	import StackLayout from '@/components/StackLayout.vue';
	import { isEqual } from 'lodash';
	import { mapActions } from 'pinia';
	import { useApplicationStore } from '@/stores/application.js';
	import { useEventsStore } from '@/stores/events.js';
	import { useSessionVariablesStore } from '@/stores/session-variables.js';
	import { useValidationsStore } from '@/stores/validations.js';
	import { useMessageBoxStore } from '@/stores/message-box.js';
	import * as types from '@/types/types.js';
	import typesHelper from '@/types/types-helper.js';
	import typeResolver from '@/utils/type-resolver.js';
	import fileHandler from '@/utils/file-handler.js';
	import consoleLogGroupHelper from '@/utils/console-log-group-helper.js';
	import expressionHelper from '@/utils/expression-helper.js';
	import navigation from '@/utils/navigation.js';
	import errorHandling from '@/utils/error-handling.js';
	import dayjs from 'dayjs';
	import * as signalR from '@microsoft/signalr';

	export default {
		name: 'DefaultTemplate',

		inject: ['$http', '$notification', '$debugLogger', '$globalScripts'],

		components: {
			StackLayout
		},

		props: {
			settings: Object,
			loggedInUser: Object
		},

		data() {
			return {
				
			}
		},

		created() {
			const validationsStore = useValidationsStore();
			validationsStore.clearValidationsState();
		},

		async mounted() {
			

			await this.executeEvent({ handler: this.DefaultTemplate_46Load, eventId: '33526503-dd4a-4de5-b458-4378dd505757' });

			const applicationStore = useApplicationStore();
			applicationStore.isAppBusy = false;
},

		watch: {
			$route(to, from) {
				if (to.path === from.path && !isEqual(to.query, from.query)) {
					location.reload();
				}
			}
		},

		methods: {
			...mapActions(useEventsStore, ['executeEvent']),

			async DefaultTemplate_46Load(logDebugInfo = true, logGroupId = null) {
	let scriptInstanceId = Math.floor(Math.random() * Date.now());
	
	if (import.meta.env.VITE_APP_DEBUG === 'true') {
	if (logDebugInfo) {
		if (logGroupId == null) {
		
		logGroupId = consoleLogGroupHelper.generateGroupId();
	}
	}
}
	let connectionId;
if (import.meta.env.VITE_APP_DEBUG === 'true') {
	if (logDebugInfo) {
		if (this.$debugLogger.state === signalR.HubConnectionState.Connected) {
			connectionId = await this.$debugLogger.invoke('GetConnectionId');
		}
	}
}
	
	let DefaultTemplate_46Load_values = {};
	
	

	
}
		}
	};
</script>