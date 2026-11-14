import { createRouter, createWebHistory } from 'vue-router';

// 路由目录要等外壳与第一个业务模块有了再建；现在只有首页
export function createAppRouter() {
  return createRouter({
    history: createWebHistory(),
    routes: [{ path: '/', name: 'home', component: () => import('@/shell/pages/HomePage.vue') }],
  });
}
