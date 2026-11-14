import tseslint from 'typescript-eslint';
import vueParser from 'vue-eslint-parser';
import { boundariesConfig } from '../../eslint/boundaries.js';
import { moduleRestrictions } from '../../eslint/restrictions.js';

// 边界样例专用的检查配置：和正式配置共用同一套规则，只换 src 的位置；样例只需要语法解析，不做类型检查
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
