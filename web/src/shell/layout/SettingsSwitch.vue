<script setup lang="ts">
// 偏好设置开关行组件：支持点击整行切换，参考 Vben 的 SwitchItem（MIT）
import { Switch } from 'antdv-next';

const { label, checked } = defineProps<{ label: string; checked: boolean }>();

const emit = defineEmits<{ toggle: [] }>();
</script>

<template>
  <div class="switch-row" @click="emit('toggle')">
    {{ label }}
    <!-- 阻止点击事件冒泡到整行，避免触发两次切换 -->
    <span @click.stop>
      <Switch size="small" :checked="checked" :aria-label="label" @change="emit('toggle')" />
    </span>
  </div>
</template>

<style scoped lang="scss">
.switch-row {
  box-sizing: border-box;
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
  padding: 10px 8px;
  margin: 4px 0;
  cursor: pointer;
  border-radius: var(--kmic-radius-md);

  &:hover {
    background: var(--kmic-color-accent);
  }
}
</style>
