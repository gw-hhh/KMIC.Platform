<script setup lang="ts">
// 悬停提示：浅灰底、细边框、鼠标一移上去就出现，样式照 Vben Admin 的提示（MIT）
import { Tooltip, type TooltipProps } from 'antdv-next';

const {
  title,
  placement = 'bottom',
  disabled,
} = defineProps<{
  title: string;
  placement?: TooltipProps['placement'];
  /** 按钮打开了弹出面板时关掉提示，免得盖住面板 */
  disabled?: boolean;
}>();
</script>

<template>
  <Tooltip
    :title="title"
    :placement="placement"
    :open="disabled ? false : undefined"
    :mouse-enter-delay="0"
    :arrow="false"
    :classes="{ root: 'k-tooltip' }"
  >
    <slot />
  </Tooltip>
</template>

<style lang="scss">
// 浮层挂在 body 下，只能用不带 scoped 的样式
.k-tooltip .ant-tooltip-container {
  min-height: 0;
  padding: 6px 12px;
  font-size: 12px;
  line-height: 1.5;
  color: var(--kmic-color-foreground);
  background: var(--kmic-color-accent);
  border: 1px solid var(--kmic-color-border);
  border-radius: var(--kmic-radius-md);
  box-shadow: none;
}
</style>
