import { mount } from '@vue/test-utils';
import { http, HttpResponse } from 'msw';
import { afterAll, beforeAll, beforeEach, describe, expect, test } from 'vitest';
import { userEvent } from 'vitest/browser';
import SampleForm from '@/modules/sample/pages/SampleForm.vue';
import { startWorker, worker } from './msw';

const requests: unknown[] = [];

beforeAll(startWorker);
afterAll(() => worker.stop());
beforeEach(() => {
  requests.length = 0;
  document.body.innerHTML = '';
  worker.resetHandlers(
    http.post('/api/v1/sample/samples', async ({ request }) => {
      requests.push(await request.json());
      return HttpResponse.json(
        {
          status: 400,
          code: 'Common.ValidationFailed',
          title: '提交的数据有误',
          errors: [
            { path: 'name', code: 'Sample.NameTaken', message: '名称已被使用' },
            { path: 'items[1].quantity', code: 'Sample.QuantityTooLarge', message: '数量超过库存' },
            { path: 'customerId', code: 'Sample.CustomerMissing', message: '客户不存在' },
          ],
        },
        { status: 400 },
      );
    }),
  );
});

const text = (testId: string) => document.querySelector(`[data-testid="${testId}"]`)?.textContent?.trim() ?? '';
const input = (testId: string) => document.querySelector<HTMLInputElement>(`[data-testid="${testId}"] input, input[data-testid="${testId}"]`)!;
const click = (testId: string) => userEvent.click(document.querySelector<HTMLElement>(`[data-testid="${testId}"]`)!);

async function fillValidForm() {
  await userEvent.fill(input('name'), '样品一');
  await click('add-item');
  await expect.poll(() => document.querySelector('[data-testid="qty-1"]')).not.toBeNull();
}

describe('服务端字段错误回填', () => {
  test('按路径回填到字段（含数组下标），找不到字段的汇总显示', async () => {
    mount(SampleForm, { attachTo: document.body });
    await fillValidForm();
    await click('submit');

    await expect.poll(() => text('name-error')).toBe('名称已被使用');
    await expect.poll(() => text('qty-1-error')).toBe('数量超过库存');
    expect(text('qty-0-error')).toBe('');
    expect(text('form-errors')).toBe('客户不存在');
    expect(requests).toEqual([{ name: '样品一', items: [{ quantity: 1 }, { quantity: 1 }] }]);
  });

  test('用户修改字段后，该字段的服务端错误清除，其他字段的保留', async () => {
    mount(SampleForm, { attachTo: document.body });
    await fillValidForm();
    await click('submit');
    await expect.poll(() => text('qty-1-error')).toBe('数量超过库存');

    await userEvent.fill(input('qty-1'), '2');

    await expect.poll(() => text('qty-1-error')).toBe('');
    expect(text('name-error')).toBe('名称已被使用');
  });

  test('前端 Zod 规则即时提示数组元素的错误', async () => {
    mount(SampleForm, { attachTo: document.body });
    await userEvent.fill(input('qty-0'), '0');

    await expect.poll(() => text('qty-0-error')).toBe('数量必须大于 0');
  });

  test('不修改直接再次提交：服务端错误阻止提交，修改后才能再提交', async () => {
    mount(SampleForm, { attachTo: document.body });
    await fillValidForm();
    await click('submit');
    await expect.poll(() => requests.length).toBe(1);
    await expect.poll(() => text('name-error')).toBe('名称已被使用');

    await click('submit');
    await new Promise((resolve) => setTimeout(resolve, 300));

    // 服务端错误计入字段错误，表单处于不可提交状态：不修改就不会再发请求。
    expect(requests.length).toBe(1);
  });
});
