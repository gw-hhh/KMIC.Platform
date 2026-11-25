import { describe, expect, it } from 'vitest';
import { enumLabel } from './enum-label';

describe('枚举显示名', () => {
  it('按生成的显示名显示，没有的编码原样显示', () => {
    expect(enumLabel('BackgroundJobStatus', 'Running')).toBe('运行中');
    expect(enumLabel('BackgroundJobStatus', 'Paused')).toBe('Paused');
  });
});
