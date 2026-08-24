Array.prototype.mapAsync = function (asyncCallbackFn) {
	return Promise.all(this.map(i => asyncCallbackFn(i)));
};
