import { inject, type InjectionKey } from 'vue';
import type { RouteCatalog } from './catalog';

export const catalogKey: InjectionKey<RouteCatalog> = Symbol('kmic.catalog');

export function useCatalog(): RouteCatalog {
  const catalog = inject(catalogKey, null);
  if (catalog === null) {
    throw new Error('catalogKey 未注册，请在 main.ts 里 provide');
  }
  return catalog;
}
