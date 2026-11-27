import { http, HttpResponse } from 'msw';
import { setupServer } from 'msw/node';
import { afterAll, afterEach, beforeAll, describe, expect, it } from 'vitest';
import { createSession } from '@/platform/session/session';
import { createApiClient } from './client';
import { defineModuleApi } from './define-module-api';
import { createKmicQueryClient } from './query-client';

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

const jobsApi = defineModuleApi('operations', ({ query, command }) => ({
  myJobs: query('/api/v1/operations/my-jobs'),
  myJob: query('/api/v1/operations/my-jobs/{module}/{id}'),
  cancel: command('POST', '/api/v1/operations/my-jobs/{module}/{id}/cancel'),
  markRead: command('POST', '/api/v1/notifications/site-messages/{id}/read'),
}));

function setup() {
  const session = createSession();
  session.setToken('token-1');
  const api = createApiClient({ session, origin, baseUrl: origin, locks: null });
  return { session, api, queryClient: createKmicQueryClient({ api, session }) };
}

describe('模块查询定义', () => {
  it('查询键包含 kmic、模块名、路径与参数，all 为不含参数的前缀', () => {
    const params = { path: { module: 'Sample', id: 'j1' } };

    expect(jobsApi.myJob.key({ params })).toEqual([
      'kmic',
      'operations',
      '/api/v1/operations/my-jobs/{module}/{id}',
      params,
    ]);
    expect(jobsApi.myJobs.key()).toEqual(['kmic', 'operations', '/api/v1/operations/my-jobs', {}]);
    expect(jobsApi.myJobs.all).toEqual(['kmic', 'operations', '/api/v1/operations/my-jobs']);
  });

  it('按参数拼接路径与查询字符串，并通过 QueryClient 获取请求客户端', async () => {
    let url = '';
    server.use(
      http.get(`${origin}/api/v1/operations/my-jobs`, ({ request }) => {
        url = request.url;
        return HttpResponse.json({ items: [], nextCursor: null });
      }),
    );
    const { queryClient } = setup();

    const data = await queryClient.query(
      jobsApi.myJobs.options({ params: { query: { Status: 'Running', Limit: 5 } } }),
    );

    expect(data).toEqual({ items: [], nextCursor: null });
    expect(new URL(url).search).toBe('?Status=Running&Limit=5');
  });

  it('身份变化时清空缓存', async () => {
    server.use(
      http.get(`${origin}/api/v1/operations/my-jobs`, () => HttpResponse.json({ items: [], nextCursor: null })),
    );
    const { session, queryClient } = setup();
    await queryClient.query(jobsApi.myJobs.options());

    session.changeIdentity();

    expect(queryClient.getQueryData(jobsApi.myJobs.key())).toBeUndefined();
  });
});

describe('模块命令定义', () => {
  it('依据 OpenAPI 的 x-idempotent 标记判断是否需要幂等键', () => {
    expect(jobsApi.cancel.idempotent).toBe(true);
    expect(jobsApi.markRead.idempotent).toBe(false);
  });

  it('发送请求时携带指定的幂等键与路径参数', async () => {
    let seen: { url: string; key: string | null } | null = null;
    server.use(
      http.post(`${origin}/api/v1/operations/my-jobs/:module/:id/cancel`, ({ request }) => {
        seen = { url: new URL(request.url).pathname, key: request.headers.get('Idempotency-Key') };
        return new HttpResponse(null, { status: 204 });
      }),
    );
    const { api } = setup();

    await expect(
      jobsApi.cancel.send(api, { params: { path: { module: 'Sample', id: 'j1' } } }, { idempotencyKey: 'k-1' }),
    ).resolves.toBeUndefined();
    expect(seen).toEqual({ url: '/api/v1/operations/my-jobs/Sample/j1/cancel', key: 'k-1' });
  });
});
