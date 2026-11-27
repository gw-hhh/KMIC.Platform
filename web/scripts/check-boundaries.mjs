// 校验目录分层边界与模块限制规则是否按预期生效
//
// 当导入路径无法解析时边界检查不会报错而是直接放行，因此除正式代码外还保留两组样例：
// - tests/boundaries/legal：各层允许的导入，不能有任何报错
// - tests/boundaries/violations：故意越界的导入，每个文件必须命中指定规则；
//   越界依赖仅识别 boundaries/dependencies，避免将解析失败等其他错误误判为拦截成功

import { ESLint } from 'eslint';
import { fileURLToPath } from 'node:url';
import { relative } from 'node:path';

const root = fileURLToPath(new URL('..', import.meta.url));

const expected = new Map([
  ['modules/orders/pages/ImportOtherModule.vue', 'boundaries/dependencies'],
  ['modules/orders/pages/importOtherModuleRelative.ts', 'boundaries/dependencies'],
  ['modules/orders/pages/ImportShell.vue', 'boundaries/dependencies'],
  ['platform/importShell.ts', 'boundaries/dependencies'],
  ['modules/orders/pages/rawFetch.ts', 'no-restricted-globals'],
  ['modules/orders/pages/ImportInteractiveComponent.vue', 'no-restricted-imports'],
  ['modules/orders/pages/importVxe.ts', 'no-restricted-imports'],
  ['modules/orders/pages/rawUseQuery.ts', 'no-restricted-imports'],
]);

const eslint = new ESLint({
  cwd: root,
  overrideConfigFile: 'tests/boundaries/eslint.config.js',
  ignore: false,
  errorOnUnmatchedPattern: false,
});

const failures = [];

// 合法样例：不允许出现任何错误
for (const result of await eslint.lintFiles(['tests/boundaries/legal/src/**/*.{ts,vue}'])) {
  for (const message of result.messages) {
    failures.push(
      `合法样例不应报错：${relativeTo(result.filePath, 'legal')}:${message.line} ${message.ruleId ?? ''} ${message.message}`,
    );
  }
}

// 越界样例：必须命中指定的规则，且样例清单无遗漏
const seen = new Set();
for (const result of await eslint.lintFiles(['tests/boundaries/violations/src/**/*.{ts,vue}'])) {
  const name = relativeTo(result.filePath, 'violations');
  const rules = new Set(result.messages.map((m) => m.ruleId));
  const want = expected.get(name);
  seen.add(name);
  if (want === undefined) {
    if (rules.size > 0) failures.push(`越界样例清单未收录 ${name}，实际报错：${[...rules].join('、')}`);
  } else if (!rules.has(want)) {
    failures.push(`越界样例 ${name} 应命中 ${want}，实际为：${rules.size === 0 ? '无错误' : [...rules].join('、')}`);
  }
}

for (const name of expected.keys()) {
  if (!seen.has(name)) failures.push(`越界样例 ${name} 不存在`);
}

if (failures.length > 0) {
  console.error(`边界检查未按预期生效：\n${failures.map((f) => `  - ${f}`).join('\n')}`);
  process.exit(1);
}

console.log(`边界检查有效：合法样例无错误，${expected.size} 个越界样例均已拦截`);

function relativeTo(filePath, set) {
  return relative(`${root}tests/boundaries/${set}/src`, filePath).replaceAll('\\', '/');
}
