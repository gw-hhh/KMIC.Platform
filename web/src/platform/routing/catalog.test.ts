import { describe, expect, it } from 'vitest';
import { createRouteCatalog } from './catalog';
import { defineModuleRoutes } from './define-routes';
import type { MenuGroup, RouteEntry } from './types';

const page = () => Promise.resolve({ default: {} });

const groups: MenuGroup[] = [
  { key: 'sales', title: '销售', icon: 'briefcase', order: 20 },
  { key: 'sales-orders', title: '订单', icon: 'clipboard', order: 10, parent: 'sales' },
  { key: 'system', title: '系统', icon: 'settings', order: 90 },
];

function entry(overrides: Partial<RouteEntry> & { name: string; path: string }): RouteEntry {
  return { title: overrides.name, requires: [], tab: { mode: 'singleton' }, component: page, ...overrides };
}

const entries: RouteEntry[] = [
  entry({
    name: 'sample.list',
    path: '/sample/samples',
    title: '示例列表',
    menu: { group: 'sales-orders', order: 10 },
    requires: ['Sample.Specimen.View'],
  }),
  entry({
    name: 'sample.detail',
    path: '/sample/samples/:id',
    title: '示例详情',
    tab: { mode: 'byParams', params: ['id'] },
    parent: 'sample.list',
    requires: ['Sample.Specimen.View'],
  }),
  entry({
    name: 'sample.create',
    path: '/sample/new',
    title: '新建示例',
    tab: { mode: 'multiple' },
    requires: ['Sample.Specimen.Create'],
  }),
  entry({
    name: 'system.users',
    path: '/system/users',
    title: '用户',
    menu: { group: 'system', order: 10 },
    requires: ['Identity.User.View'],
  }),
];

describe('路由目录', () => {
  it('按权限生成菜单，没有可见条目的目录整个隐藏', () => {
    const catalog = createRouteCatalog(entries, groups);

    const menu = catalog.menu((permission) => permission === 'Sample.Specimen.View');

    expect(menu.map((node) => node.title)).toEqual(['销售']);
    expect(menu[0]?.children[0]?.title).toBe('订单');
    expect(menu[0]?.children[0]?.children[0]?.routeName).toBe('sample.list');
  });

  it('菜单按 order 排序', () => {
    const catalog = createRouteCatalog(entries, groups);

    const menu = catalog.menu(() => true);

    expect(menu.map((node) => node.title)).toEqual(['销售', '系统']);
  });

  it('标签键按模式计算', () => {
    const catalog = createRouteCatalog(entries, groups);

    expect(catalog.tabKey('sample.list', {})).toBe('sample.list');
    expect(catalog.tabKey('sample.detail', { id: '7' })).toBe('sample.detail?id=7');
    expect(catalog.tabKey('sample.create', {}, 'a1')).toBe('sample.create#a1');
  });

  it('面包屑含目录和上级页面', () => {
    const catalog = createRouteCatalog(entries, groups);

    expect(catalog.breadcrumb('sample.list').map((crumb) => crumb.title)).toEqual(['销售', '订单', '示例列表']);
    expect(catalog.breadcrumb('sample.detail').map((crumb) => crumb.title)).toEqual(['示例列表', '示例详情']);
  });

  it('一次列出目录里所有的问题', () => {
    const broken = [
      entry({ name: 'a.one', path: '/a' }),
      entry({ name: 'a.one', path: '/b', menu: { group: 'nowhere', order: 1 } }),
      entry({ name: 'a.two', path: '/a', parent: 'a.missing' }),
      entry({ name: 'a.three', path: '/c', tab: { mode: 'byParams', params: ['id'] } }),
    ];

    expect(() => createRouteCatalog(broken, groups)).toThrow(/路由名 a\.one 重复/);
    expect(() => createRouteCatalog(broken, groups)).toThrow(/都是 \/a/);
    expect(() => createRouteCatalog(broken, groups)).toThrow(/目录 nowhere/);
    expect(() => createRouteCatalog(broken, groups)).toThrow(/a\.missing/);
    expect(() => createRouteCatalog(broken, groups)).toThrow(/参数 id/);
  });
});

describe('定义模块路由', () => {
  it('名字必须以模块名开头', () => {
    expect(() => defineModuleRoutes('sample', [entry({ name: 'other.list', path: '/x' })])).toThrow(/sample\./);
  });
});
