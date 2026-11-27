import { z } from 'zod';

export type HistoryStorage = Pick<Storage, 'getItem' | 'setItem'>;

export const historyLimit = 8;

const historySchema = z.array(z.string());

const storageKey = (userId: string): string => `kmic.search-history.${userId}`;

/** 仅持久化路由名称，显示标题与权限校验以当前菜单树为准 */
export function loadSearchHistory(storage: HistoryStorage, userId: string): string[] {
  try {
    const text = storage.getItem(storageKey(userId));
    const result = historySchema.safeParse(text === null ? [] : JSON.parse(text));
    return result.success ? result.data.slice(0, historyLimit) : [];
  } catch {
    return [];
  }
}

export function saveSearchHistory(storage: HistoryStorage, userId: string, routeNames: readonly string[]): void {
  try {
    storage.setItem(storageKey(userId), JSON.stringify(routeNames.slice(0, historyLimit)));
  } catch {
    // 本地存储不可用时静默忽略
  }
}

/** 将指定路由移至或插入历史记录首位，并按上限截断 */
export function addToHistory(history: readonly string[], routeName: string): string[] {
  return [routeName, ...history.filter((name) => name !== routeName)].slice(0, historyLimit);
}
