import { VueQueryPlugin } from '@tanstack/vue-query';
import { createPinia } from 'pinia';
import { createApp } from 'vue';
import { installTable } from '@/platform/table/install';
import '@/app/styles.scss';

if (import.meta.env.VITE_ZERO_RUNTIME === '1') {
  await import('antdv-next/dist/antd.css');
}
import App from './App.vue';

const app = createApp(App);
app.use(createPinia());
app.use(VueQueryPlugin);
installTable(app);
app.mount('#app');
