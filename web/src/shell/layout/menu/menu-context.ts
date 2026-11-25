import { inject, provide, type InjectionKey, type Ref } from 'vue';

export interface MenuContext {
  activeKey: Readonly<Ref<string | null>>;
  /** 当前页面所在的各级目录 */
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
    throw new Error('菜单项只能放在 SidebarMenu 里');
  }
  return context;
}
