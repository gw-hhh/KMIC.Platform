<script setup lang="ts">
import Shell from '@/shell/Shell.vue';

defineProps<{ name?: string }>();
console.info(Shell);
</script>

<template>
  <div>订单</div>
</template>
