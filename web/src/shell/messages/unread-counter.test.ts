import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import {
  createUnreadCounter,
  unreadInterval,
  type UnreadChannel,
  type UnreadEnvironment,
  type UnreadLocks,
} from './unread-counter';

// 模拟同一浏览器下的多标签页环境：按锁名互斥排队，广播频道向同名其他标签页推送消息
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

  it('启动时立即查询一次，随后每 30 秒轮询一次', async () => {
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

  it('多个可见标签页仅由一个负责轮询，并将结果同步至其余标签页', async () => {
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

  it('当前轮询标签页不可见后，由另一个可见标签页接管轮询', async () => {
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

  it('所有标签页均不可见时停止轮询', async () => {
    const hidden = createTab(createBrowser(), false);
    const counter = createUnreadCounter({ userId: 'u1', fetchCount, environment: hidden.environment });
    await vi.advanceTimersByTimeAsync(0);
    calls = 0;

    await vi.advanceTimersByTimeAsync(unreadInterval * 3);

    expect(calls).toBe(0);
    counter.stop();
  });

  it('本标签页更新未读数后立即生效，并同步广播至其他标签页', async () => {
    const browser = createBrowser();
    const a = createUnreadCounter({ userId: 'u1', fetchCount, environment: createTab(browser, true).environment });
    const b = createUnreadCounter({ userId: 'u1', fetchCount, environment: createTab(browser, false).environment });
    await vi.advanceTimersByTimeAsync(0);

    a.set(0);

    expect(b.count.value).toBe(0);
    a.stop();
    b.stop();
  });

  it('查询请求在途期间若已本地更新计数，过期响应不得覆盖最新值', async () => {
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

  it('查询失败时保留原未读计数', async () => {
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
