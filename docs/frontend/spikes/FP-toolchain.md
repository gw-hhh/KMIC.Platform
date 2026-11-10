# 前端原型 FP1～FP6：工具链与框架机制

- 状态：结论已得出；D1～D4 待决（2026-11-10）
- 来源：F02 第 2 节与第 12 节“Vite 8 / Rolldown 与所选插件的兼容”；F03 D1、第 10.3、16 节与第 19 节待确认表（边界检查对 `.vue` 的解析、TanStack Form 回填与数组字段、Antdv Next 能否去掉 `style-src 'unsafe-inline'`、`KeepAlive` 下 VXE 虚拟滚动的布局）；F04 D6（Vitest 浏览器模式与 MSW）
- 编号：前端原型用 FP 编号（08 第 4 节的原型清单只有后端），本页一次覆盖六项
- 原型代码：分支 `spike/fe-01-toolchain` 的 `web/`（一次性代码，不合入主干）
- 环境：Windows 11，Node.js 24.21.0；依赖按 S0-04 第 4 节锁定的版本（Vue 3.5.45、Vite 8.3.6、TypeScript 6.0.3、vue-tsc 3.3.14、Antdv Next 1.8.0、vxe-table 4.23.15、@tanstack/vue-form 1.33.5、Zod 4.6.7、Vitest 5.1.0、MSW 3.0.3、ESLint 10.14.0、eslint-plugin-boundaries 7.4.1）；Chromium 由 Playwright 1.64.1 提供

## 1. 结论

| # | 验证内容 | 结论 |
|---|---|---|
| FP1 | Vite 8（Rolldown）+ `@vitejs/plugin-vue` + Sass + vue-tsc | **成立**。`vue-tsc --noEmit` 通过（`.vue` 模板中 TanStack Form 的字段类型检查正常）；生产构建 4060 个模块约 1～3.5 秒；开发服务器对 `.vue`、`.ts`、`.scss` 的转换正常 |
| FP2 | `eslint-plugin-boundaries` 对 `.vue` 文件的解析 | **成立，但必须配 TypeScript 解析器**（D4）。默认的 node 解析器认不出 `@/` 别名，`@/shell`、`@/modules/other` 的越界导入**全部漏报**，只拦下了相对路径；换成 `eslint-import-resolver-typescript` 后 `.vue` 与 `.ts` 中的越界导入全部报出 |
| FP3 | TanStack Form：服务端错误按路径回填、数组字段、修改后清除 | **成立**。`errors[].path`（含 `items[1].quantity`）回填到对应字段；找不到字段的错误汇总显示；用户修改字段后只清除该字段的服务端错误；Zod 规则对数组元素即时提示。发现一个需要定下来的行为（D3） |
| FP4 | Antdv Next 的 CSS 变量模式能否去掉 `style-src 'unsafe-inline'` | **不能（当前版本）**（D2）。另发现 CSP 缺一项 `font-src`（D2） |
| FP5 | `KeepAlive` 下 VXE 虚拟滚动表格激活后的布局 | **成立，框架不需要额外处理**。滚动到第 990 行后切到另一个标签、把窗口从 1280 改为 900 宽、再切回：滚动位置与可见行不变，表头宽度跟随容器缩小；手动调用 `recalculate()` 与不调用结果相同 |
| FP6 | Vitest 浏览器模式与 MSW 的配合、组件测试的运行时长 | **成立**。MSW 在浏览器模式中拦截真实请求；2 个测试文件 6 个用例（含挂载完整外壳与 2000 行虚拟滚动表格）约 6～8 秒（含启动）。需要两处配置（第 3 节） |

另外：pnpm 12 在本机无法使用（D1）。

## 2. 主要过程与数据

### 2.1 FP2 边界检查

元素划分按 F03 第 2.1 节（`app`、`shell`、`platform`、`modules/*`、`generated`），故意越界的样例三处：模块的 `.vue` 导入 `@/shell/…`；模块的 `.vue` 导入另一个模块（别名与相对路径各一次）；`platform` 的 `.ts` 导入 `@/shell/…`。

| 解析器 | 报出的越界 |
|---|---|
| 默认（node） | 1 / 4：只有相对路径那一处 |
| `eslint-import-resolver-typescript` 4.4.5（ISC，按 `tsconfig.json` 的 `paths`） | 4 / 4 |

- 漏报时**没有任何提示**：解析不了的导入被当作外部包，规则直接放过。因此除了配置解析器，还需要一条守护：仓库中保留一组“必须报错”的越界样例，检查脚本断言它们确实报错（D4）。
- 7.x 中规则 `boundaries/no-unknown` 已改名为 `boundaries/no-unknown-dependencies`。
- “模块不得直接导入组件库交互组件”用 ESLint 自带的 `no-restricted-imports`（按导入名）实现，在 `.vue` 中同样生效。

### 2.2 FP3 表单回填

框架函数（原型中 `platform/form/serverErrors.ts`）：按路径查 `form.fieldInfo`，命中则写入该字段 `errorMap.onServer`，未命中的返回给表单顶部汇总；字段的 `onChange` 监听清除自身的 `onServer`。4 个用例全部通过（MSW 返回 400 ProblemDetails）。

