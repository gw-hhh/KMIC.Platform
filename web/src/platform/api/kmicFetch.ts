import type { components } from '@/generated/api';
import type { Session } from '@/platform/session/session';
import { KmicError, type KmicOutcome } from './kmicError';

type AuthTokens = components['schemas']['AuthTokens'];

const readMethods = new Set(['GET', 'HEAD', 'OPTIONS']);

/** navigator.locks 的最小接口 */
export interface RefreshLocks {
  request<T>(name: string, callback: () => Promise<T>): Promise<T>;
}

export interface KmicFetchOptions {
  session: Session;
  /** 默认用全局 fetch */
  fetch?: typeof fetch;
  /** 默认用当前页面的 origin */
  origin?: string;
  refreshUrl?: string;
  /** 请求超时，比后端的 30 秒略长 */
  timeoutMs?: number;
  /** 刷新令牌的跨标签页锁 */
  locks?: RefreshLocks | null;
  newId?: () => string;
}

const authPaths = ['/auth/login', '/auth/refresh'];

/**
 * 给 openapi-fetch 用的 fetch：令牌、401 刷新、幂等键、epoch 检查、错误转换
 */
export function createKmicFetch(options: KmicFetchOptions): (request: Request) => Promise<Response> {
  const session = options.session;
  const send = options.fetch ?? globalThis.fetch.bind(globalThis);
  const origin = options.origin ?? globalThis.location.origin;
  const refreshUrl = new URL(options.refreshUrl ?? '/api/v1/identity/auth/refresh', origin).href;
  const timeoutMs = options.timeoutMs ?? 35_000;
  const newId = options.newId ?? (() => crypto.randomUUID());
  const locks = options.locks === undefined ? defaultLocks() : options.locks;

  let refreshing: Promise<boolean> | null = null;

  return async (request) => {
    const isWrite = !readMethods.has(request.method.toUpperCase());
    const epoch = session.epoch;
    const pristine = request.clone();
    const idempotencyKey = isWrite ? (request.headers.get('Idempotency-Key') ?? newId()) : null;

    let expired = false;
    let response = await attempt(pristine, request);
    if (response.status === 401 && !isAuthRequest(request.url)) {
      if (await refresh()) {
        // 用同一个幂等键重放，不会重复执行
        response = await attempt(pristine, request);
      } else {
        session.expire();
        expired = true;
      }
    }

    // 身份已变，丢弃结果（登录失效除外）
    if (!expired && session.epoch !== epoch) {
      throw new KmicError({ kind: 'aborted', message: '身份已变化，请求结果已丢弃' });
    }

    if (!response.ok) {
      throw await toError(response, isWrite);
    }

    return response;

    async function attempt(source: Request, original: Request): Promise<Response> {
      const headers = new Headers(source.headers);
      if (session.token !== null && !isAuthRequest(source.url)) {
        headers.set('Authorization', `Bearer ${session.token}`);
      }

      if (idempotencyKey !== null) {
        headers.set('Idempotency-Key', idempotencyKey);
      }

      // 请求对象只能用一次
      const outgoing = new Request(source.clone(), { headers, signal: signalOf(original.signal) });
      try {
        return await send(outgoing);
      } catch (error) {
        throw toTransportError(error, isWrite, original.signal);
      }
    }
  };

  function signalOf(caller: AbortSignal): AbortSignal {
    return AbortSignal.any([caller, AbortSignal.timeout(timeoutMs)]);
  }

  // 同一标签页共用一次刷新，跨标签页用锁排队
  function refresh(): Promise<boolean> {
    refreshing ??= (locks === null ? doRefresh() : locks.request('kmic-refresh', doRefresh)).finally(() => {
      refreshing = null;
    });
    return refreshing;
  }

  async function doRefresh(): Promise<boolean> {
    let response: Response;
    try {
      // 刷新也要超时：挂住的话，所有等刷新的请求和别的标签页都会跟着卡住
      response = await send(
        new Request(refreshUrl, {
          method: 'POST',
          headers: { 'X-Requested-With': 'kmic' },
          signal: AbortSignal.timeout(timeoutMs),
        }),
      );
    } catch (error) {
      throw toTransportError(error, false, undefined);
    }

    if (!response.ok) {
      return false;
    }

    const tokens = (await response.json()) as AuthTokens;
    session.setToken(tokens.access_token);
    return true;
  }
}

// 非 HTTPS 下没有 navigator.locks
function defaultLocks(): RefreshLocks | null {
  return 'locks' in globalThis.navigator ? globalThis.navigator.locks : null;
}

function isAuthRequest(url: string): boolean {
  const path = new URL(url, 'http://localhost').pathname;
  return authPaths.some((suffix) => path.endsWith(suffix));
}

function toTransportError(error: unknown, isWrite: boolean, caller: AbortSignal | undefined): KmicError {
  if (error instanceof KmicError) {
    return error;
  }

  const unknown: KmicOutcome | undefined = isWrite ? 'unknown' : undefined;
  if (caller?.aborted === true) {
    return new KmicError({ kind: 'aborted', message: '请求已取消', cause: error });
  }

  if (error instanceof DOMException && error.name === 'TimeoutError') {
    return new KmicError({
      kind: 'timeout',
      message: '请求超时',
      ...(unknown === undefined ? {} : { outcome: unknown }),
      cause: error,
    });
  }

  return new KmicError({
    kind: 'network',
    message: '网络连接失败，请检查网络',
    ...(unknown === undefined ? {} : { outcome: unknown }),
    cause: error,
  });
}

async function toError(response: Response, isWrite: boolean): Promise<KmicError> {
  const retryAfter = parseRetryAfter(response.headers.get('Retry-After'));
  const traceId = response.headers.get('X-Trace-Id') ?? undefined;
  const problem = await readProblem(response);
  const outcome = isWrite ? outcomeOf(response.status, problem?.code) : undefined;
  return problem === null
    ? KmicError.fromStatus(response.status, traceId, outcome)
    : KmicError.fromProblem(problem, retryAfter, outcome);
}

// 500 和“结果未知”的 503，不确定是否已生效
function outcomeOf(status: number, code: string | undefined): KmicOutcome {
  return status === 500 || (status === 503 && code === 'Common.ResultUnknown') ? 'unknown' : 'failed';
}

async function readProblem(response: Response): Promise<components['schemas']['KmicProblem'] | null> {
  const type = response.headers.get('Content-Type') ?? '';
  if (!type.includes('json')) {
    return null;
  }

  try {
    const body = (await response.json()) as Partial<components['schemas']['KmicProblem']>;
    return typeof body.code === 'string' && typeof body.title === 'string'
      ? (body as components['schemas']['KmicProblem'])
      : null;
  } catch {
    return null;
  }
}

function parseRetryAfter(value: string | null): number | undefined {
  if (value === null) {
    return undefined;
  }

  const seconds = Number(value);
  return Number.isFinite(seconds) && seconds >= 0 ? seconds : undefined;
}
