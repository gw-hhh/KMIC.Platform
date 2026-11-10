<script setup lang="ts">
import { other } from '@/modules/other';
import { other as relativeOther } from '../../other/index';
import { Select } from 'antdv-next';
</script>

<template>
  <Select :value="other + relativeOther" />
</template>
