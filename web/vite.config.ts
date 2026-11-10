import { fileURLToPath, URL } from 'node:url';
import vue from '@vitejs/plugin-vue';
import { defineConfig } from 'vite';

const csp = process.env.KMIC_CSP;

export default defineConfig({
  plugins: [vue()],
  preview: {
    host: '127.0.0.1',
    port: 4173,
    strictPort: true,
    headers: csp ? { 'Content-Security-Policy': csp } : {},
  },
  resolve: {
    alias: { '@': fileURLToPath(new URL('./src', import.meta.url)) },
  },
  build: {
    rolldownOptions: {
      output: {
        codeSplitting: {
          groups: [
            { name: 'antdv', test: /node_modules[\/]antdv-next/ },
            { name: 'vxe', test: /node_modules[\/](vxe-table|@vxe-ui|xe-utils)/ },
          ],
        },
      },
    },
  },
});
