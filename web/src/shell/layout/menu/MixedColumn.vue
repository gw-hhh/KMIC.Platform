<script setup lang="ts">
import type { MenuNode } from '@/platform/routing';
import KIcon from '@/platform/ui/KIcon.vue';

defineProps<{ nodes: readonly MenuNode[]; activeKey: string | null }>();

const emit = defineEmits<{ select: [node: MenuNode]; enter: [node: MenuNode] }>();
</script>

<template>
  <ul class="mixed-column" role="menu">
    <li
      v-for="node in nodes"
      :key="node.key"
      class="item"
      :class="{ 'is-active': node.key === activeKey }"
      role="menuitem"
      tabindex="0"
      @click="emit('select', node)"
      @keydown.enter="emit('select', node)"
      @mouseenter="emit('enter', node)"
    >
      <KIcon v-if="node.icon" class="icon" :name="node.icon" :size="20" />
      <span class="name">{{ node.title }}</span>
    </li>
  </ul>
</template>

<style scoped lang="scss">
.mixed-column {
  padding: 0;
  margin: 0;
  list-style: none;
}

.item {
  display: flex;
  flex-direction: column;
  gap: 8px;
  align-items: center;
  padding: 9px 0;
  margin: 4px 8px;
  color: var(--kmic-sidebar-item-color);
  cursor: pointer;
  border-radius: var(--kmic-radius-md);
  transition:
    background var(--kmic-motion-fast) ease,
    color var(--kmic-motion-fast) ease;

  .icon {
    transition: transform var(--kmic-motion-slow);
  }

  &:not(.is-active):hover {
    color: var(--kmic-column-item-hover-color);
    background: var(--kmic-column-item-hover-background);
  }

  &:hover .icon {
    transform: scale(1.2);
  }

  &:focus-visible {
    outline: 2px solid var(--kmic-color-primary);
    outline-offset: -2px;
  }

  &.is-active {
    font-weight: 600;
    color: var(--kmic-color-primary-foreground);
    background: var(--kmic-column-item-active-background);
  }
}

.name {
  width: 100%;
  overflow: hidden;
  font-size: 12px;
  text-align: center;
  text-overflow: ellipsis;
  white-space: nowrap;
}
</style>
