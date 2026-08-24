function getChild(element, selector) {
	return [...element.children].find(c => c.matches(selector));
}

function getHiddenElementWidth(element) {
	let backupProperties = {};
	backupProperties.position = element.style.position;
	backupProperties.visibility = element.style.visibility;
	backupProperties.display = element.style.display;

	let width;
	try {
		element.style.position = 'absolute';
		element.style.visibility = 'hidden';
		element.style.display = 'block';

		width = element.offsetWidth;
	} finally {
		element.style.position = backupProperties.position;
		element.style.visibility = backupProperties.visibility;
		element.style.display = backupProperties.display;
	}

	return width;
}

function isVisible(element) {
	return !!(element.offsetWidth || element.offsetHeight || element.getClientRects().length);
}

function scrollTo(element) {
	return element.scrollIntoView({ behavior: 'smooth' });
}

export default {
	getChild,
	getHiddenElementWidth,
	isVisible,
	scrollTo
};
