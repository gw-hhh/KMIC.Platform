import dayjs from 'dayjs';
import timezone from 'dayjs/plugin/timezone';
import utc from 'dayjs/plugin/utc';

dayjs.extend(utc);
dayjs.extend(timezone);

/** 将接口返回的 ISO 时间戳（含偏移量）按指定业务时区格式化至分钟 */
export function formatDateTime(value: string, timeZone: string): string {
  return dayjs(value).tz(timeZone).format('YYYY-MM-DD HH:mm');
}

/** 当浏览器时区与业务时区在该时刻的偏移量不一致时，返回如“UTC+8”的时区标注 */
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
