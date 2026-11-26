import { formatHsl, formatHslAlpha, type Hsl } from './color';
import {
  colorSets,
  motionDurations,
  motionEasings,
  primaryPresets,
  shellSizes,
  zIndexes,
  type FontSize,
  type PrimaryKey,
  type RadiusStep,
  type ThemeMode,
} from './tokens';

export interface Appearance {
  mode: ThemeMode | 'system';
  primary: PrimaryKey;
  radius: RadiusStep;
  fontSize: FontSize;
  density: 'default' | 'compact';
  colorWeak: boolean;
  darkSidebar: boolean;
  darkHeader: boolean;
}

const defaultPrimary: Hsl = { h: 212, s: 100, l: 45 };

export const defaultAppearance: Appearance = {
  mode: 'light',
  primary: 'blue',
  radius: 8,
  fontSize: 14,
  density: 'compact',
  colorWeak: false,
  darkSidebar: false,
  darkHeader: false,
};

export interface ThemeRuntime {
  resolvedMode: ThemeMode;
  reducedMotion: boolean;
}

export function primaryColor(key: PrimaryKey): Hsl {
  return primaryPresets.find((preset) => preset.key === key)?.color ?? defaultPrimary;
}

const px = (value: number): string => `${String(value)}px`;
const ms = (value: number, runtime: ThemeRuntime): string => `${String(runtime.reducedMotion ? 0 : value)}ms`;

export function buildCssVariables(appearance: Appearance, runtime: ThemeRuntime): Record<string, string> {
  const colors = colorSets[runtime.resolvedMode];
  const dark = colorSets.dark;
  const primary = formatHsl(primaryColor(appearance.primary));
  const sidebarDark = appearance.darkSidebar || runtime.resolvedMode === 'dark';
  const sidebar = sidebarDark ? dark : colors;
  const header = appearance.darkHeader ? dark : colors;
  const radius = (offset: number): string => px(Math.max(0, appearance.radius - offset));

  return {
    '--kmic-color-background': formatHsl(colors.background),
    '--kmic-color-background-deep': formatHsl(colors.backgroundDeep),
    '--kmic-color-foreground': formatHsl(colors.foreground),
    '--kmic-color-muted-foreground': formatHsl(colors.mutedForeground),
    '--kmic-color-muted': formatHsl(colors.muted),
    // 当前标签：浅色主题主色字、主色淡底；深色主题白字、灰底，和 Vben 一致
    '--kmic-tab-active-color': runtime.resolvedMode === 'dark' ? formatHsl(colors.accentForeground) : primary,
    '--kmic-tab-active-background':
      runtime.resolvedMode === 'dark' ? formatHsl(colors.accent) : `color-mix(in srgb, ${primary} 15%, transparent)`,
    '--kmic-color-accent': formatHsl(colors.accent),
    '--kmic-color-accent-foreground': formatHsl(colors.accentForeground),
    '--kmic-color-accent-hover': formatHsl(colors.accentHover),
    '--kmic-color-border': formatHsl(colors.border),
    '--kmic-color-success': formatHsl(colors.success),
    '--kmic-color-warning': formatHsl(colors.warning),
    '--kmic-color-destructive': formatHsl(colors.destructive),
    '--kmic-color-primary': primary,
    '--kmic-color-primary-foreground': 'hsl(0 0% 100%)',
    '--kmic-color-primary-soft': `color-mix(in srgb, ${primary} 15%, transparent)`,
    '--kmic-sidebar-background': formatHsl(sidebar.background),
    '--kmic-sidebar-foreground': formatHsl(sidebar.foreground),
    '--kmic-sidebar-accent': formatHsl(sidebar.accent),
    '--kmic-sidebar-accent-hover': formatHsl(sidebar.accentHover),
    // 浅色侧栏：选中项主色字、主色淡底；深色侧栏：选中项白字、灰底，和 Vben 一致
    '--kmic-sidebar-item-color': sidebarDark
      ? formatHslAlpha(sidebar.foreground, 80)
      : formatHsl(sidebar.accentForeground),
    '--kmic-sidebar-item-hover-color': formatHsl(sidebar.accentForeground),
    // 侧栏底部按钮这类次要图标
    '--kmic-sidebar-muted-color': formatHslAlpha(sidebar.foreground, 60),
    '--kmic-sidebar-item-active-color': sidebarDark ? formatHsl(sidebar.accentForeground) : primary,
    '--kmic-sidebar-item-active-background': sidebarDark
      ? formatHsl(sidebar.accent)
      : `color-mix(in srgb, ${primary} 15%, transparent)`,
    '--kmic-sidebar-group-active-color': sidebarDark ? formatHsl(sidebar.accentForeground) : primary,
    '--kmic-sidebar-border': formatHsl(sidebar.border),
    '--kmic-header-background': formatHsl(header.background),
    '--kmic-header-foreground': formatHsl(header.foreground),
    '--kmic-header-accent': formatHsl(header.accent),
    '--kmic-header-icon-color': formatHslAlpha(header.foreground, 80),
    '--kmic-header-border': formatHsl(header.border),
    '--kmic-radius': px(appearance.radius),
    '--kmic-radius-md': radius(2),
    '--kmic-radius-sm': radius(4),
    // 两端整圆的胶囊形，不随圆角设置变
    '--kmic-radius-pill': '9999px',
    '--kmic-font-size': px(appearance.fontSize),
    '--kmic-header-height': px(shellSizes.headerHeight),
    '--kmic-tabbar-height': px(shellSizes.tabBarHeight),
    '--kmic-menu-item-height': px(shellSizes.menuItemHeight),
    '--kmic-motion-fast': ms(motionDurations.fast, runtime),
    '--kmic-motion-medium': ms(motionDurations.medium, runtime),
    '--kmic-motion-slow': ms(motionDurations.slow, runtime),
    '--kmic-motion-theme': ms(motionDurations.theme, runtime),
    '--kmic-easing-standard': motionEasings.standard,
    '--kmic-easing-drawer': motionEasings.drawer,
    '--kmic-easing-tab': motionEasings.tab,
    '--kmic-z-mask': String(zIndexes.mask),
    '--kmic-z-sidebar': String(zIndexes.sidebar),
    '--kmic-z-header': String(zIndexes.header),
    '--kmic-z-drawer': String(zIndexes.drawer),
    '--kmic-z-progress': String(zIndexes.progress),
  };
}
