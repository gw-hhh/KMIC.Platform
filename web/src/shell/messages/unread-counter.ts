import { readonly, ref, type Ref } from 'vue';

export const unreadInterval = 30_000;

export interface UnreadLocks {
  request(name: string, options: { signal: AbortSignal }, callback: () => Promise<void>): Promise<unknown>;
}

export interface UnreadChannel {
  postMessage(message: unknown): void;
  addEventListener(type: 'message', listener: (event: MessageEvent) => void): void;
  close(): void;
}

export interface UnreadEnvironment {
  /** 锁不可用时（如非 HTTPS 环境），由各可见标签页各自轮询 */
  locks: UnreadLocks | null;
  openChannel: ((name: string) => UnreadChannel) | null;
  isVisible: () => boolean;
  onVisibilityChange: (listener: () => void) => () => void;
  now: () => number;
}

export interface UnreadCounter {
  readonly count: Readonly<Ref<number | null>>;
  /** 立即拉取最新未读数并广播至同源标签页 */
  refresh(): Promise<void>;
  /** 在本标签页执行已读操作后直接更新计数，无需等待下次轮询 */
  set(count: number): void;
  stop(): void;
}

interface CountMessage {
  count: number;
  at: number;
}

function isCountMessage(data: unknown): data is CountMessage {
  return (
    typeof data === 'object' &&
    data !== null &&
    typeof (data as CountMessage).count === 'number' &&
    typeof (data as CountMessage).at === 'number'
  );
}

export function browserEnvironment(): UnreadEnvironment {
  return {
    locks: 'locks' in navigator ? navigator.locks : null,
    openChannel: typeof BroadcastChannel === 'undefined' ? null : (name) => new BroadcastChannel(name),
    isVisible: () => document.visibilityState === 'visible',
    onVisibilityChange(listener) {
      document.addEventListener('visibilitychange', listener);
      return () => {
        document.removeEventListener('visibilitychange', listener);
      };
    },
    now: () => Date.now(),
  };
}

/** 站内消息未读数：多标签页间仅由单个可见页轮询并广播结果，不可见时自动让出 */
export function createUnreadCounter(options: {
  userId: string;
  fetchCount: () => Promise<number>;
  environment: UnreadEnvironment;
}): UnreadCounter {
  const { environment: env, fetchCount } = options;
  const name = `kmic-unread.${options.userId}`;
  const count = ref<number | null>(null);
  let lastAt = Number.NEGATIVE_INFINITY;
  // 单调递增版本号：若请求返回期间版本号已变更，则丢弃过时结果
  let version = 0;
  let stopped = false;
  let leading: AbortController | null = null;
  let timer: ReturnType<typeof setTimeout> | undefined;
  let wake: (() => void) | null = null;

  const channel = env.openChannel?.(name) ?? null;
  channel?.addEventListener('message', (event) => {
    if (isCountMessage(event.data)) {
      apply(event.data);
    }
  });

  function apply(message: CountMessage): void {
    version++;
    count.value = message.count;
    lastAt = message.at;
    // 收到更新后唤醒休眠中的主轮询循环，依据最新时间戳重算等待间隔
    wake?.();
  }

  function publish(value: number): void {
    const message = { count: value, at: env.now() };
    apply(message);
    channel?.postMessage(message);
  }

  async function refresh(): Promise<void> {
    const started = version;
    try {
      const value = await fetchCount();
      // 若请求期间已发生本地已读操作或收到他页广播，则忽略本次过期响应
      if (version === started) {
        publish(value);
      }
    } catch {
      // 查询失败时保留原计数，等待下轮重试
      lastAt = env.now();
    }
  }

  // 获取跨页互斥锁后持续轮询，直至页面不可见或计数器停止
  async function lead(signal: AbortSignal): Promise<void> {
    signal.addEventListener('abort', () => wake?.(), { once: true });
    while (!signal.aborted) {
      const wait = lastAt + unreadInterval - env.now();
      if (wait <= 0) {
        await refresh();
        continue;
      }
      await new Promise<void>((resolve) => {
        wake = resolve;
        timer = setTimeout(resolve, wait);
      });
      clearTimeout(timer);
      wake = null;
    }
  }

  function update(): void {
    const shouldLead = !stopped && env.isVisible();
    if (shouldLead && leading === null) {
      const controller = new AbortController();
      leading = controller;
      const run = (): Promise<void> => lead(controller.signal);
      const started = env.locks === null ? run() : env.locks.request(name, { signal: controller.signal }, run);
      // 等锁期间若被中止，navigator.locks 将抛出 AbortError，此处安全忽略
      void Promise.resolve(started).catch(() => undefined);
    } else if (!shouldLead && leading !== null) {
      leading.abort();
      leading = null;
    }
  }

  const stopWatching = env.onVisibilityChange(update);
  // 初始化时先立即查询一次再参与主控权竞争，避免新标签页等待首个轮询周期
  void refresh().finally(update);

  return {
    count: readonly(count),
    refresh,
    set: publish,
    stop() {
      stopped = true;
      update();
      stopWatching();
      channel?.close();
    },
  };
}
