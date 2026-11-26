import type { Page } from '@playwright/test';
import { expect, test } from './support';

// 假后端里 sales 账号有 2 条未读消息和一个约 40 秒后完成的作业
async function loginAsSales(page: Page): Promise<void> {
  await page.goto('/');
  await page.getByLabel('账号').fill('sales');
  await page.getByLabel('密码').fill('x');
  await page.getByRole('button', { name: '登录' }).click();
  await expect(page.getByRole('tab', { name: /工作台/ })).toBeVisible();
}

const badge = (page: Page, label: string) => page.getByRole('button', { name: label }).locator('.ant-badge-count');

test('站内消息：未读数、点击打开链接并标为已读、全部已读', async ({ page }) => {
  await loginAsSales(page);
  await expect(badge(page, '站内消息')).toHaveText('2');

  await page.getByRole('button', { name: '站内消息' }).click();
  const items = page.locator('.kmic-inbox .inbox-item');
  await expect(items).toHaveCount(3);
  await expect(items.first()).toHaveClass(/is-unread/);
  await expect(page.locator('.kmic-inbox .inbox-dot')).toHaveCount(2);

  await page.locator('.kmic-inbox .inbox-item', { hasText: 'SP-001' }).click();
  await expect(page).toHaveURL(/\/demo\/items\/SP-001/);
  await expect(badge(page, '站内消息')).toHaveText('1');

  await page.getByRole('button', { name: '站内消息' }).click();
  await page.getByRole('button', { name: '全部标为已读' }).click();
  await expect(badge(page, '站内消息')).toHaveCount(0);
  await expect(page.locator('.kmic-inbox .inbox-dot')).toHaveCount(0);
});

test('后台作业：进度、失败原因、取消时面板不关闭，取消后不再轮询', async ({ page }) => {
  const jobPolls: string[] = [];
  page.on('request', (request) => {
    if (new URL(request.url()).pathname.endsWith('/my-jobs')) {
      jobPolls.push(request.url());
    }
  });
  await loginAsSales(page);
  await expect(badge(page, '后台作业')).toHaveText('1');

  await page.getByRole('button', { name: '后台作业' }).click();
  const panel = page.locator('.kmic-jobs');
  await expect(panel.locator('.job')).toHaveCount(3);
  await expect(panel.locator('.ant-progress')).toHaveCount(1);
  await expect(page.getByText('客户编码 C-0999 不存在')).toBeVisible();
  await expect(panel.getByText('运行中')).toBeVisible();

  await panel.locator('.job-cancel').click();
  await page.locator('.ant-popconfirm').getByRole('button', { name: '确定' }).click();
  await expect(panel.locator('.jobs')).toBeVisible();
  await expect(panel.getByText('已取消')).toHaveCount(1);
  await expect(badge(page, '后台作业')).toHaveCount(0);

  await page.mouse.click(700, 600);
  const before = jobPolls.length;
  await page.waitForTimeout(5000);
  expect(jobPolls.length).toBe(before);
});

test('作业跑完：角标消失，消息面板收到“作业已完成”', async ({ page }) => {
  test.setTimeout(90_000);
  await loginAsSales(page);
  await expect(badge(page, '后台作业')).toHaveText('1');

  await expect(badge(page, '后台作业')).toHaveCount(0, { timeout: 60_000 });
  await page.getByRole('button', { name: '后台作业' }).click();
  await expect(page.locator('.kmic-jobs').getByText('运行中')).toHaveCount(0);
  await page.mouse.click(700, 600);

  await page.getByRole('button', { name: '站内消息' }).click();
  await expect(page.getByText('后台作业已完成')).toBeVisible();
  await expect(badge(page, '站内消息')).toHaveText('3');
});
