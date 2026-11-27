import { describe, expect, it } from 'vitest';
import { contrastRatio, formatHsl } from './color';
import { primaryPresets } from './tokens';

describe('颜色计算', () => {
  it('纯黑与纯白的对比度为 21', () => {
    const ratio = contrastRatio({ h: 0, s: 0, l: 0 }, { h: 0, s: 0, l: 100 });

    expect(ratio).toBeCloseTo(21, 1);
  });

  it('正确格式化为 CSS hsl 字符串', () => {
    expect(formatHsl({ h: 212, s: 100, l: 45 })).toBe('hsl(212 100% 45%)');
  });
});

describe('主题色预设', () => {
  const white = { h: 0, s: 0, l: 100 };

  it.each(primaryPresets.map((preset) => [preset.name, preset] as const))(
    '%s 背景与白色文字的对比度不低于 4.5',
    (_, preset) => {
      expect(contrastRatio(preset.color, white)).toBeGreaterThanOrEqual(4.5);
    },
  );

  it('预设主题色的键名互不重复', () => {
    const keys = primaryPresets.map((preset) => preset.key);

    expect(new Set(keys).size).toBe(keys.length);
  });
});
