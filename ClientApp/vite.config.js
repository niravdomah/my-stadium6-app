/* eslint-disable no-undef */
import { fileURLToPath, URL } from 'url';

import { defineConfig, loadEnv } from 'vite';
import vue from '@vitejs/plugin-vue';

export default defineConfig(({ mode }) => {
	process.env = { ...process.env, ...loadEnv(mode, process.cwd()) };

	const webAppName = 'MyStadium6App';

	return {
		base: '/',
		plugins: [vue()],
		define: {
			'process.platform': `'${process.platform}'`
		},
		resolve: {
			alias: {
				'@': fileURLToPath(new URL('./src', import.meta.url))
			}
		},
		build: {
			outDir: process.env.VITE_APP_DEBUG === 'true' ? 'dist/debug' : 'dist/production',
			target: 'esnext'
		},
		css: {
			postcss: {
				plugins: [
					{
						postcssPlugin: 'internal:charset-removal',
						AtRule: {
							charset: atRule => {
								if (atRule.name === 'charset') {
									atRule.remove();
								}
							}
						}
					}
				]
			}
		}
	};
});
