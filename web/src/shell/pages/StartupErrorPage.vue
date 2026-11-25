<script setup lang="ts">
import { Button } from 'antdv-next';
import { useRouter } from 'vue-router';
import { useCurrentUserStore } from '@/platform/session/current-user';
import { useAuth } from '@/platform/session/use-auth';
import { shellTexts } from '@/platform/texts/shell';
import AuthCard from './AuthCard.vue';

const t = shellTexts.startupError;
const auth = useAuth();
const router = useRouter();
const currentUser = useCurrentUserStore();

async function retry(): Promise<void> {
  await auth.start();
  await router.replace('/');
}

async function logout(): Promise<void> {
  await auth.logout();
  await router.replace({ name: 'auth.login' });
}
</script>

<template>
  <AuthCard :title="t.title" :description="currentUser.failure?.message">
    <p v-if="currentUser.failure?.traceId" class="trace">{{ t.traceId }}：{{ currentUser.failure.traceId }}</p>
    <div class="actions">
      <Button type="primary" @click="retry">{{ t.retry }}</Button>
      <Button @click="logout">{{ t.logout }}</Button>
    </div>
  </AuthCard>
</template>

<style scoped lang="scss">
.trace {
  color: var(--kmic-color-muted-foreground);
}

.actions {
  display: flex;
  gap: 8px;
}
</style>
