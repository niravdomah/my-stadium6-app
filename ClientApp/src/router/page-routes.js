import StartPage from '@/views/StartPage.vue';

const pageRoutes = [
	{
	path: '/',
	redirect: '/StartPage'
},
{
	path: '/StartPage',
	component: StartPage,
	meta: { title: 'Start Page' }
}
];

export default pageRoutes;
