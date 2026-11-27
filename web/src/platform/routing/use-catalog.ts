import { inject, type InjectionKey } from 'vue';
import type { RouteCatalog } from './catalog';

export const catalogKey: InjectionKey<RouteCatalog> = Symbol('kmic.catalog');

export function useCatalog(): RouteCatalog {
  const catalog = inject(catalogKey, null);
  if (catalog === null) {
    throw new Error('catalogKey 未注入，请在 main.ts 中通过 app.provide 注册');
  }
  return catalog;
}
