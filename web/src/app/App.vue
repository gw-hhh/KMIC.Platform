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

// 两个汉字的按钮不插空格（“登录”而不是“登 录”），和 Vben 一致
const button = { autoInsertSpace: false };

// 静态方法（Modal.confirm 等）不在组件树里，主题和按钮设置要单独给
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
