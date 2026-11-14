<script setup lang="ts">
import { customers } from '@/modules/customers/api';

defineProps<{ name?: string }>();
console.info(customers);
</script>

<template>
  <div>订单</div>
</template>
