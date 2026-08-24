
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
		install: (app, options) => {
			function globalScripts() {
				return {
					
				}
			}

			app.provide('$globalScripts', globalScripts);
		}
	};