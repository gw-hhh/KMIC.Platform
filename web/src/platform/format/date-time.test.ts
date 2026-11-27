import { describe, expect, it } from 'vitest';
import { formatDateTime, timeZoneNote } from './date-time';

describe('时间格式化与时区说明', () => {
  it('按业务时区换算显示，不受浏览器本地时区影响', () => {
    expect(formatDateTime('2026-11-25T16:30:00Z', 'Asia/Shanghai')).toBe('2026-11-26 00:30');
    expect(formatDateTime('2026-11-25T16:30:00+08:00', 'Asia/Shanghai')).toBe('2026-11-25 16:30');
  });

  it('浏览器时区与业务时区偏移不同时才标注', () => {
    expect(timeZoneNote('2026-11-25T08:00:00Z', 'Asia/Shanghai', 'Asia/Shanghai')).toBeNull();
    expect(timeZoneNote('2026-11-25T08:00:00Z', 'Asia/Shanghai', 'Asia/Hong_Kong')).toBeNull();
    expect(timeZoneNote('2026-11-25T08:00:00Z', 'Asia/Shanghai', 'Europe/Berlin')).toBe('UTC+8');
    expect(timeZoneNote('2026-11-25T08:00:00Z', 'Asia/Kolkata', 'Asia/Shanghai')).toBe('UTC+5:30');
    expect(timeZoneNote('2026-11-25T08:00:00Z', 'America/New_York', 'Asia/Shanghai')).toBe('UTC-5');
  });
});
