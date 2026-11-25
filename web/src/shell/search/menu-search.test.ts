import { describe, expect, it } from 'vitest';
import type { MenuNode } from '@/platform/routing';
import { loadPinyinMatcher, matchInOrder, searchMenu, toSearchEntries } from './menu-search';

const page = (routeName: string, title: string): MenuNode => ({ key: routeName, title, routeName, children: [] });

const nodes: MenuNode[] = [
  {
    key: 'sales',
    title: '销售',
    children: [page('orders', '订单列表'), { key: 'reports', title: '报表', children: [page('ar-aging', 'AR 账龄')] }],
  },
  page('list-orders', '列出订单'),
];

describe('菜单搜索', () => {
  it('按菜单顺序列出页面，带上级目录，目录本身不算', () => {
    expect(toSearchEntries(nodes).map((entry) => [entry.routeName, entry.folders])).toEqual([
      ['orders', ['销售']],
      ['ar-aging', ['销售', '报表']],
      ['list-orders', []],
    ]);
  });

  it('字依次出现即命中，不要求相邻，不分大小写', () => {
    expect(matchInOrder('订单列表', '订表')).toEqual([0, 3]);
    expect(matchInOrder('AR 账龄', 'ar')).toEqual([0, 1]);
    expect(matchInOrder('订单列表', '表订')).toBeNull();
  });

  it('连续命中的排在前面，忽略关键字里的空格', () => {
    const hits = searchMenu(toSearchEntries(nodes), ' 订 单 ', matchInOrder);

    expect(hits.map((hit) => hit.entry.routeName)).toEqual(['orders', 'list-orders']);
    expect(hits[0]?.indices).toEqual([0, 1]);
  });

  it('关键字为空时没有结果', () => {
    expect(searchMenu(toSearchEntries(nodes), '  ', matchInOrder)).toEqual([]);
  });

  it('拼音全拼、首字母都能命中，也照常按汉字匹配', async () => {
    const match = await loadPinyinMatcher();

    expect(match('订单列表', 'ddlb')).toEqual([0, 1, 2, 3]);
    expect(match('订单列表', 'dingdan')).toEqual([0, 1]);
    expect(match('订单列表', '订表')).toEqual([0, 3]);
    expect(match('AR 账龄', 'zl')).not.toBeNull();
    expect(match('订单列表', 'xyz')).toBeNull();
  });
});
