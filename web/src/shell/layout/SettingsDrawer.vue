<script setup lang="ts">
// 结构、尺寸、交互照 Vben Admin 的偏好设置抽屉（MIT）
import { Alert, Drawer, InputNumber, Popconfirm, Segmented, Select } from 'antdv-next';
import { computed, ref } from 'vue';
import { fontSizes, primaryPresets, radiusSteps } from '@/platform/theme';
import { formatHsl } from '@/platform/theme/color';
import { shellTexts } from '@/platform/texts/shell';
import KIcon from '@/platform/ui/KIcon.vue';
import KTooltip from '@/platform/ui/KTooltip.vue';
import { layoutModes, tabStyles, transitionEffects, type ShellSettings } from '../settings/shell-settings';
import LayoutThumb from './LayoutThumb.vue';
import SettingsSwitch from './SettingsSwitch.vue';

const { settings, saveFailed } = defineProps<{ settings: ShellSettings; saveFailed: boolean }>();

const emit = defineEmits<{ change: [change: (draft: ShellSettings) => void]; reset: [] }>();

const open = defineModel<boolean>('open', { default: false });

const t = shellTexts.settings;
type TabKey = 'appearance' | 'layout' | 'general';
const activeTab = ref<TabKey>('appearance');
const tabOptions = computed(() => [
  { label: t.tabs.appearance, value: 'appearance' },
  { label: t.tabs.layout, value: 'layout' },
  { label: t.tabs.general, value: 'general' },
]);

const modeOptions = computed(() => [
  { label: t.appearance.light, value: 'light' as const, icon: 'sun' as const },
  { label: t.appearance.dark, value: 'dark' as const, icon: 'moon' as const },
  { label: t.appearance.system, value: 'system' as const, icon: 'monitor' as const },
]);
const densityOptions = computed(() => [
  { label: t.appearance.densityDefault, value: 'default' as const },
  { label: t.appearance.densityCompact, value: 'compact' as const },
]);
const tabStyleOptions = computed(() => tabStyles.map((value) => ({ label: t.layout.styles[value], value })));
const effectOptions = computed(() => transitionEffects.map((value) => ({ label: t.general.effects[value], value })));

// 开关行：一行就是一个开关，点整行都能切换
interface SwitchRow {
  label: string;
  get: (settings: ShellSettings) => boolean;
  set: (draft: ShellSettings, value: boolean) => void;
}

const switches = computed<Record<string, SwitchRow[]>>(() => ({
  theme: [
    {
      label: t.appearance.darkSidebar,
      get: (s: ShellSettings) => s.appearance.darkSidebar,
      set: (d: ShellSettings, v: boolean) => (d.appearance.darkSidebar = v),
    },
    {
      label: t.appearance.darkHeader,
      get: (s: ShellSettings) => s.appearance.darkHeader,
      set: (d: ShellSettings, v: boolean) => (d.appearance.darkHeader = v),
    },
  ],
  other: [
    {
      label: t.appearance.colorWeak,
      get: (s: ShellSettings) => s.appearance.colorWeak,
      set: (d: ShellSettings, v: boolean) => (d.appearance.colorWeak = v),
    },
  ],
  sidebar: [
    {
      label: t.layout.accordion,
      get: (s: ShellSettings) => s.sidebar.accordion,
      set: (d: ShellSettings, v: boolean) => (d.sidebar.accordion = v),
    },
    {
      label: t.layout.showCollapsedTitle,
      get: (s: ShellSettings) => s.sidebar.showCollapsedTitle,
      set: (d: ShellSettings, v: boolean) => (d.sidebar.showCollapsedTitle = v),
    },
    {
      label: t.layout.hoverExpandAll,
      get: (s: ShellSettings) => s.sidebar.hoverExpandAll,
      set: (d: ShellSettings, v: boolean) => (d.sidebar.hoverExpandAll = v),
    },
  ],
  breadcrumb: [
    {
      label: t.layout.showBreadcrumb,
      get: (s: ShellSettings) => s.breadcrumb.show,
      set: (d: ShellSettings, v: boolean) => (d.breadcrumb.show = v),
    },
    {
      label: t.layout.breadcrumbIcon,
      get: (s: ShellSettings) => s.breadcrumb.showIcon,
      set: (d: ShellSettings, v: boolean) => (d.breadcrumb.showIcon = v),
    },
  ],
  tabs: [
    {
      label: t.layout.showTabs,
      get: (s: ShellSettings) => s.tabs.show,
      set: (d: ShellSettings, v: boolean) => (d.tabs.show = v),
    },
    {
      label: t.layout.tabIcon,
      get: (s: ShellSettings) => s.tabs.showIcon,
      set: (d: ShellSettings, v: boolean) => (d.tabs.showIcon = v),
    },
  ],
  transition: [
    {
      label: t.general.enableTransition,
      get: (s: ShellSettings) => s.transition.enabled,
      set: (d: ShellSettings, v: boolean) => (d.transition.enabled = v),
    },
    {
      label: t.general.progress,
      get: (s: ShellSettings) => s.transition.progress,
      set: (d: ShellSettings, v: boolean) => (d.transition.progress = v),
    },
  ],
}));

