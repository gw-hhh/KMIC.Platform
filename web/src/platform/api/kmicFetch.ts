import type { components } from '@/generated/api';
import type { Session } from '@/platform/session/session';
import { KmicError, type KmicOutcome } from './kmicError';

type AuthTokens = components['schemas']['AuthTokens'];

const readMethods = new Set(['GET', 'HEAD', 'OPTIONS']);

/** 跨浏览器标签页的互斥，navigator.locks 的最小形状，测试里可以换成假的 */
export interface RefreshLocks {
  request<T>(name: string, callback: () => Promise<T>): Promise<T>;
}

export interface KmicFetchOptions {
  session: Session;
  /** 底层的 fetch，默认用全局的 */
  fetch?: typeof fetch;
  /** 相对地址的基准，默认是当前页面的来源 */
  origin?: string;
  /** 刷新令牌的地址 */
  refreshUrl?: string;
  /** 读写请求的超时，略大于后端的 30 秒请求超时 */
  timeoutMs?: number;
  /** 刷新时的跨标签页锁，默认用 navigator.locks，没有就不加锁 */
  locks?: RefreshLocks | null;
  /** 生成幂等键 */
  newId?: () => string;
}

const authPaths = ['/auth/login', '/auth/refresh'];

/**
 * 给 openapi-fetch 用的 fetch：令牌与 401 刷新、幂等键、会话纪元、错误转换
 *
 * - 带访问令牌；401 时进入刷新，同一标签页并发的 401 共享一次刷新，成功后重放一次，写请求重放用同一个幂等键
 * - 写请求自动带幂等键，没有声明幂等的接口会忽略它
 * - 发出时记下会话纪元，响应回来纪元变了就丢弃，不写缓存也不提示
 * - 所有失败都抛 KmicError；写请求区分“确定没有生效”和“结果未知”
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
        // 重放用原始请求和同一个幂等键，不会重复执行
        response = await attempt(pristine, request);
      } else {
        session.expire();
        expired = true;
      }
    }

    // 登录失效本身就是这次请求的结果，要原样报给调用方；其余情况纪元变了就说明结果属于上一个身份
    if (!expired && session.epoch !== epoch) {
      throw new KmicError({ kind: 'aborted', message: '身份已经变化，丢弃这次请求的结果' });
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

      // 每次发送都要新的请求对象：同一个请求体只能读一次
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

  // 刷新令牌：同一标签页内共享一次，跨标签页经锁串行，后拿到锁的标签页用的已经是前一个轮换后的 Cookie
  function refresh(): Promise<boolean> {
    refreshing ??= (locks === null ? doRefresh() : locks.request('kmic-refresh', doRefresh)).finally(() => {
      refreshing = null;
    });
    return refreshing;
  }

  async function doRefresh(): Promise<boolean> {
    let response: Response;
    try {
      response = await send(new Request(refreshUrl, { method: 'POST', headers: { 'X-Requested-With': 'kmic' } }));
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

// 没有 HTTPS 的环境里浏览器不提供 navigator.locks，只能不加锁
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
    message: '网络不通，请检查网络后再试',
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

// 写请求遇到 500，或 503 的“结果未知”：服务端也不确定有没有生效；其余状态码都是确定没有生效
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
