<script setup lang="ts">
import { Alert, Button, Form, FormItem, InputPassword, message } from 'antdv-next';
import { reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import { KmicError } from '@/platform/api';
import { usePage } from '@/platform/page';
import { useCurrentUserStore } from '@/platform/session/current-user';
import { useAuth } from '@/platform/session/use-auth';
import { shellTexts } from '@/platform/texts/shell';
import AuthLayout from './AuthLayout.vue';

const { standalone } = defineProps<{ standalone: boolean }>();

const t = shellTexts.changePassword;
const auth = useAuth();
const router = useRouter();
const currentUser = useCurrentUserStore();

const form = reactive({ oldPassword: '', newPassword: '', confirm: '' });
const submitting = ref(false);
const errorMessage = ref('');

// 独立认证页不属于工作区标签页，不具备 PageInstance 上下文，不可调用 usePage
if (!standalone) {
  usePage().trackDirty(() => form.oldPassword !== '' || form.newPassword !== '' || form.confirm !== '');
}

const needsOldPassword = () => currentUser.me?.hasPassword ?? true;

async function submit(): Promise<void> {
  submitting.value = true;
  errorMessage.value = '';
  try {
    await auth.changePassword(needsOldPassword() ? form.oldPassword : null, form.newPassword);
    message.success(t.done);
    form.oldPassword = form.newPassword = form.confirm = '';
    if (standalone) {
      await router.replace('/');
    }
  } catch (error) {
    if (!(error instanceof KmicError)) {
      throw error;
    }
    errorMessage.value = error.message;
  } finally {
    submitting.value = false;
  }
}

const confirmRule = {
  validator: (_: unknown, value: string) =>
    value === form.newPassword ? Promise.resolve() : Promise.reject(new Error(t.mismatch)),
};
</script>

<template>
  <component
    :is="standalone ? AuthLayout : 'div'"
    v-bind="standalone ? { title: t.title, description: t.required } : { class: 'inline' }"
  >
    <Alert v-if="errorMessage" class="error" type="error" show-icon :message="errorMessage" />
    <Form layout="vertical" :model="form" @finish="submit">
      <FormItem
        v-if="needsOldPassword()"
        name="oldPassword"
        :label="t.oldPassword"
        :rules="[{ required: true, message: t.oldRequired }]"
      >
        <InputPassword v-model:value="form.oldPassword" autocomplete="current-password" />
      </FormItem>
      <FormItem name="newPassword" :label="t.newPassword" :rules="[{ required: true, message: t.newRequired }]">
        <InputPassword v-model:value="form.newPassword" autocomplete="new-password" />
      </FormItem>
      <FormItem name="confirm" :label="t.confirmPassword" :rules="[confirmRule]">
        <InputPassword v-model:value="form.confirm" autocomplete="new-password" />
      </FormItem>
      <Button type="primary" html-type="submit" :block="standalone" :loading="submitting">{{ t.submit }}</Button>
    </Form>
  </component>
</template>

<style scoped lang="scss">
.inline {
  max-width: 400px;
  padding: 24px;
  background: var(--kmic-color-background);
  border-radius: var(--kmic-radius);
}

.error {
  margin-bottom: 16px;
}
</style>
