<script setup lang="ts">
import { Button, Result } from 'antdv-next';

defineProps<{
  status: '403' | '404' | 'error';
  title: string;
  description: string;
  primary?: string;
  secondary?: string;
}>();

defineEmits<{ primary: []; secondary: [] }>();
</script>

<template>
  <div class="state-page">
    <Result :status="status" :title="title" :sub-title="description">
      <template #extra>
        <Button v-if="primary" type="primary" @click="$emit('primary')">{{ primary }}</Button>
        <Button v-if="secondary" @click="$emit('secondary')">{{ secondary }}</Button>
      </template>
    </Result>
  </div>
</template>

<style scoped lang="scss">
.state-page {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 60%;
}
</style>
