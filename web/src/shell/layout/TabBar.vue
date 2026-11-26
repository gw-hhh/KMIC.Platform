<script setup lang="ts">
import type { MenuProps } from 'antdv-next';
import Sortable from 'sortablejs';
import { computed, h, nextTick, onBeforeUnmount, onMounted, ref, useTemplateRef, watch } from 'vue';
import { useRouter } from 'vue-router';
import type { RouteCatalog } from '@/platform/routing';
import { shellTexts } from '@/platform/texts/shell';
import KDropdown from '@/platform/ui/KDropdown.vue';
import KIcon from '@/platform/ui/KIcon.vue';
import type { IconName } from '@/platform/ui/icons';
import type { TabStyle } from '../settings/shell-settings';
import type { Tab, Workspace } from '../workspace/workspace';

const { workspace, catalog, homeRouteName, tabStyle, showIcon } = defineProps<{
  homeRouteName: string;
  workspace: Workspace;
  catalog: RouteCatalog;
  tabStyle: TabStyle;
  showIcon: boolean;
}>();

const maximized = defineModel<boolean>('maximized', { default: false });

const t = shellTexts.tabs;
const router = useRouter();
const scroller = useTemplateRef<HTMLElement>('scroller');
const list = useTemplateRef<HTMLElement>('list');
const canScrollLeft = ref(false);
const canScrollRight = ref(false);

const tabs = computed(() => workspace.tabs.value);
const closableCount = computed(() => tabs.value.filter((tab) => !tab.pinned).length);

function iconOf(tab: Tab) {
  return catalog.find(tab.routeName)?.icon;
}

function statusOf(tab: Tab) {
  return workspace.pageOf(tab.key)?.leaveStatus.value ?? 'free';
}

function menuFor(tab: Tab): MenuProps {
  const index = tabs.value.indexOf(tab);
  const others = (predicate: (other: Tab, otherIndex: number) => boolean) =>
    tabs.value.some((other, otherIndex) => !other.pinned && other.key !== tab.key && predicate(other, otherIndex));
  const isHome = tab.routeName === homeRouteName;
  const icon = (name: IconName) => h(KIcon, { name, size: 16 });
  // 顺序、图标、分隔照 Vben 的标签右键菜单
  return {
    items: [
      { key: 'close', label: t.close, icon: icon('close'), disabled: tab.pinned },
      { key: 'pin', label: tab.pinned ? t.unpin : t.pin, icon: icon(tab.pinned ? 'pin-off' : 'pin'), disabled: isHome },
      {
        key: 'maximize',
        label: maximized.value ? t.exitMaximize : t.maximize,
        icon: icon(maximized.value ? 'minimize' : 'fullscreen'),
      },
      { key: 'reload', label: t.reload, icon: icon('rotate') },
      { key: 'window', label: t.openInNewWindow, icon: icon('external-link') },
      { type: 'divider' },
      { key: 'left', label: t.closeLeft, icon: icon('arrow-left-to-line'), disabled: !others((_, i) => i < index) },
      { key: 'right', label: t.closeRight, icon: icon('arrow-right-to-line'), disabled: !others((_, i) => i > index) },
      { type: 'divider' },
      { key: 'others', label: t.closeOthers, icon: icon('fold-horizontal'), disabled: !others(() => true) },
      { key: 'all', label: t.closeAll, icon: icon('arrow-right-left'), disabled: closableCount.value === 0 },
    ],
    onClick: ({ key }) => {
      void runMenuAction(key, tab, index);
    },
  };
}

async function runMenuAction(action: string, tab: Tab, index: number): Promise<void> {
  const keysWhere = (predicate: (other: Tab, otherIndex: number) => boolean) =>
    tabs.value.filter((other, otherIndex) => !other.pinned && predicate(other, otherIndex)).map((other) => other.key);
  switch (action) {
    case 'close':
      await workspace.close(tab.key);
      break;
    case 'pin':
      workspace.togglePin(tab.key);
      break;
    case 'reload':
      await workspace.reload(tab.key);
      break;
    case 'maximize':
      await workspace.activate(tab.key);
      maximized.value = !maximized.value;
      break;
    case 'window':
      window.open(router.resolve({ name: tab.routeName, params: tab.params }).href, '_blank');
      break;
    case 'left':
      await workspace.closeMany(keysWhere((_, i) => i < index));
      break;
    case 'right':
      await workspace.closeMany(keysWhere((_, i) => i > index));
      break;
    case 'others':
      await workspace.closeMany(keysWhere((other) => other.key !== tab.key));
      break;
    case 'all':
      await workspace.closeMany(keysWhere(() => true));
      break;
  }
}

