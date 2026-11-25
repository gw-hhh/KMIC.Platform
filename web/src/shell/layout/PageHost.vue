<script setup lang="ts">
import { Spin } from 'antdv-next';
import { computed, onMounted, shallowRef, type Component } from 'vue';
import { providePage } from '@/platform/page';
import type { RouteCatalog } from '@/platform/routing';
import { useCurrentUserStore } from '@/platform/session/current-user';
import { shellTexts } from '@/platform/texts/shell';
import StatePage from '../pages/StatePage.vue';
import type { Workspace } from '../workspace/workspace';

const { tabKey, routeName, workspace, catalog } = defineProps<{
  tabKey: string;
  routeName: string;
  workspace: Workspace;
  catalog: RouteCatalog;
}>();

const t = shellTexts.pages;
const currentUser = useCurrentUserStore();

const page = workspace.pageOf(tabKey);
if (page) {
  providePage(page);
}

const entry = catalog.find(routeName);
const component = shallowRef<Component | null>(null);
const failed = shallowRef(false);

const allowed = computed(() => entry?.requires.every((permission) => currentUser.has(permission)) ?? false);

async function load(): Promise<void> {
  failed.value = false;
  try {
    component.value = (await entry?.component())?.default ?? null;
  } catch {
    failed.value = true;
  }
}

function reloadApp(): void {
  location.reload();
}

onMounted(load);
</script>

<template>
  <div class="page-host">
    <StatePage v-if="!allowed" status="403" :title="t.noAccess" :description="t.noAccessHint" />
    <StatePage
      v-else-if="failed"
      status="error"
      :title="t.loadFailed"
      :description="t.loadFailedHint"
      :primary="t.retry"
      :secondary="t.reloadApp"
      @primary="load"
      @secondary="reloadApp"
    />
    <component :is="component" v-else-if="component" />
    <div v-else class="loading"><Spin /></div>
  </div>
</template>

<style scoped lang="scss">
.page-host {
  min-height: 100%;
}

.loading {
  display: flex;
  align-items: center;
  justify-content: center;
  padding-top: 120px;
}
</style>
