import { inject, type InjectionKey } from 'vue';
import type { Session } from '@/platform/session/session';
import type { ApiClient } from './client';

export const apiKey: InjectionKey<ApiClient> = Symbol('kmic.api');
export const sessionKey: InjectionKey<Session> = Symbol('kmic.session');

export function useApi(): ApiClient {
  const api = inject(apiKey, null);
  if (api === null) {
    throw new Error('apiKey 未注册，请在 main.ts 里 provide');
  }
  return api;
}

export function useSession(): Session {
  const session = inject(sessionKey, null);
  if (session === null) {
    throw new Error('sessionKey 未注册，请在 main.ts 里 provide');
  }
  return session;
}
