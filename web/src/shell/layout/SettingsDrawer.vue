<script setup lang="ts">
import { Alert, Button, Drawer, InputNumber, Popconfirm, Segmented, Select, Switch, TabPane, Tabs } from 'antdv-next';
import { computed, ref } from 'vue';
import { fontSizes, primaryPresets, radiusSteps } from '@/platform/theme';
import { formatHsl } from '@/platform/theme/color';
import { shellTexts } from '@/platform/texts/shell';
import KIcon from '@/platform/ui/KIcon.vue';
import { layoutModes, tabStyles, transitionEffects, type ShellSettings } from '../settings/shell-settings';

const { settings, saveFailed } = defineProps<{ settings: ShellSettings; saveFailed: boolean }>();

const emit = defineEmits<{ change: [change: (draft: ShellSettings) => void]; reset: [] }>();

const open = defineModel<boolean>('open', { default: false });

const t = shellTexts.settings;
const activeTab = ref('appearance');

const modeOptions = computed(() => [
  { label: t.appearance.light, value: 'light' as const, icon: 'sun' as const },
  { label: t.appearance.dark, value: 'dark' as const, icon: 'moon' as const },
  { label: t.appearance.system, value: 'system' as const, icon: 'monitor' as const },
]);
const densityOptions = computed(() => [
  { label: t.appearance.densityDefault, value: 'default' },
  { label: t.appearance.densityCompact, value: 'compact' },
]);
const tabStyleOptions = computed(() => tabStyles.map((value) => ({ label: t.layout.styles[value], value })));
const effectOptions = computed(() => transitionEffects.map((value) => ({ label: t.general.effects[value], value })));

function change(change: (draft: ShellSettings) => void): void {
  emit('change', change);
}
</script>

