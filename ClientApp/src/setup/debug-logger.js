import * as signalR from '@microsoft/signalr';
import navigation from '@/utils/navigation.js';
import consoleLogGroupHelper from '@/utils/console-log-group-helper.js';

const connection = new signalR.HubConnectionBuilder()
	.withUrl(`${navigation.getWebAppUrl()}hubs/debugLoggerHub`)
	.configureLogging(signalR.LogLevel.Error)
	.build();

connection.on('Log', (logGroupId, message) => {
	consoleLogGroupHelper.appendToGroup(logGroupId, consoleLogGroupHelper.TypeLog, message);
	return true;
});

connection.on('GroupCollapsed', (logGroupId, groupName) => {
	consoleLogGroupHelper.appendToGroup(logGroupId, consoleLogGroupHelper.TypeGroupCollapsed, groupName);
	return true;
});

connection.on('GroupEnd', logGroupId => {
	consoleLogGroupHelper.appendToGroup(logGroupId, consoleLogGroupHelper.TypeGroupEnd);
	return true;
});

connection.on('Error', (logGroupId, message) => {
	consoleLogGroupHelper.appendToGroup(logGroupId, consoleLogGroupHelper.TypeError, message);
});

export default connection;
