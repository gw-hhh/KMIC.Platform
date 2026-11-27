import { z } from 'zod';

export interface TabStorage {
  getItem(key: string): string | null;
  setItem(key: string, value: string): void;
}

/** 持久化的标签页元数据（不含页面内部状态） */
export interface StoredTab {
  routeName: string;
  params: Record<string, string>;
  instance: string | null;
  pinned: boolean;
}

const storedTabsSchema = z.array(
  z.object({
    routeName: z.string(),
    params: z.record(z.string(), z.string()),
    instance: z.string().nullable(),
    pinned: z.boolean(),
  }),
);

const storageKey = (userId: string): string => `kmic.tabs.${userId}`;

export function loadStoredTabs(storage: TabStorage, userId: string): StoredTab[] {
  try {
    const text = storage.getItem(storageKey(userId));
    const result = storedTabsSchema.safeParse(text === null ? [] : JSON.parse(text));
    return result.success ? result.data : [];
  } catch {
    return [];
  }
}

export function saveStoredTabs(storage: TabStorage, userId: string, tabs: readonly StoredTab[]): void {
  try {
    storage.setItem(storageKey(userId), JSON.stringify(tabs));
  } catch {
    // 本地存储不可用时静默忽略
  }
}
