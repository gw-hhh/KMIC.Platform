import { useCurrentUserStore } from '@/platform/session/current-user';
import { formatDateTime, timeZoneNote } from './date-time';

const browserTimeZone = (): string => Intl.DateTimeFormat().resolvedOptions().timeZone;

/** 按当前用户的业务时区格式化，不按浏览器所在时区 */
export function useFormat() {
  const currentUser = useCurrentUserStore();
  const zone = (): string => currentUser.me?.businessTimeZone ?? browserTimeZone();

  return {
    /** 到分钟；浏览器时区不同时在后面标注业务时区 */
    dateTime(value: string): string {
      const note = timeZoneNote(value, zone(), browserTimeZone());
      const text = formatDateTime(value, zone());
      return note === null ? text : `${text}（${note}）`;
    },
  };
}