function toggle(row: SwitchRow): void {
  const next = !row.get(settings);
  emit('change', (draft) => {
    row.set(draft, next);
  });
}

function change(change: (draft: ShellSettings) => void): void {
  emit('change', change);
}
</script>

<template>
  <Drawer
    v-model:open="open"
    :size="384"
    :mask="false"
    placement="right"
    :closable="{ placement: 'end' }"
    :classes="{ root: 'settings-drawer' }"
  >
    <template #title>
      <div class="drawer-title">{{ t.title }}</div>
      <div class="drawer-subtitle">{{ t.subtitle }}</div>
    </template>
    <template #extra>
      <Popconfirm :title="t.resetConfirm" placement="bottomRight" @confirm="emit('reset')">
        <KTooltip :title="t.reset">
          <button type="button" class="header-button" :aria-label="t.reset"><KIcon name="rotate" :size="16" /></button>
        </KTooltip>
      </Popconfirm>
    </template>

    <Alert v-if="saveFailed" class="save-failed" type="warning" show-icon :message="t.saveFailed" />

    <Segmented v-model:value="activeTab" class="tabs" :options="tabOptions" block />

    <template v-if="activeTab === 'appearance'">
      <section class="block">
        <h3>{{ t.appearance.theme }}</h3>
        <div class="boxes">
          <button
            v-for="mode in modeOptions"
            :key="mode.value"
            type="button"
            class="box-item"
            :aria-pressed="settings.appearance.mode === mode.value"
            @click="change((d) => (d.appearance.mode = mode.value))"
          >
            <span class="outline-box mode-box" :class="{ 'is-active': settings.appearance.mode === mode.value }">
              <KIcon :name="mode.icon" :size="20" />
            </span>
            <span class="box-label">{{ mode.label }}</span>
          </button>
        </div>
        <SettingsSwitch
          v-for="row in switches.theme"
          :key="row.label"
          :label="row.label"
          :checked="row.get(settings)"
          @toggle="toggle(row)"
        />
      </section>

      <section class="block">
        <h3>{{ t.appearance.primary }}</h3>
        <div class="boxes">
          <button
            v-for="preset in primaryPresets"
            :key="preset.key"
            type="button"
            class="box-item"
            :aria-pressed="settings.appearance.primary === preset.key"
            @click="change((d) => (d.appearance.primary = preset.key))"
          >
            <span class="outline-box" :class="{ 'is-active': settings.appearance.primary === preset.key }">
              <span class="swatch" :style="{ background: formatHsl(preset.color) }" />
            </span>
            <span class="box-label">{{ preset.name }}</span>
          </button>
        </div>
      </section>

      <section class="block">
        <h3>{{ t.appearance.radius }}</h3>
        <div class="toggles">
          <button
            v-for="value in radiusSteps"
            :key="value"
            type="button"
            class="toggle"
            :class="{ 'is-on': settings.appearance.radius === value }"
            :aria-pressed="settings.appearance.radius === value"
            @click="change((d) => (d.appearance.radius = value))"
          >
            {{ value }}
          </button>
        </div>
      </section>

      <section class="block">
        <h3>{{ t.appearance.fontSize }}</h3>
        <div class="toggles">
          <button
            v-for="value in fontSizes"
            :key="value"
            type="button"
            class="toggle"
            :class="{ 'is-on': settings.appearance.fontSize === value }"
            :aria-pressed="settings.appearance.fontSize === value"
            @click="change((d) => (d.appearance.fontSize = value))"
          >
            {{ value }}
          </button>
        </div>
      </section>

      <section class="block">
        <h3>{{ t.appearance.density }}</h3>
        <div class="toggles">
          <button
            v-for="option in densityOptions"
            :key="option.value"
            type="button"
            class="toggle"
            :class="{ 'is-on': settings.appearance.density === option.value }"
            :aria-pressed="settings.appearance.density === option.value"
            @click="change((d) => (d.appearance.density = option.value))"
          >
            {{ option.label }}
          </button>
        </div>
      </section>

      <section class="block">
        <h3>{{ t.appearance.other }}</h3>
        <SettingsSwitch
          v-for="row in switches.other"
          :key="row.label"
          :label="row.label"
          :checked="row.get(settings)"
          @toggle="toggle(row)"
        />
      </section>
    </template>

    <template v-else-if="activeTab === 'layout'">
      <section class="block">
        <h3>{{ t.layout.mode }}</h3>
        <div class="boxes layouts">
          <button
            v-for="mode in layoutModes"
            :key="mode"
            type="button"
            class="box-item"
            :aria-pressed="settings.layout === mode"
            @click="change((d) => (d.layout = mode))"
          >
            <span class="outline-box" :class="{ 'is-active': settings.layout === mode }">
              <LayoutThumb :mode="mode" />
            </span>
            <span class="box-label">{{ t.layout.modes[mode] }}</span>
          </button>
        </div>
      </section>

      <section class="block">
        <h3>{{ t.layout.sidebar }}</h3>
        <SettingsSwitch
          v-for="row in switches.sidebar"
          :key="row.label"
          :label="row.label"
          :checked="row.get(settings)"
          @toggle="toggle(row)"
        />
      </section>

      <section class="block">
        <h3>{{ t.layout.breadcrumb }}</h3>
        <SettingsSwitch
          v-for="row in switches.breadcrumb"
          :key="row.label"
          :label="row.label"
          :checked="row.get(settings)"
          @toggle="toggle(row)"
        />
      </section>

      <section class="block">
        <h3>{{ t.layout.tabs }}</h3>
        <SettingsSwitch
          v-for="row in switches.tabs"
          :key="row.label"
          :label="row.label"
          :checked="row.get(settings)"
          @toggle="toggle(row)"
        />
        <div class="select-row">
          {{ t.layout.tabStyle }}
          <Select
            class="control"
            :style="{ width: '165px' }"
            :value="settings.tabs.style"
            :options="tabStyleOptions"
            @change="(value) => change((d) => (d.tabs.style = value as ShellSettings['tabs']['style']))"
          />
        </div>
        <div class="select-row">
          {{ t.layout.tabMax }}
          <InputNumber
            class="control"
            :style="{ width: '165px' }"
            :value="settings.tabs.max"
            :min="10"
            :max="30"
            @change="(value) => typeof value === 'number' && change((d) => (d.tabs.max = value))"
          />
        </div>
      </section>
    </template>

    <template v-else>
      <section class="block">
        <h3>{{ t.general.transition }}</h3>
        <SettingsSwitch
          v-for="row in switches.transition"
          :key="row.label"
          :label="row.label"
          :checked="row.get(settings)"
          @toggle="toggle(row)"
        />
        <div class="select-row" :class="{ 'is-disabled': !settings.transition.enabled }">
          {{ t.general.effect }}
          <Select
            class="control"
            :style="{ width: '165px' }"
            :value="settings.transition.effect"
            :options="effectOptions"
            :disabled="!settings.transition.enabled"
            @change="(value) => change((d) => (d.transition.effect = value as ShellSettings['transition']['effect']))"
          />
        </div>
      </section>
    </template>
  </Drawer>
