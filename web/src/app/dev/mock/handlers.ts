import { http, HttpResponse } from 'msw';
import type { PermissionCode } from '@/generated/permissions';
import { inboxHandlers } from './inbox';

// 本地 Mock 接口，会话状态持久化于 sessionStorage
// 测试账号：admin（双角色）、sales、viewer、newbie（需首次修改密码）、norole（当前角色失效）；密码任意，输入 wrong 时返回登录失败

interface Role {
  id: string;
  name: string;
}

const roles: Record<string, Role & { permissions: PermissionCode[] }> = {
  admin: {
    id: 'r-admin',
    name: '系统管理员',
    permissions: ['Sample.Specimen.View', 'Sample.Specimen.Create', 'Operations.Audit.View', 'Identity.User.View'],
  },
  sales: { id: 'r-sales', name: '销售经理', permissions: ['Sample.Specimen.View', 'Sample.Specimen.Create'] },
  viewer: { id: 'r-viewer', name: '只读用户', permissions: ['Sample.Specimen.View'] },
};

const accounts: Record<string, { name: string; roleKeys: string[] }> = {
  admin: { name: '陈管理', roleKeys: ['admin', 'sales'] },
  sales: { name: '李销售', roleKeys: ['sales', 'viewer'] },
  viewer: { name: '王只读', roleKeys: ['viewer'] },
  newbie: { name: '赵新人', roleKeys: ['viewer'] },
  norole: { name: '孙无角色', roleKeys: ['sales'] },
};

interface MockState {
  account: string;
  roleId: string;
  mustChangePassword: boolean;
  hasPassword: boolean;
}

const stateKey = 'kmic.mock.state';
const preferenceKey = 'kmic.mock.preference';

function readState(): MockState | null {
  const text = sessionStorage.getItem(stateKey);
  return text === null ? null : (JSON.parse(text) as MockState);
}

function writeState(state: MockState | null): void {
  if (state === null) {
    sessionStorage.removeItem(stateKey);
  } else {
    sessionStorage.setItem(stateKey, JSON.stringify(state));
  }
}

function problem(status: number, code: string, title: string): Response {
  return HttpResponse.json(
    { type: `urn:kmic:error:${code}`, title, status, code, traceId: 'mock-trace', retryable: false },
    { status, headers: { 'Content-Type': 'application/problem+json', 'X-Trace-Id': 'mock-trace' } },
  );
}

function tokens(account: string, mustChangePassword: boolean): Response {
  return HttpResponse.json({
    access_token: `mock.${account}`,
    token_type: 'Bearer',
    expires_in: 900,
    refresh_token: null,
    passwordChangeRequired: mustChangePassword,
  });
}

// 令牌与当前账号不符时按未登录处理
function authorize(request: Request): MockState | null {
  const state = readState();
  return state !== null && request.headers.get('Authorization') === `Bearer mock.${state.account}` ? state : null;
}

export const handlers = [
  http.post('/api/v1/identity/auth/login', async ({ request }) => {
    const body = (await request.json()) as { account: string; password: string };
    const account = accounts[body.account];
    if (account === undefined || body.password === 'wrong') {
      return problem(401, 'Identity.InvalidCredentials', '账号或密码错误');
    }
    const role = roles[account.roleKeys[0] ?? 'viewer'];
    const state: MockState = {
      account: body.account,
      // 模拟当前角色已从可用角色列表中移除
      roleId: body.account === 'norole' ? 'r-gone' : (role?.id ?? 'r-viewer'),
      mustChangePassword: body.account === 'newbie',
      hasPassword: true,
    };
    writeState(state);
    return tokens(body.account, state.mustChangePassword);
  }),

  http.post('/api/v1/identity/auth/refresh', () => {
    const state = readState();
    return state === null
      ? problem(401, 'Common.Unauthenticated', '请先登录')
      : tokens(state.account, state.mustChangePassword);
  }),

  http.post('/api/v1/identity/auth/logout', () => {
    writeState(null);
    return new HttpResponse(null, { status: 204 });
  }),

  http.get('/api/v1/identity/me', ({ request }) => {
    const state = authorize(request);
    if (state === null) {
      return problem(401, 'Common.Unauthenticated', '请先登录');
    }
    if (state.mustChangePassword) {
      return problem(403, 'Common.PasswordChangeRequired', '请先修改密码');
    }
    const account = accounts[state.account];
    if (account === undefined) {
      return problem(401, 'Common.Unauthenticated', '请先登录');
    }
    const available = account.roleKeys.flatMap((key) =>
      roles[key] ? [{ id: roles[key].id, name: roles[key].name }] : [],
    );
    const current = Object.values(roles).find((role) => role.id === state.roleId);
    return HttpResponse.json({
      userId: `u-${state.account}`,
      account: state.account,
      displayName: account.name,
      englishName: null,
      orgUnitId: null,
      currentRole: { id: state.roleId, name: current?.name ?? '' },
      roles: available,
      permissions: current?.permissions ?? [],
      authorizationVersion: 1,
      passwordChangeRequired: false,
      hasPassword: state.hasPassword,
      impersonating: false,
      impersonatorUserId: null,
      impersonationAllowsWrite: false,
      businessTimeZone: 'Asia/Shanghai',
    });
  }),

  http.post('/api/v1/identity/auth/switch-role', async ({ request }) => {
    const state = authorize(request) ?? readState();
    const body = (await request.json()) as { roleId: string };
    if (state === null) {
      return problem(401, 'Common.Unauthenticated', '请先登录');
    }
    writeState({ ...state, roleId: body.roleId });
    return tokens(state.account, false);
  }),

  http.post('/api/v1/identity/auth/change-password', () => {
    const state = readState();
    if (state !== null) {
      writeState({ ...state, mustChangePassword: false });
    }
    return new HttpResponse(null, { status: 204 });
  }),

  http.get('/api/v1/configuration/preferences/me', () => {
    const saved = sessionStorage.getItem(preferenceKey);
    return HttpResponse.json({
      preferences:
        saved === null
          ? []
          : [
              {
                key: 'Ui.Shell',
                displayName: '界面外壳设置',
                isFamily: true,
                value: JSON.parse(saved) as unknown,
                source: 'UserRole',
                canSet: true,
              },
            ],
    });
  }),

  http.put('/api/v1/configuration/preferences/me/:key', async ({ request }) => {
    const body = (await request.json()) as { value: unknown };
    sessionStorage.setItem(preferenceKey, JSON.stringify(body.value));
    return new HttpResponse(null, { status: 204 });
  }),

  http.post('/api/v1/configuration/preferences/me/:key/reset', () => {
    sessionStorage.removeItem(preferenceKey);
    return new HttpResponse(null, { status: 204 });
  }),

  ...inboxHandlers,
];
