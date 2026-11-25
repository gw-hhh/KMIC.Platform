import { z } from 'zod';
import { defaultAppearance, fontSizes, primaryPresets, radiusSteps, type Appearance } from '@/platform/theme';

export const layoutModes = ['vertical', 'mixed-column', 'mixed-vertical'] as const;
export type LayoutMode = (typeof layoutModes)[number];

/** 谷歌、卡片、朴素、轻快 */
export const tabStyles = ['chrome', 'card', 'plain', 'brisk'] as const;
export type TabStyle = (typeof tabStyles)[number];

export const transitionEffects = ['fade-slide', 'fade', 'fade-up', 'fade-down'] as const;
export type TransitionEffect = (typeof transitionEffects)[number];

/** 个人偏好 `Ui.Shell` 的内容 */
export interface ShellSettings {
  appearance: Appearance;
  layout: LayoutMode;
  sidebar: {
    collapsed: boolean;
    /** 单位 px */
    width: number;
    accordion: boolean;
    showCollapsedTitle: boolean;
    /** 折叠后悬停时展开整个侧栏（浮在内容上）；关闭时只弹出所在目录的子菜单，和 Vben 默认一致 */
    hoverExpandAll: boolean;
    /** 顶栏按钮把侧栏整个隐藏 */
    hidden: boolean;
  };
  breadcrumb: { show: boolean; showIcon: boolean };
  tabs: { show: boolean; style: TabStyle; showIcon: boolean; max: number };
  transition: { enabled: boolean; effect: TransitionEffect; progress: boolean };
}

export const defaultShellSettings: ShellSettings = {
  appearance: defaultAppearance,
  layout: 'vertical',
  sidebar: {
    collapsed: false,
    width: 224,
    accordion: true,
    showCollapsedTitle: false,
    hoverExpandAll: false,
    hidden: false,
  },
  breadcrumb: { show: true, showIcon: true },
  tabs: { show: true, style: 'chrome', showIcon: true, max: 15 },
  transition: { enabled: true, effect: 'fade-slide', progress: true },
};

export const shellSettingsVersion = 1;

const field = <T extends z.ZodType>(schema: T, fallback: z.infer<T>) => schema.catch(fallback);
const oneOf = (values: readonly number[]) => z.number().refine((value) => values.includes(value));

const d = defaultShellSettings;

const appearanceSchema = z.object({
  mode: field(z.enum(['light', 'dark', 'system']), d.appearance.mode),
  primary: field(
    z.string().refine((key) => primaryPresets.some((preset) => preset.key === key)),
    d.appearance.primary,
  ),
  radius: field(oneOf(radiusSteps), d.appearance.radius) as z.ZodType<Appearance['radius']>,
  fontSize: field(oneOf(fontSizes), d.appearance.fontSize) as z.ZodType<Appearance['fontSize']>,
  density: field(z.enum(['default', 'compact']), d.appearance.density),
  colorWeak: field(z.boolean(), d.appearance.colorWeak),
  darkSidebar: field(z.boolean(), d.appearance.darkSidebar),
  darkHeader: field(z.boolean(), d.appearance.darkHeader),
});

const shellSettingsSchema = z.object({
  appearance: field(appearanceSchema, d.appearance).default(d.appearance),
  layout: field(z.enum(layoutModes), d.layout),
  sidebar: field(
    z.object({
      collapsed: field(z.boolean(), d.sidebar.collapsed),
      width: field(z.number().min(60).max(320), d.sidebar.width),
      accordion: field(z.boolean(), d.sidebar.accordion),
      showCollapsedTitle: field(z.boolean(), d.sidebar.showCollapsedTitle),
      hoverExpandAll: field(z.boolean(), d.sidebar.hoverExpandAll),
      hidden: field(z.boolean(), d.sidebar.hidden),
    }),
    d.sidebar,
  ),
  breadcrumb: field(
    z.object({ show: field(z.boolean(), d.breadcrumb.show), showIcon: field(z.boolean(), d.breadcrumb.showIcon) }),
    d.breadcrumb,
  ),
  tabs: field(
    z.object({
      show: field(z.boolean(), d.tabs.show),
      style: field(z.enum(tabStyles), d.tabs.style),
      showIcon: field(z.boolean(), d.tabs.showIcon),
      max: field(z.number().int().min(10).max(30), d.tabs.max),
    }),
    d.tabs,
  ),
  transition: field(
    z.object({
      enabled: field(z.boolean(), d.transition.enabled),
      effect: field(z.enum(transitionEffects), d.transition.effect),
      progress: field(z.boolean(), d.transition.progress),
    }),
    d.transition,
  ),
});

/** 不合法的字段用默认值 */
export function parseShellSettings(raw: unknown): ShellSettings {
  return shellSettingsSchema.parse(typeof raw === 'object' && raw !== null ? raw : {});
}

export function serializeShellSettings(settings: ShellSettings): Record<string, unknown> {
  return { version: shellSettingsVersion, ...settings };
}