const activeTab = computed(() => tabs.value.find((tab) => tab.key === workspace.activeKey.value));

function onMouseDown(event: MouseEvent, tab: Tab): void {
  // 中键关闭，和浏览器一致
  if (event.button === 1 && !tab.pinned) {
    event.preventDefault();
    void workspace.close(tab.key);
  }
}

function onKeydown(event: KeyboardEvent, tab: Tab, index: number): void {
  const focusAt = (target: number): void => {
    const element = list.value?.children[(target + tabs.value.length) % tabs.value.length];
    if (element instanceof HTMLElement) {
      element.focus();
    }
  };
  if (event.key === 'ArrowRight') {
    focusAt(index + 1);
  } else if (event.key === 'ArrowLeft') {
    focusAt(index - 1);
  } else if (event.key === 'Enter') {
    void workspace.activate(tab.key);
  } else if (event.key === 'Delete' && !tab.pinned) {
    void workspace.close(tab.key);
  }
}

function updateScrollState(): void {
  const element = scroller.value;
  if (!element) {
    return;
  }
  canScrollLeft.value = element.scrollLeft > 0;
  canScrollRight.value = element.scrollLeft + element.clientWidth < element.scrollWidth - 1;
}

function scrollBy(direction: -1 | 1): void {
  scroller.value?.scrollBy({ left: direction * 200, behavior: 'smooth' });
}

function onWheel(event: WheelEvent): void {
  if (scroller.value) {
    event.preventDefault();
    scroller.value.scrollBy({ left: event.deltaY * 3, behavior: 'smooth' });
  }
}

// 当前标签滚动到可见位置
watch(
  () => workspace.activeKey.value,
  async () => {
    await nextTick();
    list.value
      ?.querySelector('.is-active')
      ?.scrollIntoView({ inline: 'nearest', block: 'nearest', behavior: 'smooth' });
  },
);

let sortable: Sortable | null = null;
let resizeObserver: ResizeObserver | null = null;

onMounted(() => {
  if (list.value) {
    sortable = Sortable.create(list.value, {
      animation: 300,
      delay: 400,
      delayOnTouchOnly: true,
      draggable: '.tab',
      filter: '.is-pinned',
      onMove: (event) => !event.related.classList.contains('is-pinned'),
      onEnd: (event) => {
        const { oldIndex, newIndex, item, from } = event;
        if (oldIndex === undefined || newIndex === undefined || oldIndex === newIndex) {
          return;
        }
        // 先还原 DOM，顺序由工作区修改
        from.removeChild(item);
        from.insertBefore(item, from.children[oldIndex] ?? null);
        workspace.move(oldIndex, newIndex);
      },
    });
  }
  resizeObserver = new ResizeObserver(updateScrollState);
  if (scroller.value) {
    resizeObserver.observe(scroller.value);
  }
  updateScrollState();
});

onBeforeUnmount(() => {
  sortable?.destroy();
  resizeObserver?.disconnect();
});
</script>

