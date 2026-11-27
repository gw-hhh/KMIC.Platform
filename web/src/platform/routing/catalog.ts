import type { PermissionCode } from '@/generated/permissions';
import type { Crumb, MenuGroup, MenuNode, RouteEntry, RouteParams } from './types';

export interface RouteCatalog {
  readonly entries: readonly RouteEntry[];
  readonly groups: readonly MenuGroup[];
  find(name: string): RouteEntry | undefined;
  /** 生成标签页唯一键；相同键仅保留单个标签页 */
  tabKey(name: string, params: RouteParams, instance?: string): string;
  menu(has: (permission: PermissionCode) => boolean): readonly MenuNode[];
  breadcrumb(name: string): readonly Crumb[];
}

/** 汇总各模块路由与菜单分组并执行完整性校验 */
export function createRouteCatalog(entries: readonly RouteEntry[], groups: readonly MenuGroup[]): RouteCatalog {
  const problems = validate(entries, groups);
  if (problems.length > 0) {
    throw new Error(`路由目录校验失败：\n${problems.map((problem) => `- ${problem}`).join('\n')}`);
  }

  const byName = new Map(entries.map((entry) => [entry.name, entry]));

  return {
    entries,
    groups,
    find: (name) => byName.get(name),

    tabKey(name, params, instance) {
      const entry = byName.get(name);
      if (!entry) {
        throw new Error(`路由 ${name} 不在目录中`);
      }
      switch (entry.tab.mode) {
        case 'singleton':
          return name;
        case 'byParams':
          return `${name}?${entry.tab.params.map((param) => `${param}=${params[param] ?? ''}`).join('&')}`;
        case 'multiple':
          return `${name}#${instance ?? ''}`;
      }
    },

    menu: (has) => buildMenu(entries, groups, has),

    breadcrumb(name) {
      const pages: Crumb[] = [];
      for (let entry = byName.get(name); entry; entry = entry.parent ? byName.get(entry.parent) : undefined) {
        pages.unshift({ title: entry.title, routeName: entry.name, ...(entry.icon ? { icon: entry.icon } : {}) });
      }
      const folders = groupChain(byName.get(name)?.menu?.group, groups).map((group) => ({
        title: group.title,
        icon: group.icon,
        key: `group:${group.key}`,
      }));
      return [...folders, ...pages];
    },
  };
}

function validate(entries: readonly RouteEntry[], groups: readonly MenuGroup[]): string[] {
  const problems: string[] = [];
  const names = new Set<string>();
  const paths = new Map<string, string>();
  const groupKeys = new Set<string>();

  for (const group of groups) {
    if (groupKeys.has(group.key)) {
      problems.push(`菜单目录 ${group.key} 重复`);
    }
    groupKeys.add(group.key);
  }
  for (const group of groups) {
    if (group.parent !== undefined && !groupKeys.has(group.parent)) {
      problems.push(`菜单目录 ${group.key} 的上级目录 ${group.parent} 未声明`);
    }
  }

  for (const entry of entries) {
    if (names.has(entry.name)) {
      problems.push(`路由名 ${entry.name} 重复`);
    }
    names.add(entry.name);

    const other = paths.get(entry.path);
    if (other !== undefined) {
      problems.push(`路由 ${entry.name} 和 ${other} 的路径都是 ${entry.path}`);
    }
    paths.set(entry.path, entry.name);

    if (entry.menu && !groupKeys.has(entry.menu.group)) {
      problems.push(`路由 ${entry.name} 关联的菜单目录 ${entry.menu.group} 未声明`);
    }
    if (entry.tab.mode === 'byParams') {
      for (const param of entry.tab.params.filter((p) => !entry.path.includes(`:${p}`))) {
        problems.push(`路由 ${entry.name} 按参数 ${param} 区分标签页，但路径 ${entry.path} 中缺少该参数`);
      }
    }
  }

  for (const entry of entries) {
    if (entry.parent !== undefined && !names.has(entry.parent)) {
      problems.push(`路由 ${entry.name} 的上级路由 ${entry.parent} 不在目录中`);
    }
  }
  return problems;
}

function groupChain(key: string | undefined, groups: readonly MenuGroup[]): MenuGroup[] {
  const chain: MenuGroup[] = [];
  let group = groups.find((g) => g.key === key);
  while (group) {
    chain.unshift(group);
    const parent = group.parent;
    group = groups.find((g) => g.key === parent);
  }
  return chain;
}

function buildMenu(
  entries: readonly RouteEntry[],
  groups: readonly MenuGroup[],
  has: (permission: PermissionCode) => boolean,
): readonly MenuNode[] {
  const visible = entries.filter((entry) => entry.menu && entry.requires.every(has));

  const build = (parent: string | undefined): MenuNode[] => {
    const items: { order: number; node: MenuNode }[] = [];

    for (const group of groups.filter((g) => g.parent === parent)) {
      const children = build(group.key);
      // 隐藏无可见子项的菜单目录
      if (children.length > 0) {
        items.push({
          order: group.order,
          node: { key: `group:${group.key}`, title: group.title, icon: group.icon, children },
        });
      }
    }
    if (parent !== undefined) {
      for (const entry of visible.filter((e) => e.menu?.group === parent)) {
        items.push({
          order: entry.menu?.order ?? 0,
          node: { key: entry.name, title: entry.title, icon: entry.icon, routeName: entry.name, children: [] },
        });
      }
    }
    return items.sort((a, b) => a.order - b.order).map((item) => item.node);
  };

  return build(undefined);
}
