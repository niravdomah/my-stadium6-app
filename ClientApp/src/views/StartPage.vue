<template>
	<DefaultTemplate v-bind="$props">
	<StackLayout>
<StadiumButton
	:ref="`StartPage_Button`"
	:control-id="`StartPage_Button`"
	:text="ButtonText"
	:tooltip="ButtonToolTip"
	:visible="ButtonVisible"
	:hasClickEvent="true"
	:classes="ButtonClasses"
	@click="
	(doneCallback) =>
		executeEvent({
			handler: () => Button_46Click(),
			eventId: 'a8394218-6985-44cf-a44a-bfb68aaf15ee',
			doneCallback,
			controlIdsToValidate: []
		})
"
/>
</StackLayout>
</DefaultTemplate>
</template>

<script>
	import DefaultTemplate from '@/views/templates/DefaultTemplate.vue';
	import StackLayout from '@/components/StackLayout.vue';
	import StadiumButton from '@/components/controls/StadiumButton.vue';
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
		name: 'StartPage',

		inject: ['$http', '$notification', '$debugLogger', '$globalScripts'],

		components: {
			DefaultTemplate,
			StackLayout,
			StadiumButton
		},

		props: {
			settings: Object,
			loggedInUser: Object
		},

		data() {
			return {
				ButtonClasses: errorHandling.invoke(() => ('control-container button-container')),
				ButtonText: errorHandling.invoke(() => (typeResolver.toString('Click Me'))),
				ButtonToolTip: errorHandling.invoke(() => (typeResolver.toString(undefined))),
				ButtonVisible: errorHandling.invoke(() => (typeResolver.toBoolean(true))),
			}
		},

		created() {
			const validationsStore = useValidationsStore();
			validationsStore.clearValidationsState();
		},

		async mounted() {
			

			await this.executeEvent({ handler: this.StartPage_46Load, eventId: '92965c67-e91d-4cc3-8f35-f51446825476' });

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

			async Button_46Click(logDebugInfo = true, logGroupId = null) {
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
	
	let Button_46Click_values = {};
	
	
try {
	if (import.meta.env.VITE_APP_DEBUG === 'true') {
	if (logDebugInfo) {
		consoleLogGroupHelper.appendToGroup(logGroupId, consoleLogGroupHelper.TypeGroup, `Button.Click`);
	}
}
	
try {
	if (import.meta.env.VITE_APP_DEBUG === 'true') {
	if (logDebugInfo) {
		consoleLogGroupHelper.appendToGroup(logGroupId, consoleLogGroupHelper.TypeGroupCollapsed, 'Notification');
consoleLogGroupHelper.appendToGroup(logGroupId, consoleLogGroupHelper.TypeLog, `Message: ${typeResolver.toString(typeResolver.toString('Hello'))}
Type: Info`);


	}
}

	

	this.$notification.showInfo(typeResolver.toString('Hello'), typeResolver.toString(''), typeResolver.toBoolean(false));

} catch(error) {
	if (!error.breadcrumb) {
		error.breadcrumb = 'Button.Click > Notification';
	}

	consoleLogGroupHelper.appendToGroup(logGroupId, consoleLogGroupHelper.TypeError, error);
	throw error;
} finally {
	if (import.meta.env.VITE_APP_DEBUG === 'true') {
	if (logDebugInfo) {
		consoleLogGroupHelper.appendToGroup(logGroupId, consoleLogGroupHelper.TypeGroupEnd);
	}
}
}
} finally {
	if (import.meta.env.VITE_APP_DEBUG === 'true') {
	if (logDebugInfo) {
		consoleLogGroupHelper.appendToGroup(logGroupId, consoleLogGroupHelper.TypeGroupEnd);
	}
}
	if (import.meta.env.VITE_APP_DEBUG === 'true') {
	if (logDebugInfo) {
		consoleLogGroupHelper.closeGroup(logGroupId);
	}
}
}

	
},

			async StartPage_46Load(logDebugInfo = true, logGroupId = null) {
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
	
	let StartPage_46Load_values = {};
	
	

	
}
		}
	};
</script>