import type { MenuGroup } from '@/platform/routing';

export const demoGroups: readonly MenuGroup[] = [
  { key: 'demo-sales', title: '销售（示例）', icon: 'briefcase', order: 10 },
  { key: 'demo-orders', title: '订单', icon: 'clipboard', order: 10, parent: 'demo-sales' },
  { key: 'demo-system', title: '系统（示例）', icon: 'settings', order: 90 },
];
