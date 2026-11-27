import type { MenuNode } from '@/platform/routing';
import type { IconName } from '@/platform/ui/icons';

export interface SearchEntry {
  routeName: string;
  title: string;
  icon?: IconName;
  /** 所属各级上级目录标题 */
  folders: readonly string[];
}

export interface SearchHit {
  entry: SearchEntry;
  /** 标题中命中字符的索引列表，用于高亮渲染 */
  indices: readonly number[];
}

export type Matcher = (text: string, keyword: string) => readonly number[] | null;

/** 按菜单树顺序扁平化提取所有可搜索页面 */
export function toSearchEntries(nodes: readonly MenuNode[], folders: readonly string[] = []): SearchEntry[] {
  return nodes.flatMap((node) =>
    node.routeName === undefined
      ? toSearchEntries(node.children, [...folders, node.title])
      : [{ routeName: node.routeName, title: node.title, ...(node.icon ? { icon: node.icon } : {}), folders }],
  );
}

/** 按顺序进行子序列匹配（不要求连续，忽略大小写） */
export const matchInOrder: Matcher = (text, keyword) => {
  const source = text.toLowerCase();
  const indices: number[] = [];
  let from = 0;
  for (const char of keyword.toLowerCase()) {
    const index = source.indexOf(char, from);
    if (index < 0) {
      return null;
    }
    indices.push(index);
    from = index + char.length;
  }
  return indices;
};

const isContiguous = (indices: readonly number[]): boolean =>
  indices.every((index, i) => i === 0 || index === (indices[i - 1] ?? 0) + 1);

/** 执行菜单搜索：连续命中结果优先排序，其余保留原菜单顺序 */
export function searchMenu(entries: readonly SearchEntry[], keyword: string, match: Matcher): SearchHit[] {
  const text = keyword.replaceAll(/\s+/g, '');
  if (text === '') {
    return [];
  }
  const hits = entries.flatMap((entry) => {
    const indices = match(entry.title, text);
    return indices === null ? [] : [{ entry, indices }];
  });
  const rank = (hit: SearchHit): number => (isContiguous(hit.indices) ? 0 : 1);
  return hits.sort((a, b) => rank(a) - rank(b));
}

/** 懒加载拼音匹配器，同时支持全拼、首字母及原字符子序列匹配 */
export async function loadPinyinMatcher(): Promise<Matcher> {
  const { match } = await import('pinyin-pro');
  return (text, keyword) => match(text, keyword) ?? matchInOrder(text, keyword);
}
