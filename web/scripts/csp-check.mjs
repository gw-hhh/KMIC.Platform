// 用法：node scripts/csp-check.mjs <截图文件名>；页面需已由 vite preview 提供。
import { chromium } from 'playwright';

const shot = process.argv[2] ?? 'csp.png';
const browser = await chromium.launch();
const page = await browser.newPage({ viewport: { width: 1280, height: 800 } });
const violations = [];
page.on('console', (message) => {
  if (message.type() === 'error') violations.push(message.text().slice(0, 200));
});
page.on('pageerror', (error) => violations.push(`pageerror: ${error.message.slice(0, 200)}`));

await page.goto('http://127.0.0.1:4173/');
await page.waitForTimeout(1500);

const result = await page.evaluate(() => {
  const primary = document.querySelector('[data-testid="tab-list"]');
  const header = document.querySelector('.vxe-table--header .vxe-header--column');
  const rows = document.querySelectorAll('.vxe-body--row').length;
  return {
    primaryButtonBackground: primary ? getComputedStyle(primary).backgroundColor : null,
    vxeHeaderHeight: header ? header.getBoundingClientRect().height : null,
    vxeRows: rows,
    styleElements: document.querySelectorAll('style').length,
  };
});

await page.click('[data-testid="tab-form"]');
await page.waitForTimeout(500);
const formOk = await page.evaluate(() => {
  const input = document.querySelector('[data-testid="name"]');
  return input ? getComputedStyle(input).borderTopStyle : null;
});

await page.screenshot({ path: shot });
console.log(JSON.stringify({ ...result, inputBorder: formOk, violations: [...new Set(violations)].slice(0, 6), violationCount: violations.length }, null, 2));
await browser.close();
