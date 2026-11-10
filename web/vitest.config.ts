import { playwright } from '@vitest/browser-playwright';
import { defineConfig, mergeConfig } from 'vitest/config';
import viteConfig from './vite.config.ts';

export default mergeConfig(
  viteConfig,
  defineConfig({
    resolve: { dedupe: ['vue'] },
    test: {
      include: ['tests/**/*.test.ts'],
      api: { host: '127.0.0.1', port: 51244, strictPort: true },
      browser: {
        enabled: true,
        provider: playwright(),
        headless: true,
        instances: [{ browser: 'chromium' }],
        viewport: { width: 1280, height: 800 },
      },
    },
  }),
);
