<script setup lang="ts">
// 面板的布局参照 Vben Admin 的通知面板
import { useQuery, useQueryClient } from '@tanstack/vue-query';
import { Badge, Popover, Spin, message } from 'antdv-next';
import { computed, onScopeDispose, ref, watch } from 'vue';
import { useCommand } from '@/platform/api';
import { useFormat } from '@/platform/format';
import { shellTexts } from '@/platform/texts/shell';
import KIcon from '@/platform/ui/KIcon.vue';
import KTooltip from '@/platform/ui/KTooltip.vue';
import { isInternalLink, siteMessagesApi, type SiteMessage } from './site-messages-api';
import { browserEnvironment, createUnreadCounter } from './unread-counter';

const { userId } = defineProps<{ userId: string }>();
const emit = defineEmits<{ openLink: [path: string] }>();

const t = shellTexts.messages;
const format = useFormat();
const queryClient = useQueryClient();
const open = ref(false);

const counter = createUnreadCounter({
  userId,
  fetchCount: async () => (await queryClient.query(siteMessagesApi.unreadCount.options())).count,
  environment: browserEnvironment(),
});
onScopeDispose(() => {
  counter.stop();
});

const unread = computed(() => counter.count.value ?? 0);

// 只在打开面板时查列表，未读在前、新的在前
const list = useQuery(
  computed(() => ({
    ...siteMessagesApi.list.options({ params: { query: { PageSize: 10 } } }),
    enabled: open.value,
  })),
);
const items = computed<readonly SiteMessage[]>(() => list.data.value?.items ?? []);

watch(open, (isOpen) => {
  if (isOpen) {
    void counter.refresh();
  }
});

const markRead = useCommand(siteMessagesApi.markRead, { invalidates: [siteMessagesApi.list.all] });
const markAllRead = useCommand(siteMessagesApi.markAllRead, { invalidates: [siteMessagesApi.list.all] });

async function openMessage(item: SiteMessage): Promise<void> {
  if (!item.isRead) {
    const result = await markRead.run({ params: { path: { id: item.id } } });
    if (result.ok) {
      counter.set(Math.max(0, unread.value - 1));
    } else {
      // 标已读失败不影响打开链接，下次再标
      markRead.reset();
    }
  }
  if (item.link !== null) {
    open.value = false;
    if (isInternalLink(item.link)) {
      emit('openLink', item.link);
    } else {
      message.warning(t.linkUnavailable);
    }
  }
}

async function readAll(): Promise<void> {
  const result = await markAllRead.run({});
  if (result.ok) {
    counter.set(0);
  } else {
    markAllRead.reset();
    message.error(result.error.message);
  }
}
</script>

<template>
  <Popover v-model:open="open" trigger="click" placement="bottomRight" :arrow="false" :classes="{ root: 'kmic-inbox' }">
    <KTooltip :title="t.button" :disabled="open">
      <button class="inbox-button" :aria-label="t.button">
        <Badge :count="unread" size="small" :overflow-count="99">
          <KIcon name="bell" :size="16" />
        </Badge>
      </button>
    </KTooltip>
    <template #content>
      <div class="inbox">
        <div class="inbox-header">
          <span>{{ t.title }}</span>
          <button
            class="inbox-action"
            :title="t.markAllRead"
            :aria-label="t.markAllRead"
            :disabled="unread === 0 || markAllRead.state.value === 'saving'"
            @click="readAll"
          >
            <KIcon name="mail-check" :size="16" />
          </button>
        </div>
        <ul v-if="items.length > 0" class="inbox-list">
          <li v-for="item in items" :key="item.id">
            <button class="inbox-item" :class="{ 'is-unread': !item.isRead }" @click="openMessage(item)">
              <span class="inbox-icon"><KIcon name="mail" :size="16" /></span>
              <span class="inbox-text">
                <span class="inbox-title">{{ item.title }}</span>
                <span class="inbox-body">{{ item.body }}</span>
                <span class="inbox-time">{{ format.dateTime(item.sentAt) }}</span>
              </span>
              <span v-if="!item.isRead" class="inbox-dot" :title="t.unread" />
            </button>
          </li>
        </ul>
        <div v-else class="inbox-empty">
          <Spin v-if="list.isPending.value && list.isFetching.value" size="small" />
          <template v-else>{{ t.empty }}</template>
        </div>
      </div>
    </template>
  </Popover>
</template>

<style scoped lang="scss">
.inbox-button {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  color: var(--kmic-header-icon-color);
  cursor: pointer;
  background: none;
  border: 0;
  border-radius: 50%;
  transition:
    background var(--kmic-motion-fast),
    color var(--kmic-motion-fast);

  &:hover {
    color: var(--kmic-header-foreground);
    background: var(--kmic-header-accent);
  }

  &:focus-visible {
    outline: 2px solid var(--kmic-color-primary);
  }

  :deep(.ant-badge) {
    color: inherit;
  }
}

.inbox {
  width: 360px;
  max-width: calc(100vw - 32px);
}

.inbox-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 16px;
}

.inbox-action {
  display: flex;
  padding: 6px;
  color: inherit;
  cursor: pointer;
  background: none;
  border: 0;
  border-radius: var(--kmic-radius);

  &:hover:not(:disabled) {
    background: var(--kmic-color-accent);
  }

  &:disabled {
    color: var(--kmic-color-muted-foreground);
    cursor: not-allowed;
  }
}

.inbox-list {
  max-height: 360px;
  padding: 0;
  margin: 0;
  overflow-y: auto;
  list-style: none;
}

.inbox-item {
  position: relative;
  display: flex;
  gap: 12px;
  align-items: flex-start;
  width: 100%;
  padding: 12px 28px 12px 16px;
  font: inherit;
  color: inherit;
  text-align: left;
  cursor: pointer;
  background: none;
  border: 0;
  border-top: 1px solid var(--kmic-color-border);
  transition: background var(--kmic-motion-fast);

  &:hover {
    background: var(--kmic-color-accent);
  }

  &:focus-visible {
    outline: 2px solid var(--kmic-color-primary);
    outline-offset: -2px;
  }
}

.inbox-icon {
  display: flex;
  flex: none;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  color: var(--kmic-color-primary);
  background: var(--kmic-color-primary-soft);
  border-radius: 50%;
}

.inbox-text {
  display: flex;
  flex: 1;
  flex-direction: column;
  gap: 4px;
  min-width: 0;
}

.inbox-title {
  font-weight: 600;
}

.inbox-body {
  display: -webkit-box;
  overflow: hidden;
  font-size: 12px;
  color: var(--kmic-color-muted-foreground);
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  white-space: pre-line;
}

.inbox-time {
  font-size: 12px;
  color: var(--kmic-color-muted-foreground);
}

.inbox-dot {
  position: absolute;
  top: 12px;
  right: 12px;
  width: 8px;
  height: 8px;
  background: var(--kmic-color-primary);
  border-radius: 50%;
}

.inbox-empty {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 150px;
  color: var(--kmic-color-muted-foreground);
  border-top: 1px solid var(--kmic-color-border);
}
</style>

<style lang="scss">
// 浮层挂在 body 下，内边距由面板自己控制
.kmic-inbox .ant-popover-container {
  padding: 0;
}
</style>
