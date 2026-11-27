<script setup lang="ts">
import { Avatar, Popover } from 'antdv-next';
import { computed, ref } from 'vue';
import type { Me } from '@/platform/session/current-user';
import { shellTexts } from '@/platform/texts/shell';
import KIcon from '@/platform/ui/KIcon.vue';

const { me } = defineProps<{ me: Me }>();

const emit = defineEmits<{ changePassword: []; logout: [] }>();

const t = shellTexts.header;
const open = ref(false);
const initial = computed(() => me.displayName.slice(0, 1).toUpperCase());

function run(action: 'changePassword' | 'logout'): void {
  open.value = false;
  if (action === 'changePassword') {
    emit('changePassword');
  } else {
    emit('logout');
  }
}
</script>

<template>
  <Popover v-model:open="open" trigger="click" placement="bottomRight" :arrow="false" :classes="{ root: 'user-menu' }">
    <button class="avatar-button" :aria-label="me.displayName">
      <Avatar :size="32" class="avatar">{{ initial }}</Avatar>
    </button>
    <template #content>
      <div class="profile">
        <Avatar :size="48" class="avatar">{{ initial }}</Avatar>
        <div class="profile-text">
          <div class="name">
            {{ me.displayName }}
            <span class="role">{{ me.currentRole.name }}</span>
          </div>
          <div class="account">{{ me.account }}</div>
        </div>
      </div>
      <div class="divider" />
      <button class="item" @click="run('changePassword')">
        <span class="item-icon"><KIcon name="key" :size="16" /></span>
        {{ t.changePassword }}
      </button>
      <div class="divider" />
      <button class="item" @click="run('logout')">
        <span class="item-icon"><KIcon name="log-out" :size="16" /></span>
        {{ t.logout }}
      </button>
    </template>
  </Popover>
</template>

<style lang="scss">
// 浮层挂载于 body，需使用全局非 scoped 样式
.user-menu .ant-popover-container {
  min-width: 240px;
  padding: 0 0 4px;
  border: 1px solid var(--kmic-color-border);
  border-radius: var(--kmic-radius-md);
}
</style>

<style scoped lang="scss">
.avatar-button {
  display: flex;
  padding: 6px;
  margin: 0 8px 0 4px;
  cursor: pointer;
  background: none;
  border: 0;
  border-radius: 50%;
  transition: background var(--kmic-motion-fast);

  &:hover {
    background: var(--kmic-header-accent);
  }

  &:focus-visible {
    outline: 2px solid var(--kmic-color-primary);
  }
}

.avatar {
  flex: none;
  color: var(--kmic-color-primary-foreground);
  background: var(--kmic-color-primary);
}

.profile {
  display: flex;
  gap: 8px;
  align-items: center;
  padding: 12px;
}

.name {
  display: flex;
  gap: 8px;
  align-items: center;
  margin-bottom: 4px;
  font-weight: 500;
  color: var(--kmic-color-foreground);
}

.role {
  padding: 0 6px;
  font-size: 12px;
  font-weight: 500;
  line-height: 20px;
  color: var(--kmic-color-success);
  background: var(--kmic-color-accent);
  border-radius: var(--kmic-radius-sm);
}

.account {
  font-size: 12px;
  color: var(--kmic-color-muted-foreground);
}

.divider {
  height: 1px;
  margin: 4px 0;
  background: var(--kmic-color-border);
}

.item {
  display: flex;
  align-items: center;
  width: calc(100% - 8px);
  height: 40px;
  padding: 0 8px;
  margin: 0 4px;
  font: inherit;
  color: inherit;
  cursor: pointer;
  background: none;
  border: 0;
  border-radius: var(--kmic-radius-sm);
  transition: background var(--kmic-motion-fast);

  &:hover {
    color: var(--kmic-color-accent-foreground);
    background: var(--kmic-color-accent);
  }
}

.item-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  margin-right: 8px;
  margin-left: -8px;
  border-radius: 50%;
}
</style>
