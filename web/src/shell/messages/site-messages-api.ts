import type { components } from '@/generated/api';
import { defineModuleApi } from '@/platform/api';

export type SiteMessage = components['schemas']['SiteMessageItem'];

/** 本人的站内消息；模块名与 Notifications 相同，缓存与它的页面共用 */
export const siteMessagesApi = defineModuleApi('notifications', ({ query, command }) => ({
  list: query('/api/v1/notifications/site-messages'),
  unreadCount: query('/api/v1/notifications/site-messages/unread-count'),
  markRead: command('POST', '/api/v1/notifications/site-messages/{id}/read'),
  markAllRead: command('POST', '/api/v1/notifications/site-messages/read-all'),
}));

/** 只接受站内的相对路径，防止消息里的链接把人带到外站 */
export function isInternalLink(link: string): boolean {
  return link.startsWith('/') && !link.startsWith('//') && !link.startsWith('/\\');
}