<template>
  <div class="tab-bar" :class="`style-${tabStyle}`">
    <button
      v-show="canScrollLeft || canScrollRight"
      class="scroll-button"
      :disabled="!canScrollLeft"
      @click="scrollBy(-1)"
    >
      <KIcon name="chevrons-left" :size="16" />
    </button>

    <div ref="scroller" class="scroller" @scroll="updateScrollState" @wheel="onWheel">
      <div ref="list" class="list" role="tablist">
        <TransitionGroup name="tab-slide">
          <KDropdown v-for="(tab, index) in tabs" :key="tab.key" :trigger="['contextmenu']" :menu="menuFor(tab)">
            <div
              class="tab"
              :class="{ 'is-active': tab.key === workspace.activeKey.value, 'is-pinned': tab.pinned }"
              role="tab"
              tabindex="0"
              :aria-selected="tab.key === workspace.activeKey.value"
              @click="workspace.activate(tab.key)"
              @mousedown="onMouseDown($event, tab)"
              @keydown="onKeydown($event, tab, index)"
            >
              <span v-if="tabStyle === 'chrome'" class="divider" />
              <span v-if="tabStyle === 'chrome'" class="background">
                <span class="background-fill" />
                <svg class="corner left" width="7" height="7"><path d="M 0 7 A 7 7 0 0 0 7 0 L 7 7 Z" /></svg>
                <svg class="corner right" width="7" height="7"><path d="M 0 0 A 7 7 0 0 0 7 7 L 0 7 Z" /></svg>
              </span>

              <span class="main">
                <KIcon v-if="showIcon && iconOf(tab)" class="tab-icon" :name="iconOf(tab) ?? 'file'" :size="16" />
                <span class="title">{{ workspace.titleOf(tab) }}</span>
                <span v-if="statusOf(tab) === 'dirty'" class="state dirty" :title="t.unsaved" />
                <KIcon
                  v-else-if="statusOf(tab) === 'saving'"
                  class="state saving"
                  name="loader"
                  :size="12"
                  :title="t.saving"
                />
                <KIcon
                  v-else-if="statusOf(tab) === 'unknown'"
                  class="state unknown"
                  name="warning"
                  :size="12"
                  :title="t.unknown"
                />
              </span>

              <span class="extra">
                <KIcon
                  v-if="tab.pinned && tab.routeName !== homeRouteName"
                  class="extra-icon pin"
                  name="pin"
                  :size="14"
                  @click.stop="workspace.togglePin(tab.key)"
                />
                <KIcon
                  v-else-if="!tab.pinned && tabs.length > 1"
                  class="extra-icon close"
                  name="close"
                  :size="12"
                  @click.stop="workspace.close(tab.key)"
                />
              </span>
            </div>
          </KDropdown>
        </TransitionGroup>
      </div>
    </div>

    <button
      v-show="canScrollLeft || canScrollRight"
      class="scroll-button"
      :disabled="!canScrollRight"
      @click="scrollBy(1)"
    >
      <KIcon name="chevrons-right" :size="16" />
    </button>

    <div class="tools">
      <KDropdown v-if="activeTab" :menu="menuFor(activeTab)" placement="bottomRight">
        <button class="tool" :aria-label="t.more"><KIcon name="grid" :size="16" /></button>
      </KDropdown>
      <button class="tool" :aria-label="t.reload" @click="activeTab && workspace.reload(activeTab.key)">
        <KIcon name="rotate" :size="16" />
      </button>
      <button class="tool" :aria-label="maximized ? t.exitMaximize : t.maximize" @click="maximized = !maximized">
        <KIcon :name="maximized ? 'minimize' : 'fullscreen'" :size="16" />
      </button>
    </div>
  </div>
</template>

<style scoped lang="scss">
// 尺寸、颜色、动画照 Vben Admin 的 tabs-ui（MIT）
$gap: 7px;

.tab-bar {
  display: flex;
  height: var(--kmic-tabbar-height);
  overflow: hidden;
  color: var(--kmic-color-foreground);
  background: var(--kmic-color-background);
  border-bottom: 1px solid var(--kmic-color-border);
}

.scroller {
  flex: 1;
  height: 100%;
  overflow: hidden;
}

.list {
  display: flex;
  width: max-content;
  height: 100%;
  padding-right: 24px;
}

.scroll-button,
.tool {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0 8px;
  color: var(--kmic-color-muted-foreground);
  cursor: pointer;
  background: none;
  border: 0;

  &:hover:not(:disabled) {
    color: var(--kmic-color-foreground);
    background: var(--kmic-color-muted);
  }

  &:disabled {
    pointer-events: none;
    opacity: 0.3;
  }
}

.scroll-button:first-child {
  border-right: 1px solid var(--kmic-color-border);
}

.scroll-button:not(:first-child) {
  border-left: 1px solid var(--kmic-color-border);
}

.tools {
  display: flex;
}

.tool {
  border-left: 1px solid var(--kmic-color-border);
}

.tab {
  position: relative;
  display: flex;
  flex: none;
  align-items: center;
  height: 100%;
  cursor: pointer;
  user-select: none;

  &:focus-visible {
    outline: 2px solid var(--kmic-color-primary);
    outline-offset: -2px;
  }
}

// 新增、关闭标签时从左侧滑入滑出，其余标签平移让位
.tab-slide-enter-active,
.tab-slide-leave-active {
  transition:
    opacity var(--kmic-motion-slow) var(--kmic-easing-tab),
    transform var(--kmic-motion-slow) var(--kmic-easing-tab);
}

.tab-slide-move {
  transition: transform var(--kmic-motion-slow);
}

.tab-slide-enter-from,
.tab-slide-leave-to {
  opacity: 0;
  transform: translateX(-15px);
}

.main {
  position: relative;
  z-index: 2;
  display: flex;
  align-items: center;
  height: 100%;
  overflow: hidden;
  color: var(--kmic-color-accent-foreground);
  white-space: nowrap;
  transition: color var(--kmic-motion-fast);
}

.tab.is-active .main {
  color: var(--kmic-tab-active-color);
}

.tab:hover .tab-icon {
  animation: icon-shrink var(--kmic-motion-slow) ease-in-out;
}

@keyframes icon-shrink {
  50% {
    transform: scale(0.8);
  }
}

.title {
  flex: 1;
  overflow: hidden;
}

