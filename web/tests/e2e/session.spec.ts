import { expect, login, loginToShell, test } from './support';

test('密码错误时停留在登录页并显示错误提示', async ({ page }) => {
  await page.goto('/');
  await page.getByLabel('账号').fill('admin');
  await page.getByLabel('密码').fill('wrong');
  await page.getByRole('button', { name: '登录' }).click();

  await expect(page.getByText('账号或密码错误')).toBeVisible();
  await expect(page).toHaveURL(/\/login/);
});

test('首次登录需先修改密码，完成后方可进入系统', async ({ page }) => {
  await login(page, 'newbie');
  await page.waitForURL('**/auth/change-password');

  await page.getByLabel('当前密码').fill('old');
  await page.getByLabel('新密码', { exact: true }).fill('abc12345');
  await page.getByLabel('确认新密码').fill('abc12345');
  await page.getByRole('button', { name: '修改密码' }).click();

  await expect(page.getByRole('tab', { name: /工作台/ })).toBeVisible();
});

test('当前角色失效时引导重新选择角色后再进入系统', async ({ page }) => {
  await login(page, 'norole');
  await page.waitForURL('**/auth/select-role');

  await page.getByRole('button', { name: '销售经理' }).click();

  await expect(page.getByRole('tab', { name: /工作台/ })).toBeVisible();
});

test('无权限的菜单目录与操作入口保持隐藏', async ({ page }) => {
  await loginToShell(page, 'viewer');

  await expect(page.getByText('销售（示例）')).toBeVisible();
  await expect(page.getByText('系统（示例）')).toBeHidden();
  await page.getByText('销售（示例）').click();
  await page.getByText('订单', { exact: true }).click();
  await expect(page.getByRole('menuitem', { name: '新建示例' })).toHaveCount(0);
});

test('未保存标签页显示状态圆点且关闭或退出需确认，刷新后保留登录态与标签页', async ({ page }) => {
  await loginToShell(page);
  await page.getByText('销售（示例）').click();
  await page.getByText('订单', { exact: true }).click();
  await page.getByRole('menuitem', { name: '新建示例' }).click();
  await page.getByPlaceholder(/随便输入/).fill('abc');
  await expect(page.locator('.state.dirty')).toHaveCount(1);

  await page.locator('.tab.is-active .extra-icon').click();
  await expect(page.getByText('有未保存的修改').first()).toBeVisible();
  await page.getByRole('button', { name: '取消', exact: true }).click();
  await expect(page.getByRole('tab', { name: /新建示例/ })).toHaveCount(1);

  await page.reload();
  await expect(page.getByRole('tab', { name: /工作台/ })).toBeVisible();
  await expect(page.getByRole('tab', { name: /新建示例/ })).toHaveCount(1);

  await page.getByPlaceholder(/随便输入/).fill('again');
  await page.locator('.avatar-button').click();
  await page.getByText('退出登录').click();
  await page.getByRole('button', { name: '确定' }).click();
  await page.getByRole('button', { name: /仍然离开/ }).click();
  await expect(page).toHaveURL(/\/login/);
});

test('退出后以同一账号重新登录：重新拉取偏好设置且工作区仅保留工作台', async ({ page }) => {
  await loginToShell(page);
  await page.locator('.avatar-button').click();
  await page.getByText('退出登录').click();
  await page.getByRole('button', { name: '确定' }).click();
  await expect(page).toHaveURL(/\/login/);
  const preferenceReads: string[] = [];
  page.on('request', (request) => {
    if (request.method() === 'GET' && request.url().includes('/configuration/preferences/me')) {
      preferenceReads.push(request.url());
    }
  });

  // 在当前路由直接重新登录而不整页刷新，以贴合用户实际交互路径
  await loginToShell(page, 'admin', { navigate: false });

  await expect(page.getByRole('tab')).toHaveCount(1);
  expect(preferenceReads.length).toBeGreaterThan(0);
});

test('切换角色后顶栏更新为新角色并重新拉取偏好设置', async ({ page }) => {
  await loginToShell(page);
  const preferenceReads: string[] = [];
  page.on('request', (request) => {
    if (request.method() === 'GET' && request.url().includes('/configuration/preferences/me')) {
      preferenceReads.push(request.url());
    }
  });

  await page.getByText('系统管理员', { exact: true }).first().click();
  await page.getByText('销售经理').click();

  await expect(page.getByText('销售经理').first()).toBeVisible();
  await expect.poll(() => preferenceReads.length).toBeGreaterThan(0);
});

test('登录页切换深浅色主题：仅持久化至本地且不请求接口，刷新后保持一致', async ({ page }) => {
  const apiCalls: string[] = [];
  await page.goto('/');
  await expect(page.getByLabel('账号')).toBeVisible();
  page.on('request', (request) => {
    if (request.url().includes('/api/')) {
      apiCalls.push(request.url());
    }
  });

  await page.getByRole('button', { name: '切换到深色' }).click();
  await expect(page.locator('html')).toHaveAttribute('data-theme', 'dark');
  expect(apiCalls).toEqual([]);

  await page.reload();
  await expect(page.locator('html')).toHaveAttribute('data-theme', 'dark');
  await page.getByRole('button', { name: '切换到浅色' }).click();
  await expect(page.locator('html')).toHaveAttribute('data-theme', 'light');
});

test('确认弹窗跟随当前主题配色，且双字按钮文本不额外插入空格', async ({ page }) => {
  await loginToShell(page);
  await page.getByRole('button', { name: '切换到深色' }).click();
  await expect(page.locator('html')).toHaveAttribute('data-theme', 'dark');

  await page.locator('.avatar-button').click();
  await page.getByText('退出登录').click();
  const confirm = page.locator('.ant-modal-confirm');
  await expect(confirm.getByRole('button', { name: '确定', exact: true })).toBeVisible();
  await expect(confirm.getByRole('button', { name: '取消', exact: true })).toBeVisible();
  const background = await confirm
    .locator('.ant-modal-container, .ant-modal-content')
    .first()
    .evaluate((el) => getComputedStyle(el).backgroundColor);
  expect(background).not.toBe('rgb(255, 255, 255)');
  await confirm.getByRole('button', { name: '取消', exact: true }).click();
});
