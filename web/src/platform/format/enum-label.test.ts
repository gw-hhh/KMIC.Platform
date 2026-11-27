import { describe, expect, it } from 'vitest';
import { enumLabel } from './enum-label';

describe('枚举显示名', () => {
  it('命中已生成的枚举映射时返回中文名称，未命中时原样返回编码', () => {
    expect(enumLabel('BackgroundJobStatus', 'Running')).toBe('运行中');
    expect(enumLabel('BackgroundJobStatus', 'Paused')).toBe('Paused');
  });
});
