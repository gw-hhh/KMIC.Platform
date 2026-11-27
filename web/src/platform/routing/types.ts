import type { Component } from 'vue';
import type { PermissionCode } from '@/generated/permissions';
import type { IconName } from '@/platform/ui/icons';

export type RouteParams = Readonly<Record<string, string>>;

/** 标签页打开模式：singleton 全局单例（列表）；byParams 按参数区分实例（详情）；multiple 每次新建实例（新建） */
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
  /** 路由名称，格式为 `模块.页面` */
  name: string;
  path: string;
  title: string;
  icon?: IconName;
  /** 菜单配置；省略则不在侧栏菜单中显示 */
  menu?: MenuPlacement;
  /** 所需权限列表（需全部满足）；无需权限时传 `[]` */
  requires: readonly PermissionCode[];
  tab: TabMode;
  /** 面包屑导航的上级路由名称 */
  parent?: string;
  component: PageLoader;
}

export interface MenuNode {
  key: string;
  title: string;
  icon?: IconName;
  /** 仅页面节点包含此字段 */
  routeName?: string;
  children: readonly MenuNode[];
}

export interface Crumb {
  title: string;
  /** 菜单目录节点的唯一键 */
  key?: string;
  icon?: IconName;
  /** 仅页面节点包含此字段 */
  routeName?: string;
}
