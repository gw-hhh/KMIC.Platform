<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, ref, toRef, useTemplateRef, watch } from 'vue';
import type { MenuNode } from '@/platform/routing';
import { provideMenu } from './menu-context';
import MenuItem from './MenuItem.vue';

const props = defineProps<{
  nodes: readonly MenuNode[];
  activeKey: string | null;
  collapsed: boolean;
  accordion: boolean;
  showCollapsedTitle: boolean;
}>();

const emit = defineEmits<{ select: [routeName: string] }>();

const rootRef = useTemplateRef<HTMLElement>('root');
const openKeys = ref<ReadonlySet<string>>(new Set());

const parents = computed(() => {
  const map = new Map<string, string | null>();
  const walk = (nodes: readonly MenuNode[], parent: string | null): void => {
    for (const node of nodes) {
      map.set(node.key, parent);
      walk(node.children, node.key);
    }
  };
  walk(props.nodes, null);
  return map;
});

const activeAncestors = computed(() => {
  const chain = new Set<string>();
  for (let key = parents.value.get(props.activeKey ?? '') ?? null; key !== null; key = parents.value.get(key) ?? null) {
    chain.add(key);
  }
  return chain;
});

// 自动展开当前页面所属目录，待展开动画完成后将活动菜单项平滑滚动至可视区居中
const scrollDelay = 320;
let scrollTimer: ReturnType<typeof setTimeout> | undefined;

watch(
  [() => props.activeKey, () => props.nodes],
  async () => {
    openKeys.value = new Set([...openKeys.value, ...activeAncestors.value]);
    await nextTick();
    clearTimeout(scrollTimer);
    if (!props.collapsed) {
      scrollTimer = setTimeout(() => {
        rootRef.value?.querySelector('.entry.is-active')?.scrollIntoView({ block: 'center', behavior: 'smooth' });
      }, scrollDelay);
    }
  },
  { immediate: true },
);

onBeforeUnmount(() => {
  clearTimeout(scrollTimer);
});

function toggle(groupKey: string): void {
  const next = new Set(openKeys.value);
  if (next.has(groupKey)) {
    next.delete(groupKey);
  } else {
    if (props.accordion) {
      const parent = parents.value.get(groupKey) ?? null;
      for (const [key, otherParent] of parents.value) {
        if (otherParent === parent) {
          next.delete(key);
        }
      }
    }
    next.add(groupKey);
  }
  openKeys.value = next;
}

provideMenu({
  activeKey: toRef(props, 'activeKey'),
  activeAncestors,
  openKeys,
  collapsed: toRef(props, 'collapsed'),
  showCollapsedTitle: toRef(props, 'showCollapsedTitle'),
  toggle,
  select: (routeName) => {
    emit('select', routeName);
  },
});
</script>

<template>
  <nav ref="root" class="sidebar-menu">
    <ul class="menu-list" role="menu">
      <MenuItem v-for="node in nodes" :key="node.key" :node="node" :level="0" :popup="false" />
    </ul>
  </nav>
</template>

<style scoped lang="scss">
.sidebar-menu {
  padding-top: 8px;
}

.menu-list {
  padding: 0;
  margin: 0;
}
</style>
