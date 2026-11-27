<script setup lang="ts">
// 偏好设置中的布局模式缩略图，图形移植自 Vben Admin（MIT）
import type { LayoutMode } from '../settings/shell-settings';

const { mode } = defineProps<{ mode: LayoutMode }>();
</script>

<template>
  <svg class="thumb" fill="none" height="66" width="104" aria-hidden="true">
    <rect fill="currentColor" fill-opacity="0.02" height="66" rx="4" width="104" />

    <!-- 垂直布局：左侧主色侧栏，右侧顶栏与内容区 -->
    <template v-if="mode === 'vertical'">
      <path class="primary" d="M0 4a4 4 0 0 1 4-4h22v66H4a4 4 0 0 1-4-4z" />
      <rect fill="#ffffff" height="9.7" rx="2" width="9.8" x="8.8" y="5.9" />
      <rect fill="#e5e5e5" height="2.8" rx="1.4" width="17.7" x="4.9" y="23.9" />
      <rect fill="#ffffff" height="2.8" rx="1.4" width="17.7" x="4.9" y="34.4" />
      <rect fill="#ffffff" height="2.8" rx="1.4" width="17.7" x="4.9" y="45" />
      <rect fill="#ffffff" height="2.8" rx="1.4" width="17.7" x="4.9" y="55.5" />
      <rect fill="currentColor" fill-opacity="0.08" height="9.1" rx="2" width="73.5" x="29" y="1.4" />
      <rect fill="#b2b2b2" height="4.4" rx="1" width="3.9" x="32" y="3.9" />
      <rect fill="#b2b2b2" height="4.4" rx="1" width="3.9" x="80.8" y="3.6" />
      <rect fill="#b2b2b2" height="4.4" rx="1" width="3.9" x="87.6" y="3.5" />
      <rect fill="#b2b2b2" height="4.4" rx="1" width="3.9" x="94.7" y="3.6" />
      <rect fill="currentColor" fill-opacity="0.08" height="21" rx="2" width="22.8" x="29.4" y="14.6" />
      <rect fill="currentColor" fill-opacity="0.08" height="21.5" rx="2" width="45.6" x="56.1" y="14.6" />
      <rect fill="currentColor" fill-opacity="0.08" height="21.7" rx="2" width="72.5" x="29" y="39.5" />
    </template>

    <!-- 双列菜单布局：窄幅主色图标列配合浅色子菜单列 -->
    <template v-else-if="mode === 'mixed-column'">
      <path class="primary" d="M0 4a2 4 0 0 1 2-4h8v66H2a2 4 0 0 1-2-4z" />
      <rect fill="#ffffff" height="7.7" rx="2" width="8.2" x="0.6" y="1.4" />
      <rect fill="#e5e5e5" height="2.8" rx="1.4" width="5.5" x="1.6" y="15.5" />
      <rect fill="#e5e5e5" height="2.8" rx="1.4" width="5.5" x="1.8" y="28.4" />
      <rect fill="#e5e5e5" height="2.8" rx="1.4" width="5.5" x="1.6" y="41.8" />
      <rect fill="#e5e5e5" height="2.8" rx="1.4" width="5.5" x="1.6" y="55.4" />
      <rect fill="currentColor" fill-opacity="0.08" height="65.7" width="12.5" x="9.9" y="0" />
      <rect fill="currentColor" fill-opacity="0.08" height="9.1" rx="2" width="75.9" x="25.4" y="1.4" />
      <rect fill="#b2b2b2" height="4.4" rx="1" width="3.9" x="27.9" y="3.7" />
      <rect fill="#b2b2b2" height="4.4" rx="1" width="3.9" x="80.8" y="3.6" />
      <rect fill="#b2b2b2" height="4.4" rx="1" width="3.9" x="87.8" y="3.7" />
      <rect fill="#b2b2b2" height="4.4" rx="1" width="3.9" x="94.7" y="3.6" />
      <rect fill="currentColor" fill-opacity="0.08" height="21" rx="2" width="28.4" x="26.1" y="14.6" />
      <rect fill="currentColor" fill-opacity="0.08" height="21.5" rx="2" width="42.9" x="58.8" y="14.6" />
      <rect fill="currentColor" fill-opacity="0.08" height="21.7" rx="2" width="75.1" x="26.3" y="39.7" />
    </template>

    <!-- 混合垂直布局：主色顶栏承载一级菜单，左侧浅色侧栏承载下级菜单 -->
    <template v-else>
      <rect class="primary" height="9.1" width="104" x="0" y="0" />
      <rect fill="#ffffff" height="7.1" rx="2" width="7.8" x="1.5" y="0.9" />
      <rect fill="#e5e5e5" height="2.8" rx="1.4" width="7.5" x="15.6" y="3.2" />
      <rect fill="#e5e5e5" height="2.8" rx="1.4" width="7.5" x="28.1" y="3.1" />
      <rect fill="#e5e5e5" height="2.8" rx="1.4" width="7.5" x="41.3" y="3.2" />
      <rect fill="#e5e5e5" height="2.8" rx="1.4" width="7.5" x="54.2" y="3.1" />
      <rect fill="#ffffff" height="4.4" rx="1" width="3" x="98.2" y="1.9" />
      <rect fill="currentColor" fill-opacity="0.08" height="56.8" width="15.4" x="0" y="9" />
      <rect fill="currentColor" fill-opacity="0.08" height="4" rx="0.6" width="9.2" x="2.4" y="15" />
      <rect fill="currentColor" fill-opacity="0.08" height="4" rx="0.6" width="9.2" x="2.4" y="28.1" />
      <rect fill="currentColor" fill-opacity="0.08" height="4" rx="0.6" width="9.2" x="2.2" y="40.9" />
      <rect fill="currentColor" fill-opacity="0.08" height="4" rx="0.6" width="9.2" x="2.2" y="54" />
      <rect fill="currentColor" fill-opacity="0.08" height="21" rx="1.7" width="26.6" x="19.4" y="13.7" />
      <rect fill="currentColor" fill-opacity="0.08" height="21.5" rx="2" width="44.1" x="53.4" y="13.5" />
      <rect fill="currentColor" fill-opacity="0.08" height="21.7" rx="2" width="78.4" x="19.9" y="39.3" />
    </template>
  </svg>
</template>

<style scoped lang="scss">
.thumb {
  display: block;
}

.primary {
  fill: var(--kmic-color-primary);
}
</style>
