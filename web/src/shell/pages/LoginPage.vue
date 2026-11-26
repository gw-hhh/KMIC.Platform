<script setup lang="ts">
import { Alert, Button, Form, FormItem, Input, InputPassword } from 'antdv-next';
import { reactive, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { KmicError } from '@/platform/api';
import { useAuth } from '@/platform/session/use-auth';
import { shellTexts } from '@/platform/texts/shell';
import AuthLayout from './AuthLayout.vue';

const t = shellTexts.login;
const auth = useAuth();
const route = useRoute();
const router = useRouter();

const form = reactive({ account: '', password: '' });
const submitting = ref(false);
const errorMessage = ref('');

async function submit(): Promise<void> {
  submitting.value = true;
  errorMessage.value = '';
  try {
    await auth.login(form.account.trim(), form.password);
    // 跳转到哪由路由守卫决定
    const redirect = route.query['redirect'];
    await router.replace(
      typeof redirect === 'string' && redirect.startsWith('/') && !redirect.startsWith('//') ? redirect : '/',
    );
  } catch (error) {
    if (!(error instanceof KmicError)) {
      throw error;
    }
    errorMessage.value = error.message;
  } finally {
    submitting.value = false;
  }
}
</script>

<template>
  <AuthLayout :title="t.title" :description="t.description">
    <Alert v-if="errorMessage" class="error" type="error" show-icon :message="errorMessage" />
    <Form layout="vertical" :model="form" @finish="submit">
      <FormItem name="account" :label="t.account" :rules="[{ required: true, message: t.accountRequired }]">
        <Input v-model:value="form.account" autocomplete="username" autofocus />
      </FormItem>
      <FormItem name="password" :label="t.password" :rules="[{ required: true, message: t.passwordRequired }]">
        <InputPassword v-model:value="form.password" autocomplete="current-password" />
      </FormItem>
      <Button type="primary" html-type="submit" block :loading="submitting">{{ t.submit }}</Button>
    </Form>
  </AuthLayout>
</template>

<style scoped lang="scss">
.error {
  margin-bottom: 16px;
}
</style>
