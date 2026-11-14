import { fileURLToPath, URL } from 'node:url';
import vue from '@vitejs/plugin-vue';
import { defineConfig } from 'vitest/config';

// 前端和 API 同域（ALB 按路径转发），开发时把 /api 转到本机的 Api
export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: { '@': fileURLToPath(new URL('./src', import.meta.url)) },
    // 测试里依赖预构建后可能出现两份 Vue，组件注册会失败
    dedupe: ['vue'],
  },
  server: {
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
