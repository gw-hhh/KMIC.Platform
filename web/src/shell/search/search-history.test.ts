import { describe, expect, it } from 'vitest';
import {
  addToHistory,
  historyLimit,
  loadSearchHistory,
  saveSearchHistory,
  type HistoryStorage,
} from './search-history';

function memoryStorage(): HistoryStorage & { data: Map<string, string> } {
  const data = new Map<string, string>();
  return {
    data,
    getItem: (key) => data.get(key) ?? null,
    setItem: (key, value) => {
      data.set(key, value);
    },
  };
}

describe('搜索最近访问记录', () => {
  it('最近访问项置于首位且去重，超出上限时淘汰最旧记录', () => {
    let history: string[] = [];
    for (let i = 0; i < historyLimit + 2; i++) {
      history = addToHistory(history, `page-${String(i)}`);
    }
    history = addToHistory(history, 'page-5');

    expect(history).toHaveLength(historyLimit);
    expect(history[0]).toBe('page-5');
    expect(history.filter((name) => name === 'page-5')).toHaveLength(1);
    expect(history).not.toContain('page-0');
  });

  it('按用户标识隔离存储最近访问记录', () => {
    const storage = memoryStorage();
    saveSearchHistory(storage, 'u1', ['orders']);
    saveSearchHistory(storage, 'u2', ['ar-aging']);

    expect(loadSearchHistory(storage, 'u1')).toEqual(['orders']);
    expect(loadSearchHistory(storage, 'u2')).toEqual(['ar-aging']);
    expect(loadSearchHistory(storage, 'u3')).toEqual([]);
  });

  it('存储内容损坏或存储不可用时按空记录处理且不抛出异常', () => {
    const storage = memoryStorage();
    storage.data.set('kmic.search-history.u1', '{not json');
    storage.data.set('kmic.search-history.u2', JSON.stringify({ orders: true }));
    const broken: HistoryStorage = {
      getItem: () => {
        throw new Error('blocked');
      },
      setItem: () => {
        throw new Error('blocked');
      },
    };

    expect(loadSearchHistory(storage, 'u1')).toEqual([]);
    expect(loadSearchHistory(storage, 'u2')).toEqual([]);
    expect(loadSearchHistory(broken, 'u1')).toEqual([]);
    expect(() => {
      saveSearchHistory(broken, 'u1', ['orders']);
    }).not.toThrow();
  });
});
