import type { Page } from '@playwright/test';
import { closeSettings, expect, loginToShell, openSettings, test } from './support';

async function chooseLayout(page: Page, name: '垂直' | '双列菜单' | '混合垂直'): Promise<void> {
  await openSettings(page, '布局');
  await page.getByRole('button', { name, exact: true }).click();
  await closeSettings(page);
}

const mainLeft = (page: Page): Promise<number> =>
  page.locator('div.main').evaluate((el) => Math.round(el.getBoundingClientRect().left));

test('双列菜单布局：第二列打开页面、取消固定后悬停展开及重新固定', async ({ page }) => {
  await loginToShell(page);
  await chooseLayout(page, '双列菜单');

  const sales = page.locator('.mixed-column .item', { hasText: '销售（示例）' });
  await sales.click();
  await expect(page.locator('.panel .panel-title', { hasText: '销售（示例）' })).toBeVisible();
  await page.locator('.panel').getByText('订单', { exact: true }).click();
  await page.getByRole('menuitem', { name: '示例列表' }).click();
  await expect(page.getByRole('tab', { name: /示例列表/ })).toHaveCount(1);
  expect(await mainLeft(page)).toBe(80 + 224);
  await expect(page.locator('.column .collapse-button')).toHaveCount(0);

  await page.locator('.panel .pin-button').click();
  await page.mouse.move(800, 400);
  await expect.poll(() => mainLeft(page)).toBe(80);
  await page.locator('.mixed-column .item', { hasText: '系统（示例）' }).hover();
  await expect(page.locator('.panel .panel-title', { hasText: '系统（示例）' })).toBeVisible();
  await page.mouse.move(800, 400);
  await expect(page.locator('.panel')).toHaveCount(0);

  await sales.hover();
  await page.locator('.panel .pin-button').click();
  await page.mouse.move(800, 400);
  await expect.poll(() => mainLeft(page)).toBe(80 + 224);
});

test('混合垂直布局：顶栏一级目录高亮并联动侧栏子菜单，窄屏下改为抽屉式导航', async ({ page }) => {
  await loginToShell(page);
  await page.getByText('销售（示例）').click();
  await page.getByText('订单', { exact: true }).click();
  await page.getByRole('menuitem', { name: '示例列表' }).click();
  await chooseLayout(page, '混合垂直');

  await expect(page.locator('.header-menu .item.is-active', { hasText: '销售（示例）' })).toBeVisible();
  await page.locator('.header-menu .item', { hasText: '系统（示例）' }).click();
  await expect(page.getByRole('menuitem', { name: '示例报表' })).toBeVisible();
  expect(await page.locator('aside.sidebar').evaluate((el) => Math.round(el.getBoundingClientRect().top))).toBe(50);

  await page.setViewportSize({ width: 700, height: 800 });
  await expect(page.locator('.header-menu')).toHaveCount(0);
  await page.setViewportSize({ width: 1440, height: 800 });

  await chooseLayout(page, '垂直');
  await expect(page.locator('aside .logo-text')).toBeVisible();
});

test('垂直布局折叠侧栏后宽度收缩至 60px', async ({ page }) => {
  await loginToShell(page);
  await page.getByRole('button', { name: '展开/收起菜单' }).first().click();
  await page.mouse.move(800, 400);

  await expect
    .poll(() => page.locator('aside.sidebar').evaluate((el) => Math.round(el.getBoundingClientRect().width)))
    .toBeLessThanOrEqual(61);
});

test('偏好设置：点击整行、开关本身及空格键均只切换一次状态，模式按钮支持键盘选择', async ({ page }) => {
  await loginToShell(page);
  await openSettings(page, '外观');
  const toggle = page.getByRole('switch', { name: '色弱模式' });
  const checked = async (): Promise<string | null> => toggle.getAttribute('aria-checked');

  const initial = await checked();
  await page
    .locator('.switch-row')
    .filter({ hasText: '色弱模式' })
    .click({ position: { x: 20, y: 10 } });
  await expect(toggle).not.toHaveAttribute('aria-checked', initial ?? '');
  const afterRow = await checked();
  await toggle.click();
  await expect(toggle).not.toHaveAttribute('aria-checked', afterRow ?? '');
  const afterClick = await checked();
  await toggle.focus();
  await page.keyboard.press('Space');
  await expect(toggle).not.toHaveAttribute('aria-checked', afterClick ?? '');

  const dark = page.getByRole('button', { name: '深色', exact: true });
  await dark.focus();
  await page.keyboard.press('Enter');
  await expect(dark).toHaveAttribute('aria-pressed', 'true');
  await expect(page.locator('html')).toHaveAttribute('data-theme', 'dark');
  await page.getByRole('button', { name: '浅色', exact: true }).click();
});
