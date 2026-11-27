import type { ApiClient } from '@/platform/api';
import { KmicError } from '@/platform/api';
import type { useCurrentUserStore } from './current-user';
import type { Session } from './session';

type CurrentUser = ReturnType<typeof useCurrentUserStore>;

export interface AuthService {
  start(): Promise<void>;
  login(account: string, password: string): Promise<void>;
  logout(): Promise<void>;
  reload(): Promise<void>;
  changePassword(oldPassword: string | null, newPassword: string): Promise<void>;
  switchRole(roleId: string): Promise<void>;
}

export interface AuthServiceOptions {
  api: ApiClient;
  session: Session;
  currentUser: CurrentUser;
}

export function createAuthService({ api, session, currentUser }: AuthServiceOptions): AuthService {
  session.onExpired(() => {
    currentUser.setStatus('anonymous');
  });

  async function reload(): Promise<void> {
    try {
      const { data } = await api.GET('/api/v1/identity/me');
      if (data) {
        currentUser.setMe(data);
      }
    } catch (error) {
      if (!(error instanceof KmicError)) {
        throw error;
      }
      if (error.status === 401) {
        currentUser.setStatus('anonymous');
      } else if (error.code === 'Common.PasswordChangeRequired') {
        currentUser.setStatus('password-change-required');
      } else {
        currentUser.setStatus('failed', error);
      }
    }
  }

  return {
    async start() {
      currentUser.setStatus('starting');
      await reload();
    },

    async login(account, password) {
      const { data } = await api.POST('/api/v1/identity/auth/login', {
        body: { account, password, clientType: 'web' },
      });
      if (!data) {
        return;
      }
      session.setToken(data.access_token);
      session.changeIdentity();
      await reload();
    },

    async logout() {
      try {
        await api.POST('/api/v1/identity/auth/logout');
      } finally {
        // 即使服务端注销请求失败，仍强制清理本地会话
        session.expire();
      }
    },

    reload,

    async changePassword(oldPassword, newPassword) {
      await api.POST('/api/v1/identity/auth/change-password', { body: { oldPassword, newPassword } });
      await reload();
    },

    async switchRole(roleId) {
      const { data } = await api.POST('/api/v1/identity/auth/switch-role', { body: { roleId } });
      if (data) {
        session.setToken(data.access_token);
      }
      session.changeIdentity();
      await reload();
    },
  };
}
