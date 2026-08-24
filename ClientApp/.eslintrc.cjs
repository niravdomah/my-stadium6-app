/* eslint-env node */
require('@rushstack/eslint-patch/modern-module-resolution');

module.exports = {
	root: true,
	extends: ['plugin:vue/vue3-essential', 'eslint:recommended', '@vue/eslint-config-prettier'],
	env: {
		'vue/setup-compiler-macros': true
	},
	parserOptions: {
		ecmaVersion: 13
	},
	rules: {
		'no-console': process.env.NODE_ENV === 'production' ? 'warn' : 'off',
		'no-debugger': process.env.NODE_ENV === 'production' ? 'warn' : 'off',
		'no-unused-vars': ['warn', { argsIgnorePattern: '^_', ignoreRestSiblings: true }],
		'vue/multi-word-component-names': 'off'
	}
};