服务端字段错误存在时，表单处于“有错误、不可提交”的状态：**不修改就再次点保存，不会发请求**（D3）。

### 2.3 FP4 CSP

检查方法：生产构建，`vite preview` 加 CSP 响应头，Playwright 打开页面，记录违规与关键元素的计算样式。

| `style-src` | Antdv Next 样式 | VXE 表格 | 违规 |
|---|---|---|---|
| `'self' 'unsafe-inline'`（F03 现写法） | 正常 | 正常 | 只有 VXE 图标字体被拦（`data:` 字体，见下） |
| `'self'` | **失效**：主按钮背景变灰、输入框为浏览器默认边框 | 正常 | 24 处 |
| `'self'` + 零运行时模式（`theme.zeroRuntime` + 预编译的 `antd.css`，934 KB） | **失效**：仍有 4 段运行时注入的 `<style>`（主题变量）被拦 | 正常 | 13 处 |
| `'self' 'nonce-…'` + `ConfigProvider` 的 `csp.nonce` | 组件样式正常 | 正常 | 8 处：Antdv Next 的全局主题变量（`.css-var-root`）没有带 nonce；VXE 运行时插入的 `<style>`（`:root{--dom-main-z-index…}` 等）不支持 nonce |

- 只有 `<style>` 元素与 HTML 中的 `style` 属性受 `style-src` 限制；Vue 的 `:style` 绑定经 CSSOM 设置，不受影响。
- nonce 方案即使两个库都支持，也需要 Nginx 为每个请求生成随机值、同时写入响应头与 `index.html`（`sub_filter`），静态文件不能再整体缓存。
- VXE 的样式表以 `data:` 内嵌图标字体（排序、筛选等图标）。F03 第 16 节的 CSP 没有写 `font-src`，会退回 `default-src 'self'`，图标字体被拦。

### 2.4 FP5、FP6

- FP5 两个用例（激活时重新计算 / 不重新计算）的实测数据相同：切走前 `scrollTop = 47649`、首个可见行 `SP-00990`、宽 1280；切回后 `scrollTop` 与可见行不变，宽 900，表头与容器之差（纵向滚动条 14px）前后一致。VXE 4.23 自身处理了激活。
- FP6 中 MSW 的 `onUnhandledRequest: 'error'` 生效（未声明的请求让测试失败）。

## 3. 阶段 2 建立 `web/` 时直接采用的配置（不涉及取舍）

| 项 | 做法 | 原因 |
|---|---|---|
| Vitest 浏览器模式的服务地址 | `test.api: { host: '127.0.0.1', port: <固定端口>, strictPort: true }` | 默认地址下 Chromium 访问 `localhost:<随机端口>` 被拒绝，测试一个都跑不起来；`test.browser.api` 在 Vitest 5 中已弃用 |
| 测试中的 Vue 去重 | `resolve.dedupe: ['vue']` | 不加时依赖预构建后出现两份 Vue，VXE 表格的列注册失败，表格显示“暂无数据”；生产构建不受影响 |
| Vite 分包 | `build.rolldownOptions.output.codeSplitting` | `advancedChunks` 已弃用 |
| Sass | 不再写 `css.preprocessorOptions.scss.api` | Vite 8 默认即现代编译器，该选项已移除（写了类型检查报错） |
| pnpm 安装脚本 | `pnpm-workspace.yaml` 中 `allowBuilds` 显式列出 `@parcel/watcher`、`vue-demi` 为 `false` | pnpm 11 默认拦截依赖的安装脚本并把它当作错误（`pnpm exec` 也会因此失败）；这两个脚本对本项目无用 |
| VXE 注册 | 只 `app.use(VxeTable)`，`@vxe-ui/core` 由它自己引用 | 官方写法；单独注册 `@vxe-ui/core` 类型检查不通过 |
| 构建产物 | antdv 分包 401 KB（gzip 131 KB）、vxe 分包 535 KB（gzip 164 KB），超过默认的 500 KB 提示线 | 路由级懒加载在阶段 2 按 F03 第 14 节做；两个库的分包本身无法再拆 |
| 版本 | 安装时已有若干补丁版本（Vite 8.3.7、ESLint 插件、MSW 3.0.4 等） | 建立 `web/` 时按质量原则取当时的最新稳定版，重新锁定（S0-04） |

## 4. 待决事项

### D1 pnpm 用哪个大版本（修订 S0-04 第 4 节）

pnpm 12（12.16.1、12.18.0、12.19.0）在本机遇到 `package.json` 中固定包管理器版本的字段（`packageManager` 或 `devEngines.packageManager`）时立即崩溃（Windows 访问冲突 0xC0000005，没有错误信息）；去掉该字段则正常。经 Corepack 与 npx 两种方式都一样；pnpm 11（11.13.0、11.28.9）在同样条件下正常。网上没有找到相同的报告。

