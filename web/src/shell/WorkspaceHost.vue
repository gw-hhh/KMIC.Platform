<script setup lang="ts">
import { Button, Space } from 'antdv-next';
import { computed, defineComponent, h, markRaw, shallowRef, type Component } from 'vue';
import SampleForm from '@/modules/sample/pages/SampleForm.vue';
import SampleList from '@/modules/sample/pages/SampleList.vue';

interface Tab {
  key: string;
  title: string;
  page: Component;
}

// KeepAlive 按组件名匹配，同一页面组件开多个标签时，每个标签需要一个以标签键命名的包装组件。
const wrappers = new Map<string, Component>();
function wrapperFor(tab: Tab): Component {
  let wrapper = wrappers.get(tab.key);
  if (!wrapper) {
    wrapper = markRaw(defineComponent({ name: tab.key, setup: () => () => h(tab.page) }));
    wrappers.set(tab.key, wrapper);
  }
  return wrapper;
}

const tabs = shallowRef<Tab[]>([
  { key: 'tab-list', title: '列表', page: SampleList },
  { key: 'tab-form', title: '表单', page: SampleForm },
]);
const activeKey = shallowRef('tab-list');
const active = computed(() => tabs.value.find((tab) => tab.key === activeKey.value));
const include = computed(() => tabs.value.map((tab) => tab.key));
</script>

<template>
  <Space data-testid="tabbar">
    <Button
      v-for="tab in tabs"
      :key="tab.key"
      :type="tab.key === activeKey ? 'primary' : 'default'"
      :data-testid="tab.key"
      @click="activeKey = tab.key"
    >
      {{ tab.title }}
    </Button>
  </Space>
  <main class="content">
    <KeepAlive :include="include" :max="15">
      <component :is="wrapperFor(active)" v-if="active" :key="active.key" />
    </KeepAlive>
  </main>
</template>

<style scoped lang="scss">
.content {
  padding: var(--kmic-space-md);
}
</style>
