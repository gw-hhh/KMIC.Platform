import { theme as antdTheme, type ThemeConfig } from 'antdv-next';
import { primaryColor, type Appearance, type ThemeRuntime } from './appearance';
import { formatHsl } from './color';
import { colorSets } from './tokens';

// 组件库要用具体色值，不能传 var()
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
      colorTextBase: formatHsl(colors.foreground),
      colorBorder: formatHsl(colors.border),
      colorError: formatHsl(colors.destructive),
      colorWarning: formatHsl(colors.warning),
      colorSuccess: formatHsl(colors.success),
      borderRadius: appearance.radius,
      fontSize: appearance.fontSize,
      motion: !runtime.reducedMotion,
    },
  };
}