<template>
  <Drawer v-model:open="open" :title="t.title" :size="384" :mask="false" placement="right">
    <template #extra>
      <Popconfirm :title="t.resetConfirm" @confirm="emit('reset')">
        <Button size="small">{{ t.reset }}</Button>
      </Popconfirm>
    </template>

    <Alert v-if="saveFailed" class="save-failed" type="warning" show-icon :message="t.saveFailed" />

    <Tabs v-model:active-key="activeTab">
      <TabPane key="appearance" :tab="t.tabs.appearance">
        <section class="block">
          <h4>{{ t.appearance.theme }}</h4>
          <div class="modes">
            <button
              v-for="mode in modeOptions"
              :key="mode.value"
              class="mode-card"
              :class="{ 'is-selected': settings.appearance.mode === mode.value }"
              @click="change((d) => (d.appearance.mode = mode.value))"
            >
              <span class="mode-icon"><KIcon :name="mode.icon" :size="20" /></span>
              {{ mode.label }}
            </button>
          </div>
        </section>

        <section class="block">
          <h4>{{ t.appearance.primary }}</h4>
          <div class="swatches">
            <button
              v-for="preset in primaryPresets"
              :key="preset.key"
              class="swatch"
              :class="{ 'is-selected': settings.appearance.primary === preset.key }"
              :style="{ background: formatHsl(preset.color) }"
              :title="preset.name"
              @click="change((d) => (d.appearance.primary = preset.key))"
            >
              <KIcon v-if="settings.appearance.primary === preset.key" name="check" :size="16" />
            </button>
          </div>
        </section>

        <section class="block">
          <h4>{{ t.appearance.radius }}</h4>
          <Segmented
            :value="settings.appearance.radius"
            :options="radiusSteps.map((value) => ({ label: `${value}`, value }))"
            block
            @change="(value) => change((d) => (d.appearance.radius = value as ShellSettings['appearance']['radius']))"
          />
        </section>

        <section class="block">
          <h4>{{ t.appearance.fontSize }}</h4>
          <Segmented
            :value="settings.appearance.fontSize"
            :options="fontSizes.map((value) => ({ label: `${value}`, value }))"
            block
            @change="
              (value) => change((d) => (d.appearance.fontSize = value as ShellSettings['appearance']['fontSize']))
            "
          />
        </section>

        <section class="block">
          <h4>{{ t.appearance.density }}</h4>
          <Segmented
            :value="settings.appearance.density"
            :options="densityOptions"
            block
            @change="(value) => change((d) => (d.appearance.density = value as ShellSettings['appearance']['density']))"
          />
        </section>

        <section class="block">
          <label class="row">
            {{ t.appearance.darkSidebar }}
            <Switch
              :checked="settings.appearance.darkSidebar"
              @change="(v) => change((d) => (d.appearance.darkSidebar = Boolean(v)))"
            />
          </label>
          <label class="row">
            {{ t.appearance.darkHeader }}
            <Switch
              :checked="settings.appearance.darkHeader"
              @change="(v) => change((d) => (d.appearance.darkHeader = Boolean(v)))"
            />
          </label>
          <label class="row">
            {{ t.appearance.colorWeak }}
            <Switch
              :checked="settings.appearance.colorWeak"
              @change="(v) => change((d) => (d.appearance.colorWeak = Boolean(v)))"
            />
          </label>
        </section>
      </TabPane>

      <TabPane key="layout" :tab="t.tabs.layout">
        <section class="block">
          <h4>{{ t.layout.mode }}</h4>
          <div class="layouts">
            <button
              v-for="mode in layoutModes"
              :key="mode"
              class="layout-card"
              :class="{ 'is-selected': settings.layout === mode }"
              @click="change((d) => (d.layout = mode))"
            >
              <span class="preview" :class="`preview-${mode}`">
                <span class="p-side" />
                <span class="p-column" />
                <span class="p-head" />
              </span>
              <span class="layout-name">{{ t.layout.modes[mode] }}</span>
            </button>
          </div>
        </section>

        <section class="block">
          <h4>{{ t.layout.sidebar }}</h4>
          <label class="row">
            {{ t.layout.accordion }}
            <Switch
              :checked="settings.sidebar.accordion"
              @change="(v) => change((d) => (d.sidebar.accordion = Boolean(v)))"
            />
          </label>
          <label class="row">
            {{ t.layout.showCollapsedTitle }}
            <Switch
              :checked="settings.sidebar.showCollapsedTitle"
              @change="(v) => change((d) => (d.sidebar.showCollapsedTitle = Boolean(v)))"
            />
          </label>
          <label class="row">
            {{ t.layout.hoverExpandAll }}
            <Switch
              :checked="settings.sidebar.hoverExpandAll"
              @change="(v) => change((d) => (d.sidebar.hoverExpandAll = Boolean(v)))"
            />
          </label>
        </section>

        <section class="block">
          <h4>{{ t.layout.breadcrumb }}</h4>
          <label class="row">
            {{ t.layout.showBreadcrumb }}
            <Switch
              :checked="settings.breadcrumb.show"
              @change="(v) => change((d) => (d.breadcrumb.show = Boolean(v)))"
            />
          </label>
          <label class="row">
            {{ t.layout.breadcrumbIcon }}
            <Switch
              :checked="settings.breadcrumb.showIcon"
              @change="(v) => change((d) => (d.breadcrumb.showIcon = Boolean(v)))"
            />
          </label>
        </section>

        <section class="block">
          <h4>{{ t.layout.tabs }}</h4>
          <label class="row">
            {{ t.layout.showTabs }}
            <Switch :checked="settings.tabs.show" @change="(v) => change((d) => (d.tabs.show = Boolean(v)))" />
          </label>
          <label class="row">
            {{ t.layout.tabStyle }}
            <Select
              class="control"
              :value="settings.tabs.style"
              :options="tabStyleOptions"
              @change="(value) => change((d) => (d.tabs.style = value as ShellSettings['tabs']['style']))"
            />
          </label>
          <label class="row">
            {{ t.layout.tabIcon }}
            <Switch :checked="settings.tabs.showIcon" @change="(v) => change((d) => (d.tabs.showIcon = Boolean(v)))" />
          </label>
          <label class="row">
            {{ t.layout.tabMax }}
            <InputNumber
              class="control"
              :value="settings.tabs.max"
              :min="10"
              :max="30"
              @change="(value) => typeof value === 'number' && change((d) => (d.tabs.max = value))"
            />
          </label>
        </section>
      </TabPane>

      <TabPane key="general" :tab="t.tabs.general">
        <section class="block">
          <h4>{{ t.general.transition }}</h4>
          <label class="row">
            {{ t.general.enableTransition }}
            <Switch
              :checked="settings.transition.enabled"
              @change="(v) => change((d) => (d.transition.enabled = Boolean(v)))"
            />
          </label>
          <label class="row">
            {{ t.general.effect }}
            <Select
              class="control"
              :value="settings.transition.effect"
              :options="effectOptions"
              :disabled="!settings.transition.enabled"
              @change="(value) => change((d) => (d.transition.effect = value as ShellSettings['transition']['effect']))"
            />
          </label>
          <label class="row">
            {{ t.general.progress }}
            <Switch
              :checked="settings.transition.progress"
              @change="(v) => change((d) => (d.transition.progress = Boolean(v)))"
            />
          </label>
        </section>
      </TabPane>
    </Tabs>
  </Drawer>
