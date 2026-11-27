import { QueryClient, type QueryFunctionContext } from '@tanstack/vue-query';
import type { Session } from '@/platform/session/session';
import type { ApiClient } from './client';
import { KmicError } from './kmicError';

// 查询函数上下文里给的是 query-core 的 QueryClient 基类，按它做键；vue-query 的 QueryClient 是它的子类，同样能查
type AnyQueryClient = QueryFunctionContext['client'];

const clients = new WeakMap<AnyQueryClient, ApiClient>();

export const maxQueryRetries = 2;

/** 创建平台查询客户端；会话身份变更时自动清空缓存，防止跨用户或跨角色数据串用 */
export function createKmicQueryClient(options: { api: ApiClient; session: Session }): QueryClient {
  const client = new QueryClient({
    defaultOptions: {
      queries: {
        // 默认视为即时过期，字典等低频变动数据应在各自查询中单独配置 staleTime
        staleTime: 0,
        // 禁用窗口聚焦自动刷新，避免切换窗口时打断当前操作
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

/** 从上下文 QueryClient 获取关联的 ApiClient，避免模块 api.ts 显式注入依赖 */
export function apiOf(client: AnyQueryClient): ApiClient {
  const api = clients.get(client);
  if (api === undefined) {
    throw new Error('QueryClient 必须通过 createKmicQueryClient 创建');
  }
  return api;
}
