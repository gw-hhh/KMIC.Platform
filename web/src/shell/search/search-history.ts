import { z } from 'zod';

export type HistoryStorage = Pick<Storage, 'getItem' | 'setItem'>;

/** 最多记几条 */
export const historyLimit = 8;

const historySchema = z.array(z.string());

const storageKey = (userId: string): string => `kmic.search-history.${userId}`;

/** 只存路由名，标题和权限按当前菜单算 */
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
    // 存储不可用时忽略
  }
}

/** 最近打开的排在最前 */
export function addToHistory(history: readonly string[], routeName: string): string[] {
  return [routeName, ...history.filter((name) => name !== routeName)].slice(0, historyLimit);
}
