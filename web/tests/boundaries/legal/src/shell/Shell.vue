<script setup lang="ts">
import type { Client } from '@/platform/api';
import type { Paths } from '@/generated/api';

defineProps<{ client: Client; paths: Paths }>();
</script>

<template>
  <div>外壳</div>
</template>
