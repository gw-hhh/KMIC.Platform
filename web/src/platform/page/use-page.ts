import { inject, provide, type InjectionKey } from 'vue';
import type { PageInstance } from './page-instance';

const pageKey: InjectionKey<PageInstance> = Symbol('kmic.page');

export function providePage(page: PageInstance): void {
  provide(pageKey, page);
}

/** 注入当前页面实例；位于非页面环境（如外壳组件）时返回 null */
export function injectPage(): PageInstance | null {
  return inject(pageKey, null);
}

export function usePage(): PageInstance {
  const page = inject(pageKey, null);
  if (page === null) {
    throw new Error('usePage 必须在页面组件内部调用');
  }
  return page;
}
