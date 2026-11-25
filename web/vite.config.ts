import { rm } from 'node:fs/promises';
import { fileURLToPath, URL } from 'node:url';
import vue from '@vitejs/plugin-vue';
import { defineConfig } from 'vitest/config';

// 前端和 API 同域（ALB 按路径转发），开发时把 /api 转到本机的 Api
export default defineConfig({
  plugins: [
    vue(),
    {
      // 假后端的 Service Worker 只在开发时用，不进发布包
      name: 'kmic-drop-mock-worker',
      apply: 'build',
      closeBundle: async () => {
        await rm(fileURLToPath(new URL('./dist/mockServiceWorker.js', import.meta.url)), { force: true });
      },
    },
  ],
  resolve: {
    alias: { '@': fileURLToPath(new URL('./src', import.meta.url)) },
    // 测试里依赖预构建后可能出现两份 Vue，组件注册会失败
    dedupe: ['vue'],
  },
  server: {
    // 默认只听 IPv6 的 ::1，localhost 解析成 127.0.0.1 时会打不开
    host: '127.0.0.1',
    port: 5173,
    proxy: {
      '/api': { target: 'http://localhost:5080', changeOrigin: false },
    },
  },
  build: {
    rolldownOptions: {
      output: {
        codeSplitting: {
          groups: [{ name: 'vue', test: /node_modules[\\/](vue|vue-router|pinia|@vue)[\\/]/ }],
        },
      },
    },
  },
  test: {
    environment: 'node',
    include: ['src/**/*.test.ts', 'tests/**/*.test.ts'],
  },
});
