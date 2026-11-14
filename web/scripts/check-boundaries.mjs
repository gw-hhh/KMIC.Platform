// 守护目录边界与模块限制确实在起作用
//
// 边界检查在“解析不了导入”时不会报错，只是悄悄放过。所以除了检查正式代码，还要保留两组样例：
// - tests/boundaries/legal：各层允许的导入，不能有任何错误
// - tests/boundaries/violations：故意越界的导入，每个文件必须命中指定的规则，
//   越界依赖只认 boundaries/dependencies，免得把解析失败之类的别的错误误当成拦截成功

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
]);

const eslint = new ESLint({
  cwd: root,
  overrideConfigFile: 'tests/boundaries/eslint.config.js',
  ignore: false,
  errorOnUnmatchedPattern: false,
});

const failures = [];

// 合法样例：一个错误都不能有
for (const result of await eslint.lintFiles(['tests/boundaries/legal/src/**/*.{ts,vue}'])) {
  for (const message of result.messages) {
    failures.push(
      `合法样例不应报错：${relativeTo(result.filePath, 'legal')}:${message.line} ${message.ruleId ?? ''} ${message.message}`,
    );
  }
}

// 越界样例：必须命中指定的规则，并且样例清单里没有遗漏的文件
const seen = new Set();
for (const result of await eslint.lintFiles(['tests/boundaries/violations/src/**/*.{ts,vue}'])) {
  const name = relativeTo(result.filePath, 'violations');
  const rules = new Set(result.messages.map((m) => m.ruleId));
  const want = expected.get(name);
  seen.add(name);
  if (want === undefined) {
    if (rules.size > 0) failures.push(`越界样例清单里没有 ${name}，但它报了 ${[...rules].join('、')}`);
  } else if (!rules.has(want)) {
    failures.push(
      `越界样例 ${name} 应该命中 ${want}，实际是 ${rules.size === 0 ? '没有任何错误' : [...rules].join('、')}`,
    );
  }
}

for (const name of expected.keys()) {
  if (!seen.has(name)) failures.push(`越界样例 ${name} 不存在`);
}

if (failures.length > 0) {
  console.error(`边界检查没有按预期工作：\n${failures.map((f) => `  - ${f}`).join('\n')}`);
  process.exit(1);
}

console.log(`边界检查有效：合法样例没有错误，${expected.size} 个越界样例都被拦下`);

function relativeTo(filePath, set) {
  return relative(`${root}tests/boundaries/${set}/src`, filePath).replaceAll('\\', '/');
}
