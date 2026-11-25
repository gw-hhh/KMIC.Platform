import { readonly, ref, type Ref } from 'vue';

/** 未读数的查询间隔 */
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
  /** 没有时（非 HTTPS）每个可见的标签页各自轮询 */
  locks: UnreadLocks | null;
  openChannel: ((name: string) => UnreadChannel) | null;
  isVisible: () => boolean;
  onVisibilityChange: (listener: () => void) => () => void;
  now: () => number;
}

export interface UnreadCounter {
  readonly count: Readonly<Ref<number | null>>;
  /** 马上查一次并告诉其他标签页 */
  refresh(): Promise<void>;
  /** 本标签页标了已读后直接改，不等下一次轮询 */
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

/**
 * 站内消息未读数。多个标签页时只由一个可见的标签页轮询，结果分享给其他标签页；
 * 轮询的标签页不可见时让出，换另一个可见的接手
 */
export function createUnreadCounter(options: {
  userId: string;
  fetchCount: () => Promise<number>;
  environment: UnreadEnvironment;
}): UnreadCounter {
  const { environment: env, fetchCount } = options;
  const name = `kmic-unread.${options.userId}`;
  const count = ref<number | null>(null);
  let lastAt = Number.NEGATIVE_INFINITY;
  // 每次数字更新加一；查询返回时已经变过，说明结果旧了
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
    // 刚有人查过，轮询的标签页按新的时间重新排
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
      // 查询期间标了已读或收到其他标签页的结果，这次的数已经过时
      if (version === started) {
        publish(value);
      }
    } catch {
      // 查不到时保留原来的数，下一轮再查
      lastAt = env.now();
    }
  }

  // 拿到锁后一直轮询，直到不可见或停止
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
      // 排队时被取消会以 AbortError 结束
      void Promise.resolve(started).catch(() => undefined);
    } else if (!shouldLead && leading !== null) {
      leading.abort();
      leading = null;
    }
  }

  const stopWatching = env.onVisibilityChange(update);
  // 先查一次再决定由谁轮询，新开的标签页不用等别人的下一轮
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
