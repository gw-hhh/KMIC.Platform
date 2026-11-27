import tseslint from 'typescript-eslint';
import vueParser from 'vue-eslint-parser';
import { boundariesConfig } from '../../eslint/boundaries.js';
import { moduleRestrictions } from '../../eslint/restrictions.js';

// 边界测试样例专用 ESLint 配置：复用正式规则集并替换 src 根目录；样例仅需语法解析，不做类型检查
const sets = ['legal', 'violations'];

export default [
  {
    files: ['**/*.{ts,vue}'],
    languageOptions: {
      parser: vueParser,
      parserOptions: { parser: tseslint.parser, extraFileExtensions: ['.vue'] },
    },
  },
  ...sets.flatMap((set) => [
    ...boundariesConfig({ root: `tests/boundaries/${set}/src`, tsconfig: `tests/boundaries/${set}/tsconfig.json` }),
    ...moduleRestrictions(`tests/boundaries/${set}/src/modules`),
  ]),
];