</template>

<style lang="scss">
// 抽屉挂在 body 下：标题区和内容区的内边距照 Vben
.settings-drawer {
  .ant-drawer-header {
    padding: 12px 16px;
  }

  .ant-drawer-body {
    padding: 0 16px 16px;
  }
}
</style>

<style scoped lang="scss">
.drawer-title {
  font-size: 16px;
  font-weight: 600;
  line-height: 1.5;
}

.drawer-subtitle {
  font-size: 12px;
  font-weight: normal;
  color: var(--kmic-color-muted-foreground);
}

.header-button {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  color: var(--kmic-color-foreground);
  cursor: pointer;
  background: none;
  border: 0;
  border-radius: 50%;
  transition: background var(--kmic-motion-fast);

  &:hover {
    background: var(--kmic-color-accent);
  }
}

.save-failed {
  margin-top: 12px;
}

.tabs {
  margin-top: 16px;
}

.block {
  display: flex;
  flex-direction: column;
  padding: 16px 0;

  h3 {
    margin: 0 0 12px;
    font-size: 16px;
    font-weight: 600;
    line-height: 1;
  }
}

.boxes {
  display: grid;
  grid-template-columns: repeat(3, auto);
  justify-content: space-between;
  width: 100%;
}

.layouts {
  display: flex;
  flex-wrap: wrap;
  gap: 20px;
}

