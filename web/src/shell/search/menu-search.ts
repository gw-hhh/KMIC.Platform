import type { MenuNode } from '@/platform/routing';
import type { IconName } from '@/platform/ui/icons';

/** 可以搜到的页面 */
export interface SearchEntry {
  routeName: string;
  title: string;
  icon?: IconName;
  /** 上级目录的标题 */
  folders: readonly string[];
}

export interface SearchHit {
  entry: SearchEntry;
  /** 标题中命中的字的位置，用于高亮 */
  indices: readonly number[];
}

/** 命中时返回标题中命中的字的位置 */
export type Matcher = (text: string, keyword: string) => readonly number[] | null;

/** 按菜单顺序列出所有页面 */
export function toSearchEntries(nodes: readonly MenuNode[], folders: readonly string[] = []): SearchEntry[] {
  return nodes.flatMap((node) =>
    node.routeName === undefined
      ? toSearchEntries(node.children, [...folders, node.title])
      : [{ routeName: node.routeName, title: node.title, ...(node.icon ? { icon: node.icon } : {}), folders }],
  );
}

/** 关键字的字依次出现在标题里即命中，不要求相邻，不分大小写 */
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

/** 连续命中的排在前面，其余按菜单顺序 */
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

/** 再支持拼音全拼和首字母，第一次打开搜索时才加载拼音库 */
export async function loadPinyinMatcher(): Promise<Matcher> {
  const { match } = await import('pinyin-pro');
  return (text, keyword) => match(text, keyword) ?? matchInOrder(text, keyword);
}
