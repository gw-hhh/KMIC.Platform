// 找出没有 nonce 的 <style> 与通过 HTML 解析写入的 style 属性（CSSOM 设置的样式不受 CSP 限制）。
import { chromium } from 'playwright';

const browser = await chromium.launch();
const page = await browser.newPage();
await page.addInitScript(() => {
  window.__inlineSources = [];
  new MutationObserver((records) => {
    for (const record of records) {
      for (const node of record.addedNodes) {
        if (node.nodeType !== 1) continue;
        const element = node;
        const styled = [element, ...element.querySelectorAll('[style], style')];
        for (const item of styled) {
          if (item.tagName === 'STYLE' && !item.nonce) {
            window.__inlineSources.push(`<style> 无 nonce：${item.textContent.slice(0, 80)} | 父元素 ${item.parentElement?.tagName}`);
          }
        }
      }
    }
  }).observe(document, { childList: true, subtree: true });
});
page.on('console', (m) => {
  if (m.type() === 'error') console.log('CSP:', m.location().url.split('/').pop(), m.location().lineNumber);
});
await page.goto('http://127.0.0.1:4173/');
await page.waitForTimeout(1500);
console.log((await page.evaluate(() => window.__inlineSources)).join('\n'));
await browser.close();
