import { inject, type InjectionKey } from 'vue';
import type { AuthService } from './auth-service';

export const authKey: InjectionKey<AuthService> = Symbol('kmic.auth');

export function useAuth(): AuthService {
  const auth = inject(authKey, null);
  if (auth === null) {
    throw new Error('authKey 未注入，请在 main.ts 中通过 app.provide 注册');
  }
  return auth;
}
