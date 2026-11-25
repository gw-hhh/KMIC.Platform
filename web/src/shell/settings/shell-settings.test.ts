import { describe, expect, it } from 'vitest';
import { defaultShellSettings, parseShellSettings, serializeShellSettings } from './shell-settings';

describe('外壳设置的读取', () => {
  it('没有保存过时用默认值', () => {
    expect(parseShellSettings(undefined)).toEqual(defaultShellSettings);
    expect(parseShellSettings({})).toEqual(defaultShellSettings);
  });

  it('保存的值读回来不变', () => {
    const settings = {
      ...defaultShellSettings,
      layout: 'mixed-column' as const,
      appearance: { ...defaultShellSettings.appearance, mode: 'dark' as const, primary: 'rose', radius: 12 as const },
      tabs: { ...defaultShellSettings.tabs, style: 'card' as const, max: 20 },
    };

    expect(parseShellSettings(serializeShellSettings(settings))).toEqual(settings);
  });

  it('不合法的字段取默认值，其余字段照常读', () => {
    const parsed = parseShellSettings({
      layout: 'hologram',
      appearance: { mode: 'dark', primary: 'no-such-color', radius: 5 },
      sidebar: { width: 9999, collapsed: true },
      tabs: { max: 5, style: 'card' },
    });

    expect(parsed.layout).toBe('vertical');
    expect(parsed.appearance.mode).toBe('dark');
    expect(parsed.appearance.primary).toBe('blue');
    expect(parsed.appearance.radius).toBe(8);
    expect(parsed.sidebar.width).toBe(224);
    expect(parsed.sidebar.collapsed).toBe(true);
    expect(parsed.tabs.max).toBe(15);
    expect(parsed.tabs.style).toBe('card');
  });

  it('未知字段忽略', () => {
    const parsed = parseShellSettings({ futureFeature: { enabled: true }, layout: 'mixed-vertical' });

    expect(parsed.layout).toBe('mixed-vertical');
    expect(parsed).not.toHaveProperty('futureFeature');
  });

  it('不是对象时用默认值', () => {
    expect(parseShellSettings('oops')).toEqual(defaultShellSettings);
    expect(parseShellSettings(null)).toEqual(defaultShellSettings);
  });
});
