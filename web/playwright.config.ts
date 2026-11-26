import { defineConfig, devices } from '@playwright/test';

// 外壳的端到端回归：用假后端（MSW），不需要真实后端和数据库。
// 用单独的端口，开着的 pnpm dev 不受影响
const port = 5180;
const outDir = 'node_modules/.cache/e2e-dist';

export default defineConfig({
  testDir: 'tests/e2e',
  fullyParallel: true,
  forbidOnly: true,
  retries: 0,
  reporter: [['list']],
  use: {
    baseURL: `http://127.0.0.1:${String(port)}`,
    viewport: { width: 1440, height: 800 },
    trace: 'retain-on-failure',
  },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'], viewport: { width: 1440, height: 800 } } }],
  webServer: {
    // 先按假后端模式构建再预览：开发服务第一次访问时会重新打包依赖并整页刷新，容易让用例偶发失败
    command: [
      `node node_modules/vite/bin/vite.js build --mode mock --outDir ${outDir} --emptyOutDir`,
      `node node_modules/vite/bin/vite.js preview --mode mock --outDir ${outDir} --port ${String(port)} --strictPort`,
    ].join(' && '),
    url: `http://127.0.0.1:${String(port)}`,
    reuseExistingServer: false,
    // 开发服务会把浏览器里的告警转发到这里，用例只看页面本身
    stdout: 'ignore',
    stderr: 'ignore',
    timeout: 120_000,
  },
});
