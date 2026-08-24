function getAntiforgeryToken() {
	return {
		'X-CSRF-TOKEN': readCookie('CSRF-TOKEN')
	};
}

function readCookie(name) {
	let match = document.cookie.match(`(?:^|; *)${name}=([^;]*)(?:;|$)`);

	return match ? match[1] : null;
}

export default { getAntiforgeryToken };
