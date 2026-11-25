import { defineModuleRoutes } from '@/platform/routing';

export const homeRouteName = 'shell.home';

export const shellRoutes = defineModuleRoutes('shell', [
  {
    name: homeRouteName,
    path: '/',
    title: '工作台',
    icon: 'home',
    requires: [],
    tab: { mode: 'singleton' },
    component: () => import('./pages/HomePage.vue'),
  },
  {
    name: 'shell.change-password',
    path: '/account/password',
    title: '修改密码',
    icon: 'key',
    requires: [],
    tab: { mode: 'singleton' },
    component: () => import('./pages/ChangePasswordPage.vue'),
  },
]);
