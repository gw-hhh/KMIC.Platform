<script setup lang="ts">
import { Button } from 'antdv-next';
import { useRouter } from 'vue-router';
import { useCurrentUserStore } from '@/platform/session/current-user';
import { useAuth } from '@/platform/session/use-auth';
import { shellTexts } from '@/platform/texts/shell';
import AuthCard from './AuthCard.vue';

const t = shellTexts.selectRole;
const auth = useAuth();
const router = useRouter();
const currentUser = useCurrentUserStore();

async function select(roleId: string): Promise<void> {
  await auth.switchRole(roleId);
  await router.replace('/');
}

async function logout(): Promise<void> {
  await auth.logout();
  await router.replace({ name: 'auth.login' });
}
</script>

<template>
  <AuthCard :title="t.title" :description="t.description">
    <div class="roles">
      <Button v-for="role in currentUser.me?.roles ?? []" :key="role.id" block @click="select(role.id)">{{
        role.name
      }}</Button>
    </div>
    <Button type="link" class="logout" @click="logout">{{ t.logout }}</Button>
  </AuthCard>
</template>

<style scoped lang="scss">
.roles {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.logout {
  margin-top: 16px;
  padding: 0;
}
</style>
