<script setup lang="ts">
import type { MenuNode } from '@/platform/routing';
import KIcon from '@/platform/ui/KIcon.vue';

defineProps<{ nodes: readonly MenuNode[]; activeKey: string | null }>();

const emit = defineEmits<{ select: [node: MenuNode] }>();
</script>

<template>
  <ul class="header-menu" role="menubar">
    <li
      v-for="node in nodes"
      :key="node.key"
      class="item"
      :class="{ 'is-active': node.key === activeKey }"
      role="menuitem"
      tabindex="0"
      @click="emit('select', node)"
      @keydown.enter="emit('select', node)"
    >
      <KIcon v-if="node.icon" class="icon" :name="node.icon" :size="16" />
      <span>{{ node.title }}</span>
    </li>
  </ul>
</template>

<style scoped lang="scss">
.header-menu {
  display: flex;
  gap: 2px;
  align-items: center;
  min-width: 0;
  padding: 0;
  margin: 0 0 0 8px;
  overflow: hidden;
  list-style: none;
}

.item {
  display: flex;
  flex: none;
  gap: 6px;
  align-items: center;
  height: 40px;
  padding: 0 12px;
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
    background: var(--kmic-header-accent);

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
}
</style>
