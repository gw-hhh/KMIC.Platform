// 初值取自 Vben Admin（MIT）的默认主题
import type { Hsl } from './color';

export type ThemeMode = 'light' | 'dark';

export interface ColorSet {
  background: Hsl;
  /** 内容区底色 */
  backgroundDeep: Hsl;
  /** 浮层底色：下拉、弹出框、对话框 */
  popover: Hsl;
  foreground: Hsl;
  mutedForeground: Hsl;
  /** 标签栏工具按钮等的悬停底色，比 accent 略深 */
  muted: Hsl;
  /** 悬停底色 */
  accent: Hsl;
  /** 悬停底色上的文字，比正文深一档 */
  accentForeground: Hsl;
  /** 已有悬停底色的元素（如侧栏底部按钮）再悬停时 */
  accentHover: Hsl;
  border: Hsl;
  success: Hsl;
  warning: Hsl;
  destructive: Hsl;
}

export const colorSets: Record<ThemeMode, ColorSet> = {
  light: {
    background: { h: 0, s: 0, l: 100 },
    backgroundDeep: { h: 216, s: 20.11, l: 95.47 },
    popover: { h: 0, s: 0, l: 100 },
    foreground: { h: 210, s: 6, l: 21 },
    mutedForeground: { h: 240, s: 3.8, l: 46.1 },
    muted: { h: 240, s: 4.8, l: 95.9 },
    accent: { h: 240, s: 5, l: 96 },
    accentForeground: { h: 240, s: 6, l: 10 },
    accentHover: { h: 200, s: 10, l: 90 },
    border: { h: 240, s: 5.9, l: 90 },
    success: { h: 144, s: 57, l: 40 },
    warning: { h: 42, s: 84, l: 45 },
    destructive: { h: 359.33, s: 100, l: 60 },
  },
  dark: {
    background: { h: 222.34, s: 10.43, l: 12.27 },
    backgroundDeep: { h: 220, s: 13.06, l: 9 },
    popover: { h: 0, s: 0, l: 14.2 },
    foreground: { h: 0, s: 0, l: 95 },
    mutedForeground: { h: 240, s: 5, l: 64.9 },
    muted: { h: 240, s: 3.7, l: 15.9 },
    accent: { h: 216, s: 5, l: 19 },
    accentForeground: { h: 0, s: 0, l: 98 },
    accentHover: { h: 216, s: 5, l: 24 },
    border: { h: 240, s: 3.7, l: 22 },
    success: { h: 144, s: 57, l: 50 },
    warning: { h: 42, s: 84, l: 55 },
    destructive: { h: 359.33, s: 100, l: 65 },
  },
};

export interface PrimaryPreset {
  key: string;
  name: string;
  color: Hsl;
}

// Vben 的 14 种主题色，白字对比度不足 4.5 的已调暗，按钮上的字要看得清
export const primaryPresets: readonly PrimaryPreset[] = [
  { key: 'blue', name: '默认蓝', color: { h: 212, s: 100, l: 45 } },
  { key: 'violet', name: '紫罗兰', color: { h: 245, s: 82, l: 66 } },
  { key: 'pink', name: '樱花粉', color: { h: 347, s: 77, l: 52 } },
  { key: 'amber', name: '琥珀', color: { h: 42, s: 84, l: 32 } },
  { key: 'sky', name: '天蓝', color: { h: 199, s: 89, l: 37 } },
  { key: 'green', name: '浅绿', color: { h: 144, s: 57, l: 33 } },
  { key: 'zinc', name: '锌色灰', color: { h: 240, s: 6, l: 10 } },
  { key: 'forest', name: '深绿', color: { h: 161, s: 94, l: 27 } },
  { key: 'navy', name: '深蓝', color: { h: 221, s: 83, l: 53 } },
  { key: 'orange', name: '橙黄', color: { h: 25, s: 95, l: 39 } },
  { key: 'rose', name: '玫瑰红', color: { h: 346, s: 77, l: 50 } },
  { key: 'neutral', name: '中性色', color: { h: 0, s: 0, l: 9 } },
  { key: 'slate', name: '石板灰', color: { h: 215, s: 25, l: 27 } },
  { key: 'gray', name: '中灰', color: { h: 220, s: 9, l: 46 } },
];

export type PrimaryKey = (typeof primaryPresets)[number]['key'];

export const radiusSteps = [0, 4, 8, 12, 16] as const;
export type RadiusStep = (typeof radiusSteps)[number];

export const fontSizes = [13, 14, 16] as const;
export type FontSize = (typeof fontSizes)[number];

export const shellSizes = {
  headerHeight: 50,
  tabBarHeight: 38,
  sidebarWidth: 224,
  sidebarMinWidth: 60,
  sidebarMaxWidth: 320,
  menuItemHeight: 38,
  mixedColumnWidth: 80,
} as const;

/** 单位毫秒，减少动画时为 0 */
export const motionDurations = { fast: 150, medium: 200, slow: 300, theme: 450 } as const;

export const motionEasings = {
  standard: 'ease-in-out',
  drawer: 'cubic-bezier(0.22, 1, 0.36, 1)',
  tab: 'cubic-bezier(0.25, 0.8, 0.5, 1)',
} as const;

export const zIndexes = { mask: 190, sidebar: 200, header: 210, drawer: 1000, progress: 2000 } as const;