.state {
  flex: none;
  margin-left: 6px;

  &.dirty {
    width: 6px;
    height: 6px;
    background: var(--kmic-color-primary);
    border-radius: 50%;
  }

  &.saving {
    color: var(--kmic-color-primary);
    animation: spin 1s linear infinite;
  }

  &.unknown {
    color: var(--kmic-color-warning);
  }
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

// 关闭按钮一直显示，贴在标签内侧右边
.extra {
  position: absolute;
  top: 50%;
  right: 6px;
  z-index: 3;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 16px;
  height: 16px;
  transform: translateY(-50%);
}

.extra-icon {
  color: var(--kmic-color-accent-foreground);
  cursor: pointer;
  border-radius: 50%;
  opacity: 0.8;
  transition: all var(--kmic-motion-fast);

  &.close {
    margin-top: 2px;
  }

  .tab.is-active & {
    opacity: 1;
  }

  &:hover {
    background: var(--kmic-color-accent);
    opacity: 1;
  }
}

// 谷歌样式
.style-chrome {
  .scroller {
    padding-top: 3px;
  }

  .tab {
    margin-right: -12px;
    padding: 0 4px;

    &.is-active {
      z-index: 2;
    }
  }

  .main {
    padding: 0 16px 0 8px;
    margin: 0 #{$gap * 2};
    border-radius: var(--kmic-radius-md) var(--kmic-radius-md) 0 0;
  }

  .tab-icon {
    margin-right: 4px;
  }

  .extra {
    right: $gap;
  }

  .divider {
    position: absolute;
    top: 50%;
    left: $gap;
    width: 1px;
    height: 16px;
    background: var(--kmic-color-border);
    transform: translateY(-50%);
    transition: all var(--kmic-motion-fast);
  }

  .tab:first-child .divider,
  .tab.is-active .divider {
    opacity: 0;
  }

  .background {
    position: absolute;
    inset: 0;
    z-index: -1;
    padding: 0 #{$gap - 1};
    transition: opacity var(--kmic-motion-fast);
  }

  .background-fill {
    display: block;
    height: 100%;
    border-radius: $gap $gap 0 0;
    transition: all var(--kmic-motion-fast);
  }

  .corner {
    position: absolute;
    bottom: 0;
    fill: transparent;
    transition: all var(--kmic-motion-fast);

    &.left {
      left: -1px;
    }

    &.right {
      right: -1px;
    }
  }

  .tab.is-active {
    .background-fill {
      background: var(--kmic-tab-active-background);
    }

    .corner {
      fill: var(--kmic-tab-active-background);
    }
  }

  .tab:not(.is-active):hover {
    .background {
      padding-bottom: 2px;
    }

    .background-fill {
      margin: 0 2px;
      background: var(--kmic-color-accent);
      border-radius: var(--kmic-radius-md);
    }

    .divider {
      opacity: 0;
    }
  }

  .tab:not(.is-active):hover + .tab .divider,
  .tab.is-active + .tab .divider {
    opacity: 0;
  }
}

// 卡片、朴素、轻快：标题左右 12/16px，关闭按钮离右边 6px，当前标签的关闭按钮跟着变色
.style-card,
.style-plain,
.style-brisk {
  .tab {
    transition: all var(--kmic-motion-fast);

    &:not(.is-active):hover {
      background: var(--kmic-color-accent);
    }

    &.is-active {
      background: var(--kmic-tab-active-background);

      .extra-icon {
        color: var(--kmic-tab-active-color);
      }
    }
  }

  .main {
    padding-right: 12px;
    margin: 0 16px 0 12px;
  }

  .tab-icon {
    margin-right: 8px;
  }
}

.style-card {
  .list {
    align-items: center;
  }

  .tab {
    height: calc(100% - 6px);
    margin-left: 8px;
    border: 1px solid var(--kmic-color-border);
    border-radius: var(--kmic-radius-md);
  }
}

.style-plain,
.style-brisk {
  .tab {
    &:not(:first-child) {
      border-left: 1px solid var(--kmic-color-border);
    }

    &:last-child {
      border-right: 1px solid var(--kmic-color-border);
    }
  }
}

// 轻快：底部主色线，悬停时从左往右展开
.style-brisk .tab {
  &::after {
    position: absolute;
    bottom: 0;
    left: 0;
    width: 100%;
    height: 1.5px;
    content: '';
    background: var(--kmic-color-primary);
    transform: scaleX(0);
    transform-origin: left;
    transition: transform var(--kmic-motion-slow) ease-out;
  }

  &:hover::after,
  &.is-active::after {
    transform: scaleX(1);
  }
}
</style>
