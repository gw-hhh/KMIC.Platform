import { inject, provide, type InjectionKey, type Ref } from 'vue';

export interface MenuContext {
  activeKey: Readonly<Ref<string | null>>;
  /** 当前活动页面所属的各级祖先目录键集合 */
  activeAncestors: Readonly<Ref<ReadonlySet<string>>>;
  openKeys: Readonly<Ref<ReadonlySet<string>>>;
  collapsed: Readonly<Ref<boolean>>;
  showCollapsedTitle: Readonly<Ref<boolean>>;
  toggle(groupKey: string): void;
  select(routeName: string): void;
}

const menuContextKey: InjectionKey<MenuContext> = Symbol('kmic.menu');

export function provideMenu(context: MenuContext): void {
  provide(menuContextKey, context);
}

export function useMenu(): MenuContext {
  const context = inject(menuContextKey, null);
  if (context === null) {
    throw new Error('MenuItem 必须在 SidebarMenu 组件内部使用');
  }
  return context;
}
