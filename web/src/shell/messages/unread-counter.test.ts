import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import {
  createUnreadCounter,
  unreadInterval,
  type UnreadChannel,
  type UnreadEnvironment,
  type UnreadLocks,
} from './unread-counter';

// 同一个浏览器里的多个标签页：锁按名字互斥排队，频道广播给同名的其他标签页
function createBrowser() {
  const holders = new Map<string, boolean>();
  const queues = new Map<string, (() => void)[]>();
  const channels = new Set<{ name: string; listeners: ((event: MessageEvent) => void)[] }>();

  const locks: UnreadLocks = {
    request(name, { signal }, callback) {
      return new Promise((resolve, reject) => {
        const start = (): void => {
          holders.set(name, true);
          void callback()
            .then(resolve, reject)
            .finally(() => {
              holders.set(name, false);
              queues.get(name)?.shift()?.();
            });
        };
        if (holders.get(name) === true) {
          const queue = queues.get(name) ?? [];
          queue.push(start);
          queues.set(name, queue);
          signal.addEventListener('abort', () => {
            queues.set(
              name,
              (queues.get(name) ?? []).filter((item) => item !== start),
            );
            reject(new DOMException('aborted', 'AbortError'));
          });
        } else {
          start();
        }
      });
    },
  };

  function openChannel(name: string): UnreadChannel {
    const self = { name, listeners: [] as ((event: MessageEvent) => void)[] };
    channels.add(self);
    return {
      postMessage(data) {
        for (const other of channels) {
          if (other !== self && other.name === name) {
            other.listeners.forEach((listener) => {
              listener(new MessageEvent('message', { data }));
            });
          }
        }
      },
      addEventListener(_type, listener) {
        self.listeners.push(listener);
      },
      close() {
        channels.delete(self);
      },
    };
  }

  return { locks, openChannel };
}

function createTab(browser: ReturnType<typeof createBrowser>, visible: boolean) {
  const listeners = new Set<() => void>();
  const tab = {
    visible,
    setVisible(value: boolean) {
      tab.visible = value;
      listeners.forEach((listener) => {
        listener();
      });
    },
  };
  const environment: UnreadEnvironment = {
    locks: browser.locks,
    openChannel: browser.openChannel,
    isVisible: () => tab.visible,
    onVisibilityChange(listener) {
      listeners.add(listener);
      return () => listeners.delete(listener);
    },
    now: () => Date.now(),
  };
  return { tab, environment };
}

describe('站内消息未读数', () => {
  let server: number;
  let calls: number;
  const fetchCount = (): Promise<number> => {
    calls++;
    return Promise.resolve(server);
  };

  beforeEach(() => {
    vi.useFakeTimers();
    server = 3;
    calls = 0;
  });
  afterEach(() => {
    vi.useRealTimers();
  });

  it('打开时查一次，之后每 30 秒查一次', async () => {
    const { environment } = createTab(createBrowser(), true);
    const counter = createUnreadCounter({ userId: 'u1', fetchCount, environment });
    await vi.advanceTimersByTimeAsync(0);

    expect(counter.count.value).toBe(3);
    expect(calls).toBe(1);

    server = 5;
    await vi.advanceTimersByTimeAsync(unreadInterval);
    expect(counter.count.value).toBe(5);
    expect(calls).toBe(2);
    counter.stop();
  });

  it('多个可见的标签页只有一个在轮询，结果分享给其他标签页', async () => {
    const browser = createBrowser();
    const a = createUnreadCounter({ userId: 'u1', fetchCount, environment: createTab(browser, true).environment });
    const b = createUnreadCounter({ userId: 'u1', fetchCount, environment: createTab(browser, true).environment });
    await vi.advanceTimersByTimeAsync(0);
    calls = 0;

    server = 7;
    await vi.advanceTimersByTimeAsync(unreadInterval * 3);

    expect(calls).toBe(3);
    expect([a.count.value, b.count.value]).toEqual([7, 7]);
    a.stop();
    b.stop();
  });

  it('轮询的标签页不可见后，由另一个可见的接手', async () => {
    const browser = createBrowser();
    const first = createTab(browser, true);
    const a = createUnreadCounter({ userId: 'u1', fetchCount, environment: first.environment });
    const b = createUnreadCounter({ userId: 'u1', fetchCount, environment: createTab(browser, true).environment });
    await vi.advanceTimersByTimeAsync(0);

    first.tab.setVisible(false);
    server = 9;
    await vi.advanceTimersByTimeAsync(unreadInterval);

    expect(b.count.value).toBe(9);
    expect(a.count.value).toBe(9);
    a.stop();
    b.stop();
  });

  it('都不可见时不轮询', async () => {
    const hidden = createTab(createBrowser(), false);
    const counter = createUnreadCounter({ userId: 'u1', fetchCount, environment: hidden.environment });
    await vi.advanceTimersByTimeAsync(0);
    calls = 0;

    await vi.advanceTimersByTimeAsync(unreadInterval * 3);

    expect(calls).toBe(0);
    counter.stop();
  });

  it('本标签页标了已读后直接更新，其他标签页同步', async () => {
    const browser = createBrowser();
    const a = createUnreadCounter({ userId: 'u1', fetchCount, environment: createTab(browser, true).environment });
    const b = createUnreadCounter({ userId: 'u1', fetchCount, environment: createTab(browser, false).environment });
    await vi.advanceTimersByTimeAsync(0);

    a.set(0);

    expect(b.count.value).toBe(0);
    a.stop();
    b.stop();
  });

  it('查询期间标了已读，返回的旧数不覆盖', async () => {
    const { environment } = createTab(createBrowser(), true);
    let release: (value: number) => void = () => undefined;
    let slow = false;
    const counter = createUnreadCounter({
      userId: 'u1',
      fetchCount: () =>
        slow
          ? new Promise<number>((resolve) => {
              release = resolve;
            })
          : Promise.resolve(2),
      environment,
    });
    await vi.advanceTimersByTimeAsync(0);

    slow = true;
    const refreshing = counter.refresh();
    counter.set(0);
    release(2);
    await refreshing;

    expect(counter.count.value).toBe(0);
    counter.stop();
  });

  it('查询失败时保留原来的数', async () => {
    const { environment } = createTab(createBrowser(), true);
    let fail = false;
    const counter = createUnreadCounter({
      userId: 'u1',
      fetchCount: () => (fail ? Promise.reject(new Error('断网')) : Promise.resolve(4)),
      environment,
    });
    await vi.advanceTimersByTimeAsync(0);

    fail = true;
    await vi.advanceTimersByTimeAsync(unreadInterval);

    expect(counter.count.value).toBe(4);
    counter.stop();
  });

  it('停止后不再轮询', async () => {
    const { environment } = createTab(createBrowser(), true);
    const counter = createUnreadCounter({ userId: 'u1', fetchCount, environment });
    await vi.advanceTimersByTimeAsync(0);
    counter.stop();
    calls = 0;

    await vi.advanceTimersByTimeAsync(unreadInterval * 2);

    expect(calls).toBe(0);
  });
});
