import { http, HttpResponse } from 'msw';
import { setupServer } from 'msw/node';
import { afterAll, afterEach, beforeAll, describe, expect, it } from 'vitest';
import { createSession } from '@/platform/session/session';
import { createApiClient } from './client';
import { KmicError } from './kmicError';

const origin = 'http://kmic.test';
const server = setupServer();

beforeAll(() => {
  server.listen({ onUnhandledFrame: 'error' });
});
afterEach(() => {
  server.resetHandlers();
});
afterAll(() => {
  server.close();
});

function problem(status: number, code: string, extra: Record<string, unknown> = {}) {
  return HttpResponse.json(
    { type: `urn:kmic:error:${code}`, title: '提示文字', status, code, traceId: 'trace-1', retryable: false, ...extra },
    { status, headers: { 'Content-Type': 'application/problem+json', 'X-Trace-Id': 'trace-1' } },
  );
}

function newClient(overrides: { locks?: null; timeoutMs?: number } = {}) {
  const session = createSession();
  session.setToken('token-1');
  let id = 0;
  const client = createApiClient({
    session,
    origin,
    baseUrl: origin,
    locks: null,
    newId: () => `key-${String(++id)}`,
    ...overrides,
  });
  return { client, session };
}

const refreshed = (token: string) =>
  HttpResponse.json({
    access_token: token,
    token_type: 'Bearer',
    expires_in: 900,
    refresh_token: null,
    passwordChangeRequired: false,
  });

describe('读请求', () => {
  it('带访问令牌，成功时返回数据', async () => {
    let authorization: string | null = null;
    server.use(
      http.get(`${origin}/api/v1/identity/me`, ({ request }) => {
        authorization = request.headers.get('Authorization');
        return HttpResponse.json({ displayName: '甲' });
      }),
    );
    const { client } = newClient();

    const { data } = await client.GET('/api/v1/identity/me');

    expect(authorization).toBe('Bearer token-1');
    expect(data).toEqual({ displayName: '甲' });
  });

  it('读请求不带幂等键', async () => {
    let key: string | null = 'unset';
    server.use(
      http.get(
        `${origin}/api/v1/identity/me`,
        ({ request }) => ((key = request.headers.get('Idempotency-Key')), HttpResponse.json({})),
      ),
    );
    const { client } = newClient();

    await client.GET('/api/v1/identity/me');

    expect(key).toBeNull();
  });
});

