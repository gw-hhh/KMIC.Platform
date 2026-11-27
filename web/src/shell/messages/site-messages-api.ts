import type { components } from '@/generated/api';
import { defineModuleApi } from '@/platform/api';

export type SiteMessage = components['schemas']['SiteMessageItem'];

/** 当前用户的站内消息接口；与 Notifications 模块共用同一查询缓存键前缀 */
export const siteMessagesApi = defineModuleApi('notifications', ({ query, command }) => ({
  list: query('/api/v1/notifications/site-messages'),
  unreadCount: query('/api/v1/notifications/site-messages/unread-count'),
  markRead: command('POST', '/api/v1/notifications/site-messages/{id}/read'),
  markAllRead: command('POST', '/api/v1/notifications/site-messages/read-all'),
}));

/** 仅允许站内相对路径，防止消息链接跳转至外部站点 */
export function isInternalLink(link: string): boolean {
  return link.startsWith('/') && !link.startsWith('//') && !link.startsWith('/\\');
}
