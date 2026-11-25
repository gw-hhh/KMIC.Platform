<script setup lang="ts">
import { useRouter } from 'vue-router';
import { shellTexts } from '@/platform/texts/shell';
import StatePage from './StatePage.vue';

const t = shellTexts.pages;
const router = useRouter();
</script>

<template>
  <StatePage
    status="404"
    :title="t.notFound"
    :description="t.notFoundHint"
    :primary="t.backHome"
    @primary="router.push('/')"
  />
</template>
