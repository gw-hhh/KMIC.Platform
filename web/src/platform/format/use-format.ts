import { useCurrentUserStore } from '@/platform/session/current-user';
import { formatDateTime, timeZoneNote } from './date-time';

const browserTimeZone = (): string => Intl.DateTimeFormat().resolvedOptions().timeZone;

/** 按当前用户的业务时区格式化日期时间，而非浏览器本地时区 */
export function useFormat() {
  const currentUser = useCurrentUserStore();
  const zone = (): string => currentUser.me?.businessTimeZone ?? browserTimeZone();

  return {
    /** 精确至分钟；若与浏览器时区不一致，在末尾附加业务时区标注 */
    dateTime(value: string): string {
      const note = timeZoneNote(value, zone(), browserTimeZone());
      const text = formatDateTime(value, zone());
      return note === null ? text : `${text}（${note}）`;
    },
  };
}
