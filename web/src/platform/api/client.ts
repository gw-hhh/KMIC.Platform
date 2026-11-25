import createClient from 'openapi-fetch';
import type { paths } from '@/generated/api';
import { createKmicFetch, type KmicFetchOptions } from './kmicFetch';

/**
 * 带类型的 API 客户端，请求失败抛 KmicError
 */
export function createApiClient(options: KmicFetchOptions & { baseUrl?: string }) {
  return createClient<paths>({
    baseUrl: options.baseUrl ?? '',
    fetch: createKmicFetch(options),
  });
}

export type ApiClient = ReturnType<typeof createApiClient>;