</template>

<style scoped lang="scss">
.save-failed {
  margin-bottom: 12px;
}

.block {
  margin-bottom: 20px;

  h4 {
    margin: 0 0 8px;
    font-weight: 600;
  }
}

.row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  min-height: 36px;
}

.control {
  width: 140px;
}

.hint {
  color: var(--kmic-color-muted-foreground);
}

.layouts {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 12px;
}

.layout-card {
  display: flex;
  flex-direction: column;
  gap: 6px;
  align-items: center;
  padding: 0;
  color: inherit;
  cursor: pointer;
  background: none;
  border: 0;

  &:hover .preview,
  &.is-selected .preview {
    outline-color: var(--kmic-color-primary);
  }
}

// 缩略图：深色块是侧栏和顶栏，浅色是内容区
.preview {
  position: relative;
  width: 100%;
  aspect-ratio: 4 / 3;
  overflow: hidden;
  background: var(--kmic-color-background-deep);
  border-radius: var(--kmic-radius);
  outline: 2px solid var(--kmic-color-border);
  outline-offset: 2px;
  transition: outline-color var(--kmic-motion-fast);

  span {
    position: absolute;
    background: var(--kmic-color-primary);
  }
}

.p-side {
  top: 0;
  bottom: 0;
  left: 0;
  width: 22%;
  opacity: 0.7;
}

.p-column {
  display: none;
}

.p-head {
  top: 0;
  right: 0;
  left: 22%;
  height: 16%;
  opacity: 0.25;
}

.preview-mixed-column {
  .p-side {
    width: 12%;
  }

  .p-column {
    top: 0;
    bottom: 0;
    left: 12%;
    display: block;
    width: 18%;
    opacity: 0.35;
  }

  .p-head {
    left: 30%;
  }
}

.preview-mixed-vertical {
  .p-side {
    top: 16%;
    opacity: 0.35;
  }

  .p-head {
    left: 0;
    opacity: 0.7;
  }
}

.swatches {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  gap: 8px;
}

.modes {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 12px;
}

.mode-card {
  display: flex;
  flex-direction: column;
  gap: 6px;
  align-items: center;
  padding: 0;
  color: inherit;
  cursor: pointer;
  background: none;
  border: 0;

  &:hover .mode-icon,
  &.is-selected .mode-icon {
    outline-color: var(--kmic-color-primary);
  }
}

.mode-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 100%;
  height: 48px;
  border-radius: var(--kmic-radius);
  outline: 2px solid var(--kmic-color-border);
  outline-offset: 2px;
  transition: outline-color var(--kmic-motion-fast);
}

.swatch {
  display: flex;
  align-items: center;
  justify-content: center;
  aspect-ratio: 1;
  color: var(--kmic-color-primary-foreground);
  cursor: pointer;
  border: 2px solid transparent;
  border-radius: var(--kmic-radius);
  outline: 2px solid transparent;
  outline-offset: 2px;
  transition: outline-color var(--kmic-motion-fast);

  &:hover,
  &.is-selected {
    outline-color: var(--kmic-color-primary);
  }
}
</style>
