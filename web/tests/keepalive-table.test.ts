import { VueQueryPlugin } from '@tanstack/vue-query';
import { mount } from '@vue/test-utils';
import { createPinia } from 'pinia';
import { afterEach, describe, expect, test } from 'vitest';
import { page, userEvent } from 'vitest/browser';
import { nextTick } from 'vue';
import App from '@/app/App.vue';
import SampleList from '@/modules/sample/pages/SampleList.vue';
import { installTable } from '@/platform/table/install';

function mountApp() {
  return mount(App, {
    attachTo: document.body,
    global: { plugins: [createPinia(), VueQueryPlugin, { install: installTable }] },
  });
}

const tableRoot = () => document.querySelector<HTMLElement>('[data-testid="sample-table"]')!;

function layout() {
  const root = tableRoot();
  const header = root.querySelector<HTMLElement>('.vxe-table--header')!;
  const firstRow = root.querySelector<HTMLElement>('.vxe-body--row');
  const rowTop = firstRow?.getBoundingClientRect().top ?? 0;
  const visibleNumbers = [...root.querySelectorAll<HTMLElement>('.vxe-table--main-wrapper .vxe-body--row')]
    .filter((row) => {
      const rect = row.getBoundingClientRect();
      return rect.bottom > rowTop && rect.top < rowTop + 380;
    })
    .map((row) => row.querySelector('[colid] .vxe-cell')?.textContent?.trim());
  return { rootWidth: root.clientWidth, headerWidth: header.getBoundingClientRect().width, visibleNumbers };
}

async function run(recalculate: boolean) {
  history.replaceState(null, '', recalculate ? '?recalc=1' : '?recalc=0');
  await page.viewport(1280, 800);
  const wrapper = mountApp();
  await expect.poll(() => tableRoot()?.querySelectorAll('.vxe-body--row').length ?? 0).toBeGreaterThan(5);

  const list = wrapper.findComponent(SampleList);
  const table = (list.vm as unknown as {
    table: { scrollToRow: (row: unknown) => Promise<void>; getData: (index: number) => unknown; getScroll: () => { scrollTop: number } };
  }).table;
  await table.scrollToRow(table.getData(999));
  await expect.poll(() => table.getScroll().scrollTop, { timeout: 3000 }).toBeGreaterThan(0);
  await expect.poll(() => layout().visibleNumbers[0]).not.toBe('SP-00001');
  const before = { scrollTop: table.getScroll().scrollTop, ...layout() };

  await userEvent.click(document.querySelector<HTMLElement>('[data-testid="tab-form"]')!);
  await expect.poll(() => document.querySelector('[data-testid="sample-form"]')).not.toBeNull();
  await page.viewport(900, 700);
  await new Promise((resolve) => setTimeout(resolve, 200));
  await userEvent.click(document.querySelector<HTMLElement>('[data-testid="tab-list"]')!);
  await nextTick();
  await new Promise((resolve) => setTimeout(resolve, 300));
  const after = { scrollTop: table.getScroll().scrollTop, ...layout() };

  wrapper.unmount();
  return { before, after };
}

afterEach(() => {
  document.body.innerHTML = '';
});

describe('KeepAlive 下的 VXE 虚拟滚动表格', () => {
  test('激活时重新计算：滚动位置保留，宽度跟随窗口，可见行正确', async () => {
    const { before, after } = await run(true);
    console.info('recalc=1', JSON.stringify({ before, after }));

    expect(after.scrollTop).toBe(before.scrollTop);
    expect(after.visibleNumbers[0]).toBe(before.visibleNumbers[0]);
    expect(after.rootWidth).toBeLessThan(before.rootWidth);
    // 表头随容器同步缩小（两者之差是纵向滚动条的宽度，前后一致）。
    expect(after.rootWidth - after.headerWidth).toBe(before.rootWidth - before.headerWidth);
  });

  test('不手动重新计算：VXE 在激活时自行恢复滚动并按新宽度布局', async () => {
    const { before, after } = await run(false);
    console.info('recalc=0', JSON.stringify({ before, after }));

    expect(before.scrollTop).toBeGreaterThan(0);
    expect(after.scrollTop).toBe(before.scrollTop);
    expect(after.visibleNumbers[0]).toBe(before.visibleNumbers[0]);
    expect(after.rootWidth - after.headerWidth).toBe(before.rootWidth - before.headerWidth);
  });
});
