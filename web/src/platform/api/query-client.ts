import { QueryClient } from '@tanstack/vue-query';
import type { Session } from '@/platform/session/session';
import type { ApiClient } from './client';
import { KmicError } from './kmicError';

const clients = new WeakMap<QueryClient, ApiClient>();

/** 可重试的错误最多再试两次 */
export const maxQueryRetries = 2;

/**
 * 查询缓存。身份一变整个清空，不同用户、角色的数据不会串
 */
export function createKmicQueryClient(options: { api: ApiClient; session: Session }): QueryClient {
  const client = new QueryClient({
    defaultOptions: {
      queries: {
        // 每次进入或激活都当作可能过期；字典等长期不变的数据在各自的查询里另设
        staleTime: 0,
        // 后台系统切回窗口就刷新会打断阅读
        refetchOnWindowFocus: false,
        retry: (failureCount, error) => error instanceof KmicError && error.retryable && failureCount < maxQueryRetries,
      },
    },
  });
  clients.set(client, options.api);
  options.session.onEpochChanged(() => {
    client.clear();
  });
  return client;
}

/** 查询函数经上下文里的 QueryClient 拿到请求客户端，模块的 api.ts 不需要注入 */
export function apiOf(client: QueryClient): ApiClient {
  const api = clients.get(client);
  if (api === undefined) {
    throw new Error('QueryClient 不是 createKmicQueryClient 创建的');
  }
  return api;
}
