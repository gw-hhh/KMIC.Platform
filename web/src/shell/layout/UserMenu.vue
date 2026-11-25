<script setup lang="ts">
import { Avatar, Popover, Tag } from 'antdv-next';
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
    <button class="avatar-button" :title="me.displayName">
      <Avatar :size="32" class="avatar">{{ initial }}</Avatar>
    </button>
    <template #content>
      <div class="profile">
        <Avatar :size="48" class="avatar">{{ initial }}</Avatar>
        <div class="profile-text">
          <div class="name">
            {{ me.displayName }}
            <Tag class="role" :bordered="false" color="success">{{ me.currentRole.name }}</Tag>
          </div>
          <div class="account">{{ me.account }}</div>
        </div>
      </div>
      <div class="divider" />
      <button class="item" @click="run('changePassword')">
        <KIcon name="key" :size="16" />
        {{ t.changePassword }}
      </button>
      <div class="divider" />
      <button class="item" @click="run('logout')">
        <KIcon name="log-out" :size="16" />
        {{ t.logout }}
      </button>
    </template>
  </Popover>
</template>

<style scoped lang="scss">
.avatar-button {
  display: flex;
  padding: 4px;
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
  color: var(--kmic-color-primary-foreground);
  background: var(--kmic-color-primary);
}

.profile {
  display: flex;
  gap: 8px;
  align-items: center;
  min-width: 220px;
  padding: 8px 8px 12px;
}

.name {
  display: flex;
  gap: 8px;
  align-items: center;
  font-weight: 500;
}

.role {
  margin: 0;
}

.account {
  margin-top: 4px;
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
  gap: 10px;
  align-items: center;
  width: 100%;
  height: 36px;
  padding: 0 8px;
  color: inherit;
  cursor: pointer;
  background: none;
  border: 0;
  border-radius: var(--kmic-radius);
  transition: background var(--kmic-motion-fast);

  &:hover {
    background: var(--kmic-color-accent);
  }
}
</style>