| 选项 | 做法 | 优点 | 缺点 |
|---|---|---|---|
| **A 用 pnpm 11 最新版（11.28.9），`packageManager` 固定版本** | Corepack 按 `packageManager` 自动使用同一版本 | 所有人、CI 用同一版本，由文件保证；11 系列仍在发布补丁 | 不是最新大版本；需要跟踪 12 的修复后升级（登记实施待办） |
| B 用 pnpm 12，不写 `packageManager` | 版本只写在文档与 CI 中 | 最新大版本 | 版本靠人记，开发机之间会不一致；违背“锁文件 + 固定工具版本”的做法（S0-04 第 1 节） |
| C 用 pnpm 12，开发机改用 WSL / Linux 运行前端命令 | — | 最新大版本 | 只为绕开一个缺陷就改变开发方式；Windows 上的编辑器集成受影响 |

- **建议**：A；向 pnpm 提交问题报告，修复后升级（实施待办）。
- **取舍**：暂时不用最新大版本。

### D2 CSP 的样式与字体来源（修订 F03 第 16 节）

| 选项 | 做法 | 优点 | 缺点 |
|---|---|---|---|
| **A 保持 `style-src 'self' 'unsafe-inline'`，增加 `font-src 'self' data:`** | F03 原写法 + 字体来源 | 现有两个库都能正常工作；`script-src 'self'` 不变，脚本注入仍被完全阻止 | 样式注入不受 CSP 限制（风险远低于脚本注入：不能执行代码，主要是界面伪装与少量信息泄露） |
| B nonce | Nginx 按请求生成 nonce；`ConfigProvider` 传入；向 Antdv Next、VXE 提问题或打补丁，补上缺 nonce 的两处 | 样式注入也受限 | 依赖两个上游库修改；`index.html` 不能再整体缓存；工作量与维护成本明显 |
| C 去掉 `data:` 字体：替换 VXE 的图标 | 用 Lucide 覆盖 VXE 图标 | `font-src` 可以保持 `'self'` | 要覆盖 VXE 全部内置图标，随版本维护 |

- **建议**：A；“能否去掉 `'unsafe-inline'`”的待确认关闭，结论为“当前版本不能”，在依赖大版本升级时复查（实施待办）。
- **取舍**：接受样式注入不受 CSP 限制。

### D3 服务端字段错误存在时能否再次提交（补充 F03 第 10.3 节）

| 选项 | 做法 | 优点 | 缺点 |
|---|---|---|---|
| **A 不能：修改该字段后才能再提交** | TanStack Form 的默认行为：服务端错误与前端校验错误一样计入“有错误” | 避免用户不改内容反复提交同一份会被拒绝的数据；与 F03 第 10.3 节“修改后清除”一致 | 错误原因在服务端已经消失时（如冲突的名称已被别人改掉），用户要先动一下字段才能重新提交 |
| B 能：提交时先清除全部服务端错误 | `KForm` 在提交前清除 `onServer` | 用户可以直接重试 | 不改内容的重复提交会再次得到同样的错误 |

- **建议**：A。服务端字段错误绝大多数由字段内容引起；少数“原因已消失”的情况，用户改动字段即可，代价很小。
- **取舍**：上面缺点一栏的少数情况多一步操作。

### D4 边界检查的解析器与守护（补充 F03 D1、F02 第 11 节）

| 选项 | 做法 | 优点 | 缺点 |
|---|---|---|---|
| **A 引入 `eslint-import-resolver-typescript`，并加“必须报错”的样例检查** | 解析器按 `tsconfig.json` 的 `paths` 解析 `@/`；在测试目录保留一组越界样例，检查脚本断言 ESLint 对它们报出指定规则 | `@/` 别名照常使用；解析器配置一旦失效（升级、改别名），检查会失败而不是静默放过 | 多一个开发依赖（ISC 许可） |
| B 禁用 `@/` 别名，全部用相对路径 | 默认 node 解析器即可 | 不加依赖 | 深层目录中满屏 `../../../`；仍然需要守护，否则新人加回别名后又会静默失效 |

- **建议**：A。
- **取舍**：多一个开发依赖；样例检查要随目录规则一起维护。

## 5. 对其他文档的影响

| 文档 | 修改 | 状态 |
|---|---|---|
| F02 第 12 节 | “Vite 8 / Rolldown 与所选插件的兼容”关闭（FP1） | 已修订（2026-11-10） |
| F03 第 19 节待确认表 | `KeepAlive` 下的 VXE 布局（FP5）、TanStack Form 回填与数组字段（FP3）关闭；`.vue` 解析、`'unsafe-inline'` 两项转为本页 D4、D2 | 已修订（2026-11-10） |
| F04 第 11.1 节、待确认表 | Vitest 浏览器模式与 MSW 关闭（FP6），第 3 节的两处配置写入 | 已修订（2026-11-10） |
| S0-04 第 4 节、F02 第 2 节 | pnpm 版本（D1） | 确认后修订 |
| F03 第 16 节 | CSP（D2） | 确认后修订 |
| F03 第 10.3 节 | 服务端错误与再次提交（D3） | 确认后修订 |
| F03 D1、F02 第 11 节 | 解析器依赖与样例检查（D4） | 确认后修订 |
