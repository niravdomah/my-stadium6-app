import { defineRule, configure } from 'vee-validate';
import { required } from '@vee-validate/rules';

defineRule('required', required);
defineRule('email', value => {
	if (!value || !value.length) return true;

	const validEmailPattern = /^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]{1,64}@[a-zA-Z0-9-]{1,64}\.[.a-zA-Z0-9-]{2,64}$/;
	return validEmailPattern.test(value);
});

configure({
	validateOnInput: true,
	generateMessage: context => {
		if (context.rule.name === 'required') return `The ${context.field} field is required.`;

		if (context.rule.name === 'email') return 'Please enter a valid email address.';

		return `The field ${context.field} is invalid`;
	}
});
