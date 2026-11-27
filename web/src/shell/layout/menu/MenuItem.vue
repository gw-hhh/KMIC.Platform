<script setup lang="ts">
// 尺寸、配色与动效参考 Vben Admin 的 menu-ui（MIT）
import { Popover } from 'antdv-next';
import { computed } from 'vue';
import type { MenuNode } from '@/platform/routing';
import KIcon from '@/platform/ui/KIcon.vue';
import KTooltip from '@/platform/ui/KTooltip.vue';
import { useMenu } from './menu-context';

const props = defineProps<{ node: MenuNode; level: number; popup: boolean }>();

const menu = useMenu();

const isGroup = computed(() => props.node.children.length > 0);
const isActive = computed(() => !isGroup.value && props.node.routeName === menu.activeKey.value);
const isActiveGroup = computed(() => isGroup.value && menu.activeAncestors.value.has(props.node.key));
const isOpen = computed(() => menu.openKeys.value.has(props.node.key));
// 悬停弹出的子菜单始终以展开态渲染
const isCollapsed = computed(() => menu.collapsed.value && !props.popup);
const indent = computed(() => `${String(props.level === 0 ? 8 : 16 + props.level * 16)}px`);

function onClick(): void {
  if (isGroup.value) {
    menu.toggle(props.node.key);
  } else if (props.node.routeName !== undefined) {
    menu.select(props.node.routeName);
  }
}
</script>

<template>
  <li class="menu-item" role="none">
    <Popover
      v-if="isCollapsed && isGroup"
      placement="right"
      :align="{ offset: [10, 0] }"
      :mouse-enter-delay="0.3"
      :mouse-leave-delay="0.3"
      :arrow="false"
      :classes="{ root: 'menu-popup' }"
    >
      <template #content>
        <ul class="menu-list">
          <MenuItem v-for="child in node.children" :key="child.key" :node="child" :level="0" popup />
        </ul>
      </template>
      <div
        class="entry is-collapsed"
        :class="{ 'is-active-group': isActiveGroup, 'with-title': menu.showCollapsedTitle.value }"
        role="menuitem"
        tabindex="0"
      >
        <KIcon class="icon" :name="node.icon ?? 'folder'" :size="menu.showCollapsedTitle.value ? 20 : 16" />
        <span v-if="menu.showCollapsedTitle.value" class="collapsed-title">{{ node.title }}</span>
      </div>
    </Popover>

    <KTooltip v-else-if="isCollapsed" :title="node.title" placement="right">
      <div
        class="entry is-collapsed"
        :class="{ 'is-active': isActive, 'with-title': menu.showCollapsedTitle.value }"
        role="menuitem"
        tabindex="0"
        @click="onClick"
        @keydown.enter="onClick"
      >
        <KIcon class="icon" :name="node.icon ?? 'file'" :size="menu.showCollapsedTitle.value ? 20 : 16" />
        <span v-if="menu.showCollapsedTitle.value" class="collapsed-title">{{ node.title }}</span>
      </div>
    </KTooltip>

    <template v-else>
      <div
        class="entry"
        :class="{ 'is-active': isActive, 'is-active-group': isActiveGroup, 'is-group': isGroup }"
        :style="{ paddingLeft: indent }"
        :aria-expanded="isGroup ? isOpen : undefined"
        :data-menu-key="node.key"
        role="menuitem"
        tabindex="0"
        @click="onClick"
        @keydown.enter="onClick"
      >
        <KIcon v-if="node.icon" class="icon" :name="node.icon" :size="16" />
        <span class="title">{{ node.title }}</span>
        <KIcon v-if="isGroup" class="arrow" :class="{ 'is-open': isOpen }" name="chevron-down" :size="16" />
      </div>
      <div v-if="isGroup" class="children" :class="{ 'is-open': isOpen }">
        <ul class="menu-list">
          <MenuItem v-for="child in node.children" :key="child.key" :node="child" :level="level + 1" :popup="popup" />
        </ul>
      </div>
    </template>
  </li>
</template>

<style lang="scss">
// 浮层挂载于 body，需使用全局非 scoped 样式
.menu-popup .ant-popover-container {
  width: 240px;
  padding: 10px 0;
  background: var(--kmic-sidebar-background);
  border: 1px solid var(--kmic-sidebar-border);
  border-radius: var(--kmic-radius-md);
  animation: menu-popup-in var(--kmic-motion-slow) cubic-bezier(0.16, 1, 0.3, 1);
}

@keyframes menu-popup-in {
  from {
    opacity: 0;
    transform: translateX(50px);
  }

  to {
    opacity: 1;
    transform: translateX(0);
  }
}
</style>

<style scoped lang="scss">
.menu-item {
  list-style: none;
}

.entry {
  position: relative;
  display: flex;
  gap: 8px;
  align-items: center;
  height: var(--kmic-menu-item-height);
  padding-right: 12px;
  margin: 0 8px 2px;
  color: var(--kmic-sidebar-item-color);
  white-space: nowrap;
  cursor: pointer;
  border-radius: var(--kmic-radius);
  transition:
    background var(--kmic-motion-fast) ease,
    color var(--kmic-motion-fast) ease,
    padding var(--kmic-motion-fast) ease;

  &.is-group {
    padding-right: 32px;
  }

  .icon {
    flex: none;
    transition: transform var(--kmic-motion-slow);
  }

  &:hover .icon {
    transform: scale(1.2);
  }

  // 选中项在悬停时保留激活态底色
  &:not(.is-active):hover {
    color: var(--kmic-sidebar-item-hover-color);
    background: var(--kmic-sidebar-accent);
  }

  &:focus-visible {
    outline: 2px solid var(--kmic-color-primary);
    outline-offset: -2px;
  }

  &.is-active {
    color: var(--kmic-sidebar-item-active-color);
    background: var(--kmic-sidebar-item-active-background);
  }

  // 活动页面所属目录仅高亮文字颜色，不添加背景底色
  &.is-active-group {
    color: var(--kmic-sidebar-group-active-color);
  }

  &.is-collapsed {
    flex-direction: column;
    gap: 0;
    justify-content: center;
    height: 47px;
    padding: 0;
    margin: 4px 6px;
    transition: all var(--kmic-motion-slow);

    &.is-active,
    &.is-active-group {
      color: var(--kmic-sidebar-item-active-color);
      background: var(--kmic-sidebar-item-active-background);
    }

    &.with-title {
      height: 64px;
      margin: 4px 8px;
    }
  }
}

.title {
  flex: 1;
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
}

.collapsed-title {
  max-width: 100%;
  margin-top: 8px;
  overflow: hidden;
  font-size: 12px;
  line-height: normal;
  text-overflow: ellipsis;
}

.arrow {
  position: absolute;
  top: 50%;
  right: 10px;
  margin-top: -8px;
  transition: transform var(--kmic-motion-slow) ease;

  &.is-open {
    transform: rotate(180deg);
  }
}

.children {
  display: grid;
  grid-template-rows: 0fr;
  transition: grid-template-rows var(--kmic-motion-medium) var(--kmic-easing-standard);

  &.is-open {
    grid-template-rows: 1fr;
  }

  > .menu-list {
    min-height: 0;
    overflow: hidden;
  }
}

.menu-list {
  padding: 0;
  margin: 0;
}
</style>
