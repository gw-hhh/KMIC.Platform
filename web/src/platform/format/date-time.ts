import dayjs from 'dayjs';
import timezone from 'dayjs/plugin/timezone';
import utc from 'dayjs/plugin/utc';

dayjs.extend(utc);
dayjs.extend(timezone);

/** 接口里的时刻（带偏移的 ISO 字符串）按业务时区显示到分钟 */
export function formatDateTime(value: string, timeZone: string): string {
  return dayjs(value).tz(timeZone).format('YYYY-MM-DD HH:mm');
}

/** 浏览器所在时区和业务时区在这个时刻的偏移不同时，返回“UTC+8”这样的标注 */
export function timeZoneNote(value: string, timeZone: string, browserTimeZone: string): string | null {
  const at = dayjs(value);
  const offset = at.tz(timeZone).utcOffset();
  if (offset === at.tz(browserTimeZone).utcOffset()) {
    return null;
  }
  const hours = Math.trunc(Math.abs(offset) / 60);
  const minutes = Math.abs(offset) % 60;
  const sign = offset < 0 ? '-' : '+';
  return `UTC${sign}${String(hours)}${minutes === 0 ? '' : `:${String(minutes).padStart(2, '0')}`}`;
}