.box-item {
  display: flex;
  flex-direction: column;
  padding: 0;
  font: inherit;
  color: inherit;
  cursor: pointer;
  background: none;
  border: 0;

  &:focus-visible {
    outline: none;

    .outline-box {
      outline: 2px solid var(--kmic-color-primary);
    }
  }
}

// 细描边方框；悬停时主色描边从中间展开，选中后描边加粗变主色
.outline-box {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 4px;
  border-radius: var(--kmic-radius-md);
  outline: 1px solid var(--kmic-color-border);

  &::after {
    position: absolute;
    top: 50%;
    left: 50%;
    z-index: 1;
    width: 1px;
    height: 0;
    content: '';
    border-radius: var(--kmic-radius-sm);
    outline: 2px solid transparent;
    opacity: 0;
    transition: all var(--kmic-motion-slow);
  }

  &.is-active {
    outline: 2px solid var(--kmic-color-primary);

    &::after {
      display: none;
    }
  }

  &:not(.is-active):hover::after {
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    padding: 4px;
    outline-color: var(--kmic-color-primary);
    opacity: 1;
  }
}

.mode-box {
  padding: 16px 36px;
}

.swatch {
  width: 20px;
  height: 20px;
  margin: 8px 32px;
  border-radius: var(--kmic-radius-md);
}

.box-label {
  display: block;
  margin: 8px 0;
  font-size: 12px;
  color: var(--kmic-color-muted-foreground);
  text-align: center;
}

.toggles {
  display: flex;
  gap: 8px;
}

.toggle {
  width: 64px;
  height: 28px;
  font: inherit;
  font-size: 12px;
  color: var(--kmic-color-foreground);
  cursor: pointer;
  background: none;
  border: 1px solid var(--kmic-color-border);
  border-radius: var(--kmic-radius-sm);
  transition:
    background var(--kmic-motion-fast),
    color var(--kmic-motion-fast);

  &:hover:not(.is-on) {
    background: var(--kmic-color-accent);
  }

  &.is-on {
    color: var(--kmic-color-primary-foreground);
    background: var(--kmic-color-primary);
    border-color: var(--kmic-color-primary);
  }
}

.select-row {
  box-sizing: border-box;
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
  padding: 4px 8px;
  margin: 4px 0;
  border-radius: var(--kmic-radius-md);

  &:hover {
    background: var(--kmic-color-accent);
  }

  &.is-disabled {
    opacity: 0.5;
  }
}

.control {
  width: 165px;
}
</style>
