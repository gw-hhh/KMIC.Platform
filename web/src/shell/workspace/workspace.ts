import { computed, reactive, shallowRef, type ComputedRef } from 'vue';
import type { RouteLocationNormalized, RouteLocationRaw, Router } from 'vue-router';
import { createPageInstance, type LeaveStatus, type PageInstance } from '@/platform/page';
import type { RouteCatalog, RouteParams } from '@/platform/routing';
import { loadStoredTabs, saveStoredTabs, type StoredTab, type TabStorage } from './tab-storage';

export interface Tab {
  readonly key: string;
  readonly routeName: string;
  readonly params: RouteParams;
  readonly instance: string | null;
  readonly pinned: boolean;
  /** 页面重新加载代次（用于强制刷新 KeepAlive 实例） */
  readonly generation: number;
}

export interface LeaveReport {
  key: string;
  title: string;
  status: Exclude<LeaveStatus, 'free'>;
}

export interface WorkspaceOptions {
  router: Router;
  catalog: RouteCatalog;
  /** 首页路由名称（默认固定且不可关闭） */
  homeRouteName: string;
  epoch: () => number;
  maxTabs: () => number;
  /** 存在未保存或保存中状态时的离开确认回调，返回 true 表示确认离开 */
  confirmLeave: (reports: readonly LeaveReport[]) => Promise<boolean>;
  /** 标签页达到上限时的替换确认回调；candidate 为建议关闭的候选标签 */
  confirmReplace: (candidate: { key: string; title: string } | null) => Promise<boolean>;
  storage: TabStorage | null;
  userId: () => string | null;
}

/** 工作区多标签状态管理接口；所有标签页变更均须经由该接口执行 */
export interface Workspace {
  readonly tabs: ComputedRef<readonly Tab[]>;
  readonly activeKey: ComputedRef<string | null>;
  titleOf(tab: Tab): string;
  pageOf(key: string): PageInstance | undefined;
  open(name: string, params?: RouteParams): Promise<void>;
  activate(key: string): Promise<void>;
  close(key: string): Promise<boolean>;
  closeMany(keys: readonly string[]): Promise<boolean>;
  togglePin(key: string): void;
  /** 调整标签页顺序（仅允许在固定或非固定同类分组内移动） */
  move(from: number, to: number): void;
  reload(key: string): Promise<boolean>;
  /** 身份代际变更后，递增无未保存变更页面的 generation 以重建实例 */
  onEpochChange(): void;
  canLeaveAll(): Promise<boolean>;
  /** 导航前置钩子（校验标签上限与替换逻辑），返回 false 时中止跳转 */
  beforeNavigate(to: RouteLocationNormalized): Promise<boolean>;
  commit(to: RouteLocationNormalized): void;
  reset(): void;
  restore(): void;
  ensureHome(): void;
}

const instanceQuery = '_t';

