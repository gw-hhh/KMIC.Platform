import { createRouter, createWebHistory, type Router } from 'vue-router';
import type { RouteCatalog } from '@/platform/routing';
import type { useCurrentUserStore } from '@/platform/session/current-user';
import type { NavigationState } from '@/shell/navigation-state';
import type { useShellSettingsStore } from '@/shell/settings/shell-settings-store';
import type { Workspace } from '@/shell/workspace/workspace';

export interface RouterDeps {
  workspace: Workspace;
  navigation: NavigationState;
  currentUser: ReturnType<typeof useCurrentUserStore>;
  shellSettings: ReturnType<typeof useShellSettingsStore>;
  loginRestored: Promise<void>;
}

const stub = { render: () => null };

export function createAppRouter(catalog: RouteCatalog): Router {
  return createRouter({
    history: createWebHistory(),
    routes: [
      {
        path: '/login',
        name: 'auth.login',
        component: () => import('@/shell/pages/LoginPage.vue'),
        meta: { layout: 'blank' },
      },
      {
        path: '/auth/change-password',
        name: 'auth.change-password',
        component: () => import('@/shell/pages/StandalonePasswordPage.vue'),
        meta: { layout: 'blank' },
      },
      {
        path: '/auth/select-role',
        name: 'auth.select-role',
        component: () => import('@/shell/pages/RoleSelectPage.vue'),
        meta: { layout: 'blank' },
      },
      {
        path: '/auth/startup-error',
        name: 'auth.startup-error',
        component: () => import('@/shell/pages/StartupErrorPage.vue'),
        meta: { layout: 'blank' },
      },
      ...catalog.entries.map((entry) => ({ path: entry.path, name: entry.name, component: stub })),
      { path: '/:pathMatch(.*)*', name: 'not-found', component: () => import('@/shell/pages/NotFoundPage.vue') },
    ],
  });
}

export function installNavigationGuards(router: Router, deps: RouterDeps): void {
  const { workspace, navigation, currentUser, shellSettings } = deps;
  let shellReadyFor: string | null = null;

  async function prepareShell(userId: string): Promise<void> {
    if (shellReadyFor === userId) {
      return;
    }
    await shellSettings.load();
    workspace.reset();
    workspace.restore();
    workspace.ensureHome();
    shellReadyFor = userId;
  }

  router.beforeEach(async (to) => {
    navigation.start();
    await deps.loginRestored;

    // 没进入系统时只能停在对应的页面
    const forced: Partial<Record<typeof currentUser.status, string>> = {
      anonymous: 'auth.login',
      'password-change-required': 'auth.change-password',
      'role-unavailable': 'auth.select-role',
      failed: 'auth.startup-error',
    };
    const target = forced[currentUser.status];
    if (target !== undefined) {
      // 退出或进不了系统后，下次进入外壳要重新加载偏好、重建工作区，即使还是同一个账号
      shellReadyFor = null;
      if (to.name === target) {
        return true;
      }
      const redirect = to.fullPath === '/' || to.meta['layout'] === 'blank' ? {} : { redirect: to.fullPath };
      return { name: target, query: currentUser.status === 'anonymous' ? redirect : {} };
    }

    if (to.meta['layout'] === 'blank') {
      return '/';
    }
    if (currentUser.me) {
      await prepareShell(currentUser.me.userId);
    }
    return workspace.beforeNavigate(to);
  });

  router.afterEach((to, _from, failure) => {
    navigation.finish();
    if (!failure && currentUser.status === 'ready' && to.meta['layout'] !== 'blank') {
      workspace.commit(to);
    }
  });

  router.onError(() => {
    navigation.finish();
  });
}
