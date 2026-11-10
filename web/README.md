# 前端工具链原型（一次性代码，不合入主干）

结论见主干 `docs/frontend/spikes/FP-toolchain.md`。

```powershell
corepack pnpm install
corepack pnpm run build                      # vue-tsc + vite build
$env:KMIC_RESOLVER='typescript'; corepack pnpm exec eslint src   # 边界检查；src/**/bad 下是故意越界的样例
corepack pnpm exec msw init public --save=false
corepack pnpm exec vitest run                # 浏览器模式（Chromium）
# CSP：$env:KMIC_CSP='...'; node node_modules/vite/bin/vite.js preview; node scripts/csp-check.mjs
```
