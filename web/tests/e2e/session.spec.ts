import { expect, login, loginToShell, test } from './support';

test('错误密码：留在登录页并提示', async ({ page }) => {
  await page.goto('/');
  await page.getByLabel('账号').fill('admin');
  await page.getByLabel('密码').fill('wrong');
  await page.getByRole('button', { name: '登录' }).click();

  await expect(page.getByText('账号或密码错误')).toBeVisible();
  await expect(page).toHaveURL(/\/login/);
});

test('首次登录必须先改密码，改完进入系统', async ({ page }) => {
  await login(page, 'newbie');
  await page.waitForURL('**/auth/change-password');

  await page.getByLabel('当前密码').fill('old');
  await page.getByLabel('新密码', { exact: true }).fill('abc12345');
  await page.getByLabel('确认新密码').fill('abc12345');
  await page.getByRole('button', { name: '修改密码' }).click();

  await expect(page.getByRole('tab', { name: /工作台/ })).toBeVisible();
});

test('当前角色失效：先选角色再进入系统', async ({ page }) => {
  await login(page, 'norole');
  await page.waitForURL('**/auth/select-role');

  await page.getByRole('button', { name: '销售经理' }).click();

  await expect(page.getByRole('tab', { name: /工作台/ })).toBeVisible();
});

test('没有权限的菜单和入口不显示', async ({ page }) => {
  await loginToShell(page, 'viewer');

  await expect(page.getByText('销售（示例）')).toBeVisible();
  await expect(page.getByText('系统（示例）')).toBeHidden();
  await page.getByText('销售（示例）').click();
  await page.getByText('订单', { exact: true }).click();
  await expect(page.getByRole('menuitem', { name: '新建示例' })).toHaveCount(0);
});

test('未保存的标签：有圆点，关闭和退出都要先确认；刷新后登录和标签都还在', async ({ page }) => {
  await loginToShell(page);
  await page.getByText('销售（示例）').click();
  await page.getByText('订单', { exact: true }).click();
  await page.getByRole('menuitem', { name: '新建示例' }).click();
  await page.getByPlaceholder(/随便输入/).fill('abc');
  await expect(page.locator('.state.dirty')).toHaveCount(1);

  await page.locator('.tab.is-active .extra-icon').click();
  await expect(page.getByText('请先保存').first()).toBeVisible();
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

test('退出后用同一账号重新登录：重新读取偏好，工作区只有工作台', async ({ page }) => {
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

  // 在当前页面直接登录，不整页刷新，和用户的实际操作一样
  await loginToShell(page, 'admin', { navigate: false });

  await expect(page.getByRole('tab')).toHaveCount(1);
  expect(preferenceReads.length).toBeGreaterThan(0);
});

test('切换角色：顶栏显示新角色，并重新读取偏好', async ({ page }) => {
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

test('登录页切换深浅色：只存本机，不调接口，刷新后保留', async ({ page }) => {
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

test('确认框跟随主题，按钮文字不插空格', async ({ page }) => {
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
