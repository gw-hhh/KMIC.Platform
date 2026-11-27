import { inject, type InjectionKey } from 'vue';
import type { RouteParams } from './types';

export type Navigate = (name: string, params?: RouteParams) => Promise<void>;

export const navigateKey: InjectionKey<Navigate> = Symbol('kmic.navigate');

export function useNavigate(): Navigate {
  const navigate = inject(navigateKey, null);
  if (navigate === null) {
    throw new Error('navigateKey 未注入，请在 main.ts 中通过 app.provide 注册');
  }
  return navigate;
}
