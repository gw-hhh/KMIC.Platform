<script setup lang="ts">
// 可滚动区域：滚动后顶部出现渐变阴影和分隔线，提示上面还有内容，照 Vben Admin 的 scrollbar（MIT）
import { ref } from 'vue';

const atTop = ref(true);
const atBottom = ref(false);

function onScroll(event: Event): void {
  const box = event.target as HTMLElement;
  atTop.value = box.scrollTop <= 0;
  // scrollTop 有小数，差 1px 以内算到底
  atBottom.value = box.scrollTop + box.clientHeight >= box.scrollHeight - 1;
}
</script>

<template>
  <div class="scroll-shadow">
    <div class="shadow is-top" :class="{ 'is-visible': !atTop }" />
    <div class="scroll-body" @scroll.passive="onScroll">
      <slot />
    </div>
    <div class="shadow is-bottom" :class="{ 'is-visible': !atTop && !atBottom }" />
  </div>
</template>

<style scoped lang="scss">
.scroll-shadow {
  position: relative;
  display: flex;
  flex-direction: column;
  min-height: 0;
}

.scroll-body {
  flex: 1;
  min-height: 0;
  overflow: hidden auto;
}

.shadow {
  position: absolute;
  right: 0;
  left: 0;
  z-index: 1;
  height: 48px;
  pointer-events: none;
  opacity: 0;
  transition: opacity var(--kmic-motion-slow) ease-in-out;

  &.is-visible {
    opacity: 1;
  }

  &.is-top {
    top: 0;
    background: linear-gradient(to bottom, var(--kmic-sidebar-background), transparent);

    &.is-visible {
      border-top: 1px solid var(--kmic-sidebar-border);
    }
  }

  &.is-bottom {
    bottom: 0;
    background: linear-gradient(to top, var(--kmic-sidebar-background), transparent);

    &.is-visible {
      border-bottom: 1px solid var(--kmic-sidebar-border);
    }
  }
}
</style>