export function createWorkspace(options: WorkspaceOptions): Workspace {
  const { router, catalog } = options;
  const state = reactive<{ tabs: Tab[]; activeKey: string | null; visited: string[] }>({
    tabs: [],
    activeKey: null,
    visited: [],
  });
  const pages = shallowRef(new Map<string, PageInstance>());

  const statusOf = (key: string): LeaveStatus => pages.value.get(key)?.leaveStatus.value ?? 'free';

  function titleOf(tab: Tab): string {
    return pages.value.get(tab.key)?.title.value ?? catalog.find(tab.routeName)?.title ?? tab.routeName;
  }

  function createPage(key: string): void {
    pages.value.get(key)?.destroy();
    pages.value = new Map(pages.value).set(
      key,
      createPageInstance({ key, epoch: options.epoch(), isActive: () => state.activeKey === key }),
    );
  }

  function removePage(key: string): void {
    pages.value.get(key)?.destroy();
    const next = new Map(pages.value);
    next.delete(key);
    pages.value = next;
  }

  function resolve(route: RouteLocationNormalized): { key: string; instance: string | null } | null {
    const name = typeof route.name === 'string' ? route.name : null;
    const entry = name === null ? undefined : catalog.find(name);
    if (name === null || entry === undefined) {
      return null;
    }
    const raw = route.query[instanceQuery];
    const instance = entry.tab.mode === 'multiple' && typeof raw === 'string' ? raw : null;
    return { key: catalog.tabKey(name, toParams(route.params), instance ?? ''), instance };
  }

  function persist(): void {
    const userId = options.userId();
    if (options.storage === null || userId === null) {
      return;
    }
    const stored: StoredTab[] = state.tabs.map((tab) => ({
      routeName: tab.routeName,
      params: { ...tab.params },
      instance: tab.instance,
      pinned: tab.pinned,
    }));
    saveStoredTabs(options.storage, userId, stored);
  }

  function insert(tab: Tab): void {
    const firstUnpinned = state.tabs.findIndex((t) => !t.pinned);
    if (tab.pinned && firstUnpinned !== -1) {
      state.tabs.splice(firstUnpinned, 0, tab);
    } else {
      state.tabs.push(tab);
    }
    createPage(tab.key);
  }

  async function ensureCanLeave(keys: readonly string[]): Promise<boolean> {
    const reports = keys.flatMap((key): LeaveReport[] => {
      const tab = state.tabs.find((t) => t.key === key);
      const status = statusOf(key);
      return tab === undefined || status === 'free' ? [] : [{ key, title: titleOf(tab), status }];
    });
    if (reports.length === 0) {
      return true;
    }
    if (!(await options.confirmLeave(reports))) {
      return false;
    }
    // 仅放行确认弹窗中涵盖的标签页，防止确认期间新增脏状态被误关闭
    const confirmed = new Set(reports.map((report) => report.key));
    return keys.every((key) => confirmed.has(key) || statusOf(key) === 'free');
  }

  async function activate(key: string): Promise<void> {
    const tab = state.tabs.find((t) => t.key === key);
    if (tab !== undefined) {
      await router.push(locationOf(tab));
    }
  }

  async function leaveClosedActive(closedKey: string): Promise<void> {
    const previous = [...state.visited]
      .reverse()
      .find((key) => key !== closedKey && state.tabs.some((t) => t.key === key));
    const target =
      state.tabs.find((t) => t.key === previous) ?? state.tabs.find((t) => t.routeName === options.homeRouteName);
    if (target === undefined) {
      await router.push({ name: options.homeRouteName });
    } else {
      await activate(target.key);
    }
  }

  const workspace: Workspace = {
    tabs: computed(() => state.tabs),
    activeKey: computed(() => state.activeKey),
    titleOf,
    pageOf: (key) => pages.value.get(key),
    activate,

    async open(name, params = {}) {
      const entry = catalog.find(name);
      if (entry === undefined) {
        throw new Error(`路由 ${name} 未在路由目录中注册`);
      }
      const instance = entry.tab.mode === 'multiple' ? crypto.randomUUID().slice(0, 8) : null;
      const existing = state.tabs.find((t) => t.key === catalog.tabKey(name, params, instance ?? ''));
      await router.push(
        existing
          ? locationOf(existing)
          : { name, params, ...(instance ? { query: { [instanceQuery]: instance } } : {}) },
      );
    },

    close: (key) => workspace.closeMany([key]),

    async closeMany(keys) {
      const closable = keys.filter((key) => state.tabs.some((t) => t.key === key && !t.pinned));
      if (closable.length === 0 || !(await ensureCanLeave(closable))) {
        return false;
      }
      const closedActive = state.activeKey !== null && closable.includes(state.activeKey) ? state.activeKey : null;
      state.tabs = state.tabs.filter((t) => !closable.includes(t.key));
      state.visited = state.visited.filter((key) => !closable.includes(key));
      closable.forEach(removePage);
      persist();
      if (closedActive !== null) {
        await leaveClosedActive(closedActive);
      }
      return true;
    },

    togglePin(key) {
      const index = state.tabs.findIndex((t) => t.key === key);
      const tab = state.tabs[index];
      if (tab === undefined || tab.routeName === options.homeRouteName) {
        return;
      }
      state.tabs.splice(index, 1);
      const boundary = state.tabs.findIndex((t) => !t.pinned);
      const end = boundary === -1 ? state.tabs.length : boundary;
      state.tabs.splice(tab.pinned ? end : 0, 0, { ...tab, pinned: !tab.pinned });
      persist();
    },

    move(from, to) {
      const tab = state.tabs[from];
      const target = state.tabs[to];
      if (tab === undefined || target === undefined || tab.pinned !== target.pinned || from === to) {
        return;
      }
      state.tabs.splice(from, 1);
      state.tabs.splice(to, 0, tab);
      persist();
    },

    async reload(key) {
      const index = state.tabs.findIndex((t) => t.key === key);
      const tab = state.tabs[index];
      if (tab === undefined || !(await ensureCanLeave([key]))) {
        return false;
      }
      state.tabs.splice(index, 1, { ...tab, generation: tab.generation + 1 });
      createPage(key);
      return true;
    },

    onEpochChange() {
      state.tabs = state.tabs.map((tab) => {
        if (statusOf(tab.key) !== 'free') {
          return tab;
        }
        createPage(tab.key);
        return { ...tab, generation: tab.generation + 1 };
      });
    },

    canLeaveAll: () => ensureCanLeave(state.tabs.map((t) => t.key)),

    async beforeNavigate(to) {
      const target = resolve(to);
      if (target === null || state.tabs.some((t) => t.key === target.key) || state.tabs.length < options.maxTabs()) {
        return true;
      }
      // 优先建议关闭最久未访问、未固定且无未保存变更的标签页
      const replaceable = (t: Tab | undefined): t is Tab => t !== undefined && !t.pinned && statusOf(t.key) === 'free';
      const candidate =
        state.visited.map((key) => state.tabs.find((t) => t.key === key)).find(replaceable) ??
        state.tabs.find(replaceable);
      if (
        !(await options.confirmReplace(candidate ? { key: candidate.key, title: titleOf(candidate) } : null)) ||
        !candidate
      ) {
        return false;
      }
      state.tabs = state.tabs.filter((t) => t.key !== candidate.key);
      state.visited = state.visited.filter((key) => key !== candidate.key);
      removePage(candidate.key);
      return true;
    },

    commit(to) {
      const target = resolve(to);
      if (target === null) {
        state.activeKey = null;
        return;
      }
      if (!state.tabs.some((t) => t.key === target.key)) {
        insert({
          key: target.key,
          routeName: to.name as string,
          params: toParams(to.params),
          instance: target.instance,
          pinned: to.name === options.homeRouteName,
          generation: 0,
        });
      }
      state.activeKey = target.key;
      state.visited = [...state.visited.filter((key) => key !== target.key), target.key];
      persist();
    },

    reset() {
      pages.value.forEach((page) => {
        page.destroy();
      });
      pages.value = new Map();
      state.tabs = [];
      state.visited = [];
      state.activeKey = null;
    },

    ensureHome() {
      if (!state.tabs.some((t) => t.routeName === options.homeRouteName)) {
        const key = catalog.tabKey(options.homeRouteName, {});
        insert({ key, routeName: options.homeRouteName, params: {}, instance: null, pinned: true, generation: 0 });
      }
    },

    restore() {
      const userId = options.userId();
      if (options.storage === null || userId === null) {
        return;
      }
      for (const stored of loadStoredTabs(options.storage, userId)) {
        if (catalog.find(stored.routeName) === undefined) {
          continue;
        }
        const key = catalog.tabKey(stored.routeName, stored.params, stored.instance ?? '');
        if (!state.tabs.some((t) => t.key === key)) {
          insert({ key, ...stored, generation: 0 });
        }
      }
    },
  };
  return workspace;
}

function locationOf(tab: Tab): RouteLocationRaw {
  return {
    name: tab.routeName,
    params: { ...tab.params },
    ...(tab.instance === null ? {} : { query: { [instanceQuery]: tab.instance } }),
  };
}

function toParams(raw: RouteLocationNormalized['params']): RouteParams {
  return Object.fromEntries(
    Object.entries(raw).map(([name, value]) => [name, Array.isArray(value) ? value.join('/') : value]),
  );
}
