<script setup lang="ts">
import { useForm } from '@tanstack/vue-form';
import { Button, Input, InputNumber } from 'antdv-next';
import { ref } from 'vue';
import { z } from 'zod';
import { postJson } from '@/platform/api/client';
import { applyServerErrors, clearServerError, type ServerFieldError } from '@/platform/form/serverErrors';

const schema = z.object({
  name: z.string().min(1, '请输入名称'),
  items: z.array(z.object({ quantity: z.number().positive('数量必须大于 0') })),
});

const unmatched = ref<ServerFieldError[]>([]);
const saved = ref(false);

const form = useForm({
  defaultValues: { name: '', items: [{ quantity: 1 }] } as z.input<typeof schema>,
  validators: { onChange: schema },
  onSubmit: async ({ value }) => {
    const result = await postJson('/api/v1/sample/samples', value);
    if (result.ok) {
      saved.value = true;
      return;
    }
    unmatched.value = applyServerErrors(form, result.problem.errors ?? []);
  },
});

function messages(errors: unknown[]): string {
  return errors
    .map((error) => (typeof error === 'string' ? error : (error as { message?: string } | undefined)?.message))
    .filter(Boolean)
    .join('；');
}
</script>

<template>
  <form data-testid="sample-form" @submit.prevent="form.handleSubmit()">
    <form.Field name="name" :listeners="{ onChange: clearServerError }">
      <template #default="{ field }">
        <label>名称
          <Input
            data-testid="name"
            :value="field.state.value"
            @update:value="(value: string) => field.handleChange(value)"
          />
        </label>
        <span data-testid="name-error">{{ messages(field.state.meta.errors) }}</span>
      </template>
    </form.Field>

    <form.Field name="items" mode="array">
      <template #default="{ field }">
        <div v-for="(_, index) in field.state.value" :key="index">
          <form.Field :name="`items[${index}].quantity`" :listeners="{ onChange: clearServerError }">
            <template #default="{ field: sub }">
              <InputNumber
                :data-testid="`qty-${index}`"
                :value="sub.state.value"
                @update:value="(value: number | null) => sub.handleChange(value ?? 0)"
              />
              <span :data-testid="`qty-${index}-error`">{{ messages(sub.state.meta.errors) }}</span>
            </template>
          </form.Field>
        </div>
        <Button data-testid="add-item" @click="field.pushValue({ quantity: 1 })">添加明细</Button>
      </template>
    </form.Field>

    <ul data-testid="form-errors">
      <li v-for="error in unmatched" :key="error.path">{{ error.message }}</li>
    </ul>
    <Button html-type="submit" type="primary" data-testid="submit">保存</Button>
    <span v-if="saved" data-testid="saved">已保存</span>
  </form>
</template>