describe('错误转换', () => {
  it('服务端的问题转成 KmicError，保留错误码、字段错误、版本和追踪号', async () => {
    server.use(
      http.post(`${origin}/api/v1/identity/auth/change-password`, () =>
        problem(400, 'Common.ValidationFailed', {
          errors: [{ path: 'newPassword', code: 'Required', message: '必填' }],
          currentVersion: 7,
          retryable: true,
        }),
      ),
    );
    const { client } = newClient();

    const failure = await client
      .POST('/api/v1/identity/auth/change-password', { body: { newPassword: '' } as never })
      .catch((e: unknown) => e);

    expect(failure).toBeInstanceOf(KmicError);
    const error = failure as KmicError;
    expect([error.kind, error.status, error.code, error.traceId, error.retryable, error.currentVersion]).toEqual([
      'problem',
      400,
      'Common.ValidationFailed',
      'trace-1',
      true,
      7,
    ]);
    expect(error.errors).toEqual([{ path: 'newPassword', code: 'Required', message: '必填' }]);
    expect(error.outcome).toBe('failed');
  });

  it('429 带 Retry-After', async () => {
    server.use(
      http.get(`${origin}/api/v1/identity/me`, () =>
        HttpResponse.json(
          {
            type: 'x',
            title: '请求太频繁',
            status: 429,
            code: 'Common.TooManyRequests',
            traceId: 't',
            retryable: true,
          },
          { status: 429, headers: { 'Content-Type': 'application/problem+json', 'Retry-After': '12' } },
        ),
      ),
    );
    const { client } = newClient();

    const error = (await client.GET('/api/v1/identity/me').catch((e: unknown) => e)) as KmicError;

    expect([error.status, error.retryAfterSeconds]).toEqual([429, 12]);
  });

  it('不是 ProblemDetails 的响应按状态码归类', async () => {
    server.use(
      http.get(
        `${origin}/api/v1/identity/me`,
        () =>
          new HttpResponse('<html>Bad Gateway</html>', {
            status: 502,
            headers: { 'Content-Type': 'text/html', 'X-Trace-Id': 'gw-1' },
          }),
      ),
    );
    const { client } = newClient();

    const error = (await client.GET('/api/v1/identity/me').catch((e: unknown) => e)) as KmicError;

    expect([error.kind, error.status, error.code, error.traceId]).toEqual(['problem', 502, undefined, 'gw-1']);
  });

  it('断网：读请求没有结局，写请求是结果未知', async () => {
    server.use(
      http.get(`${origin}/api/v1/identity/me`, () => HttpResponse.error()),
      http.post(`${origin}/api/v1/identity/auth/logout`, () => HttpResponse.error()),
    );
    const { client } = newClient();

    const read = (await client.GET('/api/v1/identity/me').catch((e: unknown) => e)) as KmicError;
    const write = (await client.POST('/api/v1/identity/auth/logout').catch((e: unknown) => e)) as KmicError;

    expect([read.kind, read.outcome]).toEqual(['network', undefined]);
    expect([write.kind, write.outcome]).toEqual(['network', 'unknown']);
  });

  it('写请求遇到 500 或 503“结果未知”算结果未知，其他情况是确定没有生效', async () => {
    server.use(
      http.post(`${origin}/api/v1/identity/auth/logout`, () => problem(500, 'Common.Unexpected')),
      http.post(`${origin}/api/v1/identity/auth/switch-role`, () => problem(503, 'Common.ResultUnknown')),
      http.post(`${origin}/api/v1/identity/auth/change-password`, () => problem(503, 'Common.MessagingSuspended')),
    );
    const { client } = newClient();

    const a = (await client.POST('/api/v1/identity/auth/logout').catch((e: unknown) => e)) as KmicError;
    const b = (await client
      .POST('/api/v1/identity/auth/switch-role', { body: {} as never })
      .catch((e: unknown) => e)) as KmicError;
    const c = (await client
      .POST('/api/v1/identity/auth/change-password', { body: {} as never })
      .catch((e: unknown) => e)) as KmicError;

    expect([a.outcome, b.outcome, c.outcome]).toEqual(['unknown', 'unknown', 'failed']);
  });

  it('调用方取消是 aborted，不当作错误提示', async () => {
    server.use(
      http.get(`${origin}/api/v1/identity/me`, async () => (await new Promise(() => {}), HttpResponse.json({}))),
    );
    const { client } = newClient();
    const controller = new AbortController();

    const pending = client.GET('/api/v1/identity/me', { signal: controller.signal }).catch((e: unknown) => e);
    controller.abort();

    expect(((await pending) as KmicError).kind).toBe('aborted');
  });

  it('超时', async () => {
    server.use(
      http.get(`${origin}/api/v1/identity/me`, async () => (await new Promise(() => {}), HttpResponse.json({}))),
    );
    const { client } = newClient({ timeoutMs: 50 });

    const error = (await client.GET('/api/v1/identity/me').catch((e: unknown) => e)) as KmicError;

    expect(error.kind).toBe('timeout');
  });
});

describe('幂等键', () => {
  it('写请求自动带幂等键，调用方给了就用调用方的', async () => {
    const keys: (string | null)[] = [];
    server.use(
      http.post(
        `${origin}/api/v1/identity/auth/logout`,
        ({ request }) => (keys.push(request.headers.get('Idempotency-Key')), new HttpResponse(null, { status: 204 })),
      ),
    );
    const { client } = newClient();

    await client.POST('/api/v1/identity/auth/logout');
    await client.POST('/api/v1/identity/auth/logout', { headers: { 'Idempotency-Key': 'mine' } as never });

    expect(keys).toEqual(['key-1', 'mine']);
  });
});

