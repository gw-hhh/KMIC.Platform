<script setup lang="ts">
import { ConfigProvider } from 'antdv-next';
import zhCN from 'antdv-next/locale/zh_CN';
import { h, watchEffect } from 'vue';
import { RouterView, useRoute } from 'vue-router';
import ShellLayout from '@/shell/layout/ShellLayout.vue';
import { homeRouteName } from '@/shell/routes';
import { useShellSettingsStore } from '@/shell/settings/shell-settings-store';

const route = useRoute();
const shellSettings = useShellSettingsStore();

// 禁止两字按钮自动插入空格（如保持“登录”而非“登 录”）
const button = { autoInsertSpace: false };

// 静态方法（如 Modal.confirm）脱离组件树渲染，需单独注入主题与按钮配置
watchEffect(() => {
  ConfigProvider.config({
    theme: shellSettings.antdTheme,
    locale: zhCN,
    holderRender: (children) => h(ConfigProvider, { button }, () => children),
  });
});
</script>

<template>
  <ConfigProvider :theme="shellSettings.antdTheme" :locale="zhCN" :button="button">
    <RouterView v-if="route.meta['layout'] === 'blank'" />
    <ShellLayout v-else-if="route.name !== undefined" :home-route-name="homeRouteName" />
  </ConfigProvider>
</template>
