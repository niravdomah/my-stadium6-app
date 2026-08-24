import PageNotFoundError from '@/views/errors/PageNotFoundError.vue';
import ForbiddenError from '@/views/errors/ForbiddenError.vue';
import UnauthenticatedError from '@/views/errors/UnauthenticatedError.vue';

import Authentication from '@/views/authentication/Authentication.vue';
import Login from '@/views/authentication/Login.vue';
import ResetPassword from '@/views/authentication/ResetPassword.vue';
import ResetPasswordEmailConfirmation from '@/views/authentication/ResetPasswordEmailConfirmation.vue';
import UpdatePassword from '@/views/authentication/UpdatePassword.vue';

import Callback from '@/views/authentication/Callback.vue';
import Logout from '@/views/authentication/Logout.vue';

import Administration from '@/views/administration/Administration.vue';
import Users from '@/views/administration/users/Users.vue';
import AddUser from '@/views/administration/users/AddUser.vue';
import EditUser from '@/views/administration/users/EditUser.vue';
import Pages from '@/views/administration/pages/Pages.vue';
import EditPage from '@/views/administration/pages/EditPage.vue';
import Roles from '@/views/administration/roles/Roles.vue';
import AddRole from '@/views/administration/roles/AddRole.vue';
import EditRole from '@/views/administration/roles/EditRole.vue';

import pageRoutes from '@/router/page-routes.js';

const routes = [
	{
		path: '/Authentication',
		redirect: '/Authentication/Login',
		component: Authentication,
		meta: { allowAnonymous: true },
		children: [
			{
				path: 'Login',
				name: 'Login',
				component: Login,
				meta: { allowAnonymous: true }
			},
			{
				path: 'Login/ResetPassword',
				name: 'ResetPassword',
				component: ResetPassword,
				meta: { allowAnonymous: true }
			},
			{
				path: 'Login/ResetPasswordEmailConfirmation',
				name: 'ResetPasswordEmailConfirmation',
				component: ResetPasswordEmailConfirmation,
				meta: { allowAnonymous: true }
			},
			{
				path: 'Login/UpdatePassword',
				name: 'UpdatePassword',
				component: UpdatePassword,
				meta: { allowAnonymous: true }
			}
		]
	},
	{
		path: '/Admin',
		redirect: '/Admin/Users',
		component: Administration,
		meta: { requiresAdministrator: true },
		children: [
			{
				path: 'Users',
				name: 'Users',
				component: Users
			},
			{
				path: 'Users/AddUser',
				name: 'AddUser',
				component: AddUser
			},
			{
				path: 'Users/EditUser',
				name: 'EditUser',
				component: EditUser
			},
			{
				path: 'Pages',
				name: 'Pages',
				component: Pages
			},
			{
				path: 'Pages/EditPage',
				name: 'EditPage',
				component: EditPage
			},
			{
				path: 'Roles',
				name: 'Roles',
				component: Roles
			},
			{
				path: 'Roles/AddRole',
				name: 'AddRole',
				component: AddRole
			},
			{
				path: 'Roles/EditRole',
				name: 'EditRole',
				component: EditRole
			}
		]
	},
	{
		path: '/callback',
		component: Callback,
		meta: { allowAnonymous: true }
	},
	{
		path: '/logout',
		component: Logout,
		meta: { allowAnonymous: true }
	},
	...pageRoutes,
	{
		path: '/Error/Forbidden',
		name: 'Forbidden',
		component: ForbiddenError,
		meta: { allowAnonymous: true }
	},
	{
		path: '/:pathMatch(.*)*',
		name: 'PageNotFound',
		component: PageNotFoundError,
		meta: { allowAnonymous: true }
	},
	{
		path: '/Error/Unauthenticated',
		name: 'Unauthenticated',
		component: UnauthenticatedError,
		meta: { allowAnonymous: true }
	}
];

export default routes;
