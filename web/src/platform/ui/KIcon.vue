<script setup lang="ts">
import { computed } from 'vue';
import { icons, type IconName } from './icons';

const { name, size = 16 } = defineProps<{ name: IconName; size?: number }>();

const component = computed(() => icons[name]);
const dimension = computed(() => `${String(size)}px`);
</script>

<template>
  <component :is="component" class="k-icon" :width="dimension" :height="dimension" aria-hidden="true" />
</template>

<style scoped lang="scss">
.k-icon {
  flex: none;
}
</style>
