import { expect, test as base, type Page } from '@playwright/test';

/** 假后端里的账号：admin 全部权限；viewer 只读；newbie 要先改密码；norole 当前角色已失效 */
export type Account = 'admin' | 'viewer' | 'newbie' | 'norole';

/**
 * 每个用例结束时检查：没有页面错误，也没有请求外部域名
 */
export const test = base.extend<{ watch: { errors: string[]; external: string[] } }>({
  watch: [
    async ({ page }, use) => {
      const errors: string[] = [];
      const external: string[] = [];
      page.on('pageerror', (error) => errors.push(error.message));
      page.on('console', (message) => {
        // 401（未登录时探测登录状态）和 403（要先改密码、角色失效）由应用自己处理，浏览器仍会记一条资源加载失败
        const expected = /^Failed to load resource: .* status of 40[13] /.test(message.text());
        if (message.type() === 'error' && !expected) {
          errors.push(message.text());
        }
      });
      page.on('request', (request) => {
        const url = new URL(request.url());
        if (url.protocol.startsWith('http') && !['127.0.0.1', 'localhost'].includes(url.hostname)) {
          external.push(request.url());
        }
      });
      const watch = { errors, external };
      await use(watch);
      expect(errors, '页面错误').toEqual([]);
      expect(external, '外部请求').toEqual([]);
    },
    { auto: true },
  ],
});

export { expect };

/** 默认先打开首页；已经在登录页（如刚退出）时传 navigate: false，不整页刷新 */
export async function login(page: Page, account: Account = 'admin', options = { navigate: true }): Promise<void> {
  if (options.navigate) {
    await page.goto('/');
  }
  await page.getByLabel('账号').fill(account);
  await page.getByLabel('密码').fill('x');
  await page.getByRole('button', { name: '登录' }).click();
}

export async function loginToShell(
  page: Page,
  account: Account = 'admin',
  options = { navigate: true },
): Promise<void> {
  await login(page, account, options);
  await expect(page.getByRole('tab', { name: /工作台/ })).toBeVisible();
}

/** 用 Ctrl K 搜索并打开第一个结果 */
export async function openBySearch(page: Page, keyword: string): Promise<void> {
  await page.keyboard.press('Control+k');
  const dialog = page.locator('.kmic-search .search-container');
  await expect(dialog).toBeVisible();
  await page.keyboard.type(keyword);
  await expect(dialog.locator('.search-item').first()).toBeVisible();
  await page.keyboard.press('Enter');
  await expect(dialog).toBeHidden();
}

/** 打开偏好设置抽屉的某个页签 */
export async function openSettings(page: Page, tab: '外观' | '布局' | '通用'): Promise<void> {
  await page.getByRole('button', { name: '偏好设置' }).click();
  const drawer = page.locator('.ant-drawer');
  await expect(drawer).toBeVisible();
  await drawer.getByText(tab, { exact: true }).first().click();
}

export async function closeSettings(page: Page): Promise<void> {
  await page.locator('.ant-drawer-close').click();
  await expect(page.locator('.ant-drawer-content')).toBeHidden();
}
