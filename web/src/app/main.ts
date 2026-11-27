import { VueQueryPlugin } from '@tanstack/vue-query';
import { createPinia } from 'pinia';
import { createApp } from 'vue';
import { apiKey, createApiClient, createKmicQueryClient, sessionKey } from '@/platform/api';
import { createRouteCatalog, catalogKey, navigateKey, type RouteEntry } from '@/platform/routing';
import { createSession } from '@/platform/session/session';
import { createAuthService } from '@/platform/session/auth-service';
import { useCurrentUserStore } from '@/platform/session/current-user';
import { authKey } from '@/platform/session/use-auth';
import { createNavigationState, navigationKey } from '@/shell/navigation-state';
import { homeRouteName, shellRoutes } from '@/shell/routes';
import { useShellSettingsStore } from '@/shell/settings/shell-settings-store';
import { workspaceKey } from '@/shell/workspace/use-workspace';
import { createWorkspace } from '@/shell/workspace/workspace';
import App from './App.vue';
import { menuGroups } from './menu-groups';
import { createAppRouter, installNavigationGuards } from './router';
import { confirmLeave, confirmReplace } from './workspace-dialogs';
import './styles/global.scss';

// pnpm dev 使用本地 Mock 服务，pnpm dev:api 连接真实后端
const mock = import.meta.env.VITE_API_MODE === 'mock' ? await import('./dev') : null;
await mock?.startMock();

const entries: readonly RouteEntry[] = [...shellRoutes, ...(mock?.demoRoutes ?? [])];
const catalog = createRouteCatalog(entries, [...menuGroups, ...(mock?.demoGroups ?? [])]);

const session = createSession();
const api = createApiClient({ session });
const queryClient = createKmicQueryClient({ api, session });

const pinia = createPinia();
const app = createApp(App);
app.use(pinia);
app.use(VueQueryPlugin, { queryClient });
app.provide(sessionKey, session);
app.provide(apiKey, api);

const currentUser = useCurrentUserStore(pinia);
const auth = createAuthService({ api, session, currentUser });
const shellSettings = useShellSettingsStore(pinia);
const navigation = createNavigationState();

const router = createAppRouter(catalog);

const workspace = createWorkspace({
  router,
  catalog,
  homeRouteName,
  epoch: () => session.epoch,
  maxTabs: () => shellSettings.settings.tabs.max,
  confirmLeave,
  confirmReplace,
  storage: sessionStorage,
  userId: () => currentUser.me?.userId ?? null,
});

installNavigationGuards(router, { workspace, navigation, currentUser, shellSettings, loginRestored: auth.start() });

app.provide(authKey, auth);
app.provide(catalogKey, catalog);
app.provide(navigationKey, navigation);
app.provide(workspaceKey, workspace);
app.provide(navigateKey, (name, params) => workspace.open(name, params));
app.use(router);
app.mount('#app');

// 启动动画淡出后移除；后台标签页可能不触发 transitionend，超时后强制移除
const loading = document.getElementById('app-loading');
if (loading !== null) {
  loading.classList.add('is-done');
  loading.addEventListener('transitionend', () => {
    loading.remove();
  });
  setTimeout(() => {
    loading.remove();
  }, 1000);
}
