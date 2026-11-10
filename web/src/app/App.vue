<script setup lang="ts">
import { ConfigProvider } from 'antdv-next';
import WorkspaceHost from '@/shell/WorkspaceHost.vue';

const zeroRuntime = import.meta.env.VITE_ZERO_RUNTIME === '1';
// 生产中由 Nginx 按请求生成并同时写入响应头与这个 meta（原型中用固定值模拟）。
const nonce = document.querySelector<HTMLMetaElement>('meta[name="kmic-csp-nonce"]')?.content;
const csp = nonce ? { nonce } : {};
</script>

<template>
  <ConfigProvider :csp="csp" :theme="{ cssVar: { key: 'kmic' }, hashed: false, zeroRuntime }">
    <WorkspaceHost />
  </ConfigProvider>
</template>
