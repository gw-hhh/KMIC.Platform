<script setup lang="ts">
import { ConfigProvider } from 'antdv-next';
import zhCN from 'antdv-next/locale/zh_CN';
import { watchEffect } from 'vue';
import { RouterView, useRoute } from 'vue-router';
import ShellLayout from '@/shell/layout/ShellLayout.vue';
import { homeRouteName } from '@/shell/routes';
import { useShellSettingsStore } from '@/shell/settings/shell-settings-store';

const route = useRoute();
const shellSettings = useShellSettingsStore();

// 静态方法（Modal.confirm 等）要单独设置主题
watchEffect(() => {
  ConfigProvider.config({ theme: shellSettings.antdTheme, locale: zhCN });
});
</script>

<template>
  <ConfigProvider :theme="shellSettings.antdTheme" :locale="zhCN">
    <RouterView v-if="route.meta['layout'] === 'blank'" />
    <ShellLayout v-else-if="route.name !== undefined" :home-route-name="homeRouteName" />
  </ConfigProvider>
</template>
