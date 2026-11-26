import { theme as antdTheme, type ThemeConfig } from 'antdv-next';
import { primaryColor, type Appearance, type ThemeRuntime } from './appearance';
import { formatHsl } from './color';
import { colorSets } from './tokens';

// 组件库要用具体色值，不能传 var()。几种底色照 Vben 直接给定，不让深色算法从 colorBgBase 推（推出来偏蓝）
export function buildAntdTheme(appearance: Appearance, runtime: ThemeRuntime): ThemeConfig {
  const colors = colorSets[runtime.resolvedMode];
  const algorithm = [runtime.resolvedMode === 'dark' ? antdTheme.darkAlgorithm : antdTheme.defaultAlgorithm];
  if (appearance.density === 'compact') {
    algorithm.push(antdTheme.compactAlgorithm);
  }

  return {
    algorithm,
    token: {
      colorPrimary: formatHsl(primaryColor(appearance.primary)),
      colorBgBase: formatHsl(colors.background),
      colorBgContainer: formatHsl(colors.background),
      colorBgElevated: formatHsl(colors.popover),
      colorBgLayout: formatHsl(colors.backgroundDeep),
      colorTextBase: formatHsl(colors.foreground),
      colorBorder: formatHsl(colors.border),
      colorBorderSecondary: formatHsl(colors.border),
      colorError: formatHsl(colors.destructive),
      colorWarning: formatHsl(colors.warning),
      colorSuccess: formatHsl(colors.success),
      borderRadius: appearance.radius,
      fontSize: appearance.fontSize,
      motion: !runtime.reducedMotion,
    },
  };
}
