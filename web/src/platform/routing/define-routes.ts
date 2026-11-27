import type { RouteEntry } from './types';

export function defineModuleRoutes(moduleName: string, entries: readonly RouteEntry[]): readonly RouteEntry[] {
  for (const entry of entries) {
    if (!entry.name.startsWith(`${moduleName}.`)) {
      throw new Error(`路由 ${entry.name} 必须以 ${moduleName}. 为前缀`);
    }
  }
  return entries;
}
