const TypeGroup = 'Group';
const TypeGroupCollapsed = 'GroupCollapsed';
const TypeGroupEnd = 'GroupEnd';
const TypeError = 'Error';
const TypeLog = 'Log';

const logGroups = {};
const closedGroupIds = [];

function generateGroupId() {
	return Math.floor(Math.random() * Date.now());
}

function appendToGroup(groupId, logType, value = null) {
	if (!(groupId in logGroups)) logGroups[groupId] = [];

	logGroups[groupId].push({
		logType: logType,
		value: value,
		log: function () {
			if (this.logType == TypeGroup) console.group(this.value);
			else if (this.logType == TypeGroupCollapsed) console.groupCollapsed(this.value);
			else if (this.logType == TypeGroupEnd) console.groupEnd();
			else if (this.logType == TypeError) console.error(this.value);
			else console.log(this.value);
		}
	});
}

function appendGroupsToMainGroup(mainGroupId, groupIds) {
	for (let groupIndex = 0; groupIndex < groupIds.length; groupIndex++) {
		let logsToMove = logGroups[groupIds[groupIndex]];
		if (logsToMove !== undefined)
			for (let logIndex = 0; logIndex < logsToMove.length; logIndex++) logGroups[mainGroupId].push(logsToMove[logIndex]);

		delete logGroups[groupIds[groupIndex]];
	}
}

function closeGroup(groupId) {
	if (logGroups[groupId] === undefined) return;

	closedGroupIds.push(groupId);
}

function pollClosedGroups() {
	setTimeout(() => {
		try {
			if (closedGroupIds.length == 0) return;

			let groupIdCount = closedGroupIds.length;

			for (let closedGroupIndex = 0; closedGroupIndex < groupIdCount; closedGroupIndex++) {
				let groupId = closedGroupIds[closedGroupIndex];

				for (let logIndex = 0; logIndex < logGroups[groupId].length; logIndex++) logGroups[groupId][logIndex].log();

				delete logGroups[groupId];
			}

			closedGroupIds.splice(0, groupIdCount);
		} finally {
			pollClosedGroups();
		}
	}, 500);
}

export default {
	TypeGroup,
	TypeGroupCollapsed,
	TypeGroupEnd,
	TypeError,
	TypeLog,
	generateGroupId,
	appendToGroup,
	appendGroupsToMainGroup,
	closeGroup,
	pollClosedGroups
};
