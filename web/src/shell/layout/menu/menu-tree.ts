import type { Crumb, MenuNode } from '@/platform/routing';
import type { IconName } from '@/platform/ui/icons';

interface CrumbPage {
  title: string;
  routeName: string;
  icon?: IconName;
}

/** 面包屑的一级，目录带上它下面的页面，用于下拉切换 */
export interface CrumbView extends Crumb {
  pages: readonly CrumbPage[];
}

function contains(node: MenuNode, routeName: string): boolean {
  return node.routeName === routeName || node.children.some((child) => contains(child, routeName));
}

/** 页面所在的一级目录 */
export function rootOf(nodes: readonly MenuNode[], routeName: string | null): MenuNode | undefined {
  return routeName === null ? undefined : nodes.find((node) => contains(node, routeName));
}

function find(nodes: readonly MenuNode[], key: string): MenuNode | undefined {
  for (const node of nodes) {
    const found = node.key === key ? node : find(node.children, key);
    if (found) {
      return found;
    }
  }
  return undefined;
}

function pagesOf(node: MenuNode): CrumbPage[] {
  return node.children.flatMap((child) =>
    child.routeName === undefined
      ? pagesOf(child)
      : [{ title: child.title, routeName: child.routeName, icon: child.icon }],
  );
}

export function toCrumbViews(crumbs: readonly Crumb[], nodes: readonly MenuNode[]): CrumbView[] {
  return crumbs.map((crumb) => {
    const node = crumb.key === undefined ? undefined : find(nodes, crumb.key);
    return { ...crumb, pages: node ? pagesOf(node) : [] };
  });
}
