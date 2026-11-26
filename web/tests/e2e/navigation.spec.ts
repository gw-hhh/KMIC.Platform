// 搜索用拼音首字母：sllb 示例列表、slbb 示例报表、xjsl 新建示例
// cspell:ignore sllb slbb xjsl
import { expect, loginToShell, openBySearch, test } from './support';

test('搜索：拼音首字母、汉字、最近访问、没有结果的提示、关闭后焦点回到按钮', async ({ page }) => {
  await loginToShell(page);
  const dialog = page.locator('.kmic-search .search-container');

  await page.keyboard.press('Control+k');
  await expect(dialog).toBeVisible();
  await expect(page.getByRole('combobox')).toBeFocused();
  await expect(dialog.getByText('没有')).toBeVisible();
  await page.keyboard.type('sllb');
  await expect(dialog.locator('.search-item').first().locator('.search-title')).toContainText('示例列表');
  await page.keyboard.press('Enter');
  await expect(dialog).toBeHidden();
  await expect(page).toHaveURL(/\/demo\/items/);

  await page.keyboard.press('Control+k');
  await expect(dialog.locator('.search-item')).toHaveCount(1);
  await page.keyboard.type('zzzz');
  await expect(dialog.getByText('zzzz')).toBeVisible();
  await page.keyboard.press('Control+a');
  await page.keyboard.type('报表');
  await page.keyboard.press('Enter');
  await expect(page).toHaveURL(/\/demo\/report/);

  await page.locator('.search-button').click();
  await expect(dialog).toBeVisible();
  await page.keyboard.press('Escape');
  await expect(page.locator('.search-button')).toBeFocused();
});

test('切回标签：入场时就在原来的滚动位置，焦点和输入的内容都还在', async ({ page }) => {
  await loginToShell(page);
  await openBySearch(page, 'sllb');
  await openBySearch(page, 'slbb');
  await page.getByRole('tab', { name: /示例列表/ }).click();
  // 等切换动画结束，只剩当前页面一层
  await expect(page.locator('.tab-stage > [data-tab-page]')).toHaveCount(1);

  const scroller = page.locator('.tab-content');
  // 示例报表页也有这个输入框，失活的页面被 KeepAlive 留着，只看当前显示的
  const input = page.getByLabel('底部输入框').filter({ visible: true });
  await expect(page).toHaveURL(/\/demo\/items/);
  await input.scrollIntoViewIfNeeded();
  await input.fill('写到一半');
  const savedTop = await scroller.evaluate((el) => el.scrollTop);
  expect(savedTop).toBeGreaterThan(200);

  await page.getByRole('tab', { name: /示例报表/ }).click();
  await expect(page).toHaveURL(/\/demo\/report/);
  const otherTop = await scroller.evaluate((el) => el.scrollTop);

  // 每帧记录滚动位置，确认入场时没有先出现在顶部再跳下去
  await page.evaluate(() => {
    const box = document.querySelector('.tab-content');
    const tops: number[] = [];
    Object.assign(window, { kmicTops: tops });
    const tick = (): void => {
      tops.push(box?.scrollTop ?? -1);
      if (tops.length < 40) {
        requestAnimationFrame(tick);
      }
    };
    requestAnimationFrame(tick);
  });
  await page.getByRole('tab', { name: /示例列表/ }).click();
  await expect.poll(() => page.evaluate(() => (window as unknown as { kmicTops: number[] }).kmicTops.length)).toBe(40);
  const tops = await page.evaluate(() => (window as unknown as { kmicTops: number[] }).kmicTops);
  const afterSwitch = tops.slice(tops.findIndex((top) => top !== otherTop));
  expect(afterSwitch.every((top) => Math.abs(top - savedTop) < 2)).toBe(true);
  await expect(input).toBeFocused();
  await expect(input).toHaveValue('写到一半');
});

test('用键盘切换标签时焦点留在标签栏', async ({ page }) => {
  await loginToShell(page);
  await openBySearch(page, 'sllb');
  await openBySearch(page, 'slbb');

  await page.getByRole('tab', { name: /示例列表/ }).focus();
  await page.keyboard.press('Enter');
  await expect(page).toHaveURL(/\/demo\/items/);

  await expect(page.getByRole('tab', { name: /示例列表/ })).toBeFocused();
});

test('快速连续切换：同时最多一出一进，停在最后点的页面', async ({ page }) => {
  await loginToShell(page);
  for (const keyword of ['sllb', 'slbb', 'xjsl', 'yh']) {
    await openBySearch(page, keyword);
  }
  await page.evaluate(() => {
    const stage = document.querySelector('.tab-stage');
    const state = { layers: 0, stop: false };
    Object.assign(window, { kmicLayers: state });
    const count = (): number => stage?.querySelectorAll(':scope > [data-tab-page]').length ?? 0;
    const tick = (): void => {
      state.layers = Math.max(state.layers, count());
      if (!state.stop) {
        requestAnimationFrame(tick);
      }
    };
    requestAnimationFrame(tick);
  });

  for (const name of [/示例报表/, /新建示例/, /用户（示例）/, /示例列表/, /示例报表/]) {
    await page.getByRole('tab', { name }).click({ delay: 0 });
  }
  await expect(page).toHaveURL(/\/demo\/report/);
  await expect(page.locator('.tab-stage > [data-tab-page]')).toHaveCount(1);

  const layers = await page.evaluate(() => {
    const state = (window as unknown as { kmicLayers: { layers: number; stop: boolean } }).kmicLayers;
    state.stop = true;
    return state.layers;
  });
  expect(layers).toBeLessThanOrEqual(2);
});

test('系统设置了减少动画：过渡时长为 0，切换立即完成', async ({ page }) => {
  await page.emulateMedia({ reducedMotion: 'reduce' });
  await loginToShell(page);
  await openBySearch(page, 'sllb');

  const duration = await page.evaluate(() =>
    getComputedStyle(document.documentElement).getPropertyValue('--kmic-motion-slow').trim(),
  );
  expect(duration).toBe('0ms');
  await page.getByRole('tab', { name: /工作台/ }).click();
  await expect(page.locator('.tab-stage > [data-tab-page]')).toHaveCount(1);
});
