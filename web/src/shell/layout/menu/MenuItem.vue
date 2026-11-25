<script setup lang="ts">
import { Popover, Tooltip } from 'antdv-next';
import { computed } from 'vue';
import type { MenuNode } from '@/platform/routing';
import KIcon from '@/platform/ui/KIcon.vue';
import { useMenu } from './menu-context';

const props = defineProps<{ node: MenuNode; level: number; popup: boolean }>();

const menu = useMenu();

const isGroup = computed(() => props.node.children.length > 0);
const isActive = computed(() => !isGroup.value && props.node.routeName === menu.activeKey.value);
const isActiveGroup = computed(() => isGroup.value && menu.activeAncestors.value.has(props.node.key));
const isOpen = computed(() => menu.openKeys.value.has(props.node.key));
// 弹出的子菜单始终展开显示
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
      placement="rightTop"
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
      <div class="entry is-collapsed" :class="{ 'is-active-group': isActiveGroup }" role="menuitem" tabindex="0">
        <KIcon v-if="node.icon" class="icon" :name="node.icon" :size="16" />
        <span v-if="menu.showCollapsedTitle.value" class="collapsed-title">{{ node.title }}</span>
      </div>
    </Popover>

    <Tooltip v-else-if="isCollapsed" placement="right" :title="node.title">
      <div
        class="entry is-collapsed"
        :class="{ 'is-active': isActive }"
        role="menuitem"
        tabindex="0"
        @click="onClick"
        @keydown.enter="onClick"
      >
        <KIcon v-if="node.icon" class="icon" :name="node.icon" :size="16" />
        <span v-if="menu.showCollapsedTitle.value" class="collapsed-title">{{ node.title }}</span>
      </div>
    </Tooltip>

    <template v-else>
      <div
        class="entry"
        :class="{ 'is-active': isActive, 'is-active-group': isActiveGroup }"
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
        <KIcon v-if="isGroup" class="arrow" :class="{ 'is-open': isOpen }" name="chevron-right" :size="16" />
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
// 折叠后悬停弹出的子菜单，样式和侧栏一致
.menu-popup .ant-popover-container {
  min-width: 180px;
  padding: 6px 0;
  background: var(--kmic-sidebar-background);
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
  margin: 2px 8px;
  color: var(--kmic-sidebar-foreground);
  white-space: nowrap;
  cursor: pointer;
  border-radius: var(--kmic-radius);
  transition:
    background var(--kmic-motion-fast) ease,
    color var(--kmic-motion-fast) ease;

  .icon {
    transition: transform var(--kmic-motion-slow);
  }

  &:hover {
    background: var(--kmic-sidebar-accent);

    .icon {
      transform: scale(1.2);
    }
  }

  &:focus-visible {
    outline: 2px solid var(--kmic-color-primary);
    outline-offset: -2px;
  }

  &.is-active {
    color: var(--kmic-color-primary);
    background: var(--kmic-color-primary-soft);
  }

  &.is-active-group {
    color: var(--kmic-color-primary);
  }

  &.is-collapsed {
    flex-direction: column;
    gap: 4px;
    align-items: center;
    justify-content: center;
    height: auto;
    min-height: 46px;
    margin: 4px 6px;
    padding-right: 0;

    // 折叠后当前页面所在的目录也带底色，和展开时的选中项一致
    &.is-active-group {
      background: var(--kmic-color-primary-soft);
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
  overflow: hidden;
  font-size: 12px;
  text-overflow: ellipsis;
}

.arrow {
  transition: transform var(--kmic-motion-slow);

  &.is-open {
    transform: rotate(90deg);
  }
}

// 展开动画
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
