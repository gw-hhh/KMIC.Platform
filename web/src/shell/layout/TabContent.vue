<script setup lang="ts">
import { computed, defineComponent, h, useTemplateRef, type Component } from 'vue';
import type { RouteCatalog } from '@/platform/routing';
import type { ShellSettings } from '../settings/shell-settings';
import type { Workspace, Tab } from '../workspace/workspace';
import PageHost from './PageHost.vue';

const { workspace, catalog, settings } = defineProps<{
  workspace: Workspace;
  catalog: RouteCatalog;
  settings: ShellSettings;
}>();

const scroller = useTemplateRef<HTMLElement>('scroller');

// KeepAlive 按组件名缓存，所以按标签键包一层
const wrappers = new Map<string, Component>();

function idOf(tab: Tab): string {
  return `${tab.key}@${String(tab.generation)}`;
}

function wrapperOf(tab: Tab): Component {
  const id = idOf(tab);
  let wrapper = wrappers.get(id);
  if (wrapper === undefined) {
    wrapper = defineComponent({
      name: id,
      setup: () => () =>
        h(PageHost, { tabKey: tab.key, routeName: tab.routeName, workspace, catalog, 'data-tab-page': '' }),
    });
    wrappers.set(id, wrapper);
  }
  return wrapper;
}

const active = computed(() => workspace.tabs.value.find((tab) => tab.key === workspace.activeKey.value));
const included = computed(() => workspace.tabs.value.map(idOf));
const animated = computed(() => settings.transition.enabled);
const transitionName = computed(() => `page-${settings.transition.effect}`);

// 页面元素随 KeepAlive 一直保留，关闭标签后跟着回收，所以按元素记
interface PageMemory {
  scrollTop: number;
  focused: HTMLElement | null;
}
const memories = new WeakMap<Element, PageMemory>();

function remember(page: Element, change: Partial<PageMemory>): void {
  memories.set(page, { scrollTop: 0, focused: null, ...memories.get(page), ...change });
}

function trackFocus(event: FocusEvent): void {
  const target = event.target;
  const page = target instanceof HTMLElement ? target.closest('[data-tab-page]') : null;
  if (target instanceof HTMLElement && page) {
    remember(page, { focused: target });
  }
}

// 还在离场的页面；新的切换开始时立即结束，同一时刻最多一出一进，快速切换时不会几层页面叠在一起
const leaving = new Map<HTMLElement, () => void>();

function finishLeaving(): void {
  for (const finish of [...leaving.values()]) {
    finish();
  }
}

function onBeforeLeave(el: Element): void {
  finishLeaving();
  remember(el, { scrollTop: scroller.value?.scrollTop ?? 0 });
}

function onLeave(el: Element, done: () => void): void {
  const page = el as HTMLElement;
  const finish = (): void => {
    if (leaving.delete(page)) {
      page.style.top = '';
      done();
    }
  };
  leaving.set(page, finish);
  if (!animated.value) {
    finish();
    return;
  }
  // 离场的样式在两帧之后才加上，那时过渡才开始；减少动画时没有过渡，直接结束
  requestAnimationFrame(() => {
    requestAnimationFrame(() => {
      void Promise.allSettled(page.getAnimations().map((animation) => animation.finished)).then(finish);
    });
  });
}

// 新页面插入后、画出来之前恢复滚动位置，入场时就在原来的位置
function onEnter(el: Element): void {
  const box = scroller.value;
  if (box) {
    const before = box.scrollTop;
    box.scrollTop = memories.get(el)?.scrollTop ?? 0;
    // 离场的页面浮在上方，跟着挪回原处，看上去不动
    const shift = box.scrollTop - before;
    for (const page of leaving.keys()) {
      page.style.top = `${String((Number.parseFloat(page.style.top) || 0) + shift)}px`;
    }
  }
  restoreFocus(el);
}

// 用键盘在标签栏、菜单里操作时不抢焦点
function restoreFocus(page: Element): void {
  const focused = memories.get(page)?.focused;
  const current = document.activeElement;
  const keyboardElsewhere =
    current instanceof HTMLElement && current.matches(':focus-visible') && !page.contains(current);
  if (focused?.isConnected && page.contains(focused) && !keyboardElsewhere) {
    focused.focus({ preventScroll: true });
  }
}
</script>

<template>
  <main ref="scroller" class="tab-content">
    <div class="tab-stage" @focusin="trackFocus">
      <Transition
        :name="transitionName"
        :css="animated"
        appear
        @before-leave="onBeforeLeave"
        @leave="onLeave"
        @enter="onEnter"
      >
        <KeepAlive :include="included" :max="settings.tabs.max">
          <component :is="wrapperOf(active)" v-if="active" :key="idOf(active)" />
        </KeepAlive>
      </Transition>
      <slot v-if="!active" />
    </div>
  </main>
</template>

<style lang="scss">
.tab-stage {
  position: relative;
  min-height: 100%;
}

// 淡出的页面不占位，避免跳动
.tab-stage > .page-fade-slide-leave-active,
.tab-stage > .page-fade-leave-active,
.tab-stage > .page-fade-up-leave-active,
.tab-stage > .page-fade-down-leave-active {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
}

.tab-content:has(> .tab-stage > .page-fade-slide-enter-active, > .tab-stage > .page-fade-slide-leave-active) {
  overflow-x: hidden;
}

.page-fade-slide-enter-active,
.page-fade-slide-leave-active {
  transition:
    opacity var(--kmic-motion-slow),
    transform var(--kmic-motion-slow);
}

.page-fade-slide-enter-from {
  opacity: 0;
  transform: translateX(-30px);
}

.page-fade-slide-leave-to {
  opacity: 0;
  transform: translateX(30px);
}

.page-fade-enter-active,
.page-fade-leave-active {
  transition: opacity var(--kmic-motion-medium) ease-in-out;
}

.page-fade-enter-from,
.page-fade-leave-to {
  opacity: 0;
}

.page-fade-up-enter-active,
.page-fade-up-leave-active,
.page-fade-down-enter-active,
.page-fade-down-leave-active {
  transition:
    opacity var(--kmic-motion-slow),
    transform var(--kmic-motion-slow);
}

.page-fade-up-enter-from {
  opacity: 0;
  transform: translateY(10%);
}

.page-fade-up-leave-to {
  opacity: 0;
  transform: translateY(-10%);
}

.page-fade-down-enter-from {
  opacity: 0;
  transform: translateY(-10%);
}

.page-fade-down-leave-to {
  opacity: 0;
  transform: translateY(10%);
}
</style>
