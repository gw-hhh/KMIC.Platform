import type { Page } from '@playwright/test';
import { expect, loginToShell, test } from './support';

const mainLeft = (page: Page): Promise<number> =>
  page.locator('div.main').evaluate((el) => Math.round(el.getBoundingClientRect().left));
// 拖拽宽度换算允许 1 像素的取整误差
const offBy = async (page: Page, target: number): Promise<number> => Math.abs((await mainLeft(page)) - target);
const sidebarWidth = (page: Page): Promise<number> =>
  page.locator('aside.sidebar').evaluate((el) => Math.round(el.getBoundingClientRect().width));

test('拖拽侧栏：拖拽过程仅预览线跟随，释放后更新宽度；拖至最窄折叠为图标，双击恢复默认宽度', async ({ page }) => {
  await loginToShell(page);
  const bar = page.locator('.drag-bar');
  const box = await bar.boundingBox();
  if (box === null) {
    throw new Error('未找到侧栏拖拽分隔线');
  }
  const y = box.y + 300;

  await page.mouse.move(box.x + 1, y);
  await page.mouse.down();
  await page.mouse.move(box.x - 60, y, { steps: 5 });
  expect(await sidebarWidth(page)).toBe(225);
  await expect(page.locator('.drag-bar.is-dragging')).toBeVisible();
  await page.mouse.up();
  await expect.poll(() => offBy(page, 164)).toBeLessThanOrEqual(1);

  const box2 = await bar.boundingBox();
  if (box2 === null) {
    throw new Error('未找到侧栏拖拽分隔线');
  }
  await page.mouse.move(box2.x + 1, y);
  await page.mouse.down();
  await page.mouse.move(20, y, { steps: 8 });
  await expect(page.locator('.drag-bar.is-out')).toBeVisible();
  await page.mouse.up();
  await page.mouse.move(900, 400);
  await expect.poll(() => mainLeft(page)).toBe(60);

  await page.locator('aside.sidebar').hover();
  expect(await mainLeft(page)).toBe(60);
  await page.locator('.collapse-button').click();
  await page.mouse.move(900, 400);
  await expect.poll(() => offBy(page, 164)).toBeLessThanOrEqual(1);

  await page.locator('.drag-bar').dblclick();
  await expect.poll(() => mainLeft(page)).toBe(224);
});

test('侧栏取消固定后支持悬停展开与移开收起并可重新固定，顶栏按钮可切换侧栏显隐', async ({ page }) => {
  await loginToShell(page);

  await page.locator('aside.sidebar').hover();
  await page.getByRole('button', { name: '取消固定' }).click();
  // 鼠标仍悬停在侧栏上时保持浮动展开，主内容区布局不变
  await expect.poll(() => mainLeft(page)).toBe(60);
  expect(await sidebarWidth(page)).toBeGreaterThan(150);
  await page.mouse.move(900, 400);
  await expect.poll(() => sidebarWidth(page)).toBeLessThanOrEqual(61);
  await page.mouse.move(30, 300);
  await expect.poll(() => sidebarWidth(page)).toBeGreaterThan(150);
  await page.getByRole('button', { name: '固定侧栏' }).click();
  await page.mouse.move(900, 400);
  await expect.poll(() => mainLeft(page)).toBe(224);

  await page.getByRole('button', { name: '隐藏侧栏' }).click();
  await expect.poll(() => mainLeft(page)).toBe(0);
  await page.getByRole('button', { name: '显示侧栏' }).click();
  await expect.poll(() => mainLeft(page)).toBe(224);
});
