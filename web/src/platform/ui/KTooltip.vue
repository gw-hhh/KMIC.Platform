<script setup lang="ts">
// 悬停提示：浅灰底色、细边框与零延迟触发，样式参考 Vben Admin（MIT）
import { Tooltip, type TooltipProps } from 'antdv-next';

const {
  title,
  placement = 'bottom',
  disabled,
} = defineProps<{
  title: string;
  placement?: TooltipProps['placement'];
  /** 弹出面板打开时隐藏提示，避免遮挡 */
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
// 浮层挂载于 body，需使用全局非 scoped 样式
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
