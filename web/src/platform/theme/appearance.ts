import { formatHsl, type Hsl } from './color';
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
  const sidebar = appearance.darkSidebar ? dark : colors;
  const header = appearance.darkHeader ? dark : colors;

  return {
    '--kmic-color-background': formatHsl(colors.background),
    '--kmic-color-background-deep': formatHsl(colors.backgroundDeep),
    '--kmic-color-foreground': formatHsl(colors.foreground),
    '--kmic-color-muted-foreground': formatHsl(colors.mutedForeground),
    '--kmic-color-accent': formatHsl(colors.accent),
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
    '--kmic-sidebar-border': formatHsl(sidebar.border),
    '--kmic-header-background': formatHsl(header.background),
    '--kmic-header-foreground': formatHsl(header.foreground),
    '--kmic-header-accent': formatHsl(header.accent),
    '--kmic-header-border': formatHsl(header.border),
    '--kmic-radius': px(appearance.radius),
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
