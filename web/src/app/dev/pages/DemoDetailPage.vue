<script setup lang="ts">
import { Card, Checkbox } from 'antdv-next';
import { ref } from 'vue';
import { useRoute } from 'vue-router';
import { usePage } from '@/platform/page';

const route = useRoute();
const page = usePage();
const id = String(route.params['id']);
const edited = ref(false);

page.setTitle(`示例 ${id}`);
page.trackDirty(() => edited.value);
</script>

<template>
  <Card :title="`示例 ${id}`">
    <Checkbox v-model:checked="edited">模拟有未保存的修改</Checkbox>
  </Card>
</template>
