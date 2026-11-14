import { createPinia } from 'pinia';
import { createApp } from 'vue';
import { createApiClient } from '@/platform/api';
import { createSession } from '@/platform/session/session';
import App from './App.vue';
import { createAppRouter } from './router';

// 会话和请求客户端在这里创建一次，之后经依赖注入交给外壳与页面
const session = createSession();
const api = createApiClient({ session });

const app = createApp(App);
app.use(createPinia());
app.use(createAppRouter());
app.provide('kmic.session', session);
app.provide('kmic.api', api);
app.mount('#app');
