<script setup lang="ts">
import { Card, Input } from 'antdv-next';
import { ref } from 'vue';
import { usePage } from '@/platform/page';

const name = ref('');

usePage().trackDirty(() => name.value !== '');
</script>

<template>
  <Card title="新建示例">
    <Input v-model:value="name" placeholder="随便输入内容后尝试关闭标签页" />
  </Card>
</template>
