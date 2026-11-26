<script setup lang="ts">
// 偏好设置里的开关行：点整行都能切换，照 Vben 的 SwitchItem（MIT）
import { Switch } from 'antdv-next';

const { label, checked } = defineProps<{ label: string; checked: boolean }>();

const emit = defineEmits<{ toggle: [] }>();
</script>

<template>
  <div class="switch-row" @click="emit('toggle')">
    {{ label }}
    <!-- 开关自己处理点击和键盘，点击不能再冒泡到整行，否则切两次 -->
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
