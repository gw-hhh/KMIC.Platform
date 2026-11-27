import { useDebounceFn, useMediaQuery } from '@vueuse/core';
import { defineStore } from 'pinia';
import { computed, ref, toRaw, watch } from 'vue';
import { KmicError, useApi } from '@/platform/api';
import { useCurrentUserStore } from '@/platform/session/current-user';
import {
  applyTheme,
  buildAntdTheme,
  motionDurations,
  switchWithRipple,
  type ThemeMode,
  type ThemeRuntime,
} from '@/platform/theme';
import { parseShellSettings, serializeShellSettings, type ShellSettings } from './shell-settings';

const preferenceKey = 'Ui.Shell';
const lastModeKey = 'kmic.theme-mode';
const saveDelayMs = 1000;

type Mode = ShellSettings['appearance']['mode'];

/** 外壳偏好状态管理：变更即时生效并防抖 1 秒同步至服务端；同时缓存于本地以消除首屏默认主题闪烁 */
export const useShellSettingsStore = defineStore('shellSettings', () => {
  const api = useApi();
  const currentUser = useCurrentUserStore();

  const settings = ref<ShellSettings>(readCache(null));
  const saveFailed = ref(false);

  const prefersDark = useMediaQuery('(prefers-color-scheme: dark)');
  const reducedMotion = useMediaQuery('(prefers-reduced-motion: reduce)');

  const resolvedMode = computed<ThemeMode>(() => {
    const mode = settings.value.appearance.mode;
    return mode === 'system' ? (prefersDark.value ? 'dark' : 'light') : mode;
  });
  const runtime = computed<ThemeRuntime>(() => ({
    resolvedMode: resolvedMode.value,
    reducedMotion: reducedMotion.value,
  }));
  const antdTheme = computed(() => buildAntdTheme(settings.value.appearance, runtime.value));

  watch(
    [() => settings.value.appearance, runtime],
    () => {
      applyTheme(document.documentElement, settings.value.appearance, runtime.value);
      writeLastMode(settings.value.appearance.mode);
    },
    { immediate: true, deep: true },
  );

  const save = useDebounceFn(async () => {
    try {
      await api.PUT('/api/v1/configuration/preferences/me/{key}', {
        params: { path: { key: preferenceKey } },
        body: { value: serializeShellSettings(settings.value) },
      });
      saveFailed.value = false;
    } catch (error) {
      if (!(error instanceof KmicError)) {
        throw error;
      }
      // 持久化失败时不回滚当前预览状态，仅展示失败提示
      saveFailed.value = true;
    }
  }, saveDelayMs);

  // 未登录状态（如在登录页切换深浅模式）仅写入本地缓存
  function update(change: (draft: ShellSettings) => void): void {
    const draft = structuredClone(toRaw(settings.value));
    change(draft);
    settings.value = draft;
    const userId = currentUser.me?.userId ?? null;
    writeCache(userId, draft);
    if (userId !== null) {
      void save();
    }
  }

  /** 切换深浅模式，并以点击坐标为圆心触发扩散过渡 */
  async function toggleMode(origin: { x: number; y: number }): Promise<void> {
    const next = resolvedMode.value === 'dark' ? 'light' : 'dark';
    await switchWithRipple(
      origin,
      () => {
        update((draft) => {
          draft.appearance.mode = next;
        });
      },
      { reducedMotion: reducedMotion.value, durationMs: motionDurations.theme },
    );
  }

  async function load(): Promise<void> {
    const userId = currentUser.me?.userId ?? null;
    settings.value = readCache(userId);
    try {
      const { data } = await api.GET('/api/v1/configuration/preferences/me');
      const item = data?.preferences.find((preference) => preference.key === preferenceKey);
      if (item && item.source !== 'Default') {
        settings.value = parseShellSettings(item.value);
        writeCache(userId, settings.value);
      }
    } catch (error) {
      // 远程读取失败时保留本地缓存配置
      if (!(error instanceof KmicError)) {
        throw error;
      }
    }
  }

  async function reset(): Promise<void> {
    await api.POST('/api/v1/configuration/preferences/me/{key}/reset', { params: { path: { key: preferenceKey } } });
    await load();
  }

  return { settings, resolvedMode, antdTheme, reducedMotion, saveFailed, update, toggleMode, load, reset };
});

const cacheKey = (userId: string | null): string => `kmic.shell.${userId ?? 'anonymous'}`;

function readCache(userId: string | null): ShellSettings {
  let raw: unknown;
  try {
    const text = localStorage.getItem(cacheKey(userId));
    raw = text === null ? undefined : JSON.parse(text);
  } catch {
    raw = undefined;
  }
  const settings = parseShellSettings(raw);
  // 未登录时沿用上次记录的主题模式
  return raw === undefined && userId === null
    ? { ...settings, appearance: { ...settings.appearance, mode: readLastMode() } }
    : settings;
}

function writeCache(userId: string | null, settings: ShellSettings): void {
  try {
    localStorage.setItem(cacheKey(userId), JSON.stringify(serializeShellSettings(settings)));
  } catch {
    // 本地存储不可用时静默忽略
  }
}

function readLastMode(): Mode {
  try {
    const value = localStorage.getItem(lastModeKey);
    return value === 'light' || value === 'dark' || value === 'system' ? value : 'system';
  } catch {
    return 'system';
  }
}

function writeLastMode(mode: Mode): void {
  try {
    localStorage.setItem(lastModeKey, mode);
  } catch {
    // 本地存储不可用时静默忽略
  }
}
