import type { Component } from 'vue';
import type { PermissionCode } from '@/generated/permissions';
import type { IconName } from '@/platform/ui/icons';

export type RouteParams = Readonly<Record<string, string>>;

/**
 * singleton 只开一个（列表）；byParams 按参数各开一个（详情）；multiple 每次新开（新建）
 */
export type TabMode =
  | { readonly mode: 'singleton' }
  | { readonly mode: 'byParams'; readonly params: readonly string[] }
  | { readonly mode: 'multiple' };

export interface MenuGroup {
  key: string;
  title: string;
  icon: IconName;
  order: number;
  parent?: string;
}

export interface MenuPlacement {
  group: string;
  order: number;
}

export type PageLoader = () => Promise<{ default: Component }>;

export interface RouteEntry {
  /** `模块.页面` */
  name: string;
  path: string;
  title: string;
  icon?: IconName;
  /** 不写则不出现在菜单中 */
  menu?: MenuPlacement;
  /** 需全部具备；不需要权限写 `[]` */
  requires: readonly PermissionCode[];
  tab: TabMode;
  /** 面包屑的上一级 */
  parent?: string;
  component: PageLoader;
}

export interface MenuNode {
  key: string;
  title: string;
  icon?: IconName;
  /** 只有页面有 */
  routeName?: string;
  children: readonly MenuNode[];
}

export interface Crumb {
  title: string;
  /** 目录的菜单键 */
  key?: string;
  icon?: IconName;
  /** 只有页面有 */
  routeName?: string;
}
