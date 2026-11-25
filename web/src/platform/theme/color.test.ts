import { describe, expect, it } from 'vitest';
import { contrastRatio, formatHsl } from './color';
import { primaryPresets } from './tokens';

describe('颜色', () => {
  it('黑白对比度是 21', () => {
    const ratio = contrastRatio({ h: 0, s: 0, l: 0 }, { h: 0, s: 0, l: 100 });

    expect(ratio).toBeCloseTo(21, 1);
  });

  it('写成 CSS 的 hsl 值', () => {
    expect(formatHsl({ h: 212, s: 100, l: 45 })).toBe('hsl(212 100% 45%)');
  });
});

describe('主题色预设', () => {
  const white = { h: 0, s: 0, l: 100 };

  it.each(primaryPresets.map((preset) => [preset.name, preset] as const))(
    '%s 上的白字对比度不低于 4.5',
    (_, preset) => {
      expect(contrastRatio(preset.color, white)).toBeGreaterThanOrEqual(4.5);
    },
  );

  it('键不重复', () => {
    const keys = primaryPresets.map((preset) => preset.key);

    expect(new Set(keys).size).toBe(keys.length);
  });
});