describe('401 与刷新', () => {
  it('刷新成功后重放原请求一次，写请求用同一个幂等键', async () => {
    const seen: { auth: string | null; key: string | null }[] = [];
    let refreshCalls = 0;
    server.use(
      http.post(`${origin}/api/v1/identity/auth/logout`, ({ request }) => {
        seen.push({ auth: request.headers.get('Authorization'), key: request.headers.get('Idempotency-Key') });
        return seen.length === 1 ? problem(401, 'Common.Unauthenticated') : new HttpResponse(null, { status: 204 });
      }),
      http.post(`${origin}/api/v1/identity/auth/refresh`, ({ request }) => {
        refreshCalls++;
        expect(request.headers.get('X-Requested-With')).toBe('kmic');
        expect(request.headers.get('Authorization')).toBeNull();
        return refreshed('token-2');
      }),
    );
    const { client, session } = newClient();

    await client.POST('/api/v1/identity/auth/logout');

    expect(refreshCalls).toBe(1);
    expect(seen).toEqual([
      { auth: 'Bearer token-1', key: 'key-1' },
      { auth: 'Bearer token-2', key: 'key-1' },
    ]);
    expect(session.token).toBe('token-2');
  });

  it('同一标签页并发的 401 共享一次刷新', async () => {
    let refreshCalls = 0;
    server.use(
      http.get(`${origin}/api/v1/identity/me`, ({ request }) =>
        request.headers.get('Authorization') === 'Bearer token-2'
          ? HttpResponse.json({ ok: true })
          : problem(401, 'Common.Unauthenticated'),
      ),
      http.post(`${origin}/api/v1/identity/auth/refresh`, async () => {
        refreshCalls++;
        await new Promise((resolve) => setTimeout(resolve, 30));
        return refreshed('token-2');
      }),
    );
    const { client } = newClient();

    const results = await Promise.all([
      client.GET('/api/v1/identity/me'),
      client.GET('/api/v1/identity/me'),
      client.GET('/api/v1/identity/me'),
    ]);

    expect(refreshCalls).toBe(1);
    expect(results.map((r) => r.data)).toEqual([{ ok: true }, { ok: true }, { ok: true }]);
  });

  it('刷新被拒绝：登录失效，原来的 401 报给调用方并通知监听者', async () => {
    server.use(
      http.get(`${origin}/api/v1/identity/me`, () => problem(401, 'Common.Unauthenticated')),
      http.post(`${origin}/api/v1/identity/auth/refresh`, () => problem(401, 'Identity.SessionExpired')),
    );
    const { client, session } = newClient();
    let expiredCalls = 0;
    session.onExpired(() => expiredCalls++);
    const epoch = session.epoch;

    const error = (await client.GET('/api/v1/identity/me').catch((e: unknown) => e)) as KmicError;

    expect([error.status, error.code]).toEqual([401, 'Common.Unauthenticated']);
    expect([session.token, expiredCalls, session.epoch > epoch]).toEqual([null, 1, true]);
  });

  it('登录和刷新自己的 401 不再触发刷新', async () => {
    let refreshCalls = 0;
    server.use(
      http.post(`${origin}/api/v1/identity/auth/login`, () => problem(401, 'Identity.LoginFailed')),
      http.post(`${origin}/api/v1/identity/auth/refresh`, () => (refreshCalls++, refreshed('x'))),
    );
    const { client } = newClient();

    const error = (await client
      .POST('/api/v1/identity/auth/login', { body: { account: 'a', password: 'b', clientType: 'web' } })
      .catch((e: unknown) => e)) as KmicError;

    expect([error.code, refreshCalls]).toEqual(['Identity.LoginFailed', 0]);
  });

  it('跨标签页的锁：刷新在锁里执行', async () => {
    const order: string[] = [];
    const locks = {
      async request<T>(name: string, callback: () => Promise<T>) {
        order.push(`lock:${name}`);
        const result = await callback();
        order.push('unlock');
        return result;
      },
    };
    server.use(
      http.get(`${origin}/api/v1/identity/me`, ({ request }) =>
        request.headers.get('Authorization') === 'Bearer token-2'
          ? HttpResponse.json({})
          : problem(401, 'Common.Unauthenticated'),
      ),
      http.post(`${origin}/api/v1/identity/auth/refresh`, () => (order.push('refresh'), refreshed('token-2'))),
    );
    const session = createSession();
    session.setToken('token-1');
    const client = createApiClient({ session, origin, baseUrl: origin, locks });

    await client.GET('/api/v1/identity/me');

    expect(order).toEqual(['lock:kmic-refresh', 'refresh', 'unlock']);
  });
});

describe('会话纪元', () => {
  it('响应回来时身份已经变了：丢弃，不当作错误提示', async () => {
    const { client, session } = newClient();
    server.use(
      http.get(`${origin}/api/v1/identity/me`, async () => {
        await new Promise((resolve) => setTimeout(resolve, 20));
        return HttpResponse.json({ displayName: '上一个身份的数据' });
      }),
    );

    const pending = client.GET('/api/v1/identity/me').catch((e: unknown) => e);
    session.changeIdentity();

    const error = (await pending) as KmicError;
    expect([error.kind]).toEqual(['aborted']);
  });
});
