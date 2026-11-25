import { inject, provide, type InjectionKey } from 'vue';
import type { PageInstance } from './page-instance';

const pageKey: InjectionKey<PageInstance> = Symbol('kmic.page');

export function providePage(page: PageInstance): void {
  provide(pageKey, page);
}

/** 不在页面里（如外壳的组件）时为 null */
export function injectPage(): PageInstance | null {
  return inject(pageKey, null);
}

export function usePage(): PageInstance {
  const page = inject(pageKey, null);
  if (page === null) {
    throw new Error('usePage 要在页面组件中调用');
  }
  return page;
}
