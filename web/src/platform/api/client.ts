import createClient from 'openapi-fetch';
import type { paths } from '@/generated/api';
import { createKmicFetch, type KmicFetchOptions } from './kmicFetch';

/**
 * 类型化的 API 客户端：路径、参数、请求体、响应都有类型
 *
 * 请求失败一律抛 KmicError，所以调用方拿到的 data 一定有值，不用再判断 error
 */
export function createApiClient(options: KmicFetchOptions & { baseUrl?: string }) {
  return createClient<paths>({
    baseUrl: options.baseUrl ?? '',
    fetch: createKmicFetch(options),
  });
}

export type ApiClient = ReturnType<typeof createApiClient>;
